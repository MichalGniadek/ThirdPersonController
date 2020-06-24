using UnityEngine;

namespace ThirdPersonController
{
    [System.Serializable]
    public class PlayerInAirState : PlayerState
    {
        [SerializeField] public float additionalGravity = 0f;
        [SerializeField] float moveForce = 0f;
        [SerializeField] float maxSpeed = 0f;
        [SerializeField] float rotationSpeed = 0f;

        public override PlayerState Process(Vector3 inputWorldDirection)
        {
            if (movement.OnGround()) return movement.walkingState;

            movement.animator.SetFloat("WalkingSpeed", Mathf.Min(1f,
                CurrentVelocity.Horizontal().magnitude / maxSpeed));

            movement.animator.SetBool("Landing", movement.Landing());

            return this;
        }

        public override void FixedProcess(Vector3 inputWorldDirection)
        {
            movement.rb.AddForce(Vector3.down * additionalGravity);

            if (inputWorldDirection.sqrMagnitude > 0.05f &&
                CurrentVelocity.Horizontal().magnitude < maxSpeed)
            {
                // Movement
                movement.rb.AddForce(inputWorldDirection.normalized * moveForce);

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
            movement.animator.SetBool("InAir", true);
        }

        protected override void ExitImpl()
        {
            Debug.Log("exit");
            movement.animator.SetBool("InAir", false);
        }
    }
}