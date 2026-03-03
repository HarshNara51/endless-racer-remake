using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject pausePanel;

    [Header("Gameplay Audio (from UniversalAudioManager)")]
    public AudioSource gameplayBGM;
    public AudioSource gameplayAmbience;

    [Header("Pause Menu Music")]
    public AudioSource pauseMusic;

    private bool isPaused = false;
    private bool isGameOver = false;

    void Update()
    {
        if (isGameOver) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        pausePanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        // Stop gameplay audio
        if (gameplayBGM != null)
            gameplayBGM.Pause();

        if (gameplayAmbience != null)
            gameplayAmbience.Pause();

        // Play pause music
        if (pauseMusic != null)
            pauseMusic.Play();
    }

    public void Resume()
    {
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        // Stop pause music
        if (pauseMusic != null)
            pauseMusic.Stop();

        // Resume gameplay audio
        if (gameplayBGM != null)
            gameplayBGM.Play();

        if (gameplayAmbience != null)
            gameplayAmbience.Play();
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        // Stop pause music if playing
        if (pauseMusic != null)
            pauseMusic.Stop();

        SceneManager.LoadScene("Main_Menu");
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game");
    }

    public void DisablePause()
    {
        isGameOver = true;
        pausePanel.SetActive(false);
    }
}