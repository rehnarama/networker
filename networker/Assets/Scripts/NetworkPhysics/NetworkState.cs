using System.Net;

namespace Network
{
  using System;
  using Network.Events;
  using Network.Packets.Signalling;
  using Network.Signalling;
  using Physics;

  public static class NetworkState
  {
    public static bool IsServer { get; private set; }
    public static bool IsClient { get; private set; }

    public static PhysicsServer Server { get; set; }
    public static PhysicsClient Client { get; set; }

    public static EventSerializer Serializer { get; private set; }

    public static event PhysicsServer.OnEventHandler OnEvent {
      add
      {
        if (NetworkState.IsServer)
        {
          NetworkState.Server.OnEvent += value;
        }
        else
        {
          NetworkState.Client.OnEvent += value;
        }
      }
      remove
      {
        if (NetworkState.IsServer)
        {
          NetworkState.Server.OnEvent -= value;
        }
        else
        {
          NetworkState.Client.OnEvent -= value;
        }
      }
    }

    public static void StartPhysicsServer(IPacketSerializer packetSerializer, EventSerializer eventSerializer = null, int port = Config.SERVER_PORT)
    {
      Serializer = eventSerializer;
      if (!IsServer)
      {
        IsServer = true;

        var udpConnection = new UDPConnection(packetSerializer);
        var udpServer = new Network.Server(udpConnection);
        Server = new PhysicsServer(udpServer);
        Server.Listen(port);

      }
    }

    internal static void RegisterBody(int id, NetworkedBody networkedBody)
    {
      if (IsServer)
      {
        Server.RegisterBody(id, networkedBody);
      }
      else
      {
        Client.RegisterBody(id, networkedBody);
      }
    }

    public static void StartPhysicsClient(IPEndPoint serverEndpoint, IPacketSerializer packetSerializer, Events.EventSerializer eventSerializer = null, int port = Config.CLIENT_PORT)
    {
      Serializer = eventSerializer;
      if (!IsClient)
      {
        IsClient = true;
        var udpConnection = new UDPConnection(packetSerializer);
        var udpClient = new Network.Client(serverEndpoint, udpConnection);
        Client = new PhysicsClient(udpClient);
        Client.Listen(port);
        Client.TryJoin();
      }
    }

    public static MultiPlayerInput Input
    {
      get
      {
        if (IsServer)
        {
          return Server.PlayerInputs;
        }
        else
        {
          return Client.PlayerInputs;
        }
      }
    }

    public static MultiPlayerInput PreviousInput
    {
      get
      {
        if (IsServer)
        {
          return Server.PreviousPlayerInputs;
        }
        else
        {
          return Client.PreviousPlayerInputs;
        }
      }
    }

    public static int PlayerId
    {
      get
      {
        if (IsClient)
        {
          return Client.PlayerId;
        }
        else
        {
          return -1;
        }
      }
    }
  }
}