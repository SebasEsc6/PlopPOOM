using UnityEngine;
using Unity.Netcode;

public class WeaponBase : NetworkBehaviour
{
    public SO_Weapons so_weapon;
    public WeaponStats runtimeStats;
    public NetworkStatsController statsController;

    [Header("Prefabs / References")]
    public Transform firePoint;
    public GameObject bulletPrefab;

    [HideInInspector] public NetworkObject currentBullet;
    [HideInInspector] public bool isCharging;

    public float lastShotTime = -999f;
    private float chargeTime; // lleva el avance de carga

    public int finalDamage;
    private NetworkBulletController bulletCtrl;
    [HideInInspector] public FollowDuringCharge followDuringCharge;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        InitializeStatsFromData();
    }

    protected virtual void InitializeStatsFromData()
    {
        if (so_weapon == null || so_weapon.stats == null)
        {
            Debug.LogError("Weapon data or stats not assigned.");
            return;
        }

        runtimeStats = so_weapon.stats.Clone(); //save data for runtime if is necessary 
    }

    public virtual void BeginCharge()
    {
        if (!IsOwner)
            if (isCharging || statsController.CurrentAmmo.Value <= 0 || !CanShoot()) return;

        currentBullet = NetworkObjectPool.Singleton.GetNetworkObject(bulletPrefab, firePoint.position, Quaternion.identity);
        currentBullet.Spawn(true);
        if (NetworkManager.Singleton.IsServer)
        {
            if (currentBullet.OwnerClientId != OwnerClientId)
            {
                currentBullet.ChangeOwnership(OwnerClientId);
            }
        }

        bulletCtrl = currentBullet.GetComponent<NetworkBulletController>();
        bulletCtrl.OnBeforeReturnToPool += HandleBulletReturn;
        bulletCtrl.DeactivateCollisionOnStart(transform.root.gameObject);
        bulletCtrl.SetStartScale(runtimeStats.startScale);

        followDuringCharge = currentBullet.GetComponent<FollowDuringCharge>();
        followDuringCharge.enabled = true;
        followDuringCharge.Init(firePoint);

        var rb = currentBullet.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        SetupBulletInitialState(currentBullet.transform); // variable logic per weapon
        isCharging = true;
        chargeTime = 0f;
    }

    public virtual void ReleaseCharge()
    {
        if (!isCharging || currentBullet == null) return;
        isCharging = false;

        followDuringCharge.enabled = false;

        FinalizeBullet(currentBullet.transform); // variable logic per weapon
        Debug.Log("I release the bullet");

        currentBullet.transform.SetParent(null);
        SpendAmmo(currentBullet.transform); // variable logic per weapon

        bulletCtrl = null;
        currentBullet = null;
        lastShotTime = Time.time;
        chargeTime = 0f;
    }

    void Update()
    {
        if (isCharging && currentBullet != null && currentBullet.IsSpawned)
        {
            chargeTime += Time.deltaTime;
            float t = Mathf.Clamp01(chargeTime / runtimeStats.timeToCharge);
            float scale = Mathf.Lerp(runtimeStats.startScale, runtimeStats.maxScale, t);
            currentBullet.transform.localScale = Vector3.one * scale;
        }
    }

    private void HandleBulletReturn(NetworkBulletController returningBullet)
    {
        if (isCharging && currentBullet != null &&
            returningBullet == bulletCtrl)
        {
            isCharging = false;
            currentBullet = null;
        }
        returningBullet.OnBeforeReturnToPool -= HandleBulletReturn;
    }

    protected virtual void SetupBulletInitialState(Transform bulletTr)
    {
        bulletTr.localScale = Vector3.one * runtimeStats.startScale;
    }

    protected virtual void FinalizeBullet(Transform bulletTr)
    {
        float t = Mathf.InverseLerp(runtimeStats.startScale, runtimeStats.maxScale, bulletTr.localScale.x);

        finalDamage = Mathf.RoundToInt(Mathf.Lerp(runtimeStats.minDamage, runtimeStats.maxDamage, t));
        float speed = Mathf.Lerp(runtimeStats.minSpeed, runtimeStats.maxSpeed, t);
        Vector2 velocity = new(Mathf.Sign(transform.localScale.x) * speed, 0);

        bulletCtrl.Init(finalDamage, runtimeStats.bulletLifeTime, velocity);
        Debug.Log(transform.root.gameObject);
    }

    protected virtual void SpendAmmo(Transform bulletTr)
    {
        float t = Mathf.InverseLerp(runtimeStats.startScale, runtimeStats.maxScale, bulletTr.localScale.x);
        int ammoCost = Mathf.RoundToInt(Mathf.Lerp(1, 5, t));
        statsController.SpendAmmo(ammoCost - statsController.minAmmoPrice);
    }


    public virtual void Reload()
    {
        Debug.Log("Reloading...");
        runtimeStats.ammoAmount = so_weapon.stats.ammoAmount;
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
        so_weapon = dataWeapon;
        firePoint = fire;
        bulletPrefab = bullet;
        runtimeStats = so_weapon.stats.Clone();
    }
}
