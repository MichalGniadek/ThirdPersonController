using UnityEngine;
using Cinemachine;

namespace ThirdPersonController
{
    public class ThirdPersonMovement : MonoBehaviour
    {
        public Rigidbody rb = null;
        public Animator animator = null;
        public CinemachineFreeLook cinemachineCamera = null;
        public new Camera camera = null;
        [Space]

        #region States
        public PlayerWalkingState walkingState = new PlayerWalkingState();
        #endregion

        PlayerState currentState;

        [HideInInspector] public Vector3 inputDirection = new Vector3();
        [HideInInspector] public Vector2 mouse = new Vector2();

        void Awake()
        {
            walkingState.movement = this;

            currentState = walkingState;
            currentState.Enter();
        }

        void Update()
        {
            currentState.Process(inputDirection);
        }

        void FixedUpdate()
        {
            Vector3 inputWorldDirection =
                camera.transform.forward.Horizontal() * inputDirection.z +
                camera.transform.right * inputDirection.x;
            currentState.FixedProcess(inputWorldDirection.normalized);
        }
    }
}