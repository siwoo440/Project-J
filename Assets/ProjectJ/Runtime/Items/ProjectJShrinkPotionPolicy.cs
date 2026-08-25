using UnityEngine; // Mathf 사용

namespace ProjectJ.Items
{
    public enum ProjectJShrinkPotionState
    {
        Inactive = 0,
        Active = 1,
        RestorePending = 2
    }

    public static class ProjectJShrinkPotionPolicy
    {
        public const float DurationSeconds = 6f; // 소형화 지속 시간
        public const float ScaleMultiplier = 0.8f; // 외형·충돌체 80% 축소

        public const float StandingBaseHeight = 2f; // 현재 Player 기본 Collider 높이
        public const float CrouchBaseHeight = 1f; // 현재 Player 앉기 Collider 높이
        public const float BaseRadius = 0.4f; // 현재 Player 기본 Collider 반경
        public const float RestoreClearanceRadiusScale = 0.95f; // 바닥 접촉 오검출 완화

        public static float CalculateColliderHeight(
            float baseHeight,
            bool shrinkApplied
        )
        {
            float safeHeight =
                Mathf.Max(
                    0f,
                    baseHeight
                );

            return
                shrinkApplied
                    ? safeHeight * ScaleMultiplier
                    : safeHeight;
        }

        public static float CalculateColliderRadius(
            float baseRadius,
            bool shrinkApplied
        )
        {
            float safeRadius =
                Mathf.Max(
                    0f,
                    baseRadius
                );

            return
                shrinkApplied
                    ? safeRadius * ScaleMultiplier
                    : safeRadius;
        }

        public static bool ShouldApplyShrink(
            ProjectJShrinkPotionState state
        )
        {
            return
                state ==
                    ProjectJShrinkPotionState.Active ||
                state ==
                    ProjectJShrinkPotionState.RestorePending;
        }

        public static bool CanUse(
            bool runnerReady,
            bool gameplayAllowed,
            ProjectJShrinkPotionState state
        )
        {
            return
                runnerReady &&
                gameplayAllowed &&
                state ==
                    ProjectJShrinkPotionState.Inactive;
        }

        public static ProjectJShrinkPotionState ResolveExpiredState(
            bool canRestore
        )
        {
            return
                canRestore
                    ? ProjectJShrinkPotionState.Inactive
                    : ProjectJShrinkPotionState.RestorePending;
        }

        public static ProjectJShrinkPotionState ResolvePendingState(
            bool canRestore
        )
        {
            return
                canRestore
                    ? ProjectJShrinkPotionState.Inactive
                    : ProjectJShrinkPotionState.RestorePending;
        }

        public static float CalculateMovementSpeed(
            float baseSpeed
        )
        {
            return baseSpeed;
        }

        public static float CalculateJumpSpeed(
            float baseSpeed
        )
        {
            return baseSpeed;
        }

        public static float CalculatePresentationValue(
            float baseValue,
            bool shrinkApplied
        )
        {
            return
                shrinkApplied
                    ? baseValue * ScaleMultiplier
                    : baseValue;
        }
    }
}
