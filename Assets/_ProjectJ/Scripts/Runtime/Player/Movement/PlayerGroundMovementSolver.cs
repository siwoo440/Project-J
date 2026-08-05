using UnityEngine; // Unity 벡터와 수학 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스 선언
{ // 네임스페이스 범위 시작
    public static class PlayerGroundMovementSolver // 지상 이동 속도 계산 기능 선언
    { // 클래스 범위 시작
        private const float VelocityThreshold = 0.0001f; // 유효 수평 속도 판정 기준

        public static bool IsOppositeDirection(Vector3 currentVelocity, Vector3 targetVelocity) // 현재 속도와 목표 속도의 반대 방향 여부 반환
        { // 메서드 범위 시작
            if (currentVelocity.sqrMagnitude <= VelocityThreshold) // 현재 속도 유효 여부 확인
            { // 조건 범위 시작
                return false; // 정지 상태의 반대 방향 판정 제외
            } // 조건 범위 종료

            if (targetVelocity.sqrMagnitude <= VelocityThreshold) // 목표 속도 유효 여부 확인
            { // 조건 범위 시작
                return false; // 입력 해제 상태의 반대 방향 판정 제외
            } // 조건 범위 종료

            float directionDot = Vector3.Dot(currentVelocity.normalized, targetVelocity.normalized); // 현재 방향과 목표 방향의 내적 계산
            return directionDot < 0f; // 90도보다 큰 반대 방향 입력 여부 반환
        } // 메서드 범위 종료

        public static float SelectAcceleration(Vector3 currentVelocity, Vector3 targetVelocity, float acceleration, float deceleration) // 현재 이동 상태에 맞는 가속도 선택
        { // 메서드 범위 시작
            float safeAcceleration = Mathf.Max(0f, acceleration); // 음수가 아닌 일반 가속도 보정
            float safeDeceleration = Mathf.Max(0f, deceleration); // 음수가 아닌 감속도 보정

            if (targetVelocity.sqrMagnitude <= VelocityThreshold) // 이동 입력 해제 여부 확인
            { // 조건 범위 시작
                return safeDeceleration; // 정지 목표에 지상 감속도 반환
            } // 조건 범위 종료

            if (IsOppositeDirection(currentVelocity, targetVelocity)) // 반대 방향 입력 여부 확인
            { // 조건 범위 시작
                return safeAcceleration + safeDeceleration; // 빠른 정지와 재가속을 합친 전환 가속도 반환
            } // 조건 범위 종료

            return safeAcceleration; // 같은 방향 또는 측면 입력에 일반 가속도 반환
        } // 메서드 범위 종료

        public static Vector3 CalculateNextVelocity(Vector3 currentVelocity, Vector3 targetVelocity, float acceleration, float deceleration, float deltaTime) // 한 프레임 뒤 지상 수평 속도 계산
        { // 메서드 범위 시작
            if (deltaTime <= 0f) // 유효하지 않은 프레임 시간 확인
            { // 조건 범위 시작
                return currentVelocity; // 현재 속도 변경 없이 반환
            } // 조건 범위 종료

            float selectedAcceleration = SelectAcceleration(currentVelocity, targetVelocity, acceleration, deceleration); // 현재 입력 상태에 맞는 가속도 선택
            return Vector3.MoveTowards(currentVelocity, targetVelocity, selectedAcceleration * deltaTime); // 목표 속도를 향한 프레임 단위 속도 전환
        } // 메서드 범위 종료
    } // 클래스 범위 종료
} // 네임스페이스 범위 종료
