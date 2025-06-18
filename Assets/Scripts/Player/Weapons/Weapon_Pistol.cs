using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Weapon_Pistol : WeaponBase
{
    public override void OnNetworkSpawn()
    {
        enabled = HasAuthority;
    }

    /// <summary>
    /// Begins charging the shot and instantiates a pooled bullet that follows the firePoint.
    /// </summary>
    public override void BeginCharge()
    {
        if (isCharging || statsController.CurrentAmmo.Value <= 0) return;

        var bulletObj = NetworkObjectPool.Singleton.GetNetworkObject(bulletPrefab, firePoint.position, Quaternion.identity);
        bulletObj.Spawn();
        currentBullet = bulletObj;

        followDuringCharge = currentBullet.GetComponent<FollowDuringCharge>();
        followDuringCharge.enabled = true;
        followDuringCharge.Init(firePoint);

        var rb = currentBullet.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        currentBullet.transform.localScale = Vector3.one * runtimeStats.startScale;

        isCharging = true;
        chargeCo = StartCoroutine(ChargeRoutine(currentBullet.transform));
    }

    /// <summary>
    /// Releases the charged bullet, applies damage and velocity, and schedules return to pool.
    /// </summary>
    public override void ReleaseCharge()
    {
        if (!isCharging || currentBullet == null) return;
        isCharging = false;
        if (chargeCo != null) StopCoroutine(chargeCo);

        followDuringCharge.enabled = false;

        float t = Mathf.InverseLerp(runtimeStats.startScale, runtimeStats.maxScale,
                                    currentBullet.transform.localScale.x);

        int dmg = Mathf.RoundToInt(Mathf.Lerp(runtimeStats.minDamage, runtimeStats.maxDamage, t));
        float speed = Mathf.Lerp(runtimeStats.minSpeed, runtimeStats.maxSpeed, t);

        var bulletCtrl = currentBullet.GetComponent<NetworkBulletController>();
        Vector2 velocity = new(Mathf.Sign(transform.localScale.x) * speed, 0);
        bulletCtrl.Init(gameObject, dmg, runtimeStats.bulletLifeTime, velocity);

        currentBullet.transform.SetParent(null);

        int ammoCost = Mathf.RoundToInt(Mathf.Lerp(1, 5, t));
        statsController.SpendAmmoServerRpc(ammoCost);

        currentBullet = null;
    }

    /// <summary>
    /// Smoothly scales the bullet during the charging phase.
    /// </summary>
    IEnumerator ChargeRoutine(Transform bulletTr)
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
