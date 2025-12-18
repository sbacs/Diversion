using UnityEngine;
using UnityEngine.InputSystem;

public class Controller : MonoBehaviour
{
    private PlayerControls controls;
    public Rigidbody rb;

    [Header("Movement")]
    private Vector2 moveInput;
    private bool jumpPressed;
    public float speed = 5f;
    public float jumpStrength = 10f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.1f;

    [Header("Camera")]
    public Transform cameraTransform;
    public float lookSensitivity = 2f;
    private float xRotation = 0f;
    private float yRotation = 0f;
    private Vector2 lookInput;

    [Header("Interaction")]
    private GameObject heldObject;
    private Rigidbody heldObjectRb;
    public float holdDistance = 2f;
    public float holdSmoothSpeed = 1f;
    private Vector3 holdVelocity;

    [Header("Highlight")]
    public Material highlightMaterial;
    private GameObject highlightedOBJ;
    private Material[] highlightedOriginalMaterials;

    private void Awake()
    {
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Player.Enable();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Jump.performed += ctx => jumpPressed = true;

        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        controls.Player.Grab.performed += ctx => Grab();
        controls.Player.Interact.performed += ctx => Interact();
        controls.Player.Throw.performed += ctx => Throw();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        controls.Player.Disable();
    }

    private void FixedUpdate()
    {
        // Movement (based on camera forward/right projected on XZ)
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;
        forward.y = 0f;
        right.y = 0f;

        Vector3 movement =
            (forward.normalized * moveInput.y + right.normalized * moveInput.x).normalized
            * speed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + movement);

        // Yaw rotation on Rigidbody (prevents collision torque spin)
        yRotation += lookInput.x * lookSensitivity;
        rb.MoveRotation(Quaternion.Euler(0f, yRotation, 0f));

        // Jump
        bool isGrounded = Physics.Raycast(
            groundCheck.position,
            Vector3.down,
            groundCheckDistance,
            groundLayer
        );

        if (jumpPressed && isGrounded)
            rb.AddForce(Vector3.up * jumpStrength, ForceMode.Impulse);

        jumpPressed = false;

        // Held object follow
        if (heldObjectRb != null)
        {
            Vector3 targetPosition = cameraTransform.position + cameraTransform.forward * (holdDistance * 1.5f);

            holdVelocity = (targetPosition - heldObjectRb.position) / Time.fixedDeltaTime;

            heldObjectRb.position = Vector3.Lerp(
                heldObjectRb.position,
                targetPosition,
                holdSmoothSpeed * Time.fixedDeltaTime
            );
        }
    }

    private void LateUpdate()
    {
        // Pitch rotation on camera only
        float mouseY = lookInput.y * lookSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        Highlight();
    }

    private void Interact()
    {
        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, 200f))
        {
            if (hit.collider.TryGetComponent<Iinteractable>(out var interactable))
                interactable.interact();
        }
    }

    private void Grab()
    {
        if (heldObjectRb == null)
        {
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, 200f))
            {
                heldObject = hit.collider.gameObject;
                heldObjectRb = hit.collider.attachedRigidbody;

                if (heldObjectRb != null)
                {
                    heldObjectRb.useGravity = false;
                    heldObjectRb.linearVelocity = Vector3.zero;
                    heldObjectRb.angularVelocity = Vector3.zero;
                }
            }
        }
        else
        {
            heldObjectRb.useGravity = true;
            heldObjectRb.linearVelocity = Vector3.zero;

            heldObject = null;
            heldObjectRb = null;
        }
    }

    private void Throw()
    {
        if (heldObjectRb != null)
        {
            heldObjectRb.useGravity = true;
            heldObjectRb.AddForce(cameraTransform.forward * 15f, ForceMode.Impulse);

            heldObject = null;
            heldObjectRb = null;
        }
    }

    private void Highlight()
    {
        bool hitSomething = Physics.Raycast(
            cameraTransform.position,
            cameraTransform.forward,
            out RaycastHit hit,
            3f
        );

        GameObject hitObject =
            hitSomething && hit.collider.CompareTag("Interactable")
            ? hit.collider.gameObject
            : null;

        if (hitObject == highlightedOBJ)
            return;

        // Restore previous
        if (highlightedOBJ != null)
        {
            MeshRenderer prevRenderer = highlightedOBJ.GetComponent<MeshRenderer>();
            if (prevRenderer != null)
                prevRenderer.sharedMaterials = highlightedOriginalMaterials;

            highlightedOBJ = null;
            highlightedOriginalMaterials = null;
        }

        // Apply new
        if (hitObject != null)
        {
            MeshRenderer renderer = hitObject.GetComponent<MeshRenderer>();
            if (renderer == null) return;

            highlightedOBJ = hitObject;
            highlightedOriginalMaterials = renderer.sharedMaterials;

            Material[] newMats = new Material[highlightedOriginalMaterials.Length + 1];
            highlightedOriginalMaterials.CopyTo(newMats, 0);
            newMats[newMats.Length - 1] = highlightMaterial;

            renderer.sharedMaterials = newMats;
        }
    }
}
