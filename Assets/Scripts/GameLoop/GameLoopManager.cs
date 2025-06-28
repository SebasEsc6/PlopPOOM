using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using Unity.Collections;
using UnityEngine;

public struct PlayerStats : INetworkSerializable, System.IEquatable<PlayerStats>
{
    public ulong clientId;
    public int kills;
    public int lives;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref kills);
        serializer.SerializeValue(ref lives);
    }

    public bool Equals(PlayerStats other) => clientId == other.clientId;
}

public class GameLoopManager : NetworkBehaviour
{
    [Header("Map Stats")]
    public float deadHeight = -5;

    [Header("Match Rules")]
    [SerializeField] private int maxKillsToWin = 3;

    public GameManager gameManager;
    public PickableSpawner spawner;

    [SerializeField] private CinemachineTargetGroup targetGroup;

    public List<GameObject> players = new();

    [SerializeField] private Transform[] spawnPoints;

    public NetworkList<PlayerStats> playerStatsList = new();
    public float timeToStart = 5;
    public NetworkVariable<float> countdownTimer = new(5f,
    NetworkVariableReadPermission.Everyone,
    NetworkVariableWritePermission.Server);
    void Awake()
    {
        gameManager = GameManager.Instance;
        gameManager.gameLoopManager = this;
        gameManager.Spawner = spawner;
        gameManager.SetState(new WaitingState());
    }

    private void Start()
    {
        if (gameManager.currentState is WaitingState)
        {
            FindAndAddPlayers();
        }

        if (IsServer)
        {
            countdownTimer.Value = timeToStart;
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        if (gameManager.currentState is WaitingState && countdownTimer.Value > 0f)
        {
            countdownTimer.Value -= Time.deltaTime;

            if (countdownTimer.Value <= 0f)
            {
                countdownTimer.Value = 0f;
                Debug.Log("[GameLoop] Countdown finished. Start match.");
            }
        }
    }


    public void FindAndAddPlayers()
    {
        PlayerController[] foundPlayers = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        foreach (var pc in foundPlayers)
        {
            GameObject playerObj = pc.gameObject;

            if (!players.Contains(playerObj))
            {
                players.Add(playerObj);
                playerObj.GetComponent<PlayerController>().gameLoopManager = this;

                ulong id = playerObj.GetComponent<NetworkObject>().OwnerClientId;

                if (IsServer)
                {
                    playerStatsList.Add(new PlayerStats
                    {
                        clientId = id,
                        kills = 0,
                        lives = 3
                    });
                }

                if (targetGroup != null)
                {
                    targetGroup.AddMember(playerObj.transform, 1, 2);
                }
            }
        }
    }

    public Vector3 GetRandomSpawnPosition()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[Respawn] No spawn points set.");
            return Vector3.zero;
        }

        int index = UnityEngine.Random.Range(0, spawnPoints.Length);
        return spawnPoints[index].position;
    }

    #region Player Stats
    public void RegisterKill(ulong attackerId)
    {
        //!!! DONT PUT VALIDATION AS if (!IsServer) return; IT DONS'T WORK >:c
        for (int i = 0; i < playerStatsList.Count; i++)
        {
            if (playerStatsList[i].clientId == attackerId)
            {
                var stat = playerStatsList[i];
                stat.kills++;
                playerStatsList[i] = stat;
                Debug.Log($"[Stats] Player {attackerId} got a kill. Total kills: {stat.kills}");
                break;
            }
        }
        CheckEndGameConditions();
    }

    public void ReduceLife(ulong victimId)
    {
        //!!!DONT PUT VALIDATION AS if (!IsServer) return; IT DONS'T WORK >:c 
        for (int i = 0; i < playerStatsList.Count; i++)
        {
            if (playerStatsList[i].clientId == victimId)
            {
                var stat = playerStatsList[i];
                stat.lives = Mathf.Max(0, stat.lives - 1);
                playerStatsList[i] = stat;
                Debug.Log($"[Stats] Player {victimId} lost a life. Remaining: {stat.lives}");
                break;
            }
        }
        CheckEndGameConditions();
    }
    private void CheckEndGameConditions()
    {
        foreach (var stat in playerStatsList)
        {
            if (stat.kills >= maxKillsToWin)
            {
                Debug.Log($"[GameLoop] Player {stat.clientId} won by kills!");
                gameManager.SetState(new EndedState());
                //? ============HERE CAN PUT THE FEEDBACK WHO WIN==============
                return;
            }
        }

        bool allDead = true;
        foreach (var stat in playerStatsList)
        {
            if (stat.lives > 0)
            {
                allDead = false;
                break;
            }
        }

        if (allDead)
        {
            Debug.Log("[GameLoop] All players are out of lives. Game Over.");
            gameManager.SetState(new EndedState()); // nobody win
        }
    }


    public bool TryGetPlayerStats(ulong clientId, out PlayerStats stats)
    {
        for (int i = 0; i < playerStatsList.Count; i++)
        {
            if (playerStatsList[i].clientId == clientId)
            {
                stats = playerStatsList[i];
                return true;
            }
        }

        stats = default;
        return false;
    }
    #endregion
}
