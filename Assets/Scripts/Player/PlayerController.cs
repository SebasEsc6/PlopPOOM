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

    [SerializeField] private WeaponHandler weaponHandler;

    [SerializeField] private WeaponSpawner weaponSpawner; //! DELETE THIS REFERENCES IS ONLY FOR TESTING


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
        weaponHandler.LoadWeapon(0);
        shootController.SetCurrentWeapon();
        weaponSpawner.SpawnSingleWeapon();
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

        if (dir != 0)
        {
            float direction = Mathf.Sign(dir);
            transform.localScale = new Vector3(direction * 0.7f, transform.localScale.y, 1);

            if (weaponHandler != null)
                weaponHandler.SetDirection(direction);
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

        if (col.CompareTag("Weapon"))
        {
            HandleWeaponPickup(col);
        }
    }
    
    void HandleWeaponPickup(Collider2D col)
    {
        if (!col.TryGetComponent(out WeaponIndentifier weaponIdComponent)) return;

        int weaponId = weaponIdComponent.sO_Weapons.weaponId;
        weaponHandler.LoadWeapon(weaponId);
        shootController.SetCurrentWeapon();
    }
}
