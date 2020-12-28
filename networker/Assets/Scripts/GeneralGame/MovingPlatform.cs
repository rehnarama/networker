using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetworkedBody))]
public class MovingPlatform : MonoBehaviour
{
  public Vector3[] points;
  public float timePerPoint;

  private float t;

  private int nextPointIndex = 0;
  private Vector3 nextPoint;
  private Vector3 prevPoint;

  private Rigidbody body;


  private HashSet<Rigidbody> objectsOnTop = new HashSet<Rigidbody>();
  private Vector3 lastPosition;


  void Awake()
  {
    body = GetComponent<Rigidbody>();

    body.position = points[nextPointIndex];
    nextPoint = points[nextPointIndex];
    CalculateNextPoint();

    lastPosition = body.position;

  }

  private void CalculateNextPoint()
  {
    nextPointIndex = (nextPointIndex + 1) % points.Length;
    prevPoint = nextPoint;
    nextPoint = points[nextPointIndex];

    var newVelocity = (nextPoint - prevPoint) / timePerPoint;
    body.velocity = newVelocity;
  }

  private void Update()
  {
    if (Vector3.Distance(transform.position, nextPoint) < 0.5f)
    {
      CalculateNextPoint();
    }
  }

  private void LateUpdate()
  {
    var deltaPosition = body.transform.position - lastPosition;

    foreach (var rb in objectsOnTop)
    {
      rb.transform.position += deltaPosition;
    }

    lastPosition = body.transform.position;
  }

  private void OnCollisionEnter(Collision other)
  {
    // other.rigidbody.transform.SetParent(transform);
    objectsOnTop.Add(other.rigidbody);
  }

  private void OnCollisionExit(Collision other)
  {
    // other.rigidbody.transform.parent = null;

    objectsOnTop.Remove(other.rigidbody);
  }
}
