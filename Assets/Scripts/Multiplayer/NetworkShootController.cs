using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class NetworkShootController : NetworkBehaviour
{
    [Header("Bullet Stats")]
    [SerializeField] GameObject bulletPrefab;     
    [SerializeField] float minSpeed  = 5f;
    [SerializeField] float maxSpeed  = 20f;
    [SerializeField] int   minDamage = 10;
    [SerializeField] int   maxDamage = 50;
    [SerializeField] float bulletLifeTime = 3f;

    [Header("Charge Settings")]
    [SerializeField] float maxChargeTime = 2f;
    [SerializeField] float startScale = .2f;
    [SerializeField] float maxScale   = 2f;
    [SerializeField] Transform firePoint;

    GameObject  chargingBullet;
    Coroutine   chargeRoutine;
    NetworkStatsController _stats;
    bool        isCharging;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();     
        enabled = HasAuthority;
        _stats  = GetComponent<NetworkStatsController>();
    }

    public void OnShootStarted()   
    {
        if (isCharging || _stats.CurrentAmmo.Value == 0) return;
        isCharging = true;
        chargeRoutine = StartCoroutine(ChargeCo());
    }

    public void OnShootCanceled()  
    {
        if (!isCharging) return;
        isCharging = false;
        if (chargeRoutine != null) StopCoroutine(chargeRoutine);
        float t = ReleaseCharge();                            
        ShootServerRpc(t, firePoint.position, Mathf.Sign(transform.localScale.x));
        int ammoCost = Mathf.RoundToInt(Mathf.Lerp(1, 5, t));
        _stats.SpendAmmoServerRpc(ammoCost);
    }

    IEnumerator ChargeCo()
    {
        float timer = 0f;
        chargingBullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        chargingBullet.transform.SetParent(firePoint);
        chargingBullet.transform.localPosition = Vector3.zero;
        chargingBullet.transform.localScale    = Vector3.one * startScale;

        while (isCharging && timer < maxChargeTime)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / maxChargeTime);
            chargingBullet.transform.localScale =
                Vector3.one * Mathf.Lerp(startScale, maxScale, t);
            yield return null;
        }
    }

    float ReleaseCharge()
    {
        Destroy(chargingBullet);
        chargingBullet = null;

        float scale = transform.localScale.x;   
        float finalScale = Mathf.Clamp(transform.localScale.magnitude, startScale, maxScale);
        return Mathf.InverseLerp(startScale, maxScale, finalScale);
    }

    [ServerRpc(RequireOwnership = true)]
    void ShootServerRpc(float tCharge, Vector3 spawnPos, float dirX,
                        ServerRpcParams _ = default)
    {
        var go         = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
        var netObj     = go.GetComponent<NetworkObject>();
        netObj.Spawn();

        int   dmg   = Mathf.RoundToInt(Mathf.Lerp(minDamage, maxDamage, tCharge));
        float speed = Mathf.Lerp(minSpeed,  maxSpeed,  tCharge);

        go.GetComponent<NetworkBulletController>()
          .ServerInit(dmg, new Vector2(dirX * speed, 0), bulletLifeTime);
    }
}
