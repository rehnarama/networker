
namespace Network
{
  using Packets;

  public interface IPacketSerializer
  {
    PacketType Serialize(Serializer s, ref IPacket p);
  }
}