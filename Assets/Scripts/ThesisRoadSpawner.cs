using System.Collections.Generic;
using UnityEngine;

public class ThesisRoadSpawner : MonoBehaviour
{
    [Header("Sequential Road Pattern (ORDER MATTERS)")]
    public List<GameObject> roadSequence;

    [Header("Manual Bounds Protection")]
    [Tooltip("Create an Empty GameObject in the exact center of your play area and drag it here")]
    public Transform mapCenterPoint; 
    [Tooltip("The X (Width) and Y (Length) size of your safe zone. You will see a RED BOX in the scene!")]
    public Vector2 mapBoundsSize = new Vector2(200f, 200f); 
    
    [Header("Emergency Steering Prefabs")]
    public GameObject leftTurnPrefab;
    public GameObject rightTurnPrefab;

    [Header("References")]
    public Transform playerCar;
    public Camera mainCamera;

    [Header("Generation Settings")]
    public int initialTiles = 1;
    public float spawnDistance = 15f;

    [Header("Debug")]
    public bool enableDebugLogs = true;

    // Runtime
    private List<GameObject> activeTiles = new List<GameObject>();
    private Transform previousExitPoint;
    private bool gameStarted = false;
    private int sequenceIndex = 0;

    // ===================== UNITY =====================

    void Awake()
    {
        if (enableDebugLogs) Debug.Log($"[RoadSpawner][{Time.time:F2}s] AWAKE");
    }

    void Start()
    {
        if (roadSequence == null || roadSequence.Count == 0)
        {
            Debug.LogError("[RoadSpawner] Road sequence is EMPTY!");
            return;
        }

        if (mapCenterPoint == null)
        {
            Debug.LogError("[RoadSpawner] Map Center Point is missing! Please create an Empty GameObject, place it in the middle of the map, and assign it.");
        }

        if (enableDebugLogs) Debug.Log($"[RoadSpawner] Sequence size = {roadSequence.Count}");

        SpawnFirstTile();

        for (int i = 0; i < initialTiles; i++)
            SpawnNextTile();
    }

    void Update()
    {
        if (playerCar == null || previousExitPoint == null)
            return;

        if (!gameStarted)
        {
            if (Vector3.Distance(playerCar.position, Vector3.zero) > 3f)
            {
                gameStarted = true;
                if (enableDebugLogs) Debug.Log($"[RoadSpawner][{Time.time:F2}s] Game started");
            }
            else
                return;
        }

        float dist = Vector3.Distance(playerCar.position, previousExitPoint.position);

        if (dist < spawnDistance)
        {
            SpawnNextTile();
        }
    }

    // ===================== SPAWN =====================

    void SpawnFirstTile()
    {
        GameObject tile = Instantiate(roadSequence[0]);
        tile.transform.position = Vector3.zero;
        tile.transform.rotation = Quaternion.identity;

        sequenceIndex = 1;
        FinalizeTile(tile);

        if (enableDebugLogs) Debug.Log($"[SPAWN][{Time.time:F2}s] FIRST → {tile.name}");
    }

    void SpawnNextTile()
    {
        if (sequenceIndex >= roadSequence.Count)
            sequenceIndex = 0;

        GameObject plannedPrefab = roadSequence[sequenceIndex];
        GameObject tile = Instantiate(plannedPrefab);
        AlignTile(tile);

        if (IsOutOfBounds(tile))
        {
            if (enableDebugLogs) Debug.LogWarning("[SPAWN] Out of bounds detected! Triggering emergency turn.");
            
            Destroy(tile); 

            GameObject turnPrefab = GetTurnTowardsCenter();
            
            tile = Instantiate(turnPrefab);
            AlignTile(tile);
        }
        else
        {
            sequenceIndex++;
        }

        FinalizeTile(tile);

        if (enableDebugLogs) Debug.Log($"[SPAWN][{Time.time:F2}s] NEXT → {tile.name}");

        DestroyAllButLastTwo();
    }

    void FinalizeTile(GameObject tile)
    {
        tile.transform.SetParent(transform);
        activeTiles.Add(tile);
        previousExitPoint = GetChildRecursive(tile.transform, "ExitPoint");
    }

    // ===================== ALIGNMENT =====================

    void AlignTile(GameObject tile)
    {
        Transform entry = GetChildRecursive(tile.transform, "EntryPoint");
        if (entry == null || previousExitPoint == null)
            return;

        Quaternion rot = Quaternion.FromToRotation(entry.forward, previousExitPoint.forward);
        tile.transform.rotation = rot * tile.transform.rotation;

        Vector3 offset = previousExitPoint.position - entry.position;
        tile.transform.position += offset;
    }

    // ===================== BOUNDS CHECKING =====================

    bool IsOutOfBounds(GameObject tile)
    {
        if (mapCenterPoint == null) return false;

        Transform exit = GetChildRecursive(tile.transform, "ExitPoint");
        Vector3 checkPos = exit != null ? exit.position : tile.transform.position;
        
        Vector3 center = mapCenterPoint.position;

        // Calculate the invisible walls based on your custom size
        float minX = center.x - (mapBoundsSize.x / 2f);
        float maxX = center.x + (mapBoundsSize.x / 2f);
        float minZ = center.z - (mapBoundsSize.y / 2f);
        float maxZ = center.z + (mapBoundsSize.y / 2f);

        if (checkPos.x < minX || checkPos.x > maxX || checkPos.z < minZ || checkPos.z > maxZ)
        {
            return true;
        }

        return false;
    }

    GameObject GetTurnTowardsCenter()
    {
        if (mapCenterPoint == null) return leftTurnPrefab; 

        Vector3 directionToCenter = (mapCenterPoint.position - previousExitPoint.position).normalized;
        float crossProductY = Vector3.Cross(previousExitPoint.forward, directionToCenter).y;

        if (crossProductY > 0) return rightTurnPrefab;
        else return leftTurnPrefab;
    }

    // ===================== HARD DESTROY =====================

    void DestroyAllButLastTwo()
    {
        while (activeTiles.Count > 2)
        {
            GameObject old = activeTiles[0];
            activeTiles.RemoveAt(0);
            Destroy(old);
        }
    }

    // ===================== UTIL =====================

    Transform GetChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform found = GetChildRecursive(child, name);
            if (found != null) return found;
        }
        return null;
    }

    // ===================== GIZMOS (THE MAGIC) =====================
    
    // This draws a red box in your Scene view so you can visually see the exact boundaries!
    void OnDrawGizmos()
    {
        if (mapCenterPoint != null)
        {
            Gizmos.color = Color.red;
            // Draw a wire cube representing the safe zone. (Height is arbitrary just so you can see it).
            Vector3 size3D = new Vector3(mapBoundsSize.x, 50f, mapBoundsSize.y);
            Gizmos.DrawWireCube(mapCenterPoint.position, size3D);
        }
    }
}