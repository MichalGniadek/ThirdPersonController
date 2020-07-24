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

        public override PlayerState Process(Vector3 velocityRelativeToCamera)
        {
            if (movement.OnGround(out var hitInfo)) return movement.walkingState;

            if (Input.GetKeyUp(KeyCode.Space) ||
                !movement.IsNearValidWall(out wallHitInfo, canBeTheSameWall: true))
            {
                Jump(verticalJumpForce);
                movement.rigidbody.AddForce(wallHitInfo.normal * horizontalJumpForce);
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
            Vector3 vel = movement.rigidbody.velocity;
            vel.y = Mathf.Clamp(vel.y, -maxVerticalVelocity, maxVerticalVelocity);
            movement.rigidbody.velocity = vel;
        }

        protected override void ExitImpl()
        {
        }
    }
}