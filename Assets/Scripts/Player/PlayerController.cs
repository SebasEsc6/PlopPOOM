using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    PlayerInput playerInput;
    InputAction moveAction, jumpAction, shootAction;

    [SerializeField] private ClientAuthoritativeMovement authoritativeMovement;
    [SerializeField] private NetworkStatsController statsController;
    [SerializeField] private NetworkShootController shootController;
    #region LIFE CYCLE

    public bool CanExecuteClientLogic() => IsSpawned && HasAuthority;

    void Awake()
    {
        // anim = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();
        if (moveAction == null)
            RegisterInputCallbacks();

    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        ApplyAuthorityState();
    }

    protected override void OnOwnershipChanged(ulong prevOwner, ulong newOwner)
    {
        base.OnOwnershipChanged(prevOwner, newOwner);
        ApplyAuthorityState();
    }

    public void RegisterInputCallbacks()
    {
        moveAction = playerInput.actions["Movement"];
        jumpAction = playerInput.actions["Jump"];
        shootAction = playerInput.actions["Shoot"];

        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;
        shootAction.performed += OnShoot;
        shootAction.canceled += OnShoot; ;
        jumpAction.performed += OnJumpPerformed;
    }

    void OnEnable()
    {
        if (CanExecuteClientLogic())
            RegisterInputCallbacks();
    }

    void OnDisable()
    {
        if (!CanExecuteClientLogic() || moveAction == null) return;
        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMove;
        jumpAction.performed -= OnJumpPerformed;
        shootAction.performed -= OnShoot;
        shootAction.canceled -= OnShoot;
    }

    public void ApplyAuthorityState()
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
        if (!CanExecuteClientLogic()) return;

        float dir = Mathf.Clamp(ctx.ReadValue<Vector2>().x, -1f, 1f);

        if (!Mathf.Approximately(dir, authoritativeMovement._moveDir.Value))
        {
            authoritativeMovement._moveDir.Value = dir;
        }
    }

    void OnShoot(InputAction.CallbackContext ctx)
    {
        if (!CanExecuteClientLogic()) return;

        if (ctx.phase == InputActionPhase.Performed)
        {
            shootController.BeginCharge();
            Debug.Log("shoooooot");
        }
        else if (ctx.phase == InputActionPhase.Canceled)
            shootController.ReleaseCharge();
    }

    void OnJumpPerformed(InputAction.CallbackContext _)
    {
        if (!CanExecuteClientLogic()) return;
        authoritativeMovement.PerformJump();
    }
    #endregion

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!IsOwner || !IsSpawned) return;

        if (col.TryGetComponent(out NetworkBulletController bullet))
        {
            var data = new DamageData
            {
                amount = bullet.GetDamage(),
                attackerId = bullet.OwnerClientId,
                bulletId = bullet.BulletId,
                validationToken = bullet.Token,
                hitPoint = transform.position,
                timeSent = NetworkManager.ServerTime.Time
            };

            statsController.TakeDamage(data);
        }
    }
}
