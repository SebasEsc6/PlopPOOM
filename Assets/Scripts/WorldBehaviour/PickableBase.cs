using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PickableBase : NetworkBehaviour, IPickable
{
    public SpriteRenderer spriteRenderer;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    public virtual void OnPickedUp(GameObject picker)
    {
        // SwitchOwnership(picker);
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnPickedUp(collision.gameObject);
            NetworkObject.Despawn();
        }
    }

    public virtual IEnumerator DespawnAfterTimeLife(float lifeTime)
    {
        yield return new WaitForSeconds(lifeTime);
        NetworkObject.Despawn();
    }

    public virtual void SwitchOwnership(GameObject obj)
    {
        var netObj = GetComponent<NetworkObject>();
        var ownerClientId = obj.GetComponent<NetworkObject>().OwnerClientId;

        if (netObj.OwnerClientId != ownerClientId)
            netObj.ChangeOwnership(ownerClientId);

        netObj.Despawn();
    }
}
