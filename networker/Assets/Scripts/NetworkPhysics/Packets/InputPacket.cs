using System;
using System.Collections.Generic;
using System.Linq;

namespace Network.Packets
{
  using Physics;

  [Serializable]
  public struct InputPacket : IPacket
  {
    public PacketType Type => PacketType.Input;


    public PlayerInput input;

    private PhysicsState[] _AuthorityPositions;
    public PhysicsState[] AuthorityPositions { get => _AuthorityPositions; set => _AuthorityPositions = value; }
    public InputPacket(PlayerInput input, PhysicsState[] authorityPositions)
    {
      this.input = input;
      this._AuthorityPositions = authorityPositions;
    }

    public void Serialize(Serializer serializer)
    {
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
    }
  }
}

