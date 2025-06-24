using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PickableSpawner : NetworkBehaviour
{
    [SerializeField] private List<GameObject> powerUpsPrefabs;
    [SerializeField] private List<GameObject> itemsPrefabs;

    [ContextMenu("Spawn Random Item")]
    public void SpawnRandomItem()
    {
        int randomIndex = Random.Range(0, itemsPrefabs.Count);
        var itemSpawned = NetworkObjectPool.Singleton.GetNetworkObject(
            itemsPrefabs[randomIndex],
            new Vector3(0, 2, 0),
            Quaternion.identity
        );

        itemSpawned.Spawn(true); 
    }
    
    [ContextMenu("Spawn Random PowerUp")]
    public void SpawnRandomPowerUp()
    {
        int randomIndex = Random.Range(0, powerUpsPrefabs.Count);
        var powerUpSpawned = NetworkObjectPool.Singleton.GetNetworkObject(
            powerUpsPrefabs[randomIndex], 
            new Vector3(0, 2, 0), 
            Quaternion.identity
        );

        powerUpSpawned.Spawn(true);
    }
}
