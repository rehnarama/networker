using Network;
using Network.Events;
using UnityEngine;

namespace Events
{
  public struct KickEvent : IGameEvent
  {
    public int EventNumber { get; set; }
    public GameEvents Type => GameEvents.Kick;


    private int bodyId;
    public int BodyId { get => bodyId; set => bodyId = value; }
    private float[] force;
    public Vector3 Force { get => force.ToVector3(); set => force = value.ToFloatArray(); }

    public KickEvent(int BodyId, Vector3 Force) : this()
    {
      bodyId = BodyId;
      force = Force.ToFloatArray();
    }

    public void Serialize(Serializer serializer)
    {
      serializer.SerializeInt(ref bodyId);
      serializer.SerializeVector3(ref force);
    }
  }
}

