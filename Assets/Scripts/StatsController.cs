using System.Collections;
using System.Runtime;
using UnityEngine;

public class StatsController : MonoBehaviour
{
    [Header("Health Stats")]
    public int currentHealth;
    public int maxHealth;

    [SerializeField] private Color hitColor;
    [SerializeField] private float hitTime;

    [Header("Ammo Stats")]
    public int currentAmmo;
    public int maxAmmo;
    public int ammoReloadAmount;
    public bool isDie = false;
    public Animator _animator;

    private SpriteRenderer _spriteRenderer;


    private void Start()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void FixedUpdate()
    {
        Die();
    }

    public void ReceiveDamage(int dmg)
    {
        currentHealth -= dmg;
        CinemachineCameraEffects.Instance.CameraMovement(5,1,0.5f);
        StartCoroutine(Hit());
    }

    private IEnumerator Hit()
    {
        _spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(hitTime);
        _spriteRenderer.color = Color.white;

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

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bubble"))
        {
            var bullet = other.GetComponent<BulletController>();
            ReceiveDamage(bullet.damage);
        }

        if(other.CompareTag("Fall"))
        {
            ReceiveDamage(100000000);
        }

        if (other.CompareTag("Ammo") && currentAmmo < maxAmmo)
        {
            Reload();
            Destroy(other.gameObject);
        }
    }

    private void Die()
    {
        if (currentHealth <= 0)
        {
            //TODO: WHEN THE PLAYER DIE
            Debug.Log(gameObject.name + ", Die");
            isDie = true;
            CinemachineCameraEffects.Instance.CameraMovement(10,1,1f);

            Destroy(gameObject);
        }
    }
}
