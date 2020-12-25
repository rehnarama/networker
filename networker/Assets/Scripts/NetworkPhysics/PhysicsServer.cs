using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Network.Physics
{
  using System;
  using System.Net;
  using Network.Events;
  using Packets;



  public class PhysicsServer : IDisposable
  {
    private const int MAX_OBJECTS = 32;
    private const int MAX_INPUTS = 32;

    private Server server;

    private int frameCount = 0;
    private int eventCount = 0;

    private int bodyIdCounter = 0;

    private Dictionary<int, int> acks = new Dictionary<int, int>();
    private int LatestAckedFrame
    {
      get
      {
        return acks.Aggregate(frameCount, (min, ack) => Math.Min(ack.Value, min));
      }
    }
    private Dictionary<int, int> eventAcks = new Dictionary<int, int>();
    private int LatestAckedEvent
    {
      get
      {
        return eventAcks.Aggregate(eventCount, (min, ack) => Math.Min(ack.Value, min));
      }
    }

    private Dictionary<int, PriorityBody> idPriorityMap = new Dictionary<int, PriorityBody>();

    public MultiPlayerInput PlayerInputs
    {
      get
      {
        return bufferedInputs.Last();
      }
    }
    public Queue<MultiPlayerInput> bufferedInputs { get; private set; } = new Queue<MultiPlayerInput>();

    public Queue<IEvent> events = new Queue<IEvent>();

    public delegate void OnEventHandler(IEvent e);
    public event OnEventHandler OnEvent;
    public delegate void OnJoinHandler(int playerId);
    public event OnJoinHandler OnPlayerJoin;

    public Dictionary<IPEndPoint, int> Players = new Dictionary<IPEndPoint, int>();

    public PhysicsServer(Server server)
    {
      this.server = server;
      server.OnJoin += OnJoin;
      server.OnReceive += OnPacket;
      bufferedInputs.Enqueue(MultiPlayerInput.Create());
    }

    private bool disposedValue;

    public void Listen(int port = Server.PORT)
    {
      server.Listen(port);
    }

    public void RegisterBody(int id, NetworkedBody body, bool isImportant = false)
    {
      bodyIdCounter = Math.Max(bodyIdCounter, id);
      var priorityBody = new PriorityBody() { BodyId = id, Priority = 0, IsImportant = isImportant, Body = body };
      idPriorityMap.Add(id, priorityBody);
    }

    public int FindNextFreeBodyId()
    {
      int idCandidate = bodyIdCounter;
      while (idPriorityMap.ContainsKey(idCandidate))
      {
        idCandidate++;
      }
      bodyIdCounter++;
      return idCandidate;
    }

    private void IncreasePriorities()
    {
      foreach (var kvp in idPriorityMap)
      {
        var priorityBody = kvp.Value;
        var body = priorityBody.Body;

        if (body.body.IsSleeping())
        {
          priorityBody.Priority += 1;
        }
        else
        {
          priorityBody.Priority += 20;
        }
      }
    }

    internal void ProcessPackets()
    {
      server.ProcessPackets();
    }

    public void Tick()
    {
      IncreasePriorities();

      var physicsData = (from kvp in idPriorityMap
                         orderby kvp.Value descending
                         select new PhysicsState()
                         {
                           Id = kvp.Value.BodyId,
                           Position = kvp.Value.Body.body.position,
                           Rotation = kvp.Value.Body.body.rotation,
                           Velocity = kvp.Value.Body.body.velocity,
                           AngularVelocity = kvp.Value.Body.body.angularVelocity
                         }).Take(MAX_OBJECTS).ToArray();

      foreach (var data in physicsData)
      {
        idPriorityMap[data.Id].Priority = 0;
      }

      var packet = new PhysicsPacket(frameCount, bufferedInputs.ToArray(), physicsData, events.ToArray());

      server.Broadcast(packet);

      frameCount++;

      bufferedInputs.Enqueue(bufferedInputs.Last().Copy()); // Create a copy
      while (bufferedInputs.Count > MAX_INPUTS)
      {
        bufferedInputs.Dequeue();
      }
    }

    private void OnJoin(JoinPacket packet, IPEndPoint remoteEndPoint)
    {
      int playerId;

      if (!Players.ContainsKey(remoteEndPoint))
      {
        var playerIds = Players.Values;
        // Assign a player id to player
        int lowestFree = 0;
        while (playerIds.Contains(lowestFree))
        {
          lowestFree++;
        }

        playerId = lowestFree;
        Players[remoteEndPoint] = playerId;
      }
      else
      {
        playerId = Players[remoteEndPoint];
      }

      server.Send(new JoinAckPacket(playerId), remoteEndPoint);
      OnPlayerJoin?.Invoke(playerId);
    }

    private void OnPacket(IPacket packet, IPEndPoint remoteEndPoint)
    {
      int playerId;

      switch (packet.Type)
      {
        case PacketType.Input:
          HandleInputPacket(packet, remoteEndPoint);
          break;
        case PacketType.PhysicsAck:
          if (Players.TryGetValue(remoteEndPoint, out playerId))
          {
            var ackPacket = (PhysicsAckPacket)packet;
            acks[playerId] = ackPacket.frame;
          }

          var largestAckedFrame = LatestAckedFrame;
          while (frameCount - largestAckedFrame > bufferedInputs.Count)
          {
            bufferedInputs.Dequeue();
          }
          break;
        case PacketType.EventAck:
          if (Players.TryGetValue(remoteEndPoint, out playerId))
          {
            var ackPacket = (EventAckPacket)packet;
            eventAcks[playerId] = ackPacket.id;
          }

          var largestAckedEvent = LatestAckedEvent;
          while (eventCount - largestAckedEvent > events.Count)
          {
            events.Dequeue();
          }
          break;
      }
    }

    private void HandleInputPacket(IPacket packet, IPEndPoint remoteEndPoint)
    {
      if (Players.TryGetValue(remoteEndPoint, out var playerId))
      {
        var inputPacket = (InputPacket)packet;
        bufferedInputs.Last().Inputs[playerId] = inputPacket.input;

        foreach (var authorityPosition in inputPacket.AuthorityPositions)
        {
          if (idPriorityMap.TryGetValue(authorityPosition.Key, out var pb))
          {
            if (
              pb.Body.playerAuthority == playerId &&
              Vector3.Distance(pb.Body.body.position, authorityPosition.Value) < PhysicsConstants.MAX_AUTHORITY_DISTANCE_DIFF
            )
            {
              pb.Body.body.position = authorityPosition.Value;
            }
          }
        }
      }
    }

    public void InvokeEvent(IEvent e)
    {
      e.EventNumber = eventCount;
      eventCount++;
      events.Enqueue(e);
      OnEvent?.Invoke(e);
    }

    public (int Frame, int UnackedFrames, int PlayersJoined, float AvgInPacketSize, float AvgOutPacketSize) GetDiagnostics()
    {
      return (
        Frame: frameCount,
        UnackedFrames: frameCount - LatestAckedFrame,
        PlayersJoined: Players.Count(),
        AvgInPacketSize: server.connection.AvgInPacketSize,
        AvgOutPacketSize: server.connection.AvgOutPacketSize
      );
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          // TODO: dispose managed state (managed objects)
        }

        // TODO: free unmanaged resources (unmanaged objects) and override finalizer
        // TODO: set large fields to null
        server?.Dispose();
        server = null;
        disposedValue = true;
      }
    }

    ~PhysicsServer()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: false);
    }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }

  internal class PriorityBody : IComparable
  {
    public bool IsImportant { get; set; }
    public int Priority { get; set; }
    public int BodyId { get; set; }
    public NetworkedBody Body { get; set; }

    public int CompareTo(object obj)
    {
      if (obj == null) return 1;

      PriorityBody otherPriorityBody = obj as PriorityBody;
      if (otherPriorityBody != null)
      {
        if (IsImportant)
        {
          return 1;
        }
        else
        {
          return this.Priority.CompareTo(otherPriorityBody.Priority);
        }
      }
      else
      {
        throw new ArgumentException("Object is not a PriorityBody");
      }
    }
  }
}