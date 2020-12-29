using System.Collections;
using System.Collections.Generic;
using Events;
using Network;
using Network.Events;
using UnityEngine;

public class LevelSpawner : MonoBehaviour
{
  public Vector3[] spawnPoints;

  private void Awake()
  {
    if (NetworkState.IsServer)
    {
      int i = 0;
      foreach (var player in NetworkState.Server.Players.Values)
      {
        NetworkState.Server.InvokeEvent(new InstantiateEvent(
          spawnPoints[(i++) % spawnPoints.Length],
          InstantiateEvent.InstantiateTypes.Player,
          player,
          NetworkState.Server.FindNextFreeBodyId()
        ));
      }
    }
  }


  public void HandleOnEvent(IEvent e)
  {
    var gameEvent = (IGameEvent)e;
    if (NetworkState.IsServer)
    {
      if (gameEvent.Type == GameEvents.Death)
      {
        var deathEvent = (DeathEvent)gameEvent;
        var position = spawnPoints[Random.Range(0, spawnPoints.Length)];
        var players = FindObjectsOfType<PlayerController>();
        foreach (var player in players)
        {
          if (player.PlayerId == deathEvent.Player)
          {
            player.transform.position = position;
            break;
          }
        }
      }
    }
  }

}
