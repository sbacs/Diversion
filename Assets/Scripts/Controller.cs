using UnityEngine;
using UnityEngine.InputSystem;

public class FPSController : MonoBehaviour
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

    [Header("Interaction")]
    private GameObject heldObject;
    private Rigidbody heldObjectRb;
    public float holdDistance = 2f;
    public float holdSmoothSpeed = 10f;
    private Vector3 holdVelocity;

    private Vector2 lookInput;

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

        controls.Player.Throw.performed += ctx => handleThrow();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    private void OnDisable()
    {
        controls.Player.Disable();
    }

    private void FixedUpdate()
    {
        // Player movement
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0;
        right.y = 0;
        Vector3 movement = (forward * moveInput.y + right * moveInput.x) * speed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + movement);

        bool isGrounded = Physics.Raycast(groundCheck.position, Vector3.down, groundCheckDistance, groundLayer);
        if (jumpPressed && isGrounded)
            rb.AddForce(Vector3.up * jumpStrength, ForceMode.Impulse);
        jumpPressed = false;

        // Move held object smoothly with Lerp and calculate velocity
        if (heldObject != null && heldObjectRb != null)
        {
            Vector3 targetPosition = cameraTransform.position + cameraTransform.forward;
            // Calcola la velocità basata sul movimento
            holdVelocity = (targetPosition - heldObjectRb.position) / 5f / Time.fixedDeltaTime;
            // Lerp per smooth follow
            heldObjectRb.position = Vector3.Lerp(heldObjectRb.position, targetPosition, holdSmoothSpeed * Time.fixedDeltaTime);
        }
    }

    private void Interact()
    {
        RaycastHit hit;

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, 200f))
        {
            if (hit.collider.TryGetComponent<Iinteractable>(out var interactable))
            {
                interactable.interact();
            }
        }
    }

    private void LateUpdate()
    {
        // Camera rotation
        float mouseX = lookInput.x * lookSensitivity;
        float mouseY = lookInput.y * lookSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
        Highlight();
    }



    private void Grab()
    {
        if (heldObject == null)
        {
            RaycastHit hit;
            if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, 200f))
            {
                heldObject = hit.collider.gameObject;
                heldObjectRb = hit.collider.attachedRigidbody;

                if (heldObjectRb != null)
                {
                    heldObjectRb.useGravity = false;
                    heldObjectRb.linearVelocity = Vector3.zero; // reset velocity
                }
                Debug.Log("Picked up: " + heldObject.name);
            }
        }
        else
        {
            if (heldObjectRb != null)
            {
                heldObjectRb.useGravity = true;
                heldObjectRb.linearVelocity = holdVelocity; // mantieni momentum
            }
            Debug.Log("Dropped: " + heldObject.name);
            heldObject = null;
            heldObjectRb = null;
        }
    }

    public Material highlightMaterial;

    private GameObject highlightedOBJ;
    private Material[] highlightedOriginalMaterials;

    private void Highlight()
    {
        RaycastHit hit;
        bool hitSomething = Physics.Raycast(
            cameraTransform.position,
            cameraTransform.forward,
            out hit,
            3f
        );

        GameObject hitObject =
            hitSomething && hit.collider.CompareTag("Interactable")
            ? hit.collider.gameObject
            : null;

        // Se stiamo guardando lo stesso oggetto, non fare nulla
        if (hitObject == highlightedOBJ)
            return;


        // Ripristina il precedente
        if (highlightedOBJ != null)
        {
            MeshRenderer prevRenderer = highlightedOBJ.GetComponent<MeshRenderer>();
            if (prevRenderer != null)
                prevRenderer.sharedMaterials = highlightedOriginalMaterials;

            highlightedOBJ = null;
            highlightedOriginalMaterials = null;
        }

        // Evidenzia il nuovo
        if (hitObject != null)
        {
            MeshRenderer renderer = hitObject.GetComponent<MeshRenderer>();
            if (renderer == null)
                return;

            highlightedOBJ = hitObject;


            // Salva materiali originali
            highlightedOriginalMaterials = renderer.sharedMaterials;

            Material[] newMats = new Material[highlightedOriginalMaterials.Length + 1];
            for (int i = 0; i < highlightedOriginalMaterials.Length; i++)
                newMats[i] = highlightedOriginalMaterials[i];

            newMats[newMats.Length - 1] = highlightMaterial;
            renderer.sharedMaterials = newMats;
        }
    }


    private void handleThrow()
    {
        if (heldObject)
        {
            heldObjectRb.useGravity = true;
            heldObjectRb.AddForce(cameraTransform.forward * 15f, ForceMode.Impulse);

            heldObject = null;
            heldObjectRb = null;

        }
    }


}
