using System.Collections.Generic;
using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> weapons;
    public void SpawnSingleWeapon()
    {
        var weaponSpawned = NetworkObjectPool.Singleton.GetNetworkObject(weapons[0], new Vector3(2, 0, 0), Quaternion.identity);
        weaponSpawned.Spawn();
    }
}
