using Unity.Netcode;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class NetworkBulletController : NetworkBehaviour
{
    readonly NetworkVariable<int> damage = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private uint bulletId;
    private byte validationToken;

    private static uint bulletCounter = 0;

    /// <summary>
    /// Initializes the bullet's logic and launches it with specific values.
    /// </summary>
    /// <param name="shooter">The player GameObject that fired the bullet.</param>
    /// <param name="bulletPool">Reference to the pool to return to later.</param>
    /// <param name="dmg">Damage value to apply on impact.</param>
    /// <param name="lifetime">Time after which it returns to the pool.</param>
    /// <param name="velocity">Initial velocity vector for the bullet.</param>
    public void Init(GameObject shooter, int dmg, float lifetime, Vector2 velocity)
    {
        if (IsOwner && damage.Value != dmg)
            damage.Value = dmg;

        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = velocity;

        bulletId = bulletCounter++;
        validationToken = (byte)Random.Range(1, 255); // ⚠️ evitar 0 como token

        TokenValidator.Register(bulletId, validationToken);

        if (shooter.TryGetComponent(out Collider2D ownerCol) &&
            TryGetComponent(out Collider2D bulletCol))
        {
            Physics2D.IgnoreCollision(ownerCol, bulletCol);
        }

        StartCoroutine(DespawnAfterDelay(lifetime));
    }


    IEnumerator DespawnAfterDelay(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        NetworkObject.Despawn();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log($"[Bullet] Triggered with: {col.name}");

        if (col.TryGetComponent<NetworkObject>(out var netObj))
        {
            if (netObj.TryGetComponent(out IDamageable damageable))
            {
                var data = new DamageData
                {
                    amount = damage.Value,
                    attackerId = OwnerClientId,
                    hitPoint = transform.position,
                    timeSent = NetworkManager.ServerTime.Time,
                    bulletId = bulletId,
                    validationToken = validationToken
                };
                Debug.Log($"Calling TakeDamage in the {netObj}");

                damageable.TakeDamage(data);
            }
        }

        NetworkObject.Despawn();
    }

    [ServerRpc(RequireOwnership = true)]
    void ApplyDamageServerRpc(ulong targetId, Vector3 hitPoint)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(targetId, out var target)) return;

        if (target.TryGetComponent(out IDamageable damageable))
        {
            Debug.Log($"[Bullet] Found IDamageable in target {targetId}");

            var data = new DamageData
            {
                amount = damage.Value,
                attackerId = OwnerClientId,
                hitPoint = hitPoint,
                timeSent = NetworkManager.ServerTime.Time,
                bulletId = bulletId,
                validationToken = validationToken
            };

            damageable.TakeDamage(data);
        }
        else
        {
            Debug.LogWarning($"[Bullet] Target {targetId} has no IDamageable");
        }
    }
}
