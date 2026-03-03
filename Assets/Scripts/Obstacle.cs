using UnityEngine;
using UnityEngine.SceneManagement;

public class Obstacle : MonoBehaviour
{
    [Header("Damage Settings")]
    public int baseDamage = 10;

    // 1 = Cone (slow)
    // 2 = Truck (restart)
    public int obstacleType;

    [Header("Yeet Settings (Type 1)")]
    public float sideForce = 8f;
    public float upwardForce = 3f;
    public float spinForce = 5f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Collision(GameObject player)
    {
        Rigidbody playerRb = player.GetComponent<Rigidbody>();

        Debug.Log("Collided");

        if (obstacleType == 1)
        {
            // Slow player instantly
            PlayerCollisionHandler playerHandler = player.GetComponent<PlayerCollisionHandler>();

            if (playerHandler != null)
            {
                playerHandler.Stun(1f);
            }

            if (rb != null)
            {
                rb.isKinematic = false;

                Vector3 sideDirection = transform.right;
                Vector3 forceDirection = (sideDirection * sideForce) + (Vector3.up * upwardForce);

                rb.AddForce(forceDirection, ForceMode.Impulse);
                rb.AddTorque(Random.insideUnitSphere * spinForce, ForceMode.Impulse);
            }
        }

        else if (obstacleType == 2)
        {
            // Restart game
            Time.timeScale = 1f;
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
    }
}