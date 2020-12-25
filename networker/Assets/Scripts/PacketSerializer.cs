using Network;
using Network.Packets;

public class PacketSerializer : BasePacketSerializer
{
  public override PacketType Serialize(Serializer serializer, ref IPacket packet)
  {
    var type = base.Serialize(serializer, ref packet);

    bool shouldSerialize =
      type == PacketType.Input ||
      type == PacketType.Physics ||
      type == PacketType.PhysicsAck;

    if (serializer.IsReader)
    {
      switch (type)
      {
        case PacketType.Input:
          packet = new InputPacket();
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

    return type;
  }
}
