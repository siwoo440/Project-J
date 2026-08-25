using UnityEngine; // 안전한 수치 보정 사용

namespace ProjectJ.Items // 아이템 공통 정책 네임스페이스
{
    public static class ProjectJHammerPolicy // 망치 밀치기 강화 정책
    {
        public const float DurationSeconds = 6f; // 망치 지속 시간
        public const float HammerPushRangeMeters = 3.2f; // 망치 적용 밀치기 사거리
        public const float HammerPushForceMetersPerSecond = 11f; // 망치 적용 외부 속도
        public const float HammerPushCooldownSeconds = 1.4f; // 망치 적용 밀치기 재사용 시간

        public static bool CanActivate( // 망치 사용 가능 여부 계산
            bool isAlreadyActive // 현재 망치 활성 상태
        )
        {
            return !isAlreadyActive; // 중첩 활성화 차단
        }

        public static float ResolvePushRange( // 최종 밀치기 사거리 계산
            float baseRange, // 기존 밀치기 사거리
            bool isHammerActive // 망치 활성 상태
        )
        {
            float safeBaseRange = Mathf.Max(0f, baseRange); // 잘못된 음수 기본값 보정

            return isHammerActive
                ? HammerPushRangeMeters // 활성 중 망치 사거리 사용
                : safeBaseRange; // 비활성 중 기존 사거리 유지
        }

        public static float ResolvePushForce( // 최종 밀치기 외력 계산
            float baseForce, // 기존 밀치기 외력
            bool isHammerActive // 망치 활성 상태
        )
        {
            float safeBaseForce = Mathf.Max(0f, baseForce); // 잘못된 음수 기본값 보정

            return isHammerActive
                ? HammerPushForceMetersPerSecond // 활성 중 망치 외력 사용
                : safeBaseForce; // 비활성 중 기존 외력 유지
        }

        public static float ResolvePushCooldown( // 최종 밀치기 재사용 시간 계산
            float baseCooldown, // 기존 밀치기 재사용 시간
            bool isHammerActive // 망치 활성 상태
        )
        {
            float safeBaseCooldown = Mathf.Max(0f, baseCooldown); // 잘못된 음수 기본값 보정

            return isHammerActive
                ? HammerPushCooldownSeconds // 활성 중 망치 재사용 시간 사용
                : safeBaseCooldown; // 비활성 중 기존 재사용 시간 유지
        }
    }
}
