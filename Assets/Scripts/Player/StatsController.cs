using UnityEngine;
using System.Collections;

public class StatsController : PooledBehaviour
{
    [Header("Stats")]
    public int maxHealth = 3;
    public int currentHealth;
    public int maxAmmo = 10;
    public int currentAmmo;
    public int ammoReloadAmount = 20; // amount of ammo reloaded per reload action

    [Header("State Flags")]
    public bool isDie;            // true while dead
    private bool _invulnerable;   // optional spawn grace

    [Header("Refs")]
    public Animator _animator;
    public Rigidbody2D rb;
    public Collider2D[] hitColliders;


    // --- Pool lifecycle ----------------------------------------------------

    public override void OnSpawnedFromPool()
    {
        Debug.Log($"[{name}] OnSpawnedFromPool");

        // Reset core stats
        currentHealth = maxHealth;
        currentAmmo = maxAmmo;
        isDie = false;

        if (_animator)
        {
            _animator.Rebind();
            _animator.Update(0f);

            _animator.ResetTrigger( "Jump");
            _animator.ResetTrigger("Shoot");
            _animator.ResetTrigger("Damage");
            _animator.SetBool("Defeat", false);
            _animator.SetBool("Dissapear", false);

            _animator.SetFloat("MoveSpeed", 0f);
        }

        // Reset physics
        if (rb) { rb.linearVelocity = Vector2.zero; rb.angularVelocity = 0f; }
        if (hitColliders != null)
            foreach (var c in hitColliders) if (c) c.enabled = true;

        StartCoroutine(SpawnGrace(0.25f));
    }

    public override void OnDespawnedToPool()
    {
        Debug.Log($"[{name}] OnDespawnedToPool");

        // Stop all local coroutines/effects that could leak into next life
        StopAllCoroutines();
        _invulnerable = false;

        // Ensure colliders are enabled for next spawn
        if (hitColliders != null)
            foreach (var c in hitColliders) if (c) c.enabled = true;

        // Clear rigidbody motion
        if (rb) { rb.linearVelocity = Vector2.zero; rb.angularVelocity = 0f; }

        // Reset core stats
        currentHealth = maxHealth;
        currentAmmo = maxAmmo;
        isDie = false;
    }

    private IEnumerator SpawnGrace(float seconds)
    {
        _invulnerable = true;
        yield return new WaitForSeconds(seconds);
        _invulnerable = false;
    }

    // --- Damage / Death (single-flight safe) --------------------------------

    public void ReceiveDamage(int amount, Component source = null)
    {
        if (isDie || _invulnerable) return;

        Debug.Log($"[{name}] Damage {amount} from {source?.name ?? "Unknown"}");

        currentHealth -= Mathf.Max(1, amount);
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            DieOnce();
        }
        else
        {
            if (_animator) _animator.SetTrigger("Damage");
        }
    }

    public void DieOnce()
    {
        StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine()
    {
        isDie = true;

        Debug.Log($"[{name}] Died at {Time.time}");

        // disable colliders to avoid further hits while we play death anim
        if (hitColliders != null)
            foreach (var c in hitColliders) if (c) c.enabled = false;

        if (_animator)
        {
            _animator.SetBool("Defeat", true);
            yield return new WaitForSeconds(1f);
        }
        else
        {
            yield return null;
        }

        // Return avatar to pool (Root stays alive)
        PoolManager.TryDespawn(gameObject);
    }

    // Utility used by HUD or other systems if needed
    public void SpendAmmo(int amount)
    {
        currentAmmo = Mathf.Max(0, currentAmmo - amount);
    }

    public void Reload()
    {
        int bulletsNeeded = maxAmmo - currentAmmo;
        int bulletsToReload = Mathf.Min(bulletsNeeded, ammoReloadAmount);
        currentAmmo += bulletsToReload;
    }
}
