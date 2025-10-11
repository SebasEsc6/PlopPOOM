using TMPro;
using UnityEngine;

public class LivesDropdownBinder : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;   
    [SerializeField] private int[] optionLives = { 1, 2, 3 }; 

    private void Awake()
    {
        if (!dropdown) dropdown = GetComponent<TMP_Dropdown>();
    }

    private void OnEnable()
    {
        if (dropdown) dropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    private void OnDisable()
    {
        if (dropdown) dropdown.onValueChanged.RemoveListener(OnDropdownChanged);
    }

    private void Start()
    {
        OnDropdownChanged(dropdown ? dropdown.value : 1);
    }

    public void OnDropdownChanged(int index)
    {
        int lives = (optionLives != null && index >= 0 && index < optionLives.Length)
            ? optionLives[index]
            : Mathf.Max(1, index + 1);

        MatchConfig.SetLives(lives);
        // Debug.Log($"Lives set to {lives}");
    }
}
