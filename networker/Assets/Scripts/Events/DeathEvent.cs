using Network;
using Network.Events;
using UnityEngine;

namespace Events
{
  public struct DeathEvent : IGameEvent
  {
    public int EventNumber { get; set; }
    public GameEvents Type => GameEvents.Death;

    private int player;
    public int Player { get => player; set => player = value; }

    public DeathEvent(int player) : this()
    {
      this.player = player;
    }

    public void Serialize(Serializer serializer)
    {
      serializer.SerializeInt(ref player);
    }
  }
}

