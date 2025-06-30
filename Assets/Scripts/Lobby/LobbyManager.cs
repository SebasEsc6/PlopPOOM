using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System.Collections;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private TMP_Text codeLabel;
    [SerializeField] private GameObject playerInfoComponentPrefab;
    [SerializeField] private Transform playerListContainer;
    [SerializeField] private Toggle publicToggle;
    [SerializeField] private Button startBtn;
    [SerializeField] private Button leaveLobbyBtn;
    [SerializeField] private Button copyCodeBtn;

    private Coroutine pollCoroutine;

    private void Awake()
    {
        // Register handler for force return to main menu
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("ForceReturnToMainMenu", (senderClientId, reader) =>
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
            });
        }

        NetworkManager.Singleton.CustomMessagingManager
    .RegisterNamedMessageHandler("NewMemberJoined", (sender, reader) =>
    {
        if (GameNetwork.Instance.IsHost)
            RefreshLobbyFromServer();
    });
    }

    private void OnEnable()
    {
        var net = GameNetwork.Instance;

        GameNetwork.Instance.OnLobbyJoined += UpdateUI;
        GameNetwork.Instance.OnLobbyUpdated += UpdateUI;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        pollCoroutine = StartCoroutine(PollLobbyCoroutine());
    }

    private void OnDisable()
    {
        var net = GameNetwork.Instance;

        GameNetwork.Instance.OnLobbyJoined -= UpdateUI;
        GameNetwork.Instance.OnLobbyUpdated -= UpdateUI;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;

        if (pollCoroutine != null) StopCoroutine(pollCoroutine);
    }

    private void Start()
    {
        startBtn.onClick.AddListener(() =>
            GameNetwork.Instance.StartGame()
        );
        startBtn.interactable = GameNetwork.Instance.IsHost;
        publicToggle.onValueChanged.AddListener(isOn =>
        {
            Debug.Log($"[LobbyManager] Toggle clicked → publicToggle.isOn = {isOn}");
            GameNetwork.Instance.SetLobbyPrivacy(!isOn);
        });
        leaveLobbyBtn.onClick.AddListener(async () => await LeaveLobbyAndGoToMainMenu());
        copyCodeBtn.onClick.AddListener(CopyLobbyCodeToClipboard);
        UpdateUI();
    }

    private void UpdateUI()
    {
        var lobby = GameNetwork.Instance.CurrentLobby;
        if (lobby == null) return;

        // show code
        codeLabel.text = $"Code: {lobby.LobbyCode}";
        publicToggle.SetIsOnWithoutNotify(!lobby.IsPrivate);
        UpdatePlayerList();
        // Only allow start if at least 2 players and is host
        startBtn.interactable = (lobby.Players != null && lobby.Players.Count >= 2 && GameNetwork.Instance.IsHost);
    }

    private IEnumerator PollLobbyCoroutine()
    {
        while (true)
        {
            RefreshLobbyFromServer(); // Your async method to fetch latest lobby data
            yield return new WaitForSeconds(3f);
        }
    }

    private async void RefreshLobbyFromServer()
    {
        if (GameNetwork.Instance.CurrentLobby != null)
        {
            var lobbyId = GameNetwork.Instance.CurrentLobby.Id;
            var latestLobby = await Unity.Services.Lobbies.LobbyService.Instance.GetLobbyAsync(lobbyId);
            GameNetwork.Instance.SetCurrentLobby(latestLobby); UpdateUI();
        }
    }

    private void UpdatePlayerList()
    {
        var lobby = GameNetwork.Instance.CurrentLobby;
        if (lobby == null)
        {
            // Clear the list if lobby is null
            foreach (Transform child in playerListContainer)
                Destroy(child.gameObject);
            return;
        }

        // Clear existing UI elements
        foreach (Transform child in playerListContainer)
            Destroy(child.gameObject);

        if (lobby.Players != null && lobby.Players.Count > 0)
        {
            foreach (var p in lobby.Players)
            {
                var playerUI = Instantiate(playerInfoComponentPrefab, playerListContainer);
                string displayName;
                var playerText = playerUI.GetComponentInChildren<TMP_Text>();
                if (p.Data != null && p.Data.ContainsKey("name") && !string.IsNullOrEmpty(p.Data["name"].Value))
                {
                    displayName = p.Data["name"].Value;
                    playerText.text = displayName;
                }
            }
        }
    }

    private void CopyLobbyCodeToClipboard()
    {
        var lobby = GameNetwork.Instance.CurrentLobby;
        if (lobby != null)
        {
            GUIUtility.systemCopyBuffer = lobby.LobbyCode;
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (GameNetwork.Instance.IsHost)
        {
            using (var writer = new FastBufferWriter(1, Unity.Collections.Allocator.Temp))
            {
                NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("PlayerLeft", writer);
            }
        }
    }

    public async System.Threading.Tasks.Task LeaveLobbyAndGoToMainMenu()
    {
        if (GameNetwork.Instance != null)
        {
            // If host, notify all clients to return to main menu
            if (GameNetwork.Instance.IsHost)
            {
                using (var writer = new FastBufferWriter(1, Unity.Collections.Allocator.Temp))
                {
                    NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("ForceReturnToMainMenu", writer);
                }
            }
            await GameNetwork.Instance.LeaveLobbyAsync();
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
