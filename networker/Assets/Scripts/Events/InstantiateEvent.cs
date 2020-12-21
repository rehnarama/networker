using Network;
using Network.Events;
using UnityEngine;

namespace Events
{
  public struct InstantiateEvent : IGameEvent
  {
    public int EventNumber { get; set; }
    public GameEvents Type => GameEvents.Instantiate;

    private float[] _Position;
    public Vector3 Position { get => _Position.ToVector3(); set => _Position = value.ToFloatArray(); }

    public InstantiateEvent(int eventNumber, Vector3 position)
    {
      EventNumber = eventNumber;
      _Position = position.ToFloatArray();
    }


    public void Serialize(Serializer serializer)
    {
      serializer.SerializeFloatArray(ref _Position);
    }
  }
}