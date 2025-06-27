using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private string gameSceneName = "GameScene";

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
        var go = Instantiate(playerPrefab);
        go.GetComponent<NetworkObject>()
          .SpawnAsPlayerObject(clientId, true);
        _spawned.Add(clientId);
        Debug.Log($"[PlayerSpawner] Spawned player for Client {clientId}");
    }
}
