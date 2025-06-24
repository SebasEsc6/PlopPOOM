using UnityEngine;

public class ItemBase : PickableBase
{
    [SerializeField] protected SO_Item sO_Item;
    [SerializeField] protected CollisionDispatcher dispatcher;

    public virtual void ApplyEffect()
    {
        dispatcher.ConfigureCollisionData(CollisionFlags.Item, (ushort)sO_Item.itemId);
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ApplyEffect();
            OnPickedUp(collision.gameObject);
            
            NetworkObject.Despawn();
        }
    }
}
