using UnityEngine;

namespace ThirdPersonController
{
    [System.Serializable]
    public class WalkingState : PlayerState
    {
        [SerializeField, Tooltip("Force applied when moving")]
        float moveForce = 0f;
        [SerializeField, Tooltip("Force applied to stop movement")]
        float horizontalDrag = 0f;
        [SerializeField, Tooltip("Maximum speed when walking")]
        float walkSpeed = 0f;
        [SerializeField, Tooltip("Maximum speed when sprinting")]
        float sprintSpeed = 0f;
        [SerializeField, Tooltip("Maximum speed when crouching")]
        float crouchSpeed = 0f;
        [SerializeField, Tooltip("Mulitplier applied to max speed to sideways movement")]
        float sideMaxSpeedMutliplier = 0f;
        [SerializeField, Tooltip("Minimum speed when you slide instead of crouching")]
        float minSpeedForSlide = 0f;
        [SerializeField, Tooltip("Force applied when jumping")]
        float jumpForce = 0f;

        enum Mode { Walking, Sprinting, Crouching }
        Mode mode = Mode.Walking;

        RaycastHit groundCheckHitInfo = new RaycastHit();

        float GetMaxSpeed()
        {
            switch (mode)
            {
                case Mode.Sprinting: return sprintSpeed;
                case Mode.Crouching: return crouchSpeed;
                default: return walkSpeed;
            }
        }

        public override PlayerState Process(Vector3 velocityRelativeToCamera)
        {
            if (!movement.OnGround(out groundCheckHitInfo))
            {
                movement.animator.CrossFade("Fall", 0.5f);
                return movement.inAirState;
            }
            if (Input.GetKeyDown(KeyCode.E)) return movement.dashState;
            if (Input.GetKeyDown(KeyCode.Q)) return movement.rollState;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Jump(jumpForce);
                return movement.inAirState;
            }

            // Handle mode changes
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

            movement.animator.SetFloat("WalkingSpeed",
                Mathf.Min(1f, movement.HorizontalVelocity / sprintSpeed));

            HandleRotation();

            return this;
        }

        public override void FixedProcess(Vector3 velocityRelativeToCamera)
        {
            movement.rigidbody.AddForce(-groundCheckHitInfo.normal);

            Vector3 groundForward = Vector3.ProjectOnPlane(
                movement.CameraForward,
                groundCheckHitInfo.normal);

            Vector3 groundRight = Vector3.ProjectOnPlane(
                movement.CameraRight,
                groundCheckHitInfo.normal);

            HandleMovementInAxis(
                velocityRelativeToCamera.x, movement.inputDirection.x,
                groundForward, GetMaxSpeed(),
                horizontalDrag, moveForce);

            HandleMovementInAxis(
                velocityRelativeToCamera.z, movement.inputDirection.z,
                groundRight, GetMaxSpeed() * sideMaxSpeedMutliplier,
                horizontalDrag, moveForce);
        }

        protected override void EnterImpl()
        {
            mode = Mode.Walking;
        }
        protected override void ExitImpl() { }
    }
}