using System;
using System.Net;
using Network.Packets;
using Network.Packets.Signalling;

namespace Network.Signalling
{
  public class SignallingClient
  {
    private UDPConnection connection;
    private readonly IPEndPoint signallingServer;

    public delegate void OnHostListHandler(SignallingHostList hostList);
    public event OnHostListHandler OnHostList;

    public SignallingClient(IPEndPoint server, int port)
    {
      connection = new UDPConnection(new BasePacketSerializer());
      connection.Listen(port);
      connection.OnPacket += HandleOnPacket;
      this.signallingServer = server;
    }

    public void StopHosting()
    {
      // TODO: stop hosting
    }

    public SignallingClient(IPEndPoint server, UDPConnection connection)
    {
      this.connection = connection;
      this.signallingServer = server;
      connection.OnPacket += HandleOnPacket;
    }

    public void Tick()
    {
      connection.ProcessPackets();
    }

    private void HandleOnPacket(IPacket packet, IPEndPoint endPoint)
    {
      if (packet.Type == Packets.PacketType.SignallingHostList)
      {
        var signallignHostList = (SignallingHostListPacket)packet;
        OnHostList?.Invoke(signallignHostList.List);
      }
    }

    public void RequestList()
    {
      // TODO retry until we get one
      connection.Send(new SignallingRequestHostListPacket(), signallingServer);
    }

    public void StartHosting()
    {

      // TODO loop to continue sending
      connection.Send(new SignallingHostPacket() { Name = "My server" }, signallingServer);
    }
  }
}