using Unity.Netcode;
using UnityEngine;

public class NetworkBulletController : NetworkBehaviour
{
    [SerializeField] ParticleSystem hitFx;

    readonly NetworkVariable<int> damage = new(
        0, NetworkVariableReadPermission.Everyone,
           NetworkVariableWritePermission.Server);

    Rigidbody2D rb;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    public void ServerInit(int dmg, Vector2 velocity, float life)
    {
        if (!IsServer) return;
        damage.Value = dmg;
        rb.linearVelocity  = velocity;
        Invoke(nameof(Despawn), life);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent(out NetworkStatsController stats))
                stats.TakeDamageServerRpc(damage.Value);

        PlayHitFxClientRpc(transform.position);
        Despawn();
    }

    [ClientRpc]
    void PlayHitFxClientRpc(Vector3 pos)
    {
        if (hitFx == null) return;
        var fx = Instantiate(hitFx, pos, Quaternion.identity);
        fx.Play();
        Destroy(fx.gameObject, fx.main.duration);
    }

    void Despawn()
    {
        if (IsServer) GetComponent<NetworkObject>().Despawn();
    }
}
