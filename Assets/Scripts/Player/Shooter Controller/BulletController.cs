using System.Collections.Generic;
using UnityEngine;

public class BulletController : PooledBehaviour
{
    [Header("Stats")]
    public int damage = 10;
    [SerializeField] private float lifetime = 5f;

    [Header("FX")]
    [SerializeField] private GameObject particleBubbles;
    private BulletParticleVFX _childVfx;

    [Header("Physics")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D col;

    // Lifetime
    private float _timer;

    // Collision ignores we applied to the owner's colliders
    private readonly List<Collider2D> _ignoredOwnerColliders = new();

    private void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!col) col = GetComponent<Collider2D>();
        if (particleBubbles)
        {
            _childVfx = particleBubbles.GetComponent<BulletParticleVFX>();
            if (_childVfx == null) _childVfx = particleBubbles.AddComponent<BulletParticleVFX>();
        }
    }

    public override void OnSpawnedFromPool()
    {
        _timer = lifetime;

        if (rb)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        if (_childVfx)
        {
            if (particleBubbles.transform.parent != transform)
                particleBubbles.transform.SetParent(transform, false);

            particleBubbles.SetActive(false);
            var ps = particleBubbles.GetComponent<ParticleSystem>();
            if (ps) { ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); ps.Clear(true); }
        }
    }

    public override void OnDespawnedToPool()
    {
        // Revert owner collision ignores
        if (col && _ignoredOwnerColliders.Count > 0)
        {
            foreach (var oc in _ignoredOwnerColliders)
            {
                if (oc) Physics2D.IgnoreCollision(col, oc, false);
            }
        }
        _ignoredOwnerColliders.Clear();

        if (rb) rb.linearVelocity = Vector2.zero;
        _timer = 0f;
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
            ReturnToPool();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Play bubble FX detached from bullet
        if (_childVfx)
            _childVfx.PlayDetached(transform, transform.position);

        // Apply damage if the target has StatsController
        var stats = other.GetComponentInParent<StatsController>();
        if (stats != null)
        {
            stats.ReceiveDamage(damage, this);
        }

        // Return bullet to pool
        ReturnToPool();
    }
}
