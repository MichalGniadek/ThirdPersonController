using UnityEngine;

namespace ThirdPersonController
{
    [System.Serializable]
    public class WalkingState : PlayerState
    {
        [SerializeField, Tooltip("Force applied to rigidbody when walking")]
        float walkForce = 0f;
        [SerializeField, Tooltip("Force applied to rigidbody when sprinting")]
        float sprintForce = 0f;
        [SerializeField, Tooltip("Force applied to rigidbody when crouching")]
        float crouchForce = 0f;
        [SerializeField, Tooltip("Dictates whether you slide or crouch")]
        float minSpeedForSlide = 0f;
        [SerializeField, Tooltip("Maximum speed after which there's no force applied")]
        float maxSpeed = 0f;
        [SerializeField, Tooltip("Speed of character rotation")]
        float rotationSpeed = 0f;
        [SerializeField, Tooltip("Force applied vertically to rigidbody when jumping")]
        float jumpForce = 0f;

        enum Mode { Walking, Sprinting, Crouching }
        Mode mode = Mode.Walking;

        float GetForce()
        {
            switch (mode)
            {
                case Mode.Sprinting: return sprintForce;
                case Mode.Crouching: return crouchForce;
                default: return walkForce;
            }
        }

        public override PlayerState Process(Vector3 inputWorldDirection)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (movement.HorizontalVelocity < minSpeedForSlide)
                    mode = Mode.Crouching;
                else return movement.slideState;
            }
            else if (Input.GetKey(KeyCode.LeftShift)) mode = Mode.Sprinting;
            else mode = Mode.Walking;

            if (Input.GetKeyDown(KeyCode.E)) return movement.dashState;
            if (Input.GetKeyDown(KeyCode.Q)) return movement.rollState;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                movement.rigidbody.AddForce(Vector3.up * jumpForce);
                return movement.inAirState;
            }

            movement.animator.SetFloat("WalkingSpeed", Mathf.Min(1f,
                movement.HorizontalVelocity / maxSpeed));

            return this;
        }

        public override void FixedProcess(Vector3 inputWorldDirection)
        {
            movement.rigidbody.AddForce(Vector3.down);

            if (inputWorldDirection.sqrMagnitude > 0.05f &&
                movement.HorizontalVelocity < maxSpeed)
            {
                // Movement
                movement.rigidbody.AddForce(inputWorldDirection.normalized * GetForce());

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

        protected override void EnterImpl() { }
        protected override void ExitImpl() { }
    }
}