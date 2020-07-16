using UnityEngine;

namespace ThirdPersonController
{
    public class ThirdPersonMovement : MonoBehaviour
    {
        public Animator animator = null;
        public new Rigidbody rigidbody = null;
        public new Collider collider = null;
        public new Camera camera = null;
        public Transform model = null;

        [SerializeField] LayerMask groundLayer = new LayerMask();

        [Space]
        [SerializeField] float horizontalDrag = 0f;
        public float HorizontalDrag => horizontalDrag;

        #region States
        [Space]
        public WalkingState walkingState = new WalkingState();
        public InAirState inAirState = new InAirState();
        public DashState dashState = new DashState();
        public RollState rollState = new RollState();
        public SlideState slideState = new SlideState();
        #endregion

        float timeSinceStateChange = 0f;
        public float TimeSinceStateChange => timeSinceStateChange;

        PlayerState currentState;

        [HideInInspector] public Vector3 inputWorldDirection = new Vector3();

        [HideInInspector] public bool isSprinting = false;
        [HideInInspector] public bool isJumping = false;

        public float HorizontalVelocity => rigidbody.velocity.Horizontal().magnitude;

        public Vector3 CameraForward =>
                camera.transform.forward.Horizontal().normalized;

        public Vector3 CameraRight =>
                camera.transform.right.Horizontal().normalized;

        public Vector3 inputDirection = new Vector2();

        void Awake()
        {
            walkingState.movement = this;
            inAirState.movement = this;
            dashState.movement = this;
            rollState.movement = this;
            slideState.movement = this;

            currentState = walkingState;
            currentState.Enter();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            inputDirection = new Vector3(
                 Input.GetAxis("Vertical"),
                 0f,
                 Input.GetAxis("Horizontal")
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

                timeSinceStateChange = 0f;
            }
            else
            {
                timeSinceStateChange += Time.deltaTime;
            }
        }

        void FixedUpdate()
        {
            currentState.FixedProcess(GetVelocityRelativeToCamera());
        }

        Vector3 GetVelocityRelativeToCamera()
        {
            float lookAngle = camera.transform.rotation.eulerAngles.y;
            float moveAngle = Mathf.Rad2Deg *
                Mathf.Atan2(rigidbody.velocity.x, rigidbody.velocity.z);

            float deltaAngle = Mathf.DeltaAngle(lookAngle, moveAngle);

            return new Vector3(
                Mathf.Cos(deltaAngle * Mathf.Deg2Rad),
                0f,
                Mathf.Cos((90 - deltaAngle) * Mathf.Deg2Rad)
            ) * rigidbody.velocity.magnitude;
        }

        public bool OnGround()
        {
            return Physics.Raycast(transform.position + new Vector3(0, 0.1f, 0),
                Vector3.down, 0.15f, groundLayer) && rigidbody.velocity.y <= 0;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position + new Vector3(0, 0.1f, 0),
                Vector3.down * 0.15f);
        }
    }
}