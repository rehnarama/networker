using System.Net;

namespace Network
{
  using Network.Events;
  using Physics;

  public static class NetworkState
  {
    public static bool IsServer { get; private set; }
    public static bool IsClient { get; private set; }

    public static EventSerializer Serializer { get; private set; }

    public static void StartPhysicsServer(EventSerializer eventSerializer = null)
    {
      Serializer = eventSerializer;
      if (!IsServer)
      {
        IsServer = true;
        PhysicsServer.Instance.Listen();
      }
    }

    public static void StartPhysicsClient(IPEndPoint serverEndpoint, Events.EventSerializer eventSerializer = null, int port = Server.PORT + 1)
    {
      Serializer = eventSerializer;
      if (!IsClient)
      {
        IsClient = true;
        PhysicsClient.ServerEndpoint = serverEndpoint;
        PhysicsClient.Instance.Listen(port);
        PhysicsClient.Instance.TryJoin();
      }
    }

    public static MultiPlayerInput Input
    {
      get
      {
        if (IsServer)
        {
          return PhysicsServer.Instance.PlayerInputs;
        }
        else
        {
          return PhysicsClient.Instance.PlayerInputs;
        }
      }
    }

    public static int PlayerId
    {
      get
      {
        if (IsClient)
        {
          return PhysicsClient.Instance.PlayerId;
        }
        else
        {
          return -1;
        }
      }
    }
  }
}