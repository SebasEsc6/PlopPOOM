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
    bool canDoubleJump;
    float currentSpeed;
    InputAction moveAction, jumpAction, shootAction;             
    NetworkShootController _ShootController;

    readonly NetworkVariable<float> _moveDir = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);       

    float MoveDir => _moveDir.Value;

    #region LIFE CYCLE

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        _ShootController = GetComponent<NetworkShootController>();
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

    void RegisterInputCallbacks()
    {
        moveAction = playerInput.actions["Movement"];
        jumpAction = playerInput.actions["Jump"];
        shootAction = playerInput.actions["Shoot"];

        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;
        shootAction.performed += OnShoot;
        shootAction.canceled += OnShoot;;
        jumpAction.performed += OnJumpPerformed;
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
    #endregion

    #region INPUT CALLBACKS

    void OnMove(InputAction.CallbackContext ctx)
    {
        if (!HasAuthority) return;
        float dir = Mathf.Clamp(ctx.ReadValue<Vector2>().x, -1f, 1f);
        _moveDir.Value = dir;                             
    }

    void OnShoot(InputAction.CallbackContext ctx)
    {
        if (!HasAuthority) return;

        if (ctx.phase == InputActionPhase.Performed)  
            _ShootController.BeginCharge();
        else if (ctx.phase == InputActionPhase.Canceled) 
            _ShootController.ReleaseCharge();
    }

    void OnJumpPerformed(InputAction.CallbackContext _)
    {
        if (!HasAuthority) return;
        PerformJump();                               
    }
    #endregion

    #region JUMP LOGIC

    void PerformJump()
    {
        bool grounded = Physics2D.OverlapCircle(groundCheck.position,
                                                groundRadius, groundLayer);
        if (!grounded && !canDoubleJump) return;

        if (grounded)
            canDoubleJump = true;        
        else
            canDoubleJump = false;       

        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        anim.SetTrigger("Jump");
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
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
