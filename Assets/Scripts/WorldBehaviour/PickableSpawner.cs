using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PickableSpawner : NetworkBehaviour
{
    [Header("Item & PowerUp Prefabs")]
    [SerializeField] private List<GameObject> itemsPrefabs;
    [SerializeField] private List<GameObject> powerUpsPrefabs;

    private GameObject activeItem;
    private GameObject activePowerUp;

    public void TrySpawnRandomItem()
    {
        if (activeItem != null) return;

        var prefab = itemsPrefabs[Random.Range(0, itemsPrefabs.Count)];
        var instance = NetworkObjectPool.Singleton.GetNetworkObject(prefab, GetItemSpawnPos(), Quaternion.identity);
        instance.Spawn();
        activeItem = instance.gameObject;
    }

    public void TrySpawnPowerUp()
    {
        // If a power-up is already active, do not spawn another
        if (activePowerUp != null) return;

        // 1. Calculate the total spawn probability from all power-up prefabs
        float totalProb = 0f;
        foreach (var prefab in powerUpsPrefabs)
        {
            var powerUp = prefab.GetComponent<PowerUpBase>();
            if (powerUp?.sO_PowerUps == null) continue;

            totalProb += powerUp.sO_PowerUps.spawnProb;
        }

        // Abort if total probability is zero or invalid
        if (totalProb <= 0f) return;

        // 2. Roll a random value between 0 and the total probability
        float roll = Random.Range(0f, totalProb);
        float cumulative = 0f;

        // 3. Iterate through prefabs and accumulate probabilities until the roll fits
        foreach (var prefab in powerUpsPrefabs)
        {
            var powerUp = prefab.GetComponent<PowerUpBase>();
            if (powerUp?.sO_PowerUps == null) continue;

            cumulative += powerUp.sO_PowerUps.spawnProb;

            // If the random roll falls within this range, spawn this power-up
            if (roll <= cumulative)
            {
                var instance = NetworkObjectPool.Singleton.GetNetworkObject(
                    prefab,
                    GetPowerUpSpawnPos(),
                    Quaternion.identity
                );
                instance.Spawn();
                activePowerUp = instance.gameObject;
                break;
            }
        }
    }

    public void ReleasePowerUp(GameObject obj)
    {
        if (activePowerUp == obj)
            activePowerUp = null;
    }

    public void ReleaseItem(GameObject obj)
    {
        if (activeItem == obj)
            activeItem = null;
    }

    private Vector3 GetItemSpawnPos()
    {
        return new Vector3(Random.Range(-3f, 3f), 2f, Random.Range(-3f, 3f));
    }

    private Vector3 GetPowerUpSpawnPos()
    {
        return new Vector3(Random.Range(-3f, 3f), 2f, Random.Range(-3f, 3f));
    }
}
