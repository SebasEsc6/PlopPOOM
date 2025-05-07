using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class NetworkBulletController : NetworkBehaviour
{
    readonly NetworkVariable<int> damage = new(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);   

    public void SetDamage(int dmg)    
    {
        if (IsOwner) damage.Value = dmg;
    }

    public void ScheduleDespawn(float t)
    {
        if (IsOwner) Invoke(nameof(ServerDespawn), t);
    }

    void ServerDespawn() => GetComponent<NetworkObject>().Despawn();

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!IsServer) return;                     
        if (col.TryGetComponent(out NetworkStatsController stats))
            stats.TakeDamageServerRpc(damage.Value);
        ServerDespawn();
    }
}
