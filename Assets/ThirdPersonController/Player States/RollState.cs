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

        public override PlayerState Process(Vector3 inputWorldDirection)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0) return movement.walkingState;
            return this;
        }

        public override void FixedProcess(Vector3 inputWorldDirection)
        {
            movement.rigidbody.AddForce(rollDirection * sustainedForce);
        }

        protected override void EnterImpl()
        {
            movement.animator.SetFloat("Roll Duration Modifier",
                1 / rollAnimationDuration / duration);
            movement.animator.CrossFade("Roll", 0.1f);

            currentTime = duration;
            rollDirection = movement.rigidbody.velocity.Horizontal().normalized;
            movement.rigidbody.AddForce(rollDirection * impulseForce, ForceMode.Impulse);

            SetHeight(height);
        }

        protected override void ExitImpl()
        {
            SetHeight(1f);
        }

        void SetHeight(float h)
        {
            var scale = movement.collider.transform.localScale;
            scale.y = h;
            movement.collider.transform.localScale = scale;
        }
    }
}