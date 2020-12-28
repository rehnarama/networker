using System.Collections;
using System.Collections.Generic;
using Events;
using Network;
using UnityEngine;

public class OnLevel1Load : MonoBehaviour
{
  private void Awake()
  {

    if (NetworkState.IsServer)
    {
      var x = 0;
      foreach (var player in NetworkState.Server.Players.Values)
      {
        NetworkState.Server.InvokeEvent(new InstantiateEvent(
          new Vector3(x++, 4, 0),
          InstantiateEvent.InstantiateTypes.Player,
          player,
          NetworkState.Server.FindNextFreeBodyId()
        ));
      }
    }
  }
}
