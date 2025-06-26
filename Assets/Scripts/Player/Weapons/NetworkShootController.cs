using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class NetworkShootController : MonoBehaviour
{

    [SerializeField] protected Transform weaponHandler;

    [SerializeField] protected NetworkStatsController statsController;

    public WeaponBase currentWeapon;
    public bool canShoot = false;


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
    }

    public void ReleaseCharge()
    {
        if (!canShoot) return;
        currentWeapon.ReleaseCharge();
    }
}
