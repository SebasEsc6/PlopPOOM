using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class NetworkShootController : MonoBehaviour
{

    [SerializeField] protected Transform weaponHandler;

    [SerializeField] protected NetworkStatsController statsController;

    public WeaponBase currentWeapon;


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
        currentWeapon.BeginCharge();
    }

    public void ReleaseCharge()
    {
        currentWeapon.ReleaseCharge();
    }
}
