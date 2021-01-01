using System;
using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Network.Packets;


namespace Network
{

  public class UDPConnection
  {
    public static Random SimulationRNG = new Random();
    public static bool SimulatePacketLoss = false;
    public static float PacketLossSimulationPercentage = 0.1f;
    public static int LatencySimulation = 0;

    protected UdpClient udpClient;
    protected ConcurrentQueue<UdpReceiveResult> receivedStack = new ConcurrentQueue<UdpReceiveResult>();
    private int port = -1;
    public int Port
    {
      get
      {
        return port;
      }
    }

    private bool disposedValue;

    public float AvgInPacketSize { get; set; } = 0f;
    public float AvgOutPacketSize { get; set; } = 0f;

    public delegate void OnPacketHandler(IPacket packet, IPEndPoint from);
    public event OnPacketHandler OnPacket;

    IPacketSerializer packetSerializer;

    public UDPConnection(IPacketSerializer packetSerializer)
    {
      this.packetSerializer = packetSerializer;
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
            Console.Error.WriteLine(e);
            throw e;
          }
        }
      }
    }

    public void Listen(int port)
    {
      this.port = TryBindPort(port);
      Console.WriteLine($"Bound port: {this.port}");

      Thread thread = new Thread(new ThreadStart(ReceiveLoop));
      thread.Start();
    }

    public void Send<T>(T packet, IPEndPoint to) where T : IPacket
    {
      Send(packet, new IPEndPoint[] { to });
    }

    public async void Send<T>(T packet, IEnumerable<IPEndPoint> to) where T : IPacket
    {
      Serializer s = Serializer.CreateWriter();
      IPacket p = packet;
      packetSerializer.Serialize(s, ref p);
      var data = s.ToByteArray();

      AvgOutPacketSize = (AvgOutPacketSize * 100 + data.Length) / 101f;

      if (SimulatePacketLoss)
      {
        if (SimulationRNG.NextDouble() < PacketLossSimulationPercentage)
        {
          return;
        }
      }

      if (LatencySimulation > 0)
      {
        await System.Threading.Tasks.Task.Delay(LatencySimulation);
      }

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
        AvgInPacketSize = (AvgInPacketSize * 100 + data.Buffer.Length) / 101f;

        receivedStack.Enqueue(data);
      }
    }

    public void ProcessPackets()
    {
      while (this.receivedStack.Count > 0)
      {
        if (this.receivedStack.TryDequeue(out var data))
        {
          var s = Serializer.CreateReader(data.Buffer);
          IPacket packet = new JoinPacket();
          packetSerializer.Serialize(s, ref packet);

          OnPacket?.Invoke(packet, data.RemoteEndPoint);
        }
      }
    }

    public virtual void Dispose(bool disposing)
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