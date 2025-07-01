using UnityEngine;
using TMPro;

public class LobbyButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtLobbyName;
    [SerializeField] private TextMeshProUGUI txtPlayersCount;

    public void Setup(string lobbyName, int players, int max)
    {
        txtLobbyName.text = lobbyName;
        txtPlayersCount.text = $"{players}/{max}";
    }
}