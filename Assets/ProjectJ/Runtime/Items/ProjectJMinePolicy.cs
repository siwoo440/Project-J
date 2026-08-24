using UnityEngine; // 안전한 수치와 Vector3 계산 사용

namespace ProjectJ.Items // 아이템 공통 정책 네임스페이스
{
    public static class ProjectJMinePolicy // 지뢰 수치·판정 정책
    {
        public const float LifetimeSeconds = 25f; // 설치 유지 시간
        public const float ArmSeconds = 0.75f; // 설치 후 활성화 시간
        public const float TriggerRadius = 2.25f; // 상대 감지 반경
        public const float ExplosionRadius = 3.5f; // 폭발 적용 반경
        public const float OutwardVelocity = 8f; // 바깥쪽 폭발 속도
        public const float UpwardVelocity = 6f; // 위쪽 폭발 속도
        public const float MinimumGroundDot = 0.65f; // 설치 지면 최소 각도 값
        public const float MinimumMineSeparation = 1.5f; // 지뢰 사이 최소 거리
        public const float StartProtectionRadius = 2.5f; // 시작 지점 보호 반경
        public const float StartProtectionVerticalTolerance = 3f; // 시작 지점 수직 허용 범위
        public const float PlacementForwardDistance = 1.5f; // 사용자 전방 설치 거리
        public const float PlacementRayStartHeight = 1.5f; // 설치 Ray 시작 높이
        public const float PlacementRayDistance = 4f; // 설치 Ray 길이
        public const float PlacementWidth = 1.2f; // 설치 공간 너비
        public const float PlacementHeight = 0.4f; // 설치 공간 높이

        public static bool CanPlace( // 설치 가능 여부 계산
            bool groundFound, // 지면 탐색 여부
            float groundDot, // 지면 위쪽 각도 값
            bool commonPlacementAllowed, // 공통 설치 구역 허용 여부
            bool separatedFromMines // 기존 지뢰 간격 허용 여부
        )
        {
            return
                groundFound && // 지면 존재 조건
                groundDot >= MinimumGroundDot && // 경사 허용 조건
                commonPlacementAllowed && // 공통 금지 구역 제외
                separatedFromMines; // 지뢰 중첩 제외
        }

        public static bool CanAffectTarget( // 폭발 Target 적용 가능 여부 계산
            bool runnerReady, // Runner 준비 여부
            bool gameplayAllowed, // 경기 입력 허용 여부
            bool isOwner, // 설치 소유자 여부
            bool isFinished, // 완주 여부
            bool isRespawnProtected, // 부활 보호 여부
            bool isShielded // Jelly 보호막 여부
        )
        {
            return
                runnerReady && // 서버 권한 준비 조건
                gameplayAllowed && // 경기 진행 조건
                !isOwner && // 소유자 제외 조건
                !isFinished && // 완주자 제외 조건
                !isRespawnProtected && // 부활 보호 제외 조건
                !isShielded; // Jelly 보호막 제외 조건
        }

        public static bool ShouldTrigger( // 지뢰 폭발 시작 여부 계산
            bool isArmed, // 활성화 여부
            bool hasValidTarget // 유효 Target 존재 여부
        )
        {
            return isArmed && hasValidTarget; // 활성화 후 상대 접근 조건
        }

        public static bool IsSeparatedFromMine( // 기존 지뢰와 간격 판정
            float distance // 두 지뢰 사이 거리
        )
        {
            return Mathf.Max(0f, distance) >= MinimumMineSeparation; // 최소 거리 경계 포함
        }

        public static bool IsInsideProtectedStartRadius( // Fusion 시작 지점 보호 범위 판정
            Vector3 candidatePosition, // 설치 후보 위치
            Vector3 startPosition // 시작 부활 위치
        )
        {
            float verticalDistance = Mathf.Abs( // 수직 거리 계산
                candidatePosition.y - startPosition.y // 두 위치 높이 차이
            );

            if (verticalDistance > StartProtectionVerticalTolerance) // 다른 높이 구간 확인
            {
                return false; // 수직 보호 범위 밖 처리
            }

            Vector2 candidateHorizontal = new Vector2( // 설치 위치 수평 좌표 생성
                candidatePosition.x, // 설치 위치 X
                candidatePosition.z // 설치 위치 Z
            );
            Vector2 startHorizontal = new Vector2( // 시작 위치 수평 좌표 생성
                startPosition.x, // 시작 위치 X
                startPosition.z // 시작 위치 Z
            );

            return
                (candidateHorizontal - startHorizontal).sqrMagnitude <= // 수평 제곱 거리 비교
                StartProtectionRadius * StartProtectionRadius; // 보호 반경 제곱
        }

        public static Vector3 CreateExplosionVelocityChange( // 위쪽·바깥쪽 폭발 외력 계산
            Vector3 minePosition, // 지뢰 위치
            Vector3 targetPosition, // Target 위치
            Vector3 fallbackDirection // 같은 위치 대체 방향
        )
        {
            Vector3 outwardDirection = targetPosition - minePosition; // Target 바깥 방향 계산
            outwardDirection.y = 0f; // 수평 바깥 방향 유지

            if (outwardDirection.sqrMagnitude <= 0.0001f) // 같은 수평 위치 확인
            {
                outwardDirection = fallbackDirection; // 대체 방향 사용
                outwardDirection.y = 0f; // 대체 방향 수평화
            }

            if (outwardDirection.sqrMagnitude <= 0.0001f) // 대체 방향 누락 확인
            {
                outwardDirection = Vector3.forward; // 기본 전방 사용
            }

            outwardDirection.Normalize(); // 일정한 바깥쪽 힘 유지

            return
                outwardDirection * OutwardVelocity + // 바깥쪽 폭발 속도
                Vector3.up * UpwardVelocity; // 위쪽 폭발 속도
        }
    }
}
