using UnityEngine;
using TMPro;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIGameLoopManager : MonoBehaviour
{
    [Header("Countdown")]
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private GameLoopManager gameLoop;

    [Header("Feedback")]
    [SerializeField] private GameObject feedbackUI;
    [SerializeField] private TextMeshProUGUI nameTxt;
    [SerializeField] private Image winSprite;

    [Header("Sprites")]
    [SerializeField] private List<Sprite> winSprites;

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

    public void ActiveFeedback(PlayerDataNet netData)
    {
        feedbackUI.SetActive(true);
        nameTxt.text = netData.playerName.ToString();

        var sprite = FindSpriteByName(netData.playerName.ToString());
        if (sprite != null)
            winSprite.sprite = sprite;
        else
            Debug.LogWarning($"[UI] No sprite found for player: {netData.playerName}");
    }

    private Sprite FindSpriteByName(string name)
    {
        return winSprites.Find(s => s.name == name);
    }
}
