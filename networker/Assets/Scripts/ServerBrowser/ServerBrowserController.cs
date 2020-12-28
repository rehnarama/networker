﻿using System;
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

  public TMPro.TMP_Text nothingFoundText;

  public LoadingText serverLoadingText;

  private void Awake()
  {
    searchClient = new SignallingClient(Config.SIGNALLING_SERVER_ENDPOINT, Config.CLIENT_PORT);
    searchClient.OnHostList += OnHostList;

    hostList.OnJoin += HandleOnJoin;

    nothingFoundText.gameObject.SetActive(false);
  }

  private void HandleOnJoin(IPEndPoint endPoint)
  {
    NetworkState.StartPhysicsClient(endPoint, new PacketSerializer(), new GameEventSerializer());
    GotoLobby();
  }

  private void GotoLobby()
  {
    SceneManager.LoadScene("GameLobby", LoadSceneMode.Single);
  }

  public void Host()
  {
    NetworkState.StartPhysicsServer(new PacketSerializer(), new GameEventSerializer());
    NetworkState.StartPhysicsClient(new IPEndPoint(IPAddress.Loopback, Config.SERVER_PORT), new PacketSerializer(), new GameEventSerializer());
    GotoLobby();
  }

  private void OnHostList(SignallingHostList hostList)
  {
    this.hostList.HostList = hostList;
    if (hostList.Servers.Length == 0)
    {
      nothingFoundText.gameObject.SetActive(true);
    }
    else
    {
      nothingFoundText.gameObject.SetActive(false);
    }

    serverLoadingText.Reset();
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