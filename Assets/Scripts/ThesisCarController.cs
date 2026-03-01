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
    public float turnSpeed = 70f;
    public float highSpeedSteerDropoff = 0.3f;
    public float steerResponsiveness = 5f;
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

    // ================= 3 GEAR AUDIO SYSTEM =================
    [Header("Engine Audio - 3 Gear System")]
    public AudioSource engineAudioSource;

    public AudioClip gear1Clip;
    public AudioClip gear2Clip;
    public AudioClip gear3Clip;

    [Header("Gear Speed Thresholds")]
    public float gear1MaxSpeed = 25f;
    public float gear2MaxSpeed = 55f;

    private int currentGear = 0;
    // =======================================================

    private float currentSpeed = 0f;
    private float steerLerp = 0f;
    private float wheelRotationAmount = 0f;
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

        if (engineAudioSource != null)
        {
            engineAudioSource.loop = true;
            engineAudioSource.playOnAwake = false;
        }
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
        HandleAudio();
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
            float dynamicTurnSpeed = turnSpeed * Mathf.Lerp(1.0f, highSpeedSteerDropoff, speedFactor);
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
        wheelRotationAmount += currentSpeed * wheelSpinSpeed * Time.deltaTime;
        float visualSteerAngle = steerLerp * maxSteerAngle;

        foreach (Transform wheel in frontWheels)
        {
            if (wheel == null) continue;
            wheel.localRotation = Quaternion.Euler(wheelRotationAmount, visualSteerAngle, 0);
        }

        foreach (Transform wheel in backWheels)
        {
            if (wheel == null) continue;
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

    // ================= 3 GEAR AUDIO LOGIC =================
    void HandleAudio()
    {
        if (engineAudioSource == null) return;

        float speed = Mathf.Abs(currentSpeed);
        int targetGear = 0;

        if (speed <= 0.1f)
        {
            engineAudioSource.Stop();
            currentGear = 0;
            return;
        }

        if (speed <= gear1MaxSpeed)
            targetGear = 1;
        else if (speed <= gear2MaxSpeed)
            targetGear = 2;
        else
            targetGear = 3;

        if (targetGear != currentGear)
        {
            currentGear = targetGear;

            switch (currentGear)
            {
                case 1:
                    engineAudioSource.clip = gear1Clip;
                    break;
                case 2:
                    engineAudioSource.clip = gear2Clip;
                    break;
                case 3:
                    engineAudioSource.clip = gear3Clip;
                    break;
            }

            if (engineAudioSource.clip != null)
                engineAudioSource.Play();
        }
    }
}