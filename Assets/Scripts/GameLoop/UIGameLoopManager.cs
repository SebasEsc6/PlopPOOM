using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class UIGameLoopManager : MonoBehaviour
{
    [Header("Countdown")]
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Player Stats")]
    [SerializeField] private TextMeshProUGUI killsText;
    [SerializeField] private TextMeshProUGUI livesText;

   [SerializeField] private GameLoopManager gameLoop;
   private bool countdownEnded = false;

    private void Update()
    {
        if (gameLoop == null || !gameLoop.IsSpawned) return;

        UpdateCountdown();
        UpdateLocalPlayerStats();
    }

    void UpdateCountdown()
{
    float time = gameLoop.countdownTimer.Value;

    if (time > 0f)
    {
        countdownText.text = Mathf.CeilToInt(time).ToString();
    }
    else if (!countdownEnded)
    {
        countdownEnded = true;
        countdownText.text = "";
        countdownText.gameObject.SetActive(false);
    }
}

    void UpdateLocalPlayerStats()
    {
        if (gameLoop.TryGetPlayerStats(NetworkManager.Singleton.LocalClientId, out var stats))
        {
            killsText.text = $"Kills: {stats.kills}";
            livesText.text = $"Lives: {stats.lives}";
        }
    }
}
