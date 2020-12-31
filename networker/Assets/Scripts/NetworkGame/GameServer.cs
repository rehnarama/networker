using Events;

namespace Network.Game
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Network.Events;
  using Network.Physics;

  public class GameServer
  {
    private PhysicsServer physicsServer;

    private Dictionary<int, PlayerListItem> players;

    public GameServer(PhysicsServer physicsServer)
    {
      this.physicsServer = physicsServer;
      players = new Dictionary<int, PlayerListItem>();

      this.physicsServer.OnClientEvent += HandleOnEvent;
      this.physicsServer.OnPlayerJoin += HandleOnPlayerJoin;
    }

    private void HandleOnPlayerJoin(int playerId)
    {
      HandleNameChange(playerId, $"Player {playerId + 1}");
    }

    private void HandleOnEvent(IEvent e, int playerId)
    {
      var ge = (IGameEvent)e;

      switch (ge.Type)
      {
        case GameEvents.NameChange:
          var nce = (NameChangeEvent)ge;
          HandleNameChange(playerId, nce.NewName);
          break;
      }
    }

    private void HandleNameChange(int playerId, string name)
    {
      players[playerId] = new PlayerListItem(playerId, name);

      BroadcastPlayerList();
    }

    private void BroadcastPlayerList()
    {
      var playerList = new PlayerList(players.Values.ToArray());

      physicsServer.InvokeEvent(new PlayerListEvent(playerList));
    }
  }
}
