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

        [SerializeField, Tooltip("Layer mask of all objects player should collide with")]
        LayerMask collisionLayer = new LayerMask();

        [SerializeField, Tooltip("Length of a raycast that checks if player is standing on ground")]
        float groundCheckLength = 0f;

        [SerializeField, Tooltip("Length of raycast that checks if player can stop crouching")]
        float standCheckLength = 0f;

        [SerializeField] float colliderRadius = 0f;
        [SerializeField, HideInInspector]
        private readonly Vector3[] spreadTable =
            {Vector3.zero, Vector3.forward, Vector3.right, Vector3.back, Vector3.left };

        [Header("Checks for wall running")]
        [SerializeField] float wallCheckLength = 0f;
        [SerializeField] float wallCheckUpOffset = 0f;

        [Header("Vertical check for ledges")]
        [SerializeField] float ledgeCheckForwardOffset = 0f;
        [SerializeField] float ledgeCheckUpOffset = 0f;
        [SerializeField] float verticalLedgeCheckLength = 0f;
        [Header("Horizontal checks for ledges")]
        [SerializeField] float horizontalLedgeCheckLength = 0f;
        [SerializeField] float ledgeCheckSideSpread = 0f;
        [SerializeField] float freeHangUpOffset = 0f;

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
                        collisionLayer);
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
                        collisionLayer);
            }

            return canStand;
        }

        public bool IsNearValidWall(out RaycastHit wallHitInfo, bool frontBack = false)
        {
            Vector3 basePos = YOffsetPosition(wallCheckUpOffset);

            Vector3 raycastDirection;
            if (frontBack) raycastDirection = model.forward;
            else raycastDirection = model.right;

            Physics.Raycast(basePos, raycastDirection, out var hitInfo1,
                wallCheckLength, collisionLayer);
            Physics.Raycast(basePos, -raycastDirection, out var hitInfo2,
                wallCheckLength, collisionLayer);

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
            public bool freeHang;

            public struct SideRayInfo
            {
                public bool sidewaysHit;
                public RaycastHit sidewaysInfo;
                public bool forwardHit;
                public RaycastHit forwardInfo;
                public bool cornerHit;
                public RaycastHit cornerInfo;
            }

            public SideRayInfo right;
            public SideRayInfo left;

            public Vector3 GetAverageNormal()
            {
                Vector3 average = horizontalInfo.normal;
                int i = 0;
                if (right.sidewaysHit || right.forwardHit)
                {
                    average += right.forwardInfo.normal;
                    i++;
                }
                if (left.sidewaysHit || left.forwardHit)
                {
                    average += left.forwardInfo.normal;
                    i++;
                }
                return average / i;
            }
        }

        public bool CheckLedge(out LedgeInfo ledgeInfo)
        {
            Vector3 verticalCheckPosition =
                    YOffsetPosition(ledgeCheckUpOffset) +
                    model.forward * ledgeCheckForwardOffset;

            bool b = Physics.Raycast(verticalCheckPosition,
                                     Vector3.down,
                                     out ledgeInfo.verticalInfo,
                                     verticalLedgeCheckLength,
                                     collisionLayer);

            Vector3 horizontalCheckPostion = transform.position;
            horizontalCheckPostion.y = ledgeInfo.verticalInfo.point.y - 0.1f;

            b &= Physics.Raycast(horizontalCheckPostion,
                                 model.forward,
                                 out ledgeInfo.horizontalInfo,
                                 horizontalLedgeCheckLength,
                                 collisionLayer);

            ledgeInfo.freeHang = !Physics.Raycast(YOffsetPosition(freeHangUpOffset),
                                                  model.forward,
                                                  horizontalLedgeCheckLength);

            ledgeInfo.right = RaycastSide(model.right);
            ledgeInfo.left = RaycastSide(-model.right);

            return b;

            LedgeInfo.SideRayInfo RaycastSide(Vector3 sideDirection)
            {
                var info = new LedgeInfo.SideRayInfo();

                Vector3 checkPosition = horizontalCheckPostion;
                checkPosition.y -= 0.1f;

                info.sidewaysHit = Physics.Raycast(checkPosition,
                                                   sideDirection,
                                                   out info.sidewaysInfo,
                                                   ledgeCheckSideSpread,
                                                   collisionLayer);

                if (info.sidewaysHit) return info;

                Vector3 forwardCheckPosition = checkPosition +
                                                sideDirection * ledgeCheckSideSpread;

                info.forwardHit = Physics.Raycast(forwardCheckPosition,
                                                  model.forward,
                                                  out info.forwardInfo,
                                                  horizontalLedgeCheckLength + 0.5f,
                                                  collisionLayer);

                if (info.forwardHit) return info;

                Vector3 cornerCheckPosition =
                                    forwardCheckPosition
                                    + sideDirection * (1.2f * ledgeCheckSideSpread)
                                    + model.forward * (2 * ledgeCheckSideSpread);

                info.cornerHit = Physics.Raycast(cornerCheckPosition,
                                                 -sideDirection,
                                                 out info.cornerInfo,
                                                 horizontalLedgeCheckLength + 0.5f,
                                                 collisionLayer);

                return info;
            }
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

            Vector3 wallCheckBasePos = YOffsetPosition(wallCheckUpOffset);
            Gizmos.DrawRay(wallCheckBasePos, model.right * wallCheckLength);
            Gizmos.DrawRay(wallCheckBasePos, -model.right * wallCheckLength);

            Gizmos.color = Color.blue;
            Vector3 ledgeCheckPosition =
                YOffsetPosition(ledgeCheckUpOffset) +
                model.forward * ledgeCheckForwardOffset;

            Gizmos.DrawRay(ledgeCheckPosition, Vector3.down * verticalLedgeCheckLength);
            Gizmos.DrawRay(YOffsetPosition(freeHangUpOffset),
                model.forward * horizontalLedgeCheckLength);
        }

        void OnValidate()
        {
            spreadTable[0] = Vector3.zero;
            spreadTable[1] = Vector3.forward * colliderRadius;
            spreadTable[2] = Vector3.right * colliderRadius;
            spreadTable[3] = Vector3.back * colliderRadius;
            spreadTable[4] = Vector3.left * colliderRadius;
        }
    }
}