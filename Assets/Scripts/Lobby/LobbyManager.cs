using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class GameLobbyManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button createButton;
    [SerializeField] private Button quickJoinButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button startGameButton;

    [SerializeField] private TMP_InputField codeInput;
    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField] private TextMeshProUGUI playersListText;

    [Header("Settings")]
    [SerializeField] private string gameSceneName = "YourGameScene";
    [SerializeField] private int maxPlayers = 4;

    private Lobby hostLobby;
    private Lobby joinedLobby;
    private float heartbeatTimer = 15f;
    private float pollTimer = 1.1f;
    private string playerName;

    private async void Start()
    {
        // initialize Unity services and auth
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        playerName = "Player" + Random.Range(10, 99);

        // bind UI
        createButton.onClick.AddListener(OnCreateClicked);
        quickJoinButton.onClick.AddListener(OnQuickJoinClicked);
        joinButton.onClick.AddListener(OnJoinClicked);
        startGameButton.onClick.AddListener(OnStartGameClicked);

        startGameButton.interactable = false;
    }

    private void Update()
    {
        HandleHeartbeat();
        HandleLobbyPolling();
    }

    private void OnCreateClicked() => _ = CreateAndHost();
    private void OnQuickJoinClicked() => _ = QuickJoin();
    private void OnJoinClicked()
    {
        var code = codeInput.text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            Log("Enter a valid lobby code.");
            return;
        }
        _ = JoinByCode(code);
    }

    private void OnStartGameClicked()
    {
        if (!NetworkManager.Singleton.IsHost) return;
        Log("Starting game...");
        NetworkManager.Singleton.SceneManager.LoadScene(
            gameSceneName,
            UnityEngine.SceneManagement.LoadSceneMode.Single
        );
    }

    private async Task CreateAndHost()
    {
        DisableButtons();
        Log("Allocating Relay...");
        // 1) Allocate Relay for (maxPlayers - 1) clients
        var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
        var relayData = allocation.ToRelayServerData("udp");

        // 2) Configure transport
        var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        utp.SetRelayServerData(relayData);

        // 3) Start host
        Log("Starting Host...");
        NetworkManager.Singleton.StartHost();

        // 4) Create Lobby with join code embedded
        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        Log($"Relay join code: {joinCode}");

        var options = new CreateLobbyOptions
        {
            IsPrivate = false,
            Player = GetPlayer(),
            Data = new System.Collections.Generic.Dictionary<string, DataObject>
            {
                { "RelayCode", new DataObject(DataObject.VisibilityOptions.Public, joinCode) },
                { "PlayerName", new DataObject(DataObject.VisibilityOptions.Public, playerName) }
            }
        };

        hostLobby = await LobbyService.Instance.CreateLobbyAsync("MyLobby", maxPlayers, options);
        joinedLobby = hostLobby;

        Log($"Lobby created ({hostLobby.LobbyCode}). Players:");
        UpdatePlayersList();

        // enable start game for host
        startGameButton.interactable = true;
    }

    private async Task QuickJoin()
    {
        DisableButtons();
        Log("Quick-joining lobby...");
        joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(new QuickJoinLobbyOptions { Player = GetPlayer() });

        // extract Relay code
        var joinCode = joinedLobby.Data["RelayCode"].Value;
        Log($"Found lobby {joinedLobby.LobbyCode}. Joining Relay...");
        await JoinRelayAndStartClient(joinCode);
    }

    private async Task JoinByCode(string lobbyCode)
    {
        DisableButtons();
        Log($"Joining {lobbyCode}...");
        joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(
            lobbyCode,
            new JoinLobbyByCodeOptions { Player = GetPlayer() }
        );

        UpdatePlayersList();
        var code = joinedLobby.Data["RelayCode"].Value;
        await JoinRelayAndStartClient(code);
    }

    private async Task JoinRelayAndStartClient(string joinCode)
    {
        // join Relay allocation
        var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        var relayData = joinAllocation.ToRelayServerData("udp");

        var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        utp.SetRelayServerData(relayData);

        // start client and load scene
        Log("Starting Client...");
        NetworkManager.Singleton.StartClient();
        Log("Joined! Waiting for host to start game...");
    }

    private void HandleHeartbeat()
    {
        if (hostLobby == null) return;
        heartbeatTimer -= Time.deltaTime;
        if (heartbeatTimer > 0f) return;
        heartbeatTimer = 15f;
        _ = LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
    }

    private void HandleLobbyPolling()
    {
        if (joinedLobby == null) return;
        pollTimer -= Time.deltaTime;
        if (pollTimer > 0f) return;
        pollTimer = 1.1f;
        _ = RefreshLobby();
    }

    private async Task RefreshLobby()
    {
        joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby.Id);
        UpdatePlayersList();
    }

    private void UpdatePlayersList()
    {
        var sb = new System.Text.StringBuilder();
        foreach (var p in joinedLobby.Players)
        {
            sb.AppendLine($"{p.Id} – {p.Data["PlayerName"].Value}");
        }
        playersListText.text = sb.ToString();
        Log("Updated player list.");
    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new System.Collections.Generic.Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
            }
        };
    }

    private void DisableButtons()
    {
        createButton.interactable = false;
        quickJoinButton.interactable = false;
        joinButton.interactable = false;
    }

    private void Log(string message)
    {
        Debug.Log(message);
        logText.text = message;
    }
}
