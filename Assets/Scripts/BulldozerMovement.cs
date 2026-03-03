using UnityEngine;

public class BulldozerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Increase this in the Inspector to cover more of the road!")]
    public float moveDistance = 12f; 
    
    [Header("Difficulty Speeds")]
    public float easySpeed = 2f;
    public float hardSpeed = 5f;

    private float currentSpeed;
    private Vector3 startPosition;
    private Quaternion startRotation;

    void Start()
    {
        startPosition = transform.position;
        // Save the initial rotation so we know which way is "forward" down the road
        startRotation = transform.rotation; 
    }

    public void InitializeDifficulty(GameDifficulty difficulty)
    {
        currentSpeed = (difficulty == GameDifficulty.Easy) ? easySpeed : hardSpeed;
    }

 void Update()
    {
        // 1. Move the Bulldozer (Position)
        float timeValue = Time.time * currentSpeed;
        float offset = Mathf.Sin(timeValue) * moveDistance;
        transform.position = startPosition + new Vector3(offset, 0, 0); 
        
        // 2. Rotate the Bulldozer (Facing Direction)
        float direction = Mathf.Cos(timeValue); 
        
        if (direction > 0.01f)
        {
            // Moving Right -> Swapped to -90 (or 270) to put it in Drive!
            transform.rotation = startRotation * Quaternion.Euler(0, -90, 0);
        }
        else if (direction < -0.01f)
        {
            // Moving Left -> Swapped to 90 to put it in Drive!
            transform.rotation = startRotation * Quaternion.Euler(0, 90, 0);
        }
    }
}