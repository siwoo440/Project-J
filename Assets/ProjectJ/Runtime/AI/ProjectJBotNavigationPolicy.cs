using System.Collections.Generic; // Route 위치 목록 사용
using UnityEngine; // Vector3와 Mathf 사용

namespace ProjectJ.AI
{
    public static class ProjectJBotNavigationPolicy
    {
        private const float DirectionEpsilonSquared =
            0.0001f; // 방향 계산 최소 제곱 크기

        public static Vector3 ResolvePlanarDirection(
            Vector3 currentPosition,
            Vector3 targetPosition
        )
        {
            Vector3 direction =
                targetPosition -
                currentPosition; // Target 방향 계산

            direction.y =
                0f; // 수직 성분 제거

            if (
                direction.sqrMagnitude <=
                DirectionEpsilonSquared
            )
            {
                return Vector3.zero; // 수평 방향 없음 처리
            }

            return direction.normalized; // 정규화 수평 방향 반환
        }

        public static bool HasReached(
            Vector3 currentPosition,
            Vector3 targetPosition,
            float arrivalRadius
        )
        {
            float safeRadius =
                Mathf.Max(
                    0f,
                    arrivalRadius
                ); // 음수 반경 방지

            return
                (
                    targetPosition -
                    currentPosition
                ).sqrMagnitude <=
                safeRadius *
                safeRadius; // 3D 도달 거리 판정
        }

        public static bool ShouldPulseJump(
            bool requiresJump,
            bool isGrounded,
            float planarDistance,
            float jumpTriggerDistance,
            bool jumpConsumed
        )
        {
            if (
                !requiresJump ||
                !isGrounded ||
                jumpConsumed
            )
            {
                return false; // 점프 불필요 상태 차단
            }

            float safeTriggerDistance =
                Mathf.Max(
                    0f,
                    jumpTriggerDistance
                ); // 음수 점프 거리 방지

            return
                planarDistance <=
                safeTriggerDistance; // 점프 접근 거리 판정
        }

        public static int FindNearestRouteIndex(
            Vector3 currentPosition,
            IReadOnlyList<Vector3> routePositions,
            int minimumIndex
        )
        {
            if (
                routePositions == null ||
                routePositions.Count == 0
            )
            {
                return -1; // Route 없음 처리
            }

            int startIndex =
                Mathf.Clamp(
                    minimumIndex,
                    0,
                    routePositions.Count - 1
                ); // 최소 검색 Index 보정

            int nearestIndex =
                startIndex; // 최초 후보 Index 설정

            float nearestDistanceSquared =
                (
                    routePositions[startIndex] -
                    currentPosition
                ).sqrMagnitude; // 최초 후보 거리 계산

            for (
                int index = startIndex + 1;
                index < routePositions.Count;
                index++
            )
            {
                float candidateDistanceSquared =
                    (
                        routePositions[index] -
                        currentPosition
                    ).sqrMagnitude; // 후보 Route 거리 계산

                if (
                    candidateDistanceSquared >=
                    nearestDistanceSquared
                )
                {
                    continue; // 더 먼 Route 제외
                }

                nearestDistanceSquared =
                    candidateDistanceSquared; // 최근접 거리 갱신

                nearestIndex =
                    index; // 최근접 Index 갱신
            }

            return nearestIndex; // 최근접 Route Index 반환
        }
    }
}
