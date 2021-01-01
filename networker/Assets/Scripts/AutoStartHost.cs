using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Events;
using Network;
using UnityEngine;

public class AutoStartHost : MonoBehaviour
{
  private void Awake()
  {
    if (!NetworkState.IsClient && !NetworkState.IsServer)
    {
      NetworkState.StartPhysicsServer(new PacketSerializer(), new GameEventSerializer());
      NetworkState.StartPhysicsClient(new IPEndPoint(IPAddress.Loopback, Config.SERVER_PORT), new PacketSerializer(), new GameEventSerializer());
    }
  }
}
