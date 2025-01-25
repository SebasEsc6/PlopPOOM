using System.Runtime;
using UnityEngine;

public class StatsController : MonoBehaviour
{
    [Header("Health Stats")]
    public int currentHealth;
    public int maxHealth;

    [Header("Ammo Stats")]
    public int currentAmmo;
    public int maxAmmo;
    public int ammoReloadAmount;

    private void FixedUpdate() {
        Die();
    }

    public void ReceiveDamage(int dmg)
    {
        currentHealth -= dmg;
    }

    public void SpendAmmo(int spendedAmmo)
    {
        currentAmmo -= spendedAmmo;
    }

    public void Reload()
    {
        int bulletsNeeded = maxAmmo - currentAmmo;
        int bulletsToReload = Mathf.Min(bulletsNeeded, ammoReloadAmount);
        currentAmmo += bulletsToReload;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Bubble"))
        {
            var bullet = other.GetComponent<BulletController>();
            ReceiveDamage(bullet.damage);
        }

        if (other.CompareTag("Ammo") && currentAmmo < maxAmmo)
        {
            Reload();
            Destroy(other.gameObject);
        }
    }

    private void Die()
    {
        if (currentHealth <=0)
        {
            //TODO: WHEN THE PLAYER DIE
            Debug.Log(gameObject.name + ", Die");
        }
    }





}
