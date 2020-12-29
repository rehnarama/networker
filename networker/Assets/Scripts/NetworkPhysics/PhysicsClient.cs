using System.Net;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace Network.Physics
{
  using Network.Events;
  using Packets;

  public class PhysicsClient : IDisposable
  {
    private const int N_BUFFER_FRAMES = 5;
    private const float RETRY_JOIN_TIME = 0.3f;

    private Client client;

    private int currentFrame = 0;
    private int largestFrame = 0;
    private int latestEvent = -1;
    private bool initialFrameSent = false;
    private bool buffering = true;
    private float lastSentJoinPacket;
    private bool hasJoined = false;

    private Dictionary<int, NetworkedBody> networkBodies = new Dictionary<int, NetworkedBody>();
    private Dictionary<int, NetworkedBody> authorityBodies = new Dictionary<int, NetworkedBody>();

    private Dictionary<int, PhysicsState[]> bufferedPhysicsStates = new Dictionary<int, PhysicsState[]>();
    private Dictionary<int, MultiPlayerInput> bufferedInputs = new Dictionary<int, MultiPlayerInput>();

    public MultiPlayerInput PreviousPlayerInputs { get; private set; } = MultiPlayerInput.Create();
    public MultiPlayerInput PlayerInputs { get; private set; } = MultiPlayerInput.Create();
    public PlayerInput PlayerInput { get; private set; } = PlayerInput.Create();
    public int PlayerId { get; private set; } = -1;

    public event PhysicsServer.OnEventHandler OnEvent;

    public PhysicsClient(Client client)
    {
      this.client = client;
      client.OnPacket += OnPacket;
    }

    private bool disposedValue;


    public void Listen(int port)
    {
      client.Listen(port);
    }

    public void RegisterBody(int id, NetworkedBody body)
    {
      networkBodies.Add(id, body);
      if (body.playerAuthority == PlayerId)
      {
        authorityBodies.Add(id, body);
      }
    }

    public int FindNextFreeBodyId()
    {
      return networkBodies.Keys.Aggregate((largest, next) => Math.Max(largest, next)) + 1;
    }

    private void OnPacket(IPacket packet, IPEndPoint remoteEndPoint)
    {
      if (!hasJoined && packet.Type != PacketType.JoinAck)
      {
        // If we haven't joined, we should disregard this. Else
        // we might appear in an inconsistent state since we do 
        // not have our player id yet
        return;
      }

      switch (packet.Type)
      {
        case PacketType.Physics:
          PhysicsPacket physicsPacket = (PhysicsPacket)packet;
          var frame = physicsPacket.frame;
          bufferedPhysicsStates.Add(frame, physicsPacket.states);
          var inputs = physicsPacket.inputs;
          for (int i = 0; i < inputs.Length; i++)
          {
            bufferedInputs[i + frame] = inputs[inputs.Length - i - 1];
          }

          if (!initialFrameSent)
          {
            currentFrame = physicsPacket.frame;
            initialFrameSent = true;
          }
          largestFrame = Math.Max(largestFrame, physicsPacket.frame);

          if (physicsPacket.events?.Length > 0)
          {
            processEvents(physicsPacket.events);
          }

          client.Send(new PhysicsAckPacket(physicsPacket.frame));
          break;
        case PacketType.JoinAck:
          hasJoined = true;
          JoinAckPacket joinAckPacket = (JoinAckPacket)packet;
          PlayerId = joinAckPacket.playerId;
          break;
      }
    }

    internal void ProcessPackets()
    {
      client.ProcessPackets();
    }

    private void processEvents(IEvent[] events)
    {
      foreach (var e in events)
      {
        if (latestEvent < e.EventNumber)
        {
          latestEvent = e.EventNumber;
          OnEvent?.Invoke(e);
        }
      }

      client.Send(new EventAckPacket(latestEvent));
    }

    private void HandlePhysicsFrame(PhysicsState[] states, MultiPlayerInput input)
    {
      PreviousPlayerInputs = PlayerInputs;
      PlayerInputs = input;

      // By overwriting with local input, we avoid jitter if server
      // hasn't seen local input yet
      PlayerInputs.Inputs[PlayerId] = PlayerInput;

      foreach (var data in states)
      {
        if (networkBodies.TryGetValue(data.Id, out var go))
        {
          if (
            go.playerAuthority != PlayerId ||
            Vector3.Distance(go.body.position, data.Position) > PhysicsConstants.MAX_AUTHORITY_DISTANCE_DIFF
          )
          {
            go.body.position = data.Position;
            go.body.velocity = data.Velocity;
            go.body.rotation = data.Rotation;
            go.body.angularVelocity = data.AngularVelocity;
          }
        }
      }
    }

    public void Tick()
    {
      if (Time.time - lastSentJoinPacket > RETRY_JOIN_TIME && !hasJoined)
      {
        TryJoin();
      }

      if (hasJoined)
      {
        TickInput();
        TickPhysics();
      }
    }

    public void TryJoin()
    {
      lastSentJoinPacket = Time.time;

      client.Send(new JoinPacket());
    }

    private void TickInput()
    {
      var authorityPositions = from body in authorityBodies
                               select new PhysicsState()
                               {
                                 Id = body.Key,
                                 Position = body.Value.body.position,
                                 Velocity = body.Value.body.velocity,
                                 Rotation = body.Value.body.rotation,
                                 AngularVelocity = body.Value.body.angularVelocity
                               };

      client.Send(new InputPacket(PlayerInput, authorityPositions.ToArray()));
    }

    private void TickPhysics()
    {
      if (!initialFrameSent)
      {
        // This means we haven't received initial frame yet, so we cant possibly
        // have something to do...
        // Maybe we haven't "joined" yet?
        return;
      }
      if (buffering && largestFrame >= currentFrame + N_BUFFER_FRAMES)
      {
        buffering = false;
      }
      else if (!buffering && largestFrame - currentFrame < N_BUFFER_FRAMES / 2)
      {
        buffering = true;
      }

      if (buffering)
      {
        // Keep on buffering a while more so we can have jitter-free simulation
        return;
      }

      // Try to adaptively fast-forward (if we have leeway) to keep latency minimal
      bool canFastForward = largestFrame > currentFrame + N_BUFFER_FRAMES;
      var nextFrame = canFastForward ? currentFrame + 2 : currentFrame + 1;

      PhysicsState[] states = new PhysicsState[0];
      bufferedPhysicsStates.TryGetValue(nextFrame, out states);

      MultiPlayerInput input = MultiPlayerInput.Create();
      bufferedInputs.TryGetValue(nextFrame, out input);
      input.Inputs[PlayerId] = PlayerInput;

      HandlePhysicsFrame(states, input);
      for (var i = currentFrame; i <= nextFrame; i++)
      {
        bufferedPhysicsStates.Remove(i);
        bufferedInputs.Remove(i);
      }

      currentFrame = nextFrame;
    }

    public (int CurrentFrame, int BufferedFrames, float AvgInPacketSize, float AvgOutPacketSize) GetDiagnostics()
    {
      return (
        CurrentFrame: currentFrame,
        BufferedFrames: largestFrame - currentFrame,
        AvgInPacketSize: client.connection.AvgInPacketSize,
        AvgOutPacketSize: client.connection.AvgOutPacketSize
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
        client?.Dispose();
        client = null;
        disposedValue = true;
      }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~PhysicsClient()
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
}