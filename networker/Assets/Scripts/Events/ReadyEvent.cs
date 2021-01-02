using Network;
using Network.Events;

namespace Events
{
  public struct ReadyEvent : IGameEvent
  {
    public int EventNumber { get; set; }
    public GameEvents Type => GameEvents.Ready;


    private bool ready;
    public bool Ready { get => ready; set => ready = value; }


    public void Serialize(Serializer serializer)
    {
      serializer.SerializeBool(ref ready);
    }
  }
}


