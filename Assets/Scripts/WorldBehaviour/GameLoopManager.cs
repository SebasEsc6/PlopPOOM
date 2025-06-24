using UnityEngine;

public class GameLoopManager : MonoBehaviour
{
    [SerializeField]  private float itemSpawnCooldown = 5f;
    [SerializeField]  private float powerUpSpawnCooldown = 10f;

    private float itemTimer;
    private float powerUpTimer;

    [SerializeField] private PickableSpawner spawner;

    void Update()
    {
        itemTimer += Time.deltaTime;
        powerUpTimer += Time.deltaTime;

        if (itemTimer >= itemSpawnCooldown)
        {
            spawner.TrySpawnRandomItem();
            itemTimer = 0f;
        }

        if (powerUpTimer >= powerUpSpawnCooldown)
        {
            spawner.TrySpawnPowerUp();
            powerUpTimer = 0f;
        }
    }

}
