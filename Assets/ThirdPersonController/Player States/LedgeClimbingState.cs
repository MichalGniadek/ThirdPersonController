using UnityEngine;

namespace ThirdPersonController
{
    [System.Serializable]
    public class LedgeClimbingState : PlayerState
    {
        [SerializeField] Vector2 bracedGrabOffset = new Vector2();
        [SerializeField] Vector2 hangGrabOffset = new Vector2();

        [SerializeField] float moveSpeed = 0f;
        [SerializeField] float jumpForce = 0f;

        ThirdPersonMovement.LedgeInfo ledgeInfo;
        Vector3 wallDirection = new Vector3();

        public override PlayerState Process(Vector3 velocityRelativeToCamera)
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                movement.animator.CrossFade("Fall", 0.1f);
                return movement.inAirState;
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Jump(jumpForce);
                return movement.inAirState;
            }

            movement.CheckLedge(out ledgeInfo);
            HandleRotationAndPosition(immediate: false);

            if ((movement.inputDirection.z > 0 && ledgeInfo.right) ||
               (movement.inputDirection.z < 0 && ledgeInfo.left))
            {
                movement.transform.Translate(wallDirection * moveSpeed *
                    movement.inputDirection.z * Time.deltaTime);

                movement.animator.SetFloat("Ledge Climb Direction",
                                       movement.inputDirection.z,
                                       0.1f,
                                       Time.deltaTime);
            }
            else
            {
                movement.animator.SetFloat("Ledge Climb Direction",
                                       0,
                                       0.1f,
                                       Time.deltaTime);
            }

            movement.animator.SetFloat("Ledge Climb Is Hang",
                                       ledgeInfo.freeHang ? 1 : 0,
                                       0.5f,
                                       Time.deltaTime);

            // Ugly fix
            movement.transform.rotation = Quaternion.identity;
            return this;
        }

        public override void FixedProcess(Vector3 velocityRelativeToCamera) { }

        protected override void EnterImpl()
        {
            movement.rigidbody.velocity = new Vector3();
            movement.CheckLedge(out ledgeInfo);

            HandleRotationAndPosition(immediate: true);

            movement.animator.CrossFade("Ledge Climb", 0.1f);
            movement.animator.SetFloat("Ledge Climb Is Hang", ledgeInfo.freeHang ? 1 : 0);
        }

        protected override void ExitImpl()
        {
            movement.model.transform.localPosition = new Vector3();
        }

        void HandleRotationAndPosition(bool immediate)
        {
            // Set position
            movement.transform.position =
                    ledgeInfo.horizontalInfo.point - GrabOffset(bracedGrabOffset);

            // Set local position depending on free hang
            Vector3 targetLocalPos =
                ledgeInfo.freeHang ? -GrabOffset(hangGrabOffset) : new Vector3();

            if (immediate) movement.model.localPosition = targetLocalPos;
            else movement.model.localPosition = Vector3.MoveTowards(
                                movement.model.localPosition,
                                targetLocalPos,
                                GrabOffset(hangGrabOffset).magnitude * Time.deltaTime
                            );

            // Set rotation
            Quaternion targetRotation =
                Quaternion.LookRotation(-ledgeInfo.GetAverageNormal(), Vector3.up);

            if (immediate) movement.model.rotation = targetRotation;
            else movement.model.rotation = Quaternion.RotateTowards(
                                                    movement.model.rotation,
                                                    targetRotation,
                                                    90f * Time.deltaTime
                                                );

            // Set wall direction
            wallDirection =
                Vector3.Cross(ledgeInfo.horizontalInfo.normal, Vector3.up).normalized;
        }

        Vector3 GrabOffset(Vector2 vec) =>
            vec.y * Vector3.up +
            vec.x * movement.model.forward;
    }
}