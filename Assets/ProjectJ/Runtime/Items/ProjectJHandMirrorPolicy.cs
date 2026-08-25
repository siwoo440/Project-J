using UnityEngine; // Vector3와 수학 검증 사용

namespace ProjectJ.Items
{
    public static class ProjectJHandMirrorPolicy
    {
        public const int NetworkItemId = 30; // 손거울 고정 Network ID
        public const float DurationSeconds = 4f; // 기획 지속 시간
        public const float ReflectionSeparationMeters = 0.35f; // 반사 직후 자기 Collider 재충돌 방지 간격

        private const float DirectionMinimumSqrMagnitude = 0.0001f;

        public static bool CanActivate(
            bool authorityReady,
            bool runnerReady,
            bool gameplayAllowed
        )
        {
            return
                authorityReady &&
                runnerReady &&
                gameplayAllowed;
        }

        public static bool CanReflect(
            bool authorityReady,
            bool mirrorActive,
            bool gameplayAllowed,
            bool isIncomingOwner,
            bool isRewinding
        )
        {
            return
                authorityReady &&
                mirrorActive &&
                gameplayAllowed &&
                !isIncomingOwner &&
                !isRewinding;
        }

        public static Vector3 ResolveReflectedDirection(
            Vector3 incomingDirection,
            Vector3 fallbackDirection
        )
        {
            Vector3 sourceDirection =
                IsValidDirection(incomingDirection)
                    ? incomingDirection
                    : fallbackDirection;

            if (!IsValidDirection(sourceDirection))
            {
                sourceDirection =
                    Vector3.forward;
            }

            return
                -sourceDirection.normalized;
        }

        public static Vector3 ResolveSeparatedPosition(
            Vector3 contactPoint,
            Vector3 reflectedDirection
        )
        {
            Vector3 direction =
                IsValidDirection(reflectedDirection)
                    ? reflectedDirection.normalized
                    : Vector3.back;

            return
                contactPoint +
                direction *
                ReflectionSeparationMeters;
        }

        public static bool ShouldPreferPreviousOwnerAsTarget(
            bool previousOwnerExists,
            bool previousOwnerGameplayAllowed,
            bool previousOwnerTrackable,
            bool previousOwnerIsNewOwner
        )
        {
            return
                previousOwnerExists &&
                previousOwnerGameplayAllowed &&
                previousOwnerTrackable &&
                !previousOwnerIsNewOwner;
        }

        private static bool IsValidDirection(
            Vector3 direction
        )
        {
            return
                IsFinite(direction.x) &&
                IsFinite(direction.y) &&
                IsFinite(direction.z) &&
                direction.sqrMagnitude >
                DirectionMinimumSqrMagnitude;
        }

        private static bool IsFinite(
            float value
        )
        {
            return
                !float.IsNaN(value) &&
                !float.IsInfinity(value);
        }
    }
}
