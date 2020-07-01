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

        public abstract PlayerState Process(Vector3 inputWorldDirection);
        public abstract void FixedProcess(Vector3 inputWorldDirection);
        protected abstract void EnterImpl();
        protected abstract void ExitImpl();
    }

}