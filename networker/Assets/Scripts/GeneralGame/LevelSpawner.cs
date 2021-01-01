using System.Collections;
using System.Collections.Generic;
using Events;
using Network;
using Network.Events;
using UnityEngine;

public class LevelSpawner : MonoBehaviour
{
  public Vector3[] spawnPoints;

  private NetworkManager networkManager;

  private int latestSpawnIndex = 0;

  private void Awake()
  {
    if (NetworkState.IsServer)
    {
      foreach (var player in NetworkState.Server.Players.Values)
      {
        SpawnPlayer(player);
      }
    }

  }

  private void SpawnPlayer(int player)
  {
    NetworkState.Server.InvokeEvent(new InstantiateEvent(
      spawnPoints[(latestSpawnIndex++) % spawnPoints.Length],
      Quaternion.identity,
      InstantiateEvent.InstantiateTypes.Player,
      player,
      NetworkState.Server.FindNextFreeBodyId()
    ));
  }

  private void OnEnable()
  {
    NetworkManager.Instance.onEvent.AddListener(HandleOnEvent);

    if (NetworkState.IsServer)
    {
      NetworkState.Server.OnPlayerJoin += HandlePlayerJoin;
    }
  }

  private void OnDisable()
  {
    NetworkManager.Instance.onEvent.RemoveListener(HandleOnEvent);

    if (NetworkState.IsServer)
    {
      NetworkState.Server.OnPlayerJoin -= HandlePlayerJoin;
    }
  }

  private void HandlePlayerJoin(int playerId)
  {
    SpawnPlayer(playerId);
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
