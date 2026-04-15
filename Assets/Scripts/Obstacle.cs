using UnityEngine;

[RequireComponent(typeof(Rigidbody))] 
public class Obstacle : MonoBehaviour
{
    [Header("Core Settings")]
    public int obstacleType;
    public int baseDamage = 5;

    [Header("Physics Impact")]
    public float sideForce = 8f;
    public float upwardForce = 3f;
    public float spinForce = 5f;

    private Rigidbody rb;
    private Collider col; // 🔥 We need this to control the Trigger!

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        
        // Freeze them in place so they don't fall
        rb.useGravity = false;
        rb.isKinematic = true;

        // 🔥 THE BRICK WALL FIX: Force them to be triggers at start!
        // Now you will safely pass into them from EVERY angle.
        if (col != null) col.isTrigger = true; 

        AssignDamageBasedOnType();
    }

    void AssignDamageBasedOnType()
    {
        switch (obstacleType)
        {
            case 1: baseDamage = 5; break; 
            case 2: baseDamage = 15; break;
            case 3: baseDamage = 8; break; 
            case 4: case 5: baseDamage = 10; break; 
            case 6: case 7: case 8: case 9: baseDamage = 12; break; 
        }
    }

    public void Collision(GameObject player)
    {
        PlayerCollisionHandler playerHandler = player.GetComponent<PlayerCollisionHandler>();

        switch (obstacleType)
        {
            case 1: 
                if (playerHandler != null) playerHandler.Stun(1f);
                YeetObject(1f, 1f, player);
                break;
            case 2: 
                if (playerHandler != null) playerHandler.Stun(2f);
                break;
            case 3: 
            case 6: 
                if (playerHandler != null) playerHandler.Stun(1.2f);
                RollObject(player);
                break;
            case 4: 
            case 5: 
                if (playerHandler != null) playerHandler.Stun(1.2f);
                PushObject(2f, player);
                break;
            case 7: 
            case 8: 
            case 9: 
                if (playerHandler != null) playerHandler.Stun(1.5f);
                PushObject(2.5f, player);
                break;
        }

        if (HitUIManager.Instance != null)
        {
            string cleanName = gameObject.name.Replace("(Clone)", "").Trim();
            HitUIManager.Instance.RegisterHit(cleanName);
        }
    }

    // ---------- PHYSICS REACTIONS ----------

    // 🔥 THE NEW MAGIC METHOD
    void PrepareForImpact(GameObject player)
    {
        rb.isKinematic = false;
        rb.useGravity = true;

        if (col != null)
        {
            // 1. THE SINKING FIX: Make it solid so it bounces on the asphalt!
            col.isTrigger = false;

            // 2. THE EXPLOSION FIX: Tell this object to permanently ignore the car's physics.
            // This stops the car from flipping over when the object suddenly becomes solid!
            Collider[] playerColliders = player.GetComponentsInChildren<Collider>();
            foreach (Collider pCol in playerColliders)
            {
                Physics.IgnoreCollision(col, pCol);
            }
        }
    }

    void YeetObject(float sideMul, float upMul, GameObject player)
    {
        PrepareForImpact(player); // 🔥 Run the magic trick!
        
        Vector3 pushDirection = (transform.position - player.transform.position).normalized;
        pushDirection.y = 0.5f; // Tiny upward pop to clear the bumper
        Vector3 force = (pushDirection * sideForce * sideMul) + (Vector3.up * upwardForce * upMul);
        
        rb.AddForce(force, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * spinForce, ForceMode.Impulse);
    }

    void PushObject(float strength, GameObject player)
    {
        PrepareForImpact(player); // 🔥 Run the magic trick!
        
        Vector3 pushDirection = (transform.position - player.transform.position).normalized;
        pushDirection.y = 0.5f; // Tiny upward pop to clear the bumper
        Vector3 force = pushDirection * sideForce * strength;
        
        rb.AddForce(force, ForceMode.Impulse);
    }

    void RollObject(GameObject player)
    {
        PrepareForImpact(player); // 🔥 Run the magic trick!
        
        Vector3 pushDirection = (transform.position - player.transform.position).normalized;
        pushDirection.y = 0.2f;
        
        rb.AddForce(pushDirection * sideForce * 0.5f, ForceMode.Impulse);
        rb.AddTorque(transform.right * spinForce * 2f, ForceMode.Impulse);
    }
}