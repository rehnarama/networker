using System.Collections;
using System.Collections.Generic;
using Network;
using Network.Physics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NetworkedBody))]
public class PlayerController : MonoBehaviour
{
  private Rigidbody rb;
  private NetworkedBody nb;

  public int PlayerId
  {
    get
    {
      return nb.playerAuthority;
    }
  }


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

  private bool previousSpaceDown = false;
  private Collider latestWallJumpCollider = null;

  public bool IsControlling
  {
    get
    {
      return
        Mathf.Abs(Network.NetworkState.Input.For(nb.playerAuthority).GetAnalog("Horizontal")) > Mathf.Epsilon ||
        Mathf.Abs(Network.NetworkState.Input.For(nb.playerAuthority).GetAnalog("Vertical")) > Mathf.Epsilon;
    }
  }

  void Start()
  {

    rb = GetComponent<Rigidbody>();
    nb = GetComponent<NetworkedBody>();

    if (NetworkState.IsClient)
    {
      if (NetworkState.Client.PlayerId == nb.playerAuthority)
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
      if (NetworkState.Client.PlayerId == nb.playerAuthority)
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


    HandleWallRunning();
    handleLooking();
  }

  void FixedUpdate()
  {
    handleWalking();

    HandleJumping();
  }


  private void HandleWallRunning()
  {
    Quaternion targetQuat = Quaternion.identity;
    if (IsWallRunning(out var hit))
    {
      if (rb.velocity.y < 0)
      {
        rb.AddForce(Physics.gravity * rb.mass * -0.5f); // Decrease gravity by 50%
      }

      targetQuat = Quaternion.FromToRotation(Vector3.up, (hit.normal + Vector3.up).normalized);

      rb.AddForce(-hit.normal * 0.2f);
    }

    body.rotation = Quaternion.Lerp(body.rotation, targetQuat, 0.1f);
  }


  private void handleLooking()
  {
    var mouseX = Network.NetworkState.Input.For(nb.playerAuthority).GetAnalog("Mouse X");
    var mouseY = Network.NetworkState.Input.For(nb.playerAuthority).GetAnalog("Mouse Y");

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
    var sidewards = Vector3.right * Network.NetworkState.Input.For(nb.playerAuthority).GetAnalog("Horizontal");
    var forwards = Vector3.forward * Network.NetworkState.Input.For(nb.playerAuthority).GetAnalog("Vertical");
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

    // If not moving in air, try to be still in xz-direction
    if ((sidewards + forwards).sqrMagnitude < Mathf.Epsilon && IsGrounded(out var _))
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
    if (spaceDown && (IsGrounded(out var hit)))
    {
      rb.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
      if (hit.transform.TryGetComponent<Rigidbody>(out var groundRb))
      {
        // This will handle jumping from moving platform
        rb.AddForce(groundRb.velocity, ForceMode.VelocityChange);
      }
    }
    else if (!previousSpaceDown && spaceDown && IsWallRunning(out var wallHit) && latestWallJumpCollider != wallHit.collider)
    {
      rb.AddForce((wallHit.normal * 2 + Vector3.up).normalized * jumpPower, ForceMode.VelocityChange);
      if (wallHit.transform.TryGetComponent<Rigidbody>(out var groundRb))
      {
        // This will handle jumping from moving platform
        rb.AddForce(groundRb.velocity, ForceMode.VelocityChange);
      }
      latestWallJumpCollider = wallHit.collider;
    }
    else if (IsGrounded(out hit))
    {
      latestWallJumpCollider = null;
    }

    previousSpaceDown = spaceDown;
  }

  private bool IsGrounded(out RaycastHit hit)
  {
    return Physics.Raycast(transform.position + Vector3.up * 0.05f, Vector3.down, out hit, 0.1f);
  }

  private bool IsWallRunning(out RaycastHit hit)
  {
    hit = new RaycastHit();
    return
      !IsGrounded(out var groundHit) &&
     (Physics.Raycast(transform.position, head.transform.localRotation * Vector3.left, out hit, 0.6f) &&
      hit.collider != latestWallJumpCollider ||
      Physics.Raycast(transform.position, head.transform.localRotation * Vector3.right, out hit, 0.6f) &&
      hit.collider != latestWallJumpCollider);

  }

  private void HandleKick()
  {
    var leftMouseDown = Network.NetworkState.Input.For(nb.playerAuthority).GetDigital((int)KeyCode.Mouse0);

    // Physics.BoxCast
  }
}
