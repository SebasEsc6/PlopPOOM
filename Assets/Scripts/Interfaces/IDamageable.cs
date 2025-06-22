using UnityEngine;

public struct DamageData
{
    public int amount;
    public ulong attackerId;
    public Vector3 hitPoint;
    public double timeSent;
    public uint bulletId;
    public byte validationToken;
}

public interface IDamageable
{
    void TakeDamage(DamageData dmgData);
}