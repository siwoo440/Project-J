using UnityEngine; // 거리 보정 사용

namespace ProjectJ.Items // 아이템 공통 정책 네임스페이스
{
    public static class ProjectJPufferBalloonSuitPolicy // 복어 풍선옷 정책
    {
        public const float DurationSeconds = 5f; // 효과 지속 시간
        public const float DetectionRadiusMeters = 1.2f; // 근접 자동 감지 반경
        public const float PushSpeedMetersPerSecond = 6f; // 바깥 방향 외부 속도
        public const float PerTargetCooldownSeconds = 1f; // 대상별 재발동 간격

        public static bool CanActivate( // 아이템 사용 가능 여부 계산
            bool isAlreadyActive, // 현재 효과 활성 상태
            bool gameplayAllowed, // 경기 입력 허용 상태
            bool runnerReady // Fusion Runner·권한 준비 상태
        )
        {
            return
                !isAlreadyActive &&
                gameplayAllowed &&
                runnerReady; // 중첩·경기 잠금·권한 실패 차단
        }

        public static bool IsInsideDetectionRadius( // 감지 반경 포함 여부 계산
            float distanceMeters // 사용자와 대상 사이 거리
        )
        {
            float safeDistance = Mathf.Abs(distanceMeters); // 잘못된 음수 거리 보정
            return safeDistance <= DetectionRadiusMeters; // 경계 포함 판정
        }

        public static bool CanTriggerTarget( // 대상별 자동 밀치기 가능 여부 계산
            bool isSelf, // 자기 자신 여부
            bool isTargetCooldownActive // 대상별 재발동 제한 여부
        )
        {
            return
                !isSelf &&
                !isTargetCooldownActive; // 자신과 쿨타임 중 대상 제외
        }
    }
}
