using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
    public enum Difficulty { Easy, Hard }
    public Difficulty currentDifficulty;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            if (currentDifficulty == Difficulty.Easy)
            {
                // Stop momentum only
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            else if (currentDifficulty == Difficulty.Hard)
            {
                Debug.Log("Game Over");
                // Replace later with your GameOver() call
            }
        }
    }
}