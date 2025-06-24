using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;

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
        var instance = Instantiate(playerPrefab);
        var netObj = instance.GetComponent<NetworkObject>();
        // give ownership so each player controls their character
        netObj.SpawnAsPlayerObject(clientId, true);
    }
}
