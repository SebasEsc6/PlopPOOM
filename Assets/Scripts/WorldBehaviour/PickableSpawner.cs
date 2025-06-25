using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PickableSpawner : NetworkBehaviour
{
    [Header("Pickable Prefabs")]
    [SerializeField] private List<GameObject> itemsPrefabs;
    [SerializeField] private List<GameObject> powerUpsPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private int maxActivePickables = 3;

    private readonly List<GameObject> activePickables = new();

    // === Public Spawn Methods ===

    public void TrySpawnRandomItem()
    {
        if (!CanSpawnMore()) return;

        var prefab = itemsPrefabs[Random.Range(0, itemsPrefabs.Count)];
        var instance = NetworkObjectPool.Singleton.GetNetworkObject(
            prefab,
            GetItemSpawnPos(),
            Quaternion.identity
        );

        instance.Spawn();
        activePickables.Add(instance.gameObject);
    }

    public void TrySpawnPowerUp()
    {
        if (!CanSpawnMore()) return;

        float totalProb = 0f;

        foreach (var prefab in powerUpsPrefabs)
        {
            var powerUp = prefab.GetComponent<PowerUpBase>();
            if (powerUp?.sO_PowerUps == null) continue;

            totalProb += powerUp.sO_PowerUps.spawnProb;
        }

        if (totalProb <= 0f) return;

        float roll = Random.Range(0f, totalProb);
        float cumulative = 0f;

        foreach (var prefab in powerUpsPrefabs)
        {
            var powerUp = prefab.GetComponent<PowerUpBase>();
            if (powerUp?.sO_PowerUps == null) continue;

            cumulative += powerUp.sO_PowerUps.spawnProb;

            if (roll <= cumulative)
            {
                var instance = NetworkObjectPool.Singleton.GetNetworkObject(
                    prefab,
                    GetPowerUpSpawnPos(),
                    Quaternion.identity
                );

                instance.Spawn();
                activePickables.Add(instance.gameObject);
                break;
            }
        }
    }

    // === Release ===

    public void Release(GameObject obj)
    {
        activePickables.Remove(obj);
    }

    // === Helpers ===

    private bool CanSpawnMore() => activePickables.Count < maxActivePickables;

    private Vector3 GetItemSpawnPos()
    {
        return new Vector3(Random.Range(-3f, 3f), 2f, Random.Range(-3f, 3f));
    }

    private Vector3 GetPowerUpSpawnPos()
    {
        return new Vector3(Random.Range(-3f, 3f), 2f, Random.Range(-3f, 3f));
    }
}
