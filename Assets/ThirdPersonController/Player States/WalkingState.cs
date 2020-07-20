﻿using UnityEngine;

namespace ThirdPersonController
{
    [System.Serializable]
    public class WalkingState : PlayerState
    {
        [SerializeField, Tooltip("Force applied when moving")]
        float moveForce = 0f;
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

            movement.animator.SetFloat("WalkingSpeed",
                Mathf.Min(1f, movement.HorizontalVelocity / sprintSpeed));

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

            HandleMovementInAxis(velocityRelativeToCamera.x, movement.inputDirection.x,
                movement.CameraForward, GetMaxSpeed());
            HandleMovementInAxis(velocityRelativeToCamera.z, movement.inputDirection.z,
                movement.CameraRight, GetMaxSpeed() * sideMaxSpeedMutliplier);

            void HandleMovementInAxis(float velocity, float input, Vector3 direction,
                float axisMaxSpeed)
            {
                // Counter movement
                // Either opposite or input is zero 
                // or velocity 0 (=> applied force is also zero)
                if (velocity * input <= 0)
                {
                    movement.rigidbody.AddForce(movement.HorizontalDrag
                                                * velocity
                                                * -direction);
                }

                if ((input > 0 && velocity < axisMaxSpeed) ||
                    (input < 0 && velocity > -axisMaxSpeed))
                {
                    movement.rigidbody.AddForce(moveForce * direction
                        * Mathf.Sign(input));
                }
            }
        }

        protected override void EnterImpl()
        {
            mode = Mode.Walking;
        }
        protected override void ExitImpl() { }
    }
}