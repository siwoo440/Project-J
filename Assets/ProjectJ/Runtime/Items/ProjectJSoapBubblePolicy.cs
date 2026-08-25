using UnityEngine; // Mathf 사용

namespace ProjectJ.Items
{
    public static class ProjectJSoapBubblePolicy
    {
        public const float DurationSeconds = 2.5f; // 최대 이동 제한 시간
        public const float ProjectileSpeed = 13f; // 직선 투사체 속도
        public const float MaximumTravelDistance = 16f; // 최대 이동 거리
        public const float CollisionRadius = 0.3f; // 프로토타입 충돌 반경
        public const int EscapeJumpPressCount = 6; // 조기 탈출 점프 입력 횟수

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

        public static bool ShouldRestrictLocomotion(
            bool active
        )
        {
            return active;
        }

        public static bool ShouldCountJumpPress(
            bool active,
            bool jumpPressed,
            bool previousJumpPressed
        )
        {
            return
                active &&
                jumpPressed &&
                !previousJumpPressed;
        }

        public static int GetNextJumpPressCount(
            int currentCount
        )
        {
            return Mathf.Clamp(
                Mathf.Max(0, currentCount) + 1,
                0,
                EscapeJumpPressCount
            );
        }

        public static bool HasEscaped(
            int jumpPressCount
        )
        {
            return jumpPressCount >= EscapeJumpPressCount;
        }

        public static bool HasReachedTravelLimit(
            float travelledDistance
        )
        {
            return
                travelledDistance >=
                MaximumTravelDistance;
        }

        public static float GetRefreshedDuration(
            float currentRemaining
        )
        {
            return DurationSeconds;
        }
    }
}
