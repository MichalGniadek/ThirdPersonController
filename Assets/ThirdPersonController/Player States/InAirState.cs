using UnityEngine;

namespace ThirdPersonController
{
    [System.Serializable]
    public class InAirState : PlayerState
    {
        [SerializeField, Tooltip("Additional force applied downwards")]
        public float additionalGravity = 0f;
        [SerializeField, Tooltip("Force applied to rigidbody when moving in air")]
        float moveForce = 0f;
        [SerializeField, Tooltip("Maximum speed after which there's no force applied")]
        float maxSpeed = 0f;
        [SerializeField, Tooltip("Speed of character rotation")]
        float rotationSpeed = 0f;
        [Space]
        [SerializeField, Tooltip("Number of additional airjumps"), Min(0)]
        int numberOfAirJumps = 0;
        [SerializeField, Tooltip("Force applied vertically to rigidbody when jumping")]
        float airJumpForce = 0f;

        int currentAirJumps = 0;

        public override PlayerState Process(Vector3 inputWorldDirection)
        {
            // So we don't detect ground immediately after jumping
            if (movement.TimeSinceStateChange > 0.1f && movement.OnGround())
            {
                movement.animator.CrossFade("Land", 0.1f);
                return movement.walkingState;
            }

            if (Input.GetKeyDown(KeyCode.E)) return movement.dashState;

            if (currentAirJumps > 0 && Input.GetKeyDown(KeyCode.Space))
            {
                movement.animator.CrossFade("Jump", 0.1f);
                currentAirJumps--;
                movement.rigidbody.AddForce(Vector3.up * airJumpForce);
            }
            return this;
        }

        public override void FixedProcess(Vector3 inputWorldDirection)
        {
            movement.rigidbody.AddForce(Vector3.down * additionalGravity);

            if (inputWorldDirection.sqrMagnitude > 0.05f &&
                movement.rigidbody.velocity.Horizontal().magnitude < maxSpeed)
            {
                // Movement
                movement.rigidbody.AddForce(inputWorldDirection.normalized * moveForce);

                // Rotation
                float targetAngle = Mathf.Rad2Deg *
                    Mathf.Atan2(inputWorldDirection.x, inputWorldDirection.z);

                float deltaAngle =
                    Mathf.DeltaAngle(movement.transform.eulerAngles.y, targetAngle);

                if (Mathf.Abs(deltaAngle) > 3f)
                {
                    float angleDirection = Mathf.Sign(deltaAngle);
                    movement.rigidbody.AddTorque(0f, angleDirection * rotationSpeed, 0f);
                }
            }
        }

        protected override void EnterImpl()
        {
            currentAirJumps = numberOfAirJumps;
        }

        protected override void ExitImpl() { }
    }
}