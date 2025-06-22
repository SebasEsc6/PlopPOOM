using Unity.Netcode;
using UnityEngine;

public class WeaponIndentifier : PickableBase
{
    public SO_Weapons sO_Weapons;



    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        spriteRenderer.sprite = sO_Weapons.sprite;
        DespawnAfterTimeLife(sO_Weapons.lifeTime);
    }

    public override void OnPickedUp(GameObject picker)
    {
        SwitchOwnership(picker);
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            SwitchOwnership(collision.gameObject);
        }
    }


}
