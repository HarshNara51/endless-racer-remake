using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro; 

public class MainMenuManager : MonoBehaviour
{
    [Header("Splash Elements")]
    public GameObject splashPanel;
    public TextMeshProUGUI pressAnyKeyText; 
    public float blinkSpeed = 2.0f;

    [Header("Menu Panels (NEW)")]
    public GameObject levelSelectPanel;      // Drag your new LevelSelectPanel here!
    public GameObject difficultySelectPanel; // Drag your new DifficultySelectPanel here!
    
    [Header("Transition Settings")]
    public CanvasGroup faderGroup; 
    public float fadeDuration = 0.5f; // Slightly faster for a snappier menu feel!

    private string selectedLevel = ""; 
    private bool isAtSplash = true;

    void Start()
    {
        // Ensure the menu starts in the correct state
        if (splashPanel != null) splashPanel.SetActive(true);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (difficultySelectPanel != null) difficultySelectPanel.SetActive(false);
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
                isAtSplash = false;
                StartCoroutine(TransitionPanels(splashPanel, levelSelectPanel));
            }
        }
    }

    // --- STEP 1: PLAYER SELECTS THE MAP ---
    public void SelectLevel(string sceneName)
    {
        selectedLevel = sceneName;
        Debug.Log("📍 [MAIN MENU] Map Selected: " + sceneName);
        
        // Fade out Level Select, Fade in Difficulty Select
        StartCoroutine(TransitionPanels(levelSelectPanel, difficultySelectPanel));
    }

    // --- STEP 2: PLAYER SELECTS DIFFICULTY & LAUNCHES ---
    public void SetHardModeAndStart(bool isHard)
    {
        if (string.IsNullOrEmpty(selectedLevel)) return; 

        // Apply setting to your singleton
        if (GameSettings.Instance != null)
        {
            GameSettings.Instance.isHardMode = isHard;
            Debug.Log("🏁 [MAIN MENU] Difficulty locked: " + (isHard ? "HARD" : "EASY"));
        }

        // Fade out to black, then load the scene!
        StartCoroutine(FadeAndLoadScene(selectedLevel));
    }

    // --- TRANSITION LOGIC ---
    IEnumerator TransitionPanels(GameObject panelToHide, GameObject panelToShow)
    {
        // Fade to black
        yield return StartCoroutine(Fade(0, 1));

        // Swap panels while screen is dark
        panelToHide.SetActive(false);
        panelToShow.SetActive(true);

        // Fade back in to see the new buttons
        yield return StartCoroutine(Fade(1, 0));
    }

    IEnumerator FadeAndLoadScene(string sceneName)
    {
        // Fade to black completely
        yield return StartCoroutine(Fade(0, 1));
        
        // Load the actual game level
        SceneManager.LoadScene(sceneName);
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