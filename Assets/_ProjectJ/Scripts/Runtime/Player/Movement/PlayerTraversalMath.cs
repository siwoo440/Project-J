using UnityEngine; // Unity 벡터와 수학 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스 선언
{ // 플레이어 기능 범위
    public static class PlayerTraversalMath // 이동 지형 보정 계산 기능 선언
    { // 지형 보정 계산 범위
        private const float DirectionThreshold = 0.0001f; // 유효 방향 판정 기준

        public static Vector3 CalculateAirVelocity(Vector3 currentVelocity, Vector3 desiredDirection, float groundSpeed, float controlRatio, float acceleration, float deltaTime) // 관성을 보존하는 공중 수평 속도 계산
        { // 공중 속도 계산 범위
            Vector3 horizontalVelocity = Vector3.ProjectOnPlane(currentVelocity, Vector3.up); // 현재 속도의 수평 성분 계산
            Vector3 horizontalDirection = Vector3.ProjectOnPlane(desiredDirection, Vector3.up); // 입력 방향의 수평 성분 계산

            if (horizontalDirection.sqrMagnitude <= DirectionThreshold) // 유효 공중 입력 없음 확인
            { // 관성 유지 범위
                return horizontalVelocity; // 입력 없는 공중 관성 보존
            } // 관성 유지 범위 종료

            float safeGroundSpeed = Mathf.Max(0f, groundSpeed); // 음수가 아닌 지상 기준 속도 보정
            float safeControlRatio = Mathf.Clamp01(controlRatio); // 공중 제어 비율 범위 제한
            float safeAcceleration = Mathf.Max(0f, acceleration); // 음수가 아닌 공중 가속도 보정
            float safeDeltaTime = Mathf.Max(0f, deltaTime); // 음수가 아닌 프레임 시간 보정
            float minimumControlledSpeed = safeGroundSpeed * safeControlRatio; // 정지 상태 공중 제어 속도 계산
            float preservedSpeed = Mathf.Max(horizontalVelocity.magnitude, minimumControlledSpeed); // 기존 관성과 최소 제어 속도 중 큰 값 선택
            Vector3 targetVelocity = horizontalDirection.normalized * preservedSpeed; // 입력 방향의 목표 공중 속도 계산
            return Vector3.MoveTowards(horizontalVelocity, targetVelocity, safeAcceleration * safeDeltaTime); // 제한된 가속도의 공중 방향 전환
        } // 공중 속도 계산 범위 종료

        public static bool IsWalkableSlope(Vector3 groundNormal, float slopeLimit) // 지면 법선의 이동 가능한 경사 여부 반환
        { // 경사 판정 범위
            if (groundNormal.sqrMagnitude <= DirectionThreshold) // 유효하지 않은 지면 법선 확인
            { // 법선 누락 범위
                return false; // 이동 불가능 경사 반환
            } // 법선 누락 범위 종료

            float slopeAngle = Vector3.Angle(groundNormal, Vector3.up); // 지면 법선의 경사 각도 계산
            return slopeAngle <= Mathf.Clamp(slopeLimit, 0f, 90f); // 캐릭터 경사 제한 이내 여부 반환
        } // 경사 판정 범위 종료

        public static Vector3 AlignVelocityToGround(Vector3 velocity, Vector3 groundNormal, float slopeLimit) // 경사면을 따르는 이동 속도 계산
        { // 경사 속도 계산 범위
            if (velocity.sqrMagnitude <= DirectionThreshold) // 유효 이동 속도 없음 확인
            { // 정지 속도 범위
                return Vector3.zero; // 정지 속도 반환
            } // 정지 속도 범위 종료

            if (!IsWalkableSlope(groundNormal, slopeLimit)) // 이동 가능한 경사 여부 확인
            { // 경사 적용 제외 범위
                return velocity; // 기존 속도 유지
            } // 경사 적용 제외 범위 종료

            Vector3 slopeVelocity = Vector3.ProjectOnPlane(velocity, groundNormal); // 경사면 위 이동 방향 계산

            if (slopeVelocity.sqrMagnitude <= DirectionThreshold) // 경사 투영 결과 유효성 확인
            { // 투영 실패 범위
                return velocity; // 기존 속도 유지
            } // 투영 실패 범위 종료

            return slopeVelocity.normalized * velocity.magnitude; // 기존 속력 크기를 보존한 경사 이동 반환
        } // 경사 속도 계산 범위 종료

        public static Vector3 CalculateCornerCorrectedDirection(Vector3 desiredDirection, Vector3 obstacleNormal, float correctionStrength) // 장애물 모서리를 따르는 보정 방향 계산
        { // 모서리 방향 계산 범위
            Vector3 horizontalDirection = Vector3.ProjectOnPlane(desiredDirection, Vector3.up); // 입력 방향의 수평 성분 계산

            if (horizontalDirection.sqrMagnitude <= DirectionThreshold) // 유효 이동 방향 없음 확인
            { // 방향 없음 범위
                return Vector3.zero; // 보정 없는 방향 반환
            } // 방향 없음 범위 종료

            Vector3 horizontalNormal = Vector3.ProjectOnPlane(obstacleNormal, Vector3.up); // 장애물 법선의 수평 성분 계산

            if (horizontalNormal.sqrMagnitude <= DirectionThreshold) // 유효 장애물 법선 없음 확인
            { // 법선 없음 범위
                return horizontalDirection.normalized; // 원래 이동 방향 반환
            } // 법선 없음 범위 종료

            Vector3 slideDirection = Vector3.ProjectOnPlane(horizontalDirection, horizontalNormal.normalized); // 장애물 표면을 따르는 미끄럼 방향 계산

            if (slideDirection.sqrMagnitude <= DirectionThreshold) // 모서리 진행 방향 없음 확인
            { // 정면 장애물 범위
                return horizontalDirection.normalized; // 원래 방향 유지
            } // 정면 장애물 범위 종료

            float safeStrength = Mathf.Clamp01(correctionStrength); // 보정 강도 범위 제한
            return Vector3.Slerp(horizontalDirection.normalized, slideDirection.normalized, safeStrength).normalized; // 원래 방향과 모서리 방향 혼합
        } // 모서리 방향 계산 범위 종료
    } // 지형 보정 계산 범위 종료
} // 플레이어 기능 범위 종료
