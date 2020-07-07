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

        float currentTime = 0f;
        Vector3 dashDirection;

        public override PlayerState Process(Vector3 inputWorldDirection)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0) return movement.inAirState;
            return this;
        }

        public override void FixedProcess(Vector3 inputWorldDirection)
        {
            movement.rigidbody.AddForce(dashDirection * sustainedForce);
        }

        protected override void EnterImpl()
        {
            currentTime = duration;
            dashDirection = movement.rigidbody.velocity.Horizontal().normalized;
            movement.rigidbody.AddForce(dashDirection * impulseForce, ForceMode.Impulse);
        }

        protected override void ExitImpl() { }
    }
}