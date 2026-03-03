using UnityEngine;

public class ObstacleCollision : MonoBehaviour
{
    public GameDifficulty currentDifficulty;

    void OnCollisionEnter(Collision collision)
    {
        // Rule 5: Solid colliders, collision always detected
        if (collision.gameObject.CompareTag("Player"))
        {
            if (currentDifficulty == GameDifficulty.Easy)
            {
                // Rule 3 (Easy): Non-fatal, car loses momentum
                Debug.Log("Hit Obstacle (EASY): Player slowed down!");
                
                // TODO: Link this to your player script
                // collision.gameObject.GetComponent<PlayerController>().LoseMomentum();
            }
            else if (currentDifficulty == GameDifficulty.Hard)
            {
                // Rule 3 (Hard): Instant Game Over
                Debug.Log("Hit Obstacle (HARD): Game Over!");
                
                // TODO: Link this to your GameManager
                // GameManager.Instance.TriggerGameOver();
            }
        }
    }
}