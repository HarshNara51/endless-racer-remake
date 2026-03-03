using UnityEngine;

// Global difficulty tracker (you can move this to your GameManager later)
public enum GameDifficulty { Easy, Hard }

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Difficulty State")]
    public GameDifficulty currentDifficulty;

    [Header("Obstacle Prefabs")]
    public GameObject conePrefab;
    public GameObject bulldozerPrefab;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Difficulty Settings - Spawn Chance")]
    [Range(0f, 1f)] public float easySpawnChance = 0.3f; // Lower density
    [Range(0f, 1f)] public float hardSpawnChance = 0.7f; // Higher density

    void Start()
    {
        SpawnObstacles();
    }

    void SpawnObstacles()
    {
        // Safety check to prevent silent errors if points aren't assigned
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        // Rule 3: Density changes based on difficulty
        float currentSpawnChance = (currentDifficulty == GameDifficulty.Easy) ? easySpawnChance : hardSpawnChance;

        foreach (Transform point in spawnPoints)
        {
            // Rule 2: Randomness ONLY decides IF an obstacle spawns...
            if (Random.value < currentSpawnChance)
            {
                // ...and WHETHER it is a cone or bulldozer (50/50 chance)
                GameObject prefabToSpawn = (Random.value > 0.5f) ? conePrefab : bulldozerPrefab;

                // Spawn the object at the marker's location
                GameObject obj = Instantiate(prefabToSpawn, point.position, point.rotation);
                
                // Parent to the ROAD, keeping world transform intact.
                // NOTE: We removed obj.transform.localScale = Vector3.one; to fix the scaling/squish bug!
                obj.transform.SetParent(transform, true); 

                // Pass the difficulty to the bulldozer so it knows how fast to move
                BulldozerMovement movement = obj.GetComponent<BulldozerMovement>();
                if (movement != null)
                {
                    movement.InitializeDifficulty(currentDifficulty);
                }
            }
        }
    }
}