using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("Selection State")]
    private string selectedLevel = ""; // Stores the map name until we are ready to launch

    [Header("Panels & Transitions")]
    public GameObject splashPanel;
    public GameObject menuPanel;
    public CanvasGroup faderGroup;
    public float fadeDuration = 0.5f;

    private bool isAtSplash = true;

    void Update()
    {
        if (isAtSplash && Input.anyKeyDown)
        {
            StartCoroutine(TransitionToMenu());
        }
    }

    // 1. CALL THIS ON DAY/NIGHT BUTTONS
    public void SelectLevel(string sceneName)
    {
        selectedLevel = sceneName;
        Debug.Log("📍 [MAIN MENU] Map Selected: " + sceneName + ". Ready to choose difficulty!");
        
        // Optional: You could add a visual highlight here so the player knows which is picked
    }

    // 2. CALL THIS ON EASY/HARD BUTTONS
    public void SetHardModeAndStart(bool isHard)
    {
        // First, check if they even picked a level yet
        if (string.IsNullOrEmpty(selectedLevel))
        {
            Debug.LogWarning("⚠️ [MAIN MENU] Please select a level (Day or Night) first!");
            return; 
        }

        // Set the difficulty globally
        GameSettings.Instance.isHardMode = isHard;
        Debug.Log("🏁 [MAIN MENU] Difficulty locked: " + (isHard ? "HARD" : "EASY"));

        // Launch the stored level
        Debug.Log("🚀 [MAIN MENU] Launching Game: " + selectedLevel);
        SceneManager.LoadScene(selectedLevel);
    }

    // --- Splash Transition Logic ---
    IEnumerator TransitionToMenu()
    {
        isAtSplash = false;
        yield return StartCoroutine(Fade(0, 1));
        splashPanel.SetActive(false);
        menuPanel.SetActive(true);
        yield return StartCoroutine(Fade(1, 0));
    }

    IEnumerator Fade(float start, float end)
    {
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            faderGroup.alpha = Mathf.Lerp(start, end, timer / fadeDuration);
            yield return null;
        }
        faderGroup.alpha = end;
    }
}