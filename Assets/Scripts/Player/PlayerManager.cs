using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputManager))]
public class PlayerManager : MonoBehaviour
{
    [Header("Player Prefabs")]
    [SerializeField] private GameObject player1Prefab;
    [SerializeField] private GameObject player2Prefab;

    [Header("Spawns (index 0 -> P1, index 1 -> P2)")]
    [SerializeField] private Transform[] spawnPoints; // Optional, size 2

    private readonly List<PlayerInput> connectedPlayers = new();
    private PlayerInputManager _inputManager;

    private void Awake()
    {
        // Ensure there is a PlayerInputManager and configure it for 2 local players.
        _inputManager = GetComponent<PlayerInputManager>();
        // _inputManager.maxPlayerCount = 2;
        _inputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersWhenButtonIsPressed;
        _inputManager.playerPrefab = player1Prefab; // First join uses P1 prefab (if you need them different)
        // Make sure your PlayerInput prefab(s) have "Auto-Switch" OFF in the Inspector.
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
        // Lock control scheme switching so devices don't get "stolen" by other players.
        playerInput.neverAutoSwitchControlSchemes = true;

        // If the device is a Gamepad, force the Gamepad control scheme for this player.
        // (Replace "Gamepad" with your scheme name if different.)
        var firstDevice = playerInput.devices.FirstOrDefault();
        if (firstDevice is Gamepad gp)
        {
            playerInput.SwitchCurrentControlScheme("Gamepad", gp);
        }

        // Assign player index and optional spawn position.
        int index = connectedPlayers.Count; // 0 for first, 1 for second
        connectedPlayers.Add(playerInput);
        playerInput.gameObject.name = $"Player_{index + 1}";

        if (spawnPoints != null && spawnPoints.Length > index && spawnPoints[index] != null)
        {
            playerInput.transform.SetPositionAndRotation(
                spawnPoints[index].position, spawnPoints[index].rotation);
        }

        // If you keep per-player cosmetics / logic, set an ID on your own controller here.
        var eventController = playerInput.GetComponent<EventController>();
        // if (eventController != null)
        // {
        //     // Example: 0 for P1, 1 for P2
        //     eventController.SetPlayerID(index);
        // }

        // After first join, swap prefab so the next join uses the second variant.
        if (connectedPlayers.Count == 1 && player2Prefab != null)
        {
            _inputManager.playerPrefab = player2Prefab;
        }

        // Lock joining once we reach 2 players; re-enabled on player left.
        if (connectedPlayers.Count >= _inputManager.maxPlayerCount)
        {
            _inputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;
        }

        // Optional: handle hotplug safety per player.
        playerInput.onDeviceLost += _ => PausePlayer(playerInput, true);
        playerInput.onDeviceRegained += _ => PausePlayer(playerInput, false);

        Debug.Log($"[JOIN] Player {index + 1} -> Devices: {string.Join(", ", playerInput.devices.Select(d => d.displayName))}");
    }

    private void HandlePlayerLeft(PlayerInput playerInput)
    {
        int idx = connectedPlayers.IndexOf(playerInput);
        if (idx >= 0) connectedPlayers.RemoveAt(idx);

        // Allow new players to join again up to 2.
        if (connectedPlayers.Count < _inputManager.maxPlayerCount)
        {
            _inputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersWhenButtonIsPressed;
        }

        // Reset to P1 prefab so the next join re-uses the correct sequence.
        _inputManager.playerPrefab = connectedPlayers.Count == 0 ? player1Prefab : player2Prefab;

        Debug.Log($"[LEAVE] Player removed. Now {connectedPlayers.Count} connected.");
    }

    private void PausePlayer(PlayerInput input, bool isPaused)
    {
        // Implement your own pause/lock here: disable movement/shoot, show UI, etc.
        var ec = input.GetComponent<EventController>();
        if (ec != null) ec.canControl = !isPaused;
    }
}
