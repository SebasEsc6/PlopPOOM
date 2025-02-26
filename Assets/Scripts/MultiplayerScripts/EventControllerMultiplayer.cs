using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class EventControllerMultiplayer : NetworkBehaviour
{
    private PlayerInputs _playerInputs;
    private MovementControllerMultiplayer _movementController;
    private ShootController _shootController;

    void Awake()
    {
        _playerInputs = new();

        _movementController = GetComponent<MovementControllerMultiplayer>();
        _shootController = GetComponent<ShootController>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            if (_movementController) _movementController.enabled = false;
            if (_shootController) _shootController.enabled = false;
            enabled = false;
            return;
        }

        _playerInputs.Multiplayer.Enable();

        _playerInputs.Multiplayer.Movement.performed += OnMove;
        _playerInputs.Multiplayer.Movement.canceled += CancelMove;

        _playerInputs.Multiplayer.Jump.performed += OnJump;

        _playerInputs.Multiplayer.Shoot.started += OnShoot;
        _playerInputs.Multiplayer.Shoot.canceled += OnShootCanceled;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        _playerInputs.Multiplayer.Disable();

        _playerInputs.Multiplayer.Movement.performed -= OnMove;
        _playerInputs.Multiplayer.Movement.canceled -= CancelMove;

        _playerInputs.Multiplayer.Jump.performed -= OnJump;

        _playerInputs.Multiplayer.Shoot.started -= OnShoot;
        _playerInputs.Multiplayer.Shoot.canceled -= OnShootCanceled;
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return; 
        _movementController.SwitchVelocity(_shootController.isCharging);
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        _movementController.SetMoveDirection(context.ReadValue<Vector2>().x);
    }

    private void CancelMove(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        _movementController.SetMoveDirection(0);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        _movementController.Jump();
    }

    private void OnShoot(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        _shootController.BeginCharge();
    }

    private void OnShootCanceled(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        _shootController.ReleaseCharge();
    }
}
