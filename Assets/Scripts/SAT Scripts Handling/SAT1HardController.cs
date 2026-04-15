using UnityEngine;
using System.Collections;

namespace SAT1Controller
{
    public class SAT1HardController : MonoBehaviour
    {
        [Header("Car Settings (HARD MODE)")]
        public float acceleration = 3500f;
        public float arcadeBoostForce = 4000f;
        public float maxSpeed = 70f;
        public float turnSpeed = 3f;
        public float driftFactor = 0.85f;
        public float driftBoost = 1.0f;
        public float brakeForce = 12000f;
        public float gripStrength = 3f;

        [Header("Input Smoothing (NEW)")]
        public float throttleResponse = 2f;
        public float brakeResponse = 2f;

        [Header("Impact Settings")]
        public float heavyStunDuration = 3f;

        [Header("Wheel Colliders")]
        public WheelCollider frontLeftWheel, frontRightWheel, rearLeftWheel, rearRightWheel;

        [Header("Wheel Mesh Transforms")]
        public Transform frontLeftTransform, frontRightTransform, rearLeftTransform, rearRightTransform;

        private Rigidbody rb;
        private bool isDrifting = false;
        private bool isBraking = false;
        private bool isStunned = false;

        // NEW smoothing variables
        private float currentTorque = 0f;
        private float currentBrakeForce = 0f;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.mass = 600f;
            rb.linearDamping = 0.015f;
            rb.angularDamping = 0.2f; // less stability
            rb.centerOfMass = new Vector3(0, -0.35f, 0);
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
                ResetBrakes();
                return;
            }

            float moveInput = Input.GetAxis("Vertical");
            float speedMultiplier = isDrifting ? driftBoost : 1f;

            // Throttle smoothing (NEW)
            float targetTorque = moveInput * acceleration * speedMultiplier;
            currentTorque = Mathf.Lerp(currentTorque, targetTorque, Time.fixedDeltaTime * throttleResponse);

            if (moveInput == 0 && !isBraking)
            {
                rearLeftWheel.brakeTorque = 200f;
                rearRightWheel.brakeTorque = 200f;
                rearLeftWheel.motorTorque = 0;
                rearRightWheel.motorTorque = 0;
            }
            else if (!isBraking && rb.linearVelocity.magnitude < maxSpeed)
            {
                ResetBrakes();

                rearLeftWheel.motorTorque = currentTorque;
                rearRightWheel.motorTorque = currentTorque;

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

            float steerAngle = steerInput * 25f;
            frontLeftWheel.steerAngle = steerAngle;
            frontRightWheel.steerAngle = steerAngle;

            if (rb.linearVelocity.magnitude > 5f)
            {
                rb.AddTorque(transform.up * steerInput * turnSpeed * 0.5f * rb.linearVelocity.magnitude);
            }
        }

        void ApplyArcadeGrip()
        {
            if (!isDrifting && (rearLeftWheel.isGrounded || rearRightWheel.isGrounded))
            {
                Vector3 forwardVelocity = transform.forward * Vector3.Dot(rb.linearVelocity, transform.forward);
                Vector3 verticalVelocity = Vector3.up * rb.linearVelocity.y;

                rb.linearVelocity = Vector3.Lerp(
                    rb.linearVelocity,
                    forwardVelocity + verticalVelocity,
                    Time.fixedDeltaTime * gripStrength
                );
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
                    Time.fixedDeltaTime * 3f
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

                // Brake delay (NEW)
                currentBrakeForce = Mathf.Lerp(currentBrakeForce, brakeForce, Time.fixedDeltaTime * brakeResponse);

                ApplyBrake(currentBrakeForce);

                // destabilize slightly
                rb.angularVelocity *= 0.97f;
            }
            else
            {
                isBraking = false;

                currentBrakeForce = Mathf.Lerp(currentBrakeForce, 0, Time.fixedDeltaTime * brakeResponse);
                ApplyBrake(currentBrakeForce);
            }
        }

        void ApplyBrake(float force)
        {
            frontLeftWheel.brakeTorque = force;
            frontRightWheel.brakeTorque = force;
            rearLeftWheel.brakeTorque = force;
            rearRightWheel.brakeTorque = force;
        }

        void ResetBrakes()
        {
            frontLeftWheel.brakeTorque = 0;
            frontRightWheel.brakeTorque = 0;
            rearLeftWheel.brakeTorque = 0;
            rearRightWheel.brakeTorque = 0;
        }

        void ApplyDownforce()
        {
            float speed = rb.linearVelocity.magnitude;
            rb.AddForce(-transform.up * speed * 40f);
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
                    rb.linearVelocity *= 0.7f;
                    rb.angularVelocity += Random.insideUnitSphere * 2f; // destabilize
                }
            }
            else if (other.gameObject.CompareTag("Obstacle"))
            {
                rb.linearVelocity *= 0.6f;
                rb.angularVelocity += Random.insideUnitSphere * 3f;
            }
        }

        IEnumerator StunRoutine(float stunDuration)
        {
            isStunned = true;
            rb.linearVelocity *= 0.2f;
            yield return new WaitForSeconds(stunDuration);
            isStunned = false;
        }
    }
}