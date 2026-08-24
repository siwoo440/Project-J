using UnityEngine; // Vector3와 각도 계산 사용

namespace ProjectJ.Items // 아이템 시스템 네임스페이스
{
    public static class ProjectJFireworkPolicy // 폭죽 공통 판정 정책
    {
        public const float PreparationSeconds = 0.9f; // 기획서 준비 시간
        public const float Range = 8f; // 기획서 사거리
        public const float TotalAngle = 100f; // 기획서 전체 부채꼴 각도
        public const float VelocityChange = 9f; // 기획서 외부 속도

        private const float DirectionEpsilon = 0.0001f; // 영벡터 판정 기준
        private const float BoundaryEpsilon = 0.0001f; // 범위 경계 오차

        public static bool CanBeginPreparation( // 준비 시작 가능 여부 계산
            bool runnerAvailable, // Runner 존재 여부
            bool gameplayInputAllowed, // 경기 입력 허용 여부
            bool alreadyPreparing // 기존 준비 상태
        )
        {
            return
                runnerAvailable && // Runner 필수 조건
                gameplayInputAllowed && // 경기 진행 조건
                !alreadyPreparing; // 중복 준비 차단
        }

        public static bool ShouldCancelPreparation( // 준비 취소 여부 계산
            bool gameplayInputAllowed, // 경기 입력 허용 여부
            int startingRespawnCount, // 준비 시작 부활 횟수
            int currentRespawnCount // 현재 부활 횟수
        )
        {
            return
                !gameplayInputAllowed || // 경기 종료·완주 취소
                startingRespawnCount != currentRespawnCount; // 준비 중 부활 취소
        }

        public static bool IsTargetWithinArea( // 전방 범위 Target 판정
            Vector3 origin, // 사용자 위치
            Vector3 forward, // 사용자 전방
            Vector3 targetPosition, // Target 위치
            float range, // 최대 사거리
            float totalAngle // 전체 부채꼴 각도
        )
        {
            float safeRange = Mathf.Max(0f, range); // 음수 사거리 보정
            Vector3 toTarget = targetPosition - origin; // Target 방향 계산

            if (
                safeRange <= 0f || // 사용할 수 없는 사거리 검사
                toTarget.sqrMagnitude <= DirectionEpsilon || // 자기 위치 Target 제외
                forward.sqrMagnitude <= DirectionEpsilon // 전방 정보 누락 제외
            )
            {
                return false; // 잘못된 범위 입력 처리
            }

            float maximumDistanceSquared = safeRange * safeRange; // 사거리 제곱 계산

            if (toTarget.sqrMagnitude > maximumDistanceSquared + BoundaryEpsilon) // 사거리 초과 검사
            {
                return false; // 원거리 Target 제외
            }

            float halfAngle = Mathf.Clamp(totalAngle * 0.5f, 0f, 180f); // 전체 각도를 반각으로 변환
            float targetAngle = Vector3.Angle(forward, toTarget); // 전방 기준 Target 각도 계산
            return targetAngle <= halfAngle + BoundaryEpsilon; // 각도 경계 포함 결과
        }

        public static bool IsTargetWithinDefaultArea( // 확정 폭죽 범위 Target 판정
            Vector3 origin, // 사용자 위치
            Vector3 forward, // 사용자 전방
            Vector3 targetPosition // Target 위치
        )
        {
            return IsTargetWithinArea( // 공통 범위 판정 호출
                origin, // 사용자 위치 전달
                forward, // 사용자 전방 전달
                targetPosition, // Target 위치 전달
                Range, // 확정 사거리 전달
                TotalAngle // 확정 전체 각도 전달
            );
        }

        public static Vector3 CreateHorizontalVelocityChange( // 수평 밀치기 외력 계산
            Vector3 origin, // 사용자 위치
            Vector3 targetPosition, // Target 위치
            Vector3 fallbackForward, // 같은 위치 대체 전방
            float speed // 적용 외부 속도
        )
        {
            Vector3 direction = targetPosition - origin; // 사용자에서 Target 방향 계산
            direction.y = 0f; // 수직 성분 제거

            if (direction.sqrMagnitude <= DirectionEpsilon) // 같은 수평 위치 검사
            {
                direction = fallbackForward; // 사용자 전방 대체
                direction.y = 0f; // 대체 전방 수직 성분 제거
            }

            if (direction.sqrMagnitude <= DirectionEpsilon) // 대체 전방 누락 검사
            {
                direction = Vector3.forward; // 월드 전방 최종 대체
            }

            float safeSpeed = Mathf.Max(0f, speed); // 음수 속도 보정
            return direction.normalized * safeSpeed; // 확정 수평 외력 반환
        }

        public static Vector3 CreateDefaultHorizontalVelocityChange( // 확정 폭죽 수평 외력 계산
            Vector3 origin, // 사용자 위치
            Vector3 targetPosition, // Target 위치
            Vector3 fallbackForward, // 같은 위치 대체 전방
            Vector3 currentExternalVelocity // 기존 외부 속도
        )
        {
            Vector3 desiredVelocity = CreateHorizontalVelocityChange( // 목표 수평 속도 계산
                origin, // 사용자 위치 전달
                targetPosition, // Target 위치 전달
                fallbackForward, // 대체 전방 전달
                VelocityChange // 확정 외부 속도 전달
            );

            Vector3 currentHorizontalVelocity = currentExternalVelocity; // 기존 외부 속도 복사
            currentHorizontalVelocity.y = 0f; // 기존 수직 성분 제외
            return desiredVelocity - currentHorizontalVelocity; // 9m/s 목표 보정량 반환
        }
    }
}
