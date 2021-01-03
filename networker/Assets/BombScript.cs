using System.Collections;
using System.Collections.Generic;
using Events;
using Network;
using UnityEngine;

public class BombScript : MonoBehaviour
{

  public float bombPower;

  public float liveTime;
  public float speed;

  private float spawnTime;

  private Vector3 previousPosition;
  private Rigidbody body;

  private void Start()
  {
    spawnTime = Time.time;
    body = GetComponent<Rigidbody>();
    body.velocity = transform.rotation * Vector3.forward * speed;
    previousPosition = body.position;
  }

  // Update is called once per frame
  void Update()
  {
    if (Time.time - spawnTime > liveTime)
    {
      NetworkState.Destroy(gameObject);
    }

  }

  private void FixedUpdate()
  {
    if (NetworkState.IsServer)
    {
      var delta = body.position - previousPosition;
      var ray = new Ray(previousPosition, delta);
      if (Physics.Raycast(ray, out var hit, delta.magnitude))
      {
        OnHit(hit.transform.gameObject);
      }

      previousPosition = body.position;
    }
  }

  private void OnHit(GameObject otherGo)
  {
    if (NetworkState.IsServer)
    {
      NetworkState.Destroy(gameObject);
      if (otherGo.TryGetComponent<NetworkedBody>(out var nb))
      {
        NetworkState.Server.InvokeEvent(new KickEvent(
          nb.id,
          (transform.rotation * Vector3.forward + Vector3.up).normalized * bombPower
        ));
      }
    }
  }
}
