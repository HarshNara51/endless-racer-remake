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
        if (treePrefabs.Length == 0 || mapCenterPoint == null) return;

        // Create an empty folder object in the hierarchy to keep things clean
        GameObject forestContainer = new GameObject("ProceduralForest");

        for (int i = 0; i < numberOfTrees; i++)
        {
            // Pick a random tree from your 7 options
            GameObject randomTree = treePrefabs[Random.Range(0, treePrefabs.Length)];

            // Pick a random X and Z inside your red box
            float randomX = mapCenterPoint.position.x + Random.Range(-mapBoundsSize.x / 2f, mapBoundsSize.x / 2f);
            float randomZ = mapCenterPoint.position.z + Random.Range(-mapBoundsSize.y / 2f, mapBoundsSize.y / 2f);

            // Raycast down to find the exact height of the terrain at that spot
            Vector3 rayStart = new Vector3(randomX, 1000f, randomZ);
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 2000f))
            {
                // Spawn the tree at the terrain height
                Vector3 spawnPos = hit.point;
                GameObject newTree = Instantiate(randomTree, spawnPos, Quaternion.Euler(0, Random.Range(0f, 360f), 0));
                
                // Put it in the folder
                newTree.transform.SetParent(forestContainer.transform);
            }
        }
    }
}