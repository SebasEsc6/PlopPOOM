using UnityEngine;

public struct DamageData
{
    public int amount;
    public ulong attackerId;
    public Vector3 hitPoint;
}

public interface IDamageable
{
    void TakeDamage(DamageData dmgData);
}