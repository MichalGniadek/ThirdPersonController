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
        /// Called every update if state is active. Return state FSM should change to.
        /// Return this if you don't want to change state.
        /// </summary>
        public abstract PlayerState Process(Vector3 inputWorldDirection);
        public abstract void FixedProcess(Vector3 inputWorldDirection);
        protected abstract void EnterImpl();
        protected abstract void ExitImpl();
    }

}