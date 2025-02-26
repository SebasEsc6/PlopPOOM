using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class MovementControllerMultiplayer : NetworkBehaviour
{
    private Rigidbody2D rb;
    private Animator _animator;

    [SerializeField] private float speedMovement;
    [SerializeField] private float jumpForce;
    [SerializeField] private float rayDistance;
    [SerializeField] private LayerMask groundLayer;

    public bool canJump;
    private bool canDoubleJump;
    private float currentSpeed;
    private float moveDirection;
    public void SetMoveDirection(float direction)
    {
        moveDirection = direction;
    }

    [SerializeField] private GameObject jumpParticles;

    // Network variables to sync position and movement state
    private NetworkVariable<Vector2> networkPosition = new NetworkVariable<Vector2>(
        default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private NetworkVariable<float> networkMoveDirection = new NetworkVariable<float>(
        default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private NetworkVariable<bool> networkJumpState = new NetworkVariable<bool>(
        default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private NetworkVariable<bool> networkDoubleJumpState = new NetworkVariable<bool>(
        default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            // Disable components that should not be active in non-owner clients
            if (rb) rb.bodyType = RigidbodyType2D.Kinematic; // Prevents non-owners from modifying physics
            if (_animator) _animator.enabled = false;
            enabled = false; // Disables this script on non-owner clients
        }
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            HandleMovement();
            ValidateJump();
            networkPosition.Value = transform.position; // Sync position
            networkMoveDirection.Value = moveDirection; // Sync movement state
        }
        else
        {
            ApplyNetworkSync(); // Sync movement for non-owners
        }
    }

    private void HandleMovement()
    {
        rb.linearVelocity = new Vector2(moveDirection * currentSpeed, rb.linearVelocity.y);

        if (moveDirection < 0)
            transform.localScale = new Vector3(-0.7f, transform.localScale.y, transform.localScale.z);
        else if (moveDirection > 0)
            transform.localScale = new Vector3(0.7f, transform.localScale.y, transform.localScale.z);

        _animator.SetFloat("Speed", Mathf.Abs(moveDirection));
    }

    private void ApplyNetworkSync()
    {
        // Lerp position for smooth transition
        transform.position = Vector2.Lerp(transform.position, networkPosition.Value, Time.deltaTime * 10f);

        // Update movement animation based on networked direction
        _animator.SetFloat("Speed", Mathf.Abs(networkMoveDirection.Value));

        // Flip character based on networked movement direction
        if (networkMoveDirection.Value < 0)
            transform.localScale = new Vector3(-0.7f, transform.localScale.y, transform.localScale.z);
        else if (networkMoveDirection.Value > 0)
            transform.localScale = new Vector3(0.7f, transform.localScale.y, transform.localScale.z);
    }

    public void SwitchVelocity(bool isSlow)
    {
        // Adjust speed when slowing down
        currentSpeed = isSlow ? speedMovement / 2 : speedMovement;
    }

    public void Jump()
    {
        if (!IsOwner) return;

        if (canJump)
        {
            ApplyJump(); // Apply jump locally
            JumpServerRpc();
        }
        else if (!canJump && canDoubleJump)
        {
            ApplyJump();
            canDoubleJump = false;
            JumpServerRpc();
            TriggerParticlesServerRpc();
        }
    }

    private void ApplyJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        _animator.SetTrigger("Jump");
        canJump = false;
    }

    [ServerRpc]
    private void JumpServerRpc()
    {
        networkJumpState.Value = true;
        networkDoubleJumpState.Value = false;
        UpdateJumpStateClientRpc();
    }

    [ClientRpc]
    private void UpdateJumpStateClientRpc()
    {
        if (!IsOwner && _animator != null)
        {
            _animator.SetTrigger("Jump");
        }
    }

    private void ValidateJump()
    {
        Vector2 rayOrigin = transform.position;
        Vector2 direction = Vector2.down;
        Debug.DrawRay(rayOrigin, direction * rayDistance, Color.red);

        RaycastHit2D hitInfo = Physics2D.Raycast(rayOrigin, direction, rayDistance, groundLayer);
        bool wasJumping = !canJump;
        canJump = hitInfo.collider != null;

        // Allow double jump reset when landing
        if (!wasJumping && canJump)
            canDoubleJump = true;

        if (IsServer)
        {
            networkJumpState.Value = canJump;
            networkDoubleJumpState.Value = canDoubleJump;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TriggerParticlesServerRpc()
    {
        TriggerParticlesClientRpc();
    }

    [ClientRpc]
    private void TriggerParticlesClientRpc()
    {
        StartCoroutine(TurnParticles());
    }

    private IEnumerator TurnParticles()
    {
        jumpParticles.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        jumpParticles.SetActive(false);
    }
}
