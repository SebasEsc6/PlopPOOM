using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
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

    private void Awake()
    {
        // Get components provided on this player instance
        _playerInput = GetComponent<PlayerInput>();
        _movement = GetComponent<MovementController>();
        _shoot = GetComponent<ShootController>();
    }

    private void OnEnable()
    {
        // Cache actions from the current action map (e.g., "Gameplay")
        var map = _playerInput.currentActionMap;
        _move = map["Movement"];
        _jump = map["Jump"];
        _shootAction = map["Shoot"];

        // Subscribe to per-player actions (already paired to the device that joined)
        _move.performed += OnMove;
        _move.canceled += OnMoveCanceled;

        _jump.performed += OnJump;

        _shootAction.started += OnShootStarted;
        _shootAction.canceled += OnShootCanceled;

        map.Enable();
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        _move.performed -= OnMove;
        _move.canceled -= OnMoveCanceled;
        _jump.performed -= OnJump;
        _shootAction.started -= OnShootStarted;
        _shootAction.canceled -= OnShootCanceled;
    }

    private void FixedUpdate()
    {
        if (!canControl) return;
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
