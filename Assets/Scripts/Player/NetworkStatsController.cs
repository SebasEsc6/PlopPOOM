using Unity.Netcode;
using UnityEngine;

[DisallowMultipleComponent]
public class NetworkStatsController : NetworkBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] int maxHealth = 100;

    [Header("Ammo")]
    [SerializeField] int maxAmmo = 20;
    [SerializeField] float reloadTime = 2f;

    public NetworkVariable<int> CurrentHealth = new(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public NetworkVariable<int> CurrentAmmo = new(
        20,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            CurrentHealth.Value = maxHealth;
            CurrentAmmo.Value = maxAmmo;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpendAmmoServerRpc(int amount)
    {
        if (CurrentAmmo.Value < amount) return;
        CurrentAmmo.Value -= amount;
        if (CurrentAmmo.Value == 0)
            Invoke(nameof(FullReload), reloadTime);
    }

    void FullReload() => CurrentAmmo.Value = maxAmmo;

    public void TakeDamage(DamageData dmgData)
    {
        Debug.Log($"[Stats] IsOwner: {IsOwner}");
        Debug.Log($"[Stats] LocalClientId={NetworkManager.Singleton.LocalClientId}, OwnerClientId={OwnerClientId}");

        Debug.Log($"[Stats] TakeDamage invoked on client {NetworkManager.Singleton.LocalClientId}: " +
                  $"New damage = {dmgData.amount} from {dmgData.attackerId}");

        if (!IsOwner) return;

        // if (!TokenValidator.ValidateAndConsume(dmgData.bulletId, dmgData.validationToken))
        //     return;

        int oldHealth = CurrentHealth.Value;
        int newHealth = Mathf.Max(0, oldHealth - dmgData.amount);

        Debug.Log($"[Stats] Applying damage: {dmgData.amount} → HP {oldHealth} → {newHealth}");

        CurrentHealth.Value = Mathf.Max(0, CurrentHealth.Value - dmgData.amount);
    }

    void Die()
    {
        // notificar a todos para FX/muerte
        // Aquí puedes agregar lógica adicional como despawn o desactivación
        // GetComponent<ClientAuthoritativeMovement>()?.enabled = false;
    }
}
