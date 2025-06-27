using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private string gameSceneName;

    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += SpawnForClient;
    }

    private void OnDisable()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= SpawnForClient;
    }

    private void SpawnForClient(ulong clientId)
    {
        if (SceneManager.GetActiveScene().name != gameSceneName) return;

        var instance = Instantiate(playerPrefab);
        var netObj = instance.GetComponent<NetworkObject>();
        // give ownership so each player controls their character
        netObj.SpawnAsPlayerObject(clientId, true);
    }
}
