using UnityEngine;

public class GameLoopManager : MonoBehaviour
{
    public GameManager gameManager;
    public PickableSpawner spawner;

    void Awake()
    {
        gameManager = GameManager.Instance;

        gameManager.gameLoopManager = this;
        gameManager.Spawner = spawner;
        gameManager.SetState(new WaitingState());
    }
}
