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


    /// <summary>
    /// Initializes the bullet's logic and launches it with specific values.
    /// </summary>
    /// <param name="bulletPool">Reference to the pool to return to later.</param>
    /// <param name="dmg">Damage value to apply on impact.</param>
    /// <param name="lifetime">Time after which it returns to the pool.</param>
    /// <param name="velocity">Initial velocity vector for the bullet.</param>
    public void Init(int dmg, float lifetime, Vector2 velocity)
    {
        if (IsOwner && damage.Value != dmg)
            damage.Value = dmg;

        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = velocity;

        StartCoroutine(DespawnAfterDelay(lifetime));
    }


    IEnumerator DespawnAfterDelay(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        NetworkObject.Despawn();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!IsServer) return;

        if (col.TryGetComponent(out NetworkStatsController stats))
            stats.TakeDamageServerRpc(damage.Value);

        NetworkObject.Despawn();
    }
}
