using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Events;
using Network;
using Network.Events;
using Network.Signalling;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerBrowserController : MonoBehaviour
{
  public HostListController hostList;

  private SignallingClient searchClient;


  private float lastRefresh = float.NegativeInfinity;
  public float refreshTime = 2f;

  private void Awake()
  {
    searchClient = new SignallingClient(Config.SIGNALLING_SERVER_ENDPOINT, Config.CLIENT_PORT);
    searchClient.OnHostList += OnHostList;

    hostList.OnJoin += HandleOnJoin;
  }

  private void HandleOnJoin(IPEndPoint endPoint)
  {
    NetworkState.StartPhysicsClient(endPoint, new PacketSerializer(), new EventSerializer());
    GotoLobby();
  }

  private void GotoLobby()
  {
    SceneManager.LoadScene("GameLobby", LoadSceneMode.Single);
  }

  public void Host()
  {
    NetworkState.StartPhysicsServer(new PacketSerializer(), new GameEventSerializer());
    GotoLobby();
  }

  private void OnHostList(SignallingHostList hostList)
  {
    this.hostList.HostList = hostList;
  }

  private void Update()
  {
    if (Time.time - lastRefresh > refreshTime)
    {
      searchClient.RequestList();
      lastRefresh = Time.time;
    }

    searchClient?.Tick();
  }
}
