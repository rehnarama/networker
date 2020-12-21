using System;

namespace Network.Packets
{
  using Events;
  using Physics;

  [Serializable]
  public struct PhysicsPacket : IPacket
  {

    public PacketType Type => PacketType.Physics;
    public int frame;
    public MultiPlayerInput[] inputs;
    public PhysicsState[] states;
    public IEvent[] events;

    public PhysicsPacket(int frame, MultiPlayerInput[] inputs, PhysicsState[] states, IEvent[] events)
    {
      this.frame = frame;
      this.inputs = inputs;
      this.states = states;
      this.events = events;
    }

    public void Serialize(Serializer serializer, EventSerializer eventSerializer = null)
    {
      serializer.SerializeInt(ref frame);

      int inputsLength = inputs?.Length ?? 0;
      serializer.SerializeInt(ref inputsLength);
      if (serializer.IsReader)
      {
        inputs = new MultiPlayerInput[inputsLength];
      }
      for (int i = 0; i < inputsLength; i++)
      {
        MultiPlayerInput.Serialize(serializer, ref inputs[i]);
      }

      int statesLength = states?.Length ?? 0;
      serializer.SerializeInt(ref statesLength);
      if (serializer.IsReader)
      {
        states = new PhysicsState[statesLength];
      }
      for (int i = 0; i < statesLength; i++)
      {
        PhysicsState.Serialize(serializer, ref states[i]);
      }

      if (eventSerializer != null)
      {
        eventSerializer.SerializeEvents(serializer, ref events);
      }
    }

    public void Serialize(Serializer serializer)
    {
      Serialize(serializer, NetworkState.Serializer);
    }
  }
}