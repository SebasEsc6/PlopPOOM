using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Single responder that handles Damage, Buff and Pickup based on flags.
/// </summary>
[DisallowMultipleComponent]
public class UniversalCollisionResponder : NetworkBehaviour, ICollisionResponder
{
    [SerializeField] private NetworkStatsController statsController;
    // [SerializeField] private BuffSystem buffSystem;

    // Flag definitions
    private const byte DamageFlag = 1 << 0;
    private const byte BuffFlag = 1 << 1;
    private const byte PickupFlag = 1 << 2;

    /// <summary>
    /// Called on owner when a collision message arrives.
    /// </summary>
    public void OnCollision(CollisionMessageInfo msg)
    {
        if (!IsOwner) return;

        var flags = (CollisionFlags)msg.Flags;

        switch (flags)
        {
            case CollisionFlags.Damage:
                var dmg = new DamageData
                {
                    amount = msg.Value,
                    attackerId = msg.Source,
                    timeSent = msg.Timestamp
                };
                statsController.TakeDamage(dmg);
                break;
            
            case CollisionFlags.Item:
                statsController.ApplyItemEffect(msg.Value);
                break;

            case CollisionFlags.PowerUp:
                statsController.ActivatePowerUp(msg.Value);
                break;

            default:
                Debug.Log("Flag not founded");
                break;
        }
    }

}
