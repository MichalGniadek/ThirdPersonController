using System.Collections;
using UnityEngine;

namespace ThirdPersonController
{
    [System.Serializable]
    public class LedgeClimbingState : PlayerState
    {
        [SerializeField, Tooltip("Use this to correct position during \"braced\" animation")]
        Vector2 bracedGrabOffset = new Vector2();
        [SerializeField, Tooltip("Use this to correct position during \"hang\" animation")]
        Vector2 hangGrabOffset = new Vector2();

        [SerializeField, Tooltip("Move speed during moving left/ right when on a ledge")]
        float moveSpeed = 0f;
        [SerializeField] float jumpForce = 0f;

        ThirdPersonMovement.LedgeInfo ledgeInfo;
        Vector3 wallDirection = new Vector3();

        IEnumerator moveCoroutine = null;
        bool endingClimb = false;

        public override PlayerState Process(Vector3 velocityRelativeToCamera)
        {
            if (endingClimb)
            {
                moveCoroutine.MoveNext();
                movement.rigidbody.velocity = new Vector3();
                var state = movement.animator.GetCurrentAnimatorStateInfo(0);
                if (!state.IsName("Ledge Climb") && !state.IsName("End Ledge Climb"))
                {
                    movement.animator.applyRootMotion = false;
                    moveCoroutine = null;
                    movement.collider.gameObject.SetActive(true);
                    return movement.walkingState;
                }
                return this;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift) &&
                movement.TimeSinceStateChange > 0.1f)
            {
                movement.animator.applyRootMotion = true;
                movement.animator.CrossFade("End Ledge Climb", 0.2f);
                moveCoroutine = MoveDuringEndClimb(yVelocity1: 1.7f, time1: 0.18f,
                                                    time2: 0.34f);
                endingClimb = true;
                movement.collider.gameObject.SetActive(false);
                movement.rigidbody.velocity = new Vector3();
                return this;
            }

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

            if (movement.inputDirection.z == 0)
            {
                movement.animator.SetFloat("Ledge Climb Direction",
                                                        0, 0.1f, Time.deltaTime);
            }
            else
            {
                float direction = Mathf.Sign(movement.inputDirection.z);
                var sideInfo = direction > 0 ? ledgeInfo.right : ledgeInfo.left;

                // Inner corner
                if (sideInfo.sidewaysHit)
                {
                    movement.model.Rotate(0f, direction * 90f, 0f);
                }
                else
                {
                    // Standard move
                    if (sideInfo.forwardHit)
                    {
                        movement.transform.Translate(direction * wallDirection
                                                     * moveSpeed
                                                     * Time.deltaTime);

                        movement.animator.SetFloat("Ledge Climb Direction",
                                                    direction, 0.1f, Time.deltaTime);
                    }
                    else
                    {
                        // Outer corner
                        if (sideInfo.cornerHit)
                        {
                            movement.transform.RotateAround(
                                movement.transform.position + movement.model.forward * 2,
                                Vector3.up, -direction * 90f);
                            movement.model.Rotate(0f, -direction * 90f, 0f);
                        }
                        // No more ledge
                        else
                        {
                            movement.animator.SetFloat("Ledge Climb Direction",
                                                        0, 0.1f, Time.deltaTime);
                        }
                    }

                }
            }

            movement.animator.SetFloat("Ledge Climb Is Hang",
                                       ledgeInfo.freeHang ? 1 : 0,
                                       0.5f,
                                       Time.deltaTime);

            // Ugly fix
            movement.transform.rotation = Quaternion.identity;
            return this;
        }

        public override void FixedProcess(Vector3 velocityRelativeToCamera)
        {
            if (endingClimb)
            {
                // There is some kind of force in positive Z. No idea what.
                // That's why there's a counter movement
                var state = movement.animator.GetCurrentAnimatorStateInfo(0);
                if (state.IsName("End Ledge Climb"))
                {
                    movement.transform.Translate(0f, 0f, -0.03f);
                }
            }
        }

        protected override void EnterImpl()
        {
            endingClimb = false;
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
            Vector3 bracedGrabOffset3 = bracedGrabOffset.y * Vector3.up +
                                        bracedGrabOffset.x * movement.model.forward;

            Vector3 hangGrabOffset3 = hangGrabOffset.y * Vector3.up +
                                      hangGrabOffset.x * movement.model.forward;

            // Set position
            movement.transform.position =
                    ledgeInfo.horizontalInfo.point - bracedGrabOffset3;

            // Set local position depending on free hang
            Vector3 targetLocalPos =
                ledgeInfo.freeHang ? -hangGrabOffset3 : new Vector3();

            if (immediate) movement.model.localPosition = targetLocalPos;
            else movement.model.localPosition = Vector3.MoveTowards(
                                movement.model.localPosition,
                                targetLocalPos,
                                hangGrabOffset3.magnitude * Time.deltaTime
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

        IEnumerator MoveDuringEndClimb(float yVelocity1, float time1, float time2)
        {
            Vector3 velocity = new Vector3(0f, yVelocity1, 0f);
            float currentTime = 0f;
            bool secondStage = false;
            while (true)
            {
                if (currentTime >= time1 + time2) break;
                else if (!secondStage && currentTime >= time1)
                {
                    secondStage = true;
                    velocity = -ledgeInfo.horizontalInfo.normal * 3f;

                    float yTarget = ledgeInfo.verticalInfo.point.y - 1.7f;
                    float remainingDistance = yTarget - movement.transform.position.y;
                    velocity.y = remainingDistance / time2;
                }

                currentTime += Time.deltaTime;
                movement.transform.Translate(velocity * Time.deltaTime);
                yield return null;
            }
        }
    }
}