using Unity.Netcode;
using UnityEngine;

public struct CollisionMessageInfo : INetworkSerializable
{
    public byte Flags;         // Bit-flags: category (Damage, Buff) + type subflags
    public ushort Value;       // Generic payload: damage amount, buff ID, pickup qty
    public ulong Source;       // Origin ClientId
    public ulong Destination;  // Target NetworkObjectId
    public double Timestamp;    // ServerTime for ordering
    public int Data1;          // Optional: e.g. buff duration or ammo type
    public int Data2;          // Optional: e.g. secondary quantity

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Flags);
        serializer.SerializeValue(ref Value);
        serializer.SerializeValue(ref Source);
        serializer.SerializeValue(ref Destination);
        serializer.SerializeValue(ref Timestamp);
        serializer.SerializeValue(ref Data1);
        serializer.SerializeValue(ref Data2);
    }
}
