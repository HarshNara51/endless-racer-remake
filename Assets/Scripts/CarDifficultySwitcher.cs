using UnityEngine;
using UnityEngine.SceneManagement; // Added this to read the scene name!

public class CarDifficultySwitcher : MonoBehaviour
{
    [Header("Script References")]
    public MonoBehaviour easyScript; 
    public MonoBehaviour hardScript; 

    void Start()
    {
        // Get the name of the current level to confirm we are in the right place
        string currentLevelName = SceneManager.GetActiveScene().name;
        Debug.Log("🌍 [GAME LEVEL] Successfully loaded into scene: [" + currentLevelName + "]");

        if (GameSettings.Instance != null)
        {
            Debug.Log("⚙️ [GAME LEVEL] GameSettings found! Applying player choice. Hard Mode = " + GameSettings.Instance.isHardMode);

            if (GameSettings.Instance.isHardMode)
            {
                easyScript.enabled = false;
                hardScript.enabled = true;
                Debug.Log("✅ [SUCCESS] Thesis Hard Controller Activated! Mario Kart handling disabled.");
            }
            else
            {
                easyScript.enabled = true;
                hardScript.enabled = false;
                Debug.Log("✅ [SUCCESS] Easy Mario Kart Controller Activated! Thesis handling disabled.");
            }
        }
        else
        {
            // If you hit play directly in the Day/Night level without using the menu
            easyScript.enabled = true;
            hardScript.enabled = false;
            Debug.LogWarning("⚠️ [WARNING] No GameSettings found! Did you start from the Main Menu? Defaulting to Easy Mode.");
        }
    }
}