using UnityEngine;

public class ClickAudioManager : MonoBehaviour
{
    public AudioSource clickSource;
    public AudioClip clickClip;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (clickSource != null && clickClip != null)
                clickSource.PlayOneShot(clickClip);
        }
    }
}