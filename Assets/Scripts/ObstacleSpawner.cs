using UnityEngine;

// Global difficulty tracker
public enum GameDifficulty { Easy, Hard }

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Difficulty State")]
    public GameDifficulty currentDifficulty;

    [Header("Obstacle Prefabs (Add ALL here)")]
    public GameObject[] obstaclePrefabs;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    [Header("Difficulty Settings - Spawn Chance")]
    [Range(0f, 1f)] public float easySpawnChance = 0.3f;
    [Range(0f, 1f)] public float hardSpawnChance = 0.7f;

    [Header("Ground Layer (IMPORTANT)")]
    public LayerMask groundLayer;

    void Start()
    {
        SpawnObstacles();
    }

    void SpawnObstacles()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;

        float currentSpawnChance = (currentDifficulty == GameDifficulty.Easy) ? easySpawnChance : hardSpawnChance;

        foreach (Transform point in spawnPoints)
        {
            if (Random.value < currentSpawnChance)
            {
                int randomIndex = Random.Range(0, obstaclePrefabs.Length);
                GameObject prefabToSpawn = obstaclePrefabs[randomIndex];

                // Spawn slightly above
                GameObject obj = Instantiate(prefabToSpawn, point.position + Vector3.up * 5f, point.rotation);

                // 🔥 FIX FLOATING USING LAYER MASK
                RaycastHit hit;
                if (Physics.Raycast(obj.transform.position, Vector3.down, out hit, 100f, groundLayer))
                {
                    obj.transform.position = hit.point;
                }

                obj.transform.SetParent(transform, true);

                // Optional movement script
                BulldozerMovement movement = obj.GetComponent<BulldozerMovement>();
                if (movement != null)
                {
                    movement.InitializeDifficulty(currentDifficulty);
                }
            }
        }
    }
}