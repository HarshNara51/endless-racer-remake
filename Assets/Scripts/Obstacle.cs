using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [Header("Damage Settings")]
    public int baseDamage = 5; // This will now auto-set based on the type!

    // 1 = Cone (slow, -5 HP)
    // 2 = Bulldozer (heavy stun, -15 HP)
    public int obstacleType;

    [Header("Yeet Settings (Type 1)")]
    public float sideForce = 8f;
    public float upwardForce = 3f;
    public float spinForce = 5f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // --- NEW: Automatically set the exact health drain based on the obstacle type! ---
        if (obstacleType == 1)
        {
            baseDamage = 5;  // 5% health loss (out of 100 maxHP)
        }
        else if (obstacleType == 2)
        {
            baseDamage = 15; // 15% health loss
        }
    }

    public void Collision(GameObject player)
    {
        PlayerCollisionHandler playerHandler = player.GetComponent<PlayerCollisionHandler>();

        // Debug.Log("Collided with type: " + obstacleType);

        if (obstacleType == 1)
        {
            // --- CONE BEHAVIOR ---
            // Short stun (1 second)
            if (playerHandler != null)
            {
                playerHandler.Stun(1f);
            }

            // Yeet the cone into the air
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
            // --- BULLDOZER BEHAVIOR ---
            // Notice: The SceneManager code is completely gone! 
            
            // Heavy stun (2 seconds) to simulate a massive crash
            if (playerHandler != null)
            {
                playerHandler.Stun(2f);
            }
            
            // The 15% health reduction is automatically handled by your CarHealth.cs script!
        }
    }
}