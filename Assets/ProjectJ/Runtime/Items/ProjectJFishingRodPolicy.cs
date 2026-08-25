using UnityEngine; // Vector3와 안전한 거리 계산 사용

namespace ProjectJ.Items
{
    public static class ProjectJFishingRodPolicy
    {
        public const float PullDurationSeconds = 0.6f; // 연결 유지 시간
        public const float MaximumRangeMeters = 14f; // 최대 조준·연결 거리
        public const float PullSpeedMetersPerSecond = 8f; // 대상 당김 속도

        public static bool IsWithinRange(
            float distanceMeters
        )
        {
            return
                distanceMeters >= 0f &&
                distanceMeters <= MaximumRangeMeters;
        }

        public static bool CanAffectTarget(
            bool runnerReady,
            bool gameplayAllowed,
            bool isOwner,
            bool isFinished,
            bool isRespawnProtected,
            bool isShielded
        )
        {
            return
                runnerReady &&
                gameplayAllowed &&
                !isOwner &&
                !isFinished &&
                !isRespawnProtected &&
                !isShielded;
        }

        public static Vector3 CalculatePullVelocity(
            Vector3 sourcePosition,
            Vector3 targetPosition
        )
        {
            Vector3 toSource = sourcePosition - targetPosition;
            toSource.y = 0f; // 기존 수평 외력 규칙에 맞춰 높이 성분 제외

            if (toSource.sqrMagnitude <= 0.0001f)
            {
                return Vector3.zero;
            }

            return
                toSource.normalized *
                PullSpeedMetersPerSecond;
        }

        public static bool CanMaintainConnection(
            bool timerActive,
            bool gameplayAllowed,
            float distanceMeters,
            bool lineClear
        )
        {
            return
                timerActive &&
                gameplayAllowed &&
                IsWithinRange(distanceMeters) &&
                lineClear;
        }
    }
}
