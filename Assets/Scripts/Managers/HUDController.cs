using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    [Header("Scripts References")]
    [SerializeField] private PartyController partyController;

    [Header("Health UI")]
    [SerializeField] private Image healthBarImgP1;
    [SerializeField] private Image healthBarImgP2;

    [Header("Ammo UI")]
    [SerializeField] private Image ammoBarImgP1;
    [SerializeField] private Image ammoBarImgP2;

    [Header("Kills Score")]
    [SerializeField] private TextMeshProUGUI txtKillsP1;
    [SerializeField] private TextMeshProUGUI txtKillsP2;
    [SerializeField] private TextMeshProUGUI txtLifesP1;
    [SerializeField] private TextMeshProUGUI txtLifesP2;

    [Header("Scene Manager")]
    [SerializeField] private string menuSceneName;

    private StatsController p1, p2;

    private void Start()
    {
        // Some objects initialize OnEnable order-dependent; ensure we’re subscribed
        TrySubscribe();
    }

    private void OnDisable()
    {
        if (partyController != null)
            partyController.OnAvatarReady -= HandleAvatarReady;

        p1 = null;
        p2 = null;
    }

    private void TrySubscribe()
    {
        if (partyController != null)
        {
            // Avoid double-subscription
            partyController.OnAvatarReady -= HandleAvatarReady;
            partyController.OnAvatarReady += HandleAvatarReady;
        }
    }

    // NOTE: your PartyController signature currently passes (int index, StatsController stats)
    private void HandleAvatarReady(int index, StatsController stats)
    {
        if (index == 0) p1 = stats; else p2 = stats;
        RefreshOnce();
    }

    // ✅ UI should update in Update with null-guards
    private void Update()
    {
        // Party may not exist yet on first frame(s) after reload
        if (!partyController) return;

        // Kills/Lifes don’t depend on p1/p2, but guard partyController anyway
        SetKillScore();

        // Bars only when we have stats
        if (p1 != null)
        {
            SetBar(healthBarImgP1, p1.currentHealth, p1.maxHealth);
            SetBar(ammoBarImgP1, p1.currentAmmo, p1.maxAmmo);
        }
        if (p2 != null)
        {
            SetBar(healthBarImgP2, p2.currentHealth, p2.maxHealth);
            SetBar(ammoBarImgP2, p2.currentAmmo, p2.maxAmmo);
        }
    }

    private void RefreshOnce()
    {
        if (p1 != null)
        {
            SetBar(healthBarImgP1, p1.currentHealth, p1.maxHealth);
            SetBar(ammoBarImgP1, p1.currentAmmo, p1.maxAmmo);
        }
        if (p2 != null)
        {
            SetBar(healthBarImgP2, p2.currentHealth, p2.maxHealth);
            SetBar(ammoBarImgP2, p2.currentAmmo, p2.maxAmmo);
        }
    }

    private void SetBar(Image bar, int currentValue, int maxValue)
    {
        if (!bar || maxValue <= 0) return;
        bar.fillAmount = Mathf.Clamp01((float)currentValue / maxValue);
    }

    private void SetKillScore()
    {
        if (!partyController) return;
        if (txtKillsP1) txtKillsP1.text = partyController.player1Kills.ToString();
        if (txtKillsP2) txtKillsP2.text = partyController.player2Kills.ToString();
        if (txtLifesP1) txtLifesP1.text = (3 - partyController.player2Kills).ToString();
        if (txtLifesP2) txtLifesP2.text = (3 - partyController.player1Kills).ToString();
    }

    public void Pause() => Time.timeScale = 0;
    public void Play() => Time.timeScale = 1;

    public void ReLoadScene()
    {
        Time.timeScale = 1; // set before reload
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(menuSceneName, LoadSceneMode.Single);
    }
}
