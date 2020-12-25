using System;
using System.Collections.Generic;
using UnityEngine;

namespace Network.Physics
{
  [Serializable]
  public struct PhysicsState
  {
    private int _id;
    public int Id
    {
      get
      {
        return _id;
      }
      set
      {
        _id = value;
      }
    }
    private float[] _position;
    public Vector3 Position
    {
      get
      {
        return _position.ToVector3();
      }
      set
      {
        _position = value.ToFloatArray();
      }
    }
    public float[] _velocity;
    public Vector3 Velocity
    {
      get
      {
        return _velocity.ToVector3();
      }
      set
      {
        _velocity = value.ToFloatArray();
      }
    }
    private float[] _rotation;
    public Quaternion Rotation
    {
      get
      {
        return _rotation.ToQuaternion();
      }
      set
      {
        _rotation = value.ToFloatArray();
      }
    }

    private float[] _angularVelocity;
    public Vector3 AngularVelocity
    {
      get
      {
        return _angularVelocity.ToVector3();
      }
      set
      {
        _angularVelocity = value.ToFloatArray();
      }
    }

    public bool IsAtRest
    {
      get
      {
        return AngularVelocity.sqrMagnitude == 0 && Velocity.sqrMagnitude == 0;
      }
    }

    public static void Serialize(Serializer serializer, ref PhysicsState state)
    {
      serializer.SerializeInt(ref state._id);
      serializer.SerializeFloatArray(ref state._position);
      serializer.SerializeFloatArray(ref state._rotation);

      bool atRest = serializer.IsWriter ? state.IsAtRest : false;
      serializer.SerializeBool(ref atRest);
      if (!atRest)
      {
        serializer.SerializeFloatArray(ref state._velocity);
        serializer.SerializeFloatArray(ref state._angularVelocity);
      }
      else if (serializer.IsReader)
      {
        state._velocity = new float[] { 0, 0, 0 };
        state._angularVelocity = new float[] { 0, 0, 0 };
      }
    }

  }

}