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


    private Dictionary<IPEndPoint, IPEndPoint> joinRequests = new Dictionary<IPEndPoint, IPEndPoint>();
    private DateTime lastJoinRequestSent = DateTime.Now;


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

      if (DateTime.Now - lastJoinRequestSent > TimeSpan.FromSeconds(0.5))
      {
        foreach (var (from, to) in joinRequests)
        {
          connection.Send(
            new SignallingJoinRequest()
            {
              IsAck = false,
              FromEndPoint = from
            },
            to
          );
        }

        lastJoinRequestSent = DateTime.Now;
      }
    }

    private void HandleOnPacket(IPacket packet, IPEndPoint from)
    {
      if (packet.Type == PacketType.SignallingHost)
      {
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
      else if (packet.Type == PacketType.SignallingJoinRequest)
      {
        var joinPacket = (SignallingJoinRequest)packet;
        // Definitely should be! No one should send this to the signallign
        if (joinPacket.IsAck)
        {
          joinRequests.Remove(from);
        }
      }
      else if (packet.Type == PacketType.SignallingRequestHolePunch)
      {
        var requestPacket = (SignallingRequestHolePunch)packet;
        if (!joinRequests.ContainsKey(from))
        {
          joinRequests[from] = requestPacket.ToEndPoint;
        }

        connection.Send(new SignallingRequestHolePunch()
        {
          IsAck = true,
          ToEndPoint = requestPacket.ToEndPoint
        }, from);
      }
    }
  }
}