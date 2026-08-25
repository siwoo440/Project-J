using UnityEngine; // 안전한 수치 보정 사용

namespace ProjectJ.Items // 아이템 공통 정책 네임스페이스
{
    public static class ProjectJJetpackPolicy // 제트팩 이동·상태 정책
    {
        public const float DurationSeconds = 5f; // 확정 연료 지속 시간
        public const float PrototypeAscentSpeedMetersPerSecond = 4f; // 프로토타입 상승 속도
        public const float PrototypeHorizontalControlMultiplier = 1f; // 프로토타입 수평 조정 배율
        public const float CeilingProbeSkinMeters = 0.05f; // 천장 판정 여유 거리

        public static bool CanApplyMovement( // 제트팩 이동 적용 여부 계산
            bool isActive, // Networked 활성 상태
            bool gameplayInputAllowed // 경기 조작 허용 여부
        )
        {
            return isActive && gameplayInputAllowed; // 활성·경기 허용 상태만 이동 적용
        }

        public static float CalculateHorizontalMovementSpeed( // 제트팩 수평 이동 속도 계산
            float baseSpeed, // 기존 이동 시스템 속도
            bool isActive // 제트팩 활성 여부
        )
        {
            float safeBaseSpeed = Mathf.Max(0f, baseSpeed); // 음수 이동 속도 방지

            if (!isActive)
            {
                return safeBaseSpeed; // 비활성 상태 기존 속도 유지
            }

            return safeBaseSpeed * PrototypeHorizontalControlMultiplier; // 활성 상태 프로토타입 배율 적용
        }

        public static float ResolveVerticalVelocity( // 제트팩 최종 수직 속도 계산
            float gravityResolvedVelocity, // 기존 중력 계산 이후 수직 속도
            bool isActive, // 제트팩 활성 여부
            bool gameplayInputAllowed, // 경기 조작 허용 여부
            bool ceilingBlocked // 천장 차단 여부
        )
        {
            if (!CanApplyMovement(isActive, gameplayInputAllowed))
            {
                return gravityResolvedVelocity; // 비활성·잠금 상태 기존 이동 유지
            }

            if (ceilingBlocked)
            {
                return Mathf.Min(0f, gravityResolvedVelocity); // 천장 접촉 시 위쪽 속도 제거
            }

            return Mathf.Max( // 기존 점프 상승 속도 보존
                gravityResolvedVelocity, // 중력 반영 기존 속도
                PrototypeAscentSpeedMetersPerSecond // 최소 제트팩 상승 속도
            );
        }
    }
}
