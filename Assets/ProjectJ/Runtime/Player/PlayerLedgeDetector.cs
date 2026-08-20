using UnityEngine;

namespace ProjectJ.Player
{
    [RequireComponent(typeof(CapsuleCollider))]
    public sealed class PlayerLedgeDetector : MonoBehaviour
    {
        [SerializeField]
        [Min(0f)]
        private float minLedgeHeight = 0.45f;

        [SerializeField]
        [Min(0f)]
        private float maxLedgeHeight = 1.4f;

        [SerializeField]
        [Min(0f)]
        private float wallCheckDistance = 0.8f;

        [SerializeField]
        [Min(0f)]
        private float topProbeForwardOffset = 0.2f;

        [SerializeField]
        [Min(0f)]
        private float topProbeExtraHeight = 0.2f;

        [SerializeField]
        [Range(0f, 89f)]
        private float topSurfaceMaxAngle = 45f;

        [SerializeField]
        [Min(0f)]
        private float landingForwardOffset = 0.35f;

        [SerializeField]
        [Min(0f)]
        private float landingClearancePadding = 0.03f;

        [SerializeField]
        private LayerMask ledgeLayers;

        private CapsuleCollider capsuleCollider;
        private float standingHeight;
        private float standingRadius;

        public bool HasLedge { get; private set; }

        public Vector3 LedgeWallPoint { get; private set; }

        public Vector3 LedgeTopPoint { get; private set; }

        public Vector3 LedgeWallNormal { get; private set; }

        public Vector3 LedgeTopNormal { get; private set; }

        public float LedgeHeight { get; private set; }

        private void Awake()
        {
            capsuleCollider = GetComponent<CapsuleCollider>();

            standingHeight = capsuleCollider.height;
            standingRadius = capsuleCollider.radius;

            ApplyFallbackSettings();
            ClearLedge();
        }

        private void FixedUpdate()
        {
            DetectLedge();
        }

