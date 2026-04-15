using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; 

public class CarHealth : MonoBehaviour
{
    [Header("Off-Road Engine Settings")]
    public int maxHP = 100;
    private float currentHP;

    [Header("UI References")]
    public Slider hpSlider;
    public TMP_Text hpPercentText;
    public Image fillImage;

    [Header("Warning UI")]
    public TMP_Text offRoadWarningText;
    public float warningBlinkSpeed = 5f;

    [Header("Health Colors")]
    public Color highHealthColor = Color.green;
    public Color midHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;

    [Header("Off-Road Drain Settings")]
    public float easyDrainPerSecond = 3f;
    public float hardDrainPerSecond = 6f;

    // 🔥 THE FIX: Replaced buggy integers with bulletproof time trackers!
    private float lastVegetationTouch = -10f;
    private float lastRoadTouch = -10f;
    private int roadLayer;

    private bool isGracePeriodActive = true;
    private bool isStalled = false; 
    private Rigidbody rb;

    void Start()
    {
        currentHP = maxHP;
        roadLayer = LayerMask.NameToLayer("Road");
        rb = GetComponent<Rigidbody>();

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = maxHP;
        }

        if (offRoadWarningText != null)
            offRoadWarningText.gameObject.SetActive(false);

        UpdateUI();
        StartCoroutine(GracePeriodRoutine());
    }

    void Update()
    {
        if (!isStalled)
        {
            HandleOffRoadState();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Obstacles still trigger their physical hits instantly here
        Obstacle obstacle = other.GetComponent<Obstacle>();
        if (obstacle != null)
        {
            obstacle.Collision(gameObject);
            return;
        }
    }

    // 🔥 THE FIX: Constantly refreshes the timer as long as you are touching it
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Tree"))
        {
            lastVegetationTouch = Time.time;
        }
        
        if (other.gameObject.layer == roadLayer)
        {
            lastRoadTouch = Time.time;
        }
    }

    void HandleOffRoadState()
    {
        // 🔥 THE FIX: If we touched grass in the last 0.15s, and haven't touched a road in 0.15s... we are off-road!
        bool touchingVeg = (Time.time - lastVegetationTouch) < 0.15f;
        bool touchingRoad = (Time.time - lastRoadTouch) < 0.15f;

        bool isOffRoad = touchingVeg && !touchingRoad;

        if (isGracePeriodActive)
        {
            isOffRoad = false;
        }

        // Warning UI
        if (offRoadWarningText != null)
        {
            offRoadWarningText.gameObject.SetActive(isOffRoad);

            if (isOffRoad)
            {
                float alpha = Mathf.Abs(Mathf.Sin(Time.time * warningBlinkSpeed));
                Color c = offRoadWarningText.color;
                offRoadWarningText.color = new Color(c.r, c.g, c.b, alpha);
            }
        }

        // Drain HP
        if (isOffRoad)
        {
            float drain = GameSettings.Instance != null && GameSettings.Instance.isHardMode ? hardDrainPerSecond : easyDrainPerSecond;
            currentHP -= drain * Time.deltaTime;

            if (currentHP <= 0)
            {
                currentHP = 0;
                UpdateUI();
                StartCoroutine(EngineStallRoutine()); 
                return;
            }
            UpdateUI();
        }
        else if (currentHP < maxHP)
        {
            currentHP += (easyDrainPerSecond / 2f) * Time.deltaTime;
            if (currentHP > maxHP) currentHP = maxHP;
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        float healthPercent = currentHP / maxHP;

        if (hpSlider != null) hpSlider.value = currentHP;
        if (hpPercentText != null) hpPercentText.text = Mathf.RoundToInt(healthPercent * 100f) + "%";

        if (fillImage != null)
        {
            if (healthPercent > 0.6f) fillImage.color = highHealthColor;
            else if (healthPercent > 0.3f) fillImage.color = midHealthColor;
            else fillImage.color = lowHealthColor;
        }
    }

    IEnumerator EngineStallRoutine()
    {
        isStalled = true;

        MonoBehaviour easyScript = GetComponent("SAT1Controller.SAT1EasyController") as MonoBehaviour;
        MonoBehaviour hardScript = GetComponent("SAT1Controller.SAT1HardController") as MonoBehaviour;

        MonoBehaviour activeScript = null;
        if (easyScript != null && easyScript.enabled) activeScript = easyScript;
        if (hardScript != null && hardScript.enabled) activeScript = hardScript;

        if (activeScript != null) activeScript.enabled = false;

        if (rb != null) rb.linearVelocity = Vector3.zero;

        yield return new WaitForSeconds(4f);

        if (activeScript != null) activeScript.enabled = true;

        currentHP = maxHP * 0.5f;
        
        isGracePeriodActive = true; 
        isStalled = false;
        UpdateUI();

        yield return new WaitForSeconds(3f);
        
        isGracePeriodActive = false; 
    }

    IEnumerator GracePeriodRoutine()
    {
        yield return new WaitForSecondsRealtime(12f);
        isGracePeriodActive = false;
    }
}