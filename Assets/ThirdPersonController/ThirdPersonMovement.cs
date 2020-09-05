using UnityEngine;

namespace ThirdPersonController
{
    [SelectionBase]
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

        [Space]
        [SerializeField] float wallCheckLength = 0f;
        [SerializeField] float wallCheckYOffset = 0f;

        [Space]
        [SerializeField] float ledgeCheckForwardOffset = 0f;
        [SerializeField] float ledgeCheckUpOffset = 0f;
        [SerializeField] float ledgeCheckLength = 0f;
        [SerializeField] float freeHangUpOffset = 0f;
        [SerializeField] float freeHangLength = 0f;
        [SerializeField] float ledgeCheckSideSpread = 0f;

        #region States
        [Space]
        public WalkingState walkingState = new WalkingState();
        public InAirState inAirState = new InAirState();
        public DashState dashState = new DashState();
        public RollState rollState = new RollState();
        public SlideState slideState = new SlideState();
        public WallRunningState wallRunningState = new WallRunningState();
        public LadderClimbingState ladderClimbingState = new LadderClimbingState();
        public LedgeClimbingState ledgeCimbingState = new LedgeClimbingState();
        #endregion

        float timeSinceStateChange = 0f;
        public float TimeSinceStateChange => timeSinceStateChange;

        public PlayerState previousState { private set; get; } = null;
        PlayerState currentState;
        public PlayerState nextState { private set; get; } = null;

        [HideInInspector] public Vector3 inputWorldDirection = new Vector3();

        public float HorizontalVelocity => rigidbody.velocity.Horizontal().magnitude;

        public Vector3 CameraForward =>
                camera.transform.forward.Horizontal().normalized;

        public Vector3 CameraRight =>
                camera.transform.right.Horizontal().normalized;

        [HideInInspector] public Vector3 inputDirection = new Vector3();

        [HideInInspector] public LadderScript currentLadder = null;
        public bool NearLadder => currentLadder != null;

        void Awake()
        {
            walkingState.movement = this;
            inAirState.movement = this;
            dashState.movement = this;
            rollState.movement = this;
            slideState.movement = this;
            wallRunningState.movement = this;
            ladderClimbingState.movement = this;
            ledgeCimbingState.movement = this;

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
                camera.transform.forward.Horizontal() * inputDirection.x +
                camera.transform.right.Horizontal() * inputDirection.z;

            nextState = currentState.Process(inputWorldDirection);
            if (nextState != currentState)
            {
                currentState.Exit();
                previousState = currentState;
                currentState = nextState;
                currentState.Enter();

                timeSinceStateChange = 0f;
            }
            else
            {
                timeSinceStateChange += Time.deltaTime;
            }
            //Debug.Log(currentState);
        }

        void FixedUpdate()
        {
            currentState.FixedProcess(GetVelocityRelativeToCamera());
        }

        public bool CouldReturnToState(PlayerState state) =>
            previousState != state || timeSinceStateChange > 0.1f;

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

        private Vector3 YOffsetPosition(float y) => collider.position + Vector3.up * y;

