using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectJ.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(CapsuleCollider))]
    public sealed class PlayerCameraRelativeMovement : MonoBehaviour
    {
        [SerializeField]
        [Min(0f)]
        private float moveSpeed = 6f;

        [SerializeField]
        [Min(0f)]
        private float sprintSpeed = 9f;

        [SerializeField]
        [Min(0f)]
        private float crouchSpeed = 3.5f;

        [SerializeField]
        [Min(0f)]
        private float crouchHeight = 1.2f;

        [SerializeField]
        [Min(0f)]
        private float standingSpaceCheckPadding = 0.02f;

        [SerializeField]
        [Range(0f, 89f)]
        private float maxSlopeAngle = 45f;

        [SerializeField]
        [Min(0f)]
        private float groundProbeDistance = 0.6f;

        [SerializeField]
        [Min(0f)]
        private float groundSnapDistance = 0.25f;

        [SerializeField]
        [Min(0f)]
        private float groundSnapSpeed = 4f;

        [SerializeField]
        [Min(0f)]
        private float maxStepHeight = 0.4f;

        [SerializeField]
        [Min(0f)]
        private float stepCheckDistance = 0.6f;

        [SerializeField]
        [Min(0f)]
        private float stepUpSpeed = 3f;

        [SerializeField]
        [Min(0f)]
        private float acceleration = 30f;

        [SerializeField]
        [Min(0f)]
        private float deceleration = 40f;

        [SerializeField]
        [Min(0f)]
        private float airAcceleration = 12f;

        [SerializeField]
        [Min(0f)]
        private float airDeceleration = 6f;

        [SerializeField]
        [Min(0f)]
        private float maxStamina = 100f;

        [SerializeField]
        [Min(0f)]
        private float staminaDrainRate = 25f;

        [SerializeField]
        [Min(0f)]
        private float staminaRecoveryRate = 20f;

        [SerializeField]
        [Min(0f)]
        private float jumpVelocity = 8f;

        [SerializeField]
        private float gravity = -22f;

        [SerializeField]
        [Min(0f)]
        private float coyoteTime = 0.12f;

        [SerializeField]
        [Min(0f)]
        private float jumpBufferTime = 0.12f;

        [SerializeField]
        [Min(0f)]
        private float groundCheckRadius = 0.22f;

        [SerializeField]
        [Min(0f)]
        private float groundCheckOffset = 0.08f;

        [SerializeField]
        private LayerMask groundLayers;

        [SerializeField]
        private Transform cameraReference;

        private Rigidbody body;
        private PlayerInput playerInput;
        private Collider groundCollider;
        private CapsuleCollider capsuleCollider;
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction sprintAction;
        private InputAction crouchAction;
        private Vector2 moveInput;
        private float coyoteTimer;
        private float jumpBufferTimer;
        private float standingColliderHeight;
        private Vector3 standingColliderCenter;
        private bool sprintHeld;
        private bool sprintExhausted;
        private bool crouchHeld;

        public bool IsGrounded { get; private set; }

        public bool IsSprinting { get; private set; }

        public bool IsCrouching { get; private set; }

        public float CurrentStamina { get; private set; }

        public float MaxStamina
        {
            get
            {
                return maxStamina;
            }
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            playerInput = GetComponent<PlayerInput>();
            groundCollider = GetComponent<Collider>();
            capsuleCollider = GetComponent<CapsuleCollider>();

            standingColliderHeight = capsuleCollider.height;
            standingColliderCenter = capsuleCollider.center;

            body.useGravity = false;

            ApplyFallbackSettings();
            CurrentStamina = maxStamina;

            TryFindCamera();
        }

        private void OnEnable()
        {
            moveAction = playerInput.actions.FindAction("Move", true);
            jumpAction = playerInput.actions.FindAction("Jump", true);
            sprintAction = playerInput.actions.FindAction("Sprint", true);
            crouchAction = playerInput.actions.FindAction("Crouch", true);

            moveAction.performed += OnMoveChanged;
            moveAction.canceled += OnMoveChanged;

            jumpAction.performed += OnJumpPerformed;

            sprintAction.performed += OnSprintChanged;
            sprintAction.canceled += OnSprintChanged;

            crouchAction.performed += OnCrouchChanged;
            crouchAction.canceled += OnCrouchChanged;
        }

        private void OnDisable()
        {
            if (moveAction != null)
            {
                moveAction.performed -= OnMoveChanged;
                moveAction.canceled -= OnMoveChanged;
            }

            if (jumpAction != null)
            {
                jumpAction.performed -= OnJumpPerformed;
            }

            if (sprintAction != null)
            {
                sprintAction.performed -= OnSprintChanged;
                sprintAction.canceled -= OnSprintChanged;
            }

            if (crouchAction != null)
            {
                crouchAction.performed -= OnCrouchChanged;
                crouchAction.canceled -= OnCrouchChanged;
            }

            moveInput = Vector2.zero;
            coyoteTimer = 0f;
            jumpBufferTimer = 0f;
            sprintHeld = false;
            sprintExhausted = false;
            crouchHeld = false;
            IsSprinting = false;

            ApplyCrouchState(false);
        }

        private void FixedUpdate()
        {
            if (cameraReference == null)
            {
                TryFindCamera();
            }

            bool canStandUp = CanStandUp();
            bool shouldCrouch = DetermineCrouchState(
                crouchHeld,
                IsCrouching,
                canStandUp
            );

            ApplyCrouchState(shouldCrouch);

            Vector3 moveDirection = Vector3.zero;

            if (cameraReference != null)
            {
                moveDirection = CalculateMoveDirection(
                    moveInput,
                    cameraReference.forward,
                    cameraReference.right
                );
            }

            Vector3 currentVelocity = body.linearVelocity;

            bool hasGroundSurface = TryGetGroundSurface(
                out RaycastHit groundHit,
                out float groundGap
            );

            bool hasWalkableGround =
                hasGroundSurface &&
                IsSlopeWalkable(
                    groundHit.normal,
                    maxSlopeAngle
                );

            bool rawGrounded =
                CheckGrounded();

            bool groundedOnWalkableSurface =
                rawGrounded &&
                (
                    !hasGroundSurface ||
                    hasWalkableGround
                );

            bool closeEnoughForSnap =
                hasWalkableGround &&
                currentVelocity.y <= 0.1f &&
                groundGap <= groundSnapDistance;

            IsGrounded =
                groundedOnWalkableSurface ||
                closeEnoughForSnap;

            bool groundedForJump =
                IsGrounded &&
                currentVelocity.y <= 0.1f;

            coyoteTimer = CalculateCoyoteTimer(
                coyoteTimer,
                groundedForJump,
                coyoteTime,
                Time.fixedDeltaTime
            );

            jumpBufferTimer = CalculateJumpBufferTimer(
                jumpBufferTimer,
                Time.fixedDeltaTime
            );

            bool shouldJump = CanUseBufferedJump(
                coyoteTimer,
                jumpBufferTimer
            );

            if (shouldJump)
            {
                coyoteTimer = 0f;
                jumpBufferTimer = 0f;
            }

            bool isAirborne = IsAirborneForHorizontalControl(
                IsGrounded,
                currentVelocity.y,
                shouldJump
            );

            bool sprintAllowed = CanSprint(
                sprintHeld,
                moveInput,
                isAirborne,
                CurrentStamina,
                sprintExhausted,
                IsCrouching
            );

            float nextStamina = CalculateStamina(
                CurrentStamina,
                maxStamina,
                sprintAllowed,
                staminaDrainRate,
                staminaRecoveryRate,
                Time.fixedDeltaTime
            );

            if (sprintAllowed && nextStamina <= 0f)
            {
                sprintExhausted = true;
            }

            CurrentStamina = nextStamina;

            IsSprinting =
                sprintAllowed &&
                CurrentStamina > 0f;

            bool useCrouchSpeed =
                IsCrouching &&
                !isAirborne;

            float targetMoveSpeed = SelectMoveSpeed(
                useCrouchSpeed,
                IsSprinting,
                crouchSpeed,
                moveSpeed,
                sprintSpeed
            );

            Vector2 horizontalChangeRates =
                SelectHorizontalChangeRates(
                    isAirborne,
                    acceleration,
                    deceleration,
                    airAcceleration,
                    airDeceleration
                );

            bool useGroundSurfaceMovement =
                IsGrounded &&
                hasWalkableGround &&
                !shouldJump;

            Vector3 calculatedVelocity;

            if (useGroundSurfaceMovement)
            {
                calculatedVelocity = CalculateSurfaceVelocity(
                    currentVelocity,
                    moveDirection,
                    groundHit.normal,
                    targetMoveSpeed,
                    horizontalChangeRates.x,
                    horizontalChangeRates.y,
                    Time.fixedDeltaTime
                );
            }
            else
            {
                Vector3 horizontalVelocity = CalculateHorizontalVelocity(
                    currentVelocity,
                    moveDirection,
                    targetMoveSpeed,
                    horizontalChangeRates.x,
                    horizontalChangeRates.y,
                    Time.fixedDeltaTime
                );

                float verticalVelocity = CalculateVerticalVelocity(
                    currentVelocity.y,
                    IsGrounded,
                    shouldJump,
                    jumpVelocity,
                    gravity,
                    Time.fixedDeltaTime
                );

                calculatedVelocity = new Vector3(
                    horizontalVelocity.x,
                    verticalVelocity,
                    horizontalVelocity.z
                );
            }

            bool shouldSnapDown = ShouldApplyGroundSnap(
                hasWalkableGround,
                rawGrounded,
                currentVelocity.y,
                groundGap,
                groundSnapDistance,
                shouldJump
            );

            if (shouldSnapDown)
            {
                calculatedVelocity.y = Mathf.Min(
                    calculatedVelocity.y,
                    -groundSnapSpeed
                );
            }

            bool shouldStepUp = TryGetStepAssist(
                moveDirection,
                shouldJump
            );

            if (shouldStepUp)
            {
                calculatedVelocity.y = Mathf.Max(
                    calculatedVelocity.y,
                    stepUpSpeed
                );
            }

            body.linearVelocity = calculatedVelocity;

            if (moveDirection.sqrMagnitude > 0.0001f)
            {
                body.MoveRotation(
                    Quaternion.LookRotation(
                        moveDirection,
                        Vector3.up
                    )
                );
            }
        }

        private void OnMoveChanged(InputAction.CallbackContext context)
        {
            moveInput = Vector2.ClampMagnitude(
                context.ReadValue<Vector2>(),
                1f
            );
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            jumpBufferTimer = Mathf.Max(
                0f,
                jumpBufferTime
            );
        }

        private void OnSprintChanged(InputAction.CallbackContext context)
        {
            sprintHeld = context.ReadValueAsButton();

            if (!sprintHeld)
            {
                sprintExhausted = false;
            }
        }

        private void OnCrouchChanged(InputAction.CallbackContext context)
        {
            crouchHeld = context.ReadValueAsButton();
        }

        private void ApplyCrouchState(bool shouldCrouch)
        {
            if (capsuleCollider == null)
            {
                return;
            }

            IsCrouching = shouldCrouch;

            if (!shouldCrouch)
            {
                capsuleCollider.height =
                    standingColliderHeight;

                capsuleCollider.center =
                    standingColliderCenter;

                return;
            }

            float minimumHeight =
                capsuleCollider.radius * 2f;

            float targetHeight = Mathf.Clamp(
                crouchHeight,
                minimumHeight,
                standingColliderHeight
            );

            capsuleCollider.height = targetHeight;

            Vector3 crouchCenter =
                standingColliderCenter;

            crouchCenter.y = CalculateCrouchCenterY(
                standingColliderCenter.y,
                standingColliderHeight,
                targetHeight
            );

            capsuleCollider.center = crouchCenter;
        }

        private bool CanStandUp()
        {
            if (capsuleCollider == null)
            {
                return true;
            }

            CalculateCapsuleWorldPoints(
                transform,
                standingColliderCenter,
                standingColliderHeight,
                capsuleCollider.radius,
                standingSpaceCheckPadding,
                out Vector3 pointA,
                out Vector3 pointB,
                out float probeRadius
            );

            bool colliderState =
                capsuleCollider.enabled;

            capsuleCollider.enabled = false;

            bool isBlocked = Physics.CheckCapsule(
                pointA,
                pointB,
                probeRadius,
                groundLayers,
                QueryTriggerInteraction.Ignore
            );

            capsuleCollider.enabled =
                colliderState;

            return !isBlocked;
        }

        private bool CheckGrounded()
        {
            Bounds bounds = groundCollider.bounds;

            Vector3 checkPosition = new Vector3(
                bounds.center.x,
                bounds.min.y + groundCheckOffset,
                bounds.center.z
            );

            return Physics.CheckSphere(
                checkPosition,
                groundCheckRadius,
                groundLayers,
                QueryTriggerInteraction.Ignore
            );
        }

        private bool TryGetGroundSurface(
            out RaycastHit hit,
            out float groundGap
        )
        {
            Bounds bounds = groundCollider.bounds;

            const float probeStartOffset = 0.05f;

            Vector3 origin =
                bounds.center +
                Vector3.up * probeStartOffset;

            float distanceFromOriginToBottom =
                bounds.extents.y +
                probeStartOffset;

            float castDistance =
                distanceFromOriginToBottom +
                groundProbeDistance;

            bool hasHit = Physics.Raycast(
                origin,
                Vector3.down,
                out hit,
                castDistance,
                groundLayers,
                QueryTriggerInteraction.Ignore
            );

            if (!hasHit)
            {
                groundGap = float.PositiveInfinity;
                return false;
            }

            groundGap = CalculateGroundGap(
                hit.distance,
                distanceFromOriginToBottom
            );

            return true;
        }

        private bool TryGetStepAssist(
            Vector3 moveDirection,
            bool shouldJump
        )
        {
            if (!IsGrounded)
            {
                return false;
            }

            Vector3 forward = Vector3.ProjectOnPlane(
                moveDirection,
                Vector3.up
            );

            if (forward.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            forward.Normalize();

            Bounds bounds = groundCollider.bounds;

            const float lowerProbeHeight = 0.05f;
            const float upperProbePadding = 0.05f;

            Vector3 lowerOrigin = new Vector3(
                bounds.center.x,
                bounds.min.y + lowerProbeHeight,
                bounds.center.z
            );

            Vector3 upperOrigin = new Vector3(
                bounds.center.x,
                bounds.min.y +
                    maxStepHeight +
                    upperProbePadding,
                bounds.center.z
            );

            bool lowerBlocked = Physics.Raycast(
                lowerOrigin,
                forward,
                out RaycastHit lowerHit,
                stepCheckDistance,
                groundLayers,
                QueryTriggerInteraction.Ignore
            );

            bool upperBlocked = Physics.Raycast(
                upperOrigin,
                forward,
                stepCheckDistance,
                groundLayers,
                QueryTriggerInteraction.Ignore
            );

            bool lowerSurfaceIsStepFace =
                lowerBlocked &&
                lowerHit.normal.y < 0.2f;

            return CanUseStepAssist(
                lowerSurfaceIsStepFace,
                upperBlocked,
                IsGrounded,
                shouldJump,
                body.linearVelocity.y
            );
        }

        private void ApplyFallbackSettings()
        {
            if (sprintSpeed <= 0f)
            {
                sprintSpeed = 9f;
            }

            if (crouchSpeed <= 0f)
            {
                crouchSpeed = 3.5f;
            }

            if (crouchHeight <= 0f)
            {
                crouchHeight = 1.2f;
            }

            if (standingSpaceCheckPadding < 0f)
            {
                standingSpaceCheckPadding = 0f;
            }

            maxSlopeAngle = Mathf.Clamp(
                maxSlopeAngle,
                0f,
                89f
            );

            if (maxSlopeAngle <= 0f)
            {
                maxSlopeAngle = 45f;
            }

            if (groundProbeDistance <= 0f)
            {
                groundProbeDistance = 0.6f;
            }

            if (groundSnapDistance <= 0f)
            {
                groundSnapDistance = 0.25f;
            }

            if (groundSnapSpeed <= 0f)
            {
                groundSnapSpeed = 4f;
            }

            if (maxStepHeight <= 0f)
            {
                maxStepHeight = 0.4f;
            }

            if (stepCheckDistance <= 0f)
            {
                stepCheckDistance = 0.6f;
            }

            if (stepUpSpeed <= 0f)
            {
                stepUpSpeed = 3f;
            }

            if (airAcceleration <= 0f)
            {
                airAcceleration = 12f;
            }

            if (airDeceleration <= 0f)
            {
                airDeceleration = 6f;
            }

            if (maxStamina <= 0f)
            {
                maxStamina = 100f;
            }

            if (staminaDrainRate <= 0f)
            {
                staminaDrainRate = 25f;
            }

            if (staminaRecoveryRate <= 0f)
            {
                staminaRecoveryRate = 20f;
            }

            if (jumpVelocity <= 0f)
            {
                jumpVelocity = 8f;
            }

            if (gravity >= 0f)
            {
                gravity = -22f;
            }

            if (coyoteTime <= 0f)
            {
                coyoteTime = 0.12f;
            }

            if (jumpBufferTime <= 0f)
            {
                jumpBufferTime = 0.12f;
            }

            if (groundCheckRadius <= 0f)
            {
                groundCheckRadius = 0.22f;
            }

            if (groundCheckOffset <= 0f)
            {
                groundCheckOffset = 0.08f;
            }

            if (groundLayers.value == 0)
            {
                groundLayers = LayerMask.GetMask(
                    "World",
                    "Obstacle"
                );
            }
        }

        private void TryFindCamera()
        {
            if (Camera.main != null)
            {
                cameraReference =
                    Camera.main.transform;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Collider targetCollider = groundCollider;

            if (targetCollider == null)
            {
                targetCollider =
                    GetComponent<Collider>();
            }

            if (targetCollider == null)
            {
                return;
            }

            Bounds bounds = targetCollider.bounds;

            Vector3 checkPosition = new Vector3(
                bounds.center.x,
                bounds.min.y + groundCheckOffset,
                bounds.center.z
            );

            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(
                checkPosition,
                groundCheckRadius
            );

            Gizmos.color = Color.cyan;

            Vector3 probeOrigin =
                bounds.center +
                Vector3.up * 0.05f;

            Gizmos.DrawLine(
                probeOrigin,
                probeOrigin +
                    Vector3.down *
                    (
                        bounds.extents.y +
                        0.05f +
                        groundProbeDistance
                    )
            );

            CapsuleCollider targetCapsule =
                capsuleCollider;

            if (targetCapsule == null)
            {
                targetCapsule =
                    GetComponent<CapsuleCollider>();
            }

            if (targetCapsule == null)
            {
                return;
            }

            CalculateCapsuleWorldPoints(
                transform,
                standingColliderCenter == Vector3.zero
                    ? targetCapsule.center
                    : standingColliderCenter,
                standingColliderHeight <= 0f
                    ? targetCapsule.height
                    : standingColliderHeight,
                targetCapsule.radius,
                standingSpaceCheckPadding,
                out Vector3 pointA,
                out Vector3 pointB,
                out float probeRadius
            );

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(
                pointA,
                probeRadius
            );

            Gizmos.DrawWireSphere(
                pointB,
                probeRadius
            );
        }

        public static Vector3 CalculateMoveDirection(
            Vector2 input,
            Vector3 cameraForward,
            Vector3 cameraRight
        )
        {
            Vector2 clampedInput =
                Vector2.ClampMagnitude(
                    input,
                    1f
                );

            Vector3 forward =
                Vector3.ProjectOnPlane(
                    cameraForward,
                    Vector3.up
                );

            Vector3 right =
                Vector3.ProjectOnPlane(
                    cameraRight,
                    Vector3.up
                );

            if (forward.sqrMagnitude > 0.0001f)
            {
                forward.Normalize();
            }

            if (right.sqrMagnitude > 0.0001f)
            {
                right.Normalize();
            }

            Vector3 moveDirection =
                forward * clampedInput.y +
                right * clampedInput.x;

            if (moveDirection.sqrMagnitude > 1f)
            {
                moveDirection.Normalize();
            }

            return moveDirection;
        }

        public static bool DetermineCrouchState(
            bool crouchHeld,
            bool isCurrentlyCrouching,
            bool canStandUp
        )
        {
            if (crouchHeld)
            {
                return true;
            }

            if (
                isCurrentlyCrouching &&
                !canStandUp
            )
            {
                return true;
            }

            return false;
        }

        public static bool IsSlopeWalkable(
            Vector3 surfaceNormal,
            float maxSlopeAngle
        )
        {
            if (surfaceNormal.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            float angle = Vector3.Angle(
                surfaceNormal,
                Vector3.up
            );

            return angle <= Mathf.Clamp(
                maxSlopeAngle,
                0f,
                89f
            );
        }

        public static Vector3 ProjectDirectionOnSlope(
            Vector3 moveDirection,
            Vector3 surfaceNormal
        )
        {
            float inputMagnitude = Mathf.Clamp01(
                moveDirection.magnitude
            );

            if (
                inputMagnitude <= 0f ||
                surfaceNormal.sqrMagnitude <= 0.0001f
            )
            {
                return Vector3.zero;
            }

            Vector3 projected =
                Vector3.ProjectOnPlane(
                    moveDirection,
                    surfaceNormal
                );

            if (projected.sqrMagnitude <= 0.0001f)
            {
                return Vector3.zero;
            }

            return projected.normalized *
                inputMagnitude;
        }

        public static Vector3 CalculateSurfaceVelocity(
            Vector3 currentVelocity,
            Vector3 moveDirection,
            Vector3 surfaceNormal,
            float moveSpeed,
            float acceleration,
            float deceleration,
            float deltaTime
        )
        {
            Vector3 safeNormal =
                surfaceNormal.sqrMagnitude > 0.0001f
                    ? surfaceNormal.normalized
                    : Vector3.up;

            Vector3 currentSurfaceVelocity =
                Vector3.ProjectOnPlane(
                    currentVelocity,
                    safeNormal
                );

            Vector3 surfaceDirection =
                ProjectDirectionOnSlope(
                    moveDirection,
                    safeNormal
                );

            Vector3 targetVelocity =
                surfaceDirection *
                Mathf.Max(0f, moveSpeed);

            bool hasMoveInput =
                moveDirection.sqrMagnitude > 0.0001f;

            float changeRate = hasMoveInput
                ? Mathf.Max(0f, acceleration)
                : Mathf.Max(0f, deceleration);

            return Vector3.MoveTowards(
                currentSurfaceVelocity,
                targetVelocity,
                changeRate *
                    Mathf.Max(0f, deltaTime)
            );
        }

        public static float CalculateGroundGap(
            float groundHitDistance,
            float distanceFromProbeToColliderBottom
        )
        {
            return Mathf.Max(
                0f,
                groundHitDistance -
                    Mathf.Max(
                        0f,
                        distanceFromProbeToColliderBottom
                    )
            );
        }

        public static bool ShouldApplyGroundSnap(
            bool hasWalkableGround,
            bool isAlreadyGrounded,
            float currentVerticalVelocity,
            float groundGap,
            float snapDistance,
            bool shouldJump
        )
        {
            return hasWalkableGround &&
                !isAlreadyGrounded &&
                !shouldJump &&
                currentVerticalVelocity <= 0.1f &&
                groundGap >= 0f &&
                groundGap <= Mathf.Max(
                    0f,
                    snapDistance
                );
        }

        public static bool CanUseStepAssist(
            bool lowerProbeBlocked,
            bool upperProbeBlocked,
            bool isGrounded,
            bool shouldJump,
            float currentVerticalVelocity
        )
        {
            return lowerProbeBlocked &&
                !upperProbeBlocked &&
                isGrounded &&
                !shouldJump &&
                currentVerticalVelocity <= 0.1f;
        }

        public static bool IsAirborneForHorizontalControl(
            bool isGrounded,
            float currentVerticalVelocity,
            bool shouldJump
        )
        {
            return shouldJump ||
                !isGrounded ||
                currentVerticalVelocity > 0.1f;
        }

        public static bool CanSprint(
            bool sprintHeld,
            Vector2 moveInput,
            bool isAirborne,
            float currentStamina,
            bool sprintExhausted,
            bool isCrouching
        )
        {
            bool hasMoveInput =
                moveInput.sqrMagnitude > 0.0001f;

            return sprintHeld &&
                hasMoveInput &&
                !isAirborne &&
                currentStamina > 0f &&
                !sprintExhausted &&
                !isCrouching;
        }

        public static float SelectMoveSpeed(
            bool isCrouching,
            bool isSprinting,
            float crouchMoveSpeed,
            float normalMoveSpeed,
            float sprintMoveSpeed
        )
        {
            if (isCrouching)
            {
                return Mathf.Max(
                    0f,
                    crouchMoveSpeed
                );
            }

            return isSprinting
                ? Mathf.Max(0f, sprintMoveSpeed)
                : Mathf.Max(0f, normalMoveSpeed);
        }

        public static float CalculateCrouchCenterY(
            float standingCenterY,
            float standingHeight,
            float crouchingHeight
        )
        {
            float safeStandingHeight =
                Mathf.Max(
                    0f,
                    standingHeight
                );

            float safeCrouchingHeight =
                Mathf.Clamp(
                    crouchingHeight,
                    0f,
                    safeStandingHeight
                );

            float heightDifference =
                safeStandingHeight -
                safeCrouchingHeight;

            return standingCenterY -
                heightDifference * 0.5f;
        }

        public static void CalculateCapsuleWorldPoints(
            Transform targetTransform,
            Vector3 localCenter,
            float localHeight,
            float localRadius,
            float padding,
            out Vector3 pointA,
            out Vector3 pointB,
            out float probeRadius
        )
        {
            Vector3 lossyScale =
                targetTransform.lossyScale;

            float horizontalScale = Mathf.Max(
                Mathf.Abs(lossyScale.x),
                Mathf.Abs(lossyScale.z)
            );

            float verticalScale =
                Mathf.Abs(lossyScale.y);

            float safeRadius = Mathf.Max(
                0.01f,
                localRadius *
                    horizontalScale
            );

            float safeHeight = Mathf.Max(
                safeRadius * 2f,
                localHeight *
                    verticalScale
            );

            float halfSegmentLength =
                Mathf.Max(
                    0f,
                    safeHeight * 0.5f -
                        safeRadius
                );

            Vector3 worldCenter =
                targetTransform.TransformPoint(
                    localCenter
                );

            Vector3 axis =
                targetTransform.up;

            pointA =
                worldCenter +
                axis * halfSegmentLength;

            pointB =
                worldCenter -
                axis * halfSegmentLength;

            probeRadius = Mathf.Max(
                0.01f,
                safeRadius -
                    Mathf.Max(
                        0f,
                        padding
                    )
            );
        }

        public static float CalculateStamina(
            float currentStamina,
            float maxStamina,
            bool isSprinting,
            float drainRate,
            float recoveryRate,
            float deltaTime
        )
        {
            float safeMaxStamina =
                Mathf.Max(
                    0f,
                    maxStamina
                );

            float safeCurrentStamina =
                Mathf.Clamp(
                    currentStamina,
                    0f,
                    safeMaxStamina
                );

            float safeDeltaTime =
                Mathf.Max(
                    0f,
                    deltaTime
                );

            float staminaChange =
                isSprinting
                    ? -Mathf.Max(
                        0f,
                        drainRate
                    ) * safeDeltaTime
                    : Mathf.Max(
                        0f,
                        recoveryRate
                    ) * safeDeltaTime;

            return Mathf.Clamp(
                safeCurrentStamina +
                    staminaChange,
                0f,
                safeMaxStamina
            );
        }

        public static Vector2 SelectHorizontalChangeRates(
            bool isAirborne,
            float groundAcceleration,
            float groundDeceleration,
            float airAcceleration,
            float airDeceleration
        )
        {
            if (isAirborne)
            {
                return new Vector2(
                    Mathf.Max(
                        0f,
                        airAcceleration
                    ),
                    Mathf.Max(
                        0f,
                        airDeceleration
                    )
                );
            }

            return new Vector2(
                Mathf.Max(
                    0f,
                    groundAcceleration
                ),
                Mathf.Max(
                    0f,
                    groundDeceleration
                )
            );
        }

        public static Vector3 CalculateHorizontalVelocity(
            Vector3 currentVelocity,
            Vector3 moveDirection,
            float moveSpeed,
            float acceleration,
            float deceleration,
            float deltaTime
        )
        {
            Vector3 currentHorizontalVelocity =
                new Vector3(
                    currentVelocity.x,
                    0f,
                    currentVelocity.z
                );

            Vector3 clampedDirection =
                Vector3.ClampMagnitude(
                    moveDirection,
                    1f
                );

            Vector3 targetVelocity =
                clampedDirection *
                Mathf.Max(
                    0f,
                    moveSpeed
                );

            bool hasMoveInput =
                clampedDirection.sqrMagnitude >
                    0.0001f;

            float changeRate =
                hasMoveInput
                    ? Mathf.Max(
                        0f,
                        acceleration
                    )
                    : Mathf.Max(
                        0f,
                        deceleration
                    );

            float maxVelocityChange =
                changeRate *
                Mathf.Max(
                    0f,
                    deltaTime
                );

            return Vector3.MoveTowards(
                currentHorizontalVelocity,
                targetVelocity,
                maxVelocityChange
            );
        }

        public static float CalculateCoyoteTimer(
            float currentTimer,
            bool isGrounded,
            float coyoteTime,
            float deltaTime
        )
        {
            float safeCoyoteTime =
                Mathf.Max(
                    0f,
                    coyoteTime
                );

            if (isGrounded)
            {
                return safeCoyoteTime;
            }

            return Mathf.MoveTowards(
                Mathf.Max(
                    0f,
                    currentTimer
                ),
                0f,
                Mathf.Max(
                    0f,
                    deltaTime
                )
            );
        }

        public static float CalculateJumpBufferTimer(
            float currentTimer,
            float deltaTime
        )
        {
            return Mathf.MoveTowards(
                Mathf.Max(
                    0f,
                    currentTimer
                ),
                0f,
                Mathf.Max(
                    0f,
                    deltaTime
                )
            );
        }

        public static bool CanUseBufferedJump(
            float coyoteTimer,
            float jumpBufferTimer
        )
        {
            return coyoteTimer > 0f &&
                jumpBufferTimer > 0f;
        }

        public static float CalculateVerticalVelocity(
            float currentVerticalVelocity,
            bool isGrounded,
            bool shouldJump,
            float jumpVelocity,
            float gravity,
            float deltaTime
        )
        {
            float safeJumpVelocity =
                Mathf.Max(
                    0f,
                    jumpVelocity
                );

            float safeGravity =
                Mathf.Min(
                    0f,
                    gravity
                );

            float safeDeltaTime =
                Mathf.Max(
                    0f,
                    deltaTime
                );

            if (shouldJump)
            {
                return safeJumpVelocity;
            }

            bool canUseGroundState =
                isGrounded &&
                currentVerticalVelocity <= 0.1f;

            if (canUseGroundState)
            {
                return 0f;
            }

            return currentVerticalVelocity +
                safeGravity *
                    safeDeltaTime;
        }
    }
}
