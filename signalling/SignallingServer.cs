using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

    public Dictionary<IPEndPoint, SignallingHost> Hosts { get; set; } = new Dictionary<IPEndPoint, SignallingHost>();
    private Dictionary<IPEndPoint, DateTime> LastUpdated { get; set; } = new Dictionary<IPEndPoint, DateTime>();
    private const float EVICTION_TIME = 5f;



    public SignallingServer(int port = 1302)
    {
      connection = new UDPConnection(new SignallingPacketSerializer());
      connection.Listen(port);
      connection.OnPacket += HandleOnPacket;


      timer = new Timer(16);
      timer.Elapsed += Tick;
      timer.AutoReset = true;
      timer.Start();
    }


    private void Tick(object sender, ElapsedEventArgs e)
    {
      connection.ProcessPackets();

      var toEvict = new List<IPEndPoint>();

      foreach (var kvp in LastUpdated)
      {
        var ep = kvp.Key;
        var lastUpdated = kvp.Value;

        if (DateTime.Now - lastUpdated > TimeSpan.FromSeconds(EVICTION_TIME))
        {
          toEvict.Add(ep);
        }
      }

      foreach (var ep in toEvict)
      {
        LastUpdated.Remove(ep);
        Hosts.Remove(ep);
      }
    }

    private void HandleOnPacket(IPacket packet, IPEndPoint from)
    {
      if (packet.Type == PacketType.SignallingHost)
      {
        // TODO: remove from list if not updated at least 10s aparts

        LastUpdated[from] = DateTime.Now;
        var signallingHost = (SignallingHostPacket)packet;
        Hosts[from] = new Network.Signalling.SignallingHost()
        {
          EndPoint = from,
          Name = signallingHost.Name
        };

        Console.WriteLine($"Starting to host: {signallingHost.Name} from {from}");
      }
      else if (packet.Type == PacketType.SignallingRequestHostList)
      {
        connection.Send(new SignallingHostListPacket()
        {
          List = new SignallingHostList() { Servers = Hosts.Values.ToArray() }
        }, from);
      }
    }
  }
}