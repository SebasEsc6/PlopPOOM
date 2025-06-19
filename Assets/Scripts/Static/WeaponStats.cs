[System.Serializable]
public class WeaponStats
{
    public float cadence;
    public int ammoAmount;
    public float minDamage;
    public float maxDamage;
    public float timeToCharge;
    public float startScale;
    public float maxScale;
    public float minSpeed;
    public float maxSpeed;
    public float bulletLifeTime;

    public void ApplyModifier(float damageMultiplier)
    {
        minDamage *= damageMultiplier;
        maxDamage *= damageMultiplier;
    }

    public WeaponStats Clone()
    {
        return (WeaponStats)this.MemberwiseClone();
    }
}
