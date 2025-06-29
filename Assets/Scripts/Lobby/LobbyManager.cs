using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.Netcode;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private TMP_Text codeLabel;
    [SerializeField] private TMP_Text playersText;
    [SerializeField] private Toggle publicToggle;
    [SerializeField] private Button startBtn;
    [SerializeField] private Button refreshBtn;
    [SerializeField] private Button leaveLobbyBtn;
    [SerializeField] private Button copyCodeBtn;

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
    }

    private void OnEnable()
    {
        var net = GameNetwork.Instance;

        GameNetwork.Instance.OnLobbyJoined += UpdateUI;
        GameNetwork.Instance.OnLobbyUpdated += UpdateUI;
    }

    private void OnDisable()
    {
        var net = GameNetwork.Instance;

        GameNetwork.Instance.OnLobbyJoined -= UpdateUI;
        GameNetwork.Instance.OnLobbyUpdated -= UpdateUI;
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
        refreshBtn.onClick.AddListener(UpdatePlayerList);
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
    }

    private void UpdatePlayerList()
    {
        var lobby = GameNetwork.Instance.CurrentLobby;
        if (lobby == null)
        {
            playersText.text = "Waiting for players...";
            return;
        }
        if (lobby.Players != null && lobby.Players.Count > 0)
        {
            var lines = new List<string>();
            foreach (var p in lobby.Players)
            {
                if (p.Data != null && p.Data.ContainsKey("name")
                    && !string.IsNullOrEmpty(p.Data["name"].Value))
                {
                    lines.Add(p.Data["name"].Value);
                }
                else
                {
                    lines.Add(p.Id);
                }
            }
            playersText.text = string.Join("\n", lines);
        }
        else
        {
            playersText.text = "Waiting for players...";
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
