using UnityEngine;

public class ForestManager : MonoBehaviour
{
    [Header("Forest Settings")]
    public GameObject[] treePrefabs; // Drag all 7 tree prefabs here!
    public int numberOfTrees = 1000;
    
    [Header("Spawn Boundaries")]
    public Transform mapCenterPoint; 
    public Vector2 mapBoundsSize = new Vector2(150f, 150f); // Match your Road Spawner red box!

    void Start()
    {
        GenerateForest();
    }

void GenerateForest()
    {
        if (treePrefabs.Length == 0 || mapCenterPoint == null || Terrain.activeTerrain == null) return;

        GameObject forestContainer = new GameObject("ProceduralForest");

        for (int i = 0; i < numberOfTrees; i++)
        {
            GameObject randomTree = treePrefabs[Random.Range(0, treePrefabs.Length)];

            float randomX = mapCenterPoint.position.x + Random.Range(-mapBoundsSize.x / 2f, mapBoundsSize.x / 2f);
            float randomZ = mapCenterPoint.position.z + Random.Range(-mapBoundsSize.y / 2f, mapBoundsSize.y / 2f);

            // MAGIC FIX: Ask the Terrain exactly what its height is here!
            float terrainY = Terrain.activeTerrain.SampleHeight(new Vector3(randomX, 0, randomZ)) 
                             + Terrain.activeTerrain.transform.position.y;

            Vector3 spawnPos = new Vector3(randomX, terrainY, randomZ);
            GameObject newTree = Instantiate(randomTree, spawnPos, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
            
            newTree.transform.SetParent(forestContainer.transform);
        }
    }
}