using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumenController : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource musicSource; // Assign the AudioSource component for the music
    public Slider volumeSlider;    // Assign the UI Slider component

    private const string VolumePrefKey = "MusicVolume"; // Key to save and load the volume setting

    void Start()
    {
        // Load the saved volume or set a default value (1f = 100%)
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
        musicSource.volume = savedVolume;

        // Configure the Slider with the saved value
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    // Method to update the volume
    public void SetVolume(float volume)
    {
        musicSource.volume = volume;

        // Save the volume for future sessions
        PlayerPrefs.SetFloat(VolumePrefKey, volume);
    }
}
