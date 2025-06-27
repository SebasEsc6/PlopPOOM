using System;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Collections.Generic;

public class GameNetwork : MonoBehaviour
{
    public static GameNetwork Instance { get; private set; }
    public Lobby CurrentLobby { get; private set; }
    public bool IsHost => NetworkManager.Singleton.IsHost;

    [SerializeField] private string lobbySceneName;
    [SerializeField] private string gameSceneName;
    [SerializeField] private int maxPlayers = 4;

    public event Action OnLobbyJoined;
    public event Action OnLobbyUpdated;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeServices();
        }
        else Destroy(gameObject);
    }

    private async void InitializeServices()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    public async void CreateAndHostLobby()
    {
        var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
        var relayData = AllocationUtils.ToRelayServerData(allocation, "dtls");
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(relayData);

        string lobbyName = LobbyUtils.GenerateLobbyName();
        string playerName = LobbyUtils.GeneratePlayerName();

        var options = new CreateLobbyOptions
        {
            Player = new Player(
                id: AuthenticationService.Instance.PlayerId,
                data: new Dictionary<string, PlayerDataObject>
                {
                    { "name", new PlayerDataObject(
                        PlayerDataObject.VisibilityOptions.Member,
                        playerName
                    ) }
                }
            )
        };

        CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

        NetworkManager.Singleton.StartHost();

        OnLobbyJoined?.Invoke();

        NetworkManager.Singleton.SceneManager
            .LoadScene(lobbySceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public async void QuickJoinLobby()
    {
        string playerName = LobbyUtils.GeneratePlayerName();

        var quickJoinOptions = new QuickJoinLobbyOptions
        {
            Player = new Player(
                id: AuthenticationService.Instance.PlayerId,
                data: new Dictionary<string, PlayerDataObject>
                {
                    { "name", new PlayerDataObject(
                        PlayerDataObject.VisibilityOptions.Member,
                        playerName
                    ) }
                }
            )
        };

        CurrentLobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinOptions);
        var code = CurrentLobby.LobbyCode;

        var allocation = await RelayService.Instance.JoinAllocationAsync(code);
        var relayData = AllocationUtils.ToRelayServerData(allocation, "dtls");
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(relayData);

        NetworkManager.Singleton.StartClient();
        OnLobbyJoined?.Invoke();
    }

    public async void JoinByCodeLobby(string code)
    {
        CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code);

        var allocation = await RelayService.Instance.JoinAllocationAsync(code);
        var relayData = AllocationUtils.ToRelayServerData(allocation, "dtls");
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(relayData);

        NetworkManager.Singleton.StartClient();
        OnLobbyJoined?.Invoke();
    }

    public async void SetLobbyPrivacy(bool isPrivate)
    {
        CurrentLobby = await LobbyService.Instance.UpdateLobbyAsync(
            CurrentLobby.Id,
            new UpdateLobbyOptions { IsPrivate = isPrivate }
        );
        OnLobbyUpdated?.Invoke();
    }

    public void StartGame()
    {
        if (!IsHost) return;
        NetworkManager.Singleton.SceneManager
            .LoadScene(gameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
