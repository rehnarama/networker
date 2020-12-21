using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Network.Physics
{
  using System;
  using System.Net;
  using Network.Events;
  using Packets;

  internal class PriorityBody : IComparable
  {
    public bool IsImportant { get; set; }
    public int Priority { get; set; }
    public int BodyId { get; set; }
    public Rigidbody Body { get; set; }

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

  public class PhysicsServer : Server
  {
    private const int MAX_OBJECTS = 32;
    private const int MAX_INPUTS = 32;

    private int frameCount = 0;
    private int eventCount = 0;

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

    private Dictionary<IPEndPoint, int> players = new Dictionary<IPEndPoint, int>();

    private PhysicsServer()
    {
      bufferedInputs.Enqueue(MultiPlayerInput.Create());
    }

    private static PhysicsServer _Instance = null;
    public static PhysicsServer Instance
    {
      get
      {
        if (_Instance == null)
        {
          _Instance = new PhysicsServer();
        }
        return _Instance;
      }
    }

    public void RegisterBody(int id, Rigidbody body, bool isImportant = false)
    {
      var priorityBody = new PriorityBody() { BodyId = id, Priority = 0, IsImportant = isImportant, Body = body };
      idPriorityMap.Add(id, priorityBody);
    }

    public int FindNextFreeBodyId()
    {
      return idPriorityMap.Keys.Aggregate((largest, next) => Math.Max(largest, next)) + 1;
    }

    private void IncreasePriorities()
    {
      foreach (var kvp in idPriorityMap)
      {
        var priorityBody = kvp.Value;
        var body = priorityBody.Body;

        if (body.IsSleeping())
        {
          priorityBody.Priority += 1;
        }
        else
        {
          priorityBody.Priority += 20;
        }
      }
    }

    public void Tick()
    {
      IncreasePriorities();

      var physicsData = (from kvp in idPriorityMap
                         orderby kvp.Value descending
                         select new PhysicsState()
                         {
                           Id = kvp.Value.BodyId,
                           Position = kvp.Value.Body.position,
                           Rotation = kvp.Value.Body.rotation,
                           Velocity = kvp.Value.Body.velocity,
                           AngularVelocity = kvp.Value.Body.angularVelocity
                         }).Take(MAX_OBJECTS).ToArray();

      foreach (var data in physicsData)
      {
        idPriorityMap[data.Id].Priority = 0;
      }

      var packet = new PhysicsPacket(frameCount, bufferedInputs.ToArray(), physicsData, events.ToArray());

      Broadcast(packet);

      frameCount++;

      bufferedInputs.Enqueue(bufferedInputs.Last().Copy()); // Create a copy
      while (bufferedInputs.Count > MAX_INPUTS)
      {
        bufferedInputs.Dequeue();
      }
    }

    public override void OnJoin(JoinPacket packet, IPEndPoint remoteEndPoint)
    {
      base.OnJoin(packet, remoteEndPoint);

      int playerId;

      if (!players.ContainsKey(remoteEndPoint))
      {
        var playerIds = players.Values;
        // Assign a player id to player
        int lowestFree = 0;
        while (playerIds.Contains(lowestFree))
        {
          lowestFree++;
        }

        playerId = lowestFree;
        players[remoteEndPoint] = playerId;
      }
      else
      {
        playerId = players[remoteEndPoint];
      }

      Send(new JoinAckPacket(playerId), remoteEndPoint);
    }

    protected override void OnPacket(IPacket packet, IPEndPoint remoteEndPoint)
    {
      base.OnPacket(packet, remoteEndPoint);
      int playerId;

      switch (packet.Type)
      {
        case PacketType.Input:
          if (players.TryGetValue(remoteEndPoint, out playerId))
          {
            var inputPacket = (InputPacket)packet;
            bufferedInputs.Last().Inputs[playerId] = inputPacket.input;
          }
          break;
        case PacketType.PhysicsAck:
          if (players.TryGetValue(remoteEndPoint, out playerId))
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
          if (players.TryGetValue(remoteEndPoint, out playerId))
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
        PlayersJoined: players.Count(),
        AvgInPacketSize: avgInPacketSize,
        AvgOutPacketSize: avgOutPacketSize
      );
    }
  }
}