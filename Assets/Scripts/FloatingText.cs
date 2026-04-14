using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 100f;
    public float lifetime = 2f;

    private TMP_Text text;
    private float timer = 0f;
    private Color startColor;

    void Start()
    {
        text = GetComponent<TMP_Text>();
        startColor = text.color;
    }

    void Update()
    {
        // Move upward
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // Fade out over time
        timer += Time.deltaTime;
        float alpha = Mathf.Lerp(1f, 0f, timer / lifetime);

        Color c = startColor;
        c.a = alpha;
        text.color = c;

        // Destroy after lifetime
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}