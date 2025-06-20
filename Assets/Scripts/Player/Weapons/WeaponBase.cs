using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class WeaponBase : NetworkBehaviour
{
    [SerializeField] protected SO_Weapons weaponData;

    // local copy of stats while runtime
    public WeaponStats runtimeStats;

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

    protected float lastShotTime = -999f;

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
        if (isCharging || statsController.CurrentAmmo.Value <= 0 || !CanShoot()) return;

        currentBullet = NetworkObjectPool.Singleton.GetNetworkObject(bulletPrefab, firePoint.position, Quaternion.identity);

        // switch owner after spawn
        currentBullet.GetComponent<NetworkObject>().Spawn(true);
        currentBullet.GetComponent<NetworkObject>().ChangeOwnership(OwnerClientId); // asigne owner

        followDuringCharge = currentBullet.GetComponent<FollowDuringCharge>();
        followDuringCharge.enabled = true;
        followDuringCharge.Init(firePoint);

        var rb = currentBullet.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        SetupBulletInitialState(currentBullet.transform); // variable logic per weapon

        isCharging = true;
        chargeCo = StartCoroutine(ChargeRoutine(currentBullet.transform));
    }

    public virtual void ReleaseCharge()
    {
        if (!isCharging || currentBullet == null) return;
        isCharging = false;
        if (chargeCo != null) StopCoroutine(chargeCo);

        followDuringCharge.enabled = false;

        FinalizeBullet(currentBullet.transform); // variable logic per weapon

        currentBullet.transform.SetParent(null);
        SpendAmmo(currentBullet.transform); // variable logic per weapon

        currentBullet = null;
        lastShotTime = Time.time;
    }

    protected virtual void SetupBulletInitialState(Transform bulletTr)
    {
        bulletTr.localScale = Vector3.one * runtimeStats.startScale;
    }

    protected virtual void FinalizeBullet(Transform bulletTr)
    {
        float t = Mathf.InverseLerp(runtimeStats.startScale, runtimeStats.maxScale, bulletTr.localScale.x);

        int dmg = Mathf.RoundToInt(Mathf.Lerp(runtimeStats.minDamage, runtimeStats.maxDamage, t));
        float speed = Mathf.Lerp(runtimeStats.minSpeed, runtimeStats.maxSpeed, t);
        Vector2 velocity = new(Mathf.Sign(transform.localScale.x) * speed, 0);

        var bulletCtrl = bulletTr.GetComponent<NetworkBulletController>();
        bulletCtrl.Init(gameObject, dmg, runtimeStats.bulletLifeTime, velocity);
    }

    protected virtual void SpendAmmo(Transform bulletTr)
    {
        float t = Mathf.InverseLerp(runtimeStats.startScale, runtimeStats.maxScale, bulletTr.localScale.x);
        int ammoCost = Mathf.RoundToInt(Mathf.Lerp(1, 5, t));
        statsController.SpendAmmoServerRpc(ammoCost);
    }


    public virtual void Reload()
    {
        Debug.Log("Reloading...");
        runtimeStats.ammoAmount = weaponData.stats.ammoAmount;
    }

    protected bool CanShoot()
    {
        return Time.time >= lastShotTime + runtimeStats.cadence;
    }

    public WeaponStats GetCurrentStats()
    {
        return runtimeStats;
    }

    public void SetReferences(SO_Weapons dataWeapon, Transform fire, GameObject bullet)
    {
        weaponData = dataWeapon;
        firePoint = fire;
        bulletPrefab = bullet;
        runtimeStats = weaponData.stats.Clone();
    }
    
    /// <summary>
    /// Smoothly scales the bullet during the charging phase.
    /// </summary>
    public virtual IEnumerator ChargeRoutine(Transform bulletTr)
    {
        float time = 0f;
        while (isCharging && time < runtimeStats.timeToCharge)
        {
            time += Time.deltaTime;
            float t = time / runtimeStats.timeToCharge;
            bulletTr.localScale = Vector3.one * Mathf.Lerp(runtimeStats.startScale, runtimeStats.maxScale, t);
            yield return null;
        }
        bulletTr.localScale = Vector3.one * runtimeStats.maxScale;
    }
}
