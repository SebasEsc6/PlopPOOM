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
            var playerStats = collision.GetComponent<NetworkStatsController>();
            if (playerStats == null) return;

            bool canPick = false;

            switch (sO_Item.itemType)
            {
                case ItemType.Ammo:
                    canPick = playerStats.CanPickAmmo();
                    break;
                case ItemType.Heal:
                    canPick = playerStats.CanPickHeal();
                    break;
            }

            if (canPick)
            {
                ApplyEffect();
                NetworkObject.Despawn();
            }

        }

    }

}
