using System;
using System.Net;
using System.Timers;
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

    private Timer hostTimer;

    public SignallingClient(IPEndPoint server, int port)
    {
      connection = new UDPConnection(new BasePacketSerializer());
      connection.Listen(port);
      connection.OnPacket += HandleOnPacket;
      this.signallingServer = server;
    }

    public void StopHosting()
    {
      hostTimer?.Stop();
      hostTimer = null;
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

    public void StartHosting(string name)
    {
      hostTimer?.Stop();

      SendHostPacket(name);
      hostTimer = new Timer(2000);
      hostTimer.Elapsed += (object sender, ElapsedEventArgs e) =>
      {
        SendHostPacket(name);
      };

      hostTimer.AutoReset = true;
      hostTimer.Start();
    }

    private void SendHostPacket(string name)
    {
      connection.Send(new SignallingHostPacket() { Name = name }, signallingServer);
    }
  }
}