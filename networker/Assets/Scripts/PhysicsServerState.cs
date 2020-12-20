using System.Collections;
using System.Collections.Generic;
using Network;
using Network.Physics;
using UnityEngine;

public class PhysicsServerState : MonoBehaviour
{
  public TMPro.TMP_Text text;

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {
    if (NetworkState.IsServer)
    {
      var diagnostics = PhysicsServer.Instance.GetDiagnostics();
      text.text = $"Frame: {diagnostics.Frame}\nLatestAcked:{diagnostics.LatestAcked}";
    }
  }
}
