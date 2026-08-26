using System.Collections.Generic; // Route 위치와 순서 목록 사용
using UnityEngine; // Vector3와 Mathf 사용

namespace ProjectJ.AI
{
    public enum ProjectJBotTraversalAction // Bot 이동 행동 종류
    {
        None = 0, // 이동 불가 상태
        Walk = 1, // 걷기 이동 상태
        Jump = 2 // 점프 이동 상태
    }

    public readonly struct ProjectJBotTraversalCandidate // 센서 이동 후보 자료
    {
        public ProjectJBotTraversalCandidate( // 이동 후보 생성
            Vector3 direction, // 수평 이동 방향
            Vector3 landingPosition, // 예상 착지 위치
            float heightDelta, // 현재 위치 대비 높이 차이
            bool hasGround, // 착지 바닥 존재 여부
            bool pathClear, // 이동 경로 확보 여부
            bool hasHeadroom, // 착지 머리 공간 여부
            bool crossesGap // 틈 통과 여부
        )
        {
            Direction = direction.sqrMagnitude > 0.0001f // 유효 방향 크기 확인
                ? direction.normalized // 정규화 방향 저장
                : Vector3.zero; // 빈 방향 저장
            LandingPosition = landingPosition; // 착지 위치 저장
            HeightDelta = heightDelta; // 높이 차이 저장
            HasGround = hasGround; // 바닥 존재 상태 저장
            PathClear = pathClear; // 경로 확보 상태 저장
            HasHeadroom = hasHeadroom; // 머리 공간 상태 저장
            CrossesGap = crossesGap; // 틈 통과 상태 저장
        }

        public Vector3 Direction { get; } // 수평 이동 방향 조회

        public Vector3 LandingPosition { get; } // 예상 착지 위치 조회

        public float HeightDelta { get; } // 착지 높이 차이 조회

        public bool HasGround { get; } // 착지 바닥 존재 여부 조회

        public bool PathClear { get; } // 이동 경로 확보 여부 조회

        public bool HasHeadroom { get; } // 착지 머리 공간 여부 조회

        public bool CrossesGap { get; } // 틈 통과 여부 조회
    }

    public readonly struct ProjectJBotTraversalDecision // 최종 이동 판단 자료
    {
        public ProjectJBotTraversalDecision( // 이동 판단 생성
            bool isValid, // 유효 판단 여부
            Vector3 direction, // 선택 이동 방향
            Vector3 landingPosition, // 선택 착지 위치
            ProjectJBotTraversalAction action, // 선택 이동 행동
            float score // 선택 후보 점수
        )
        {
            IsValid = isValid; // 유효 상태 저장
            Direction = direction; // 이동 방향 저장
            LandingPosition = landingPosition; // 착지 위치 저장
            Action = action; // 이동 행동 저장
            Score = score; // 후보 점수 저장
        }

        public bool IsValid { get; } // 유효 판단 여부 조회

        public Vector3 Direction { get; } // 선택 이동 방향 조회

        public Vector3 LandingPosition { get; } // 선택 착지 위치 조회

        public ProjectJBotTraversalAction Action { get; } // 선택 이동 행동 조회

        public float Score { get; } // 선택 후보 점수 조회
    }

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

        public static ProjectJBotTraversalDecision SelectBestCandidate( // 최적 자율 이동 후보 선택
            IReadOnlyList<ProjectJBotTraversalCandidate> candidates, // 센서 이동 후보 목록
            Vector3 goalDirection, // 장거리 목표 방향
            float minimumSafeY, // 체크포인트 안전 높이
            float maximumStepHeight, // 걷기 가능한 최대 단차
            float maximumJumpHeight, // 점프 가능한 최대 높이
            float maximumSafeDrop, // 허용 가능한 최대 하강
            Vector3 failedDirection // 최근 정체 방향
        )
        {
            if (candidates == null || candidates.Count == 0) // 후보 목록 존재 확인
            {
                return default; // 이동 불가 판단 반환
            }

            Vector3 safeGoalDirection = FlattenDirection(goalDirection); // 목표 수평 방향 정규화
            Vector3 safeFailedDirection = FlattenDirection(failedDirection); // 실패 수평 방향 정규화
            float safeStepHeight = Mathf.Max(0f, maximumStepHeight); // 걷기 단차 한계 보정
            float safeJumpHeight = Mathf.Max(safeStepHeight, maximumJumpHeight); // 점프 높이 한계 보정
            float safeDrop = Mathf.Max(0f, maximumSafeDrop); // 하강 한계 보정
            ProjectJBotTraversalDecision bestDecision = default; // 최적 판단 초기화
            float bestScore = float.NegativeInfinity; // 최적 점수 초기화

            for (int index = 0; index < candidates.Count; index++) // 모든 이동 후보 순회
            {
                ProjectJBotTraversalCandidate candidate = candidates[index]; // 현재 이동 후보 조회

                if (!IsCandidateSafe(candidate, minimumSafeY, safeJumpHeight, safeDrop)) // 후보 안전 조건 확인
                {
                    continue; // 위험 후보 제외
                }

                ProjectJBotTraversalAction action = candidate.CrossesGap || candidate.HeightDelta > safeStepHeight // 점프 필요 조건 확인
                    ? ProjectJBotTraversalAction.Jump // 점프 행동 선택
                    : ProjectJBotTraversalAction.Walk; // 걷기 행동 선택
                float goalAlignment = safeGoalDirection == Vector3.zero // 목표 방향 존재 확인
                    ? 0f // 목표 방향 점수 없음
                    : Vector3.Dot(candidate.Direction, safeGoalDirection); // 목표 방향 정렬 점수 계산
                float failedAlignment = safeFailedDirection == Vector3.zero // 실패 방향 존재 확인
                    ? 0f // 실패 방향 감점 없음
                    : Mathf.Max(0f, Vector3.Dot(candidate.Direction, safeFailedDirection)); // 실패 방향 유사도 계산
                float score = goalAlignment * 4f // 목표 방향 우선 점수
                    + Mathf.Clamp(candidate.HeightDelta, -safeDrop, safeJumpHeight) * 1.5f // 상승 진행 점수
                    - failedAlignment * 10f // 최근 실패 방향 감점
                    - (candidate.CrossesGap ? 0.25f : 0f); // 불필요한 틈 점프 감점

                if (score <= bestScore) // 기존 최적 점수 이하 확인
                {
                    continue; // 낮은 점수 후보 제외
                }

                bestScore = score; // 최적 점수 갱신
                bestDecision = new ProjectJBotTraversalDecision( // 최적 이동 판단 생성
                    true, // 유효 판단 설정
                    candidate.Direction, // 선택 방향 전달
                    candidate.LandingPosition, // 착지 위치 전달
                    action, // 이동 행동 전달
                    score // 후보 점수 전달
                );
            }

            return bestDecision; // 최적 이동 판단 반환
        }

