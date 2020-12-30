using UnityEngine;
using UnityEngine.Events;
using Network;
using Network.Physics;
using Events;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour
{
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

  public NetworkedBody instantiateObject;

  public NetworkedBody playerPrefab;


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

  private void Start()
  {
    // Make sure we have one and only one NetworkManager
    if (instance != null)
    {
      Destroy(gameObject);
      return;
    }

    instance = this;
    DontDestroyOnLoad(gameObject);

    if (NetworkState.IsServer)
    {
      NetworkState.Server.OnEvent += HandleOnEvent;
    }
    else if (NetworkState.IsClient)
    {
      NetworkState.Client.OnEvent += HandleOnEvent;
    }

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
        NetworkedBody networkedBody;
        if (iEvent.InstantiateType == InstantiateEvent.InstantiateTypes.Cube)
        {
          networkedBody = Instantiate(instantiateObject, iEvent.Position, Quaternion.identity);
        }
        else
        {
          networkedBody = Instantiate(playerPrefab, iEvent.Position, Quaternion.identity);
          networkedBody.GetComponent<PlayerController>().head.GetComponent<NetworkedBody>().playerAuthority = iEvent.PlayerAuthority;
        }
        networkedBody.playerAuthority = iEvent.PlayerAuthority;

        break;
      case GameEvents.LoadScene:
        var lsEvent = (LoadSceneEvent)gameEvent;
        SceneManager.LoadScene(lsEvent.Scene);
        waitingForSceneLoad = true;
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
