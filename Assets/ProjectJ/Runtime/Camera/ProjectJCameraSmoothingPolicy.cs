using UnityEngine; // Vector3와 Mathf 기능

namespace ProjectJ.CameraSystem // 카메라 시스템 네임스페이스
{
    public static class ProjectJCameraSmoothingPolicy // 프레임 독립 카메라 위치 보간 정책
    {
        public static Vector3 CalculateNextPosition( // 다음 카메라 위치 계산
            Vector3 currentPosition, // 현재 카메라 위치
            Vector3 targetPosition, // 목표 카메라 위치
            float smoothingSpeed, // 초당 보간 속도
            float deltaTime // 현재 프레임 시간
        )
        {
            float safeSpeed = // 음수 방지 보간 속도
                Mathf.Max(0f, smoothingSpeed); // 0 이상 범위 제한

            float safeDeltaTime = // 음수 방지 프레임 시간
                Mathf.Max(0f, deltaTime); // 0 이상 범위 제한

            float interpolationFactor = // 프레임 독립 보간 비율
                1f - // 남은 거리 비율 변환
                Mathf.Exp( // 지수 감쇠 계산
                    -safeSpeed * // 보간 속도 반영
                    safeDeltaTime // 프레임 시간 반영
                );

            return Vector3.LerpUnclamped( // 현재 위치와 목표 위치 보간
                currentPosition, // 보간 시작 위치
                targetPosition, // 보간 목표 위치
                interpolationFactor // 프레임 독립 보간 비율
            );
        }

        public static bool ShouldSnap( // 순간이동 즉시 추적 여부 계산
            Vector3 currentPosition, // 현재 카메라 위치
            Vector3 targetPosition, // 목표 카메라 위치
            float snapDistance // 즉시 추적 거리 기준
        )
        {
            float safeSnapDistance = // 음수 방지 거리 기준
                Mathf.Max(0f, snapDistance); // 0 이상 범위 제한

            float squaredDistance = // 현재 위치와 목표 위치 제곱 거리
                (targetPosition - currentPosition).sqrMagnitude; // 제곱 거리 계산

            return squaredDistance >= // 즉시 추적 경계 비교
                safeSnapDistance * // 거리 기준 제곱 첫 항
                safeSnapDistance; // 거리 기준 제곱 둘째 항
        }
    }
}
