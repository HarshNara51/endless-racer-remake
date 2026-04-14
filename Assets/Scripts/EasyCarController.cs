using UnityEngine;
using TMPro;
using System.Collections;

public class EasyCarController : MonoBehaviour
{
    [Header("Engine Specs")]
    public float maxSpeed = 40f;        
    public float acceleration = 15f;    
    public float friction = 5f;        
    public float brakePower = 20f;      

    [Header("Handling")]
    public float turnSpeed = 100f;      
    public float gravity = 20f;         
    public float stickToRoadForce = 10f;
   
    [Header("Suspension")]
    public float rideHeightOffset = 0.5f;
    public float raycastLength = 3.0f;    

    [Header("UI")]
    public TMP_Text speedometerText;

    private float currentSpeed = 0f;
    private float verticalVelocity = 0f;

    private bool isSlowed = false;

    void Update()
    {
        HandleEngine();
        HandleSteering();
        ApplyPhysics();
        UpdateUI();
    }

    void HandleEngine()
    {
        if (isSlowed)
            return; // 🚨 NO acceleration allowed during slowdown

        float gasInput = Input.GetAxis("Vertical");

        if (gasInput > 0) currentSpeed += acceleration * gasInput * Time.deltaTime;
        else if (gasInput < 0) currentSpeed += brakePower * gasInput * Time.deltaTime;
        else
        {
            if (currentSpeed > 0) currentSpeed -= friction * Time.deltaTime;
            else if (currentSpeed < 0) currentSpeed += friction * Time.deltaTime;

            if (Mathf.Abs(currentSpeed) < 0.5f) currentSpeed = 0;
        }

        currentSpeed = Mathf.Clamp(currentSpeed, -10f, maxSpeed);
    }

    void HandleSteering()
    {
        if (Mathf.Abs(currentSpeed) > 0.1f)
        {
            float turnInput = Input.GetAxis("Horizontal");
            float direction = currentSpeed > 0 ? 1 : -1;
            transform.Rotate(Vector3.up * turnInput * turnSpeed * Time.deltaTime * direction);
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

    void UpdateUI()
    {
        if (speedometerText != null)
            speedometerText.text = Mathf.RoundToInt(currentSpeed).ToString() + " MPH";
    }

    // 🚨 REAL STOP SYSTEM
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle"))
        {
            StopCarHard();
        }
    }

    void StopCarHard()
    {
        currentSpeed = 0f;
        StartCoroutine(SlowdownRoutine());
    }

    IEnumerator SlowdownRoutine()
    {
        isSlowed = true;

        yield return new WaitForSeconds(2f);

        isSlowed = false;
    }
}