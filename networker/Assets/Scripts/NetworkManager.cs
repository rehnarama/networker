using UnityEngine;
using UnityEngine.Events;
using Network;
using Network.Physics;
using Events;

public class NetworkManager : MonoBehaviour
{
  public KeyCode[] registredKeys;
  public UnityEvent<IGameEvent> onEvent;

  public GameObject instantiateObject;

  public GameObject playerPrefab;

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

      PhysicsClient.Instance.PlayerInput.SetAnalog(1, Input.GetAxis("Horizontal"));
      PhysicsClient.Instance.PlayerInput.SetAnalog(2, Input.GetAxis("Vertical"));
    }

    if (NetworkState.IsServer)
    {
      if (Input.GetKeyDown(KeyCode.Space))
      {
        PhysicsServer.Instance.InvokeEvent(new InstantiateEvent());
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
        Instantiate(instantiateObject, new Vector3(0, 10, 0), Quaternion.identity);
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

  private void OnApplicationQuit() {
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
