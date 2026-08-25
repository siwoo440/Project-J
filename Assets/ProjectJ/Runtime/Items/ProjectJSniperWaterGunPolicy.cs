using UnityEngine; // Vector3와 Mathf 사용

namespace ProjectJ.Items
{
    public static class ProjectJSniperWaterGunPolicy
    {
        public const int NetworkItemId = 29; // 저격 물총 고정 Network ID
        public const float PreparationSeconds = 0.8f; // 조준 준비 시간
        public const float RangeMeters = 50f; // 최대 히트스캔 사거리
        public const float HorizontalVelocityChange = 12f; // 적중 수평 외부 속도
        public const int Zoom2X = 2; // 기본 저격 확대
        public const int Zoom4X = 4; // 최대 저격 확대

        private const float DirectionMinimumSqrMagnitude = 0.0001f;
        private const float ScrollDeadZone = 0.0001f;

        public static bool CanBeginAim(
            bool authorityReady,
            bool gameplayAllowed,
            bool alreadyAiming,
            bool slotHasSniper
        )
        {
            return
                authorityReady &&
                gameplayAllowed &&
                !alreadyAiming &&
                slotHasSniper;
        }

        public static bool ShouldCancelAim(
            bool gameplayAllowed,
            bool useHeld,
            bool selectedSlotMatches,
            bool slotStillContainsSniper,
            bool sameRespawnLife
        )
        {
            return
                !gameplayAllowed ||
                !useHeld ||
                !selectedSlotMatches ||
                !slotStillContainsSniper ||
                !sameRespawnLife;
        }

        public static bool IsInRange(
            float distance
        )
        {
            return
                distance >= 0f &&
                distance <= RangeMeters;
        }

        public static float CalculatePreparationProgress(
            float remainingSeconds
        )
        {
            if (PreparationSeconds <= 0f)
            {
                return 1f;
            }

            return
                Mathf.Clamp01(
                    1f -
                    Mathf.Max(
                        0f,
                        remainingSeconds
                    ) /
                    PreparationSeconds
                );
        }

        public static Vector3 ResolveAimDirection(
            Vector3 aimDirection,
            Vector3 fallbackDirection
        )
        {
            if (IsValidDirection(aimDirection))
            {
                return aimDirection.normalized;
            }

            if (IsValidDirection(fallbackDirection))
            {
                return fallbackDirection.normalized;
            }

            return Vector3.forward;
        }

        public static Vector3 CreateHorizontalVelocityChange(
            Vector3 aimDirection,
            Vector3 fallbackForward
        )
        {
            Vector3 resolvedAim =
                ResolveAimDirection(
                    aimDirection,
                    fallbackForward
                );

            Vector3 horizontal =
                new Vector3(
                    resolvedAim.x,
                    0f,
                    resolvedAim.z
                );

            if (!IsValidDirection(horizontal))
            {
                Vector3 resolvedFallback =
                    ResolveAimDirection(
                        fallbackForward,
                        Vector3.forward
                    );

                horizontal =
                    new Vector3(
                        resolvedFallback.x,
                        0f,
                        resolvedFallback.z
                    );
            }

            if (!IsValidDirection(horizontal))
            {
                horizontal =
                    Vector3.forward;
            }

            return
                horizontal.normalized *
                HorizontalVelocityChange;
        }

        public static float CalculateZoomedFieldOfView(
            float baseFieldOfView,
            int zoomMultiplier
        )
        {
            int resolvedZoom =
                zoomMultiplier == Zoom4X
                    ? Zoom4X
                    : Zoom2X;

            return
                Mathf.Clamp(
                    baseFieldOfView /
                    resolvedZoom,
                    1f,
                    179f
                );
        }

        public static int ResolveZoomMultiplier(
            int currentZoomMultiplier,
            float scrollDelta
        )
        {
            int resolvedCurrent =
                currentZoomMultiplier == Zoom4X
                    ? Zoom4X
                    : Zoom2X;

            if (!IsMeaningfulScroll(scrollDelta))
            {
                return resolvedCurrent;
            }

            return
                resolvedCurrent == Zoom2X
                    ? Zoom4X
                    : Zoom2X;
        }

        public static bool IsMeaningfulScroll(
            float scrollDelta
        )
        {
            return
                Mathf.Abs(scrollDelta) >
                ScrollDeadZone;
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
