using UnityEngine;

public class MapSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] mapPrefabs;
    private GameObject currentMap;

    private void Start()
    {
        LobbyDataManager.Instance.SelectedMap.OnValueChanged += OnMapChanged;
        SpawnMap(LobbyDataManager.Instance.SelectedMap.Value);
    }

    private void OnMapChanged(int oldIndex, int newIndex)
    {
        if (currentMap != null) Destroy(currentMap);
        SpawnMap(newIndex);
    }

    private void SpawnMap(int index)
    {
        currentMap = Instantiate(mapPrefabs[index]);
        Debug.Log($"[MapSpawner] Spawned map at index {index}");
    }

    private void OnDestroy()
    {
        if (LobbyDataManager.Instance != null)
            LobbyDataManager.Instance.SelectedMap.OnValueChanged -= OnMapChanged;
    }
}
