using UnityEngine; // Mathf 사용

namespace ProjectJ.Items
{
    public enum ProjectJInvisibilityPresentationMode
    {
        Visible = 0,
        Hidden = 1,
        ProximityShimmer = 2
    }

    public static class ProjectJInvisibilityCloakPolicy
    {
        public const int NetworkItemId = 28; // Installer 실행 전 사용할 고정 Network ID
        public const float DurationSeconds = 5f; // 투명 망토 지속 시간
        public const float ProximityRevealDistance = 2f; // 근거리 흔들림 표시 거리
        public const float ShimmerPeriodSeconds = 0.3f; // 근거리 깜빡임 반복 주기
        public const float ShimmerVisibleSeconds = 0.05f; // 한 주기에서 보이는 시간
        public const float ShimmerHorizontalAmplitude = 0.035f; // 좌우 흔들림 폭

        public static bool CanUse(
            bool authorityReady,
            bool gameplayAllowed,
            bool alreadyActive
        )
        {
            return
                authorityReady &&
                gameplayAllowed &&
                !alreadyActive;
        }

        public static ProjectJInvisibilityPresentationMode ResolvePresentationMode(
            bool isLocalOwner,
            bool invisible,
            float viewerDistance
        )
        {
            if (
                isLocalOwner ||
                !invisible
            )
            {
                return
                    ProjectJInvisibilityPresentationMode.Visible;
            }

            if (
                Mathf.Max(
                    0f,
                    viewerDistance
                ) <=
                ProximityRevealDistance
            )
            {
                return
                    ProjectJInvisibilityPresentationMode.ProximityShimmer;
            }

            return
                ProjectJInvisibilityPresentationMode.Hidden;
        }

        public static bool IsAutoTargetTrackable(
            bool invisible
        )
        {
            return !invisible;
        }

        public static bool ShouldBreakForPush(
            bool invisible
        )
        {
            return invisible;
        }

        public static bool ShouldBreakForSuccessfulItemUse(
            bool invisible,
            bool success,
            bool usedInvisibilityCloak
        )
        {
            return
                invisible &&
                success &&
                !usedInvisibilityCloak;
        }

        public static bool IsShimmerVisible(
            float timeSeconds
        )
        {
            float phase =
                Mathf.Repeat(
                    Mathf.Max(
                        0f,
                        timeSeconds
                    ),
                    ShimmerPeriodSeconds
                );

            return
                phase <
                    ShimmerVisibleSeconds &&
                !Mathf.Approximately(
                    phase,
                    ShimmerVisibleSeconds
                );
        }

        public static float CalculateShimmerOffset(
            float timeSeconds
        )
        {
            float phase =
                Mathf.Max(
                    0f,
                    timeSeconds
                ) /
                ShimmerPeriodSeconds;

            return
                Mathf.Sin(
                    phase *
                    Mathf.PI *
                    2f
                ) *
                ShimmerHorizontalAmplitude;
        }

        public static bool HasDurationExpired(
            float elapsedSeconds
        )
        {
            return
                elapsedSeconds >=
                DurationSeconds;
        }
    }
}
