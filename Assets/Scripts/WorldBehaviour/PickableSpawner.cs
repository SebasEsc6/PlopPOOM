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

    // 3 puntos de spawn: izquierda, centro, derecha
    private Transform leftSpawn;
    private Transform centerSpawn;
    private Transform rightSpawn;

    private List<Transform> spawnPointList = new();
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    private void Start()
    {
        CreateSpawnPointsAboveScreen();
    }

    private void CanSpawnPickables(IGameState newState)
    {
        if (newState is PlayingState)
        {
            canSpawn = true;
        }
        else
        {
            canSpawn = false;
        }
    }

    public void UpdateSpawner()
    {
        if (!IsHost) return;
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

    #region TrySpawnPickables
    public void TrySpawnRandomItem()
    {
        if (!CanSpawnMore()) return;

        var prefab = itemsPrefabs[Random.Range(0, itemsPrefabs.Count)];
        var instance = NetworkObjectPool.Singleton.GetNetworkObject(
            prefab,
            GetRandomSpawnPosition(),
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
                    GetRandomSpawnPosition(),
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
        if (!CanSpawnMore()) return;

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
                    GetRandomSpawnPosition(),
                    Quaternion.identity
                );

                instance.Spawn();
                activePickables.Add(instance.gameObject);
                break;
            }
        }
    }
    #endregion
    public void Release(GameObject obj)
    {
        activePickables.Remove(obj);
    }

    private bool CanSpawnMore() => activePickables.Count < maxActivePickables;

    private void CreateSpawnPointsAboveScreen()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 left = cam.ViewportToWorldPoint(new Vector3(0.1f, 1.1f));
        Vector3 center = cam.ViewportToWorldPoint(new Vector3(0.5f, 1.1f));
        Vector3 right = cam.ViewportToWorldPoint(new Vector3(0.9f, 1.1f));

        left.z = center.z = right.z = 0f;

        leftSpawn = new GameObject("LeftSpawn").transform;
        centerSpawn = new GameObject("CenterSpawn").transform;
        rightSpawn = new GameObject("RightSpawn").transform;

        leftSpawn.position = left;
        centerSpawn.position = center;
        rightSpawn.position = right;

        spawnPointList = new List<Transform> { leftSpawn, centerSpawn, rightSpawn };
    }

    private Vector3 GetRandomSpawnPosition()
    {
        var basePoint = spawnPointList[Random.Range(0, spawnPointList.Count)].position;
        float offsetX = Random.Range(-1f, 1f);
        return new Vector3(basePoint.x + offsetX, basePoint.y, basePoint.z);
    }
}
