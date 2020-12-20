using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MoveScript : MonoBehaviour
{
  private Rigidbody rb;
  private NetworkedBody nb;
  // Start is called before the first frame update
  void Start()
  {
    rb = GetComponent<Rigidbody>();
    nb = GetComponent<NetworkedBody>();
  }

  // Update is called once per frame
  void FixedUpdate()
  {
    var x = Network.NetworkState.Input.For(nb.playerAuthority).GetAnalog(1);
    var z = Network.NetworkState.Input.For(nb.playerAuthority).GetAnalog(2);

    rb.AddForce(new Vector3(x, 0, z) * 20f);
  }
}
