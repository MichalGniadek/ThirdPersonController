using UnityEngine;

namespace ThirdPersonController
{
    [System.Serializable]
    public class SlideState : PlayerState
    {
        const float slideAnimationDuration = 1.533f;

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
        Vector3 slideDirection;

        public override PlayerState Process(Vector3 velocityRelativeToCamera)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0) return movement.walkingState;

            HandleRotation();

            return this;
        }

        public override void FixedProcess(Vector3 velocityRelativeToCamera)
        {
            movement.rigidbody.AddForce(slideDirection * sustainedForce);
        }

        protected override void EnterImpl()
        {
            movement.animator.SetFloat("Slide Duration Modifier",
                1 / slideAnimationDuration / duration);
            movement.animator.CrossFade("Slide", 0.1f);

            currentTime = duration;
            slideDirection = movement.CameraForward.Horizontal().normalized;
            movement.rigidbody.AddForce(slideDirection * impulseForce, ForceMode.Impulse);

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