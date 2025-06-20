using UnityEngine;

[CreateAssetMenu(fileName = "SO_PowerUps", menuName = "Scriptable Objects/SO_PowerUps")]
public class SO_PowerUps : ScriptableObject
{
    public int powerUpId;
    public string powerUpName;
    public Sprite powerUpSprite;
    public float lifeTime;
    public float valueToIncrease;
    public float duration;
    public float spawnProb;
}
