using UnityEngine;
using Unity.Netcode;

public class WeaponBase : NetworkBehaviour
{
    [SerializeField] protected SO_Weapons weaponData;

    // local copy of stats while runtime
    protected WeaponStats runtimeStats;
    
    public NetworkStatsController statsController;
    [Header("Prefabs / References")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    [HideInInspector]
    public NetworkObject currentBullet;
    public Coroutine chargeCo;
    [HideInInspector]
    public FollowDuringCharge followDuringCharge;
    [HideInInspector]
    public bool isCharging;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        InitializeStatsFromData();
    }

    protected virtual void InitializeStatsFromData()
    {
        if (weaponData == null || weaponData.stats == null)
        {
            Debug.LogError("Weapon data or stats not assigned.");
            return;
        }

        runtimeStats = weaponData.stats.Clone(); //save data for runtime if is necessary 
    }

    public virtual void BeginCharge()
    {
        Debug.Log($"Charging with {runtimeStats.ammoAmount} ammo left. Damage range: {runtimeStats.minDamage}-{runtimeStats.maxDamage}");
    }

    public virtual void ReleaseCharge()
    { 
        Debug.Log($"Shooting with {runtimeStats.ammoAmount} ammo left. Damage range: {runtimeStats.minDamage}-{runtimeStats.maxDamage}");
        runtimeStats.ammoAmount--;
    }

    public virtual void Reload()
    {
        Debug.Log("Reloading...");
        runtimeStats.ammoAmount = weaponData.stats.ammoAmount;
    }

    public WeaponStats GetCurrentStats()
    {
        return runtimeStats;
    }
}
