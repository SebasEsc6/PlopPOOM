using UnityEngine;

public class PlayingState : IGameState
{
    private float itemTimer;
    private float powerUpTimer;

    private float itemCooldown = 5f;
    private float powerUpCooldown = 10f;

    public void EnterState(GameManager manager)
    {
        Debug.Log($"Enter to state {this}");
        itemTimer = 0f;
        powerUpTimer = 0f;
    }

    public void UpdateState(GameManager manager)
    {
        itemTimer += Time.deltaTime;
        powerUpTimer += Time.deltaTime;
        Debug.Log(itemTimer);

        if (itemTimer >= itemCooldown)
        {
            manager.Spawner.TrySpawnRandomItem();
            itemTimer = 0f;
        }

        if (powerUpTimer >= powerUpCooldown)
        {
            manager.Spawner.TrySpawnPowerUp();
            powerUpTimer = 0f;
        }
    }

    public void ExitState(GameManager manager)
    {
        // Optional cleanup
    }
}

