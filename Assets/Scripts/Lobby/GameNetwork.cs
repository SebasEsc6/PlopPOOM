using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay.Models;
using System.Linq;
using UnityEngine.SceneManagement;

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
    public event Action<string> OnError;

    private Task _initTask;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _initTask = InitializeServicesAsync();
        }
        else Destroy(gameObject);
    }

    private async Task InitializeServicesAsync()
    {
        try
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"Initialization failed: {e}");
            OnError?.Invoke("Network initialization failed");
        }
    }

    public async void CreateAndHostLobby()
    {
        await _initTask;
        try
        {
            if (CurrentLobby != null)
                await LeaveLobbyAsync();

            var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
            var relayData = AllocationUtils.ToRelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>()
                .SetRelayServerData(relayData);

            string lobbyName = LobbyUtils.GenerateLobbyName();
            string playerName = LobbyUtils.GeneratePlayerName();

            var options = new CreateLobbyOptions
            {
                Player = new Player(
                    id: AuthenticationService.Instance.PlayerId,
                    data: new Dictionary<string, PlayerDataObject>
                    {
                        { "name", new PlayerDataObject(
                            PlayerDataObject.VisibilityOptions.Member, playerName) }
                    }
                )
            };

            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            NetworkManager.Singleton.StartHost();
            OnLobbyJoined?.Invoke();
            NetworkManager.Singleton.SceneManager
                .LoadScene(lobbySceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        catch (Exception e)
        {
            Debug.LogError($"CreateLobby failed: {e}");
            OnError?.Invoke("Failed to create lobby");
        }
    }

    public async void QuickJoinLobby()
    {
        await _initTask;
        try
        {
            if (CurrentLobby != null)
                await LeaveLobbyAsync();

            string playerName = LobbyUtils.GeneratePlayerName();
            var quickOpts = new QuickJoinLobbyOptions
            {
                Player = new Player(
                    id: AuthenticationService.Instance.PlayerId,
                    data: new Dictionary<string, PlayerDataObject>
                    {
                        { "name", new PlayerDataObject(
                            PlayerDataObject.VisibilityOptions.Member, playerName) }
                    }
                )
            };

            CurrentLobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickOpts);
            var alloc = await RelayService.Instance.JoinAllocationAsync(CurrentLobby.LobbyCode);
            var relayData = AllocationUtils.ToRelayServerData(alloc, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>()
                .SetRelayServerData(relayData);

            NetworkManager.Singleton.StartClient();
            OnLobbyJoined?.Invoke();
        }
        catch (LobbyServiceException e) when (e.Reason == LobbyExceptionReason.LobbyNotFound)
        {
            Debug.LogWarning("No lobby found for quick join");
            OnError?.Invoke("No available lobbies");
        }
        catch (Exception e)
        {
            Debug.LogError($"QuickJoin failed: {e}");
            OnError?.Invoke("Failed to join lobby");
        }
    }

    public async void JoinByCodeLobby(string code)
    {
        await _initTask;
        try
        {
            if (CurrentLobby != null)
                await LeaveLobbyAsync();

            string playerName = LobbyUtils.GeneratePlayerName();
            var joinOpts = new JoinLobbyByCodeOptions
            {
                Player = new Player(
                    id: AuthenticationService.Instance.PlayerId,
                    data: new Dictionary<string, PlayerDataObject>
                    {
                        { "name", new PlayerDataObject(
                            PlayerDataObject.VisibilityOptions.Member, playerName) }
                    }
                )
            };

            CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, joinOpts);
            var alloc = await RelayService.Instance.JoinAllocationAsync(CurrentLobby.LobbyCode);
            var relayData = AllocationUtils.ToRelayServerData(alloc, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>()
                .SetRelayServerData(relayData);

            NetworkManager.Singleton.StartClient();
            OnLobbyJoined?.Invoke();
        }
        catch (LobbyServiceException e) when (e.Reason == LobbyExceptionReason.LobbyNotFound)
        {
            Debug.LogWarning("Lobby code not found");
            OnError?.Invoke("Lobby not found");
        }
        catch (LobbyServiceException e) when (e.Reason == LobbyExceptionReason.Conflict)
        {
            if (e.Message.Contains("already a member"))
            {
                Debug.Log("Player already in that lobby, reloading lobby scene");
                NetworkManager.Singleton.SceneManager
                    .LoadScene(lobbySceneName, LoadSceneMode.Single);
            }
            else
            {
                Debug.LogError($"Lobby conflict: {e}");
                OnError?.Invoke("Could not join lobby due to conflict");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"JoinByCode failed: {e}");
            OnError?.Invoke("Failed to join lobby");
        }
    }

    public async Task LeaveLobbyAsync()
    {
        await _initTask;
        if (CurrentLobby == null) return;
        try
        {
            if (IsHost)
                await LobbyService.Instance.DeleteLobbyAsync(CurrentLobby.Id);
            else
                await LobbyService.Instance.RemovePlayerAsync(CurrentLobby.Id,
                    AuthenticationService.Instance.PlayerId);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"LeaveLobby failed: {e}");
        }
        finally
        {
            CurrentLobby = null;
            NetworkManager.Singleton.Shutdown();
        }
    }

    public async Task<List<Lobby>> ListLobbiesAsync(int count = 10)
    {
        await _initTask;
        try
        {
            var queryOptions = new QueryLobbiesOptions
            {
                Count = count,
                Filters = new List<QueryFilter>
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op:    QueryFilter.OpOptions.GT,
                    value: "0"
                )
            },
                Order = new List<QueryOrder>
            {
                new QueryOrder(
                    asc:  false,
                    field: QueryOrder.FieldOptions.Created
                )
            }
            };

            var page = await LobbyService.Instance.QueryLobbiesAsync(queryOptions);
            return page.Results
                       .Where(l => !l.IsPrivate)
                       .ToList();
        }
        catch (Exception e)
        {
            Debug.LogError($"ListLobbiesAsync failed: {e}");
            return new List<Lobby>();
        }
    }

    public async void SetLobbyPrivacy(bool isPrivate)
    {
        await _initTask;
        try
        {
            CurrentLobby = await LobbyService.Instance.UpdateLobbyAsync(
                CurrentLobby.Id,
                new UpdateLobbyOptions { IsPrivate = isPrivate }
            );
            OnLobbyUpdated?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"SetLobbyPrivacy failed: {e}");
            OnError?.Invoke("Failed to update lobby privacy");
        }
    }

    public void StartGame()
    {
        if (!IsHost) return;
        NetworkManager.Singleton.SceneManager
            .LoadScene(gameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
