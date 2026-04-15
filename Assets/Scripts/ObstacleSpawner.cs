using UnityEngine;

public enum GameDifficulty { Easy, Hard }

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Difficulty State")]
    public GameDifficulty currentDifficulty;

    [Header("Obstacles & Locations")]
    public GameObject[] obstaclePrefabs;
    public Transform[] spawnPoints;

    [Header("Spawn Chances")]
    [Range(0f, 1f)] public float easySpawnChance = 0.3f;
    [Range(0f, 1f)] public float hardSpawnChance = 0.7f;

    void Start()
    {
        SpawnObstacles(); 
    }

    void SpawnObstacles()
    {
        if (spawnPoints == null || spawnPoints.Length == 0) return;
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0) return;

        float spawnChance = (currentDifficulty == GameDifficulty.Easy) ? easySpawnChance : hardSpawnChance;

        foreach (Transform point in spawnPoints)
        {
            if (Random.value < spawnChance)
            {
                int randomIndex = Random.Range(0, obstaclePrefabs.Length);
                GameObject prefabToSpawn = obstaclePrefabs[randomIndex];

                // 🔥 THE FIX: Just place it perfectly on the marker!
                GameObject obj = Instantiate(prefabToSpawn, point.position, point.rotation);
                obj.transform.SetParent(transform, true);

                BulldozerMovement movement = obj.GetComponent<BulldozerMovement>();
                if (movement != null)
                {
                    movement.InitializeDifficulty(currentDifficulty);
                }
            }
        }
    }
}