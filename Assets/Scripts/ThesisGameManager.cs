using UnityEngine;
using TMPro;

public class ThesisGameManager : MonoBehaviour
{
    public static ThesisGameManager Instance;

    [Header("Simulation Settings")]
    public float simulationTime = 300f; // 300 seconds = 5 Minutes
    private bool isSimulationActive = true;
    
    [Tooltip("Match this to whatever multiplier you used in the car script for MPH!")]
    public float uiSpeedMultiplier = 2.237f; 

    [Header("Player Tracking")]
    public Transform playerCar;
    public Rigidbody carRb;
    private Vector3 lastCarPosition;
    
    [HideInInspector] public int totalObstaclesHit = 0;
    private float topSpeedReached = 0f;
    private float totalDistanceTraveled = 0f;

    [Header("UI Elements")]
    public TextMeshProUGUI timerText; // Put a Timer text on your screen!
    public GameObject simulationCompleteScreen; // Drag your old Game Over screen here!
    
    [Header("End Screen Text Slots")]
    public TextMeshProUGUI finalDistanceText;
    public TextMeshProUGUI finalSpeedText;
    public TextMeshProUGUI finalObstaclesText;

    void Awake()
    {
        // Standard Singleton setup so other scripts can talk to this easily
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (playerCar != null) lastCarPosition = playerCar.position;
        
        // Ensure the end screen is hidden when the game starts
        if (simulationCompleteScreen != null) simulationCompleteScreen.SetActive(false);
    }

    void Update()
    {
        if (!isSimulationActive) return;

        // Run the timer countdown
        simulationTime -= Time.deltaTime;
        UpdateTimerUI();

        // Check if time is up!
        if (simulationTime <= 0)
        {
            EndSimulation();
            return;
        }

        // If game is running, track the data
        TrackAnalytics();
    }

    void TrackAnalytics()
    {
        if (playerCar == null || carRb == null) return;

        // 1. Calculate Distance Traveled this frame
        float distanceThisFrame = Vector3.Distance(playerCar.position, lastCarPosition);
        totalDistanceTraveled += distanceThisFrame;
        lastCarPosition = playerCar.position;

        // 2. Track Top Speed
        float currentSpeedMph = carRb.linearVelocity.magnitude * uiSpeedMultiplier;
        if (currentSpeedMph > topSpeedReached)
        {
            topSpeedReached = currentSpeedMph;
        }
    }

    // Obstacle.cs will call this when a hit happens!
    public void RegisterObstacleHit()
    {
        if (isSimulationActive)
        {
            totalObstaclesHit++;
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            // Format time nicely as MM:SS
            int minutes = Mathf.FloorToInt(Mathf.Max(simulationTime, 0) / 60F);
            int seconds = Mathf.FloorToInt(Mathf.Max(simulationTime, 0) - minutes * 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    void EndSimulation()
    {
        isSimulationActive = false;
        
        // Stop the car by disabling the handling scripts
        MonoBehaviour easyScript = playerCar.GetComponent("SAT1EasyController") as MonoBehaviour;
        MonoBehaviour hardScript = playerCar.GetComponent("SAT1HardController") as MonoBehaviour;
        
        // Using string names because they are in a namespace
        if (easyScript != null) easyScript.enabled = false;
        if (hardScript != null) hardScript.enabled = false;

        // Populate the End Screen UI with the final data
        if (finalDistanceText != null) 
            finalDistanceText.text = "Distance: " + Mathf.RoundToInt(totalDistanceTraveled) + " m";
        if (finalSpeedText != null) 
            finalSpeedText.text = "Top Speed: " + Mathf.RoundToInt(topSpeedReached) + " MPH";
        if (finalObstaclesText != null) 
            finalObstaclesText.text = "Obstacles Hit: " + totalObstaclesHit;

        // Turn on the screen!
        if (simulationCompleteScreen != null) simulationCompleteScreen.SetActive(true);
    }
}