using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class NetworkShootController : MonoBehaviour
{

    [SerializeField] protected Transform weaponHandler;

    [SerializeField] protected NetworkStatsController statsController;

    public WeaponBase currentWeapon;

    void Start()
    {
        currentWeapon = weaponHandler.GetComponentInChildren<WeaponBase>();
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
