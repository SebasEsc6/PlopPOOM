using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class MainMenuManager : MonoBehaviour
{
    [Header("Lobby Actions")]
    [SerializeField] private Button createBtn;
    [SerializeField] private Button quickBtn;
    [SerializeField] private Button joinBtn;
    [SerializeField] private TMP_InputField codeInput;

    [Header("Lobby List")]
    [SerializeField] private Button refreshBtn;
    [SerializeField] private Transform lobbyListContainer;
    [SerializeField] private Button lobbyButtonPrefab;

    private void Start()
    {
        createBtn.onClick.AddListener(() =>
            GameNetwork.Instance.CreateAndHostLobby()
        );
        quickBtn.onClick.AddListener(() =>
            GameNetwork.Instance.QuickJoinLobby()
        );
        joinBtn.onClick.AddListener(() =>
            GameNetwork.Instance.JoinByCodeLobby(codeInput.text)
        );

        refreshBtn.onClick.AddListener(() =>
            _ = RefreshLobbyList()
        );
    }

    /// <summary>
    /// Queries and displays available public lobbies.
    /// </summary>
    private async Task RefreshLobbyList()
    {
        // Clear old buttons
        foreach (Transform t in lobbyListContainer) Destroy(t.gameObject);

        var lobbies = await GameNetwork.Instance.ListLobbiesAsync(20);
        foreach (var lob in lobbies)
        {
            var btn = Instantiate(lobbyButtonPrefab, lobbyListContainer);
            btn.GetComponentInChildren<TMP_Text>().text = $"{lob.LobbyCode} ({lob.Players.Count}/{lob.MaxPlayers})";
            btn.onClick.AddListener(() =>
                GameNetwork.Instance.JoinByCodeLobby(lob.LobbyCode)
            );
        }
    }
}
