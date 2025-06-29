using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> playerPrefabs;
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

        // spawn everyone who isn't spawned yet
        foreach (ulong id in NetworkManager.Singleton.ConnectedClientsIds)
        {
            TrySpawn(id);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // only the host should actually do the spawn calls
        if (!NetworkManager.Singleton.IsHost) return;

        // if GameScene is already loaded, spawn immediately
        if (SceneManager.GetActiveScene().name == gameSceneName)
        {
            TrySpawn(clientId);
        }
    }

    private void TrySpawn(ulong clientId)
    {
        if (_spawned.Contains(clientId)) return;

        // Pick a prefab for this client
        GameObject prefabToUse = null;
        if (playerPrefabs.Count > 0)
        {
            prefabToUse = playerPrefabs[0];
            playerPrefabs.RemoveAt(0);
        }

        var go = Instantiate(prefabToUse, GetSpawnPositionForPlayer(clientId), Quaternion.identity);
        go.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

        _spawned.Add(clientId);
        Debug.Log($"[PlayerSpawner] Spawned player for Client {clientId} with prefab {prefabToUse.name} at index-based position.");
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
