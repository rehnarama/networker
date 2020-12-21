using Network;
using Network.Events;

namespace Events
{
  public enum GameEvents
  {
    Instantiate,
  }

  public interface IGameEvent : IEvent
  {
    GameEvents Type { get; }

    void Serialize(Serializer serializer);
  }
}
