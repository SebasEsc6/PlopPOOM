using UnityEngine;

[CreateAssetMenu(fileName = "Weapons", menuName = "Scriptable Objects/Weapons")]
public class SO_Weapons : ScriptableObject
{
    [Header("General Constants")]
    public int weaponId;
    public string weaponName;
    public Sprite sprite;
    [Range(0, 100)]
    public float dropProb;
    public float lifeTime = 7f;

    [Header("Stats")]
    public WeaponStats stats;
}
