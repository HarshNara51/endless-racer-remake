using System.Collections.Generic;
using UnityEngine;

public class ThesisRoadSpawner : MonoBehaviour
{
    [Header("Easy Mode Roads (WIDE)")]
    public List<GameObject> easyRoadSequence;
    public GameObject easyLeftTurnPrefab;
    public GameObject easyRightTurnPrefab;

    [Header("Hard Mode Roads (SLIM)")]
    public List<GameObject> hardRoadSequence;
    public GameObject hardLeftTurnPrefab;
    public GameObject hardRightTurnPrefab;

    [Header("Manual Bounds Protection")]
    public Transform mapCenterPoint; 
    public Vector2 mapBoundsSize = new Vector2(150f, 150f); 

    [Header("References")]
    public Transform playerCar;
    public Camera mainCamera;

    [Header("Generation Settings")]
    public int initialTiles = 1;
    public float spawnDistance = 15f;

    [Header("Destruction Settings")]
    public int maxActiveTiles = 4; 

    [Header("Debug")]
    public bool enableDebugLogs = true;

    // Runtime Tracking Variables
    private List<GameObject> activeRoadSequence;
    private GameObject activeLeftTurn;
    private GameObject activeRightTurn;
    
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
        // 🔥 THE MAGIC: Check the Singleton to see what difficulty the player chose!
        bool isHard = GameSettings.Instance != null && GameSettings.Instance.isHardMode;
        
        if (isHard)
        {
            activeRoadSequence = hardRoadSequence;
            activeLeftTurn = hardLeftTurnPrefab;
            activeRightTurn = hardRightTurnPrefab;
            if (enableDebugLogs) Debug.Log("[RoadSpawner] Loading HARD (Slim) Roads.");
        }
        else
        {
            activeRoadSequence = easyRoadSequence;
            activeLeftTurn = easyLeftTurnPrefab;
            activeRightTurn = easyRightTurnPrefab;
            if (enableDebugLogs) Debug.Log("[RoadSpawner] Loading EASY (Wide) Roads.");
        }

        if (activeRoadSequence == null || activeRoadSequence.Count == 0) 
        {
            Debug.LogError("⚠️ [RoadSpawner] Missing Road Sequence Prefabs!");
            return;
        }

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
        GameObject tile = Instantiate(activeRoadSequence[0]);
        tile.transform.position = Vector3.zero;
        tile.transform.rotation = Quaternion.identity;

        sequenceIndex = 1;
        FinalizeTile(tile);
    }

    void SpawnNextTile()
    {
        if (sequenceIndex >= activeRoadSequence.Count) sequenceIndex = 0;

        GameObject plannedPrefab = activeRoadSequence[sequenceIndex];
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
            sequenceIndex++; // Only progress sequence if it wasn't an emergency turn
        }

        FinalizeTile(tile);
        DestroyOldTiles();
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
            Vector3 directionToCenter = (center - checkPos).normalized;
            Vector3 tileForward = exit != null ? exit.forward : tile.transform.forward;

            float alignmentToCenter = Vector3.Dot(tileForward, directionToCenter);

            if (alignmentToCenter < 0.4f) 
            {
                return true; 
            }
        }

        return false;
    }

    GameObject GetTurnTowardsCenter()
    {
        if (mapCenterPoint == null) return activeLeftTurn; 

        Vector3 directionToCenter = (mapCenterPoint.position - previousExitPoint.position).normalized;
        float crossProductY = Vector3.Cross(previousExitPoint.forward, directionToCenter).y;

        if (crossProductY > 0) return activeRightTurn;
        else return activeLeftTurn;
    }

    void DestroyOldTiles()
    {
        while (activeTiles.Count > maxActiveTiles)
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