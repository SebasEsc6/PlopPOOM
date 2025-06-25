using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUIElement : MonoBehaviour
{
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image ammoBarFill;

    private NetworkStatsController stats;

    public void Initialize(NetworkStatsController statsController)
    {
        if (healthBarFill == null || ammoBarFill == null)
        {
            Debug.LogError($"[PlayerStatsUIElement] faltan referencias en {gameObject.name}", this);
            return;
        }

        stats = statsController;

        stats.OnHealthChanged += UpdateHealth;
        stats.OnAmmoChanged += UpdateAmmo;

        // Initialize UI values
        UpdateHealth(stats.CurrentHealth.Value);
        UpdateAmmo(stats.CurrentAmmo.Value);
    }

    public void Cleanup()
    {
        stats.OnHealthChanged -= UpdateHealth;
        stats.OnAmmoChanged -= UpdateAmmo;
    }

    private void UpdateHealth(int hp) => healthBarFill.fillAmount = (float)hp / stats.MaxHealth;
    private void UpdateAmmo(int ammo) => ammoBarFill.fillAmount = (float)ammo / stats.MaxAmmo;
}
