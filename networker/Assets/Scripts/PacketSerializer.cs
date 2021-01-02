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
      type == PacketType.EventAck;

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
        case PacketType.EventAck:
          packet = new EventAckPacket();
          break;
      }
    }

    if (shouldSerialize)
    {
      packet.Serialize(serializer);
    }

    return type;
  }
}
