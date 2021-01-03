using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NetworkedBody))]
public class MovingPlatform : MonoBehaviour
{
  public Vector3[] points;

  public float speed;
  private int nextPointIndex = 0;
  private Vector3 nextPoint;
  private Vector3 prevPoint;

  private Rigidbody body;




  void Awake()
  {
    body = GetComponent<Rigidbody>();

    body.position = points[nextPointIndex];
    nextPoint = points[nextPointIndex];
    CalculateNextPoint();
  }

  private void CalculateNextPoint()
  {
    nextPointIndex = (nextPointIndex + 1) % points.Length;
    prevPoint = nextPoint;
    nextPoint = points[nextPointIndex];
  }

  private void Update()
  {
    if (Vector3.Distance(transform.position, nextPoint) < 0.5f)
    {
      CalculateNextPoint();
    }

    var delta = nextPoint - transform.position;
    body.AddForce(delta.normalized * speed);
  }

}
