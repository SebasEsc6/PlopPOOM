using System.Collections;
using System.Runtime;
using UnityEngine;

public class StatsController : MonoBehaviour
{
    [Header("Health Stats")]
    public int currentHealth;
    public int maxHealth;

    [SerializeField] private float timeToDie  =1f;

    [SerializeField] private Color hitColor;
    [SerializeField] private float hitTime;

    [Header("Ammo Stats")]
    public int currentAmmo;
    public int maxAmmo;
    public bool isDie = false;
    
    [Header("Power Ups Stats")]
    public int ammoReloadAmount;

    [Header("References")]
    public Animator _animator;
    private SpriteRenderer _spriteRenderer;
    [SerializeField] private EventController _eventController;
    [SerializeField] private MovementController _movementController;



    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void ReceiveDamage(int dmg)
    {
        if(currentHealth <= 0) return;
        currentHealth -= dmg;
        CinemachineCameraEffects.Instance.CameraMovement(5, 1, 0.5f);
        StartCoroutine(Die());
        // StartCoroutine(Hit());
        _animator.SetTrigger("Damage");
        
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

    private IEnumerator Die()
    {
        if (currentHealth <= 0)
        {
            _animator.SetBool("Defeat", true);
            CinemachineCameraEffects.Instance.CameraMovement(10,1,1f);
            _eventController.canControl = false;
            isDie = true;
            yield return new WaitForSeconds(timeToDie);
            
            Debug.Log(gameObject.name + ", Die");
            Destroy(gameObject);
        }
    }
}
