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
using Unity.Collections;
using SmallHedge.SoundManager;

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
    private bool _isProcessing;

    private SoundManager _soundManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _initTask = InitServices();
        }
        else Destroy(gameObject);
    }

    private async Task InitServices()
    {
        try
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"Init failed: {e}");
            OnError?.Invoke("Network init failed");
        }
    }

    public void SetCurrentLobby(Lobby newLobby)
    {
        CurrentLobby = newLobby;
        OnLobbyUpdated?.Invoke();
    }

    public async void CreateAndHostLobby()
    {
        if (_isProcessing) return;
        _isProcessing = true;

        await _initTask;
        try
        {
            if (CurrentLobby != null)
                await LeaveLobbyAsync();

            var allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
            var relayData = AllocationUtils.ToRelayServerData(allocation, "dtls");
            var relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

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
                ),
                Data = new Dictionary<string, DataObject>
            {
                { "relayJoinCode",
                    new DataObject(
                        DataObject.VisibilityOptions.Member,
                        relayJoinCode
                    )
                }
            }
            };

            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

            NetworkManager.Singleton.StartHost();
            NetworkManager.Singleton.SceneManager
                .LoadScene(lobbySceneName, LoadSceneMode.Single);
            OnLobbyJoined?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"CreateLobby failed: {e}");
            OnError?.Invoke("Failed to create lobby");
        }
        finally { _isProcessing = false; }
    }

    public async void QuickJoinLobby()
    {
        if (_isProcessing) return;
        _isProcessing = true;

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
            string joinCode = CurrentLobby.Data["relayJoinCode"].Value;
            var alloc = await RelayService.Instance.JoinAllocationAsync(joinCode);
            var relayData = AllocationUtils.ToRelayServerData(alloc, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>()
                .SetRelayServerData(relayData);

            NetworkManager.Singleton.StartClient();

            if (!IsHost)
                SendNewMemberJoinedWhenConnected();

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
        finally { _isProcessing = false; }
    }

    public async void JoinByCodeLobby(string code)
    {
        if (_isProcessing) return;
        if (string.IsNullOrWhiteSpace(code))
        {
            OnError?.Invoke("Enter a valid lobby code");
            return;
        }
        _isProcessing = true;

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
            string joinCode = CurrentLobby.Data["relayJoinCode"].Value;
            var alloc = await RelayService.Instance.JoinAllocationAsync(joinCode);
            var relayData = AllocationUtils.ToRelayServerData(alloc, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>()
                .SetRelayServerData(relayData);

            NetworkManager.Singleton.StartClient();

            if (!IsHost)
                SendNewMemberJoinedWhenConnected();

            OnLobbyJoined?.Invoke();
        }
        catch (LobbyServiceException e) when (e.Reason == LobbyExceptionReason.LobbyNotFound)
        {
            OnError?.Invoke("Lobby not found");
        }
        catch (Exception e)
        {
            Debug.LogError($"JoinByCode failed: {e}");
            OnError?.Invoke("Failed to join lobby");
        }
        finally { _isProcessing = false; }
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

    private void SendNewMemberJoinedWhenConnected()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        SoundManager.PlaySound(SoundType.EnterLobby);
    }

    private void OnClientConnected(ulong clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnSceneLoaded;
        NetworkManager.Singleton.SceneManager.LoadScene(lobbySceneName, LoadSceneMode.Single);
    }

    private void OnSceneLoaded(ulong clientId, string sceneName, LoadSceneMode mode)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId) return;
        if (sceneName != lobbySceneName) return;

        NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnSceneLoaded;

        NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(
            "NewMemberJoined",
            NetworkManager.ServerClientId,
            new FastBufferWriter(0, Allocator.Temp)
        );
    }

    public async void SetLobbyPrivacy(bool isPrivate)
    {
        Debug.Log($"[GameNetwork] Setting lobby privacy → IsPrivate = {isPrivate}");
        CurrentLobby = await LobbyService.Instance.UpdateLobbyAsync(
            CurrentLobby.Id,
            new UpdateLobbyOptions { IsPrivate = isPrivate }
        );
        Debug.Log($"[GameNetwork] Lobby privacy now → IsPrivate = {CurrentLobby.IsPrivate}");
        OnLobbyUpdated?.Invoke();
    }

    public void StartGame()
    {
        if (!IsHost) return;
        NetworkManager.Singleton.SceneManager
            .LoadScene(gameSceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
