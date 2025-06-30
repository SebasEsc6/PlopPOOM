using Unity.Netcode;
using Unity.Collections;
using System;

public struct PlayerDataNet : INetworkSerializable, IEquatable<PlayerDataNet>
{
    public ulong clientId;
    public FixedString32Bytes playerName;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref playerName);
    }

    public bool Equals(PlayerDataNet other)
    {
        return clientId == other.clientId && playerName.Equals(other.playerName);
    }

    public override bool Equals(object obj)
    {
        return obj is PlayerDataNet other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(clientId, playerName.GetHashCode());
    }
}
