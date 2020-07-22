using UnityEngine;
using UnityEngine.Events;

namespace ThirdPersonController
{
    /// <summary>
    /// Basic finite state machine. Calls EnterImpl() and onEnter unity event
    /// when entering a state, and ExitImpl() and onExit when exiting.
    /// 
    /// Calls Process every Update and FixedProcess every FixedUpdate.
    /// </summary>
    [System.Serializable]
    public abstract class PlayerState
    {
        [System.Serializable]
        public struct StateEvents
        {
            public UnityEvent onEnter;
            public UnityEvent onExit;
        }

        [SerializeField]
        [Tooltip("Optional unity events called when entering/ exiting this state")]
        StateEvents stateEvents = new StateEvents();

        [HideInInspector]
        public ThirdPersonMovement movement = null;

        public void Enter()
        {
            EnterImpl();
            stateEvents.onEnter?.Invoke();
        }

        public void Exit()
        {
            ExitImpl();
            stateEvents.onExit?.Invoke();
        }

        /// <summary>
        /// Change animation, reset y velocity and apply force
        /// </summary>
        protected void Jump(float force)
        {
            movement.animator.CrossFade("Jump", 0.1f);
            var vel = movement.rigidbody.velocity;
            vel.y = 0f;
            movement.rigidbody.velocity = vel;
            movement.rigidbody.AddForce(Vector3.up * force);
        }

        protected void HandleMovementInAxis(
            float currentVelocity, float input,
            Vector3 axisDirection, float axisMaxSpeed,
            float horizontalDrag, float moveForce)
        {
            // Counter movement
            // Either opposite or input is zero 
            // or velocity 0 (=> applied force is also zero)
            if (currentVelocity * input <= 0)
            {
                movement.rigidbody.AddForce(horizontalDrag
                                            * currentVelocity
                                            * -axisDirection);
            }

            if ((input > 0 && currentVelocity < axisMaxSpeed) ||
                (input < 0 && currentVelocity > -axisMaxSpeed))
            {
                movement.rigidbody.AddForce(moveForce * axisDirection
                    * Mathf.Sign(input));
            }
        }

        protected void HandleRotation()
        {
            Vector3 horizontalVelocity = movement.rigidbody.velocity.Horizontal();
            if (horizontalVelocity.magnitude > 0.1f)
            {
                float targetAngle = Mathf.Rad2Deg * Mathf.Atan2(
                    horizontalVelocity.x,
                    horizontalVelocity.z);

                float deltaAngle = Mathf.DeltaAngle(targetAngle,
                    movement.model.rotation.eulerAngles.y);

                deltaAngle = Mathf.Clamp(deltaAngle, -1f, 1f);

                movement.animator.SetFloat("Turning Speed", deltaAngle,
                    0.1f, Time.deltaTime);

                movement.model.rotation = Quaternion.Euler(0, targetAngle, 0);
            }
        }

        protected void SetHeight(float h)
        {
            var scale = movement.collider.localScale;
            scale.y = h;
            movement.collider.localScale = scale;
        }

        /// <summary>
        /// Called every update if state is active. Return state FSM should change to.
        /// Return this if you don't want to change state.
        /// </summary>
        public abstract PlayerState Process(Vector3 inputWorldDirection);
        public abstract void FixedProcess(Vector3 inputWorldDirection);
        protected abstract void EnterImpl();
        protected abstract void ExitImpl();
    }

}