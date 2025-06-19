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
    }

    public virtual void OnPickedUp(GameObject picker)
    {
        Debug.Log("I was pickable");
    }

    protected virtual void Start()
    {

        Destroy(gameObject, pickable.lifeTime);
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnPickedUp(collision.gameObject);
            Destroy(gameObject);
        }
    }
}
