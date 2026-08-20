using UnityEngine;

namespace ProjectJ.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(PlayerLedgeDetector))]
    public sealed class PlayerLedgeClimber : MonoBehaviour
    {
        [SerializeField]
        [Min(0.01f)]
        private float liftDuration = 0.2f;

        [SerializeField]
        [Min(0.01f)]
        private float forwardDuration = 0.2f;

        [SerializeField]
        [Min(0f)]
        private float liftClearance = 0.08f;

        private Rigidbody body;
        private CapsuleCollider capsuleCollider;
        private PlayerLedgeDetector ledgeDetector;

        private ClimbPhase climbPhase;
        private Vector3 phaseStartPosition;
        private Vector3 liftPosition;
        private Vector3 targetPosition;
        private Quaternion startRotation;
        private Quaternion targetRotation;
        private float phaseElapsed;
        private bool previousKinematic;
        private bool previousColliderEnabled;
        private bool previousDetectorEnabled;

        public bool IsClimbing { get; private set; }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            capsuleCollider = GetComponent<CapsuleCollider>();
            ledgeDetector = GetComponent<PlayerLedgeDetector>();

            ApplyFallbackSettings();
        }

        private void FixedUpdate()
        {
            if (!IsClimbing)
            {
                return;
            }

            phaseElapsed += Time.fixedDeltaTime;

            if (climbPhase == ClimbPhase.Lift)
            {
                UpdateLiftPhase();
                return;
            }

            if (climbPhase == ClimbPhase.Forward)
            {
                UpdateForwardPhase();
            }
        }

        private void OnDisable()
        {
            if (!IsClimbing)
            {
                return;
            }

            RestorePhysicsState();
            IsClimbing = false;
            climbPhase = ClimbPhase.None;
        }

        public bool TryStartClimb(bool isCrouching)
        {
            bool canStart = CanStartClimb(
                ledgeDetector != null && ledgeDetector.HasLedge,
                IsClimbing,
                isCrouching
            );

            if (!canStart)
            {
                return false;
            }

            Vector3 ledgeTopPoint =
                ledgeDetector.LedgeTopPoint;

            Vector3 ledgeWallNormal =
                ledgeDetector.LedgeWallNormal;

            float footToBodyOffset =
                CalculateFootToBodyOffset(
                    transform.position.y,
                    capsuleCollider.bounds.min.y
                );

            targetPosition =
                CalculateTargetBodyPosition(
                    ledgeTopPoint,
                    footToBodyOffset
                );

            phaseStartPosition = body.position;

            liftPosition = CalculateLiftPosition(
                phaseStartPosition,
                targetPosition,
                liftClearance
            );

            startRotation = body.rotation;
            targetRotation = CalculateTargetRotation(
                ledgeWallNormal,
                startRotation
            );

            previousKinematic = body.isKinematic;
            previousColliderEnabled = capsuleCollider.enabled;
            previousDetectorEnabled = ledgeDetector.enabled;

            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;

            capsuleCollider.enabled = false;
            ledgeDetector.enabled = false;

            phaseElapsed = 0f;
            climbPhase = ClimbPhase.Lift;
            IsClimbing = true;

            return true;
        }

        private void UpdateLiftPhase()
        {
            float t = CalculatePhaseProgress(
                phaseElapsed,
                liftDuration
            );

            Vector3 nextPosition = Vector3.Lerp(
                phaseStartPosition,
                liftPosition,
                t
            );

            Quaternion nextRotation = Quaternion.Slerp(
                startRotation,
                targetRotation,
                t
            );

            body.MovePosition(nextPosition);
            body.MoveRotation(nextRotation);

            if (t < 1f)
            {
                return;
            }

            phaseStartPosition = liftPosition;
            phaseElapsed = 0f;
            climbPhase = ClimbPhase.Forward;
        }

        private void UpdateForwardPhase()
        {
            float t = CalculatePhaseProgress(
                phaseElapsed,
                forwardDuration
            );

            Vector3 nextPosition = Vector3.Lerp(
                phaseStartPosition,
                targetPosition,
                t
            );

            body.MovePosition(nextPosition);
            body.MoveRotation(targetRotation);

            if (t < 1f)
            {
                return;
            }

            CompleteClimb();
        }

        private void CompleteClimb()
        {
            body.position = targetPosition;
            body.rotation = targetRotation;

            RestorePhysicsState();

            if (!body.isKinematic)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            IsClimbing = false;
            climbPhase = ClimbPhase.None;
            phaseElapsed = 0f;
        }

        private void RestorePhysicsState()
        {
            capsuleCollider.enabled = previousColliderEnabled;
            ledgeDetector.enabled = previousDetectorEnabled;
            body.isKinematic = previousKinematic;
        }

        private void ApplyFallbackSettings()
        {
            if (liftDuration <= 0f)
            {
                liftDuration = 0.2f;
            }

            if (forwardDuration <= 0f)
            {
                forwardDuration = 0.2f;
            }

            if (liftClearance < 0f)
            {
                liftClearance = 0f;
            }
        }

        public static bool CanStartClimb(
            bool hasLedge,
            bool isClimbing,
            bool isCrouching
        )
        {
            return hasLedge &&
                !isClimbing &&
                !isCrouching;
        }

        public static float CalculateFootToBodyOffset(
            float bodyPositionY,
            float colliderBottomY
        )
        {
            return Mathf.Max(
                0f,
                bodyPositionY - colliderBottomY
            );
        }

        public static Vector3 CalculateTargetBodyPosition(
            Vector3 ledgeTopPoint,
            float footToBodyOffset
        )
        {
            return ledgeTopPoint +
                Vector3.up *
                Mathf.Max(
                    0f,
                    footToBodyOffset
                );
        }

        public static Vector3 CalculateLiftPosition(
            Vector3 startPosition,
            Vector3 targetPosition,
            float clearance
        )
        {
            Vector3 result = startPosition;

            result.y = Mathf.Max(
                startPosition.y,
                targetPosition.y +
                    Mathf.Max(
                        0f,
                        clearance
                    )
            );

            return result;
        }

        public static float CalculatePhaseProgress(
            float elapsed,
            float duration
        )
        {
            if (duration <= 0f)
            {
                return 1f;
            }

            return Mathf.Clamp01(
                Mathf.Max(
                    0f,
                    elapsed
                ) / duration
            );
        }

        public static Quaternion CalculateTargetRotation(
            Vector3 wallNormal,
            Quaternion fallbackRotation
        )
        {
            Vector3 facingDirection =
                Vector3.ProjectOnPlane(
                    -wallNormal,
                    Vector3.up
                );

            if (facingDirection.sqrMagnitude <= 0.0001f)
            {
                return fallbackRotation;
            }

            return Quaternion.LookRotation(
                facingDirection.normalized,
                Vector3.up
            );
        }

        private enum ClimbPhase
        {
            None,
            Lift,
            Forward
        }
    }
}
