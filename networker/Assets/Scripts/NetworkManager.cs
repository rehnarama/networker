using UnityEngine;
using UnityEngine.Events;
using Network;
using Network.Physics;
using Events;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;

public class NetworkManager : MonoBehaviour
{
  [Serializable]
  public struct InstantiatePair
  {
    public InstantiateEvent.InstantiateTypes type;
    public NetworkedBody prefab;
  }

  private static NetworkManager instance;
  public static NetworkManager Instance
  {
    get
    {
      return instance;
    }
  }

  public KeyCode[] registredKeys;
  public string[] registredAxises;
  public UnityEvent<IGameEvent> onEvent;


  public InstantiatePair[] instantiateMap;
  private Dictionary<InstantiateEvent.InstantiateTypes, NetworkedBody> instantiatePrefabMap = new Dictionary<InstantiateEvent.InstantiateTypes, NetworkedBody>();




  private Queue<IGameEvent> bufferedEvents = new Queue<IGameEvent>();
  private bool waitingForSceneLoad = false;

  private void Update()
  {
    if (NetworkState.IsClient)
    {
      if (registredKeys != null)
      {
        foreach (var key in registredKeys)
        {
          NetworkState.Client.PlayerInput.SetDigital(
            (int)key,
            Input.GetKey(key)
          );
        }
      }

      if (registredAxises != null)
      {
        foreach (var axis in registredAxises)
        {
          NetworkState.Client.PlayerInput.SetAnalog(axis, Input.GetAxisRaw(axis));
        }
      }
    }

    ProcessBufferedEvents();
  }

  private void Awake()
  {
    // Make sure we have one and only one NetworkManager
    if (instance != null)
    {
      Destroy(gameObject);
      return;
    }

    instance = this;
    DontDestroyOnLoad(gameObject);

    foreach (var pair in instantiateMap)
    {
      instantiatePrefabMap[pair.type] = pair.prefab;
    }

    NetworkState.OnEvent += HandleOnEvent;
    SceneManager.sceneLoaded += HandleOnSceneLoad;
  }

  private void HandleOnSceneLoad(Scene arg0, LoadSceneMode arg1)
  {
    waitingForSceneLoad = false;
  }

  private void OnDestroy()
  {
    if (NetworkState.IsServer)
    {
      NetworkState.Server.OnEvent -= HandleOnEvent;
    }
    else if (NetworkState.IsClient)
    {
      NetworkState.Client.OnEvent -= HandleOnEvent;
    }

    SceneManager.sceneLoaded -= HandleOnSceneLoad;
  }

  private void ProcessBufferedEvents()
  {
    int count = bufferedEvents.Count;
    for (int i = 0; i < count; i++)
    {
      var e = bufferedEvents.Dequeue();
      HandleOnEvent(e);
    }
  }

  private void HandleOnEvent(Network.Events.IEvent e)
  {
    var gameEvent = (IGameEvent)e;

    if (waitingForSceneLoad)
    {
      bufferedEvents.Enqueue(gameEvent);
      return;
    }

    onEvent?.Invoke(gameEvent);

    switch (gameEvent.Type)
    {
      case GameEvents.Instantiate:
        var iEvent = (InstantiateEvent)gameEvent;
        NetworkedBody networkedBody = Instantiate(instantiatePrefabMap[iEvent.InstantiateType], iEvent.Position, iEvent.Rotation);
        networkedBody.playerAuthority = iEvent.PlayerAuthority;

        break;
      case GameEvents.LoadScene:
        var lsEvent = (LoadSceneEvent)gameEvent;
        SceneManager.LoadScene(lsEvent.Scene);
        waitingForSceneLoad = true;
        break;
      case GameEvents.Kick:
        var ke = (KickEvent)gameEvent;
        NetworkState.RegisteredBodies[ke.BodyId].body.AddForce(ke.Force, ForceMode.VelocityChange);
        break;
      case GameEvents.Destroy:
        var de = (DestroyEvent)gameEvent;
        if (NetworkState.RegisteredBodies.ContainsKey(de.BodyId))
        {
          Destroy(NetworkState.RegisteredBodies[de.BodyId].gameObject);
          NetworkState.Deregister(de.BodyId);
        }
        break;
    }
  }

  // Update is called once per frame
  void FixedUpdate()
  {
    if (NetworkState.IsServer)
    {
      NetworkState.Server.Tick();
      NetworkState.Server.ProcessPackets();
    }
    if (NetworkState.IsClient)
    {
      NetworkState.Client.Tick();
      NetworkState.Client.ProcessPackets();
    }

  }

  private void OnApplicationQuit()
  {
    if (NetworkState.IsServer)
    {
      NetworkState.Server.Dispose();
    }
    if (NetworkState.IsClient)
    {
      NetworkState.Client.Dispose();
    }

  }
}
