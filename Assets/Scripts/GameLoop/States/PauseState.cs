using UnityEngine;

public class PauseState : IGameState
{
    public void EnterState(GameManager manager)
    {
        Time.timeScale = 0;
    }

    public void UpdateState(GameManager manager)
    {
    }
    public void ExitState(GameManager manager)
    {
        Time.timeScale = 1;
    }

}
