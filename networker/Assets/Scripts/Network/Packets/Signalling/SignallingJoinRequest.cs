using System;
using System.Net;
using Network.Signalling;

namespace Network.Packets.Signalling
{

  public struct SignallingJoinRequest : IPacket
  {
    public PacketType Type => PacketType.SignallingJoinRequest;

    private bool isAck;
    public bool IsAck
    {
      get => isAck;
      set => isAck = value;
    }

    private byte[] endPointIP;
    private int endPointPort;
    public IPEndPoint FromEndPoint
    {
      get
      {
        var ip = new IPAddress(endPointIP);
        return new IPEndPoint(ip, endPointPort);
      }
      set
      {
        endPointIP = value.Address.GetAddressBytes();
        endPointPort = value.Port;
      }
    }

    public void Serialize(Serializer serializer)
    {
      serializer.SerializeBool(ref isAck);
      serializer.SerializeByteArray(ref endPointIP);
      serializer.SerializeInt(ref endPointPort);
    }
  }
}
