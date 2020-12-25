﻿using System.Collections;
using System.Collections.Generic;
using Network;
using Network.Physics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
  private Rigidbody rb;
  private NetworkedBody nb;


  public new Camera camera;
  public Transform body;
  public Transform head;


  public float walkSpeed = 30f;
  public float lookSpeed = 250f;
  public float maxSpeed = 6f;
  public float breakingFactor = 0.3f;

  [Tooltip("From 0-Infinity. 0 is no smoothing, Infinity is barely movable.")]
  public float lookSmoothing = 0.5f;
  public float jumpPower = 2f;

  void Start()
  {

    rb = GetComponent<Rigidbody>();
    nb = GetComponent<NetworkedBody>();

    if (NetworkState.IsClient)
    {
      if (PhysicsClient.Instance.PlayerId == nb.playerAuthority)
      {
        Camera.main.gameObject.SetActive(false);
        camera.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
      }
    }

  }


  private void Update()
  {
    if (NetworkState.IsClient)
    {
      if (PhysicsClient.Instance.PlayerId == nb.playerAuthority)
      {
        if (Cursor.lockState == CursorLockMode.Locked && Input.GetKeyDown(KeyCode.Escape))
        {
          Cursor.lockState = CursorLockMode.None;
        }
        else if (Cursor.lockState == CursorLockMode.None && Input.GetMouseButtonDown(0))
        {
          Cursor.lockState = CursorLockMode.Locked;
        }
      }
    }

  }

  // Update is called once per frame
  void FixedUpdate()
  {
    handleLooking();

    handleWalking();

    HandleJumping();

    HandleBody();
  }

  private void HandleBody()
  {
    body.rotation = Quaternion.identity;
  }

  private void handleLooking()
  {
    var mouseX = Network.NetworkState.Input.For(nb.playerAuthority).GetAnalog(3);
    var mouseY = Network.NetworkState.Input.For(nb.playerAuthority).GetAnalog(4);

    float rotationHoriz = mouseX * lookSpeed * Time.deltaTime;

    float rotationVert = mouseY * lookSpeed * Time.deltaTime;

    var horizontalQuaternion = Quaternion.AngleAxis(rotationHoriz, Vector3.up);
    var verticalQuaternion = Quaternion.AngleAxis(rotationVert, Vector3.left);

    var targetQuaternion = head.transform.localRotation * horizontalQuaternion * verticalQuaternion;
    head.transform.localRotation = Quaternion.Lerp(head.transform.localRotation, targetQuaternion, 1f / (1f + lookSmoothing));
    var eulerRotation = head.transform.localRotation.eulerAngles;
    head.transform.localRotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, 0); // We only want rotation around these two, let's remove Z-axis rotation!
  }

  private void handleWalking()
  {
    var sidewards = Vector3.right * Network.NetworkState.Input.For(nb.playerAuthority).GetAnalog(1);
    var forwards = Vector3.forward * Network.NetworkState.Input.For(nb.playerAuthority).GetAnalog(2);
    var force = (sidewards + forwards) * walkSpeed;

    var rotation = Quaternion.Euler(0, head.transform.rotation.eulerAngles.y, 0);
    var directedForce = rotation * force;
    rb.AddForce(directedForce, ForceMode.Acceleration);

    // Try to limit xz movement if more than max speed
    var xzVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
    var isTooFast = xzVelocity.sqrMagnitude > maxSpeed * maxSpeed * 0.7; // *0.9 so we can keep constant max speed instead of rubber banding at top speed
    if (isTooFast)
    {
      var idealDrag = walkSpeed / maxSpeed;
      idealDrag = idealDrag / (idealDrag * Time.fixedDeltaTime + 1);
      var dragForce = -xzVelocity * idealDrag;
      rb.AddForce(dragForce, ForceMode.Acceleration);
    }

    // If not moving, try to be still in xz-direction
    if ((sidewards + forwards).sqrMagnitude < Mathf.Epsilon && IsGrounded())
    {
      rb.velocity -= new Vector3(
        rb.velocity.x,
        0f,
        rb.velocity.z
      ) * breakingFactor;
    }

  }

  private void HandleJumping()
  {
    var spaceDown = Network.NetworkState.Input.For(nb.playerAuthority).GetDigital((int)KeyCode.Space);
    if (spaceDown && IsGrounded())
    {
      rb.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
    }
  }

  private bool IsGrounded()
  {
    float distanceToFloor = 1f;
    return Physics.Raycast(transform.position, -Vector3.up, distanceToFloor + 0.1f); // Just small diff
  }

  private void HandleKick()
  {
    var leftMouseDown = Network.NetworkState.Input.For(nb.playerAuthority).GetDigital(0);

    // Physics.BoxCast
  }
}