using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class NetworkShootController : MonoBehaviour
{

    [SerializeField] protected Transform weaponHandler;
    [SerializeField] protected NetworkStatsController statsController;

    public WeaponBase currentWeapon;
    public bool canShoot = false;
    [SerializeField] private Animator animator;
    public void SetCurrentWeapon()
    {
        currentWeapon = weaponHandler.GetComponent<WeaponHandler>().currentLogic;
        if (currentWeapon != null)
        {
            currentWeapon.statsController = statsController;
        }
    }

    public void BeginCharge()
    {
        if (!canShoot) return;
        currentWeapon.BeginCharge();
        statsController.SpendAmmo(statsController.minAmmoPrice);
    }

    public void ReleaseCharge()
    {
        if (!canShoot) return;
        currentWeapon.ReleaseCharge();
        animator.SetTrigger("Shoot");
    }
}
