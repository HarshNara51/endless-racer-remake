using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [Header("Damage Settings")]
    public int baseDamage = 5;

    public int obstacleType;

    [Header("Force Settings")]
    public float sideForce = 8f;
    public float upwardForce = 3f;
    public float spinForce = 5f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        switch (obstacleType)
        {
            case 1: baseDamage = 5; break;
            case 2: baseDamage = 15; break;
            case 3: baseDamage = 8; break;
            case 4: baseDamage = 10; break;
            case 5: baseDamage = 10; break;
            case 6: baseDamage = 12; break;
            case 7: baseDamage = 12; break;
            case 8: baseDamage = 12; break;
            case 9: baseDamage = 12; break;
        }
    }

    public void Collision(GameObject player)
    {
        PlayerCollisionHandler playerHandler = player.GetComponent<PlayerCollisionHandler>();

        switch (obstacleType)
        {
            // Cone
            case 1:
                if (playerHandler != null)
                    playerHandler.Stun(1f);

                YeetObject(1f, 1f);
                break;

            // Bulldozer
            case 2:
                if (playerHandler != null)
                    playerHandler.Stun(2f);
                break;

            // Barrel group
            case 3:
                if (playerHandler != null)
                    playerHandler.Stun(1.2f);

                RollObject();
                break;

            // Boxes (4 & 5 → same behavior)
            case 4:
            case 5:
                if (playerHandler != null)
                    playerHandler.Stun(1.2f);

                PushObject(2f); // 🔥 stronger push
                break;

            // Oil Drum
            case 6:
                if (playerHandler != null)
                    playerHandler.Stun(1.3f);

                RollObject();
                break;

            // Logs
            case 7:
            case 8:
            case 9:
                if (playerHandler != null)
                    playerHandler.Stun(1.5f);

                PushObject(2.5f);
                break;
        }

        // 🔥 CLEAN NAME (NO CLONE)
        if (HitUIManager.Instance != null)
        {
            string cleanName = gameObject.name.Replace("(Clone)", "").Trim();
            HitUIManager.Instance.RegisterHit(cleanName);
        }
    }

    // ---------- HELPERS ----------

    void YeetObject(float sideMul, float upMul)
    {
        if (rb == null) return;

        rb.isKinematic = false;

        Vector3 force = (transform.right * sideForce * sideMul) + (Vector3.up * upwardForce * upMul);
        rb.AddForce(force, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * spinForce, ForceMode.Impulse);
    }

    void PushObject(float strength)
    {
        if (rb == null) return;

        rb.isKinematic = false;

        Vector3 force = (transform.right + transform.forward * 0.5f) * sideForce * strength;
        rb.AddForce(force, ForceMode.Impulse);
    }

    void RollObject()
    {
        if (rb == null) return;

        rb.isKinematic = false;

        rb.AddTorque(transform.right * spinForce * 2f, ForceMode.Impulse);
        rb.AddForce(transform.right * sideForce * 0.5f, ForceMode.Impulse);
    }
}