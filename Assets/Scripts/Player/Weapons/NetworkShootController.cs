using Unity.Netcode;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NetworkStatsController))]
public class NetworkShootController : NetworkBehaviour
{
    [Header("Prefabs / References")]
    [SerializeField] Transform firePoint;
    [SerializeField] GameObject bulletPrefab;

    [Header("Charge Settings")]
    [SerializeField] float maxChargeTime = 2f;
    [SerializeField] float startScale = .5f;
    [SerializeField] float maxScale = 2f;

    [Header("Bullet Stats")]
    [SerializeField] float minSpeed = 5f;
    [SerializeField] float maxSpeed = 20f;
    [SerializeField] int minDamage = 10;
    [SerializeField] int maxDamage = 50;
    [SerializeField] float bulletLifeTime = 3f;

    NetworkStatsController stats;
    NetworkObject currentBullet;
    Coroutine chargeCo;
    FollowDuringCharge followDuringCharge;
    bool isCharging;

    /// <summary>
    /// Enables this component if the player has authority over it.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        enabled = HasAuthority;
        stats = GetComponent<NetworkStatsController>();
    }

    /// <summary>
    /// Begins charging the shot and instantiates a pooled bullet that follows the firePoint.
    /// </summary>
    public void BeginCharge()
    {
        if (isCharging || stats.CurrentAmmo.Value <= 0) return;

        var bulletObj = NetworkObjectPool.Singleton.GetNetworkObject(bulletPrefab, firePoint.position, Quaternion.identity);
        bulletObj.SpawnWithOwnership(OwnerClientId);
        currentBullet = bulletObj;

        followDuringCharge = currentBullet.GetComponent<FollowDuringCharge>();
        followDuringCharge.enabled = true;
        followDuringCharge.Init(firePoint);

        var rb = currentBullet.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        currentBullet.transform.localScale = Vector3.one * startScale;

        isCharging = true;
        chargeCo = StartCoroutine(ChargeRoutine(currentBullet.transform));
    }

    /// <summary>
    /// Releases the charged bullet, applies damage and velocity, and schedules return to pool.
    /// </summary>
    public void ReleaseCharge()
    {
        if (!isCharging || currentBullet == null) return;
        isCharging = false;
        if (chargeCo != null) StopCoroutine(chargeCo);

        followDuringCharge.enabled = false;

        float t = Mathf.InverseLerp(startScale, maxScale,
                                    currentBullet.transform.localScale.x);

        int dmg = Mathf.RoundToInt(Mathf.Lerp(minDamage, maxDamage, t));
        float speed = Mathf.Lerp(minSpeed, maxSpeed, t);

        var bulletCtrl = currentBullet.GetComponent<NetworkBulletController>();
        Vector2 velocity = new(Mathf.Sign(transform.localScale.x) * speed, 0);
        bulletCtrl.Init(gameObject, dmg, bulletLifeTime, velocity);

        currentBullet.transform.SetParent(null);

        int ammoCost = Mathf.RoundToInt(Mathf.Lerp(1, 5, t));
        stats.SpendAmmoServerRpc(ammoCost);

        currentBullet = null;
    }

    /// <summary>
    /// Smoothly scales the bullet during the charging phase.
    /// </summary>
    IEnumerator ChargeRoutine(Transform bulletTr)
    {
        float time = 0f;
        while (isCharging && time < maxChargeTime)
        {
            time += Time.deltaTime;
            float t = time / maxChargeTime;
            bulletTr.localScale = Vector3.one * Mathf.Lerp(startScale, maxScale, t);
            yield return null;
        }
        bulletTr.localScale = Vector3.one * maxScale;
    }
}
