using Unity.Netcode;
using UnityEngine;

public class WeaponIndentifier : NetworkBehaviour
{
    public SO_Weapons sO_Weapons;
    public SpriteRenderer spriteRenderer;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        spriteRenderer.sprite = sO_Weapons.sprite;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var netObj = GetComponent<NetworkObject>();
            var ownerClientId = collision.GetComponent<NetworkObject>().OwnerClientId;

            if (netObj.OwnerClientId != ownerClientId)
                netObj.ChangeOwnership(ownerClientId);

            netObj.Despawn();
        }
    }


}
