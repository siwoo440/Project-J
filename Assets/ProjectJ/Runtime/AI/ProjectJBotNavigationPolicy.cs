using System.Collections.Generic; // Route 위치와 순서 목록 사용
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

        public static int ResolveCheckpointMinimumRouteOrder(
            int checkpointId,
            int routeOrderPerCheckpoint = 100
        )
        {
            int safeCheckpointId =
                Mathf.Max(
                    0,
                    checkpointId
                ); // 음수 Checkpoint ID 방지

            int safeOrderStep =
                Mathf.Max(
                    1,
                    routeOrderPerCheckpoint
                ); // Route Order 간격 최소값 보장

            return
                safeCheckpointId *
                safeOrderStep; // Checkpoint 기준 최소 Route Order 계산
        }

        public static int FindFirstRouteIndexAtOrAfterOrder(
            IReadOnlyList<int> routeOrders,
            int minimumRouteOrder
        )
        {
            if (
                routeOrders == null ||
                routeOrders.Count == 0
            )
            {
                return -1; // Route Order 없음 처리
            }

            for (
                int index = 0;
                index < routeOrders.Count;
                index++
            )
            {
                if (
                    routeOrders[index] >=
                    minimumRouteOrder
                )
                {
                    return index; // 최소 Route Order 이상 첫 Index 반환
                }
            }

            return -1; // 허용 가능한 Route 없음 처리
        }

        public static bool ShouldRecoverFromStuck(
            Vector3 progressAnchorPosition,
            Vector3 currentPosition,
            float minimumProgressDistance,
            float stalledSeconds,
            float stuckTimeoutSeconds
        )
        {
            float safeProgressDistance =
                Mathf.Max(
                    0f,
                    minimumProgressDistance
                ); // 최소 이동 거리 음수 방지

            float safeStalledSeconds =
                Mathf.Max(
                    0f,
                    stalledSeconds
                ); // 정체 시간 음수 방지

            float safeTimeoutSeconds =
                Mathf.Max(
                    0f,
                    stuckTimeoutSeconds
                ); // 정체 제한 시간 음수 방지

            if (
                safeStalledSeconds <
                safeTimeoutSeconds
            )
            {
                return false; // 제한 시간 전 복구 차단
            }

            Vector3 progressDelta =
                currentPosition -
                progressAnchorPosition; // 기준 위치 이후 이동량 계산

            progressDelta.y =
                0f; // 수직 점프·낙하 이동을 진행 거리에서 제외

            float progressDistanceSquared =
                progressDelta.sqrMagnitude; // 수평 진행 거리 계산

            return
                progressDistanceSquared <
                safeProgressDistance *
                safeProgressDistance; // 실질 수평 이동 부족 시 Stuck 복구 허용
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
