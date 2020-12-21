using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    string t = "";
    if (NetworkState.IsServer)
    {
      var diagnostics = PhysicsServer.Instance.GetDiagnostics();
      var properties = diagnostics.GetType().GetProperties();
      t += $"Server Diagnostics\n";
      t += $"\tFrame: {diagnostics.Frame}\n";
      t += $"\tN UnACKed Frames: {diagnostics.UnackedFrames}\n";
      t += $"\tPlayers: {diagnostics.PlayersJoined}\n";
      t += $"\tAvgInPacketSize: {RoundTo10(diagnostics.AvgInPacketSize)}\n";
      t += $"\tAvgOutPacketSize: {RoundTo10(diagnostics.AvgOutPacketSize)}\n";
    }
    if (NetworkState.IsClient)
    {
      var diagnostics = PhysicsClient.Instance.GetDiagnostics();
      t += $"Client Diagnostics:\n";
      t += $"\tCurrent Frame: {diagnostics.CurrentFrame}\n";
      t += $"\tBuffered Frames: {diagnostics.BufferedFrames}\n";
      t += $"\tAvgInPacketSize: {RoundTo10(diagnostics.AvgInPacketSize)}\n";
      t += $"\tAvgOutPacketSize: {RoundTo10(diagnostics.AvgOutPacketSize)}\n";
    }

    text.text = t;

  }

  private float RoundTo10(float n)
  {
    return Mathf.Round(n / 10) * 10;
  }
}
