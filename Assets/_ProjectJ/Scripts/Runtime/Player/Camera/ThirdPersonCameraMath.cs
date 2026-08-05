using UnityEngine; // Unity 수학 기능 참조

namespace ProjectJ.Player // 플레이어 카메라 기능 네임스페이스 선언
{ // 카메라 계산 범위
    public static class ThirdPersonCameraMath // 3인칭 카메라 거리와 시야각 계산 도구 선언
    { // 카메라 계산 기능 범위
        public static float CalculateCollisionDistance(float hitDistance, float collisionPadding, float minimumDistance, float maximumDistance) // 벽 충돌 결과 기반 안전 거리 계산
        { // 충돌 거리 계산 범위
            float safeMaximumDistance = Mathf.Max(0f, maximumDistance); // 음수가 아닌 최대 거리 보정
            float safeMinimumDistance = Mathf.Clamp(minimumDistance, 0f, safeMaximumDistance); // 최대 거리 안쪽 최소 거리 보정
            float paddedDistance = Mathf.Max(0f, hitDistance - Mathf.Max(0f, collisionPadding)); // 충돌 여유를 뺀 거리 계산
            return Mathf.Clamp(paddedDistance, safeMinimumDistance, safeMaximumDistance); // 허용 범위의 안전 거리 반환
        } // 충돌 거리 계산 범위 종료

        public static float CalculateSmoothedDistance(float currentDistance, float targetDistance, float recoverySpeed, float deltaTime) // 벽 이탈 시 카메라 거리 복귀 계산
        { // 거리 복귀 계산 범위
            float safeCurrentDistance = Mathf.Max(0f, currentDistance); // 음수가 아닌 현재 거리 보정
            float safeTargetDistance = Mathf.Max(0f, targetDistance); // 음수가 아닌 목표 거리 보정

            if (safeTargetDistance < safeCurrentDistance) // 벽에 가까워져 즉시 축소가 필요한지 확인
            { // 즉시 축소 범위
                return safeTargetDistance; // 카메라 관통 방지용 목표 거리 즉시 반환
            } // 즉시 축소 범위 종료

            float maximumChange = Mathf.Max(0f, recoverySpeed) * Mathf.Max(0f, deltaTime); // 현재 프레임 최대 복귀 거리 계산
            return Mathf.MoveTowards(safeCurrentDistance, safeTargetDistance, maximumChange); // 원래 거리 방향의 부드러운 복귀값 반환
        } // 거리 복귀 계산 범위 종료

        public static float CalculateTargetFieldOfView(bool isSprinting, float normalFieldOfView, float sprintFieldOfView) // 달리기 상태 기반 목표 시야각 계산
        { // 목표 시야각 계산 범위
            float safeNormalFieldOfView = Mathf.Clamp(normalFieldOfView, 1f, 179f); // 기본 시야각 유효 범위 보정
            float safeSprintFieldOfView = Mathf.Clamp(sprintFieldOfView, 1f, 179f); // 달리기 시야각 유효 범위 보정
            return isSprinting ? safeSprintFieldOfView : safeNormalFieldOfView; // 현재 상태에 맞는 목표 시야각 반환
        } // 목표 시야각 계산 범위 종료

        public static float CalculateSmoothedFieldOfView(float currentFieldOfView, float targetFieldOfView, float blendSpeed, float deltaTime) // 현재 시야각의 부드러운 전환 계산
        { // 시야각 전환 계산 범위
            float safeCurrentFieldOfView = Mathf.Clamp(currentFieldOfView, 1f, 179f); // 현재 시야각 유효 범위 보정
            float safeTargetFieldOfView = Mathf.Clamp(targetFieldOfView, 1f, 179f); // 목표 시야각 유효 범위 보정
            float maximumChange = Mathf.Max(0f, blendSpeed) * Mathf.Max(0f, deltaTime); // 현재 프레임 최대 시야각 변화 계산
            return Mathf.MoveTowards(safeCurrentFieldOfView, safeTargetFieldOfView, maximumChange); // 목표 방향의 부드러운 시야각 반환
        } // 시야각 전환 계산 범위 종료
    } // 카메라 계산 기능 범위 종료
} // 카메라 계산 범위 종료
