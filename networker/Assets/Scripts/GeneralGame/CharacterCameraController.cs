﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCameraController : MonoBehaviour
{
  // Start is called before the first frame update
  public GameObject head;
  public float targetDistance;
  public Vector3 diff;

  private void Start()
  {
    UpdatePosition();
  }


  // Update is called once per frame
  void LateUpdate()
  {
    UpdatePosition();
    UpdateRotation();
  }

  private void UpdatePosition()
  {
    var targetRotation = head.transform.rotation;

    var targetPosition = (head.transform.position + diff) + targetRotation * (Vector3.back * targetDistance);

    var delta = targetPosition - head.transform.position;

    if (Physics.Raycast(head.transform.position, delta, out var hit, delta.magnitude / 0.95f, LayerMask.GetMask("Terrain")))
    {
      targetPosition = head.transform.position + targetRotation * (Vector3.back * hit.distance * 0.95f);
    }
    transform.position = targetPosition;
  }

  private void UpdateRotation()
  {
    var targetRotation = head.transform.rotation;
    transform.rotation = targetRotation;
  }
}
