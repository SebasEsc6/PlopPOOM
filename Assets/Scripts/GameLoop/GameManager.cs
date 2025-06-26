using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public IGameState currentState;

    // [HideInInspector]
    //======== Game Loop Manager ========//
    public GameLoopManager gameLoopManager;
    // [HideInInspector]
    public PickableSpawner Spawner;

    public event System.Action<IGameState> OnStateChanged;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        GetDatabase();
    }

    public void SetState(IGameState newState)
    {
        currentState?.ExitState(this);
        currentState = newState;
        currentState.EnterState(this);

        OnStateChanged?.Invoke(currentState);
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

    #region states
    public void StartGame() => SetState(new PlayingState());
    public void PauseGame() => SetState(new PauseState());
    public void EndGame() => SetState(new EndedState());
    #endregion


}
