using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Network;
using Network.Physics;

[RequireComponent(typeof(Rigidbody))]
public class NetworkedBody : MonoBehaviour
{
  public int playerAuthority = -1;
  public int id = 0;

  public bool predictMovement;
  public PhysicsState? TargetState;

  private static int idCounter = 1;

  public Rigidbody body { get; private set; }

  public void Start()
  {
    if (id != 0)
    {
      RegisterBody();
    }
    else
    {
      id = idCounter + 1;
      RegisterBody();
    }
  }

  private void Update()
  {

    if (playerAuthority != NetworkState.PlayerId && TargetState.HasValue) 
    {
      if (predictMovement)
      {
        body.position = Vector3.MoveTowards(body.position, TargetState.Value.Position, PhysicsConstants.PREDICTION_MAX_MOVE);
        body.rotation = Quaternion.RotateTowards(body.rotation, TargetState.Value.Rotation, PhysicsConstants.PREDICTION_MAX_ANGLE_MOVE);
      }
      else
      {
        body.position = TargetState.Value.Position;
        body.rotation = TargetState.Value.Rotation;
      }
      body.velocity = TargetState.Value.Velocity;
      body.angularVelocity = TargetState.Value.AngularVelocity;
    }
  }

  public void RegisterBody()
  {
    idCounter = Mathf.Max(idCounter, id);
    body = GetComponent<Rigidbody>();
    NetworkState.RegisterBody(id, this);
  }

  private void OnDestroy()
  {
    NetworkState.Deregister(id);
  }
}
