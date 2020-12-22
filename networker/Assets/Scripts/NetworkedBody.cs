using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Network;
using Network.Physics;

[RequireComponent(typeof(Rigidbody))]
public class NetworkedBody : MonoBehaviour
{
  public int playerAuthority = -1;
  public int id = 0;

  private static int idCounter = 1;
  private static HashSet<int> takenIds = new HashSet<int>();

  private Rigidbody rb;

  public void Awake()
  {
    if (id != 0)
    {
      RegisterBody();
    }
  }

  public void RegisterBody()
  {
    rb = GetComponent<Rigidbody>();
    if (NetworkState.IsServer)
    {
      PhysicsServer.Instance.RegisterBody(id, rb);
    }
    else if (NetworkState.IsClient)
    {
      PhysicsClient.Instance.RegisterBody(id, rb);
    }

  }
}
