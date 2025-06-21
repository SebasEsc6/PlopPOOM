using UnityEngine;

[CreateAssetMenu(fileName = "newItem", menuName = "Scriptable Objects/Item")]
public class SO_Item : ScriptableObject, IIdentifiableSO
{
    public int Id => itemId;
    public int itemId;
    public string itemName;
    public Sprite itemSprite;
    public float lifeTime;
    public int valueToIncrease;

}
