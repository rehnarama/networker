using System.Net;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

namespace Network.Physics
{
  using Packets;

  public class PhysicsClient : Client
  {
    private const int NumberOfBufferFrames = 5;

    private int currentFrame = 0;
    private int largestFrame = 0;
    private bool initialFrameSent = false;
    private bool buffering = true;
    private bool hasJoined = false;

    private Dictionary<int, Rigidbody> gameObjects = new Dictionary<int, Rigidbody>();

    private Dictionary<int, PhysicsState[]> bufferedPhysicsStates = new Dictionary<int, PhysicsState[]>();
    private Dictionary<int, MultiPlayerInput> bufferedInputs = new Dictionary<int, MultiPlayerInput>();

    public MultiPlayerInput PlayerInputs { get; private set; } = MultiPlayerInput.Create();
    public PlayerInput PlayerInput { get; private set; } = PlayerInput.Create();
    public int PlayerId { get; private set; } = -1;

    private PhysicsClient(IPEndPoint serverEndpoint) : base(serverEndpoint)
    {
    }

    public static IPEndPoint ServerEndpoint { get; set; } = null;
    private static PhysicsClient _Instance = null;
    public static PhysicsClient Instance
    {
      get
      {
        if (_Instance == null)
        {
          if (ServerEndpoint == null)
          {
            throw new SystemException("You must set ServerEndpoint property before getting first instance");
          }
          else
          {
            _Instance = new PhysicsClient(ServerEndpoint);
          }
        }
        return _Instance;
      }
    }

    public void RegisterBody(int id, Rigidbody go)
    {
      gameObjects.Add(id, go);
    }

    protected override void OnPacket(IPacket packet, IPEndPoint remoteEndPoint)
    {
      base.OnPacket(packet, remoteEndPoint);

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
          Send(new PhysicsAckPacket(physicsPacket.frame));
          break;
        case PacketType.JoinAck:
          hasJoined = true;
          JoinAckPacket joinAckPacket = (JoinAckPacket)packet;
          PlayerId = joinAckPacket.playerId;
          break;
      }
    }

    private void HandlePhysicsFrame(PhysicsState[] states, MultiPlayerInput input)
    {

      PlayerInputs = input;

      // By overwriting with local input, we avoid jitter if server
      // hasn't seen local input yet
      PlayerInputs.Inputs[PlayerId] = PlayerInput;

      // TODO: give some slack to player object maybe?
      foreach (var data in states)
      {
        if (gameObjects.TryGetValue(data.Id, out var go))
        {
          go.position = data.Position;
          go.velocity = data.Velocity;
          go.rotation = data.Rotation;
          go.angularVelocity = data.AngularVelocity;
        }
      }
    }

    public void Tick()
    {
      TickInput();
      TickPhysics();
    }

    private void TickInput()
    {
      Send(new InputPacket(PlayerInput));
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
      if (buffering && largestFrame >= currentFrame + NumberOfBufferFrames)
      {
        buffering = false;
      }
      else if (buffering)
      {
        // Keep on buffering a while more so we can have jitter-free simulation
        return;
      }

      // Try to adaptively fast-forward (if we have leeway) to keep latency minimal
      bool canFastForward = largestFrame > currentFrame + NumberOfBufferFrames;
      var nextFrame = canFastForward ? currentFrame + 2 : currentFrame + 1;

      PhysicsState[] states = new PhysicsState[0];
      bufferedPhysicsStates.TryGetValue(nextFrame, out states);

      MultiPlayerInput input = MultiPlayerInput.Create();
      input.Inputs[PlayerId] = PlayerInput;
      bufferedInputs.TryGetValue(nextFrame, out input);

      HandlePhysicsFrame(states, input);
      for (var i = currentFrame; i <= nextFrame; i++)
      {
        bufferedPhysicsStates.Remove(i);
        bufferedInputs.Remove(i);
      }

      currentFrame = nextFrame;
    }
  }
}