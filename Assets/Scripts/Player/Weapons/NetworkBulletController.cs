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

    public int GetDamage() => damage.Value;
    public uint BulletId => bulletId;
    public byte Token => validationToken;

    private Vector3 initialPosition;
    private Vector3 initialScale;

    readonly NetworkVariable<float> startScale = new(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        transform.localScale = Vector3.one * startScale.Value;
    }

    public void SetStartScale(float scale)
    {
        if (IsOwner)
            startScale.Value = scale;
    }

    public void SetDefaultValues()
    {
        transform.position = initialPosition;
        transform.localScale = initialScale;
        GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
    }


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
        if (IsOwner && damage.Value != dmg)
            damage.Value = dmg;

        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = velocity;

        bulletId = bulletCounter++;
        validationToken = (byte)Random.Range(1, 255);

        TokenValidator.Register(bulletId, validationToken);

        StartCoroutine(DespawnAfterDelay(lifetime));
    }

    public void DeactivateCollisionOnStart(GameObject shooter)
    {
        if (shooter.TryGetComponent(out Collider2D ownerCol) &&
            TryGetComponent(out Collider2D bulletCol))
        {
            Physics2D.IgnoreCollision(ownerCol, bulletCol);
        }
    }


    IEnumerator DespawnAfterDelay(float lifetime)
    {
        yield return new WaitForSeconds(lifetime);
        SetDefaultValues();
        NetworkObject.Despawn();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!IsOwner) return;
        NetworkObject.Despawn();
        SetDefaultValues();
    }
}
