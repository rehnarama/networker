using System;
using System.Net;

namespace Network.Signalling
{
  [Serializable]
  public struct SignallingHost
  {
    private string name;
    public string Name { get => name; set => name = value; }

    private byte[] endPointIP;
    private int endPointPort;
    public IPEndPoint EndPoint
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
      serializer.SerializeString(ref name);

      serializer.SerializeByteArray(ref endPointIP);
      serializer.SerializeInt(ref endPointPort);
    }
  }
}