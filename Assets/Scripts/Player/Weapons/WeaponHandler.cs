using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private List<SO_Weapons> so_weapons;
    public GameObject bulletPrefab;
    public Transform firePoint; //TODO: Apply a offset according to sprite x limit
    public WeaponBase currentLogic;

    Vector3 fireLocalPos;

    public void LoadWeapon(int weaponId)
    {
        // search weapon by ID
        SO_Weapons weaponData = so_weapons.Find(w => w.weaponId == weaponId);

        if (weaponData == null)
        {
            Debug.LogWarning($"Weapon with ID '{weaponId}' not found.");
            return;
        }

        // delete past logic
        if (currentLogic != null)
            Destroy(currentLogic);

       
        spriteRenderer.sprite = weaponData.sprite;

        // add logic behaviour 
        currentLogic = gameObject.AddComponent(GetWeaponType(weaponData.weaponId)) as WeaponBase;
        currentLogic.SetReferences(weaponData, firePoint, bulletPrefab);
        fireLocalPos = firePoint.localPosition;
    }


    Type GetWeaponType(int id)
    {
        return id switch
        {
            0 => typeof(Weapon_Pistol),
            1 => typeof(Weapon_AK),
            _ => typeof(WeaponBase)
        };
    }

    public void SetDirection(float dir)
    {
        if (dir == 0) return;

        // flip visual
        transform.localScale = new Vector3(dir * Mathf.Abs(transform.localScale.x),
                                        transform.localScale.y,
                                        transform.localScale.z);

        if (dir < 0)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }

        // flip firepoint
            firePoint.localPosition = new Vector3(dir * Mathf.Abs(fireLocalPos.x),
                                            fireLocalPos.y,
                                            fireLocalPos.z);
    }


}
