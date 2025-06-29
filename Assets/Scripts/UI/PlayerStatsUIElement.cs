using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class PlayerStatsUIElement : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image ammoBarFill;
    [SerializeField] private TMP_Text killsText;
    [SerializeField] private TMP_Text livesText;

    private NetworkStatsController statsController;

    public void Bind(NetworkStatsController controller)
    {
        if (controller == null) throw new ArgumentNullException(nameof(controller));
        Unbind();

        statsController = controller;

        statsController.OnHealthChanged += UpdateHealthBar;
        statsController.OnAmmoChanged += UpdateAmmoBar;
        statsController.OnLivesChanged += UpdateLivesText;
        statsController.OnKillsChanged += UpdateKillsText;

        UpdateHealthBar(statsController.CurrentHealth.Value);
        UpdateAmmoBar(statsController.CurrentAmmo.Value);
        UpdateLivesText(statsController.Lives.Value);
        UpdateKillsText(statsController.Kills.Value);

    }

    public void Unbind()
    {
        if (statsController != null)
        {
            statsController.OnHealthChanged -= UpdateHealthBar;
            statsController.OnAmmoChanged -= UpdateAmmoBar;
            statsController.OnLivesChanged -= UpdateLivesText;
            statsController.OnKillsChanged -= UpdateKillsText;
            statsController = null;
        }
    }

    private void UpdateHealthBar(int hp) => healthBarFill.fillAmount = (float)hp / statsController.MaxHealth;
    private void UpdateAmmoBar(int ammo) => ammoBarFill.fillAmount = (float)ammo / statsController.MaxAmmo;
    private void UpdateLivesText(int lives) => livesText.text = lives.ToString();
    private void UpdateKillsText(int kills) => killsText.text = kills.ToString();

    private void OnDestroy()
    {
        Unbind();
    }
}
