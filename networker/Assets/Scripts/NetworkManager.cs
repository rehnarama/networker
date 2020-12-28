using UnityEngine;
using UnityEngine.Events;
using Network;
using Network.Physics;
using Events;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour
{
  private static NetworkManager instance;

  public KeyCode[] registredKeys;
  public string[] registredAxises;
  public UnityEvent<IGameEvent> onEvent;

  public NetworkedBody instantiateObject;

  public NetworkedBody playerPrefab;

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
  }

  private void HandleOnEvent(Network.Events.IEvent e)
  {
    var gameEvent = (IGameEvent)e;
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
        }
        networkedBody.playerAuthority = iEvent.PlayerAuthority;
        networkedBody.id = iEvent.BodyId;

        networkedBody.RegisterBody();
        break;
      case GameEvents.LoadScene:
        var lsEvent = (LoadSceneEvent)gameEvent;
        SceneManager.LoadScene(lsEvent.Scene);
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
