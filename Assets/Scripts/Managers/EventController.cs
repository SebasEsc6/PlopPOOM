using UnityEngine;
using UnityEngine.InputSystem;

public class EventController : MonoBehaviour
{
    private PlayerInput _playerInput;
    private MovementController _movement;
    private ShootController _shoot;

    [Header("Control Flags")]
    public bool canControl = true;

    private InputAction _move;
    private InputAction _jump;
    private InputAction _shootAction;
    private bool _bound;

    private void Awake()
    {
        // Get components provided on this avatars instance
        _movement = GetComponent<MovementController>();
        _shoot = GetComponent<ShootController>();
    }

    private void OnEnable()
    {
        // Try to bind; if PlayerInput is not available yet (prewarm), just skip.
        TryBind();
    }

    private void OnDisable()
    {
        // Unbind safely if we were bound
        if (!_bound) return;

        if (_move != null)
        {
            _move.performed -= OnMove;
            _move.canceled -= OnMoveCanceled;
        }
        if (_jump != null)
        {
            _jump.performed -= OnJump;
        }
        if (_shootAction != null)
        {
            _shootAction.started -= OnShootStarted;
            _shootAction.canceled -= OnShootCanceled;
        }

        _bound = false;
    }

    public bool TryBind()
    {
        if (_bound) return true;

        // Find PlayerInput on the parent Root (created by PlayerInputManager)
        _playerInput = GetComponentInParent<PlayerInput>(includeInactive: true);
        if (_playerInput == null) return false; // not under Root yet (pool prewarm case)

        var map = _playerInput.currentActionMap;
        if (map == null) return false; // no map selected yet

        // Use FindAction to avoid KeyNotFound + allow null checks
        _move = map.FindAction("Movement", throwIfNotFound: false);
        _jump = map.FindAction("Jump", throwIfNotFound: false);
        _shootAction = map.FindAction("Shoot", throwIfNotFound: false);

        if (_move == null || _jump == null || _shootAction == null) return false;

        // Subscribe once
        _move.performed += OnMove;
        _move.canceled += OnMoveCanceled;
        _jump.performed += OnJump;
        _shootAction.started += OnShootStarted;
        _shootAction.canceled += OnShootCanceled;

        map.Enable();
        _bound = true;
        return true;
    }

    private void FixedUpdate()
    {
        if (!canControl || !_bound) return;
        _movement.SwitchVelocity(_shoot.isCharging);
    }

    // --- Callbacks ---

    // Movement: read Vector2.x for horizontal games or full Vector2 for 2D twin-stick
    private void OnMove(InputAction.CallbackContext ctx)
    {
        if (!canControl) return;
        _movement.moveDirection = ctx.ReadValue<Vector2>().x;
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        _movement.moveDirection = 0f;
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (!canControl || !ctx.performed) return;
        _movement.Jump();
    }

    private void OnShootStarted(InputAction.CallbackContext ctx)
    {
        if (!canControl) return;
        _shoot.BeginCharge();
    }

    private void OnShootCanceled(InputAction.CallbackContext ctx)
    {
        if (!canControl) return;
        _shoot.ReleaseCharge();
    }
}
