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

            HandleFreeHangOffset(immediate: false);

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

            movement.transform.position =
                ledgeInfo.horizontalInfo.point - GrabOffset(bracedGrabOffset);

            HandleFreeHangOffset(immediate: true);

            movement.model.rotation =
                Quaternion.LookRotation(-ledgeInfo.horizontalInfo.normal, Vector3.up);

            wallDirection =
                Vector3.Cross(ledgeInfo.horizontalInfo.normal, Vector3.up).normalized;

            movement.animator.CrossFade("Ledge Climb", 0.1f);
            movement.animator.SetFloat("Ledge Climb Is Hang", ledgeInfo.freeHang ? 1 : 0);
        }

        protected override void ExitImpl()
        {
            movement.model.transform.localPosition = new Vector3();
        }

        void HandleFreeHangOffset(bool immediate)
        {
            float delta = immediate ?
                        1000f : GrabOffset(hangGrabOffset).magnitude * Time.deltaTime;

            Vector3 pos = movement.model.transform.localPosition;
            pos = Vector3.MoveTowards(
                        pos,
                        (ledgeInfo.freeHang ?
                            -GrabOffset(hangGrabOffset) :
                            new Vector3()),
                        delta);
            movement.model.transform.localPosition = pos;
        }

        Vector3 GrabOffset(Vector2 vec) =>
            vec.y * Vector3.up +
            vec.x * movement.model.forward;
    }
}