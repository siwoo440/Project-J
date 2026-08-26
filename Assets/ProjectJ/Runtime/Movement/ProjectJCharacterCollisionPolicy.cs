using UnityEngine; // Vector3와 Mathf 사용

namespace ProjectJ.Movement
{
    public static class ProjectJCharacterCollisionPolicy
    {
        private const float DirectionEpsilonSquared =
            0.0001f; // 방향 판정 최소 제곱 크기

        public static float ResolveTravelDistance(
            float requestedDistance,
            float hitDistance
        )
        {
            float safeRequestedDistance =
                Mathf.Max(
                    0f,
                    requestedDistance
                ); // 요청 이동 거리 음수 방지

            return Mathf.Clamp(
                hitDistance,
                0f,
                safeRequestedDistance
            ); // 충돌 거리 범위 제한
        }

        public static Vector3 ResolveSlideDisplacement(
            Vector3 remainingDisplacement,
            Vector3 hitNormal
        )
        {
            remainingDisplacement.y =
                0f; // 수평 이동만 Slide 대상으로 유지

            if (
                remainingDisplacement.sqrMagnitude <=
                DirectionEpsilonSquared
            )
            {
                return Vector3.zero; // 남은 이동 없음 처리
            }

            if (
                hitNormal.sqrMagnitude <=
                DirectionEpsilonSquared
            )
            {
                return Vector3.zero; // 유효하지 않은 충돌 법선 차단
            }

            Vector3 safeNormal =
                hitNormal.normalized; // 충돌 법선 정규화

            Vector3 slideDisplacement =
                Vector3.ProjectOnPlane(
                    remainingDisplacement,
                    safeNormal
                ); // 벽 내부 방향 제거

            slideDisplacement.y =
                0f; // 수직 Slide 성분 제거

            if (
                slideDisplacement.sqrMagnitude <=
                DirectionEpsilonSquared
            )
            {
                return Vector3.zero; // 미세 Slide 제거
            }

            return slideDisplacement; // 벽 접선 이동 반환
        }

        public static bool IsStepHeightAllowed(
            float currentGroundHeight,
            float candidateGroundHeight,
            float maximumStepHeight
        )
        {
            float stepHeight =
                candidateGroundHeight -
                currentGroundHeight; // 후보 계단 높이 계산

            float safeMaximumStepHeight =
                Mathf.Max(
                    0f,
                    maximumStepHeight
                ); // 최대 계단 높이 음수 방지

            return
                stepHeight > 0.01f &&
                stepHeight <=
                safeMaximumStepHeight +
                0.001f; // 작은 상승부터 최대 계단 높이까지 허용
        }

        public static bool IsWalkableGroundNormal(
            Vector3 groundNormal,
            float minimumGroundNormalY
        )
        {
            if (
                groundNormal.sqrMagnitude <=
                DirectionEpsilonSquared
            )
            {
                return false; // 유효하지 않은 Ground 법선 차단
            }

            float safeMinimumGroundNormalY =
                Mathf.Clamp01(
                    minimumGroundNormalY
                ); // Ground 기울기 기준 보정

            return
                groundNormal.normalized.y >=
                safeMinimumGroundNormalY; // 위쪽 면만 Ground로 허용
        }
    }
}
