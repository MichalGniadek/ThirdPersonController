using UnityEngine;
using UnityEngine.Events;

namespace ThirdPersonController
{
    [System.Serializable]
    public abstract class PlayerState
    {
        [System.Serializable]
        public struct StateEvents
        {
            public UnityEvent enter;
            public UnityEvent exit;
        }
        [SerializeField] StateEvents stateEvents = new StateEvents();

        [HideInInspector] public ThirdPersonMovement movement = null;

        protected Vector3 CurrentVelocity => movement.rb.velocity;

        public void Enter()
        {
            EnterImpl();
            stateEvents.enter?.Invoke();
        }

        public void Exit()
        {
            ExitImpl();
            stateEvents.exit?.Invoke();
        }

        public abstract PlayerState Process(Vector3 inputWorldDirection);
        public abstract void FixedProcess(Vector3 inputWorldDirection);
        protected abstract void EnterImpl();
        protected abstract void ExitImpl();
    }

}