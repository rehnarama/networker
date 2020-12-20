using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;


namespace Network
{
  using Packets;

  public class Server : UDPConnection, IDisposable
  {
    private bool disposedValue;
    public const int PORT = 1303;

    public HashSet<IPEndPoint> clients = new HashSet<IPEndPoint>();

    public void Broadcast(IPacket packet)
    {
      Send(packet, clients);
    }

    public void ProcessPackets()
    {
      BinaryFormatter binaryFmt = new BinaryFormatter();

      while (this.receivedStack.Count > 0)
      {
        if (this.receivedStack.TryDequeue(out var data))
        {
          var s = Serializer.CreateReader(data.Buffer);
          IPacket packet = new JoinPacket(); // Just assign something to make ref happy
          Packet.Serialize(s, ref packet);

          OnPacket(packet, data.RemoteEndPoint);
        }
      }
    }

    protected virtual void OnPacket(IPacket packet, IPEndPoint remoteEndPoint)
    {
      switch (packet.Type)
      {
        case PacketType.Join:
          clients.Add(remoteEndPoint);
          OnJoin((JoinPacket)packet, remoteEndPoint);
          UnityEngine.Debug.Log($"Join packet from {remoteEndPoint}");
          break;
        default:
          break;
      }
    }

    public virtual void OnJoin(JoinPacket packet, IPEndPoint remoteEndPoint)
    { }
  }

}