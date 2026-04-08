using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // Required for IEnumerator (Coroutines)

public class CarHealth : MonoBehaviour
{
    [Header("Health Settings")]
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

    private int vegetationContacts = 0;
    private int roadContacts = 0;
    private int roadLayer;

    // --- NEW: Grace Period Tracker ---
    private bool isGracePeriodActive = true;

    void Start()
    {
        currentHP = maxHP;
        roadLayer = LayerMask.NameToLayer("Road");

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = maxHP;
        }

        if (offRoadWarningText != null)
            offRoadWarningText.gameObject.SetActive(false);

        UpdateUI();

        // --- NEW: Start the 10-second grace period timer ---
        StartCoroutine(GracePeriodRoutine());
    }

    void Update()
    {
        HandleOffRoadState();
    }

    void OnTriggerEnter(Collider other)
    {
        Obstacle obstacle = other.GetComponent<Obstacle>();
        if (obstacle != null)
        {
            int damage = obstacle.baseDamage;

            if (GameSettings.Instance != null && GameSettings.Instance.isHardMode)
                damage *= 2;

            TakeDamage(damage);

            obstacle.Collision(gameObject);
            return;
        }

        if (other.CompareTag("Tree"))
            vegetationContacts++;

        if (other.gameObject.layer == roadLayer)
            roadContacts++;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Tree"))
        {
            vegetationContacts--;
            if (vegetationContacts < 0)
                vegetationContacts = 0;
        }

        if (other.gameObject.layer == roadLayer)
        {
            roadContacts--;
            if (roadContacts < 0)
                roadContacts = 0;
        }
    }

    void HandleOffRoadState()
    {
        bool isOffRoad = vegetationContacts > 0 && roadContacts == 0;

        // --- NEW: Override off-road damage if we are in the grace period ---
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
            float drain = easyDrainPerSecond;

            if (GameSettings.Instance != null && GameSettings.Instance.isHardMode)
                drain = hardDrainPerSecond;

            currentHP -= drain * Time.deltaTime;

            if (currentHP <= 0)
            {
                currentHP = 0;
                UpdateUI();
                GameOver();
                return;
            }

            UpdateUI();
        }
    }

    void TakeDamage(int amount)
    {
        currentHP -= amount;

        if (currentHP <= 0)
        {
            currentHP = 0;
            UpdateUI();
            GameOver();
            return;
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        float healthPercent = currentHP / maxHP;

        if (hpSlider != null)
            hpSlider.value = currentHP;

        if (hpPercentText != null)
            hpPercentText.text = Mathf.RoundToInt(healthPercent * 100f) + "%";

        if (fillImage != null)
        {
            if (healthPercent > 0.6f)
                fillImage.color = highHealthColor;
            else if (healthPercent > 0.3f)
                fillImage.color = midHealthColor;
            else
                fillImage.color = lowHealthColor;
        }
    }

    void GameOver()
    {
        GameOverManager manager = FindFirstObjectByType<GameOverManager>();
        if (manager != null)
            manager.ShowGameOver();
    }

    // --- NEW: The 10-second real-time grace period routine ---
    IEnumerator GracePeriodRoutine()
    {
        yield return new WaitForSecondsRealtime(12f);
        isGracePeriodActive = false;
    }
}