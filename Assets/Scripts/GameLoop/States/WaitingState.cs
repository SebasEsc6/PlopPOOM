using UnityEngine;

public class WaitingState : IGameState
{
    private float countdown = 5f;
    public void EnterState(GameManager manager)
    {
        Debug.Log("Waiting for players...");
    }

    public void UpdateState(GameManager manager)
    {
        countdown -= Time.deltaTime;

        if (countdown <= 0f)
        {
            manager.SetState(new PlayingState());
        }
    }
    public void ExitState(GameManager manager)
    {
    }

}
