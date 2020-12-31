using Events;

namespace Network.Game
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Network.Events;
  using Network.Physics;

  public class GameClient
  {
    private PhysicsClient physicsClient;

    private Dictionary<int, PlayerListItem> players = new Dictionary<int, PlayerListItem>();

    public event EventHandler<PlayerList> PlayerListUpdated;

    public GameClient(PhysicsClient physicsClient)
    {
      this.physicsClient = physicsClient;

      this.physicsClient.OnEvent += HandleOnEvent;
    }

    private void HandleOnEvent(IEvent e)
    {
      var ge = (IGameEvent)e;

      switch (ge.Type)
      {
        case GameEvents.PlayerList:
          var ple = (PlayerListEvent)ge;
          foreach (var pli in ple.list.Players)
          {
            players[pli.PlayerId] = pli;
          }
          PlayerListUpdated?.Invoke(this, ple.list);
          break;
      }
    }
  }
}

