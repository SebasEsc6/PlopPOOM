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
        NetworkVariableWritePermission.Server);

    public NetworkVariable<int> CurrentAmmo = new(
        20,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

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

    public void TakeDamage(DamageData dmgData)
    {
        if (!IsServer) return;
        TakeDamageServerRpc(dmgData.amount);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int dmg)
    {
        CurrentHealth.Value = Mathf.Max(0, CurrentHealth.Value - dmg);
        if (CurrentHealth.Value == 0)
            Die();
    }

    void FullReload() => CurrentAmmo.Value = maxAmmo;

    void Die()
    {
        // notificar a todos para FX/muerte
        PlayDeathFxClientRpc();
        // Aquí puedes agregar lógica adicional como despawn o desactivación
        // GetComponent<ClientAuthoritativeMovement>()?.enabled = false;
    }

    [ClientRpc]
    void PlayDeathFxClientRpc()
    {
        // aquí podrías reproducir partículas, sonido, etc.
        Debug.Log("Reproducir FX de muerte aquí");
    }
}
