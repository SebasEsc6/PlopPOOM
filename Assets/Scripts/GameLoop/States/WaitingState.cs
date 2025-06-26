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
        //TODO can change all logic here for implement the waiting room/ lobby while players enter
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
