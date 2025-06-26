using UnityEngine;

public class LobbyState : IGameState
{
    public void EnterState(GameManager manager)
    {
        Debug.Log("WE ARE IN THE LOBBY STATE");
    }

    public void UpdateState(GameManager manager)
    {
        
    }
    public void ExitState(GameManager manager)
    {
    }

}
