

using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

namespace Network
{
  using System;
  using Packets;


  public class Client : IDisposable
  {
    public UDPConnection connection;
    private bool disposedValue;
    private readonly IPEndPoint serverEndpoint;

    public event UDPConnection.OnPacketHandler OnPacket
    {
      add
      {
        connection.OnPacket += value;
      }
      remove
      {
        connection.OnPacket -= value;
      }
    }

    public Client(IPEndPoint serverEndpoint, UDPConnection connection)
    {
      this.serverEndpoint = serverEndpoint;
      this.connection = connection;
    }


    public void Send(IPacket packet)
    {
      connection.Send(packet, serverEndpoint);
    }

    public void ProcessPackets()
    {
      this.connection.ProcessPackets();

    }

    public void Listen(int port)
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