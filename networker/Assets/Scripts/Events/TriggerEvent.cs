using Network;
using Network.Events;
using UnityEngine;

namespace Events
{
  public struct TriggerEvent : IGameEvent
  {
    public enum Trigger
    {
      Kick,
      Bomb
    }

    public int EventNumber { get; set; }
    public GameEvents Type => GameEvents.Trigger;

    public Trigger trigger;



    public TriggerEvent(Trigger trigger) : this()
    {
      this.trigger = trigger;
    }

    public void Serialize(Serializer serializer)
    {
      var triggerInt = (int)trigger;
      serializer.SerializeInt(ref triggerInt);
      if (serializer.IsReader)
      {
        trigger = (Trigger)triggerInt;
      }
    }
  }
}


