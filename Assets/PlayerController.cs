using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float speed = 2f;               
    public float jumpForce = 1f;            
    public float continuousJumpForce = 10f;  
    public float downForce = 10f;            
    public float massMultiplier = 10f;       

    [Header("Death & Respawn")]
    // Y-position below which the player is considered to have fallen.
    public float deathYThreshold = -10f;

    [Header("Audio")]
    public AudioClip deathSound;          

    private AudioSource audioSource;
    private Rigidbody rb;
    private float baseMass;
    private Color baseColor;

    // Network variable for tracking death count.
    public NetworkVariable<int> deathCount = new NetworkVariable<int>(0);

    // Movement input stored locally.
    private Vector3 moveInput;
    private bool jumpPressed;
    private bool jumpHeld;
    private bool downPressed;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        baseMass = rb.mass;
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
            baseColor = rend.material.color;

        audioSource = GetComponent<AudioSource>();
        deathCount.OnValueChanged += OnDeathCountChanged;
    }

    void Update()
    {
        if (!IsOwner) return;
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Transform camTransform = Camera.main.transform;
        Vector3 camForward = camTransform.forward;
        camForward.y = 0;
        camForward.Normalize();
        Vector3 camRight = camTransform.right;
        camRight.y = 0;
        camRight.Normalize();
        moveInput = (camForward * vertical + camRight * horizontal).normalized;

        jumpPressed = Input.GetButtonDown("Jump");
        jumpHeld = Input.GetButton("Jump");
        downPressed = Input.GetKey(KeyCode.LeftControl);

        // Handle heavy weight change input on left mouse click.
        if (Input.GetMouseButtonDown(0))
        {
            ChangeHeavyWeightServerRpc(true);
        }
        if (Input.GetMouseButtonUp(0))
        {
            ChangeHeavyWeightServerRpc(false);
        }

        // Send movement input to the server.
        SendMovementInputServerRpc(moveInput, jumpPressed, jumpHeld, downPressed);
    }

    // Authoritative physics
    void FixedUpdate()
    {
        if (!IsServer) return;

        // Apply movement force.
        rb.AddForce(moveInput * speed, ForceMode.Force);

        if (jumpPressed && Mathf.Abs(rb.linearVelocity.y) < 0.01f)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        if (jumpHeld && rb.linearVelocity.y <= 0)
        {
            rb.AddForce(Vector3.up * continuousJumpForce, ForceMode.Force);
        }
        if (downPressed)
        {
            rb.AddForce(Vector3.down * downForce, ForceMode.Force);
        }

        // Check if the player has fallen
        if (transform.position.y < deathYThreshold)
        {
            // Increase the death count.
            deathCount.Value += 1;
            Respawn();
        }
    }

    // Respawn the player by resetting their position and velocity.
    private void Respawn()
    {
        Vector3 respawnPosition = new Vector3(0, 5, 0); // Adjust as needed.
        rb.linearVelocity = Vector3.zero;
        transform.position = respawnPosition;
    }

    // Plays the death sound locally if this is the owning client.
    private void OnDeathCountChanged(int previousValue, int newValue)
    {
        if (IsOwner && newValue > previousValue)
        {
            if (audioSource != null && deathSound != null)
            {
                audioSource.PlayOneShot(deathSound);
            }
        }
    }

    // ServerRpc to receive movement input from the client.
    [ServerRpc]
    private void SendMovementInputServerRpc(Vector3 moveDir, bool jumpP, bool jumpH, bool down)
    {
        moveInput = moveDir;
        jumpPressed = jumpP;
        jumpHeld = jumpH;
        downPressed = down;
    }

    // ServerRpc to update heavy weight changes.
    [ServerRpc]
    private void ChangeHeavyWeightServerRpc(bool heavy)
    {
        if (heavy)
        {
            rb.mass = baseMass * massMultiplier;
            UpdateColorClientRpc(Color.red);
        }
        else
        {
            rb.mass = baseMass;
            UpdateColorClientRpc(baseColor);
        }
    }

    // ClientRpc to update the player's color on all clients.
    [ClientRpc]
    private void UpdateColorClientRpc(Color newColor)
    {
        UpdateColor(newColor);
    }

    // render color, needs more
    private void UpdateColor(Color newColor)
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = newColor;
        }
    }
}
