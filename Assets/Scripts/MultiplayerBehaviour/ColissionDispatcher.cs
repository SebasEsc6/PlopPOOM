using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Attach to any collider object (bullets, zones, pickups).
/// On trigger, builds a CollisionMessageInfo and forwards it.
/// </summary>
[DisallowMultipleComponent]
public class CollisionDispatcher : NetworkBehaviour
{
    [Header("Collision Settings")]
    [Tooltip("Bit-flags defining this event: Damage | Buff | Pickup | ...")]
    public byte collisionFlags;

    [Tooltip("Primary payload: damage, buff ID, pickup qty")]
    public ushort value;

    [Tooltip("Optional metadata: buff duration, item subtype")]
    public int data1, data2;

    private NetworkObject netObj;

    private void Awake() => netObj = GetComponent<NetworkObject>();

    /// <summary>
    /// Configure this dispatcher from a DamageData struct.
    /// </summary>
    public void ConfigureFromDamageData(DamageData dmg)
    {
        collisionFlags = (byte)CollisionFlags.Damage;
        value = (ushort)dmg.amount;
        data1 = (int)dmg.bulletId;
        data2 = dmg.validationToken;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // only the owner processes physics
        if (!IsOwner) return;

        // must hit a NetworkObject
        var targetNetObj = other.GetComponent<NetworkObject>();
        if (targetNetObj == null) return;

        // must implement the network handler
        if (!other.TryGetComponent<ICollisionHandler>(out var handler)) return;

        // build and send collision message
        var msg = new CollisionMessageInfo
        {
            Flags = collisionFlags,
            Value = value,
            Source = netObj.OwnerClientId,
            Destination = targetNetObj.NetworkObjectId,
            Timestamp = NetworkManager.Singleton.LocalTime.Time,
            Data1 = data1,
            Data2 = data2
        };

        handler.SendCollisionMessage(msg);
    }
}