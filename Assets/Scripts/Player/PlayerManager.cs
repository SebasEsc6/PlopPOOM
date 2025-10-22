using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputManager))]
public class PlayerManager : MonoBehaviour
{
    [Header("Player Prefabs")]
    [SerializeField] private GameObject player1Prefab;
    [SerializeField] private GameObject player2Prefab;

    [Header("Spawns (index 0 -> P1, index 1 -> P2)")]
    [SerializeField] private Transform[] spawnPoints;

    private readonly List<PlayerInput> connectedPlayers = new();
    private PlayerInputManager _inputManager;

    private void Awake()
    {
        _inputManager = GetComponent<PlayerInputManager>();
        // _inputManager.maxPlayerCount = 2; // <- asegúrate de fijarlo
        _inputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersWhenButtonIsPressed;
        _inputManager.playerPrefab = player1Prefab; // primero P1
    }

    private void OnEnable()
    {
        _inputManager.onPlayerJoined += HandlePlayerJoined;
        _inputManager.onPlayerLeft   += HandlePlayerLeft;
    }

    private void OnDisable()
    {
        _inputManager.onPlayerJoined -= HandlePlayerJoined;
        _inputManager.onPlayerLeft   -= HandlePlayerLeft;
    }

    private void HandlePlayerJoined(PlayerInput playerInput)
    {
        // No robar dispositivos
        playerInput.neverAutoSwitchControlSchemes = true;

        // Índice del jugador que acaba de entrar
        int index = connectedPlayers.Count;
        connectedPlayers.Add(playerInput);
        playerInput.gameObject.name = $"Player_{index + 1}";

        // Si estamos usando el esquema "Keyboard", forzamos:
        //  - mismo teclado para ambos
        //  - action map distinto por jugador
        //  - máscara de bindings del grupo "Keyboard"
        if (playerInput.defaultControlScheme == "Keyboard" || 
            playerInput.currentControlScheme == "Keyboard")
        {
                // Empareja el teclado físico a este PlayerInput
                // (Unity permite compartirlo si tienes "Enable Split Keyboard" activado)
                if (Keyboard.current != null && !playerInput.devices.Contains(Keyboard.current))
                    // playerInput.user.AssociateDevice(Keyboard.current);
                    playerInput.user.ActivateControlScheme("Keyboard");

            // Fuerza el control scheme + máscara de bindings
            playerInput.SwitchCurrentControlScheme("Keyboard", Keyboard.current);
            playerInput.actions.bindingMask = InputBinding.MaskByGroup("Keyboard");

            // Mapa por jugador (debe existir en tu Input Action Asset)
            var mapName = (index == 0) ? "Player1" : "Player2";
            if (playerInput.actions.actionMaps.Any(m => m.name == mapName))
                playerInput.SwitchCurrentActionMap(mapName);
            else
                Debug.LogWarning($"Action Map '{mapName}' no existe en el asset.");
        }

        // Prefab del siguiente jugador (si usas dos prefabs)
        if (connectedPlayers.Count == 1 && player2Prefab != null)
            _inputManager.playerPrefab = player2Prefab;

        // Cierra el join al llegar al límite
        if (connectedPlayers.Count >= _inputManager.maxPlayerCount)
            _inputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;

        // Debug útil
        var devicesStr = string.Join(", ", playerInput.devices.Select(d => d.displayName));
        Debug.Log($"[JOIN] P{index + 1} | Map={playerInput.currentActionMap?.name} | Scheme={playerInput.currentControlScheme} | Devices=[{devicesStr}]");
    }


    private void HandlePlayerLeft(PlayerInput playerInput)
    {
        int idx = connectedPlayers.IndexOf(playerInput);
        if (idx >= 0) connectedPlayers.RemoveAt(idx);

        if (connectedPlayers.Count < _inputManager.maxPlayerCount)
            _inputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersWhenButtonIsPressed;

        _inputManager.playerPrefab = connectedPlayers.Count == 0 ? player1Prefab : player2Prefab;

        Debug.Log($"[LEAVE] Player removed. Now {connectedPlayers.Count} connected.");
    }

    private void PausePlayer(PlayerInput input, bool isPaused)
    {
        var ec = input.GetComponent<EventController>();
        if (ec != null) ec.canControl = !isPaused;
    }
}
