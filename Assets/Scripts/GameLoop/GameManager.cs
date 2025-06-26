using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    private IGameState currentState;

    public PickableSpawner Spawner;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        GetDatabase();

        SetState(new WaitingState());
    }

    public void SetState(IGameState newState)
    {
        currentState?.ExitState(this);
        currentState = newState;
        currentState.EnterState(this);
    }

    private void Update()
    {
        currentState?.UpdateState(this);
    }
    public void GetDatabase()
    {
        SORegistry.RegisterAll<SO_Item>("SO/Items");
        // SORegistry.RegisterAll<SO_Weapons>("Weapons");
        SORegistry.RegisterAll<SO_PowerUps>("SO/PowerUps");
    }
    [ContextMenu("StartSpawn")]
    public void StartSpawnItems(bool value)
    {
        if (currentState is PlayingState)
        {
            Spawner.canSpawn = value;
        }
    }

    #region states
    public void StartGame() => SetState(new PlayingState());
    public void PauseGame() => SetState(new PauseState());
    public void EndGame() => SetState(new EndedState());
    #endregion
}
