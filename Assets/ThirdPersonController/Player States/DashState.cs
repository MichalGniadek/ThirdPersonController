using UnityEngine;

namespace ThirdPersonController
{
    [System.Serializable]
    public class DashState : PlayerState
    {
        [SerializeField, Min(0)]
        float duration = 0f;
        [SerializeField, Tooltip("Force applied once when entered state")]
        float impulseForce = 0f;
        [SerializeField, Tooltip("Force applied each FixedUpdate")]
        float sustainedForce = 0f;
        [SerializeField, Tooltip("Force applied once downwards when dash ends")]
        float downForce = 0f;

        float currentTime = 0f;
        Vector3 dashDirection;

        public override PlayerState Process(Vector3 velocityRelativeToCamera)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
            {
                movement.rigidbody.AddForce(downForce * Vector3.down, ForceMode.Impulse);
                if (movement.OnGround(out var hit)) return movement.walkingState;
                else return movement.inAirState;
            }
            return this;
        }

        public override void FixedProcess(Vector3 velocityRelativeToCamera)
        {
            movement.rigidbody.AddForce(dashDirection * sustainedForce);
        }

        protected override void EnterImpl()
        {
            currentTime = duration;
            dashDirection = movement.CameraForward.Horizontal().normalized;
            movement.rigidbody.AddForce(dashDirection * impulseForce, ForceMode.Impulse);
        }

        protected override void ExitImpl() { }
    }
}