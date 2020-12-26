using System.Collections;
using System.Collections.Generic;
using Network;
using Network.Signalling;
using UnityEngine;

public class GameLobbyController : MonoBehaviour
{
  private SignallingClient hostClient;

  void Start()
  {
    if (NetworkState.IsServer)
    {
      hostClient = new SignallingClient(Config.SIGNALLING_SERVER_ENDPOINT, NetworkState.Server.Connection);
      hostClient.StartHosting();
    }
  }

  private void OnDestroy() {
    hostClient?.StopHosting();
  }
}
