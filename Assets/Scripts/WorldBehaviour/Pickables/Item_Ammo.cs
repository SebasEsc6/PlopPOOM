using UnityEngine;

public class Item_Ammo : ItemBase
{
    public override void ApplyEffect(GameObject player)
    {
        player.GetComponent<NetworkStatsController>().CurrentAmmo.Value += (int)sO_Item.valueToIncrease; //TODO method for reload but not reload more than masAmmo
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
