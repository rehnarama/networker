using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ProgressBarController))]
public class JetpackBarController : MonoBehaviour
{
  public PlayerController player;


  private ProgressBarController pbc;
  private void Start()
  {
    pbc = GetComponent<ProgressBarController>();
  }

  // Update is called once per frame
  void Update()
  {
    if (player != null)
    {
      pbc.progress = player.JetpackFuelLeft / player.maxJetpackDuration;
    }
  }
}
