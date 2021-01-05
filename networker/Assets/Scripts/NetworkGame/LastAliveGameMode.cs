using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Events;
using Network.Events;

namespace Network.Game
{
  public class LastAliveGameMode
  {
    private GameServer server;

    private enum GameStates
    {
      Playing,
      WaitingForRestart
    }

    public const int RESTART_TIME_MS = 5000;

    private GameStates gameState = GameStates.Playing;

    private HashSet<int> playerDeaths = new HashSet<int>();
    private int PlayersLeft
    {
      get
      {
        return NetworkState.Players.Count() - playerDeaths.Count;
      }
    }
    public LastAliveGameMode(GameServer server)
    {
      this.server = server;
      NetworkState.OnEvent += HandleOnEvent;
    }
    private void HandleOnEvent(IEvent e)
    {
      HandleOnEvent((IGameEvent)e);
    }

    private void HandleOnEvent(IGameEvent e)
    {
      switch (e.Type)
      {
        case GameEvents.Death:
          HandleDeath((DeathEvent)e);
          break;
      }
    }

    private async void HandleDeath(DeathEvent e)
    {
      if (NetworkState.IsServer)
      {
        playerDeaths.Add(e.Player);
        if (gameState == GameStates.Playing && PlayersLeft <= 1)
        {
          var players = new HashSet<int>(NetworkState.Players.Keys);

          foreach (var death in playerDeaths)
          {
            players.Remove(death);
          }

          NetworkState.Server.InvokeEvent(new WinEvent(players.FirstOrDefault()));
          gameState = GameStates.WaitingForRestart;

          await Task.Delay(RESTART_TIME_MS);

          NetworkState.Server.InvokeEvent(new LoadSceneEvent("Level1"));
        }
      }
    }
  }
}