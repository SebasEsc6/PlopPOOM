using UnityEngine;

[CreateAssetMenu(fileName = "SO_PowerUps", menuName = "Scriptable Objects/SO_PowerUps")]
public class SO_PowerUps : ScriptableObject, IIdentifiableSO
{
    public int Id => powerUpId;
    public int powerUpId;
    public string powerUpName;
    public Sprite powerUpSprite;
    public float lifeTime;
    public float valueToIncrease;
    public float duration;
    [Range(0, 100)]
    public float spawnProb;

}
