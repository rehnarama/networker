using System;
using System.Collections.Generic;
using System.Linq;

namespace Network.Packets
{
  using Network.Events;
  using Physics;

  [Serializable]
  public struct InputPacket : IPacket
  {
    public PacketType Type => PacketType.Input;

    public PlayerInput input;
    public int frame;

    private PhysicsState[] _AuthorityPositions;
    public PhysicsState[] AuthorityPositions { get => _AuthorityPositions; set => _AuthorityPositions = value; }

    private IEvent[] _Events;
    public IEvent[] Events { get => _Events; set => _Events = value; }

    public InputPacket(int frame, PlayerInput input, PhysicsState[] authorityPositions, IEvent[] events)
    {
      this.frame = frame;
      this.input = input;
      this._AuthorityPositions = authorityPositions;
      this._Events = events;
    }

    public void Serialize(Serializer serializer, EventSerializer eventSerializer)
    {
      serializer.SerializeInt(ref frame);

      PlayerInput.Serialize(serializer, ref input);

      int length = _AuthorityPositions?.Length ?? 0;

      serializer.SerializeInt(ref length);

      if (serializer.IsReader)
      {
        _AuthorityPositions = new PhysicsState[length];
      }

      for (int i = 0; i < length; i++)
      {
        PhysicsState.Serialize(serializer, ref _AuthorityPositions[i]);
      }

      if (eventSerializer != null)
      {
        eventSerializer.SerializeEvents(serializer, ref _Events);
      }
    }

    public void Serialize(Serializer serializer)
    {
      Serialize(serializer, NetworkState.Serializer);
    }
  }
}

