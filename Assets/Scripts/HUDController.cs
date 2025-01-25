using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    private void Start() {

    }
    private void FixedUpdate() {
        SetBar(healthBarImgP1, partyController._player1Stats.currentHealth, partyController._player1Stats.maxHealth);
        SetBar(healthBarImgP2, partyController._player2Stats.currentHealth, partyController._player2Stats.maxHealth);

        SetBar(ammoBarImgP1, partyController._player1Stats.currentAmmo, partyController._player1Stats.maxAmmo);
        SetBar(ammoBarImgP2, partyController._player2Stats.currentAmmo, partyController._player2Stats.maxAmmo);

    }


    
    private void SetBar(Image bar, int currentValue, int maxValue)
    {
        // Convert 'currentValue' and 'maxValue' to floats so we don't do integer division
        float ratio = (float)currentValue / (float)maxValue;

        // Clamp the ratio to ensure it remains between 0 and 1
        ratio = Mathf.Clamp01(ratio);

        // Assign the ratio to the bar's fill amount
        bar.fillAmount = ratio;
    }





}
