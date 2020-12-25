using System;

namespace Network.Packets
{
  public interface IPacket
  {
    PacketType Type { get; }

    void Serialize(Serializer serializer);
  }
}