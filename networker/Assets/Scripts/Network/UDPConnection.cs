#define CUSTOM_SER

using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using Network.Packets;


namespace Network
{

  public class UDPConnection
  {
    protected UdpClient udpClient;
    protected ConcurrentQueue<UdpReceiveResult> receivedStack = new ConcurrentQueue<UdpReceiveResult>();
    private int listeningOnPort = -1;

    private bool disposedValue;

    public UDPConnection()
    {
      this.udpClient = new UdpClient();
    }

    private int TryBindPort(int port)
    {
      int offset = 0;
      while (true)
      {
        try
        {
          int portToTry = port + offset;
          this.udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, portToTry));
          return portToTry;
        }
        catch (SocketException e)
        {
          if (e.ErrorCode == 10048)
          {
            // Error Code 10048 is "Address already in use", which means port and IP
            // combination is already bound. Let's try another one!
            offset++;
          }
          else
          {
            // If not this error code, well who knows what could've gone wrong.
            // Let's crash and burn.
            UnityEngine.Debug.Log(e);
            throw e;
          }
        }
      }
    }

    public void Listen(int port = Server.PORT)
    {
      listeningOnPort = TryBindPort(port);
      UnityEngine.Debug.Log($"Bound port: {port}");

      Thread thread = new Thread(new ThreadStart(ReceiveLoop));
      thread.Start();
    }

    protected void Send<T>(T packet, IPEndPoint to) where T : IPacket
    {
      Send(packet, new IPEndPoint[] { to });
    }

    protected void Send<T>(T packet, IEnumerable<IPEndPoint> to) where T : IPacket
    {
      // #if CUSTOM_SER
      Serializer s = Serializer.CreateWriter();
      // TODO: use IPacket:Packet.Serialize function to write type as well!
      IPacket p = packet;
      Packet.Serialize(s, ref p);
      var data = s.ToByteArray();
      // #else
      // BinaryFormatter binaryFmt = new BinaryFormatter();
      // var stream = new MemoryStream();
      // binaryFmt.Serialize(stream, packet);
      // var data = stream.ToArray();
      // #endif

      foreach (var endpoint in to)
      {
        udpClient.Send(data, data.Length, endpoint);
      }
    }

    private async void ReceiveLoop()
    {
      while (this.udpClient != null)
      {
        var data = await this.udpClient.ReceiveAsync();

        receivedStack.Enqueue(data);
      }
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
          // TODO delete managed resources here
        }

        if (this.udpClient != null)
        {
          this.udpClient.Close();
          this.udpClient.Dispose();
          this.udpClient = null;
        }

        disposedValue = true;
      }
    }

    // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    ~UDPConnection()
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