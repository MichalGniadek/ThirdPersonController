﻿using UnityEngine;

namespace ThirdPersonController
{
    [System.Serializable]
    public class InAirState : PlayerState
    {
        [SerializeField, Tooltip("Additional force applied downwards")]
        public float additionalGravity = 0f;
        [SerializeField, Tooltip("Force applied to stop movement")]
        float horizontalDrag = 0f;
        [SerializeField, Tooltip("Force applied to rigidbody when moving in air")]
        float moveForce = 0f;
        [SerializeField, Tooltip("Maximum speed after which there's no force applied")]
        float maxSpeed = 0f;
        [Space]
        [SerializeField, Tooltip("Number of additional airjumps"), Min(0)]
        int numberOfAirJumps = 0;
        [SerializeField, Tooltip("Force applied vertically to rigidbody when jumping")]
        float airJumpForce = 0f;

        int currentAirJumps = 0;

        RaycastHit lastWall = new RaycastHit();

        public override PlayerState Process(Vector3 velocityRelativeToCamera)
        {
            // So we don't detect ground immediately after jumping
            if (movement.TimeSinceStateChange > 0.1f && movement.OnGround(out var hit))
            {
                movement.animator.CrossFade("Land", 0.1f);
                return movement.walkingState;
            }

            if (Input.GetKeyDown(KeyCode.E)) return movement.dashState;

            RaycastHit hitInfo;
            bool alreadyJumpedThisFrame = false;

            if (movement.IsNearValidWall(out hitInfo))
            {
                if (Input.GetMouseButton(1) && !movement.wallRunningState.JumpedFromTheWall)
                {
                    return movement.wallRunningState;
                }
                else if (Input.GetKeyDown(KeyCode.Space) &&
                            hitInfo.collider != lastWall.collider)
                {
                    lastWall = hitInfo;
                    alreadyJumpedThisFrame = true;
                    movement.wallRunningState.WallJump(hitInfo.normal);
                }
            }
            else if (movement.IsNearValidWall(out hitInfo, frontBack: true))
            {
                if (Input.GetKeyDown(KeyCode.Space) &&
                            hitInfo.collider != lastWall.collider)
                {
                    lastWall = hitInfo;
                    alreadyJumpedThisFrame = true;
                    movement.wallRunningState.WallJump(hitInfo.normal);
                }
            }

            movement.wallRunningState.ResetJumpedFromTheWall();
            if (!alreadyJumpedThisFrame && currentAirJumps > 0 &&
                Input.GetKeyDown(KeyCode.Space))
            {
                currentAirJumps--;
                Jump(airJumpForce);
            }

            if (movement.NearLadder && Input.GetKey(KeyCode.F))
                return movement.ladderClimbingState;

            if (movement.inputDirection.magnitude > 0.1f)
                HandleRotation();

            return this;
        }

        public override void FixedProcess(Vector3 velocityRelativeToCamera)
        {
            movement.rigidbody.AddForce(Vector3.down * additionalGravity);

            HandleMovementInAxis(
                velocityRelativeToCamera.x, movement.inputDirection.x,
                movement.CameraForward, maxSpeed,
                horizontalDrag, moveForce);
            HandleMovementInAxis(
                velocityRelativeToCamera.z, movement.inputDirection.z,
                movement.CameraRight, maxSpeed,
                horizontalDrag, moveForce);
        }

        protected override void EnterImpl()
        {
            currentAirJumps = numberOfAirJumps;
        }

        protected override void ExitImpl()
        {
            movement.wallRunningState.ResetJumpedFromTheWall();
            if (movement.nextState != movement.wallRunningState)
                lastWall = new RaycastHit();
        }
    }
}