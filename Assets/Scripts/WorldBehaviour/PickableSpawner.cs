using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PickableSpawner : NetworkBehaviour
{
    [Header("Pickable Prefabs")]
    [SerializeField] private List<GameObject> itemsPrefabs;
    [SerializeField] private List<GameObject> powerUpsPrefabs;
    [SerializeField] private List<GameObject> weaponsPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private int maxActivePickables = 3;

    private readonly List<GameObject> activePickables = new();
    public bool canSpawn = true;

    private float itemTimer;
    private float powerUpTimer;
    private float weaponTimer;

    [SerializeField] private float itemCooldown = 5f;
    [SerializeField] private float powerUpCooldown = 10f;
    [SerializeField] private float weaponCooldown = 15f;

    public void UpdateSpawner()
    {
        if (!canSpawn) return;

        itemTimer += Time.deltaTime;
        powerUpTimer += Time.deltaTime;
        weaponTimer += Time.deltaTime;

        if (itemTimer >= itemCooldown)
        {
            TrySpawnRandomItem();
            itemTimer = 0f;
        }

        if (powerUpTimer >= powerUpCooldown)
        {
            TrySpawnPowerUp();
            powerUpTimer = 0f;
        }

        if (weaponTimer >= weaponCooldown)
        {
            TrySpawnWeapon();
            weaponTimer = 0f;
        }
    }


    // === Public Spawn Methods ===

    public void TrySpawnRandomItem()
    {
        if (!CanSpawnMore() || !canSpawn) return;

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
        if (!CanSpawnMore() || !canSpawn) return;

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

    public void TrySpawnWeapon()
    {
        if (!CanSpawnMore() || !canSpawn) return;

        float totalProb = 0f;

        foreach (var prefab in weaponsPrefabs)
        {
            var weapon = prefab.GetComponent<WeaponIndentifier>();
            if (weapon?.so_Weapons == null) continue;

            totalProb += weapon.so_Weapons.dropProb;
        }

        if (totalProb <= 0f) return;

        float roll = Random.Range(0f, totalProb);
        float cumulative = 0f;

        foreach (var prefab in weaponsPrefabs)
        {
            var weapon = prefab.GetComponent<WeaponIndentifier>();
            if (weapon?.so_Weapons == null) continue;

            cumulative += weapon.so_Weapons.dropProb;

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
