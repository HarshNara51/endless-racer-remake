using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    [Header("Building Settings")]
    public GameObject[] buildingPrefabs; 
    public int numberOfBuildings = 1000;
    
    [Header("Spawn Boundaries")]
    public Transform mapCenterPoint; 
    public Vector2 mapBoundsSize = new Vector2(150f, 150f); 

    void Start()
    {
        GenerateBuildings();
    }

    void GenerateBuildings()
    {
        if (buildingPrefabs.Length == 0 || mapCenterPoint == null || Terrain.activeTerrain == null) return;

        GameObject buildingContainer = new GameObject("ProceduralCity");

        for (int i = 0; i < numberOfBuildings; i++)
        {
            GameObject randomBuilding = buildingPrefabs[Random.Range(0, buildingPrefabs.Length)];

            float randomX = mapCenterPoint.position.x + Random.Range(-mapBoundsSize.x / 2f, mapBoundsSize.x / 2f);
            float randomZ = mapCenterPoint.position.z + Random.Range(-mapBoundsSize.y / 2f, mapBoundsSize.y / 2f);

            // Ask the Terrain exactly what its height is here!
            float terrainY = Terrain.activeTerrain.SampleHeight(new Vector3(randomX, 0, randomZ)) 
                             + Terrain.activeTerrain.transform.position.y;

            Vector3 spawnPos = new Vector3(randomX, terrainY, randomZ);
            GameObject newBuilding = Instantiate(randomBuilding, spawnPos, Quaternion.Euler(0, Random.Range(0f, 360f), 0));

            // Random scale has been completely removed! Buildings stay their true prefab size.
            
            newBuilding.transform.SetParent(buildingContainer.transform);
        }
    }
}