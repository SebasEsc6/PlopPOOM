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

    public GameManager gameManager;


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
        // weaponHandler.LoadWeapon(0);
        shootController.SetCurrentWeapon();
        gameManager = GameManager.Instance;

        if (gameManager != null)
            gameManager.OnStateChanged += HandleGameStateChange;

        // if (gameManager.currentState is LobbyState)
        // {
        //     SetFlags(false);
        // }
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

        gameManager.OnStateChanged -= HandleGameStateChange;
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
            shootController.BeginCharge();
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
        if (!IsSpawned)
        {
            Debug.LogWarning($"[Player] Cannot apply nothing: IsOwner={IsOwner}, IsSpawned={IsSpawned}");
            return;
        }
    }

    public void HandleWeaponPickup(int id)
    {
        weaponHandler.LoadWeapon(id);
        shootController.SetCurrentWeapon();
    }

    private void HandleGameStateChange(IGameState newState)
    {
        switch (newState)
        {
            case LobbyState:
                Debug.Log("GameManager switch to lobby State");
                SetFlags(false);
                break;

            case WaitingState:
                Debug.Log("GameManager switch to wating State");
                SetFlags(false);

                break;

            case PlayingState:
                Debug.Log("GameManager switch to Play State");
                SetFlags(true);
                HandleWeaponPickup(0);
                break;

            case PauseState:
                Debug.Log("GameManager switch to Pause State");
                SetFlags(false);
                break;

            case EndedState:
                Debug.Log("GameManager switch to Ended State");
                break;

            default:
                Debug.Log("Dont indentified the current state");
                break;
        }
    }

    private void SetFlags(bool value)
    {
        authoritativeMovement.canMove = value;
        shootController.canShoot = value;
    }
}
