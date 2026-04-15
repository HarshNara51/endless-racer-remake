using UnityEngine;
using System.Collections;
using TMPro; // 🔥 Added for the UI Speedometer!

namespace SAT1Controller
{
    public class SAT1EasyController : MonoBehaviour
    {
        [Header("Car Settings (EASY MODE)")]
        public float acceleration = 4000f;
        public float arcadeBoostForce = 8000f; 
        public float maxSpeed = 55f; 
        public float turnSpeed = 4f; 
        public float driftFactor = 0.95f; 
        public float driftBoost = 1.1f; 
        public float brakeForce = 8000f; 
        public float gripStrength = 10f; 

        [Header("Impact Settings")]
        public float heavyStunDuration = 3f; 

        [Header("UI Elements")]
        public TextMeshProUGUI speedText; // 🔥 Drag your Canvas Speed text here!

        [Header("Wheel Colliders")]
        public WheelCollider frontLeftWheel, frontRightWheel, rearLeftWheel, rearRightWheel;

        [Header("Wheel Mesh Transforms")]
        public Transform frontLeftTransform, frontRightTransform, rearLeftTransform, rearRightTransform;

        private Rigidbody rb;
        private bool isDrifting = false;
        private bool isBraking = false;
        private bool isStunned = false; 

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.mass = 500f;
            rb.linearDamping = 0.02f; 
            rb.angularDamping = 0.5f; 
            rb.centerOfMass = new Vector3(0, -0.4f, 0); 
        }

        void Update()
        {
            // 🔥 The Speedometer Update!
            if (speedText != null && rb != null)
            {
                // Multiply by 3.6 to convert Unity's m/s into KM/H
                int displaySpeed = Mathf.RoundToInt(rb.linearVelocity.magnitude * 2.237f);
                speedText.text = "Speed: " + displaySpeed.ToString();
            }
        }

        void FixedUpdate()
        {
            HandleAcceleration();
            HandleSteering();
            HandleDrifting();
            HandleBraking();
            ApplyArcadeGrip(); 
            ApplyDownforce();
            UpdateWheelTransforms();
        }

        void HandleAcceleration()
        {
            if (isStunned)
            {
                rearLeftWheel.motorTorque = 0;
                rearRightWheel.motorTorque = 0;
                frontLeftWheel.brakeTorque = 0;
                frontRightWheel.brakeTorque = 0;
                rearLeftWheel.brakeTorque = 0; 
                rearRightWheel.brakeTorque = 0;
                return; 
            }

            float moveInput = Input.GetAxis("Vertical");
            float speedMultiplier = isDrifting ? driftBoost : 1f;

            if (moveInput == 0 && !isBraking)
            {
                rearLeftWheel.brakeTorque = 300f; 
                rearRightWheel.brakeTorque = 300f;
                rearLeftWheel.motorTorque = 0;
                rearRightWheel.motorTorque = 0;
            }
            else if (!isBraking && rb.linearVelocity.magnitude < maxSpeed)
            {
                frontLeftWheel.brakeTorque = 0;
                frontRightWheel.brakeTorque = 0;
                rearLeftWheel.brakeTorque = 0; 
                rearRightWheel.brakeTorque = 0;
                
                rearLeftWheel.motorTorque = moveInput * acceleration * speedMultiplier;
                rearRightWheel.motorTorque = moveInput * acceleration * speedMultiplier;

                if (rearLeftWheel.isGrounded || rearRightWheel.isGrounded)
                {
                    rb.AddForce(transform.forward * moveInput * arcadeBoostForce * speedMultiplier);
                }
            }
            else
            {
                rearLeftWheel.motorTorque = 0;
                rearRightWheel.motorTorque = 0;
            }
        }

        void HandleSteering()
        {
            float steerInput = Input.GetAxis("Horizontal");
            float steerAngle = steerInput * 35f;

            frontLeftWheel.steerAngle = steerAngle;
            frontRightWheel.steerAngle = steerAngle;

            if (rb.linearVelocity.magnitude > 5f)
            {
                rb.AddTorque(transform.up * steerInput * turnSpeed * rb.linearVelocity.magnitude);
            }
        }

        void ApplyArcadeGrip()
        {
            if (!isDrifting && (rearLeftWheel.isGrounded || rearRightWheel.isGrounded))
            {
                Vector3 forwardVelocity = transform.forward * Vector3.Dot(rb.linearVelocity, transform.forward);
                Vector3 verticalVelocity = Vector3.up * rb.linearVelocity.y;
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, forwardVelocity + verticalVelocity, Time.fixedDeltaTime * gripStrength);
            }
        }

        void HandleDrifting()
        {
            if (Input.GetKey(KeyCode.F))
            {
                isDrifting = true;
                rb.linearVelocity = Vector3.Lerp(
                    rb.linearVelocity,
                    transform.forward * rb.linearVelocity.magnitude * driftFactor,
                    Time.fixedDeltaTime * 5
                );
            }
            else
            {
                isDrifting = false;
            }
        }

        void HandleBraking()
        {
            if (isStunned) return; 

            if (Input.GetKey(KeyCode.Space))
            {
                isBraking = true;
                frontLeftWheel.brakeTorque = brakeForce;
                frontRightWheel.brakeTorque = brakeForce;
                rearLeftWheel.brakeTorque = brakeForce;
                rearRightWheel.brakeTorque = brakeForce;
            }
            else
            {
                isBraking = false;
                frontLeftWheel.brakeTorque = 0;
                frontRightWheel.brakeTorque = 0;
                rearLeftWheel.brakeTorque = 0;
                rearRightWheel.brakeTorque = 0;
            }
        }

        void ApplyDownforce()
        {
            float speed = rb.linearVelocity.magnitude;
            rb.AddForce(-transform.up * speed * 50f);
        }

        void UpdateWheelTransforms()
        {
            UpdateWheel(frontLeftWheel, frontLeftTransform);
            UpdateWheel(frontRightWheel, frontRightTransform);
            UpdateWheel(rearLeftWheel, rearLeftTransform);
            UpdateWheel(rearRightWheel, rearRightTransform);
        }

        void UpdateWheel(WheelCollider collider, Transform t)
        {
            Vector3 pos;
            Quaternion rot;
            collider.GetWorldPose(out pos, out rot);
            t.position = pos;
            t.rotation = rot;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!this.enabled) return;

            Obstacle hitObstacle = other.GetComponent<Obstacle>();

            if (hitObstacle != null)
            {
                if (hitObstacle.obstacleType == 2) 
                {
                    StartCoroutine(StunRoutine(heavyStunDuration)); 
                }
                else if (hitObstacle.obstacleType == 1) 
                {
                    rb.linearVelocity *= 0.6f; 
                }
            }
            else if (other.gameObject.CompareTag("Obstacle")) 
            {
                rb.linearVelocity *= 0.4f; 
            }
        }

        IEnumerator StunRoutine(float stunDuration)
        {
            isStunned = true; 
            rb.linearVelocity *= 0.25f; 
            yield return new WaitForSeconds(stunDuration); 
            isStunned = false; 
        }
    }
}