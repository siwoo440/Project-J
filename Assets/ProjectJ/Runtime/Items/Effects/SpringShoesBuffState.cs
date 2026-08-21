using ProjectJ.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectJ.Items.Effects
{
    [DisallowMultipleComponent]
    public sealed class SpringShoesBuffState :
        MonoBehaviour
    {
        private const float ExtraJumpVelocity =
            8f;

        private const float
            CoyoteProtectionWindow =
                0.15f;

        private Rigidbody body;
        private PlayerInput playerInput;

        private PlayerCameraRelativeMovement
            movement;

        private PlayerLedgeClimber
            ledgeClimber;

        private InputAction jumpAction;

        private float activeUntil;
        private float lastGroundedTime;
        private bool extraJumpAvailable;

        public ItemDefinition Definition
        {
            get;
            private set;
        }

        public bool IsActive =>
            Time.time < activeUntil;

        public float RemainingTime =>
            Mathf.Max(
                0f,
                activeUntil - Time.time
            );

        public bool ExtraJumpAvailable =>
            IsActive &&
            extraJumpAvailable;

        private void Awake()
        {
            body =
                GetComponent<Rigidbody>();

            playerInput =
                GetComponent<PlayerInput>();

            movement =
                GetComponent<
                    PlayerCameraRelativeMovement
                >();

            ledgeClimber =
                GetComponent<
                    PlayerLedgeClimber
                >();
        }

        private void OnEnable()
        {
            if (
                playerInput == null ||
                playerInput.actions == null
            )
            {
                return;
            }

            jumpAction =
                playerInput.actions.FindAction(
                    "Jump",
                    false
                );

            if (jumpAction != null)
            {
                jumpAction.performed +=
                    OnJumpPerformed;
            }
        }

        private void OnDisable()
        {
            if (jumpAction != null)
            {
                jumpAction.performed -=
                    OnJumpPerformed;
            }

            jumpAction = null;
        }

        private void FixedUpdate()
        {
            if (!IsActive)
            {
                return;
            }

            if (
                movement != null &&
                movement.IsGrounded
            )
            {
                lastGroundedTime =
                    Time.time;

                extraJumpAvailable =
                    true;
            }
        }

        public void Activate(
            float duration,
            ItemDefinition definition
        )
        {
            Definition = definition;

            activeUntil =
                Mathf.Max(
                    activeUntil,
                    Time.time +
                    Mathf.Max(
                        0.1f,
                        duration
                    )
                );

            extraJumpAvailable =
                true;

            if (
                movement != null &&
                movement.IsGrounded
            )
            {
                lastGroundedTime =
                    Time.time;
            }
        }

        private void OnJumpPerformed(
            InputAction.CallbackContext context
        )
        {
            if (
                !IsActive ||
                !extraJumpAvailable ||
                body == null ||
                movement == null
            )
            {
                return;
            }

            if (movement.IsGrounded)
            {
                return;
            }

            if (
                ledgeClimber != null &&
                ledgeClimber.IsClimbing
            )
            {
                return;
            }

            if (
                Time.time -
                lastGroundedTime <=
                CoyoteProtectionWindow
            )
            {
                return;
            }

            Vector3 velocity =
                body.linearVelocity;

            velocity.y =
                ExtraJumpVelocity;

            body.linearVelocity =
                velocity;

            extraJumpAvailable =
                false;
        }
    }
}
