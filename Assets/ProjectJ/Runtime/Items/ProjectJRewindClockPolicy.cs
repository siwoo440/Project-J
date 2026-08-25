using UnityEngine; // Vector3와 Mathf 사용

namespace ProjectJ.Items
{
    public static class ProjectJRewindClockPolicy
    {
        public const float HistoryDurationSeconds = 5f; // 되돌릴 과거 기록 길이
        public const float RewindDurationSeconds = 0.8f; // 실제 역재생 시간
        public const float HistoryRetentionSlackSeconds = 0.5f; // 보간용 추가 보관 시간

        public static bool CanUse(
            bool runnerReady,
            bool gameplayAllowed,
            bool hasFullHistory,
            bool targetSafe,
            bool cartRiding,
            bool grapplingHookActive,
            bool rewindActive
        )
        {
            return
                runnerReady &&
                gameplayAllowed &&
                hasFullHistory &&
                targetSafe &&
                !cartRiding &&
                !grapplingHookActive &&
                !rewindActive;
        }

        public static bool ShouldRecord(
            bool gameplayAllowed,
            bool rewindActive
        )
        {
            return
                gameplayAllowed &&
                !rewindActive;
        }

        public static float CalculatePlaybackNormalized(
            float elapsedSeconds
        )
        {
            if (
                float.IsNaN(elapsedSeconds) ||
                float.IsInfinity(elapsedSeconds)
            )
            {
                return 0f;
            }

            return Mathf.Clamp01(
                elapsedSeconds /
                RewindDurationSeconds
            );
        }

        public static float CalculatePlaybackHistoryTime(
            float startHistoryTime,
            float normalized
        )
        {
            return
                startHistoryTime -
                HistoryDurationSeconds *
                Mathf.Clamp01(normalized);
        }

        public static bool IsTargetSafe(
            float targetY,
            float fallLimitY,
            bool finitePosition
        )
        {
            return
                finitePosition &&
                targetY >= fallLimitY;
        }

        public static bool IsFinitePosition(
            Vector3 position
        )
        {
            return
                IsFinite(position.x) &&
                IsFinite(position.y) &&
                IsFinite(position.z);
        }

        public static bool IsPlaybackComplete(
            float elapsedSeconds
        )
        {
            return
                elapsedSeconds >=
                RewindDurationSeconds;
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
