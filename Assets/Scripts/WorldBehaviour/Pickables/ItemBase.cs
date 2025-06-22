using UnityEngine;

public class ItemBase : PickableBase
{
    [SerializeField] protected SO_Item sO_Item;

    public virtual void ApplyEffect(GameObject player)
    {
        Debug.Log($"Apply my effect in {player}");
    }
}
