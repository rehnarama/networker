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
    public int PlayerAuthority { get; set; }
    public int BodyId { get; set; }

    public InstantiateEvent(Vector3 position, InstantiateTypes type, int playerAuthority, int bodyId, int eventNumber = 0)
    {
      _Position = position.ToFloatArray();
      InstantiateType = type;
      PlayerAuthority = playerAuthority;
      BodyId = bodyId;
      EventNumber = eventNumber;
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