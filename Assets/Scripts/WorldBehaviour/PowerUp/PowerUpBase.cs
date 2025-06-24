using UnityEngine;

public class PowerUpBase : PickableBase
{
    [SerializeField] protected SO_PowerUps sO_PowerUps;

    [SerializeField] protected CollisionDispatcher dispatcher;

    public virtual void ApplyPowerUp()
    {
        dispatcher.ConfigureCollisionData(CollisionFlags.PowerUp, (ushort)sO_PowerUps.powerUpId);
    }
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ApplyPowerUp();
            NetworkObject.Despawn();
        }
    }
}
