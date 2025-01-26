using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootController : MonoBehaviour
{
    [Header("Bullet Stats")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float speedBullet;
    
    [SerializeField] private Transform firePoint;

    private GameObject bulletParent;

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
    [SerializeField] private float bulletLifeTime;
    [SerializeField] private GameObject chargingAudio;

    void Start()
    {
        bulletParent = new GameObject();
        bulletParent.name = "Bullet Parent";

        _statsController = GetComponent<StatsController>();
    }
    private void FixedUpdate() {
        if(_statsController.isDie)
        {
            Destroy(bulletParent);
        }
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
        chargingBullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity, firePoint);
        chargingBullet.transform.localScale = Vector3.one * startScale;
        _bulletController = chargingBullet.GetComponent<BulletController>();

        // Get its Rigidbody2D (optional if you need it for velocity)
        chargingBulletRb = chargingBullet.GetComponent<Rigidbody2D>();
        if (chargingBulletRb != null)
        {
            // Temporarily no velocity while charging
            chargingBulletRb.velocity = Vector2.zero;
            chargingBulletRb.isKinematic = true; 
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

        // Get final scale to determine how "charged" it was
        float finalScale = chargingBullet.transform.localScale.x;
        // Convert that scale to a "t" factor between 0 and 1
        // since we know it goes from startScale to maxScale
        float t = Mathf.InverseLerp(startScale, maxScale, finalScale);

        // Calculate final speed and damage
        float finalSpeed = Mathf.Lerp(minSpeed, maxSpeed, t);
        float finalDamage = Mathf.Lerp(minDamage, maxDamage, t);

        int dagamageInt = Mathf.RoundToInt(finalDamage);
        _bulletController.damage = dagamageInt;

        int ammoCost = Mathf.RoundToInt(Mathf.Lerp(1, 5, t));
        // Decrease current ammo by the calculated cost
        _statsController.SpendAmmo(ammoCost);

        // If we have a rigidbody, remove isKinematic and apply velocity
        if (chargingBulletRb != null)
        {
            chargingBulletRb.isKinematic = false;

            // Launch to the right or left depending on player's facing direction
            chargingBulletRb.velocity = new Vector2(transform.localScale.x * finalSpeed, 0f);
            chargingBulletRb.gameObject.transform.parent = bulletParent.transform;
        }

        Destroy(chargingBullet, bulletLifeTime);
    }

    IEnumerator TurnOffAudio()
    {
        chargingAudio.SetActive(true);
        yield return new WaitForSeconds(.5f);
        chargingAudio.SetActive(false);
    }
}
