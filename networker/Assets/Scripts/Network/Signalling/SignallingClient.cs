using System;
using System.Collections.Generic;
using System.Net;
using System.Timers;
using Network.Packets;
using Network.Packets.Signalling;

namespace Network.Signalling
{
  public class SignallingClient
  {
    private Random rng = new Random();

    private UDPConnection connection;
    private readonly IPEndPoint signallingServer;

    public delegate void OnHostListHandler(SignallingHostList hostList);
    public event OnHostListHandler OnHostList;

    private Timer hostTimer;

    private DateTime latestHolePunchAttempt = DateTime.Now;
    private Dictionary<int, IPEndPoint> holePunchTargets = new Dictionary<int, IPEndPoint>();

    private HashSet<IPEndPoint> holePunchRequests = new HashSet<IPEndPoint>();

    public event EventHandler<IPEndPoint> HolePunchEstablished;

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

      if (DateTime.Now - latestHolePunchAttempt > TimeSpan.FromSeconds(0.5))
      {
        SendHolePunchRequests();

        SendHolePunches();

        latestHolePunchAttempt = DateTime.Now;
      }
    }

    private void SendHolePunches()
    {
      foreach (var kvp in holePunchTargets)
      {
        // TODO, look for surrounding ports as well??
        var candidatePorts = new int[5] {
          kvp.Value.Port + 0,
          kvp.Value.Port + 1,
          kvp.Value.Port + 2,
          kvp.Value.Port + 3,
          kvp.Value.Port + 4,
        };

        foreach (var port in candidatePorts)
        {
          connection.Send(new SignallingHolePunch()
          {
            Id = kvp.Key,
            IsAck = false
          }, new IPEndPoint(kvp.Value.Address, port));
        }
      }
    }

    private void SendHolePunchRequests()
    {
      foreach (var baseTarget in holePunchRequests)
      {
        connection.Send(new SignallingRequestHolePunch()
        {
          IsAck = false,
          ToEndPoint = baseTarget
        }, signallingServer);
      }
    }

    private void HandleOnPacket(IPacket packet, IPEndPoint endPoint)
    {
      switch (packet.Type)
      {
        case PacketType.SignallingHostList:
          var signallignHostList = (SignallingHostListPacket)packet;
          OnHostList?.Invoke(signallignHostList.List);
          break;
        case PacketType.SignallingHolePunch:
          OnHolePunchPacket((SignallingHolePunch)packet, endPoint);
          break;
        case PacketType.SignallingJoinRequest:
          HandleJoinRequest((SignallingJoinRequest)packet, endPoint);
          break;
      }
    }

    private void HandleJoinRequest(SignallingJoinRequest packet, IPEndPoint endPoint)
    {
      if (!packet.IsAck && !holePunchTargets.ContainsValue(packet.FromEndPoint))
      {
        StartHolePunching(packet.FromEndPoint);
        connection.Send(new SignallingJoinRequest()
        {
          IsAck = true,
          FromEndPoint = packet.FromEndPoint
        }, endPoint);
      }
    }

    public void StartRequestHolePunching(IPEndPoint baseTarget)
    {
      holePunchRequests.Add(baseTarget);
      SendHolePunchRequests();
      StartHolePunching(baseTarget);
    }

    private void StartHolePunching(IPEndPoint baseTarget)
    {
      var randomId = rng.Next(int.MinValue, int.MaxValue);
      holePunchTargets[randomId] = baseTarget;
    }

    private void OnHolePunchPacket(SignallingHolePunch packet, IPEndPoint endPoint)
    {
      if (packet.IsAck && holePunchTargets.ContainsKey(packet.Id))
      {
        holePunchTargets.Remove(packet.Id);
        HolePunchEstablished?.Invoke(this, endPoint);
      }
      else if (!packet.IsAck)
      {
        AckHolePunch(packet, endPoint);
      }
    }

    private void AckHolePunch(SignallingHolePunch packet, IPEndPoint endPoint)
    {
      connection.Send(new SignallingHolePunch() { IsAck = true, Id = packet.Id }, endPoint);
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