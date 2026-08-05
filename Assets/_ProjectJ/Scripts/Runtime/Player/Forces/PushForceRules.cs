using UnityEngine; // Unity 벡터와 수학 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스 선언
{ // 플레이어 기능 묶음
    public static class PushForceRules // 밀치기 힘 결합 규칙 선언
    { // 밀치기 규칙 묶음
        private const float MinimumDirectionSqrMagnitude = 0.0001f; // 유효 방향 최소 제곱 크기

        public static Vector3 CreateHorizontalVelocity(Vector3 direction, float force) // 수평 밀치기 속도 생성
        { // 수평 속도 생성 처리
            Vector3 horizontalDirection = Vector3.ProjectOnPlane(direction, Vector3.up); // 수직 성분을 제거한 방향 계산

            if (horizontalDirection.sqrMagnitude <= MinimumDirectionSqrMagnitude) // 유효한 수평 방향 확인
            { // 빈 방향 처리
                return Vector3.zero; // 힘 없는 속도 반환
            } // 빈 방향 처리 종료

            return horizontalDirection.normalized * Mathf.Max(0f, force); // 음수가 없는 수평 속도 반환
        } // 수평 속도 생성 종료

        public static Vector3 CombineHorizontalVelocity(Vector3 currentVelocity, Vector3 incomingVelocity, float maximumSpeed) // 기존 힘과 새 힘 합산
        { // 밀치기 힘 합산 처리
            Vector3 horizontalCurrent = Vector3.ProjectOnPlane(currentVelocity, Vector3.up); // 기존 힘의 수평 성분 계산
            Vector3 horizontalIncoming = Vector3.ProjectOnPlane(incomingVelocity, Vector3.up); // 새 힘의 수평 성분 계산
            Vector3 combinedVelocity = horizontalCurrent + horizontalIncoming; // 두 수평 힘의 벡터 합산
            float safeMaximumSpeed = Mathf.Max(0f, maximumSpeed); // 음수가 없는 최대 합산 속도 계산

            if (safeMaximumSpeed <= 0f) // 최대 속도 없음 확인
            { // 합산 차단 처리
                return Vector3.zero; // 힘 없는 속도 반환
            } // 합산 차단 처리 종료

            return Vector3.ClampMagnitude(combinedVelocity, safeMaximumSpeed); // 최대 속도로 제한한 합산 결과 반환
        } // 밀치기 힘 합산 종료

        public static bool CanAcceptPush(bool isRespawnProtected, float immunityRemaining) // 밀치기 수신 가능 여부 판정
        { // 밀치기 수신 판정 처리
            return !isRespawnProtected && immunityRemaining <= 0f; // 부활 보호와 피격 면역이 없는 상태 반환
        } // 밀치기 수신 판정 종료

        public static float CalculateImmunityRemaining(float currentRemaining, float deltaTime) // 남은 연속 피격 면역 시간 계산
        { // 면역 시간 계산 처리
            float safeCurrentRemaining = Mathf.Max(0f, currentRemaining); // 음수가 없는 현재 면역 시간 계산
            float safeDeltaTime = Mathf.Max(0f, deltaTime); // 음수가 없는 프레임 시간 계산
            return Mathf.Max(0f, safeCurrentRemaining - safeDeltaTime); // 음수가 없는 남은 면역 시간 반환
        } // 면역 시간 계산 종료
    } // 밀치기 규칙 묶음 종료
} // 플레이어 기능 묶음 종료
