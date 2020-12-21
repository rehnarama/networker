

using System;

namespace Network.Packets
{
  public interface IPacket
  {
    PacketType Type { get; }

    void Serialize(Serializer serializer);
  }

  public static class Packet
  {
    public static void Serialize(Serializer serializer, ref IPacket packet)
    {
      var typeInt = (int)packet.Type;
      serializer.SerializeInt(ref typeInt);
      if (serializer.IsReader)
      {
        var type = (PacketType)typeInt;
        switch (type)
        {
          case PacketType.Input:
            packet = new InputPacket();
            break;
          case PacketType.Join:
            packet = new JoinPacket();
            break;
          case PacketType.JoinAck:
            packet = new JoinAckPacket();
            break;
          case PacketType.Physics:
            packet = new PhysicsPacket();
            break;
          case PacketType.PhysicsAck:
            packet = new PhysicsAckPacket();
            break;
        }
      }
      packet.Serialize(serializer);
    }
  }
}