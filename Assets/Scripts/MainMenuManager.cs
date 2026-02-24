using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void SetHardMode(bool isHard)
    {
        GameSettings.Instance.isHardMode = isHard;
        Debug.Log("🏁 [MAIN MENU] Difficulty toggled. Hard Mode is now: " + isHard);
    }

    public void LoadLevel(string sceneName)
    {
        Debug.Log("🚀 [MAIN MENU] Loading Level: [" + sceneName + "] | Difficulty locked in - Hard Mode: " + GameSettings.Instance.isHardMode);
        SceneManager.LoadScene(sceneName);
    }
}