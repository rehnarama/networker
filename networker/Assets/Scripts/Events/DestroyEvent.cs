using Network;
using Network.Events;
using UnityEngine;

namespace Events
{
  public struct DestroyEvent : IGameEvent
  {
    public int EventNumber { get; set; }
    public GameEvents Type => GameEvents.Destroy;

    private int bodyId;
    public int BodyId { get => bodyId; set => bodyId = value; }

    public DestroyEvent(int BodyId) : this()
    {
      bodyId = BodyId;
    }

    public void Serialize(Serializer serializer)
    {
      serializer.SerializeInt(ref bodyId);
    }
  }
}


