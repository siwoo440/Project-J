using UnityEngine; // 수치 보정과 Vector3 계산 사용

namespace ProjectJ.Items // 아이템 공통 정책 네임스페이스
{
    public static class ProjectJBombPolicy // 폭탄 투척·폭발 정책
    {
        public const float FuseSeconds = 2.5f; // 기획 신관 시간
        public const float MaximumThrowDistance = 12f; // 기획 최대 수평 투척 거리
        public const float ExplosionRadius = 5f; // 기획 폭발 반경
        public const float CenterForce = 10f; // 폭발 중심 외부 속도
        public const float EdgeForce = 4f; // 폭발 가장자리 외부 속도
        public const float CollisionRadius = 0.3f; // 투척 충돌 검사 반경
        public const float PrototypeHorizontalThrowSpeed = 8f; // 프로토타입 수평 투척 속도
        public const float PrototypeVerticalThrowSpeed = 5f; // 프로토타입 초기 상승 속도
        public const float PrototypeGravity = -12f; // 프로토타입 폭탄 중력

        public static bool CanThrow( // 폭탄 사용 가능 여부 계산
            bool runnerReady, // 서버 Runner 준비 여부
            bool gameplayAllowed, // 경기 입력 허용 여부
            bool hasActiveBomb // 사용자 활성 폭탄 존재 여부
        )
        {
            return
                runnerReady && // 서버 권한 준비 조건
                gameplayAllowed && // 경기 진행 조건
                !hasActiveBomb; // 사용자당 활성 폭탄 1개 제한
        }

        public static bool IsWithinExplosionRadius( // 폭발 반경 포함 여부 계산
            float distance // 폭발 중심과 Target 거리
        )
        {
            return
                Mathf.Max(0f, distance) <=
                ExplosionRadius; // 5m 경계 포함
        }

        public static float CalculateExplosionForce( // 거리 감쇠 외력 계산
            float distance // 폭발 중심과 Target 거리
        )
        {
            float safeDistance = Mathf.Max(0f, distance); // 음수 거리 보정

            if (safeDistance > ExplosionRadius)
            {
                return 0f; // 폭발 반경 밖 외력 없음
            }

            float normalizedDistance = Mathf.Clamp01(
                safeDistance / ExplosionRadius
            ); // 중심 0, 가장자리 1로 정규화

            return Mathf.Lerp(
                CenterForce,
                EdgeForce,
                normalizedDistance
            ); // 중심 10에서 가장자리 4까지 선형 감쇠
        }

        public static Vector3 CreateInitialVelocity( // 포물선 초기 속도 생성
            Vector3 forward // 사용자 전방
        )
        {
            forward.y = 0f; // 수평 방향만 사용

            if (forward.sqrMagnitude <= 0.0001f)
            {
                forward = Vector3.forward; // 잘못된 방향 보정
            }

            forward.Normalize(); // 일정한 수평 속도 유지

            return
                forward * PrototypeHorizontalThrowSpeed +
                Vector3.up * PrototypeVerticalThrowSpeed; // 수평 + 상승 초기 속도
        }

        public static float GetHorizontalDistance( // 투척 수평 거리 계산
            Vector3 origin, // 최초 투척 위치
            Vector3 currentPosition // 현재 폭탄 위치
        )
        {
            Vector2 originHorizontal = new Vector2(
                origin.x,
                origin.z
            ); // 최초 XZ 좌표

            Vector2 currentHorizontal = new Vector2(
                currentPosition.x,
                currentPosition.z
            ); // 현재 XZ 좌표

            return Vector2.Distance(
                originHorizontal,
                currentHorizontal
            ); // 높이를 제외한 실제 투척 거리 반환
        }

        public static Vector3 CreateExplosionVelocityChange( // 폭발 방향과 외력 계산
            Vector3 explosionPosition, // 폭발 중심
            Vector3 targetPosition, // Target 위치
            Vector3 fallbackDirection // 같은 위치 대체 방향
        )
        {
            float distance = Vector3.Distance(
                explosionPosition,
                targetPosition
            ); // 3차원 폭발 거리 계산

            float force = CalculateExplosionForce(distance); // 거리 감쇠 외력 계산

            if (force <= 0f)
            {
                return Vector3.zero; // 범위 밖 외력 없음
            }

            Vector3 direction =
                targetPosition - explosionPosition; // 폭발 바깥 방향 계산

            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = fallbackDirection; // 같은 위치일 때 투척 방향 사용
            }

            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector3.up; // 대체 방향도 없으면 위쪽 사용
            }

            return direction.normalized * force; // 거리 감쇠 외부 속도 반환
        }
    }
}
