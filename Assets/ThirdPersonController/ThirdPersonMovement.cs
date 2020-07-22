﻿using UnityEngine;

namespace ThirdPersonController
{
    public class ThirdPersonMovement : MonoBehaviour
    {
        public Animator animator = null;
        public new Rigidbody rigidbody = null;
        public new Transform collider = null;
        public new Camera camera = null;
        public Transform model = null;

        [Space]
        [SerializeField] LayerMask groundLayer = new LayerMask();
        [SerializeField] float groundCheckLength = 0f;

        [Space]
        [SerializeField] float standCheckLength = 0f;

        [Space]
        [SerializeField] float checkSpread = 0f;
        [SerializeField, HideInInspector]
        private readonly Vector3[] spreadTable =
            {Vector3.zero, Vector3.forward, Vector3.right, Vector3.back, Vector3.left };

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

        [HideInInspector] public Vector3 inputDirection = new Vector3();

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

        private Vector3 BaseRayPosition() => collider.position - Vector3.down * 0.2f;

        public bool OnGround(out RaycastHit hitInfo)
        {
            bool raycastHit = false;
            hitInfo = new RaycastHit();

            Vector3 basePos = BaseRayPosition();
            for (int i = 0; !raycastHit && i < spreadTable.Length; i++)
            {
                raycastHit |= Physics.Raycast(
                        basePos + spreadTable[i],
                        Vector3.down,
                        out hitInfo,
                        groundCheckLength,
                        groundLayer);
            }

            return raycastHit;
        }

        public bool CanStand()
        {
            bool canStand = true;

            Vector3 basePos = BaseRayPosition();
            for (int i = 0; canStand && i < spreadTable.Length; i++)
            {
                canStand &= !Physics.Raycast(
                        basePos + spreadTable[i],
                        Vector3.up,
                        standCheckLength,
                        groundLayer);
            }

            return canStand;
        }

        void OnDrawGizmosSelected()
        {
            Vector3 basePos = BaseRayPosition();

            Gizmos.color = Color.red;
            for (int i = 0; i < spreadTable.Length; i++)
            {
                Gizmos.DrawRay(
                    basePos + spreadTable[i],
                    Vector3.down * groundCheckLength);
            }

            Gizmos.color = Color.magenta;
            for (int i = 0; i < spreadTable.Length; i++)
            {
                Gizmos.DrawRay(
                    basePos + spreadTable[i],
                    Vector3.up * standCheckLength);
            }
        }

        void OnValidate()
        {
            spreadTable[0] = Vector3.zero;
            spreadTable[1] = Vector3.forward * checkSpread;
            spreadTable[2] = Vector3.right * checkSpread;
            spreadTable[3] = Vector3.back * checkSpread;
            spreadTable[4] = Vector3.left * checkSpread;
        }
    }
}