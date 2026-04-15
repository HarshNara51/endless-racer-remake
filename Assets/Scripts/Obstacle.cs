using UnityEngine;

[RequireComponent(typeof(Rigidbody))] 
public class Obstacle : MonoBehaviour
{
    [Header("Core Settings")]
    public int obstacleType;

    [Header("Physics Impact")]
    public float sideForce = 8f;
    public float upwardForce = 3f;
    public float spinForce = 5f;

    private Rigidbody rb;
    private Collider col; 

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        
        rb.useGravity = false;
        rb.isKinematic = true;

        if (col != null) col.isTrigger = true; 
    }

    public void Collision(GameObject player)
    {
        // 1. Run the Physics Reactions
        switch (obstacleType)
        {
            case 1: 
                YeetObject(1f, 1f, player);
                break;
            case 2: 
                // Heavy object (Bulldozer) - physics handled entirely by the car bouncing off
                break;
            case 3: 
            case 6: 
                RollObject(player);
                break;
            case 4: 
            case 5: 
                PushObject(2f, player);
                break;
            case 7: 
            case 8: 
            case 9: 
                PushObject(2.5f, player);
                break;
        }

        // 2. Trigger the UI Pop-up
        if (HitUIManager.Instance != null)
        {
            string cleanName = gameObject.name.Replace("(Clone)", "").Trim();
            HitUIManager.Instance.RegisterHit(cleanName + "!");
        }

        // 3. Silently log the hit for the Thesis Experiment Data
        if (ThesisGameManager.Instance != null)
        {
            ThesisGameManager.Instance.RegisterObstacleHit();
        }
    }

    // ---------- PHYSICS REACTIONS (Phase Shift Included) ----------

    void PrepareForImpact(GameObject player)
    {
        rb.isKinematic = false;
        rb.useGravity = true;

        if (col != null)
        {
            Invoke("MakeSolid", 0.15f); // Phase shift to prevent car launching!

            Collider[] playerColliders = player.GetComponentsInChildren<Collider>();
            foreach (Collider pCol in playerColliders)
            {
                if (pCol.GetType() != typeof(WheelCollider))
                {
                    Physics.IgnoreCollision(col, pCol);
                }
            }
        }
    }

    void MakeSolid()
    {
        if (col != null) col.isTrigger = false;
    }

    void YeetObject(float sideMul, float upMul, GameObject player)
    {
        PrepareForImpact(player); 
        
        Vector3 pushDirection = (transform.position - player.transform.position).normalized;
        pushDirection.y = 0.5f; 
        Vector3 force = (pushDirection * sideForce * sideMul) + (Vector3.up * upwardForce * upMul);
        
        rb.AddForce(force, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * spinForce, ForceMode.Impulse);
    }

    void PushObject(float strength, GameObject player)
    {
        PrepareForImpact(player); 
        
        Vector3 pushDirection = (transform.position - player.transform.position).normalized;
        pushDirection.y = 0.5f; 
        Vector3 force = pushDirection * sideForce * strength;
        
        rb.AddForce(force, ForceMode.Impulse);
    }

    void RollObject(GameObject player)
    {
        PrepareForImpact(player); 
        
        Vector3 pushDirection = (transform.position - player.transform.position).normalized;
        pushDirection.y = 0.2f;
        
        rb.AddForce(pushDirection * sideForce * 0.5f, ForceMode.Impulse);
        rb.AddTorque(transform.right * spinForce * 2f, ForceMode.Impulse);
    }
}