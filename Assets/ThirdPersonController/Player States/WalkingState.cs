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
                if (movement.HorizontalVelocity > minSpeedForSlide)
                {
                    return movement.slideState;
                }
                else if (mode != Mode.Crouching)
                {
                    mode = Mode.Crouching;
                    movement.animator.CrossFade("Crouch", 0.2f);
                }
            }
            else if (Input.GetKey(KeyCode.LeftShift)) mode = Mode.Sprinting;
            else
            {
                if (mode == Mode.Crouching)
                {
                    movement.animator.CrossFade("Walk", 0.2f);
                }
                mode = Mode.Walking;
            }

            if (Input.GetKeyDown(KeyCode.E)) return movement.dashState;
            if (Input.GetKeyDown(KeyCode.Q)) return movement.rollState;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                movement.animator.CrossFade("Jump", 0.1f);
                movement.rigidbody.AddForce(Vector3.up * jumpForce);
                return movement.inAirState;
            }

            movement.animator.SetFloat("WalkingSpeed", Mathf.Min(1f,
                movement.HorizontalVelocity / maxSpeed), 0.1f, Time.deltaTime);

            if (movement.inputWorldDirection.magnitude > 0)
            {
                float targetAngle = Mathf.Rad2Deg * Mathf.Atan2(
                    movement.inputWorldDirection.x,
                    movement.inputWorldDirection.z) - 90;

                movement.model.rotation = Quaternion.Euler(0, targetAngle, 0);
            }

            return this;
        }

        public override void FixedProcess(Vector3 velocityRelativeToCamera)
        {
            movement.rigidbody.AddForce(Vector3.down);

            // Counter movement
            if (velocityRelativeToCamera.x * movement.inputDirection.x <= 0)
            {
                movement.rigidbody.AddForce(movement.HorizontalDrag
                                            * velocityRelativeToCamera.x
                                            * -movement.CameraForward);
            }
            if (velocityRelativeToCamera.z * movement.inputDirection.z <= 0)
            {
                movement.rigidbody.AddForce(movement.HorizontalDrag
                                            * velocityRelativeToCamera.z
                                            * -movement.CameraRight);
            }

            if ((movement.inputDirection.x > 0 && velocityRelativeToCamera.x < maxSpeed)
            || (movement.inputDirection.x < 0 && velocityRelativeToCamera.x > -maxSpeed))
            {
                movement.rigidbody.AddForce(GetForce() * movement.CameraForward
                    * Mathf.Sign(movement.inputDirection.x));
            }

            if ((movement.inputDirection.z > 0 && velocityRelativeToCamera.z < maxSpeed)
            || (movement.inputDirection.z < 0 && velocityRelativeToCamera.z > -maxSpeed))
            {
                movement.rigidbody.AddForce(GetForce() * movement.CameraRight
                    * Mathf.Sign(movement.inputDirection.z));
            }
        }

        protected override void EnterImpl()
        {
            mode = Mode.Walking;
        }
        protected override void ExitImpl() { }
    }
}