using UnityEngine; // 안전한 수치 보정 사용

namespace ProjectJ.Items // 아이템 공통 정책 네임스페이스
{
    public static class ProjectJSnowballPolicy // 눈덩이 수치·판정 정책
    {
        public const float DurationSeconds = 3f; // 감속 지속 시간
        public const float MovementSpeedMultiplier = 0.75f; // 이동·달리기 감속 배율
        public const float ProjectileSpeed = 16f; // 투사체 초당 이동 거리
        public const float MaximumTravelDistance = 15f; // 투사체 최대 이동 거리
        public const float CollisionRadius = 0.3f; // 투사체 충돌 반경

        public static float CalculateMovementSpeed( // 감속 후 이동 속도 계산
            float baseSpeed, // 감속 전 이동 속도
            bool isActive // 감속 활성 여부
        )
        {
            float safeBaseSpeed = Mathf.Max(0f, baseSpeed); // 음수 속도 방지

            return isActive
                ? safeBaseSpeed * MovementSpeedMultiplier
                : safeBaseSpeed; // 활성 상태에만 감속 적용
        }

        public static bool CanAffectTarget( // 적중 Target 적용 조건 계산
            bool runnerReady, // Runner 준비 여부
            bool gameplayAllowed, // 경기 입력 허용 여부
            bool isOwner, // 사용자 본인 여부
            bool isFinished, // 완주 여부
            bool isRespawnProtected, // 부활 보호 여부
            bool isShielded // 아이템 보호막 여부
        )
        {
            return
                runnerReady &&
                gameplayAllowed &&
                !isOwner &&
                !isFinished &&
                !isRespawnProtected &&
                !isShielded; // 모든 적중 조건 충족 여부 반환
        }

        public static float GetRefreshedDuration( // 재적중 지속 시간 계산
            float currentRemaining // 기존 남은 시간
        )
        {
            _ = Mathf.Max(0f, currentRemaining); // 잘못된 기존 시간 보정
            return DurationSeconds; // 중첩 없이 3초로 갱신
        }

        public static bool HasReachedTravelLimit( // 최대 거리 도달 여부 계산
            float travelledDistance // 누적 이동 거리
        )
        {
            return Mathf.Max(0f, travelledDistance) >= MaximumTravelDistance; // 15m 이상 제거 판정
        }
    }
}
