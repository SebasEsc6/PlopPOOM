using UnityEngine;

public class PoolRuntimeBootstrap : MonoBehaviour
{
    [Header("Prefabs to register at runtime")]
    [SerializeField] private GameObject player1AvatarPrefab;
    [SerializeField] private GameObject player2AvatarPrefab;
    [SerializeField] private int prewarmAvatars = 2;
    [SerializeField] private int maxAvatars = 4;
    [SerializeField] private GameObject bulletPlayer1Prefab;
    [SerializeField] private GameObject bulletPlayer2Prefab;
    [SerializeField] private int prewarmBullets = 20;
    [SerializeField] private int maxBullets = 40;

    private void Start()
    {
        // Ensure PoolManager exists in scene
        if (!PoolManager.Instance)
        {
            Debug.LogError("PoolManager instance not found. Place one empty GameObject with PoolManager in the scene.");
            return;
        }

        // Register prefabs. .
        if (player1AvatarPrefab)
            PoolManager.Instance.RegisterPrefab(player1AvatarPrefab, prewarm: prewarmAvatars, maxSize: maxAvatars);
        if (player2AvatarPrefab)
            PoolManager.Instance.RegisterPrefab(player2AvatarPrefab, prewarm: prewarmAvatars, maxSize: maxAvatars);
        if (bulletPlayer1Prefab)
            PoolManager.Instance.RegisterPrefab(bulletPlayer1Prefab, prewarm: prewarmBullets, maxSize: maxBullets);
        if (bulletPlayer2Prefab)
            PoolManager.Instance.RegisterPrefab(bulletPlayer2Prefab, prewarm: prewarmBullets, maxSize: maxBullets);
    }
}
