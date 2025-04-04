using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class EventController : MonoBehaviour
{
    private enum SelectPlayer
    {
        Player1,
        Player2
    }
    private PlayerInputs _playerInputs;
    private MovementController _movementController;
    private ShootController _shootController;
    [SerializeField] private SelectPlayer selectPlayer;

    void Awake()
    {
        _playerInputs = new();

        _movementController = GetComponent<MovementController>();
        _shootController = GetComponent<ShootController>();
    }

    public void SetPlayerID(int playerID)
    {
        selectPlayer = playerID == 0 ? SelectPlayer.Player1 : SelectPlayer.Player2;
        
        // OnDisable();
        // OnEnable();
    }

    private void OnEnable() 
    {

        if (selectPlayer == SelectPlayer.Player1)
        {
            _playerInputs.Player1.Enable();

            _playerInputs.Player1.Movement.performed += OnMove;
            _playerInputs.Player1.Movement.canceled += CancelMove;

            _playerInputs.Player1.Jump.performed += OnJump;

            _playerInputs.Player1.Shoot.started  += OnShoot;
            _playerInputs.Player1.Shoot.canceled += OnShootCanceled;

        }
        else
        {
            _playerInputs.Player2.Enable();

            _playerInputs.Player2.Movement.performed += OnMove;
            _playerInputs.Player2.Movement.canceled += CancelMove;

            _playerInputs.Player2.Jump.performed += OnJump;

            _playerInputs.Player2.Shoot.started  += OnShoot;
            _playerInputs.Player2.Shoot.canceled += OnShootCanceled;
            
        }
    }

    private void OnDisable() 
    {

        if (selectPlayer == SelectPlayer.Player1)
        {
            _playerInputs.Player1.Disable();

            _playerInputs.Player1.Movement.performed -= OnMove;
            _playerInputs.Player1.Movement.canceled -= CancelMove;

            _playerInputs.Player1.Jump.performed -= OnJump;

            _playerInputs.Player1.Shoot.started  -= OnShoot;
            _playerInputs.Player1.Shoot.canceled -= OnShootCanceled;

        }
        else
        {
            _playerInputs.Player2.Enable();

            _playerInputs.Player2.Movement.performed -= OnMove;
            _playerInputs.Player2.Movement.canceled -= CancelMove;

            _playerInputs.Player2.Jump.performed -= OnJump;

            _playerInputs.Player2.Shoot.started  -= OnShoot;
            _playerInputs.Player2.Shoot.canceled -= OnShootCanceled;
            
        }
    }
    
    private void FixedUpdate() {
        
        _movementController.SwitchVelocity(_shootController.isCharging);
    }

    private void OnMove(InputAction.CallbackContext context)
    {
       _movementController.moveDirection = context.ReadValue<Vector2>().x;
    }

    private void CancelMove(InputAction.CallbackContext context)
    {
        _movementController.moveDirection = 0;
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _movementController.Jump();
        }
    }
    private void OnShoot(InputAction.CallbackContext context)
    {
        // Debug.Log("i'm started");
        _shootController.BeginCharge();
        
    }
    private void OnShootCanceled(InputAction.CallbackContext context)
    {
        _shootController.ReleaseCharge();
    }
}