using UnityEngine; // Mathf 사용

namespace ProjectJ.Items
{
    public enum ProjectJGiantBalloonPhase
    {
        Inactive = 0,
        Rising = 1,
        Descending = 2
    }

    public static class ProjectJGiantBalloonPolicy
    {
        public const float RisingDurationSeconds = 6f; // 지속 상승 시간
        public const float DescendingDurationSeconds = 1.5f; // 종료 하강 시간
        public const float RisingSpeed = 4f; // 최소 상승 속도
        public const float HorizontalControlMultiplier = 0.6f; // 수평 조작 60%
        public const float DescendingSpeed = -2f; // 종료 단계 하강 속도

        public static bool CanUse(
            bool runnerReady,
            bool gameplayAllowed,
            bool jetpackActive,
            bool giantBalloonActive
        )
        {
            return
                runnerReady &&
                gameplayAllowed &&
                !jetpackActive &&
                !giantBalloonActive;
        }

        public static bool IsActive(
            ProjectJGiantBalloonPhase phase
        )
        {
            return
                phase !=
                ProjectJGiantBalloonPhase.Inactive;
        }

        public static bool IsRising(
            ProjectJGiantBalloonPhase phase
        )
        {
            return
                phase ==
                ProjectJGiantBalloonPhase.Rising;
        }

        public static bool IsDescending(
            ProjectJGiantBalloonPhase phase
        )
        {
            return
                phase ==
                ProjectJGiantBalloonPhase.Descending;
        }

        public static float CalculateHorizontalMovementSpeed(
            float baseSpeed,
            ProjectJGiantBalloonPhase phase
        )
        {
            float safeBaseSpeed =
                Mathf.Max(
                    0f,
                    baseSpeed
                );

            if (!IsActive(phase))
            {
                return safeBaseSpeed;
            }

            return
                safeBaseSpeed *
                HorizontalControlMultiplier;
        }

        public static float ResolveVerticalVelocity(
            float currentVelocity,
            ProjectJGiantBalloonPhase phase,
            bool gameplayAllowed,
            bool ceilingBlocked,
            bool grounded
        )
        {
            if (
                !gameplayAllowed ||
                !IsActive(phase)
            )
            {
                return currentVelocity;
            }

            if (IsRising(phase))
            {
                if (ceilingBlocked)
                {
                    return Mathf.Min(
                        0f,
                        currentVelocity
                    );
                }

                return Mathf.Max(
                    currentVelocity,
                    RisingSpeed
                );
            }

            if (grounded)
            {
                return 0f;
            }

            return DescendingSpeed;
        }

        public static ProjectJGiantBalloonPhase GetNextPhase(
            ProjectJGiantBalloonPhase phase
        )
        {
            switch (phase)
            {
                case ProjectJGiantBalloonPhase.Rising:
                    return
                        ProjectJGiantBalloonPhase.Descending;

                case ProjectJGiantBalloonPhase.Descending:
                    return
                        ProjectJGiantBalloonPhase.Inactive;

                default:
                    return
                        ProjectJGiantBalloonPhase.Inactive;
            }
        }

        public static float GetPhaseDuration(
            ProjectJGiantBalloonPhase phase
        )
        {
            switch (phase)
            {
                case ProjectJGiantBalloonPhase.Rising:
                    return RisingDurationSeconds;

                case ProjectJGiantBalloonPhase.Descending:
                    return DescendingDurationSeconds;

                default:
                    return 0f;
            }
        }

        public static bool ShouldClear(
            bool gameplayAllowed,
            bool objectValid
        )
        {
            return
                !gameplayAllowed ||
                !objectValid;
        }
    }
}
