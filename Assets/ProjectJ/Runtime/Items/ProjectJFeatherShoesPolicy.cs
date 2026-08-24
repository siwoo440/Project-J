using UnityEngine; // 안전한 수치 보정 사용

namespace ProjectJ.Items // 아이템 공통 정책 네임스페이스
{
    public static class ProjectJFeatherShoesPolicy // 깃털 신발 수치 정책
    {
        public const float DurationSeconds = 7f; // 효과 지속 시간
        public const float MovementSpeedMultiplier = 1.25f; // 이동·달리기 속도 배율
        public const float SprintStaminaDrainMultiplier = 1.15f; // 달리기 스태미나 소모 배율

        public static float CalculateMovementSpeed( // 최종 이동 속도 계산
            float baseSpeed, // 기본 이동 속도
            bool isActive // 효과 활성 여부
        )
        {
            float safeBaseSpeed = Mathf.Max(0f, baseSpeed); // 음수 속도 방지

            return isActive
                ? safeBaseSpeed * MovementSpeedMultiplier
                : safeBaseSpeed; // 활성 상태에만 속도 강화
        }

        public static float CalculateSprintStaminaDrain( // 최종 초당 소모량 계산
            float baseDrainPerSecond, // 기본 초당 소모량
            bool isActive // 효과 활성 여부
        )
        {
            float safeBaseDrain = Mathf.Max(0f, baseDrainPerSecond); // 음수 소모량 방지

            return isActive
                ? safeBaseDrain * SprintStaminaDrainMultiplier
                : safeBaseDrain; // 활성 상태에만 추가 소모 적용
        }
    }
}
