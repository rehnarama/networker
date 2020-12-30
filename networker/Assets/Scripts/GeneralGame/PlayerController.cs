using System.Collections;
using System.Collections.Generic;
using Events;
using Network;
using Network.Events;
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
  public GameObject bomb;

  public float walkSpeed = 30f;
  public float lookSpeed = 250f;
  public float maxSpeed = 6f;
  public float breakingFactor = 0.3f;

  [Tooltip("From 0-Infinity. 0 is no smoothing, Infinity is barely movable.")]
  public float lookSmoothing = 0.5f;
  public float jumpPower = 8f;
  public float jetpackPower = 50f;
  private float jetpackFuelLeft;
  public float maxJetpackDuration = 2f;
  public float kickPower = 5f;

  private Quaternion wallRunningRotation = Quaternion.identity;
  private Quaternion groundRunningRotation = Quaternion.identity;


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
    jetpackFuelLeft = maxJetpackDuration;

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

    var headNb = head.GetComponent<NetworkedBody>();
    headNb.playerAuthority = nb.playerAuthority;
    NetworkState.RegisterBody(headNb.id, headNb); // We have to re-register it to update player authority
  }

  private void OnEnable()
  {
    if (NetworkState.IsServer)
    {
      NetworkState.Server.OnClientEvent += OnClientEvent;
    }
  }
  private void OnDisable()
  {
    if (NetworkState.IsServer)
    {
      NetworkState.Server.OnClientEvent += OnClientEvent;
    }
  }
  private void OnClientEvent(IEvent e, int playerId)
  {
    IGameEvent ge = (IGameEvent)e;
    if (ge.Type == GameEvents.Trigger)
    {
      if (nb.playerAuthority == playerId)
      {
        var te = (TriggerEvent)ge;
        if (te.trigger == TriggerEvent.Trigger.Kick)
        {
          HandleKick();
        }

        if (te.trigger == TriggerEvent.Trigger.Bomb)
        {
          HandleBomb();
        }
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
    HandleLooking();
    HandleBody();
    HandleTriggerBomb();
    HandleTriggerKick();
  }

  void FixedUpdate()
  {
    HandleWalking();
    HandleJetPack();
  }


  private void HandleWallRunning()
  {

    Quaternion angleRotation = Quaternion.identity;
    if (IsWallRunning(out var hit))
    {
      if (rb.velocity.y < 0)
      {
        rb.AddForce(Physics.gravity * rb.mass * -0.5f); // Decrease gravity by 50%
      }

      angleRotation = Quaternion.FromToRotation(Vector3.up, (hit.normal * 2 + Vector3.up).normalized);

      rb.AddForce(-hit.normal * 0.2f);
    }

    wallRunningRotation = Quaternion.Slerp(wallRunningRotation, angleRotation, 0.1f);
  }


  private void HandleLooking()
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

  private void HandleBody()
  {
    var sidewards = Vector3.right * Network.NetworkState.Input.For(nb.playerAuthority).GetAnalog("Horizontal");
    var forwards = Vector3.forward * Network.NetworkState.Input.For(nb.playerAuthority).GetAnalog("Vertical");

    var rotation = Quaternion.Euler(0, head.transform.rotation.eulerAngles.y, 0);
    var force = rotation * (sidewards + forwards) / 2f + GetBreakingForce() / 20f;
    var forceRotation = Quaternion.FromToRotation(Vector3.up, force + Vector3.up);
    groundRunningRotation = Quaternion.Slerp(groundRunningRotation, forceRotation, 0.05f);

    var eulerRotation = head.transform.localRotation.eulerAngles;
    body.rotation = groundRunningRotation * wallRunningRotation * Quaternion.Euler(0, eulerRotation.y, 0);
  }

  private void HandleWalking()
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

    rb.AddForce(GetBreakingForce());
  }

  private Vector3 GetBreakingForce()
  {
    var sidewards = Vector3.right * Network.NetworkState.Input.For(nb.playerAuthority).GetAnalog("Horizontal");
    var forwards = Vector3.forward * Network.NetworkState.Input.For(nb.playerAuthority).GetAnalog("Vertical");

    // If not moving in air, try to be still in xz-direction
    if ((sidewards + forwards).sqrMagnitude < Mathf.Epsilon && IsGrounded(out var _))
    {
      return -new Vector3(
        rb.velocity.x,
        0f,
        rb.velocity.z
      ) * breakingFactor;
    }
    else
    {
      return Vector3.zero;
    }
  }

  private void HandleJetPack()
  {
    var spaceDown = Network.NetworkState.Input.For(nb.playerAuthority).GetDigital((int)KeyCode.Space);
    if (spaceDown && IsGrounded(out var hit))
    {
      rb.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
      if (hit.transform.TryGetComponent<Rigidbody>(out var groundRb))
      {
        // This will handle jetpacking from moving platform to get it's velocity
        rb.AddForce(groundRb.velocity, ForceMode.VelocityChange);
      }
    }
    else if (spaceDown && jetpackFuelLeft > 0f)
    {
      rb.AddForce(Vector3.up * jetpackPower);
      jetpackFuelLeft -= Time.deltaTime;
    }

    if (IsGrounded(out var _))
    {
      // Refill fuel
      jetpackFuelLeft = Mathf.Min(maxJetpackDuration, jetpackFuelLeft + Time.deltaTime);
    }

  }

  private bool IsGrounded(out RaycastHit hit)
  {
    return Physics.Raycast(transform.position + Vector3.up * 0.05f, Vector3.down, out hit, 0.1f);
  }

  private bool IsWallRunning(out RaycastHit hit)
  {
    hit = new RaycastHit();
    var rotation = Quaternion.Euler(0, head.transform.rotation.eulerAngles.y, 0);
    return
      !IsGrounded(out var groundHit) &&
     (Physics.Raycast(transform.position, rotation * Vector3.left, out hit, 1f) &&
      hit.collider.tag == "Terrain" ||
      Physics.Raycast(transform.position, rotation * Vector3.right, out hit, 1f) &&
      hit.collider.tag == "Terrain");

  }

  private void HandleTriggerKick()
  {
    if (nb.playerAuthority == NetworkState.Client.PlayerId)
    {
      if (Input.GetKeyDown(KeyCode.Mouse0))
      {
        NetworkState.Client.InvokeClientEvent(new TriggerEvent(TriggerEvent.Trigger.Kick));
      }
    }
  }

  private void HandleKick()
  {
    if (NetworkState.IsServer)
    {
      var rotation = Quaternion.Euler(0, head.transform.rotation.eulerAngles.y, 0);
      var direction = rotation * Vector3.forward;
      var rays = new Ray[3] {
        new Ray(transform.position, direction),
        new Ray(transform.position + Vector3.up, direction),
        new Ray(transform.position + Vector3.up * 2, direction),
      };

      HashSet<NetworkedBody> bodiesFound = new HashSet<NetworkedBody>();
      foreach (var ray in rays)
      {
        if (Physics.Raycast(ray, out var hit, 2f))
        {
          if (hit.collider.TryGetComponent<NetworkedBody>(out var hitBody))
          {
            bodiesFound.Add(hitBody);
          }
        }
      }

      foreach (var hitBody in bodiesFound)
      {
        var force = (direction + Vector3.up).normalized * kickPower;
        NetworkState.Server.InvokeEvent(new KickEvent(hitBody.id, force));
      }
    }
  }
  private void HandleTriggerBomb()
  {
    if (nb.playerAuthority == NetworkState.Client?.PlayerId)
    {
      if (Input.GetKeyDown(KeyCode.Mouse1))
      {
        NetworkState.Client.InvokeClientEvent(new TriggerEvent(TriggerEvent.Trigger.Bomb));
      }
    }
  }

  private void HandleBomb()
  {
    if (NetworkState.IsServer)
    {
      var rotation = Quaternion.Euler(0, head.transform.rotation.eulerAngles.y, 0);
      var spawnpoint =
        new Vector3(transform.position.x, transform.position.y + 2, transform.position.z) +
        head.transform.rotation * Vector3.forward * 2;
      NetworkState.Server.InvokeEvent(new InstantiateEvent(spawnpoint, rotation, InstantiateEvent.InstantiateTypes.Bomb, -1));
    }
  }
}