        public bool OnGround(out RaycastHit hitInfo)
        {
            bool raycastHit = false;
            hitInfo = new RaycastHit();

            Vector3 basePos = YOffsetPosition(0.2f);
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

            Vector3 basePos = YOffsetPosition(0.2f);
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

        public bool IsNearValidWall(out RaycastHit wallHitInfo, bool frontBack = false)
        {
            Vector3 basePos = YOffsetPosition(wallCheckYOffset);

            Vector3 raycastDirection;
            if (frontBack) raycastDirection = model.forward;
            else raycastDirection = model.right;

            Physics.Raycast(basePos, raycastDirection, out var hitInfo1,
                wallCheckLength, groundLayer);
            Physics.Raycast(basePos, -raycastDirection, out var hitInfo2,
                wallCheckLength, groundLayer);

            bool right_wall_viable = hitInfo1.collider != null;
            bool left_wall_viable = hitInfo2.collider != null;

            if (right_wall_viable && !left_wall_viable)
            {
                wallHitInfo = hitInfo1;
            }
            else if (!right_wall_viable && left_wall_viable)
            {
                wallHitInfo = hitInfo2;
            }
            else if (right_wall_viable && left_wall_viable)
            {
                if (hitInfo1.distance < hitInfo2.distance)
                {
                    wallHitInfo = hitInfo1;
                }
                else
                {
                    wallHitInfo = hitInfo2;
                }
            }
            else//both not viable
            {
                wallHitInfo = new RaycastHit();
                return false;
            }

            return true;
        }

        public struct LedgeInfo
        {
            public RaycastHit verticalInfo;
            public RaycastHit horizontalInfo;
            public RaycastHit rightInfo;
            public RaycastHit leftInfo;
            public bool freeHang;
            public bool right;
            public bool left;

            public Vector3 GetAverageNormal()
            {
                Vector3 average = horizontalInfo.normal;
                int i = 0;
                if (right)
                {
                    average += rightInfo.normal;
                    i++;
                }
                if (left)
                {
                    average += leftInfo.normal;
                    i++;
                }
                return average / i;
            }
        }

        public bool CheckLedge(out LedgeInfo ledgeInfo)
        {
            Vector3 ledgeCheckPosition =
                    YOffsetPosition(ledgeCheckUpOffset) +
                    model.forward * ledgeCheckForwardOffset;

            bool b = Physics.Raycast(ledgeCheckPosition,
                                        Vector3.down,
                                        out ledgeInfo.verticalInfo,
                                        ledgeCheckLength,
                                        groundLayer);

            Vector3 horizontalCheckPostion = transform.position;
            horizontalCheckPostion.y = ledgeInfo.verticalInfo.point.y - 0.1f;

            b &= Physics.Raycast(horizontalCheckPostion,
                                 model.forward,
                                 out ledgeInfo.horizontalInfo,
                                 freeHangLength,
                                 groundLayer);

            ledgeInfo.freeHang = !Physics.Raycast(YOffsetPosition(freeHangUpOffset),
                                                    model.forward,
                                                    freeHangLength);

            Vector3 sideCheckPosition = horizontalCheckPostion;
            sideCheckPosition.y -= 0.1f;

            ledgeInfo.right = Physics.Raycast(
                sideCheckPosition + model.transform.right * ledgeCheckSideSpread,
                model.forward,
                out ledgeInfo.rightInfo,
                freeHangLength + 0.5f,
                groundLayer);

            ledgeInfo.left = Physics.Raycast(
                sideCheckPosition - model.transform.right * ledgeCheckSideSpread,
                model.forward,
                out ledgeInfo.leftInfo,
                freeHangLength + 0.5f,
                groundLayer);

            return b;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<LadderScript>(out var l))
                currentLadder = l;
        }

        void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<LadderScript>(out var l))
                currentLadder = null;
        }

        void OnDrawGizmosSelected()
        {
            Vector3 verticalBasePos = YOffsetPosition(0.2f);

            Gizmos.color = Color.red;
            for (int i = 0; i < spreadTable.Length; i++)
            {
                Gizmos.DrawRay(
                    verticalBasePos + spreadTable[i],
                    Vector3.down * groundCheckLength);
            }

            Gizmos.color = Color.magenta;
            for (int i = 0; i < spreadTable.Length; i++)
            {
                Gizmos.DrawRay(
                    verticalBasePos + spreadTable[i],
                    Vector3.up * standCheckLength);
            }

            Vector3 wallCheckBasePos = YOffsetPosition(wallCheckYOffset);
            Gizmos.DrawRay(wallCheckBasePos, model.right * wallCheckLength);
            Gizmos.DrawRay(wallCheckBasePos, -model.right * wallCheckLength);

            Gizmos.color = Color.blue;
            Vector3 ledgeCheckPosition =
                YOffsetPosition(ledgeCheckUpOffset) +
                model.forward * ledgeCheckForwardOffset;

            Gizmos.DrawRay(ledgeCheckPosition, Vector3.down * ledgeCheckLength);
            Gizmos.DrawRay(YOffsetPosition(freeHangUpOffset),
                model.forward * freeHangLength);
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