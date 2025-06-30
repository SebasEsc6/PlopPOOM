using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public IGameState currentState;

    //======== Game Loop Manager ========//
    [HideInInspector]
    public GameLoopManager gameLoopManager;
    [HideInInspector]
    public PickableSpawner Spawner;

    public event System.Action<IGameState> OnStateChanged;

    // Networked map index
    public NetworkVariable<int> selectedMapIndex = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);
        GetDatabase();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
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

    [ServerRpc(RequireOwnership = false)]
    public void SetSelectedMapServerRpc(int index)
    {
        selectedMapIndex.Value = index;
        // A continuación, reparto la notificación a todos los clientes
        // BroadcastMapSelectedClientRpc(index);
        Debug.Log($"[GameManager] (Host) MapIndex ahora: {index}");
    }

    // // Llamada servidor→clientes: todos reciben este RPC
    // [ClientRpc]
    // private void BroadcastMapSelectedClientRpc(int index)
    // {
    //     // Aquí corro lógica en cada cliente (incluido el host, si quieres)
    //     Debug.Log($"[GameManager] (Client) recibió MapIndex: {index}");
    //     // p.ej. disparar el spawn de mapa:
    //     selectedMapIndex.Value = index;
    //     Debug.Log($"[GameManager] (Client) MapIndex ahora: {selectedMapIndex.Value}");
    // }
}
