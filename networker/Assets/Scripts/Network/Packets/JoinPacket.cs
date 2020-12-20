using System;

namespace Network.Packets
{

  [Serializable]
  public struct JoinPacket : IPacket
  {
    public PacketType Type => PacketType.Join;

    public void Serialize(Serializer serializer)
    {
    }
  }
}