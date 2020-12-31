using Network;
using Network.Events;
using Network.Game;
using UnityEngine;

namespace Events
{
  public struct PlayerListEvent : IGameEvent
  {
    public int EventNumber { get; set; }
    public GameEvents Type => GameEvents.PlayerList;

    public PlayerList list;

    public PlayerListEvent(PlayerList list) : this()
    {
      this.list = list;
    }


    public void Serialize(Serializer serializer)
    {
      list.Serialize(serializer);
    }
  }
}

