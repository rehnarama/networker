using System;

namespace Network.Packets
{

  [Serializable]
  public struct NoopPacket : IPacket
  {
    public PacketType Type => PacketType.Noop;

    public void Serialize(Serializer serializer)
    {
    }
  }
}
