﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Network;
using Network.Physics;
using UnityEngine;

public class NetworkDiagnostics : MonoBehaviour
{
  public TMPro.TMP_Text text;
  private static bool shouldShow = false;

  // Update is called once per frame
  void Update()
  {
    if (Input.GetKeyDown(KeyCode.F1))
    {
      shouldShow = !shouldShow;
    }
    if (Input.GetKeyDown(KeyCode.F2))
    {
      UDPConnection.SimulatePacketLoss = !UDPConnection.SimulatePacketLoss;
    }
    if (Input.GetKeyDown(KeyCode.F3))
    {
      UDPConnection.LatencySimulation += 20;
    }
    if (Input.GetKeyDown(KeyCode.F4))
    {
      UDPConnection.LatencySimulation -= 20;

    }


    if (shouldShow)
    {
      string t = "";

      t += $"Simulate {UDPConnection.PacketLossSimulationPercentage * 100}% packet loss? {UDPConnection.SimulatePacketLoss}\n";
      t += $"Latency simulation? {UDPConnection.LatencySimulation}ms\n";

      if (NetworkState.IsServer)
      {
        var diagnostics = NetworkState.Server.GetDiagnostics();
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
        var diagnostics = NetworkState.Client.GetDiagnostics();
        t += $"Client Diagnostics:\n";
        t += $"\tCurrent Frame: {diagnostics.CurrentFrame}\n";
        t += $"\tBuffered Frames: {diagnostics.BufferedFrames}\n";
        t += $"\tAvgInPacketSize: {RoundTo10(diagnostics.AvgInPacketSize)}\n";
        t += $"\tAvgOutPacketSize: {RoundTo10(diagnostics.AvgOutPacketSize)}\n";
      }

      text.text = t;
    }
    else
    {
      text.text = "";
    }
  }

  private float RoundTo10(float n)
  {
    return Mathf.Round(n / 10) * 10;
  }
}
