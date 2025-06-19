using UnityEngine;

[CreateAssetMenu(fileName = "newPickables", menuName = "Scriptable Objects/Pickables")]
public class SO_Pickables : ScriptableObject
{
    public int pickableId;
    public string pickableName;
    public Sprite pickableSprite;
    public float lifeTime;
    public float valueToIncrease;
}