        private void DetectLedge()
        {
            ClearLedge();

            Vector3 forward = Vector3.ProjectOnPlane(
                transform.forward,
                Vector3.up
            );

            if (forward.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            forward.Normalize();

            Bounds bounds = capsuleCollider.bounds;
            float footY = bounds.min.y;

            float wallProbeHeight =
                Mathf.Max(
                    0.05f,
                    minLedgeHeight * 0.5f
                );

            Vector3 wallOrigin = new Vector3(
                bounds.center.x,
                footY + wallProbeHeight,
                bounds.center.z
            );

            bool hasWall = Physics.Raycast(
                wallOrigin,
                forward,
                out RaycastHit wallHit,
                wallCheckDistance,
                ledgeLayers,
                QueryTriggerInteraction.Ignore
            );

            if (!hasWall)
            {
                return;
            }

            Vector3 upperOrigin = new Vector3(
                bounds.center.x,
                footY + maxLedgeHeight + 0.05f,
                bounds.center.z
            );

            bool upperBlocked = Physics.Raycast(
                upperOrigin,
                forward,
                wallCheckDistance,
                ledgeLayers,
                QueryTriggerInteraction.Ignore
            );

            if (upperBlocked)
            {
                return;
            }

            Vector3 topProbeOrigin =
                wallHit.point +
                forward * topProbeForwardOffset;

            topProbeOrigin.y =
                footY +
                maxLedgeHeight +
                topProbeExtraHeight;

            float topProbeDistance =
                maxLedgeHeight -
                minLedgeHeight +
                topProbeExtraHeight +
                0.1f;

            bool hasTopSurface = Physics.Raycast(
                topProbeOrigin,
                Vector3.down,
                out RaycastHit topHit,
                topProbeDistance,
                ledgeLayers,
                QueryTriggerInteraction.Ignore
            );

            if (!hasTopSurface)
            {
                return;
            }

            float ledgeHeight =
                topHit.point.y - footY;

            bool heightValid = IsLedgeHeightValid(
                ledgeHeight,
                minLedgeHeight,
                maxLedgeHeight
            );

            bool topSurfaceWalkable =
                IsTopSurfaceWalkable(
                    topHit.normal,
                    topSurfaceMaxAngle
                );

            Vector3 landingPoint =
                topHit.point +
                forward * landingForwardOffset;

            bool landingClear =
                HasStandingClearance(
                    landingPoint
                );

            bool validCandidate =
                IsLedgeCandidateValid(
                    true,
                    true,
                    true,
                    heightValid,
                    topSurfaceWalkable,
                    landingClear
                );

            if (!validCandidate)
            {
                return;
            }

            HasLedge = true;
            LedgeWallPoint = wallHit.point;
            LedgeTopPoint = landingPoint;
            LedgeWallNormal = wallHit.normal;
            LedgeTopNormal = topHit.normal;
            LedgeHeight = ledgeHeight;
        }

        private bool HasStandingClearance(
            Vector3 landingPoint
        )
        {
            float horizontalScale = Mathf.Max(
                Mathf.Abs(transform.lossyScale.x),
                Mathf.Abs(transform.lossyScale.z)
            );

            float verticalScale =
                Mathf.Abs(transform.lossyScale.y);

            float radius = Mathf.Max(
                0.01f,
                standingRadius * horizontalScale -
                landingClearancePadding
            );

            float height = Mathf.Max(
                radius * 2f,
                standingHeight * verticalScale
            );

            float halfSegment = Mathf.Max(
                0f,
                height * 0.5f - radius
            );

            float skinOffset = Mathf.Max(
                0.01f,
                landingClearancePadding
            );

            Vector3 center =
                landingPoint +
                Vector3.up *
                (
                    height * 0.5f +
                    skinOffset
                );

            Vector3 pointA =
                center +
                Vector3.up * halfSegment;

            Vector3 pointB =
                center -
                Vector3.up * halfSegment;

            bool colliderWasEnabled =
                capsuleCollider.enabled;

            capsuleCollider.enabled = false;

            bool blocked = Physics.CheckCapsule(
                pointA,
                pointB,
                radius,
                ledgeLayers,
                QueryTriggerInteraction.Ignore
            );

            capsuleCollider.enabled =
                colliderWasEnabled;

            return !blocked;
        }

        private void ClearLedge()
        {
            HasLedge = false;
            LedgeWallPoint = Vector3.zero;
            LedgeTopPoint = Vector3.zero;
            LedgeWallNormal = Vector3.zero;
            LedgeTopNormal = Vector3.zero;
            LedgeHeight = 0f;
        }

        private void ApplyFallbackSettings()
        {
            if (minLedgeHeight <= 0f)
            {
                minLedgeHeight = 0.45f;
            }

            if (maxLedgeHeight <= 0f)
            {
                maxLedgeHeight = 1.4f;
            }

            if (maxLedgeHeight < minLedgeHeight)
            {
                maxLedgeHeight = minLedgeHeight;
            }

            if (wallCheckDistance <= 0f)
            {
                wallCheckDistance = 0.8f;
            }

            if (topProbeForwardOffset <= 0f)
            {
                topProbeForwardOffset = 0.2f;
            }

            if (topProbeExtraHeight <= 0f)
            {
                topProbeExtraHeight = 0.2f;
            }

            topSurfaceMaxAngle = Mathf.Clamp(
                topSurfaceMaxAngle,
                0f,
                89f
            );

            if (topSurfaceMaxAngle <= 0f)
            {
                topSurfaceMaxAngle = 45f;
            }

            if (landingForwardOffset <= 0f)
            {
                landingForwardOffset = 0.35f;
            }

            if (landingClearancePadding < 0f)
            {
                landingClearancePadding = 0f;
            }

            if (ledgeLayers.value == 0)
            {
                ledgeLayers = LayerMask.GetMask(
                    "World",
                    "Obstacle"
                );
            }
        }

        private void OnDrawGizmosSelected()
        {
            CapsuleCollider targetCollider =
                capsuleCollider;

            if (targetCollider == null)
            {
                targetCollider =
                    GetComponent<CapsuleCollider>();
            }

            if (targetCollider == null)
            {
                return;
            }

            Vector3 forward = Vector3.ProjectOnPlane(
                transform.forward,
                Vector3.up
            );

            if (forward.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            forward.Normalize();

            Bounds bounds = targetCollider.bounds;
            float footY = bounds.min.y;

            float wallProbeHeight =
                Mathf.Max(
                    0.05f,
                    minLedgeHeight * 0.5f
                );

            Vector3 wallOrigin = new Vector3(
                bounds.center.x,
                footY + wallProbeHeight,
                bounds.center.z
            );

            Vector3 upperOrigin = new Vector3(
                bounds.center.x,
                footY + maxLedgeHeight + 0.05f,
                bounds.center.z
            );

            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                wallOrigin,
                wallOrigin +
                    forward * wallCheckDistance
            );

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(
                upperOrigin,
                upperOrigin +
                    forward * wallCheckDistance
            );

            if (Application.isPlaying && HasLedge)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(
                    LedgeTopPoint,
                    0.12f
                );

                Gizmos.DrawLine(
                    LedgeWallPoint,
                    LedgeTopPoint
                );
            }
        }

        public static bool IsLedgeHeightValid(
            float ledgeHeight,
            float minHeight,
            float maxHeight
        )
        {
            float safeMin =
                Mathf.Max(
                    0f,
                    minHeight
                );

            float safeMax =
                Mathf.Max(
                    safeMin,
                    maxHeight
                );

            return ledgeHeight >= safeMin &&
                ledgeHeight <= safeMax;
        }

        public static bool IsTopSurfaceWalkable(
            Vector3 surfaceNormal,
            float maxAngle
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
                maxAngle,
                0f,
                89f
            );
        }

        public static bool IsLedgeCandidateValid(
            bool hasWall,
            bool upperClear,
            bool hasTopSurface,
            bool heightValid,
            bool topSurfaceWalkable,
            bool landingClear
        )
        {
            return hasWall &&
                upperClear &&
                hasTopSurface &&
                heightValid &&
                topSurfaceWalkable &&
                landingClear;
        }
    }
}
