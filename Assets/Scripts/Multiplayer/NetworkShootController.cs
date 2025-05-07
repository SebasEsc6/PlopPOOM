// NetworkShootController.cs  ───────────────────────────────────────────
using Unity.Netcode;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NetworkStatsController))]
public class NetworkShootController : NetworkBehaviour
{
    [Header("Prefabs / Refs")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform  firePoint;

    [Header("Charge")]
    [SerializeField] float maxChargeTime = 2f;
    [SerializeField] float startScale    = .5f;
    [SerializeField] float maxScale      = 2f;

    [Header("Runtime")]
    [SerializeField] float minSpeed  = 5f;
    [SerializeField] float maxSpeed  = 20f;
    [SerializeField] int   minDamage = 10;
    [SerializeField] int   maxDamage = 50;
    [SerializeField] float bulletLifeTime = 3f;

    NetworkStatsController stats;
    NetworkObject currentBullet;
    Coroutine chargeCo;
    FollowDuringCharge followDuringCharge;
    bool isCharging;

    public override void OnNetworkSpawn()
    {
        enabled = HasAuthority;
        stats = GetComponent<NetworkStatsController>();
    }

    public void BeginCharge()
    {
        if (isCharging || stats.CurrentAmmo.Value <= 0) return;

        var go = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        currentBullet = go.GetComponent<NetworkObject>();
        currentBullet.Spawn();                         

        followDuringCharge = go.GetComponent<FollowDuringCharge>();
        followDuringCharge.enabled = true;    
        currentBullet.GetComponent<FollowDuringCharge>().Init(firePoint);    

        var rb = go.GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
        rb.linearVelocity = Vector2.zero;

        go.transform.localScale = Vector3.one * startScale;

        isCharging = true;
        chargeCo = StartCoroutine(ChargeRoutine(go.transform));
    }

    public void ReleaseCharge()
    {
        if (!isCharging) return;
        isCharging = false;
        if (chargeCo != null) StopCoroutine(chargeCo);

        followDuringCharge.enabled = false;

        float t = Mathf.InverseLerp(startScale, maxScale,
                                    currentBullet.transform.localScale.x);

        int   dmg   = Mathf.RoundToInt(Mathf.Lerp(minDamage, maxDamage, t));
        float speed = Mathf.Lerp(minSpeed,  maxSpeed,  t);

        var bulletCtrl = currentBullet.GetComponent<NetworkBulletController>();
        bulletCtrl.SetDamage(dmg);                   // NetworkVariable<int>

        var rb = currentBullet.GetComponent<Rigidbody2D>();
        rb.isKinematic = false;
        rb.linearVelocity    = new Vector2(Mathf.Sign(transform.localScale.x) * speed, 0);

        currentBullet.transform.SetParent(null);
        bulletCtrl.ScheduleDespawn(bulletLifeTime);

        int ammoCost = Mathf.RoundToInt(Mathf.Lerp(1, 5, t));
        stats.SpendAmmoServerRpc(ammoCost);

        currentBullet = null;
    }

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
