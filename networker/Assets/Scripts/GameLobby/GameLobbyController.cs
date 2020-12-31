using System;
using System.Collections;
using System.Collections.Generic;
using Events;
using Network;
using Network.Events;
using Network.Game;
using Network.Signalling;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameLobbyController : MonoBehaviour
{
  private SignallingClient hostClient;

  public InputDialogController inputDialogPrefab;

  public Button playButton;

  public PlayerListController playerList;

  public GameObject lobbyUI;

  void Start()
  {
    if (NetworkState.IsServer)
    {
      lobbyUI.SetActive(false);
      var inputDialog = Instantiate(inputDialogPrefab, FindObjectOfType<Canvas>().transform);
      inputDialog.AllowCancel = false;
      inputDialog.OnInput += StartHost;
    }
  }

  private void OnEnable()
  {
    if (NetworkState.IsClient)
    {
      NetworkState.GameClient.PlayerListUpdated += HandlePlayerListUpdated;
    }
  }

  private void OnDisable()
  {
    if (NetworkState.IsClient)
    {
      NetworkState.GameClient.PlayerListUpdated -= HandlePlayerListUpdated;
    }
  }

  private void HandlePlayerListUpdated(object sender, PlayerList e)
  {
    playerList.List = e;
  }

  void StartHost(string name)
  {
    if (NetworkState.IsServer)
    {
      hostClient = new SignallingClient(Config.SIGNALLING_SERVER_ENDPOINT, NetworkState.Server.Connection);
      hostClient.StartHosting(name);
      playButton.gameObject.SetActive(true);

      lobbyUI.SetActive(true);
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
