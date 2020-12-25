using System;
using Network.Signalling;

namespace Network.Packets.Signalling
{

  public struct SignallingRequestHostListPacket : IPacket
  {
    public PacketType Type => PacketType.SignallingRequestHostList;

    public void Serialize(Serializer serializer)
    {
    }
  }
}