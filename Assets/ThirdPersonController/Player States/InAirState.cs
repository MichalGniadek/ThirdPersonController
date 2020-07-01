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
            if (movement.OnGround()) return movement.walkingState;

            if (Input.GetKeyDown(KeyCode.E)) return movement.dashState;

            if (currentAirJumps > 0 && Input.GetKeyDown(KeyCode.Space))
            {
                currentAirJumps--;
                movement.rb.AddForce(Vector3.up * airJumpForce);
            }

            movement.animator.SetFloat("WalkingSpeed", Mathf.Min(1f,
                movement.rb.velocity.Horizontal().magnitude / maxSpeed));

            movement.animator.SetBool("Landing", movement.Landing());

            return this;
        }

        public override void FixedProcess(Vector3 inputWorldDirection)
        {
            movement.rb.AddForce(Vector3.down * additionalGravity);

            if (inputWorldDirection.sqrMagnitude > 0.05f &&
                movement.rb.velocity.Horizontal().magnitude < maxSpeed)
            {
                // Movement
                movement.rb.AddForce(inputWorldDirection.normalized * moveForce);

                // Rotation
                float targetAngle = Mathf.Rad2Deg *
                    Mathf.Atan2(inputWorldDirection.x, inputWorldDirection.z);

                float deltaAngle =
                    Mathf.DeltaAngle(movement.transform.eulerAngles.y, targetAngle);

                if (Mathf.Abs(deltaAngle) > 3f)
                {
                    float angleDirection = Mathf.Sign(deltaAngle);
                    movement.rb.AddTorque(0f, angleDirection * rotationSpeed, 0f);
                }
            }
        }

        protected override void EnterImpl()
        {
            movement.animator.SetBool("InAir", true);
            currentAirJumps = numberOfAirJumps;
        }

        protected override void ExitImpl()
        {
            movement.animator.SetBool("InAir", false);
        }
    }
}