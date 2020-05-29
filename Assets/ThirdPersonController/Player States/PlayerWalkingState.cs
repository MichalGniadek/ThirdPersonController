using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThirdPersonController
{
    [System.Serializable]
    public class PlayerWalkingState : PlayerState
    {
        [SerializeField] float walkForce = 0f;
        [SerializeField] float sprintForce = 0f;
        [SerializeField] float stopForce = 0f;
        [SerializeField] float maxSpeed = 0f;
        [SerializeField] float rotationSpeed = 0f;

        bool isSprinting = false;

        public void SetSprinting(bool isSprinting)
        {
            this.isSprinting = isSprinting;
        }

        public override PlayerState Process(Vector3 inputWorldDirection)
        {
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
            else
            {
                StopForce(-Vector3.right, CurrentVelocity.x, inputWorldDirection.x);
                StopForce(-Vector3.forward, CurrentVelocity.z, inputWorldDirection.z);
            }

            movement.animator.SetFloat("WalkingSpeed", Mathf.Min(1f,
                CurrentVelocity.Horizontal().magnitude / maxSpeed));
        }

        /// <summary>
        /// Applies force slowing down player, when he wants to stop or go
        /// in the opposite direction. Second and third arguements should
        /// be in the same world axis.
        /// </summary>
        private void StopForce(Vector3 direction, float currentVelocity, float worldInput)
        {
            if (Mathf.Abs(currentVelocity) > 0.01f)
            {
                // Apply stop force if player
                // either doesn't want to move
                // or wants to move in the opposite direction
                if (Mathf.Abs(worldInput) < 0.05f ||
                    currentVelocity * worldInput < 0)
                {
                    movement.rb.AddForce(direction * currentVelocity * stopForce);
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