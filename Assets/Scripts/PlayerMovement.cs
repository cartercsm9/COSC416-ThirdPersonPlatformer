using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;
    [SerializeField] private float speed = 10f;
    [SerializeField] private float groundDeceleration = 10f; // Deceleration factor when no input is detected on ground
    [SerializeField] private float airAcceleration = 2f; // Lower value means slower acceleration in air
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float dashForce = 15f;
    [SerializeField] private float dashCooldown = 3f;
    [SerializeField] private Transform cameraTransform; // Cinemachine free-look camera reference

    private Rigidbody rb;
    private int jumpCount = 0;
    private int maxJumps = 2; // Allow double jump
    private bool canDash = true;
    private bool isGrounded = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Freeze X and Z rotations to keep the capsule upright.
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        
        inputManager.OnMove.AddListener(MovePlayer);
        inputManager.OnSpacePressed.AddListener(JumpPlayer);
        inputManager.OnDash.AddListener(DashPlayer);
    }

    // Moves the player using direct velocity assignment for immediate response on ground,
    // and slower, gradual acceleration in air.
    private void MovePlayer(Vector2 input)
    {
        Vector3 moveDirection;
        if (cameraTransform != null)
        {
            // Determine movement direction relative to the free-look camera.
            Vector3 camForward = cameraTransform.forward;
            camForward.y = 0f;
            camForward.Normalize();
            Vector3 camRight = cameraTransform.right;
            camRight.y = 0f;
            camRight.Normalize();
            moveDirection = (camRight * input.x + camForward * input.y).normalized;
        }
        else
        {
            moveDirection = new Vector3(input.x, 0f, input.y).normalized;
        }

        if (isGrounded)
        {
            if (input.sqrMagnitude < 0.01f)
            {
                // No input: gradually decelerate horizontal movement on ground.
                Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
                horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, groundDeceleration * Time.deltaTime);
                rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
            }
            else
            {
                // Immediate, responsive movement on ground.
                rb.linearVelocity = new Vector3(moveDirection.x * speed, rb.linearVelocity.y, moveDirection.z * speed);
                
                // Rotate the player to face the movement direction.
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }
        else // In air
        {
            if (input.sqrMagnitude >= 0.01f)
            {
                // Gradually accelerate in air: use Lerp so that input modifies horizontal velocity slower.
                Vector3 targetVelocity = new Vector3(moveDirection.x * speed, rb.linearVelocity.y, moveDirection.z * speed);
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, airAcceleration * Time.deltaTime);
                
                // Rotate the player to face the movement direction.
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
            // If no input in air, let momentum carry the player.
        }
    }

    // Handles jumping with support for a double jump.
    private void JumpPlayer()
    {
        if (jumpCount < maxJumps)
        {
            // Reset vertical velocity for a consistent jump height.
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpCount++;
        }
    }

    // Performs a dash in the player's forward direction, respecting a cooldown.
    private void DashPlayer()
    {
        if (!canDash) return;
        rb.AddForce(transform.forward * dashForce, ForceMode.Impulse);
        canDash = false;
        StartCoroutine(DashCooldown());
    }

    private IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // Update grounded state and reset jump count when colliding with objects tagged "Ground".
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            jumpCount = 0;
        }
    }

    // Update grounded state when leaving collision with objects tagged "Ground".
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
