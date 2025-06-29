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

    [HideInInspector]
    public bool isAlive = true;

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

    [Header("Score Stats")]
    public NetworkVariable<int> CurrentHealth = new(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> CurrentAmmo = new(
        20,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> Lives = new(
        3, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> Kills = new(
        0, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);


    public static event Action<NetworkStatsController> OnStatsSpawned;
    public static event Action<NetworkStatsController> OnStatsDespawned;
    public event Action<int> OnHealthChanged;
    public event Action<int> OnAmmoChanged;
    public event Action<int> OnLivesChanged;
    public event Action<int> OnKillsChanged;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        CurrentHealth.OnValueChanged += (_, newVal) => OnHealthChanged?.Invoke(newVal);
        CurrentAmmo.OnValueChanged += (_, newVal) => OnAmmoChanged?.Invoke(newVal);
        Lives.OnValueChanged += (_, newVal) => OnLivesChanged?.Invoke(newVal);
        Kills.OnValueChanged += (_, newVal) => OnKillsChanged?.Invoke(newVal);

        OnStatsSpawned?.Invoke(this);

        if (IsOwner)
        {
            CurrentHealth.Value = maxHealth;
            CurrentAmmo.Value = maxAmmo;
            Lives.Value = 3;
            Kills.Value = 0;
        }

        isAlive = true;
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
        if (!IsOwner || !isAlive) return;

        int oldHealth = CurrentHealth.Value;
        int newHealth = Mathf.Max(0, oldHealth - dmgData.amount);

        Debug.Log($"[Stats] Applying damage: {dmgData.amount} → HP {oldHealth} → {newHealth}");

        CurrentHealth.Value = newHealth;
        animator.SetTrigger("Damage");

        if (newHealth <= 0)
        {
            Die(3, false, dmgData.attackerId);
        }
    }

    public void SwitchWeapon(int idWeapon)
    {
        playerController.HandleWeaponPickup(idWeapon);
    }

    public void ApplyItemEffect(int idItem)
    {
        if (!IsOwner) return;
        if (!isAlive) return;

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
        if (!isAlive) return;

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

    void Update()
    {
        if (transform.position.y <= playerController.gameLoopManager.deadHeight && isAlive)
            Die(1, true);
    }

    public void Die(float timeToDie, bool isFalled, ulong attackerId = 0)
    {
        if (!isAlive) return;

        animator.SetBool("Defeat", true);
        playerController.SetFlags(false);
        isAlive = false;

        Lives.Value--;
        if (!isFalled)
        {
            ReportKillServerRpc(attackerId);
        }

        if (Lives.Value > 0)
            StartCoroutine(HandleRespawn(timeToDie));
        else
            Debug.Log($"[Stats] Player {OwnerClientId} ran out of lives.");
    }

    public void Respawn(Vector3 respawnPosition)
    {
        if (!IsOwner) return;

        isAlive = true;
        CurrentHealth.Value = maxHealth;
        CurrentAmmo.Value = maxAmmo;

        playerController.SetFlags(true);
        playerController.weaponHandler.LoadWeapon(0);
        playerController.shootController.SetCurrentWeapon();
        transform.position = respawnPosition;

        animator.SetBool("Defeat", false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ReportKillServerRpc(ulong attackerId, ServerRpcParams rpc = default)
    {
        var attackerCtrl = GameManager.Instance.gameLoopManager
                             .statsControllers
                             .Find(c => c.OwnerClientId == attackerId);

        if (attackerCtrl == null)
        {
            Debug.LogWarning($"[ReportKill] No encontré controller para attackerId={attackerId}");
            return;
        }

        Debug.Log($"[ReportKill] Disparando IncrementKillClientRpc SOBRE controller.Owner={attackerCtrl.OwnerClientId}");

        attackerCtrl.IncrementKillClientRpc(new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { attackerId }
            }
        });
    }

    [ClientRpc]
    public void IncrementKillClientRpc(ClientRpcParams rpc = default)
    {
        Debug.Log($"[IncrementKillClientRpc] Recibido en controller.Owner={OwnerClientId}, IsOwner={IsOwner}");
        if (!IsOwner)
        {
            Debug.LogWarning("[IncrementKillClientRpc] No soy el owner de este controller, no hago nada.");
            return;
        }
        Kills.Value++;
        Debug.Log($"[IncrementKillClientRpc] Nuevo kills={Kills.Value}");
    }

    IEnumerator HandleRespawn(float timeToRespawn)
    {
        yield return new WaitForSeconds(timeToRespawn);
        Vector3 spawnPos = playerController.gameLoopManager.GetRandomSpawnPosition();
        Respawn(spawnPos);
    }

    public override void OnNetworkDespawn()
    {
        OnStatsDespawned?.Invoke(this);
        base.OnNetworkDespawn();
    }
}
