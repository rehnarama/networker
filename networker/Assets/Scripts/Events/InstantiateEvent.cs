using Network;
using Network.Events;
using UnityEngine;

namespace Events
{
  public struct InstantiateEvent : IGameEvent
  {
    public enum InstantiateTypes
    {
      Player,
      Cube
    }

    public int EventNumber { get; set; }
    public GameEvents Type => GameEvents.Instantiate;

    private float[] _Position;
    public Vector3 Position { get => _Position.ToVector3(); set => _Position = value.ToFloatArray(); }

    public InstantiateTypes InstantiateType { get; set; }

    public InstantiateEvent(int eventNumber, Vector3 position, InstantiateTypes type)
    {
      EventNumber = eventNumber;
      _Position = position.ToFloatArray();
      InstantiateType = type;
    }


    public void Serialize(Serializer serializer)
    {
      int type = (int)InstantiateType;
      serializer.SerializeInt(ref type);
      if (serializer.IsReader)
      {
        InstantiateType = (InstantiateTypes)type;
      }

      serializer.SerializeFloatArray(ref _Position);
    }
  }
}