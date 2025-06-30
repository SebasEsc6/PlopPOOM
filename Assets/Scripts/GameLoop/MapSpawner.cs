using UnityEngine;

public class MapSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] mapPrefabs;

    private void Start()
    {
        int index = GameManager.Instance.selectedMapIndex.Value;
        Debug.Log($"[MapSpawner] Selected map index: {index}");
        if (index >= 0 && index < mapPrefabs.Length)
        {
            SpawnMap(mapPrefabs[index]);
        }
        else
        {
            Debug.LogError($"Invalid map index: {index}. Check mapPrefabs array and selectedMapIndex value.");
        }
    }

    private void SpawnMap(GameObject mapToSpawn)
    {
        var map = Instantiate(mapToSpawn);
    }
}
