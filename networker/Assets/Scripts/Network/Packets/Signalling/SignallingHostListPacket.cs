using System;
using Network.Signalling;

namespace Network.Packets.Signalling
{

  public struct SignallingHostListPacket : IPacket
  {
    public PacketType Type => PacketType.SignallingHostList;

    private SignallingHostList list;
    public SignallingHostList List { get => list; set => list = value; }

    public void Serialize(Serializer serializer)
    {
      list.Serialize(serializer);
    }
  }
}