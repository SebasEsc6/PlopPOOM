using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public AudioSource musicSource;
    public AudioClip introClip;
    public AudioClip loopClip;
    public Slider volumeSlider;

    void Start()
    {
        if (musicSource == null || introClip == null || loopClip == null)
        {
            Debug.LogError("Faltan referencias en MusicManager.");
            return;
        }

        StartCoroutine(PlayIntroThenLoop());

        if (volumeSlider != null)
        {
            volumeSlider.value = musicSource.volume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    IEnumerator PlayIntroThenLoop()
    {
        musicSource.clip = introClip;
        musicSource.loop = false;
        musicSource.Play();

        yield return new WaitForSeconds(introClip.length);

        musicSource.clip = loopClip;
        musicSource.loop = true;
        musicSource.Play();
    }

    void SetVolume(float value)
    {
        musicSource.volume = value;
    }
}
