using System.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Logic for pistol weapon. Uses base charge/discharge logic, but customizes bullet behavior.
/// </summary>
public class Weapon_Pistol : WeaponBase
{
    public override void OnNetworkSpawn()
    {
        enabled = HasAuthority;
    }

    /// <summary>
    /// Optional override if you want specific scaling behavior on charge.
    /// Otherwise, use base logic.
    /// </summary>
    protected override void SetupBulletInitialState(Transform bulletTr)
    {
        bulletTr.localScale = Vector3.one * runtimeStats.startScale;
    }

    /// <summary>
    /// Applies damage and velocity based on scale. Overrides base bullet finalization.
    /// </summary>
    protected override void FinalizeBullet(Transform bulletTr)
    {
        float t = Mathf.InverseLerp(runtimeStats.startScale, runtimeStats.maxScale, bulletTr.localScale.x);

        int dmg = Mathf.RoundToInt(Mathf.Lerp(runtimeStats.minDamage, runtimeStats.maxDamage, t));
        float speed = Mathf.Lerp(runtimeStats.minSpeed, runtimeStats.maxSpeed, t);
        Vector2 velocity = new(Mathf.Sign(transform.localScale.x) * speed, 0);

        finalDamage = dmg;
        var bulletCtrl = bulletTr.GetComponent<NetworkBulletController>();
        bulletCtrl.Init(finalDamage, runtimeStats.bulletLifeTime, velocity);
    }

    /// <summary>
    /// Charges the bullet visually over time.
    /// </summary>
    public override IEnumerator ChargeRoutine(Transform bulletTr)
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

    /// <summary>
    /// Ammo cost based on how charged the shot is.
    /// </summary>
    protected override void SpendAmmo(Transform bulletTr)
    {
        float t = Mathf.InverseLerp(runtimeStats.startScale, runtimeStats.maxScale, bulletTr.localScale.x);
        int ammoCost = Mathf.RoundToInt(Mathf.Lerp(1, 5, t));
        statsController.SpendAmmo(ammoCost);
    }
}