        public static bool CanReachLanding( // 기존 이동 능력으로 착지 가능 판정
            float horizontalDistance, // 수평 착지 거리
            float heightDelta, // 착지 높이 차이
            float moveSpeed, // 기존 수평 이동 속도
            float jumpSpeed, // 기존 점프 속도
            float gravity, // 기존 중력 가속도
            float safetyMargin // 도달 거리 안전 배율
        )
        {
            float safeDistance = Mathf.Max(0f, horizontalDistance); // 음수 수평 거리 방지
            float safeMoveSpeed = Mathf.Max(0f, moveSpeed); // 음수 이동 속도 방지
            float safeJumpSpeed = Mathf.Max(0f, jumpSpeed); // 음수 점프 속도 방지
            float safeGravity = Mathf.Min(-0.0001f, gravity); // 하향 중력 보장
            float safeMargin = Mathf.Clamp01(safetyMargin); // 안전 배율 범위 보정
            float discriminant = safeJumpSpeed * safeJumpSpeed + 2f * safeGravity * heightDelta; // 착지 시간 판별식 계산

            if (discriminant < 0f) // 점프 최고점 초과 확인
            {
                return false; // 도달 불가 반환
            }

            float landingTime = (-safeJumpSpeed - Mathf.Sqrt(discriminant)) / safeGravity; // 하강 구간 착지 시간 계산

            if (landingTime <= 0f) // 유효 비행 시간 확인
            {
                return false; // 도달 불가 반환
            }

            float reachableDistance = safeMoveSpeed * landingTime * safeMargin; // 안전 여유 포함 도달 거리 계산

            return safeDistance <= reachableDistance; // 수평 착지 가능 여부 반환
        }

        public static int FindNextCheckpointIndex( // 현재 진행 이후 체크포인트 검색
            int currentCheckpointId, // 현재 활성 체크포인트 ID
            IReadOnlyList<int> checkpointIds // 정렬된 체크포인트 ID 목록
        )
        {
            if (checkpointIds == null) // 체크포인트 목록 누락 확인
            {
                return -1; // 다음 목표 없음 반환
            }

            for (int index = 0; index < checkpointIds.Count; index++) // 정렬된 체크포인트 순회
            {
                if (checkpointIds[index] > currentCheckpointId) // 현재 진행 이후 ID 확인
                {
                    return index; // 다음 체크포인트 Index 반환
                }
            }

            return -1; // 모든 체크포인트 통과 상태 반환
        }

        private static bool IsCandidateSafe( // 이동 후보 안전 조건 판정
            ProjectJBotTraversalCandidate candidate, // 검사 이동 후보
            float minimumSafeY, // 체크포인트 안전 높이
            float maximumJumpHeight, // 점프 높이 한계
            float maximumSafeDrop // 하강 높이 한계
        )
        {
            if (candidate.Direction == Vector3.zero) // 이동 방향 없음 확인
            {
                return false; // 빈 방향 후보 차단
            }

            if (!candidate.HasGround || !candidate.PathClear || !candidate.HasHeadroom) // 지형 안전 조건 확인
            {
                return false; // 불완전 후보 차단
            }

            float currentFootY = candidate.LandingPosition.y - candidate.HeightDelta; // 후보 관측 시점 발 높이 복원
            float effectiveMinimumSafeY = Mathf.Min(minimumSafeY, currentFootY); // 착지 후 현재 바닥을 안전 기준에 포함

            if (candidate.LandingPosition.y < effectiveMinimumSafeY - 0.05f) // 현재 바닥 기준 아래 착지 확인
            {
                return false; // 진행 높이 아래 후보 차단
            }

            if (candidate.HeightDelta > maximumJumpHeight) // 점프 높이 초과 확인
            {
                return false; // 높은 후보 차단
            }

            return candidate.HeightDelta >= -maximumSafeDrop; // 과도한 하강 후보 차단
        }

        private static Vector3 FlattenDirection( // 수평 방향 정규화
            Vector3 direction // 원본 3D 방향
        )
        {
            direction.y = 0f; // 수직 성분 제거

            return direction.sqrMagnitude > DirectionEpsilonSquared // 유효 수평 크기 확인
                ? direction.normalized // 정규화 방향 반환
                : Vector3.zero; // 빈 방향 반환
        }
    }
}
