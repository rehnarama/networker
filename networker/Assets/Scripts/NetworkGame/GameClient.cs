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


    public Dictionary<int, PlayerListItem> Players = new Dictionary<int, PlayerListItem>();
    public Dictionary<int, ReadyListItem> ReadyStates = new Dictionary<int, ReadyListItem>();

    public Dictionary<int, int> PlayerScores { get; private set; } = new Dictionary<int, int>();

    public event EventHandler<PlayerList> PlayerListUpdated;
    public event EventHandler<ReadyListItem[]> ReadyStatesUpdated;


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
            Players[pli.PlayerId] = pli;
          }
          PlayerListUpdated?.Invoke(this, ple.list);
          break;
        case GameEvents.ReadyList:
          var rle = (ReadyListEvent)ge;
          foreach (var rli in rle.readyStates)
          {
            ReadyStates[rli.PlayerId] = rli;
          }
          ReadyStatesUpdated?.Invoke(this, rle.readyStates);
          break;
      }
    }

    public bool IsReady(int playerId)
    {
      if (ReadyStates.ContainsKey(playerId))
      {
        return ReadyStates[playerId].Ready;
      }
      else
      {
        return false;
      }
    }
  }
}

