using UnityEngine;

public class PoolRuntimeBootstrap : MonoBehaviour
{
    [Header("Prefabs to register at runtime")]
    [SerializeField] private GameObject player1AvatarPrefab;
    [SerializeField] private GameObject player2AvatarPrefab;
    [SerializeField] private GameObject bulletPlayer1Prefab;
    [SerializeField] private GameObject bulletPlayer2Prefab;

    private void Awake()
    {
        // Ensure PoolManager exists in scene
        if (!PoolManager.Instance)
        {
            Debug.LogError("PoolManager instance not found. Place one empty GameObject with PoolManager in the scene.");
            return;
        }

        // Register prefabs. .
        if (player1AvatarPrefab)
            PoolManager.Instance.RegisterPrefab(player1AvatarPrefab, prewarm: 2, maxSize: 4);
        if (player2AvatarPrefab)
            PoolManager.Instance.RegisterPrefab(player2AvatarPrefab, prewarm: 2, maxSize: 4);
        if (bulletPlayer1Prefab)
            PoolManager.Instance.RegisterPrefab(bulletPlayer1Prefab, prewarm: 20, maxSize: 40);
        if (bulletPlayer2Prefab)
            PoolManager.Instance.RegisterPrefab(bulletPlayer2Prefab, prewarm: 20, maxSize: 40);
    }
}
