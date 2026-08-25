using UnityEngine; // Vector3와 거리 보정 사용

namespace ProjectJ.Items
{
    public static class ProjectJGrapplingHookPolicy
    {
        public const string GrappleSurfaceTag = "GrappleSurface";

        public const float DurationSeconds = 1.5f; // 최대 연결 유지 시간
        public const float MaximumRangeMeters = 20f; // 최초 부착 최대 사거리
        public const float PullSpeedMetersPerSecond = 12f; // 자기 이동 속도
        public const float ArrivalDistanceMeters = 0.75f; // Anchor 도착 판정
        public const float SweepRadiusMeters = 0.35f; // 이동 중 충돌 스윕 반경

        public static bool IsWithinInitialRange(
            float distanceMeters
        )
        {
            return
                distanceMeters >= 0f &&
                distanceMeters <= MaximumRangeMeters;
        }

        public static bool HasArrived(
            float distanceMeters
        )
        {
            return
                Mathf.Max(0f, distanceMeters) <=
                ArrivalDistanceMeters;
        }

        public static bool CanActivate(
            bool runnerReady,
            bool gameplayAllowed,
            bool isGrappleSurface,
            float distanceMeters
        )
        {
            return
                runnerReady &&
                gameplayAllowed &&
                isGrappleSurface &&
                IsWithinInitialRange(distanceMeters);
        }

        public static Vector3 CalculatePullVelocity(
            Vector3 playerPosition,
            Vector3 anchorPosition
        )
        {
            Vector3 toAnchor =
                anchorPosition - playerPosition;

            if (toAnchor.sqrMagnitude <= 0.0001f)
            {
                return Vector3.zero;
            }

            return
                toAnchor.normalized *
                PullSpeedMetersPerSecond;
        }

        public static bool CanMaintainConnection(
            bool timerActive,
            bool gameplayAllowed,
            float distanceToAnchor
        )
        {
            return
                timerActive &&
                gameplayAllowed &&
                !HasArrived(distanceToAnchor);
        }
    }
}
