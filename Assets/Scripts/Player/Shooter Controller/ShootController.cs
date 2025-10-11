using UnityEngine;
using System.Collections;

public class ShootController : MonoBehaviour, IPoolable
{
    [Header("Bullet Stats")]
    [SerializeField] private GameObject bulletPrefab;    
    [SerializeField] private Transform firePoint;

    [Header("Charge Settings")]
    [SerializeField] private float maxChargeTime = 2f;
    [SerializeField] private float startScale = 0.2f;
    [SerializeField] private float maxScale = 2f;
    [SerializeField] private float minSpeed = 5f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float minDamage = 10f;
    [SerializeField] private float maxDamage = 50f;

    // Internal references
    private GameObject chargingBullet;
    private Rigidbody2D chargingBulletRb;
    private Coroutine chargingCoroutine;
    private BulletController _bulletController;
    private StatsController _statsController;
    public bool isCharging;
    [SerializeField] private GameObject chargingAudio;

    void Start()
    {
        _statsController = GetComponent<StatsController>();
    }

    public void OnSpawnedFromPool()
    {
        if (firePoint && firePoint.parent)
            firePoint.parent.gameObject.SetActive(true);

        // clear charge state
        if (chargingCoroutine != null) { StopCoroutine(chargingCoroutine); chargingCoroutine = null; }
        chargingBullet = null;
        chargingBulletRb = null;
    }

    // Called by the pool
    public void OnDespawnedToPool()
    {
        if (chargingCoroutine != null) { StopCoroutine(chargingCoroutine); chargingCoroutine = null; }
        if (chargingBullet)
        {
            PoolManager.TryDespawn(chargingBullet);
            chargingBullet = null;
            chargingBulletRb = null;
        }
        isCharging = false;
    }

    private bool CheckAmmo()
    {
        if (_statsController.currentAmmo > 0)
        {
            return true;
        }
        else
        {
            Debug.Log("Need Ammo!!");
            return false;
        }
    }

    public void BeginCharge()
    {
        if (!CheckAmmo()) return;
        
        if (isCharging) return; // Avoid double-charging

        isCharging = true;
        StartCoroutine(TurnOffAudio());
        // Debug.Log("start charge");

        // Instantiate the bullet at the fire point with initial scale
        chargingBullet = PoolManager.Instance.Spawn(bulletPrefab, firePoint.position, Quaternion.identity, firePoint);
        chargingBullet.transform.localPosition = Vector3.zero;
        chargingBullet.transform.localRotation = Quaternion.identity;
        chargingBullet.transform.localScale = Vector3.one * startScale;
        _bulletController = chargingBullet.GetComponent<BulletController>();
        chargingBulletRb = chargingBullet.GetComponent<Rigidbody2D>();

        if (chargingBulletRb != null)
        {
            // Temporarily no velocity while charging
            chargingBulletRb.linearVelocity = Vector2.zero;
            chargingBulletRb.bodyType = RigidbodyType2D.Kinematic; 
            // isKinematic = true ensures it won't react to physics while charging (if you want).
        }

        // Start coroutine to grow bullet over time
        chargingCoroutine = StartCoroutine(ChargeBulletRoutine());
    }

    public void ReleaseCharge()
    {
        if (!isCharging) return;  // If we're not actually charging, ignore

        isCharging = false;

        // Stop the coroutine so it doesn't keep scaling
        if (chargingCoroutine != null)
        {
            StopCoroutine(chargingCoroutine);
        }

        // "Launch" the bullet
        LaunchChargedBullet();

        // Cleanup references
        chargingBullet = null;
        chargingBulletRb = null;
        chargingCoroutine = null;
        _statsController._animator.SetTrigger("Shoot");
    }

    private IEnumerator ChargeBulletRoutine()
    {
        float timer = 0f;

        // While isCharging is true, keep scaling up
        while (isCharging && timer < maxChargeTime)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / maxChargeTime);
            // Debug.Log("charging time: "+t);

            // Scale bullet
            float currentScale = Mathf.Lerp(startScale, maxScale, t);
            if (chargingBullet != null)
            {
                chargingBullet.transform.localScale = Vector3.one * currentScale;
            }

            yield return null; // wait until the next frame
        }

        // If we exit the loop because timer >= maxChargeTime, we clamp the final scale
        if (chargingBullet != null)
        {
            chargingBullet.transform.localScale = Vector3.one * maxScale;
        }
    }

    private void LaunchChargedBullet()
    {
        if (chargingBullet == null) return;

        float finalScale = chargingBullet.transform.localScale.x;
        float t = Mathf.InverseLerp(startScale, maxScale, finalScale);

        float finalSpeed = Mathf.Lerp(minSpeed, maxSpeed, t);
        float finalDamage = Mathf.Lerp(minDamage, maxDamage, t);
        _bulletController.damage = Mathf.RoundToInt(finalDamage);

        int ammoCost = Mathf.RoundToInt(Mathf.Lerp(1, 5, t));
        _statsController.SpendAmmo(ammoCost);

        if (chargingBulletRb != null)
        {
            chargingBullet.transform.SetParent(null, worldPositionStays: true);
            chargingBulletRb.linearVelocity = new Vector2(transform.localScale.x * finalSpeed, 0f);
        }
    }

    IEnumerator TurnOffAudio()
    {
        chargingAudio.SetActive(true);
        yield return new WaitForSeconds(.5f);
        chargingAudio.SetActive(false);
    }
}
