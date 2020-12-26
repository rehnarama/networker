using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;


namespace Network
{
  using Network.Signalling;
  using Packets;

  public class Server : IDisposable
  {
    private bool disposedValue;
    public UDPConnection Connection { get; private set; }

    public HashSet<IPEndPoint> clients = new HashSet<IPEndPoint>();

    public event UDPConnection.OnPacketHandler OnPacket
    {
      add
      {
        Connection.OnPacket += value;
      }
      remove
      {
        Connection.OnPacket -= value;
      }
    }
    public delegate void OnJoinHandler(JoinPacket packet, IPEndPoint from);
    public event OnJoinHandler OnJoin;


    public Server(UDPConnection connection)
    {
      this.Connection = connection;
      OnPacket += HandleOnPacket;

    }

    public void Listen(int port)
    {
      Connection.Listen(port);
    }

    public void Broadcast(IPacket packet)
    {
      Connection.Send(packet, clients);

    }

    public void Send(IPacket packet, IPEndPoint to)
    {
      Connection.Send(packet, to);
    }


    public void ProcessPackets()
    {
      this.Connection.ProcessPackets();
    }

    private void HandleOnPacket(IPacket packet, IPEndPoint remoteEndPoint)
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

        this.Connection?.Dispose();
        this.Connection = null;
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