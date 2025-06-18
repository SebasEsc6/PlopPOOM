using UnityEngine;

[CreateAssetMenu(fileName = "Weapons", menuName = "Scriptable Objects/Weapons")]
public class Weapons : ScriptableObject
{
    [Header("General Constants")]

    [Tooltip("Unique identifier for this weapon.")]
    [SerializeField] protected int weaponId;

    [Tooltip("Display name of the weapon.")]
    [SerializeField] protected string weaponName;

    [Tooltip("Icon or sprite used to represent the weapon.")]
    [SerializeField] protected Sprite sprite;

    [Header("Weapon Stats")]

    [Tooltip("Time between consecutive shots (in seconds).")]
    public float cadence;

    [Tooltip("Total ammo available for the weapon.")]
    public int ammoAmount;


    public float dropProb; // can be cool if the weapons spawn

    [Header("Bullets Stats")]
    [Tooltip("Time required to fully charge the shot.")]
    [SerializeField] protected float timeToCharge = 2f;

    [Tooltip("Initial scale of the projectile when charging begins.")]
    [SerializeField] protected float startScale = 0.2f;

    [Tooltip("Maximum scale the projectile can reach when fully charged.")]
    [SerializeField] protected float maxScale = 2f;

    [Tooltip("Minimum speed of the projectile at the lowest charge.")]
    [SerializeField] protected float minSpeed = 5f;

    [Tooltip("Maximum speed of the projectile at full charge.")]
    [SerializeField] protected float maxSpeed = 20f;

    [Tooltip("Minimum damage dealt at the lowest charge level.")]
    [SerializeField] protected float minDamage = 10f;

    [Tooltip("Maximum damage dealt at full charge.")]
    [SerializeField] protected float maxDamage = 50f;

}
