﻿using System.Collections;
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

  private void LateUpdate()
  {
    var deltaPosition = body.transform.position - lastPosition;

    HashSet<Rigidbody> destroyedObjects = new HashSet<Rigidbody>();

    foreach (var rb in objectsOnTop)
    {
      if (rb != null)
      {
        rb.transform.position += deltaPosition;
      }
      else
      {
        destroyedObjects.Add(rb);
      }
    }
    foreach (var obj in destroyedObjects)
    {
      objectsOnTop.Remove(obj);
    }

    lastPosition = body.transform.position;
  }

  private void OnCollisionEnter(Collision other)
  {
    if (other.rigidbody != null)
    {
      objectsOnTop.Add(other.rigidbody);
    }
  }

  private void OnCollisionExit(Collision other)
  {
    if (other.rigidbody != null)
    {
      objectsOnTop.Remove(other.rigidbody);
    }
  }
}
