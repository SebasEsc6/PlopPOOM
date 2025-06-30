using UnityEngine;

public class MapSelectionUI : MonoBehaviour
{

    [Header("Map Selection UI")]
    [SerializeField] private int mapPrefabIndex;

    public void OnMapSelected()
    {
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
}
