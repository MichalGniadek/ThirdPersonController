﻿using UnityEngine;

namespace ThirdPersonController
{
    [System.Serializable]
    public class PlayerWalkingState : PlayerState
    {
        [SerializeField] float walkForce = 0f;
        [SerializeField] float sprintForce = 0f;
        [SerializeField] float maxSpeed = 0f;
        [SerializeField] float rotationSpeed = 0f;
        [SerializeField] float jumpForce = 0f;

        bool isSprinting = false;

        public override PlayerState Process(Vector3 inputWorldDirection)
        {
            isSprinting = Input.GetKey(KeyCode.LeftShift);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                movement.rb.AddForce(Vector3.up * jumpForce);
                return movement.inAirState;
            }

            movement.animator.SetFloat("WalkingSpeed", Mathf.Min(1f,
                CurrentVelocity.Horizontal().magnitude / maxSpeed));

            return this;
        }

        public override void FixedProcess(Vector3 inputWorldDirection)
        {
            movement.rb.AddForce(Vector3.down);

            if (inputWorldDirection.sqrMagnitude > 0.05f &&
                CurrentVelocity.Horizontal().magnitude < maxSpeed)
            {
                // Movement
                movement.rb.AddForce(inputWorldDirection.normalized *
                    (isSprinting ? sprintForce : walkForce));

                // Rotation
                float targetAngle = Mathf.Rad2Deg *
                    Mathf.Atan2(inputWorldDirection.x, inputWorldDirection.z);

                float deltaAngle =
                    Mathf.DeltaAngle(movement.transform.eulerAngles.y, targetAngle);

                if (Mathf.Abs(deltaAngle) > 3f)
                {
                    float angleDirection = Mathf.Sign(deltaAngle);
                    movement.rb.AddTorque(0f, angleDirection * rotationSpeed, 0f);
                }
            }
        }

        protected override void EnterImpl()
        {

        }

        protected override void ExitImpl()
        {
        }
    }

}