using UnityEngine;

namespace ThirdPersonController
{
    [System.Serializable]
    public class RollState : PlayerState
    {
        const float rollAnimationDuration = 1.167f;

        [SerializeField, Min(0)]
        float duration = 0f;
        [SerializeField, Tooltip("Force applied once when entered state")]
        float impulseForce = 0f;
        [SerializeField, Tooltip("Force applied each FixedUpdate")]
        float sustainedForce = 0f;
        [SerializeField, Range(0, 1)]
        [Tooltip("Fraction of full height (y scale) that collider will change to")]
        float height = 1f;

        float currentTime = 0f;
        Vector3 rollDirection;

        public override PlayerState Process(Vector3 velocityRelativeToCamera)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0) return movement.walkingState;

            HandleRotation();

            return this;
        }

        public override void FixedProcess(Vector3 velocityRelativeToCamera)
        {
            movement.rigidbody.AddForce(rollDirection * sustainedForce);
        }

        protected override void EnterImpl()
        {
            movement.animator.SetFloat("Roll Duration Modifier",
                1 / rollAnimationDuration / duration);
            movement.animator.CrossFade("Roll", 0.1f);

            currentTime = duration;
            rollDirection = movement.CameraForward.Horizontal().normalized;
            movement.rigidbody.AddForce(rollDirection * impulseForce, ForceMode.Impulse);

            SetHeight(height);
        }

        protected override void ExitImpl()
        {
            SetHeight(1f);
        }
    }
}