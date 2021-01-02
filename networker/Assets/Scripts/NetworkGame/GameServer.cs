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

    public Dictionary<int, PlayerListItem> Players = new Dictionary<int, PlayerListItem>();
    public Dictionary<int, ReadyListItem> ReadyStates = new Dictionary<int, ReadyListItem>();

    public event EventHandler<ReadyListItem[]> ReadyStatesUpdated;

    public GameServer(PhysicsServer physicsServer)
    {
      this.physicsServer = physicsServer;

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
        case GameEvents.Ready:
          var rle = (ReadyEvent)ge;
          HandleReadyChange(playerId, rle.Ready);
          break;
      }
    }

    private void HandleNameChange(int playerId, string name)
    {
      Players[playerId] = new PlayerListItem(playerId, name);

      BroadcastPlayerList();
    }

    private void HandleReadyChange(int playerId, bool ready)
    {
      ReadyStates[playerId] = new ReadyListItem() { PlayerId = playerId, Ready = ready };

      BroadcastReadyStates();
    }

    public void ResetReadyStates()
    {
      ReadyStates = new Dictionary<int, ReadyListItem>();
      BroadcastReadyStates();
    }

    private void BroadcastReadyStates()
    {
      var statesArray = ReadyStates.Values.ToArray();
      ReadyStatesUpdated?.Invoke(this, statesArray);

      physicsServer.InvokeEvent(new ReadyListEvent() { readyStates = statesArray });
    }

    private void BroadcastPlayerList()
    {
      var playerList = new PlayerList(Players.Values.ToArray());

      physicsServer.InvokeEvent(new PlayerListEvent(playerList));
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
