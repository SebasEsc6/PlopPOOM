using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private TMP_Text codeLabel;
    [SerializeField] private TMP_Text playersText;
    [SerializeField] private Toggle publicToggle;
    [SerializeField] private Button startBtn;

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
        UpdateUI();
    }

    private void UpdateUI()
    {
        var lobby = GameNetwork.Instance.CurrentLobby;
        if (lobby == null) return;

        // show code
        codeLabel.text = $"Code: {lobby.LobbyCode}";

        // handle players safely
        if (lobby.Players != null && lobby.Players.Count > 0)
        {
            var lines = new List<string>();
            foreach (var p in lobby.Players)
            {
                // if p.Data is null or missing "name", fall back to p.Id
                if (p.Data != null && p.Data.ContainsKey("name")
                    && !string.IsNullOrEmpty(p.Data["name"].Value))
                {
                    lines.Add(p.Data["name"].Value);
                }
                else
                {
                    lines.Add(p.Id);                       // fallback
                }
            }
            playersText.text = string.Join("\n", lines);
        }
        else
        {
            playersText.text = "Waiting for players...";
        }

        publicToggle.SetIsOnWithoutNotify(!lobby.IsPrivate);
    }

}
