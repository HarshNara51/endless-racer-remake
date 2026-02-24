using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro; // Using TextMeshPro for the best looking text

public class MainMenuManager : MonoBehaviour
{
    [Header("Splash Elements")]
    public GameObject splashPanel;
    public TextMeshProUGUI pressAnyKeyText; // The "Press Any Key" text object
    public float blinkSpeed = 2.0f;

    [Header("Menu Panels")]
    public GameObject menuPanel;
    
    [Header("Transition Settings")]
    public CanvasGroup faderGroup; // The black image with a Canvas Group
    public float fadeDuration = 0.8f;

    private string selectedLevel = ""; // Stores the map choice
    private bool isAtSplash = true;

    void Start()
    {
        // Ensure the menu starts in the correct state
        if (splashPanel != null) splashPanel.SetActive(true);
        if (menuPanel != null) menuPanel.SetActive(false);
        if (faderGroup != null) faderGroup.alpha = 0;
    }

    void Update()
    {
        if (isAtSplash)
        {
            // 1. Make the "Press Any Key" text pulse/blink
            if (pressAnyKeyText != null)
            {
                float alpha = (Mathf.Sin(Time.time * blinkSpeed) + 1.0f) / 2.0f;
                pressAnyKeyText.color = new Color(pressAnyKeyText.color.r, pressAnyKeyText.color.g, pressAnyKeyText.color.b, alpha);
            }

            // 2. Listen for any input to enter the menu
            if (Input.anyKeyDown)
            {
                StartCoroutine(TransitionToMenu());
            }
        }
    }

    // --- STEP 1: PLAYER SELECTS THE MAP ---
    public void SelectLevel(string sceneName)
    {
        selectedLevel = sceneName;
        Debug.Log("📍 [MAIN MENU] Map Selected: " + sceneName + ". Now choose difficulty to start!");
    }

    // --- STEP 2: PLAYER SELECTS DIFFICULTY & LAUNCHES ---
    public void SetHardModeAndStart(bool isHard)
    {
        if (string.IsNullOrEmpty(selectedLevel))
        {
            Debug.LogWarning("⚠️ [MAIN MENU] Select a Level (Day or Night) first!");
            return; 
        }

        // Apply setting to your singleton
        if (GameSettings.Instance != null)
        {
            GameSettings.Instance.isHardMode = isHard;
            Debug.Log("🏁 [MAIN MENU] Difficulty locked: " + (isHard ? "HARD" : "EASY"));
        }

        Debug.Log("🚀 [MAIN MENU] Launching Scene: " + selectedLevel);
        SceneManager.LoadScene(selectedLevel);
    }

    // --- TRANSITION LOGIC ---
    IEnumerator TransitionToMenu()
    {
        isAtSplash = false;
        
        // Fade to black
        yield return StartCoroutine(Fade(0, 1));

        // Swap panels while screen is dark
        splashPanel.SetActive(false);
        menuPanel.SetActive(true);

        // Fade back in to see the buttons
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