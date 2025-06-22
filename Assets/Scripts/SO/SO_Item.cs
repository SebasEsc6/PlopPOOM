using UnityEngine;

[CreateAssetMenu(fileName = "newItem", menuName = "Scriptable Objects/Item")]
public class SO_Item : ScriptableObject
{
    public int pickableId;
    public string pickableName;
    public Sprite pickableSprite;
    public float lifeTime;
    public float valueToIncrease;
}
