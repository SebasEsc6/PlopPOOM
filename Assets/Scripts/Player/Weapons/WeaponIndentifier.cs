using Unity.Netcode;
using UnityEngine;

public class WeaponIndentifier : PickableBase
{
    public SO_Weapons so_Weapons;
    [SerializeField] protected CollisionDispatcher dispatcher;



    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        spriteRenderer.sprite = so_Weapons.sprite;
        DespawnAfterTimeLife(so_Weapons.lifeTime);
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            dispatcher.ConfigureCollisionData(CollisionFlags.Weapon, (ushort)so_Weapons.weaponId);
            NetworkObject.Despawn();
        }
    }


}
