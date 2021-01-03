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

  public Rigidbody body;

  public void Start()
  {
    if (id != 0)
    {
      RegisterBody();
    }
    else
    {
      id = idCounter + 1;
      RegisterBody();
    }
  }

  public void RegisterBody()
  {
    idCounter = Mathf.Max(idCounter, id);
    body = GetComponent<Rigidbody>();
    NetworkState.RegisterBody(id, this);
  }

  private void OnDestroy()
  {
    NetworkState.Deregister(id);
  }
}
