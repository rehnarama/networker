using System;

namespace Network.Packets
{
  using Physics;

  [Serializable]
  public struct InputPacket : IPacket
  {
    public PacketType Type => PacketType.Input;

    public PlayerInput input;

    public InputPacket(PlayerInput input)
    {
      this.input = input;
    }

    public void Serialize(Serializer serializer)
    {
      PlayerInput.Serialize(serializer, ref input);
    }
  }
}

