using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class GameLoopManager : NetworkBehaviour
{
    [Header("Map Stats")]
    public float deadHeight = -5;
    public float timeToStart = 5;

    [Header("Match Rules")]
    [SerializeField] private int maxKillsToWin = 3;

    [Header("References")]
    public GameManager gameManager;
    public PickableSpawner spawner;
    [SerializeField] private UIGameLoopManager uI_Manager;
    [SerializeField] private CinemachineTargetGroup targetGroup;

    [Header("Players")]
    public List<GameObject> players = new();

    [Header("Player Data")]
    public NetworkList<PlayerDataNet> playerDataList = new NetworkList<PlayerDataNet>();

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    public NetworkVariable<float> countdownTimer = new(5f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public readonly List<NetworkStatsController> statsControllers = new();

    void Awake()
    {
        gameManager = GameManager.Instance;
        gameManager.gameLoopManager = this;
        gameManager.Spawner = spawner;
        gameManager.SetState(new WaitingState());
    }

    public override void OnNetworkSpawn()
    {
        if (playerDataList == null)
            playerDataList = new NetworkList<PlayerDataNet>();

        base.OnNetworkSpawn();
    }

    private void Start()
    {
        if (gameManager.currentState is WaitingState)
        {
            AddAllPlayers();
        }

        if (IsServer)
        {
            countdownTimer.Value = timeToStart;
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        if (countdownTimer.Value > 0f)
        {
            countdownTimer.Value -= Time.deltaTime;

            if (countdownTimer.Value <= 0f)
            {
                countdownTimer.Value = 0f;
                Debug.Log("[GameLoop] Countdown finished. Start match.");
            }
        }
    }

    public void AddAllPlayers()
    {
        var found = FindAllPlayers();
        foreach (var pc in found)
        {
            RegisterPlayer(pc.gameObject);
            var ctrl = pc.GetComponent<NetworkStatsController>();
            if (ctrl != null)
            {
                statsControllers.Add(ctrl);
                ctrl.OnKillsChanged += OnPlayerKillsOrLivesChanged;
                ctrl.OnLivesChanged += OnPlayerKillsOrLivesChanged;
            }
        }
    }

    public PlayerController[] FindAllPlayers()
    {
        return Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
    }

    private void OnPlayerKillsOrLivesChanged(int _)
    {
        CheckEndGameConditions();
    }

    private void RegisterPlayer(GameObject playerObj)
    {
        if (players.Contains(playerObj)) return;

        players.Add(playerObj);
        playerObj.GetComponent<PlayerController>().gameLoopManager = this;

        ulong id = playerObj.GetComponent<NetworkObject>().OwnerClientId;

        if (targetGroup != null)
        {
            targetGroup.AddMember(playerObj.transform, 1, 2);
        }
    }

    public Vector3 GetRandomSpawnPosition()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[Respawn] No spawn points set.");
            return Vector3.zero;
        }

        int index = Random.Range(0, spawnPoints.Length);
        return spawnPoints[index].position;
    }

    private void CheckEndGameConditions()
    {
        foreach (var statsCtrl in statsControllers)
        {
            if (statsCtrl.Kills.Value >= maxKillsToWin)
            {
                Debug.Log($"[GameLoop] Player {statsCtrl.OwnerClientId} won by kills!");
                gameManager.SetState(new EndedState());
                uI_Manager.ActiveFeedback(GetPlayerDataByClientId(statsCtrl.OwnerClientId));
                return;
            }
        }

        bool allDead = true;
        foreach (var statsCtrl in statsControllers)
        {
            if (statsCtrl.Lives.Value > 0)
            {
                allDead = false;
                break;
            }
        }

        if (allDead)
        {
            Debug.Log("[GameLoop] All players are out of lives. Game Over.");
            gameManager.SetState(new EndedState());
        }

        int aliveCount = 0;
        ulong lastAliveId = 0;

        foreach (var statsCtrl in statsControllers)
        {
            if (statsCtrl.Lives.Value > 0)
            {
                aliveCount++;
                lastAliveId = statsCtrl.OwnerClientId;
            }
        }

        if (aliveCount == 1)
        {
            Debug.Log($"[GameLoop] Player {lastAliveId} is the last alive and wins!");
            gameManager.SetState(new EndedState());
            uI_Manager.ActiveFeedback(GetPlayerDataByClientId(lastAliveId));
        }
    }

    private PlayerDataNet GetPlayerDataByClientId(ulong clientId)
    {
        foreach (var playerData in playerDataList)
        {
            if (playerData.clientId == clientId)
                return playerData;
        }

        Debug.LogWarning($"[GameLoop] No PlayerData found for clientId: {clientId}");
        return default;
    }
}
