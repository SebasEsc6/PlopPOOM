using UnityEngine;
using TMPro;
using Unity.Netcode;

public class UIGameLoopManager : MonoBehaviour
{
    [Header("Countdown")]
    [SerializeField] private TextMeshProUGUI countdownText;

    [SerializeField] private GameLoopManager gameLoop;

    [SerializeField] private GameObject feedbackUI;
    private bool countdownEnded = false;

    private void Update()
    {
        if (gameLoop == null || !gameLoop.IsSpawned) return;

        UpdateCountdown();
    }

    void UpdateCountdown()
    {
        float time = gameLoop.countdownTimer.Value;

        if (time > 0.01f)
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

    public void ActiveFeedback()
    {
        feedbackUI.SetActive(true);
    }
}
