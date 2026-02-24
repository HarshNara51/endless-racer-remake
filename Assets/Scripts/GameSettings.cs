using UnityEngine;

public class GameSettings : MonoBehaviour
{
    // This allows other scripts to find this one easily
    public static GameSettings Instance;

    [Header("Player Choices")]
    public bool isHardMode = false;

    void Awake()
    {
        // This logic ensures only ONE GameSettings exists at a time
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // This keeps the object alive between scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }
}