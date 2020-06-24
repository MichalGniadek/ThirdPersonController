using UnityEngine;
using Cinemachine;

namespace ThirdPersonController
{
    public class ThirdPersonMovement : MonoBehaviour
    {
        public Rigidbody rb = null;
        public Animator animator = null;
        public new Camera camera = null;
        [Space]

        #region States
        public PlayerWalkingState walkingState = new PlayerWalkingState();
        public PlayerInAirState inAirState = new PlayerInAirState();
        #endregion

        PlayerState currentState;

        [HideInInspector] public Vector3 inputWorldDirection = new Vector3();

        [HideInInspector] public bool isSprinting = false;
        [HideInInspector] public bool isJumping = false;

        [SerializeField] LayerMask groundLayer = new LayerMask();

        void Awake()
        {
            walkingState.movement = this;
            inAirState.movement = this;

            currentState = walkingState;
            currentState.Enter();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            Vector2 inputDirection = new Vector2(
                 Input.GetAxis("Horizontal"),
                 Input.GetAxis("Vertical")
            ).normalized;

            inputWorldDirection =
                camera.transform.forward.Horizontal() * inputDirection.y +
                camera.transform.right * inputDirection.x;

            var newState = currentState.Process(inputWorldDirection);
            if (newState != currentState)
            {
                currentState.Exit();
                currentState = newState;
                currentState.Enter();
            }
        }

        void FixedUpdate()
        {
            currentState.FixedProcess(inputWorldDirection.normalized);
        }

        public bool OnGround()
        {
            return Physics.Raycast(transform.position, Vector3.down,
                0.1f, groundLayer) && rb.velocity.y <= 0;
        }

        public bool Landing()
        {
            return Physics.Raycast(transform.position, Vector3.down,
                0.5f, groundLayer) && rb.velocity.y <= 0;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, Vector3.down * 0.1f);
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, Vector3.down * 0.5f);
        }
    }
}