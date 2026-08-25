using UnityEngine; // 안전한 거리·시간 보정 사용

namespace ProjectJ.Items
{
    public static class ProjectJInkOctopusPolicy
    {
        public const float DurationSeconds = 3.5f; // 먹물 시야 방해 지속 시간
        public const float ProjectileSpeed = 16f; // 투사체 이동 속도
        public const float MaximumTravelDistance = 18f; // 투사체 최대 이동 거리
        public const float CollisionRadius = 0.3f; // 프로토타입 충돌 반경

        public const float OverlayWidthNormalized = 0.9f; // 화면 가로 90%
        public const float OverlayHeightNormalized = 0.75f; // 화면 세로 75%
        public const float OverlayAlpha = 0.82f; // 임시 먹물 불투명도

        public static bool CanAffectTarget(
            bool runnerReady,
            bool gameplayAllowed,
            bool isOwner,
            bool isFinished,
            bool isRespawnProtected
        )
        {
            return
                runnerReady &&
                gameplayAllowed &&
                !isOwner &&
                !isFinished &&
                !isRespawnProtected;
        }

        public static float GetRefreshedDuration(
            float currentRemaining
        )
        {
            _ = Mathf.Max(0f, currentRemaining);
            return DurationSeconds; // 농도 중첩 없이 항상 3.5초로 갱신
        }

        public static bool HasReachedTravelLimit(
            float travelledDistance
        )
        {
            return
                Mathf.Max(0f, travelledDistance) >=
                MaximumTravelDistance;
        }
    }
}
