using System.Collections;
using System.Collections.Generic;
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

        //Either 0, 1 or -1 depending on direction
        float climbDirection = 0f;

        LadderScript ladder = null;

        public override PlayerState Process(Vector3 velocityRelativeToCamera)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Jump(verticalJumpForce);
                movement.rigidbody.AddForce(ladder.Normal * horizontalJumpForce);
                return movement.inAirState;
            }
            if (!movement.NearLadder)
            {
                movement.rigidbody.AddForce(Vector3.up * 9, ForceMode.VelocityChange);
                movement.rigidbody.AddForce(-ladder.Normal * 9, ForceMode.VelocityChange);
                return movement.walkingState;
            }

            if (movement.inputDirection.x > 0.1f) climbDirection = 1f;
            else if (movement.inputDirection.x < -0.1f) climbDirection = -1f;
            else climbDirection = 0f;

            movement.animator.SetFloat("Ladder Climb Direction", climbDirection);
            return this;
        }

        public override void FixedProcess(Vector3 velocityRelativeToCamera)
        {
            Vector3 pos = movement.transform.position;
            pos.y += climbSpeed * climbDirection;
            movement.transform.position = pos;
        }

        protected override void EnterImpl()
        {
            movement.animator.CrossFade("Climb Ladder", 0.1f);

            movement.rigidbody.velocity = new Vector3();

            ladder = movement.currentLadder;

            Vector3 newPos = ladder.BasePosition + ladder.Normal * 1.3f;
            // Don't change y position
            newPos.y = movement.transform.position.y;
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