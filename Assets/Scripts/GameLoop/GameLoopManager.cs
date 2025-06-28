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

    public GameManager gameManager;
    public PickableSpawner spawner;

    [SerializeField] private CinemachineTargetGroup targetGroup;

    public List<GameObject> players = new();

    [SerializeField] private Transform[] spawnPoints;

    public NetworkList<PlayerStats> playerStatsList = new();

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

    public void RegisterKill(ulong attackerId)
    {
        

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
    }

    public void ReduceLife(ulong victimId)
    {

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
    }
}
