using UnityEngine;
using Network;
using Network.Physics;

public class NetworkManager : MonoBehaviour
{
  public KeyCode[] registredKeys;

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
}
