using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectJ.CameraSystem
{
    [DisallowMultipleComponent]
    public sealed class PlayerThirdPersonCamera : MonoBehaviour
    {
        [SerializeField]
        private Transform target;

        [SerializeField]
        private PlayerInput inputSource;

        [SerializeField]
        private Transform pitchPivot;

        [SerializeField]
        private Camera targetCamera;

        [SerializeField]
        [Min(0f)]
        private float mouseSensitivity = 0.15f;

        [SerializeField]
        private float minPitch = -45f;

        [SerializeField]
        private float maxPitch = 70f;

        [SerializeField]
        [Min(0f)]
        private float targetHeight = 1.5f;

        [SerializeField]
        [Min(0.1f)]
        private float cameraDistance = 7.5f;

        [SerializeField]
        [Min(0.01f)]
        private float collisionRadius = 0.25f;

        [SerializeField]
        [Min(0f)]
        private float collisionPadding = 0.15f;

        [SerializeField]
        [Min(0f)]
        private float cameraReturnSpeed = 12f;

        [SerializeField]
        private LayerMask collisionLayers;

        [SerializeField]
        private bool lockCursorOnPlay = true;

        private InputAction lookAction;
        private float yaw;
        private float pitch;
        private float currentCameraDistance;

        public float Yaw
        {
            get
            {
                return yaw;
            }
        }

        public float Pitch
        {
            get
            {
                return pitch;
            }
        }

        public float CurrentCameraDistance
        {
            get
            {
                return currentCameraDistance;
            }
        }

        private void Awake()
        {
            ApplyFallbackSettings();
            InitializeAngles();

            currentCameraDistance =
                cameraDistance;

            ApplyCameraPosition(
                currentCameraDistance
            );
        }

        private void OnEnable()
        {
            BindLookAction();

            if (
                Application.isPlaying &&
                lockCursorOnPlay
            )
            {
                LockCursor();
            }
        }

        private void OnDisable()
        {
            lookAction = null;

            if (
                Application.isPlaying &&
                lockCursorOnPlay
            )
            {
                UnlockCursor();
            }
        }

        private void LateUpdate()
        {
            if (
                target == null ||
                pitchPivot == null ||
                targetCamera == null
            )
            {
                return;
            }

            Vector2 lookDelta =
                lookAction != null
                    ? lookAction.ReadValue<Vector2>()
                    : Vector2.zero;

            Vector2 nextAngles =
                CalculateNextAngles(
                    yaw,
                    pitch,
                    lookDelta,
                    mouseSensitivity,
                    minPitch,
                    maxPitch
                );

            yaw = nextAngles.x;
            pitch = nextAngles.y;

            transform.position =
                CalculateRigPosition(
                    target.position,
                    targetHeight
                );

            transform.rotation =
                Quaternion.Euler(
                    0f,
                    yaw,
                    0f
                );

            pitchPivot.localRotation =
                Quaternion.Euler(
                    pitch,
                    0f,
                    0f
                );

            UpdateCameraCollision();
        }

        public void Configure(
            Transform newTarget,
            PlayerInput newInputSource,
            Transform newPitchPivot,
            Camera newTargetCamera
        )
        {
            target = newTarget;
            inputSource = newInputSource;
            pitchPivot = newPitchPivot;
            targetCamera = newTargetCamera;

            ApplyFallbackSettings();
            InitializeAngles();

            currentCameraDistance =
                cameraDistance;

            ApplyCameraPosition(
                currentCameraDistance
            );

            if (isActiveAndEnabled)
            {
                BindLookAction();
            }
        }

        private void UpdateCameraCollision()
        {
            float desiredDistance =
                Mathf.Max(
                    0.1f,
                    cameraDistance
                );

            Vector3 origin =
                pitchPivot.position;

            Vector3 direction =
                -pitchPivot.forward;

            bool hasHit =
                Physics.SphereCast(
                    origin,
                    collisionRadius,
                    direction,
                    out RaycastHit hit,
                    desiredDistance,
                    collisionLayers,
                    QueryTriggerInteraction.Ignore
                );

            float allowedDistance =
                CalculateCollisionAdjustedDistance(
                    hasHit,
                    hasHit
                        ? hit.distance
                        : desiredDistance,
                    collisionPadding,
                    desiredDistance
                );

            if (
                allowedDistance <
                currentCameraDistance
            )
            {
                currentCameraDistance =
                    allowedDistance;
            }
            else
            {
                currentCameraDistance =
                    Mathf.MoveTowards(
                        currentCameraDistance,
                        allowedDistance,
                        cameraReturnSpeed *
                        Time.deltaTime
                    );
            }

            ApplyCameraPosition(
                currentCameraDistance
            );
        }

        private void ApplyCameraPosition(
            float distance
        )
        {
            if (targetCamera == null)
            {
                return;
            }

            targetCamera.transform.localPosition =
                new Vector3(
                    0f,
                    0f,
                    -Mathf.Max(
                        0.05f,
                        distance
                    )
                );

            targetCamera.transform.localRotation =
                Quaternion.identity;
        }

        private void BindLookAction()
        {
            lookAction = null;

            if (
                inputSource == null ||
                inputSource.actions == null
            )
            {
                return;
            }

            lookAction =
                inputSource.actions.FindAction(
                    "Look",
                    false
                );
        }

        private void InitializeAngles()
        {
            yaw = NormalizeSignedAngle(
                transform.eulerAngles.y
            );

            if (pitchPivot == null)
            {
                pitch = 0f;
                return;
            }

            pitch = ClampPitch(
                NormalizeSignedAngle(
                    pitchPivot.localEulerAngles.x
                ),
                minPitch,
                maxPitch
            );
        }

        private void ApplyFallbackSettings()
        {
            if (mouseSensitivity <= 0f)
            {
                mouseSensitivity = 0.15f;
            }

            if (maxPitch < minPitch)
            {
                float previousMin =
                    minPitch;

                minPitch =
                    maxPitch;

                maxPitch =
                    previousMin;
            }

            if (targetHeight < 0f)
            {
                targetHeight = 0f;
            }

            if (cameraDistance <= 0f)
            {
                cameraDistance = 7.5f;
            }

            if (collisionRadius <= 0f)
            {
                collisionRadius = 0.25f;
            }

            if (collisionPadding < 0f)
            {
                collisionPadding = 0f;
            }

            if (cameraReturnSpeed <= 0f)
            {
                cameraReturnSpeed = 12f;
            }

            if (collisionLayers.value == 0)
            {
                collisionLayers =
                    LayerMask.GetMask(
                        "World",
                        "Obstacle"
                    );
            }
        }

        private static void LockCursor()
        {
            Cursor.lockState =
                CursorLockMode.Locked;

            Cursor.visible = false;
        }

        private static void UnlockCursor()
        {
            Cursor.lockState =
                CursorLockMode.None;

            Cursor.visible = true;
        }

        public static Vector2 CalculateNextAngles(
            float currentYaw,
            float currentPitch,
            Vector2 lookDelta,
            float sensitivity,
            float minimumPitch,
            float maximumPitch
        )
        {
            float safeSensitivity =
                Mathf.Max(
                    0f,
                    sensitivity
                );

            float nextYaw =
                currentYaw +
                lookDelta.x *
                safeSensitivity;

            float nextPitch =
                currentPitch -
                lookDelta.y *
                safeSensitivity;

            return new Vector2(
                NormalizeSignedAngle(
                    nextYaw
                ),
                ClampPitch(
                    nextPitch,
                    minimumPitch,
                    maximumPitch
                )
            );
        }

        public static float ClampPitch(
            float value,
            float minimumPitch,
            float maximumPitch
        )
        {
            float safeMin =
                Mathf.Min(
                    minimumPitch,
                    maximumPitch
                );

            float safeMax =
                Mathf.Max(
                    minimumPitch,
                    maximumPitch
                );

            return Mathf.Clamp(
                value,
                safeMin,
                safeMax
            );
        }

        public static float NormalizeSignedAngle(
            float angle
        )
        {
            return Mathf.Repeat(
                angle + 180f,
                360f
            ) - 180f;
        }

        public static Vector3 CalculateRigPosition(
            Vector3 targetPosition,
            float targetHeight
        )
        {
            return targetPosition +
                Vector3.up *
                Mathf.Max(
                    0f,
                    targetHeight
                );
        }

        public static float CalculateCollisionAdjustedDistance(
            bool hasHit,
            float hitDistance,
            float padding,
            float desiredDistance
        )
        {
            float safeDesiredDistance =
                Mathf.Max(
                    0.05f,
                    desiredDistance
                );

            if (!hasHit)
            {
                return safeDesiredDistance;
            }

            float safeHitDistance =
                Mathf.Max(
                    0f,
                    hitDistance
                );

            float safePadding =
                Mathf.Max(
                    0f,
                    padding
                );

            return Mathf.Clamp(
                safeHitDistance -
                safePadding,
                0.05f,
                safeDesiredDistance
            );
        }
    }
}
