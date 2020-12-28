using System.Collections;
using System.Collections.Generic;
using Events;
using Network;
using UnityEngine;

public class Lava : MonoBehaviour
{
  private void OnCollisionEnter(Collision other)
  {
    if (NetworkState.IsServer)
    {
      if (other.gameObject.TryGetComponent<PlayerController>(out var player))
      {
        NetworkState.Server.InvokeEvent(new DeathEvent(player.PlayerId));
      }
    }
  }
}
