using UnityEngine;
namespace ThirdPersonController
{
    [System.Serializable]
    public class WallRunningState : PlayerState
    {
        [SerializeField, Tooltip("Additional gravity applied to character")]
        float additionalGravity = 0f;
        [SerializeField, Tooltip("Force applied when moving")]
        float moveForce = 0f;
        [SerializeField, Tooltip("Maximum speed while wall running")]
        float maxSpeed = 0f;
        [SerializeField, Tooltip("Force applied upwards when jumping")]
        float verticalJumpForce = 0f;
        [SerializeField, Tooltip("Force applied from wall when jumping")]
        float horizontalJumpForce = 0f;
        [SerializeField, Tooltip("Maximum vertical velocity when entering this state")]
        float maxVerticalVelocity = 0f;

        RaycastHit wallHitInfo = new RaycastHit();
        Vector3 wallDirection = new Vector3();

        public bool JumpedFromTheWall { private set; get; } = false;

        public override PlayerState Process(Vector3 velocityRelativeToCamera)
        {
            if (movement.OnGround(out var hitInfo)) return movement.walkingState;

            if (Input.GetMouseButtonUp(1) ||
                !movement.IsNearValidWall(out wallHitInfo))
            {
                return movement.inAirState;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                JumpedFromTheWall = true;
                WallJump(wallHitInfo.normal);
                return movement.inAirState;
            }

            wallDirection = Vector3.Cross(wallHitInfo.normal, Vector3.up).normalized;
            if (Vector3.Angle(movement.CameraForward, wallDirection) > 90)
                wallDirection = -wallDirection;

            return this;
        }

        public override void FixedProcess(Vector3 velocityRelativeToCamera)
        {
            movement.rigidbody.AddForce(Vector3.down * additionalGravity);

            if (movement.HorizontalVelocity < maxSpeed)
                movement.rigidbody.AddForce(wallDirection * moveForce);
        }

        protected override void EnterImpl()
        {
            JumpedFromTheWall = false;

            Vector3 vel = movement.rigidbody.velocity;
            vel.y = Mathf.Clamp(vel.y, -maxVerticalVelocity, maxVerticalVelocity);
            movement.rigidbody.velocity = vel;
        }

        protected override void ExitImpl()
        {
        }

        public void ResetJumpedFromTheWall()
        {
            JumpedFromTheWall = false;
        }

        public void WallJump(Vector3 wallNormal)
        {
            Jump(verticalJumpForce);
            movement.rigidbody.AddForce(wallNormal * horizontalJumpForce);
        }
    }
}