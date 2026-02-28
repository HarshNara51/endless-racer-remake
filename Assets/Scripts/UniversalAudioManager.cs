using UnityEngine;

public class UniversalAudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource ambienceSource;

    void Start()
    {
        // Start the Background Music if a source and clip are assigned
        if (bgmSource != null && bgmSource.clip != null)
        {
            bgmSource.loop = true;
            bgmSource.Play();
        }

        // Start the Environment Ambience if a source and clip are assigned
        if (ambienceSource != null && ambienceSource.clip != null)
        {
            ambienceSource.loop = true;
            ambienceSource.Play();
        }
    }
}