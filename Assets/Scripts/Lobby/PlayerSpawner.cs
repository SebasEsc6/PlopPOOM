using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;

    [SerializeField]
    private string gameSceneName = "GameScene";

    private NetworkManager net;

    private void Awake()
    {
        net = NetworkManager.Singleton;
    }

    private void OnEnable()
    {
        net.SceneManager.OnLoadComplete += OnLoadComplete;
        net.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDisable()
    {
        net.SceneManager.OnLoadComplete -= OnLoadComplete;
        net.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode mode)
    {
        if (sceneName == gameSceneName && net.IsHost)
        {
            Spawn(clientId);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (SceneManager.GetActiveScene().name != gameSceneName) return;

        if (clientId == net.LocalClientId && net.IsHost) return;

        Spawn(clientId);
    }

    private void Spawn(ulong clientId)
    {
        var go = Instantiate(playerPrefab);
        var netObj = go.GetComponent<NetworkObject>();
        netObj.SpawnAsPlayerObject(clientId, true);
        Debug.Log($"[PlayerSpawner] Spawned player for client {clientId}");
    }
}
