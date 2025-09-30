using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private GameObject player1Prefab;
    [SerializeField] private GameObject player2Prefab;
    
    private List<PlayerInput> connectedPlayers = new List<PlayerInput>();
    private PlayerInputManager inputManager;

    private void Awake()
    {
        inputManager = GetComponent<PlayerInputManager>();
        
        if (inputManager == null)
        {
            inputManager = gameObject.AddComponent<PlayerInputManager>();
            inputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersWhenButtonIsPressed;
            inputManager.playerPrefab = player1Prefab;
        }
    }

    private void OnEnable()
    {
        inputManager.onPlayerJoined += HandlePlayerJoined;
    }

    private void OnDisable()
    {
        inputManager.onPlayerJoined -= HandlePlayerJoined;
    }

    private void HandlePlayerJoined(PlayerInput playerInput)
    {
        connectedPlayers.Add(playerInput);
        
        if (connectedPlayers.Count == 1)
        {
            EventController eventController = playerInput.GetComponent<EventController>();
            if (eventController != null)
            {
                eventController.SetPlayerID(0);
            }
            
            inputManager.playerPrefab = player2Prefab;
        }
        else if (connectedPlayers.Count == 2)
        {
            EventController eventController = playerInput.GetComponent<EventController>();
            if (eventController != null)
            {
                eventController.SetPlayerID(1);
            }
            
            inputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;
        }
        
        Debug.Log($"Jugador {connectedPlayers.Count} conectado con dispositivo: {playerInput.devices[0].name}");
    }
}