using UnityEngine;

public class PlayingState : IGameState
{

    public void EnterState(GameManager manager)
    {
        Debug.Log($"Enter to state {this}");
    }

    public void UpdateState(GameManager manager)
    {
        manager.Spawner.UpdateSpawner();
    }

    public void ExitState(GameManager manager)
    {
        // Optional cleanup
    }
}

