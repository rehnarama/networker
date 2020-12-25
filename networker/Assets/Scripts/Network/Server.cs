using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;


namespace Network
{
  using Packets;

  public class Server : IDisposable
  {
    private bool disposedValue;
    internal UDPConnection connection;

    public const int PORT = 1303;
    public HashSet<IPEndPoint> clients = new HashSet<IPEndPoint>();

    public delegate void OnReceiveHandler(IPacket packet, IPEndPoint from);
    public event OnReceiveHandler OnReceive;
    public delegate void OnJoinHandler(JoinPacket packet, IPEndPoint from);
    public event OnJoinHandler OnJoin;

    public Server()
    {
      connection = new UDPConnection();
      OnReceive += OnPacket;
    }

    public void Listen(int port)
    {
      connection.Listen(port);
    }

    public void Broadcast(IPacket packet)
    {
      connection.Send(packet, clients);

    }

    public void Send(IPacket packet, IPEndPoint to)
    {
      connection.Send(packet, to);
    }


    public void ProcessPackets()
    {
      var packets = this.connection.GetPackets();
      foreach (var packet in packets)
      {
        OnReceive?.Invoke(packet.packet, packet.from);
      }
    }

    private void OnPacket(IPacket packet, IPEndPoint remoteEndPoint)
    {
      switch (packet.Type)
      {
        case PacketType.Join:
          clients.Add(remoteEndPoint);
          OnJoin?.Invoke((JoinPacket)packet, remoteEndPoint);
          break;
        default:
          break;
      }
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          // TODO: dispose managed state (managed objects)
        }

        this.connection?.Dispose();
        this.connection = null;
        disposedValue = true;
      }
    }

    ~Server()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: false);
    }

    public void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }
  }
}