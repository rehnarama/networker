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
      Cube,
      Bomb
    }

    public int EventNumber { get; set; }
    public GameEvents Type => GameEvents.Instantiate;

    private float[] _Position;

    public Vector3 Position { get => _Position.ToVector3(); set => _Position = value.ToFloatArray(); }
    private float[] _Rotation;
    public Quaternion Rotation { get => _Rotation.ToQuaternion(); set => _Rotation = value.ToFloatArray(); }

    public InstantiateTypes InstantiateType { get; set; }
    private int _PlayerAuthority;

    public int PlayerAuthority { get => _PlayerAuthority; set => _PlayerAuthority = value; }

    public InstantiateEvent(Vector3 position, Quaternion rotation, InstantiateTypes type, int playerAuthority, int eventNumber = 0)
    {
      _Position = position.ToFloatArray();
      _Rotation = rotation.ToFloatArray();
      InstantiateType = type;
      _PlayerAuthority = playerAuthority;

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

      serializer.SerializeVector3(ref _Position);
      serializer.SerializeQuaternion(ref _Rotation);

      serializer.SerializeInt(ref _PlayerAuthority);
    }
  }
}