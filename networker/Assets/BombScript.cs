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

  private void Start()
  {
    spawnTime = Time.time;
    GetComponent<Rigidbody>().velocity = transform.rotation * Vector3.forward * speed;
  }

  // Update is called once per frame
  void Update()
  {
    if (Time.time - spawnTime > liveTime)
    {
      NetworkState.Destroy(gameObject);
    }
  }
  void OnCollisionEnter(Collision collision)
  {
    if (NetworkState.IsServer)
    {
      NetworkState.Destroy(gameObject);
      if (collision.gameObject.TryGetComponent<NetworkedBody>(out var nb))
      {
        NetworkState.Server.InvokeEvent(new KickEvent(
          nb.id,
          (transform.rotation * Vector3.forward + Vector3.up).normalized * bombPower
        ));
      }
    }
  }
}
