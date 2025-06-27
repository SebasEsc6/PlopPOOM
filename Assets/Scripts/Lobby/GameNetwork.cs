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
using System.Threading.Tasks;

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
        if (CurrentLobby != null)
            await LeaveLobbyAsync();

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
        if (CurrentLobby != null)
            await LeaveLobbyAsync();

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
        if (CurrentLobby != null)
            await LeaveLobbyAsync();

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

    /// <summary>
    /// Leaves the current lobby (delete if host, remove player if client) and shuts down Netcode
    /// </summary>
    public async Task LeaveLobbyAsync()
    {
        if (CurrentLobby == null) return;

        try
        {
            if (IsHost)
            {
                // Host delete lobby
                await LobbyService.Instance.DeleteLobbyAsync(CurrentLobby.Id);
            }
            else
            {
                // Client removes itself from lobby
                await LobbyService.Instance.RemovePlayerAsync(
                    CurrentLobby.Id,
                    AuthenticationService.Instance.PlayerId
                );
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning($"Failed to leave lobby: {e}");
        }
        finally
        {
            // Always reset state and network
            CurrentLobby = null;
            NetworkManager.Singleton.Shutdown();
        }
    }

    /// <summary>
    /// Query a list of public lobbies (max 'count').
    /// </summary>
    public async Task<List<Lobby>> ListLobbiesAsync(int count = 10)
    {
        var queryOptions = new QueryLobbiesOptions
        {
            Count = count,
            Filters = new List<QueryFilter>
        {
            new QueryFilter(
                field: QueryFilter.FieldOptions.AvailableSlots,
                op:    QueryFilter.OpOptions.EQ,
                value: "false"
            )
        }
        };
        var page = await LobbyService.Instance.QueryLobbiesAsync(queryOptions);
        return page.Results;
    }
}
