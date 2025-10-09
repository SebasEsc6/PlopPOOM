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
        // Do not allow devices to be stolen by other players
        playerInput.neverAutoSwitchControlSchemes = true;

        // --- Pick a control scheme that actually supports the joined device ---
        // Note: older Input System versions expect the *scheme name* (string),
        // and InputDevice[] as the second argument.
        var dev = playerInput.devices.FirstOrDefault(); // first paired device for this player
        var actions = playerInput.actions;

        if (dev != null && actions != null)
        {
            // Find a scheme whose binding groups support this device
            var scheme = actions.controlSchemes.FirstOrDefault(cs => cs.SupportsDevice(dev));
            if (!string.IsNullOrEmpty(scheme.name))
            {
                // Older API: pass the scheme *name* and the device(s)
                playerInput.SwitchCurrentControlScheme(scheme.name, dev);
            }
        }

        // --- Indexing and naming ---
        int index = connectedPlayers.Count; // 0 for first player, 1 for second
        connectedPlayers.Add(playerInput);
        playerInput.gameObject.name = $"Player_{index + 1}";

        // --- Optional spawn point per index ---
        if (spawnPoints != null && spawnPoints.Length > index && spawnPoints[index] != null)
        {
            playerInput.transform.SetPositionAndRotation(
                spawnPoints[index].position, spawnPoints[index].rotation);
        }

        // After first join, swap prefab so next join uses the second variant
        if (connectedPlayers.Count == 1 && player2Prefab != null)
            _inputManager.playerPrefab = player2Prefab;

        // Stop further joins when we reach the limit configured in the inspector
        if (connectedPlayers.Count >= _inputManager.maxPlayerCount)
            _inputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;

        // Hotplug safety
        playerInput.onDeviceLost += _ => PausePlayer(playerInput, true);
        playerInput.onDeviceRegained += _ => PausePlayer(playerInput, false);

        // Debug info
        var devicesStr = string.Join(", ", playerInput.devices.Select(d => $"{d.displayName} ({d.deviceId})"));
        var mapName = playerInput.currentActionMap != null ? playerInput.currentActionMap.name : "<null>";
        Debug.Log($"[JOIN] P{index + 1} | Map={mapName} | Scheme={playerInput.currentControlScheme} | Devices=[{devicesStr}]");
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
