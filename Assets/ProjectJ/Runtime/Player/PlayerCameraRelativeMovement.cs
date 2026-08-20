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
        private float acceleration = 30f;

        [SerializeField]
        [Min(0f)]
        private float deceleration = 40f;

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
        private Vector2 moveInput;
        private float coyoteTimer;
        private float jumpBufferTimer;

        public bool IsGrounded { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            playerInput = GetComponent<PlayerInput>();
            groundCollider = GetComponent<Collider>();

            body.useGravity = false;

            ApplyFallbackSettings();
            TryFindCamera();
        }

        private void OnEnable()
        {
            moveAction = playerInput.actions.FindAction("Move", true);
            jumpAction = playerInput.actions.FindAction("Jump", true);

            moveAction.performed += OnMoveChanged;
            moveAction.canceled += OnMoveChanged;
            jumpAction.performed += OnJumpPerformed;
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

            moveInput = Vector2.zero;
            coyoteTimer = 0f;
            jumpBufferTimer = 0f;
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

            Vector3 horizontalVelocity = CalculateHorizontalVelocity(
                currentVelocity,
                moveDirection,
                moveSpeed,
                acceleration,
                deceleration,
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
