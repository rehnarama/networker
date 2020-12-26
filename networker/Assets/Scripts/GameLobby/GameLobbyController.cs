using System;
using System.Collections;
using System.Collections.Generic;
using Events;
using Network;
using Network.Events;
using Network.Signalling;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameLobbyController : MonoBehaviour
{
  private SignallingClient hostClient;

  public InputDialogController inputDialogPrefab;

  public Button playButton;

  void Start()
  {
    if (NetworkState.IsServer)
    {
      var inputDialog = Instantiate(inputDialogPrefab, FindObjectOfType<Canvas>().transform);
      inputDialog.AllowCancel = false;
      inputDialog.OnInput += StartHost;
    }
  }

  void StartHost(string name)
  {
    if (NetworkState.IsServer)
    {
      hostClient = new SignallingClient(Config.SIGNALLING_SERVER_ENDPOINT, NetworkState.Server.Connection);
      hostClient.StartHosting(name);
      playButton.gameObject.SetActive(true);
    }
  }

  public void HostStartPlay()
  {
    if (NetworkState.IsServer)
    {
      NetworkState.Server.InvokeEvent(new LoadSceneEvent("Level1"));
    }
  }

  private void OnDestroy()
  {
    hostClient?.StopHosting();
  }
}
