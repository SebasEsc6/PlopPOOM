using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class Weapon_AK : WeaponBase
{
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        enabled = HasAuthority;
    }

    public override void BeginCharge()
    {
        if (isCharging || statsController.CurrentAmmo.Value <= 0 || !CanShoot()) return;

        // instance single bullet from pool
        currentBullet = NetworkObjectPool.Singleton.GetNetworkObject(bulletPrefab, firePoint.position, Quaternion.identity);
        currentBullet.Spawn(true);
        currentBullet.ChangeOwnership(OwnerClientId);

        followDuringCharge = currentBullet.GetComponent<FollowDuringCharge>();
        followDuringCharge.enabled = true;
        followDuringCharge.Init(firePoint);

        var rb = currentBullet.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        currentBullet.transform.localScale = Vector3.one * runtimeStats.startScale;

        isCharging = true;
    }

    public override void ReleaseCharge()
    {
        if (!isCharging || currentBullet == null) return;
        isCharging = false;

        followDuringCharge.enabled = false;

        float t = Mathf.InverseLerp(runtimeStats.startScale, runtimeStats.maxScale, currentBullet.transform.localScale.x);
        int dmg = Mathf.RoundToInt(Mathf.Lerp(runtimeStats.minDamage, runtimeStats.maxDamage, t));
        float speed = Mathf.Lerp(runtimeStats.minSpeed, runtimeStats.maxSpeed, t);

        Vector2 direction = new(Mathf.Sign(transform.localScale.x), 0);
        Vector3 spawnPos = firePoint.position;

        // shoot two bullets
        FireSingleBullet(spawnPos + Vector3.up * 0.1f, direction, speed, dmg);
        FireSingleBullet(spawnPos + Vector3.down * 0.1f, direction, speed, dmg);

        int ammoCost = Mathf.RoundToInt(Mathf.Lerp(1, 5, t));
        statsController.SpendAmmo(ammoCost);

        currentBullet = null;
        lastShotTime = Time.time; // apply cooldown 
    }

    void FireSingleBullet(Vector3 position, Vector2 direction, float speed, int damage)
    {
        var bullet = NetworkObjectPool.Singleton.GetNetworkObject(bulletPrefab, position, Quaternion.identity);
        bullet.Spawn(true);
        bullet.ChangeOwnership(OwnerClientId);

        var bulletCtrl = bullet.GetComponent<NetworkBulletController>();
        bulletCtrl.Init(finalDamage, runtimeStats.bulletLifeTime, direction * speed);

        bullet.transform.localScale = Vector3.one * currentBullet.transform.localScale.x;
    }
}
