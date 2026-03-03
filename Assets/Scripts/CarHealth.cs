using UnityEngine;
using TMPro;

public class CarHealth : MonoBehaviour
{
    public int maxHP = 100;
    private int currentHP;

    public TMP_Text hpText;

    void Start()
    {
        currentHP = maxHP;
        UpdateUI();
    }

    void OnTriggerEnter(Collider other)
    {
        Obstacle obstacle = other.GetComponent<Obstacle>();

        if (obstacle != null)
        {
            TakeDamage(obstacle.damage);
            Debug.Log("Hit obstacle! HP now: " + currentHP);
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
        if (hpText != null)
            hpText.text = "HP: " + currentHP;
    }

    void GameOver()
    {
        Debug.Log("GAME OVER");
        Time.timeScale = 0f;
    }
}