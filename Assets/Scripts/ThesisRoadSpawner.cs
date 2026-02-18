using System.Collections.Generic;
using UnityEngine;

public class ThesisRoadSpawner : MonoBehaviour
{
    [Header("Sequential Road Pattern (ORDER MATTERS)")]
    public List<GameObject> roadSequence;

    [Header("Manual Bounds Protection")]
    public Transform mapCenterPoint; 
    public Vector2 mapBoundsSize = new Vector2(150f, 150f); 
    
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

    void Awake()
    {
        if (enableDebugLogs) Debug.Log($"[RoadSpawner][{Time.time:F2}s] AWAKE");
    }

    void Start()
    {
        if (roadSequence == null || roadSequence.Count == 0) return;
        SpawnFirstTile();
        for (int i = 0; i < initialTiles; i++) SpawnNextTile();
    }

    void Update()
    {
        if (playerCar == null || previousExitPoint == null) return;

        if (!gameStarted)
        {
            if (Vector3.Distance(playerCar.position, Vector3.zero) > 3f) gameStarted = true;
            else return;
        }

        float dist = Vector3.Distance(playerCar.position, previousExitPoint.position);
        if (dist < spawnDistance) SpawnNextTile();
    }

    void SpawnFirstTile()
    {
        GameObject tile = Instantiate(roadSequence[0]);
        tile.transform.position = Vector3.zero;
        tile.transform.rotation = Quaternion.identity;

        sequenceIndex = 1;
        FinalizeTile(tile);
    }

    void SpawnNextTile()
    {
        if (sequenceIndex >= roadSequence.Count) sequenceIndex = 0;

        GameObject plannedPrefab = roadSequence[sequenceIndex];
        GameObject tile = Instantiate(plannedPrefab);
        AlignTile(tile);

        // STRIKE PROTOCOL: Commit to turning until we face the center safely!
        if (NeedsEmergencyTurn(tile))
        {
            if (enableDebugLogs) Debug.LogWarning("[SPAWN] Danger Zone! Forcing U-Turn towards center.");
            Destroy(tile); 

            GameObject turnPrefab = GetTurnTowardsCenter();
            tile = Instantiate(turnPrefab);
            AlignTile(tile);
        }
        else
        {
            // Only progress the sequence if the piece was safe
            sequenceIndex++;
        }

        FinalizeTile(tile);
        DestroyAllButLastTwo();
    }

    void FinalizeTile(GameObject tile)
    {
        tile.transform.SetParent(transform);
        activeTiles.Add(tile);
        previousExitPoint = GetChildRecursive(tile.transform, "ExitPoint");
    }

    void AlignTile(GameObject tile)
    {
        Transform entry = GetChildRecursive(tile.transform, "EntryPoint");
        if (entry == null || previousExitPoint == null) return;

        Quaternion rot = Quaternion.FromToRotation(entry.forward, previousExitPoint.forward);
        tile.transform.rotation = rot * tile.transform.rotation;

        Vector3 offset = previousExitPoint.position - entry.position;
        tile.transform.position += offset;
    }

    // ===================== UPGRADED BOUNDS CHECKING =====================

    bool NeedsEmergencyTurn(GameObject tile)
    {
        if (mapCenterPoint == null) return false;

        Transform exit = GetChildRecursive(tile.transform, "ExitPoint");
        Vector3 checkPos = exit != null ? exit.position : tile.transform.position;
        Vector3 center = mapCenterPoint.position;

        float minX = center.x - (mapBoundsSize.x / 2f);
        float maxX = center.x + (mapBoundsSize.x / 2f);
        float minZ = center.z - (mapBoundsSize.y / 2f);
        float maxZ = center.z + (mapBoundsSize.y / 2f);

        bool isOutsideWarningZone = (checkPos.x < minX || checkPos.x > maxX || checkPos.z < minZ || checkPos.z > maxZ);

        if (isOutsideWarningZone)
        {
            // We crossed the red line! Are we facing the center of the map yet?
            Vector3 directionToCenter = (center - checkPos).normalized;
            Vector3 tileForward = exit != null ? exit.forward : tile.transform.forward;

            // Dot Product checks alignment: 1 is perfectly facing, -1 is facing away
            float alignmentToCenter = Vector3.Dot(tileForward, directionToCenter);

            // If the road isn't pointing strongly towards the center, reject it!
            if (alignmentToCenter < 0.4f) 
            {
                return true; 
            }
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

    void DestroyAllButLastTwo()
    {
        while (activeTiles.Count > 2)
        {
            GameObject old = activeTiles[0];
            activeTiles.RemoveAt(0);
            Destroy(old);
        }
    }

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

    void OnDrawGizmos()
    {
        if (mapCenterPoint != null)
        {
            Gizmos.color = Color.red;
            Vector3 size3D = new Vector3(mapBoundsSize.x, 50f, mapBoundsSize.y);
            Gizmos.DrawWireCube(mapCenterPoint.position, size3D);
        }
    }
}