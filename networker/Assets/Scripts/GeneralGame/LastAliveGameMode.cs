using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Events;
using Network;
using Network.Events;
using UnityEngine;

public class LastAliveGameMode : MonoBehaviour
{

  private enum GameStates
  {
    Playing,
    WaitingForRestart
  }

  public const int RESTART_TIME_MS = 5000;

  private GameStates gameState = GameStates.Playing;

  public HashSet<int> PlayerDeaths { get; private set; } = new HashSet<int>();

  private int PlayersLeft
  {
    get
    {
      return NetworkState.Players.Count() - PlayerDeaths.Count;
    }
  }

  private void OnEnable()
  {
    NetworkManager.Instance.onEvent.AddListener(HandleOnEvent);
  }

  private void OnDisable()
  {
    NetworkManager.Instance.onEvent.RemoveListener(HandleOnEvent);
  }

  private void HandleOnEvent(IGameEvent e)
  {
    switch (e.Type)
    {
      case GameEvents.Death:
        HandleDeath((DeathEvent)e);
        break;
      case GameEvents.Win:
        HandleWin((WinEvent)e);
        break;
    }
  }

  private async void HandleWin(WinEvent e)
  {
    gameState = GameStates.WaitingForRestart;

    if (NetworkState.IsServer)
    {
      NetworkState.GameServer.PlayerScores[e.Player] = NetworkState.GameServer.PlayerScores.TryGetValue(e.Player, out var oldScore) ? oldScore + 1 : 1;
    }
    if (NetworkState.IsClient)
    {
      NetworkState.GameClient.PlayerScores[e.Player] = NetworkState.GameClient.PlayerScores.TryGetValue(e.Player, out var oldScore) ? oldScore + 1 : 1;
    }

    if (NetworkState.IsServer)
    {
      await Task.Delay(RESTART_TIME_MS);
      NetworkState.Server.InvokeEvent(new LoadSceneEvent("Level1"));
    }
  }

  private void HandleDeath(DeathEvent e)
  {
    PlayerDeaths.Add(e.Player);
    if (NetworkState.IsServer)
    {
      if (gameState == GameStates.Playing && PlayersLeft <= 1)
      {
        var players = new HashSet<int>(NetworkState.Players.Keys);

        foreach (var death in PlayerDeaths)
        {
          players.Remove(death);
        }


        UnityEngine.Debug.Log("Invoking win event");
        NetworkState.Server.InvokeEvent(new WinEvent(players.FirstOrDefault()));
        gameState = GameStates.WaitingForRestart;
      }
    }
  }
}
