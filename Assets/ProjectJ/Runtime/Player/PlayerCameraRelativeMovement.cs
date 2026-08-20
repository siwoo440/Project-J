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

            body.linearVelocity = new Vector3(
                moveDirection.x * moveSpeed,
                currentVelocity.y,
                moveDirection.z * moveSpeed
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
    }
}
