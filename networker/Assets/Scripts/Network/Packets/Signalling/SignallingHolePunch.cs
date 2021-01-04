using System;
using Network.Signalling;

namespace Network.Packets.Signalling
{

  public struct SignallingHolePunch : IPacket
  {
    public PacketType Type => PacketType.SignallingHolePunch;

    private bool isAck;
    public bool IsAck
    {
      get => isAck;
      set => isAck = value;
    }

    private int id;
    public int Id { get => id; set => id = value; }


    public void Serialize(Serializer serializer)
    {
      serializer.SerializeBool(ref isAck);
      serializer.SerializeInt(ref id);
    }
  }
}


