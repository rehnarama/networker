using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class WorldSpaceCanvasController : MonoBehaviour
{
  private Canvas canvas;
  void Start()
  {
    canvas = GetComponent<Canvas>();
  }

  // Update is called once per frame
  void Update()
  {

  }
}
