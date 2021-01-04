using Network;
using Network.Packets;
using Network.Packets.Signalling;

namespace Network
{
  public class BasePacketSerializer : IPacketSerializer
  {
    public virtual PacketType Serialize(Serializer serializer, ref IPacket packet)
    {
      var typeInt = (int)packet.Type;
      serializer.SerializeInt(ref typeInt);

      var type = (PacketType)typeInt;

      bool shouldSerialize =
        type == PacketType.Join ||
        type == PacketType.JoinAck ||
        type == PacketType.SignallingHost ||
        type == PacketType.SignallingHostList ||
        type == PacketType.SignallingRequestHostList ||
        type == PacketType.SignallingHolePunch ||
        type == PacketType.SignallingRequestHolePunch ||
        type == PacketType.SignallingJoinRequest;


      if (serializer.IsReader)
      {
        switch (type)
        {
          case PacketType.Join:
            packet = new JoinPacket();
            break;
          case PacketType.JoinAck:
            packet = new JoinAckPacket();
            break;
          case PacketType.SignallingHost:
            packet = new SignallingHostPacket();
            break;
          case PacketType.SignallingHostList:
            packet = new SignallingHostListPacket();
            break;
          case PacketType.SignallingRequestHostList:
            packet = new SignallingRequestHostListPacket();
            break;
          case PacketType.SignallingHolePunch:
            packet = new SignallingHolePunch();
            break;
          case PacketType.SignallingRequestHolePunch:
            packet = new SignallingRequestHolePunch();
            break;
          case PacketType.SignallingJoinRequest:
            packet = new SignallingJoinRequest();
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


}