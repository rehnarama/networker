using System;
using System.Collections.Generic;
using System.Timers;
using Network;
using Network.Packets;
using Network.Packets.Signalling;
using Network.Signalling;

namespace Signalling
{
  public class SignallingServer
  {
    public UDPConnection connection;
    private Timer timer;

    private List<Network.Signalling.SignallingHost> hosts = new List<Network.Signalling.SignallingHost>();
    public List<Network.Signalling.SignallingHost> Hosts { get => hosts; set => hosts = value; }

    public SignallingServer(int port = 1302)
    {
      connection = new UDPConnection(new SignallingPacketSerializer());
      connection.Listen(port);


      timer = new Timer(16);
      timer.Elapsed += Tick;
      timer.AutoReset = true;
      timer.Start();
    }


    private void Tick(object sender, ElapsedEventArgs e)
    {
      var packets = connection.GetPackets();

      foreach (var p in packets)
      {

        var (packet, from) = p;

        Console.WriteLine(packet.Type);

        if (packet.Type == PacketType.SignallingHost)
        {
          var signallingHost = (SignallingHostPacket)packet;
          hosts.Add(new Network.Signalling.SignallingHost()
          {
            EndPoint = from,
            Name = signallingHost.Name
          });
          Console.WriteLine($"Got: {signallingHost.Name} from: {from}");
        }
      }
    }
  }
}