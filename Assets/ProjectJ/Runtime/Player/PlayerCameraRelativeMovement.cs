using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectJ.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerInput))]
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
        private Transform cameraReference;

        private Rigidbody body;
        private PlayerInput playerInput;
        private InputAction moveAction;
        private Vector2 moveInput;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            playerInput = GetComponent<PlayerInput>();

            TryFindCamera();
        }

        private void OnEnable()
        {
            moveAction = playerInput.actions.FindAction("Move", true);
            moveAction.performed += OnMoveChanged;
            moveAction.canceled += OnMoveChanged;
        }

        private void OnDisable()
        {
            if (moveAction != null)
            {
                moveAction.performed -= OnMoveChanged;
                moveAction.canceled -= OnMoveChanged;
            }

            moveInput = Vector2.zero;
        }

        private void FixedUpdate()
        {
            if (cameraReference == null)
            {
                TryFindCamera();
            }

            if (cameraReference == null)
            {
                return;
            }

            Vector3 moveDirection = CalculateMoveDirection(
                moveInput,
                cameraReference.forward,
                cameraReference.right
            );

            Vector3 currentVelocity = body.linearVelocity;

            Vector3 horizontalVelocity = CalculateHorizontalVelocity(
                currentVelocity,
                moveDirection,
                moveSpeed,
                acceleration,
                deceleration,
                Time.fixedDeltaTime
            );

            body.linearVelocity = new Vector3(
                horizontalVelocity.x,
                currentVelocity.y,
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

        private void TryFindCamera()
        {
            if (Camera.main != null)
            {
                cameraReference = Camera.main.transform;
            }
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
    }
}
