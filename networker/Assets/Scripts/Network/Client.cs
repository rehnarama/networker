

using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

namespace Network
{
  using System;
  using Packets;


  public class Client : IDisposable
  {
    internal UDPConnection connection;
    private bool disposedValue;
    private readonly IPEndPoint serverEndpoint;

    public delegate void OnReceiveHandler(IPacket packet, IPEndPoint from);
    public event OnReceiveHandler OnReceive;

    public Client(IPEndPoint serverEndpoint)
    {
      this.serverEndpoint = serverEndpoint;
      connection = new UDPConnection();

    }


    public void Send(IPacket packet)
    {
      connection.Send(packet, serverEndpoint);
    }

    public void ProcessPackets()
    {
      var packets = this.connection.GetPackets();
      foreach (var packet in packets)
      {
        OnReceive?.Invoke(packet.packet, packet.from);
      }
    }

    internal void Listen(int port)
    {
      connection.Listen(port);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          // TODO: dispose managed state (managed objects)
        }

        connection?.Dispose();
        connection = null;
        disposedValue = true;
      }
    }

    ~Client()
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