using System.Collections;
using System.Runtime;
using UnityEngine;

public class StatsController : MonoBehaviour
{
    [Header("Health Stats")]
    public int currentHealth;
    public int maxHealth;

    [SerializeField] private float timeToDie = 1f;

    [Header("Ammo Stats")]
    public int currentAmmo;
    public int maxAmmo;
    public bool isDie = false;
    
    [Header("Power Ups Stats")]
    public int ammoReloadAmount;

    [Header("References")]
    public Animator _animator;
    [SerializeField] private EventController _eventController;

    private bool _isDying;
    private Coroutine _deathRoutine;


    public void ReceiveDamage(int dmg)
    {
        if (_isDying) return;
        currentHealth = Mathf.Max(0, currentHealth - dmg);
        CinemachineCameraEffects.Instance.CameraMovement(5, 1, 0.5f);
        _animator.SetTrigger("Damage");

        if (currentHealth <= 0)
        {
            _isDying = true;
            _deathRoutine ??= StartCoroutine(DieOnce());
        }
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


    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Fall"))
        {
            ReceiveDamage(100000000);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bubble"))
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

    private IEnumerator DieOnce()
    {
        _animator.SetBool("Defeat", true);
        _eventController.canControl = false;
        isDie = true;
        yield return new WaitForSeconds(timeToDie);
        
        gameObject.SetActive(false);
    }
}
