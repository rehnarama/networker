using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransportPlatform : MonoBehaviour
{
  private Rigidbody body;
  private HashSet<Rigidbody> objectsOnTop = new HashSet<Rigidbody>();
  private Vector3 lastPosition;

  // Start is called before the first frame update
  void Start()
  {
    lastPosition = transform.position;
    body = GetComponent<Rigidbody>();
  }

  private void LateUpdate()
  {
    var deltaPosition = transform.position - lastPosition;

    HashSet<Rigidbody> destroyedObjects = new HashSet<Rigidbody>();

    foreach (var rb in objectsOnTop)
    {
      if (rb != null)
      {
        // rb.MovePosition(rb.position + deltaPosition);
        rb.position += deltaPosition;
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

    lastPosition = transform.position;
  }


  private void OnCollisionEnter(Collision other)
  {
    // other.transform.parent = transform;
    if (other.rigidbody != null)
    {
      objectsOnTop.Add(other.rigidbody);
    }
  }

  private void OnCollisionExit(Collision other)
  {
    // other.transform.parent = null;
    if (other.rigidbody != null)
    {
      objectsOnTop.Remove(other.rigidbody);
    }
  }
}
