using Unity.Netcode;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class NetworkBulletController : NetworkBehaviour
{
    public NetworkVariable<int> damage = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private uint bulletId;
    private byte validationToken;
    private static uint bulletCounter = 0;

    private Vector3 initialPosition;
    private Vector2 initialScale;

    [SerializeField] private float lifeTime = 5f;

    public int GetDamage() => damage.Value;
    public uint BulletId => bulletId;
    public byte Token => validationToken;
    private Coroutine despawnRoutine;

    /// <summary>
    /// Initializes the bullet's logic and launches it with specific values.
    /// </summary>
    /// <param name="shooter">The player GameObject that fired the bullet.</param>
    /// <param name="bulletPool">Reference to the pool to return to later.</param>
    /// <param name="dmg">Damage value to apply on impact.</param>
    /// <param name="lifetime">Time after which it returns to the pool.</param>
    /// <param name="velocity">Initial velocity vector for the bullet.</param>
    public void Init(int dmg, float lifetime, Vector2 velocity)
    {
        if (HasAuthority)
        {
            damage.Value = dmg;
            transform.localScale = initialScale;
            var rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = velocity;

            bulletId = bulletCounter++;
            validationToken = (byte)Random.Range(1, 255);
            TokenValidator.Register(bulletId, validationToken);

            despawnRoutine = StartCoroutine(DespawnAfterDelay(lifetime));
        }
    }

    IEnumerator DespawnAfterDelay(float lifetime)
    {
        yield return new WaitForSeconds(lifeTime);

        if (HasAuthority)
        {
            ResetToPool();
            NetworkObject.Despawn();
        }
    }

    private void ResetToPool()
    {
        transform.position = initialPosition;
        transform.localScale = initialScale;
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!HasAuthority) return;

        if (despawnRoutine != null)
        {
            StopCoroutine(despawnRoutine);
            despawnRoutine = null;
        }
        TryDespawn();
    }

    private void TryDespawn()
    {
        if (!NetworkObject.IsSpawned) return;
        ResetToPool();
        NetworkObject.Despawn();
    }

    public void SetStartScale(float scale)
    {
        if (IsOwner)
            initialScale = new Vector2(scale, scale);
    }

    public void DeactivateCollisionOnStart(GameObject shooter)
    {
        if (shooter.TryGetComponent(out Collider2D ownerCol) &&
            TryGetComponent(out Collider2D bulletCol))
        {
            Physics2D.IgnoreCollision(ownerCol, bulletCol);
        }
    }
}
