using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PickableBase : NetworkBehaviour, IPickable
{
    public SO_Pickables pickable;
    public SpriteRenderer spriteRenderer;
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        spriteRenderer.sprite = pickable.pickableSprite;
        StartCoroutine(DespawnAfterTimeLife());
    }

    public virtual void OnPickedUp(GameObject picker)
    {
        Debug.Log("I was pickable");
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnPickedUp(collision.gameObject);
            NetworkObject.Despawn();
        }
    }

    public virtual IEnumerator DespawnAfterTimeLife()
    {
        yield return new WaitForSeconds(pickable.lifeTime);
        NetworkObject.Despawn();
    }
}
