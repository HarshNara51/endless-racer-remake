using UnityEngine;
using TMPro;

public class ThesisCarController : MonoBehaviour
{
    [Header("Engine Specs")]
    public float maxSpeed = 80f;        
    public float acceleration = 25f; 
    public float autoSpeedIncrease = 0.2f; 
    public float friction = 5f;      
    public float brakePower = 35f;      
    public float handbrakePower = 60f; 

    [Header("Handling (Adjust these for difficulty!)")]
    public float turnSpeed = 70f;      // Lower = Harder
    public float highSpeedSteerDropoff = 0.3f; // Lower = Harder at high speeds
    public float steerResponsiveness = 5f; // Lower = "Heavier" steering feel
    public float maxSteerAngle = 30f; 
    public float gravity = 20f;         
    public float stickToRoadForce = 10f; 
    
    [Header("Visual Wheels")]
    public Transform[] frontWheels;  
    public Transform[] backWheels;   
    public float wheelSpinSpeed = 100f;
    public float raycastLength = 3.0f;
    public float rideHeightOffset = 0.5f;

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

    // ================= NEW: AUDIO SYSTEM =================
    [Header("Audio")]
    public AudioSource engineAudioSource;
    public float minEnginePitch = 0.8f;
    public float maxEnginePitch = 2.5f;
    // =====================================================

    private float currentSpeed = 0f;
    private float steerLerp = 0f;
    private float wheelRotationAmount = 0f; // Track total rotation for wheels
    private float verticalVelocity = 0f;
    private bool isGameOver = false;
    private bool isHandbraking = false;

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

        // ================= NEW: AUDIO SETUP =================
        if (engineAudioSource != null)
        {
            engineAudioSource.loop = true;
            if (!engineAudioSource.isPlaying) engineAudioSource.Play();
        }
        // ====================================================
    }

    void Update()
    {
        if (isGameOver) return;

        isHandbraking = Input.GetKey(KeyCode.Space);

        HandleInfiniteSpeed();
        HandleEngine();
        HandleSteering();
        ApplyPhysics();
        AnimateWheels();
        HandleLighting(); 
        UpdateUI();

        // ================= NEW: AUDIO UPDATE =================
        HandleAudio();
        // =====================================================
    }

    void HandleInfiniteSpeed()
    {
        maxSpeed += autoSpeedIncrease * Time.deltaTime; 
        currentSpeed += (autoSpeedIncrease * 0.5f) * Time.deltaTime;
    }

    void HandleEngine()
    {
        float gasInput = Input.GetAxis("Vertical"); 

        if (isHandbraking)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, handbrakePower * Time.deltaTime);
        }
        else if (gasInput < 0)
        {
            currentSpeed += brakePower * gasInput * Time.deltaTime;
        }
        else if (gasInput > 0)
        {
            currentSpeed += acceleration * gasInput * Time.deltaTime;
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, friction * Time.deltaTime);
        }

        currentSpeed = Mathf.Clamp(currentSpeed, -30f, maxSpeed);
    }

    void HandleSteering()
    {
        if (Mathf.Abs(currentSpeed) > 0.1f)
        {
            float turnInput = Input.GetAxis("Horizontal"); 
            float speedFactor = Mathf.Abs(currentSpeed) / maxSpeed;
            
            // Steering gets exponentially weaker at high speeds
            float dynamicTurnSpeed = turnSpeed * Mathf.Lerp(1.0f, highSpeedSteerDropoff, speedFactor);
            
            // How fast the steering "reacts" to key presses
            steerLerp = Mathf.Lerp(steerLerp, turnInput, Time.deltaTime * steerResponsiveness);
            
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

    void AnimateWheels()
    {
        // Track the cumulative rotation to keep spinning consistent
        wheelRotationAmount += currentSpeed * wheelSpinSpeed * Time.deltaTime;
        float visualSteerAngle = steerLerp * maxSteerAngle;

        foreach (Transform wheel in frontWheels)
        {
            if (wheel == null) continue;
            // First apply the steering (Y), then the accumulated spin (X)
            wheel.localRotation = Quaternion.Euler(wheelRotationAmount, visualSteerAngle, 0);
        }

        foreach (Transform wheel in backWheels)
        {
            if (wheel == null) continue;
            // Back wheels only need the spin
            wheel.localRotation = Quaternion.Euler(wheelRotationAmount, 0, 0);
        }
    }

    void HandleLighting()
    {
        float gasInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        if (brakeMat != null)
        {
            if (gasInput < 0 || isHandbraking) brakeMat.SetColor("_EmissionColor", tailLightBrakeColor);
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

    // ================= NEW: AUDIO LOGIC =================
    void HandleAudio()
    {
        if (engineAudioSource != null)
        {
            // Calculate how fast we are going relative to max speed (absolute value for reversing)
            float speedPercent = Mathf.Abs(currentSpeed) / maxSpeed;
            
            // Lerp the pitch between the minimum (idle) and maximum (top speed)
            engineAudioSource.pitch = Mathf.Lerp(minEnginePitch, maxEnginePitch, speedPercent);
        }
    }
    // ====================================================
}