using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

[DisallowMultipleComponent]
public class NetworkStatsController : NetworkBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] int maxHealth = 100;
    [HideInInspector] public int MaxHealth => maxHealth;

    [Header("Ammo")]
    [SerializeField] int maxAmmo = 20;
    [HideInInspector] public int MaxAmmo => maxAmmo;

    [SerializeField] float reloadTime = 2f;

    [Header("Movement")]
    public float speedMovement = 5f;
    public float jumpForce = 5f;

    [SerializeField] private PlayerController playerController;
    [SerializeField] private ClientAuthoritativeMovement movementController;
    [SerializeField] private Animator animator;

    public NetworkVariable<int> CurrentHealth = new(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public NetworkVariable<int> CurrentAmmo = new(
        20,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public static event Action<NetworkStatsController> OnStatsSpawned;
    public static event Action<NetworkStatsController> OnStatsDespawned;
    public event Action<int> OnHealthChanged;
    public event Action<int> OnAmmoChanged;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner)
        {
            CurrentHealth.Value = maxHealth;
            CurrentAmmo.Value = maxAmmo;
            OnStatsSpawned?.Invoke(this);
            CurrentHealth.OnValueChanged += (_, newVal) =>
            OnHealthChanged?.Invoke(newVal);
            CurrentAmmo.OnValueChanged += (_, newVal) =>
                OnAmmoChanged?.Invoke(newVal);
        }
    }

    public void SpendAmmo(int amount)
    {
        if (CurrentAmmo.Value < amount) return;
        CurrentAmmo.Value -= amount;
    }

    void FullReload() => CurrentAmmo.Value = maxAmmo;

    private void HealAmmount(int heal)
    {
        CurrentHealth.Value = Mathf.Min(CurrentHealth.Value + heal, maxHealth);
    }

    public bool CanPickAmmo()
    {
        return CurrentAmmo.Value < maxAmmo;
    }
    public bool CanPickHeal()
    {
        return CurrentHealth.Value < maxHealth;
    }

    public void TakeDamage(DamageData dmgData)
    {
        if (!IsOwner) return;

        // if (!TokenValidator.ValidateAndConsume(dmgData.bulletId, dmgData.validationToken))
        //     return;

        int oldHealth = CurrentHealth.Value;
        int newHealth = Mathf.Max(0, oldHealth - dmgData.amount);

        Debug.Log($"[Stats] Applying damage: {dmgData.amount} → HP {oldHealth} → {newHealth}");

        CurrentHealth.Value = Mathf.Max(0, CurrentHealth.Value - dmgData.amount);
        animator.SetTrigger("Damage"); //? sebas here damage animation
        //? sebas here damage
    }

    public void SwitchWeapon(int idWeapon)
    {
        playerController.HandleWeaponPickup(idWeapon);
    }

    public void ApplyItemEffect(int idItem)
    {
        if (!IsOwner) return;

        // if (!TokenValidator.ValidateAndConsume(dmgData.bulletId, dmgData.validationToken))
        //     return;

        switch (idItem)
        {
            case 0:
                Debug.Log("Give Ammo");
                FullReload();
                break;
            case 1:
                Debug.Log("Give Heal");
                SO_Item item = SORegistry.Get<SO_Item>(1);
                Debug.Log(item.itemName);
                HealAmmount(item.valueToIncrease);
                break;
            default:
                Debug.Log("Give itemBufffff");
                break;
        }
    }
    public void ActivatePowerUp(int powerUpId)
    {
        if (!IsOwner) return;

        // if (!TokenValidator.ValidateAndConsume(dmgData.bulletId, dmgData.validationToken))
        //     return;

        switch (powerUpId)
        {
            case 0:
                Debug.Log("Give speedboost");
                SO_PowerUps powerUp = SORegistry.Get<SO_PowerUps>(0);
                StartCoroutine(SpeedBoost(powerUp));
                break;
            case 1:
                Debug.Log("Give Other powerUp");
                break;
            default:
                Debug.Log("Give powerUp");
                break;
        }
    }

    private IEnumerator SpeedBoost(SO_PowerUps sO_PowerUp)
    {
        movementController.currentSpeed += sO_PowerUp.valueToIncrease;
        yield return new WaitForSeconds(sO_PowerUp.duration);
        movementController.currentSpeed = speedMovement;
    }

    void Die()
    {
        // GetComponent<ClientAuthoritativeMovement>()?.enabled = false;
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            OnStatsDespawned?.Invoke(this);
        }
        base.OnNetworkDespawn();
    }
}
