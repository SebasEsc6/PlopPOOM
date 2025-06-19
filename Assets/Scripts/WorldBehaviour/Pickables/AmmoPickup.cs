using UnityEngine;

public class AmmoPickup : PickableBase
{
    public override void OnPickedUp(GameObject picker)
    {
        picker.GetComponent<NetworkStatsController>().CurrentAmmo.Value += (int)pickable.valueToIncrease;
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            OnPickedUp(collision.gameObject);
        }
    }
}
