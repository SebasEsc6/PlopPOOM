using Unity.Netcode;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class NetworkBulletController : NetworkBehaviour
{
    public NetworkVariable<Vector2> scale = new(
        new Vector2(1f, 1f),
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private uint bulletId;
    private byte validationToken;
    private static uint bulletCounter = 0;

    private Vector3 initialPosition;
    private Vector2 initialScale;
    private float damageToDispatch = 5;

    public event System.Action<NetworkBulletController> OnBeforeReturnToPool;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        damageToDispatch = 5;
    }


    /// <summary>
    /// Initializes the bullet's logic and launches it with specific values.
    /// </summary>
    /// <param name="dmg">Damage value to apply on impact.</param>
    /// <param name="lifetime">Time after which it returns to the pool.</param>
    /// <param name="velocity">Initial velocity vector for the bullet.</param>
    public void Init(float dmg, float lifetime, Vector2 velocity)
    {
        if (HasAuthority)
        {
            scale.Value = initialScale;
            damageToDispatch = dmg;
            var rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = velocity;

            bulletId = bulletCounter++;
            validationToken = (byte)Random.Range(1, 255);
            TokenValidator.Register(bulletId, validationToken);

            if (!NetworkObject.IsSpawned) return;
            StartCoroutine(DespawnAfterDelay(lifetime));
        }
    }

    IEnumerator DespawnAfterDelay(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);

        if (!HasAuthority) yield break;
        {
            OnBeforeReturnToPool?.Invoke(this);

            ResetToPool();
            NetworkObject.Despawn();
        }
    }

    private void ResetToPool()
    {
        transform.position = initialPosition;
        transform.localScale = initialScale;
        damageToDispatch = 0;
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!HasAuthority) return;

        if (!NetworkObject.IsSpawned) return;
        OnBeforeReturnToPool?.Invoke(this);

        var dispatcher = GetComponent<CollisionDispatcher>();
        dispatcher.ConfigureCollisionData(CollisionFlags.Damage, (ushort)damageToDispatch); //? check if this way to send dmg is secure

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
