using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button createBtn;
    [SerializeField] private Button quickBtn;
    [SerializeField] private Button joinBtn;
    [SerializeField] private TMP_InputField codeInput;

    [SerializeField] private Transform lobbyListContainer;
    [SerializeField] private Button lobbyButtonPrefab;

    private async void Start()
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

        await RefreshLobbyList();
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
