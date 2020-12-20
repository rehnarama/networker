using System;

namespace Network.Packets
{

  [Serializable]
  public struct JoinAckPacket : IPacket
  {
    public PacketType Type => PacketType.JoinAck;

    public int playerId;

    public JoinAckPacket(int playerId)
    {
      this.playerId = playerId;
    }

    public void Serialize(Serializer serializer)
    {
      serializer.SerializeInt(ref playerId);
    }
  }
}
