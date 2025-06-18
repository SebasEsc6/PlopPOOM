using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerInput))]
public class ClientAuthoritativeMovement : NetworkBehaviour
{
    [SerializeField] PlayerController playerController;
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

    bool canDoubleJump;
    float currentSpeed;

    public readonly NetworkVariable<float> _moveDir = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public float MoveDir => _moveDir.Value;

    #region LIFE CYCLE


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentSpeed = speedMovement;
        // _ShootController = GetComponent<NetworkShootController>();
    }

    #region Physics

    void FixedUpdate()
    {
        if (!playerController.CanExecuteClientLogic()) return;

        rb.linearVelocity = new Vector2(MoveDir * currentSpeed, rb.linearVelocity.y);

        if (MoveDir != 0)
            transform.localScale = new Vector3(Mathf.Sign(MoveDir) * .7f,
                                               transform.localScale.y, 1);

        anim.SetFloat("Speed", Mathf.Abs(MoveDir));
    }
    #endregion


    #region JUMP LOGIC

    public void PerformJump()
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
        jumpForce = Mathf.Max(.1f, jumpForce);
        groundRadius = Mathf.Clamp(groundRadius, .05f, .5f);
    }
#endif
    #endregion
    #endregion
}
