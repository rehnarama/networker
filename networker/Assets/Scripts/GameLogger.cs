using System;
using System.Collections;
using System.Collections.Generic;
using Events;
using Network;
using UnityEngine;

public class GameLogger : MonoBehaviour
{
  public EventLogController log;

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

  private void HandleWin(WinEvent e)
  {
    var playerName = NetworkState.Players[e.Player].Name;
    log.Log($"<color=red>{playerName}</color> won!");
    log.Log($"<color=red>{playerName}</color> now has {NetworkState.GameClient.PlayerScores[e.Player]} points");
    log.Log($"Restarting the game soon");
  }

  private void HandleDeath(DeathEvent e)
  {
    var playerName = NetworkState.Players[e.Player].Name;
    log.Log($"<color=red>{playerName}</color> died");
  }
}
