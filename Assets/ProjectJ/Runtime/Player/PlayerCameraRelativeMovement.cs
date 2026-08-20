using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectJ.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(typeof(Collider))]
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
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction sprintAction;
        private Vector2 moveInput;
        private float coyoteTimer;
        private float jumpBufferTimer;
        private bool sprintHeld;
        private bool sprintExhausted;

        public bool IsGrounded { get; private set; }

        public bool IsSprinting { get; private set; }

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

            moveAction.performed += OnMoveChanged;
            moveAction.canceled += OnMoveChanged;

            jumpAction.performed += OnJumpPerformed;

            sprintAction.performed += OnSprintChanged;
            sprintAction.canceled += OnSprintChanged;
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

            moveInput = Vector2.zero;
            coyoteTimer = 0f;
            jumpBufferTimer = 0f;
            sprintHeld = false;
            sprintExhausted = false;
            IsSprinting = false;
        }

        private void FixedUpdate()
        {
            if (cameraReference == null)
            {
                TryFindCamera();
            }

            Vector3 moveDirection = Vector3.zero;

            if (cameraReference != null)
            {
                moveDirection = CalculateMoveDirection(
                    moveInput,
                    cameraReference.forward,
                    cameraReference.right
                );
            }

            IsGrounded = CheckGrounded();

            Vector3 currentVelocity = body.linearVelocity;

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
                sprintExhausted
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

            float targetMoveSpeed = SelectMoveSpeed(
                IsSprinting,
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

            body.linearVelocity = new Vector3(
                horizontalVelocity.x,
                verticalVelocity,
                horizontalVelocity.z
            );

            if (moveDirection.sqrMagnitude > 0.0001f)
            {
                body.MoveRotation(
                    Quaternion.LookRotation(moveDirection, Vector3.up)
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
            jumpBufferTimer = Mathf.Max(0f, jumpBufferTime);
        }

        private void OnSprintChanged(InputAction.CallbackContext context)
        {
            sprintHeld = context.ReadValueAsButton();

            if (!sprintHeld)
            {
                sprintExhausted = false;
            }
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

        private void ApplyFallbackSettings()
        {
            if (sprintSpeed <= 0f)
            {
                sprintSpeed = 9f;
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
                cameraReference = Camera.main.transform;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Collider targetCollider = groundCollider;

            if (targetCollider == null)
            {
                targetCollider = GetComponent<Collider>();
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

            Gizmos.DrawWireSphere(
                checkPosition,
                groundCheckRadius
            );
        }

        public static Vector3 CalculateMoveDirection(
            Vector2 input,
            Vector3 cameraForward,
            Vector3 cameraRight
        )
        {
            Vector2 clampedInput = Vector2.ClampMagnitude(input, 1f);

            Vector3 forward = Vector3.ProjectOnPlane(
                cameraForward,
                Vector3.up
            );

            Vector3 right = Vector3.ProjectOnPlane(
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
            bool sprintExhausted
        )
        {
            bool hasMoveInput =
                moveInput.sqrMagnitude > 0.0001f;

            return sprintHeld &&
                hasMoveInput &&
                !isAirborne &&
                currentStamina > 0f &&
                !sprintExhausted;
        }

        public static float SelectMoveSpeed(
            bool isSprinting,
            float normalMoveSpeed,
            float sprintMoveSpeed
        )
        {
            return isSprinting
                ? Mathf.Max(0f, sprintMoveSpeed)
                : Mathf.Max(0f, normalMoveSpeed);
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
            float safeMaxStamina = Mathf.Max(0f, maxStamina);
            float safeCurrentStamina = Mathf.Clamp(
                currentStamina,
                0f,
                safeMaxStamina
            );

            float safeDeltaTime = Mathf.Max(0f, deltaTime);

            float staminaChange = isSprinting
                ? -Mathf.Max(0f, drainRate) * safeDeltaTime
                : Mathf.Max(0f, recoveryRate) * safeDeltaTime;

            return Mathf.Clamp(
                safeCurrentStamina + staminaChange,
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
                    Mathf.Max(0f, airAcceleration),
                    Mathf.Max(0f, airDeceleration)
                );
            }

            return new Vector2(
                Mathf.Max(0f, groundAcceleration),
                Mathf.Max(0f, groundDeceleration)
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
            Vector3 currentHorizontalVelocity = new Vector3(
                currentVelocity.x,
                0f,
                currentVelocity.z
            );

            Vector3 clampedDirection = Vector3.ClampMagnitude(
                moveDirection,
                1f
            );

            Vector3 targetVelocity =
                clampedDirection * Mathf.Max(0f, moveSpeed);

            bool hasMoveInput =
                clampedDirection.sqrMagnitude > 0.0001f;

            float changeRate = hasMoveInput
                ? Mathf.Max(0f, acceleration)
                : Mathf.Max(0f, deceleration);

            float maxVelocityChange =
                changeRate * Mathf.Max(0f, deltaTime);

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
            float safeCoyoteTime = Mathf.Max(0f, coyoteTime);

            if (isGrounded)
            {
                return safeCoyoteTime;
            }

            return Mathf.MoveTowards(
                Mathf.Max(0f, currentTimer),
                0f,
                Mathf.Max(0f, deltaTime)
            );
        }

        public static float CalculateJumpBufferTimer(
            float currentTimer,
            float deltaTime
        )
        {
            return Mathf.MoveTowards(
                Mathf.Max(0f, currentTimer),
                0f,
                Mathf.Max(0f, deltaTime)
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
            float safeJumpVelocity = Mathf.Max(0f, jumpVelocity);
            float safeGravity = Mathf.Min(0f, gravity);
            float safeDeltaTime = Mathf.Max(0f, deltaTime);

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
                safeGravity * safeDeltaTime;
        }
    }
}
