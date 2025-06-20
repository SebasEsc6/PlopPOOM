using UnityEngine;

public class Item_Heal : ItemBase
{
    public override void ApplyEffect(GameObject player)
    {
        player.GetComponent<NetworkStatsController>().CurrentHealth.Value += (int)sO_Item.valueToIncrease; //TODO method for heal but not heal more than maxHealth
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ApplyEffect(collision.gameObject);
            OnPickedUp(collision.gameObject);
        }
    }
}
