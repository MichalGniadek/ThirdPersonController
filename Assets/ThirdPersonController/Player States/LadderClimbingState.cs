using UnityEngine;

namespace ThirdPersonController
{
    [System.Serializable]
    public class LadderClimbingState : PlayerState
    {
        [SerializeField]
        float climbSpeed = 0f;
        [SerializeField, Tooltip("Force applied upwards when jumping")]
        float verticalJumpForce = 0f;
        [SerializeField, Tooltip("Force applied from wall when jumping")]
        float horizontalJumpForce = 0f;
        [SerializeField]
        [Tooltip("Offset from the top of the ladder, marking the end of the climb")]
        float ladderOffset = 0f;

        //Either 0, 1 or -1 depending on direction
        float climbDirection = 0f;

        LadderScript ladder = null;

        bool endingClimb = false;

        public override PlayerState Process(Vector3 velocityRelativeToCamera)
        {
            if (endingClimb)
            {
                var state = movement.animator.GetCurrentAnimatorStateInfo(0);
                if (!state.IsName("Climb Ladder") && !state.IsName("End Climb Ladder"))
                {
                    movement.animator.applyRootMotion = false;
                    return movement.walkingState;
                }
                return this;
            }

            if (!movement.NearLadder ||
                movement.transform.position.y > ladder.MaximumHeight - ladderOffset)
            {
                movement.animator.applyRootMotion = true;
                movement.animator.CrossFade("End Climb Ladder", 0.2f);
                endingClimb = true;
                return this;
            }

            if (movement.transform.position.y <= ladder.BasePosition.y)
            {
                if (movement.OnGround(out var h))
                {
                    movement.animator.CrossFade("Walk", 0.1f);
                    return movement.walkingState;
                }
                else
                {
                    movement.animator.CrossFade("Fall", 0.1f);
                    return movement.inAirState;
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                Jump(verticalJumpForce);
                movement.rigidbody.AddForce(ladder.Normal * horizontalJumpForce);
                return movement.inAirState;
            }

            if (movement.inputDirection.x > 0.1f) climbDirection = 1f;
            else if (movement.inputDirection.x < -0.1f) climbDirection = -1f;
            else climbDirection = 0f;

            movement.animator.SetFloat("Ladder Climb Direction", climbDirection);
            return this;
        }

        public override void FixedProcess(Vector3 velocityRelativeToCamera)
        {
            if (endingClimb) return;

            Vector3 pos = movement.transform.position;
            pos.y += climbSpeed * climbDirection;
            movement.transform.position = pos;
        }

        protected override void EnterImpl()
        {
            endingClimb = false;

            movement.animator.CrossFade("Climb Ladder", 0.1f);

            movement.rigidbody.velocity = new Vector3();

            ladder = movement.currentLadder;

            Vector3 newPos = ladder.BasePosition + ladder.Normal * 1.3f;
            // Don't change y position
            newPos.y = Mathf.Max(movement.transform.position.y, newPos.y + 0.2f);
            movement.transform.position = newPos;

            Vector3 rot = movement.model.rotation.eulerAngles;
            rot.y = ladder.Rotation + 180f;
            movement.model.rotation = Quaternion.Euler(rot);
        }

        protected override void ExitImpl()
        {
            // Otherwise character doesn't transition to other animation
            movement.animator.SetFloat("Ladder Climb Direction", 1);
        }
    }
}