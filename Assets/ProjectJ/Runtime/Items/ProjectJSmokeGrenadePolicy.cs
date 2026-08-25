using UnityEngine; // Mathf, Vector2, Vector3 사용

namespace ProjectJ.Items
{
    public static class ProjectJSmokeGrenadePolicy
    {
        public const float SmokeDurationSeconds = 6f; // 연막 유지 시간
        public const float MaximumThrowDistance = 14f; // 최대 수평 투척 거리
        public const float SmokeRadius = 5f; // 연막 효과 반경
        public const float OverlayAlpha = 0.6f; // 로컬 월드 시야 감소량
        public const int MaximumActiveZonesPerOwner = 2; // 사용자당 연막 최대 수

        public const float CollisionRadius = 0.3f; // 투척체 충돌 반경
        public const float PrototypeHorizontalThrowSpeed = 12f; // 14m급 포물선용 수평 속도
        public const float PrototypeVerticalThrowSpeed = 6f; // 초기 상승 속도
        public const float PrototypeGravity = -12f; // 포물선 중력

        public static bool CanThrow(
            bool runnerReady,
            bool gameplayAllowed
        )
        {
            return
                runnerReady &&
                gameplayAllowed;
        }

        public static bool IsWithinSmokeRadius(
            float distance
        )
        {
            return
                Mathf.Max(0f, distance) <=
                SmokeRadius;
        }

        public static float ResolveOverlayAlpha(
            int activeZoneCount
        )
        {
            return activeZoneCount > 0
                ? OverlayAlpha
                : 0f;
        }

        public static bool IsBelowFallLimit(
            float currentY,
            float fallLimitY
        )
        {
            return currentY < fallLimitY;
        }

        public static Vector3 CreateInitialVelocity(
            Vector3 forward
        )
        {
            forward.y = 0f;

            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward = Vector3.forward;
            }

            forward.Normalize();

            return
                forward * PrototypeHorizontalThrowSpeed +
                Vector3.up * PrototypeVerticalThrowSpeed;
        }

        public static float GetHorizontalDistance(
            Vector3 origin,
            Vector3 currentPosition
        )
        {
            Vector2 originHorizontal =
                new Vector2(
                    origin.x,
                    origin.z
                );

            Vector2 currentHorizontal =
                new Vector2(
                    currentPosition.x,
                    currentPosition.z
                );

            return Vector2.Distance(
                originHorizontal,
                currentHorizontal
            );
        }

        public static bool ShouldKeepSmokeZone(
            bool lifetimeActive,
            bool anyGameplayActive
        )
        {
            return
                lifetimeActive &&
                anyGameplayActive;
        }
    }
}
