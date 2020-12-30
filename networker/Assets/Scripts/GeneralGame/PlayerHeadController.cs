using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetworkedBody))]
public class PlayerHeadController : MonoBehaviour
{
  public Camera playerCamera;

  private NetworkedBody nb;

  private void Awake()
  {
    nb = GetComponent<NetworkedBody>();
  }
}
