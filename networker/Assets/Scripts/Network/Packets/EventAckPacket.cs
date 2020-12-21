using System;

namespace Network.Packets
{
  [Serializable]
  public struct EventAckPacket : IPacket
  {
    public PacketType Type => PacketType.EventAck;
    public int id;

    public EventAckPacket(int id)
    {
      this.id = id;
    }

    public void Serialize(Serializer serializer)
    {
      serializer.SerializeInt(ref id);
    }
  }
}
