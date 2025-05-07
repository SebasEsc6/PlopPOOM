using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerInput))]
public class ClientAuthoritativeMovement : NetworkBehaviour
{
    [Header("Stats")]
    [SerializeField] float speedMovement = 5f;
    [SerializeField] float jumpForce = 5f;

    [Header("Ground Check")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundRadius = .15f;

    [Header("FX")]
    [SerializeField] ParticleSystem jumpFX;

    Rigidbody2D rb;
    Animator anim;
    PlayerInput playerInput;
    bool canJump, canDoubleJump;
    float currentSpeed;

    InputAction moveAction, jumpAction;             

    readonly NetworkVariable<float> _moveDir = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);       

    public float MoveDir => _moveDir.Value;

    #region LIFE CYCLE

    void Awake()
    {
        rb   = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        currentSpeed = speedMovement;
        ApplyAuthorityState();  
    }

    protected override void OnOwnershipChanged(ulong prevOwner, ulong newOwner)
    {
        base.OnOwnershipChanged(prevOwner, newOwner);
        ApplyAuthorityState();          
    }

    void OnDisable()
    {
        if (!HasAuthority || moveAction == null) return;

        moveAction.performed -= OnMove;
        moveAction.canceled  -= OnMove;
        jumpAction.performed -= OnJumpPerformed;
    }
    
    #endregion

    #region Physics

    void FixedUpdate()
    {
        if (!HasAuthority || !IsSpawned) return;

        rb.linearVelocity = new Vector2(MoveDir * currentSpeed, rb.linearVelocity.y);

        if (MoveDir != 0)
            transform.localScale = new Vector3(Mathf.Sign(MoveDir) * .7f,
                                               transform.localScale.y, 1);

        anim.SetFloat("Speed", Mathf.Abs(MoveDir));

        CheckGrounded();
    }

    void ApplyAuthorityState()
{
    if (HasAuthority)
    {
        if (!playerInput.enabled)
            playerInput.enabled = true;

        if (moveAction == null)                     
            RegisterInputCallbacks();
    }
    else
    {
        playerInput.enabled = false;
    }
}

    void RegisterInputCallbacks()
    {
        moveAction = playerInput.actions["Movement"];
        jumpAction = playerInput.actions["Jump"];

        moveAction.performed += OnMove;
        moveAction.canceled  += OnMove;
        jumpAction.performed += OnJumpPerformed;
    }
    #endregion

    #region INPUT CALLBACKS

    void OnMove(InputAction.CallbackContext ctx)
    {
        if (!HasAuthority) return;
        float dir = Mathf.Clamp(ctx.ReadValue<Vector2>().x, -1f, 1f);
        _moveDir.Value = dir;                             
    }

    void OnJumpPerformed(InputAction.CallbackContext _)
    {
        if (!HasAuthority) return;
        JumpServerRpc();                                   
    }
    #endregion

    #region JUMP LOGIC

    [ServerRpc(RequireOwnership = true)]
    void JumpServerRpc() => JumpClientRpc();   

    [ClientRpc]
    void JumpClientRpc()
    {
        if (!canJump && !canDoubleJump) return;

        if (canJump)       canDoubleJump = true;
        else               canDoubleJump = false;

        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        anim.SetTrigger("Jump");
        if (!canJump) jumpFX?.Play();

        canJump = false;
    }

    void CheckGrounded()
    {
        canJump = Physics2D.OverlapCircle(groundCheck.position,
                                          groundRadius, groundLayer);
    }
    #endregion

    #region VALIDATE

#if UNITY_EDITOR
    void OnValidate()
    {
        speedMovement = Mathf.Max(.1f, speedMovement);
        jumpForce     = Mathf.Max(.1f, jumpForce);
        groundRadius  = Mathf.Clamp(groundRadius, .05f, .5f);
    }
#endif
    #endregion
}
