using ProjectJ.Platforms;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectJ.Player
{
    [DefaultExecutionOrder(50)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(PlayerInput))]
    [RequireComponent(
        typeof(PlayerCameraRelativeMovement)
    )]
    public sealed class PlayerSurfaceInteraction :
        MonoBehaviour
    {
        [SerializeField]
        private LayerMask surfaceLayers;

        [SerializeField]
        [Min(0.05f)]
        private float surfaceProbeDistance =
            0.55f;

        [SerializeField]
        [Min(0.05f)]
        private float springJumpRequestWindow =
            0.25f;

        private Rigidbody body;
        private Collider playerCollider;
        private PlayerInput playerInput;
        private PlayerCameraRelativeMovement
            movement;

        private InputAction jumpAction;

        private SpringPlatform currentSpring;
        private IceSurface currentIce;

        private bool springJumpPending;
        private double springJumpExpireTime;

        private Vector3 previousHorizontalVelocity;

        public SpringPlatform CurrentSpring
        {
            get
            {
                return currentSpring;
            }
        }

        public IceSurface CurrentIce
        {
            get
            {
                return currentIce;
            }
        }

        private void Awake()
        {
            ResolveReferences();

            previousHorizontalVelocity =
                GetHorizontalVelocity(
                    body != null
                        ? body.linearVelocity
                        : Vector3.zero
                );
        }

        private void OnEnable()
        {
            ResolveReferences();
            SubscribeJump();
        }

        private void OnDisable()
        {
            UnsubscribeJump();

            springJumpPending =
                false;

            currentSpring =
                null;

            currentIce =
                null;
        }

        private void FixedUpdate()
        {
            ResolveReferences();

            if (
                body == null ||
                movement == null ||
                !movement.enabled
            )
            {
                return;
            }

            DetectCurrentSurface();

            ApplyIceMovement();

            ApplySpringJump();

            previousHorizontalVelocity =
                GetHorizontalVelocity(
                    body.linearVelocity
                );
        }

        public void Configure(
            LayerMask newSurfaceLayers,
            float newSurfaceProbeDistance,
            float newSpringJumpRequestWindow
        )
        {
            surfaceLayers =
                newSurfaceLayers;

            surfaceProbeDistance =
                Mathf.Max(
                    0.05f,
                    newSurfaceProbeDistance
                );

            springJumpRequestWindow =
                Mathf.Max(
                    0.05f,
                    newSpringJumpRequestWindow
                );

            ResolveReferences();
        }

        public static Vector3
            CalculateIceVelocity(
                Vector3 previousVelocity,
                Vector3 desiredVelocity,
                float changeRate,
                float deltaTime
            )
        {
            Vector3 previous =
                GetHorizontalVelocity(
                    previousVelocity
                );

            Vector3 desired =
                GetHorizontalVelocity(
                    desiredVelocity
                );

            return
                Vector3.MoveTowards(
                    previous,
                    desired,
                    Mathf.Max(
                        0f,
                        changeRate
                    ) *
                    Mathf.Max(
                        0f,
                        deltaTime
                    )
                );
        }

        private void ApplyIceMovement()
        {
            Vector3 desiredHorizontal =
                GetHorizontalVelocity(
                    body.linearVelocity
                );

            if (
                currentIce == null ||
                !movement.IsGrounded
            )
            {
                previousHorizontalVelocity =
                    desiredHorizontal;

                return;
            }

            float changeRate =
                currentIce.SelectChangeRate(
                    previousHorizontalVelocity,
                    desiredHorizontal
                );

            Vector3 iceHorizontal =
                CalculateIceVelocity(
                    previousHorizontalVelocity,
                    desiredHorizontal,
                    changeRate,
                    Time.fixedDeltaTime
                );

            Vector3 velocity =
                body.linearVelocity;

            body.linearVelocity =
                new Vector3(
                    iceHorizontal.x,
                    velocity.y,
                    iceHorizontal.z
                );
        }

        private void ApplySpringJump()
        {
            if (!springJumpPending)
            {
                return;
            }

            if (
                Time.unscaledTimeAsDouble >
                springJumpExpireTime
            )
            {
                springJumpPending =
                    false;

                return;
            }

            if (
                currentSpring == null ||
                body.linearVelocity.y <= 0.1f
            )
            {
                return;
            }

            Vector3 velocity =
                body.linearVelocity;

            velocity.y =
                currentSpring
                    .GetBoostedJumpVelocity(
                        velocity.y
                    );

            body.linearVelocity =
                velocity;

            springJumpPending =
                false;
        }

        private void DetectCurrentSurface()
        {
            currentSpring =
                null;

            currentIce =
                null;

            if (
                playerCollider == null ||
                !movement.IsGrounded
            )
            {
                return;
            }

            Bounds bounds =
                playerCollider.bounds;

            Vector3 origin =
                bounds.center +
                Vector3.up *
                0.05f;

            float castDistance =
                bounds.extents.y +
                surfaceProbeDistance;

            bool hasHit =
                Physics.Raycast(
                    origin,
                    Vector3.down,
                    out RaycastHit hit,
                    castDistance,
                    surfaceLayers,
                    QueryTriggerInteraction.Ignore
                );

            if (!hasHit)
            {
                return;
            }

            currentSpring =
                hit.collider
                    .GetComponentInParent<
                        SpringPlatform
                    >();

            currentIce =
                hit.collider
                    .GetComponentInParent<
                        IceSurface
                    >();
        }

        private void HandleJumpPerformed(
            InputAction.CallbackContext context
        )
        {
            if (
                currentSpring == null ||
                movement == null ||
                !movement.IsGrounded
            )
            {
                return;
            }

            springJumpPending =
                true;

            springJumpExpireTime =
                Time.unscaledTimeAsDouble +
                springJumpRequestWindow;
        }

        private void SubscribeJump()
        {
            if (
                playerInput == null ||
                playerInput.actions == null ||
                jumpAction != null
            )
            {
                return;
            }

            jumpAction =
                playerInput.actions.FindAction(
                    "Jump",
                    false
                );

            if (jumpAction == null)
            {
                return;
            }

            jumpAction.performed +=
                HandleJumpPerformed;
        }

        private void UnsubscribeJump()
        {
            if (jumpAction == null)
            {
                return;
            }

            jumpAction.performed -=
                HandleJumpPerformed;

            jumpAction =
                null;
        }

        private void ResolveReferences()
        {
            if (body == null)
            {
                body =
                    GetComponent<Rigidbody>();
            }

            if (playerCollider == null)
            {
                playerCollider =
                    GetComponent<Collider>();
            }

            if (playerInput == null)
            {
                playerInput =
                    GetComponent<PlayerInput>();
            }

            if (movement == null)
            {
                movement =
                    GetComponent<
                        PlayerCameraRelativeMovement
                    >();
            }

            if (
                surfaceLayers.value == 0
            )
            {
                int worldLayer =
                    LayerMask.NameToLayer(
                        "World"
                    );

                int obstacleLayer =
                    LayerMask.NameToLayer(
                        "Obstacle"
                    );

                int mask =
                    0;

                if (worldLayer >= 0)
                {
                    mask |=
                        1 <<
                        worldLayer;
                }

                if (obstacleLayer >= 0)
                {
                    mask |=
                        1 <<
                        obstacleLayer;
                }

                surfaceLayers =
                    mask;
            }
        }

        private static Vector3
            GetHorizontalVelocity(
                Vector3 velocity
            )
        {
            return
                new Vector3(
                    velocity.x,
                    0f,
                    velocity.z
                );
        }

        private void OnValidate()
        {
            surfaceProbeDistance =
                Mathf.Max(
                    0.05f,
                    surfaceProbeDistance
                );

            springJumpRequestWindow =
                Mathf.Max(
                    0.05f,
                    springJumpRequestWindow
                );
        }
    }
}
