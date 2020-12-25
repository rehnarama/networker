using System;

namespace Network.Packets
{
  [Serializable]
  public struct PhysicsAckPacket : IPacket
  {
    public PacketType Type => PacketType.PhysicsAck;
    public int frame;

    public PhysicsAckPacket(int frame)
    {
      this.frame = frame;
    }

    public void Serialize(Serializer serializer)
    {
      serializer.SerializeInt(ref frame);
    }
  }
}