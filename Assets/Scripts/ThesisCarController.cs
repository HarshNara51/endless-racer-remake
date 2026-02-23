using UnityEngine;
using TMPro;

public class ThesisCarController : MonoBehaviour
{
    [Header("Engine Specs")]
    public float maxSpeed = 80f;        
    public float acceleration = 25f; // Slightly lowered for more "weight"
    public float friction = 5f;      // Lowered so the car rolls more
    public float brakePower = 40f;      

    [Header("Handling (Difficulty Tuned)")]
    public float turnSpeed = 90f;      
    public float highSpeedSteerDropoff = 0.5f; // How much steering you lose at max speed
    public float gravity = 20f;         
    public float stickToRoadForce = 10f; 
    
    [Header("Suspension")]
    public float rideHeightOffset = 0.5f; 
    public float raycastLength = 3.0f;    

    [Header("Visuals")]
    public Transform[] wheels;          
    public float wheelSpinSpeed = 100f;

    [Header("Detailed Lighting")]
    public MeshRenderer headlightMesh;
    public int headlightMatIndex = 0;
    public MeshRenderer brakeLightMesh;
    public int brakeLightMatIndex = 1; 
    public MeshRenderer leftSignalMesh;
    public int leftSignalMatIndex = 0;
    public MeshRenderer rightSignalMesh;
    public int rightSignalMatIndex = 0;
    public MeshRenderer reverseLightMesh;
    public int reverseLightMatIndex = 0;

    public float signalBlinkSpeed = 15f; 

    [ColorUsage(true, true)] public Color headlightOnColor = new Color(2f, 2f, 1.8f);
    [ColorUsage(true, true)] public Color tailLightIdleColor = new Color(0.5f, 0f, 0f);
    [ColorUsage(true, true)] public Color tailLightBrakeColor = new Color(4f, 0f, 0f);
    [ColorUsage(true, true)] public Color signalOffColor = new Color(0.2f, 0.1f, 0f);
    [ColorUsage(true, true)] public Color signalOnColor = new Color(4f, 1.5f, 0f);
    [ColorUsage(true, true)] public Color reverseLightOffColor = new Color(0.1f, 0.1f, 0.1f);
    [ColorUsage(true, true)] public Color reverseLightOnColor = new Color(3f, 3f, 3f);

    [Header("UI")]
    public TMP_Text speedometerText;

    private float currentSpeed = 0f;
    private float verticalVelocity = 0f; 
    private float steerLerp = 0f; // For smoother, "heavier" steering

    private Material headLightMat;
    private Material brakeMat;
    private Material leftSignalMat;
    private Material rightSignalMat;
    private Material reverseLightMat;

    void Start()
    {
        if (headlightMesh != null) headLightMat = headlightMesh.materials[headlightMatIndex];
        if (brakeLightMesh != null) brakeMat = brakeLightMesh.materials[brakeLightMatIndex];
        if (leftSignalMesh != null) leftSignalMat = leftSignalMesh.materials[leftSignalMatIndex];
        if (rightSignalMesh != null) rightSignalMat = rightSignalMesh.materials[rightSignalMatIndex];
        if (reverseLightMesh != null) reverseLightMat = reverseLightMesh.materials[reverseLightMatIndex];

        if (headLightMat != null) headLightMat.SetColor("_EmissionColor", headlightOnColor);
    }

    void Update()
    {
        HandleEngine();
        HandleSteering();
        ApplyPhysics();
        AnimateVisuals();
        HandleLighting(); 
        UpdateUI();
    }

    void HandleEngine()
    {
        float gasInput = Input.GetAxis("Vertical"); 

        if (gasInput > 0) currentSpeed += acceleration * gasInput * Time.deltaTime;
        else if (gasInput < 0) currentSpeed += brakePower * gasInput * Time.deltaTime;
        else
        {
            // Natural coasting - takes longer to stop now
            if (currentSpeed > 0) currentSpeed -= friction * Time.deltaTime;
            else if (currentSpeed < 0) currentSpeed += friction * Time.deltaTime;
            
            if(Mathf.Abs(currentSpeed) < 1f) currentSpeed = 0;
        }

        currentSpeed = Mathf.Clamp(currentSpeed, -30f, maxSpeed);
    }

    void HandleSteering()
    {
        if (Mathf.Abs(currentSpeed) > 0.1f)
        {
            float turnInput = Input.GetAxis("Horizontal"); 
            
            // --- DIFFICULTY LOGIC ---
            // 1. Understeer: Calculate how fast we are going relative to max speed
            float speedFactor = Mathf.Abs(currentSpeed) / maxSpeed;
            // Steering becomes less effective the faster you go
            float dynamicTurnSpeed = turnSpeed * Mathf.Lerp(1.0f, highSpeedSteerDropoff, speedFactor);
            
            // 2. Steering Lag: Makes the car feel "heavy" instead of a toy
            steerLerp = Mathf.Lerp(steerLerp, turnInput, Time.deltaTime * 5f);
            
            float direction = currentSpeed > 0 ? 1 : -1;
            transform.Rotate(Vector3.up * steerLerp * dynamicTurnSpeed * Time.deltaTime * direction);
        }
    }

    void ApplyPhysics()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + (Vector3.up * 1.0f); 

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, raycastLength))
        {
            Vector3 targetPosition = transform.position;
            targetPosition.y = hit.point.y + rideHeightOffset; 
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, stickToRoadForce * Time.deltaTime);
            verticalVelocity = 0; 
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
            transform.Translate(Vector3.up * verticalVelocity * Time.deltaTime, Space.World);
        }

        transform.Translate(transform.forward * currentSpeed * Time.deltaTime, Space.World);
    }

    void AnimateVisuals()
    {
        if (wheels != null)
        {
            float spin = currentSpeed * wheelSpinSpeed * Time.deltaTime;
            foreach (Transform wheel in wheels)
            {
                if(wheel != null) wheel.Rotate(Vector3.right, spin);
            }
        }
    }

    void HandleLighting()
    {
        float gasInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        if (brakeMat != null)
        {
            if (gasInput < 0) brakeMat.SetColor("_EmissionColor", tailLightBrakeColor);
            else brakeMat.SetColor("_EmissionColor", tailLightIdleColor);
        }

        bool isBlinking = Mathf.Sin(Time.time * signalBlinkSpeed) > 0;

        if (leftSignalMat != null)
        {
            if (turnInput < -0.1f && isBlinking) leftSignalMat.SetColor("_EmissionColor", signalOnColor);
            else leftSignalMat.SetColor("_EmissionColor", signalOffColor);
        }

        if (rightSignalMat != null)
        {
            if (turnInput > 0.1f && isBlinking) rightSignalMat.SetColor("_EmissionColor", signalOnColor);
            else rightSignalMat.SetColor("_EmissionColor", signalOffColor);
        }

        if (reverseLightMat != null)
        {
            if (currentSpeed < -0.1f) reverseLightMat.SetColor("_EmissionColor", reverseLightOnColor);
            else reverseLightMat.SetColor("_EmissionColor", reverseLightOffColor);
        }
    }

    void UpdateUI()
    {
        if (speedometerText != null)
            speedometerText.text = Mathf.RoundToInt(currentSpeed).ToString() + " MPH";
    }
}