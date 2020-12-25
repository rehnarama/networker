using Network;
using Network.Packets;

namespace Signalling
{
  public class SignallingPacketSerializer : Network.BasePacketSerializer
  {

    public override PacketType Serialize(Serializer serializer, ref IPacket packet)
    {
      return base.Serialize(serializer, ref packet);
    }

  }
}