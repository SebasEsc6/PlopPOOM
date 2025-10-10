using System.Collections.Generic;
using UnityEngine;

public class BulletController : PooledBehaviour
{
    [Header("Stats")]
    public int damage = 10;
    [SerializeField] private float lifetime = 5f;

    [Header("FX")]
    [SerializeField] private GameObject particleBubbles;
    [SerializeField] private float timeParticle = 0.5f;

    [Header("Physics")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D col;

    // Lifetime
    private float _timer;

    // Ownership / team
    private int _teamId = -1;
    private GameObject _ownerGO;

    // Collision ignores we applied to the owner's colliders
    private readonly List<Collider2D> _ignoredOwnerColliders = new();

    private void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!col) col = GetComponent<Collider2D>();
    }

    public override void OnSpawnedFromPool()
    {
        _timer = lifetime;
        _teamId = -1;
        _ownerGO = null;

        if (rb)
        {
            rb.linearVelocity = Vector2.zero;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
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

        _teamId = -1;
        _ownerGO = null;

        if (rb) rb.linearVelocity = Vector2.zero;
        _timer = 0f;
    }

    private void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0f)
            ReturnToPool();
    }

    /// <summary>
    /// Arms the bullet with owner/team and initial direction/speed.
    /// Shooter must call this immediately after spawning the bullet.
    /// </summary>
    public void Arm(GameObject owner, int teamId, Vector2 direction, float speed)
    {
        _ownerGO = owner;
        _teamId = teamId;

        // Initial velocity
        if (rb)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = direction.normalized * speed;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        // Ignore owner's colliders
        if (col && _ownerGO)
        {
            var ownerCols = _ownerGO.GetComponentsInChildren<Collider2D>(includeInactive: false);
            foreach (var oc in ownerCols)
            {
                if (!oc) continue;
                Physics2D.IgnoreCollision(col, oc, true);
                _ignoredOwnerColliders.Add(oc);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Apply damage if the target has StatsController
        var stats = other.GetComponentInParent<StatsController>();
        if (stats != null)
            stats.ReceiveDamage(damage);

        // Play bubble FX detached from bullet
        if (particleBubbles)
        {
            particleBubbles.SetActive(true);
            particleBubbles.transform.SetParent(null);

            var s = particleBubbles.transform.localScale;
            s.x = Mathf.Abs(s.x); s.y = Mathf.Abs(s.y); s.z = Mathf.Abs(s.z);
            particleBubbles.transform.localScale = s;

            Object.Destroy(particleBubbles, timeParticle);
        }

        // Return bullet to pool
        ReturnToPool();
    }
}
