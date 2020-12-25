using UnityEngine;
using UnityEngine.Events;
using Network;
using Network.Physics;
using Events;

public class NetworkManager : MonoBehaviour
{
  public KeyCode[] registredKeys;
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
          PhysicsClient.Instance.PlayerInput.SetDigital(
            (int)key,
            Input.GetKey(key)
          );
        }
      }

      PhysicsClient.Instance.PlayerInput.SetDigital(
        0, 
        Input.GetMouseButton(0)
      );

      PhysicsClient.Instance.PlayerInput.SetAnalog(1, Input.GetAxis("Horizontal"));
      PhysicsClient.Instance.PlayerInput.SetAnalog(2, Input.GetAxis("Vertical"));

      PhysicsClient.Instance.PlayerInput.SetAnalog(3, Input.GetAxis("Mouse X"));
      PhysicsClient.Instance.PlayerInput.SetAnalog(4, Input.GetAxis("Mouse Y"));
    }

    if (NetworkState.IsServer)
    {
      if (Input.GetKeyDown(KeyCode.F1))
      {
        foreach (var player in PhysicsServer.Instance.Players.Values)
        {
          PhysicsServer.Instance.InvokeEvent(new InstantiateEvent(
            new Vector3(0, 10, 0),
            InstantiateEvent.InstantiateTypes.Player,
            player,
            PhysicsServer.Instance.FindNextFreeBodyId()
          ));
        }
      }
    }
  }

  private void Start()
  {
    if (NetworkState.IsServer)
    {
      PhysicsServer.Instance.OnEvent += HandleOnEvent;
    }
    else if (NetworkState.IsClient)
    {
      PhysicsClient.Instance.OnEvent += HandleOnEvent;
    }
  }

  private void OnDestroy()
  {
    if (NetworkState.IsClient)
    {
      PhysicsClient.Instance.OnEvent -= HandleOnEvent;
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
    }
  }


  // Update is called once per frame
  void FixedUpdate()
  {
    if (NetworkState.IsServer)
    {
      PhysicsServer.Instance.ProcessPackets();
      PhysicsServer.Instance.Tick();
    }
    if (NetworkState.IsClient)
    {
      PhysicsClient.Instance.ProcessPackets();
      PhysicsClient.Instance.Tick();
    }
  }

  private void OnApplicationQuit()
  {
    if (NetworkState.IsServer)
    {
      PhysicsServer.Instance.Dispose();
    }
    if (NetworkState.IsClient)
    {
      PhysicsClient.Instance.Dispose();
    }

  }
}
