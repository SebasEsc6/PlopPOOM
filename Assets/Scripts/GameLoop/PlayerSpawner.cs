using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private List<PlayerData> playerDataList;
    [SerializeField] private string gameSceneName;
    [SerializeField] private GameLoopManager gameLoopManager;

    private HashSet<ulong> _spawned = new HashSet<ulong>();

    private void OnEnable()
    {
        var nm = NetworkManager.Singleton;
        nm.OnClientConnectedCallback += OnClientConnected;
        nm.SceneManager.OnLoadComplete += OnLoadComplete;
    }

    private void OnDisable()
    {
        var nm = NetworkManager.Singleton;
        if (nm != null)
        {
            nm.OnClientConnectedCallback -= OnClientConnected;
            nm.SceneManager.OnLoadComplete -= OnLoadComplete;
        }
    }

    private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode mode)
    {
        if (sceneName != gameSceneName || !NetworkManager.Singleton.IsHost) return;

        foreach (ulong id in NetworkManager.Singleton.ConnectedClientsIds)
        {
            TrySpawn(id);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsHost) return;

        if (SceneManager.GetActiveScene().name == gameSceneName)
        {
            TrySpawn(clientId);
        }
    }

    private void TrySpawn(ulong clientId)
    {
        if (_spawned.Contains(clientId)) return;

        if (playerDataList.Count == 0)
        {
            Debug.LogWarning("[PlayerSpawner] No more PlayerData to assign.");
            return;
        }

        PlayerData data = playerDataList[0];
        playerDataList.RemoveAt(0);
        data.clientId = clientId;

        var go = Instantiate(data.playerPrefab, GetSpawnPositionForPlayer(clientId), Quaternion.identity);
        go.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

        // gameLoopManager.players.Add(go);
        gameLoopManager.playerDataList.Add(data.ToNetData());

        _spawned.Add(clientId);
        Debug.Log($"[PlayerSpawner] Spawned {data.playerName} (Client {clientId}) with prefab {data.playerPrefab.name}.");
    }


    private Vector3 GetSpawnPositionForPlayer(ulong clientId)
    {
        var ids = new List<ulong>(NetworkManager.Singleton.ConnectedClientsIds);
        int index = ids.IndexOf(clientId);

        if (gameLoopManager.spawnPoints == null || gameLoopManager.spawnPoints.Length == 0)
        {
            Debug.LogWarning("[PlayerSpawner] No spawn points set in GameLoopManager.");
            return Vector3.zero;
        }

        int spawnIndex = index % gameLoopManager.spawnPoints.Length;
        return gameLoopManager.spawnPoints[spawnIndex].position;
    }
}

