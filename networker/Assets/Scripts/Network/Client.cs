

using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

namespace Network
{
  using Packets;


  public class Client : UDPConnection
  {
    private readonly IPEndPoint serverEndpoint;

    public Client(IPEndPoint serverEndpoint)
    {
      this.serverEndpoint = serverEndpoint;
    }

    public void Send(IPacket packet)
    {
      Send(packet, serverEndpoint);
    }

    public void SendJoinPacket()
    {
      Send(new JoinPacket());
    }

    public void ProcessPackets()
    {
      BinaryFormatter binaryFmt = new BinaryFormatter();

      while (this.receivedStack.Count > 0)
      {
        if (this.receivedStack.TryDequeue(out var data))
        {
          var s = Serializer.CreateReader(data.Buffer);
          IPacket packet = new JoinPacket(); // Just assign something to make ref happy
          Packet.Serialize(s, ref packet);

          OnPacket(packet, data.RemoteEndPoint);
        }
      }
    }
    protected virtual void OnPacket(IPacket packet, IPEndPoint remoteEndPoint)
    {
      switch (packet.Type)
      {
        default:
          break;
      }
    }
  }
}