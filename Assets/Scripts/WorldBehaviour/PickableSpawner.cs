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
        var weaponSpawned = NetworkObjectPool.Singleton.GetNetworkObject(
            itemsPrefabs[randomIndex], 
            new Vector3(0, 2, 0), 
            Quaternion.identity
        );

        weaponSpawned.Spawn(true); // true para asignar autoridad automáticamente si aplica
    }
}
