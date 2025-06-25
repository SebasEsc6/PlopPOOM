using UnityEngine;

public class EndedState : IGameState
{
    public void EnterState(GameManager manager)
    {
        Debug.Log("Game Over");
    }
    public void UpdateState(GameManager manager)
    {
        Debug.Log("Executing animation");
    }
    public void ExitState(GameManager manager)
    {
        Debug.Log("Going to lobby");
    }

}
