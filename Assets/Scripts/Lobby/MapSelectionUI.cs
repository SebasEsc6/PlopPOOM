using UnityEngine;
using UnityEngine.UI;

public class MapSelectionUI : MonoBehaviour
{
    [Header("Map Selection UI")]
    [SerializeField] private int mapPrefabIndex;
    private MapSelectionGroup group;
    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void SetGroup(MapSelectionGroup group)
    {
        this.group = group;
    }

    public void OnMapSelected()
    {
        if (group != null)
            group.OnButtonSelected(this);

        if (GameManager.Instance != null)
        {
            Debug.Log($"[MapSelectionUI] Map selected: {mapPrefabIndex}");
            GameManager.Instance.SetSelectedMapServerRpc(mapPrefabIndex);
        }
        else
        {
            Debug.Log("Only the host/server can select the map.");
        }
    }

    public void SetSelected(bool isSelected)
    {
        transform.localScale = isSelected ? originalScale * 1.15f : originalScale;
    }
}