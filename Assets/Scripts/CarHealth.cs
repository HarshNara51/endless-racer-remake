using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CarHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHP = 100;
    private int currentHP;

    [Header("UI References")]
    public Slider hpSlider;
    public TMP_Text hpPercentText;
    public Image fillImage;

    [Header("Health Colors")]
    public Color highHealthColor = Color.green;
    public Color midHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;

    void Start()
    {
        currentHP = maxHP;

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = maxHP;
        }

        UpdateUI();
    }

    void OnTriggerEnter(Collider other)
    {
        Obstacle obstacle = other.GetComponent<Obstacle>();

        if (obstacle != null)
        {
            int damage = obstacle.baseDamage;

            // Hard mode = 2x damage
            if (GameSettings.Instance != null && GameSettings.Instance.isHardMode)
            {
                damage *= 2;
            }

            TakeDamage(damage);
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
        float healthPercent = (float)currentHP / maxHP;

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
        Debug.Log("GAME OVER");
        Time.timeScale = 0f;
    }
}