using System.Collections.Generic; // Route 목록 사용
using Fusion; // NetworkButtons 사용
using ProjectJ.AI; // Bot Route 정책 사용
using UnityEngine; // MonoBehaviour와 Vector 타입 사용

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ProjectJNetworkPlayer))]
    [RequireComponent(typeof(ProjectJNetworkBotMarker))]
    public sealed class ProjectJNetworkBotController :
        MonoBehaviour
    {
        private const float MinimumProgressDistance =
            0.25f; // Stuck 해제에 필요한 최소 이동 거리

        private const float StuckTimeoutSeconds =
            2.5f; // Route 정체 복구 제한 시간

        private readonly List<ProjectJBotRouteNode> routeNodes =
            new List<ProjectJBotRouteNode>(); // 정렬된 Route Node 목록

        private readonly List<Vector3> routePositions =
            new List<Vector3>(); // Respawn 최근접 Route 위치 목록

        private readonly List<int> routeOrders =
            new List<int>(); // Checkpoint 최소 Route 계산용 순서 목록

        private ProjectJNetworkExternalGameplay externalGameplay; // Respawn 상태 조회 대상
        private bool initialized; // Bot 내부 초기화 여부
        private int currentRouteIndex; // 현재 목표 Route Index
        private int observedRespawnCount; // 마지막 확인 Respawn 횟수
        private bool jumpConsumedForCurrentNode; // 현재 Node 점프 입력 소비 여부
        private Vector3 progressAnchorPosition; // Stuck 이동량 측정 기준 위치
        private float stalledSeconds; // 누적 정체 시간
        private bool progressTrackingInitialized; // Stuck 위치 측정 초기화 여부

        public int CurrentRouteIndex =>
            currentRouteIndex; // 현재 Route Index 조회

        public int RouteCount =>
            routeNodes.Count; // 전체 Route 개수 조회

        public bool HasRoute =>
            routeNodes.Count > 0; // Route 존재 여부 조회

        public float StalledSeconds =>
            stalledSeconds; // 현재 정체 누적 시간 조회

        public bool TryBuildInput(
            ProjectJNetworkPlayer player,
            out ProjectJNetworkInput input
        )
        {
            input =
                default; // Bot 입력 초기화

            if (
                player == null ||
                !player.HasLocalStateAuthority
            )
            {
                return false; // State Authority 외 Bot 판단 차단
            }

            EnsureInitialized(
                player
            ); // Bot Route와 Respawn 참조 초기화

            ObserveRespawn(
                player
            ); // Respawn 후 Route 재선정

            AdvanceReachedNodes(
                player.CurrentPosition
            ); // 이미 도달한 Node 진행

            ObserveStuck(
                player
            ); // 정체 상태 확인 및 Route 복구

            if (
                currentRouteIndex < 0 ||
                currentRouteIndex >= routeNodes.Count
            )
            {
                input.AimDirection =
                    player.transform.forward; // Route 종료 시 현재 방향 유지

                return true; // Bot 입력 소유권 유지
            }

            ProjectJBotRouteNode targetNode =
                routeNodes[currentRouteIndex]; // 현재 목표 Node 조회

            Vector3 currentPosition =
                player.CurrentPosition; // 현재 Bot 위치 조회

            Vector3 targetPosition =
                targetNode.transform.position; // 현재 목표 위치 조회

            Vector3 moveDirection =
                ProjectJBotNavigationPolicy.ResolvePlanarDirection(
                    currentPosition,
                    targetPosition
                ); // Route 기준 수평 이동 방향 계산

            if (
                moveDirection.sqrMagnitude >
                0.0001f
            )
            {
                input.Move =
                    Vector2.up; // 카메라 기준 전진 입력 사용

                input.AimDirection =
                    moveDirection; // Route 방향을 가상 카메라 전방으로 사용
            }
            else
            {
                input.Move =
                    Vector2.zero; // 수평 방향 없음 처리

                input.AimDirection =
                    player.transform.forward; // 기존 몸 방향 유지
            }

            float planarDistance =
                Vector2.Distance(
                    new Vector2(
                        currentPosition.x,
                        currentPosition.z
                    ),
                    new Vector2(
                        targetPosition.x,
                        targetPosition.z
                    )
                ); // 수평 점프 접근 거리 계산

            bool pulseJump =
                ProjectJBotNavigationPolicy.ShouldPulseJump(
                    targetNode.RequiresJump,
                    player.IsGrounded,
                    planarDistance,
                    targetNode.JumpTriggerDistance,
                    jumpConsumedForCurrentNode
                ); // Route Node 점프 입력 판정

            input.Buttons.Set(
                ProjectJNetworkButton.Jump,
                pulseJump
            ); // Fusion 점프 버튼 삽입

            if (pulseJump)
            {
                jumpConsumedForCurrentNode =
                    true; // 현재 Node 점프 1회 소비
            }

            return true; // Bot 합성 입력 사용
        }

        public void RefreshRoute(
            ProjectJNetworkPlayer player
        )
        {
            routeNodes.Clear(); // 이전 Route Node 제거
            routePositions.Clear(); // 이전 Route 위치 제거
            routeOrders.Clear(); // 이전 Route Order 제거

            ProjectJBotRouteNode[] foundNodes =
                Object.FindObjectsByType<ProjectJBotRouteNode>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                ); // 현재 로드 Scene Route Node 수집

            routeNodes.AddRange(
                foundNodes
            ); // Route 후보 목록 추가

            routeNodes.Sort(
                CompareRouteNodes
            ); // RouteOrder 기준 정렬

            for (
                int index = 0;
                index < routeNodes.Count;
                index++
            )
            {
                routePositions.Add(
                    routeNodes[index].transform.position
                ); // Respawn 검색용 위치 저장

                routeOrders.Add(
                    routeNodes[index].RouteOrder
                ); // Checkpoint 복구용 Route Order 저장
            }

            int minimumRouteIndex =
                ResolveCheckpointMinimumRouteIndex(); // 현재 Checkpoint 최소 Route Index 계산

            currentRouteIndex =
                ProjectJBotNavigationPolicy.FindNearestRouteIndex(
                    player != null
                        ? player.CurrentPosition
                        : transform.position,
                    routePositions,
                    minimumRouteIndex
                ); // 허용 Route 범위에서 최근접 Route 선택

            jumpConsumedForCurrentNode =
                false; // 새 Route 점프 상태 초기화

            ResetProgressTracking(
                player != null
                    ? player.CurrentPosition
                    : transform.position
            ); // Route 갱신 후 Stuck 측정 초기화
        }

        private void EnsureInitialized(
            ProjectJNetworkPlayer player
        )
        {
            if (initialized)
            {
                return; // 중복 초기화 차단
            }

            externalGameplay =
                GetComponent<ProjectJNetworkExternalGameplay>(); // Respawn 상태 컴포넌트 조회

            observedRespawnCount =
                externalGameplay != null
                    ? externalGameplay.RespawnCount
                    : 0; // 최초 Respawn 횟수 저장

            RefreshRoute(
                player
            ); // 최초 Route 수집

            initialized =
                true; // 초기화 완료 표시
        }

        private void ObserveRespawn(
            ProjectJNetworkPlayer player
        )
        {
            if (externalGameplay == null)
            {
                return; // Respawn 컴포넌트 없음 처리
            }

            int currentRespawnCount =
                externalGameplay.RespawnCount; // 현재 Respawn 횟수 조회

            if (
                currentRespawnCount ==
                observedRespawnCount
            )
            {
                return; // Respawn 변화 없음 처리
            }

            observedRespawnCount =
                currentRespawnCount; // Respawn 횟수 갱신

            int minimumRouteIndex =
                ResolveCheckpointMinimumRouteIndex(); // 부활 Checkpoint 최소 Route Index 계산

            currentRouteIndex =
                ProjectJBotNavigationPolicy.FindNearestRouteIndex(
                    player.CurrentPosition,
                    routePositions,
                    minimumRouteIndex
                ); // 이전 Checkpoint Route를 제외하고 최근접 Route 재선정

            jumpConsumedForCurrentNode =
                false; // 부활 후 점프 상태 초기화

            ResetProgressTracking(
                player.CurrentPosition
            ); // 부활 후 Stuck 측정 초기화
        }

        private void ObserveStuck(
            ProjectJNetworkPlayer player
        )
        {
            if (
                currentRouteIndex < 0 ||
                currentRouteIndex >= routeNodes.Count
            )
            {
                ResetProgressTracking(
                    player.CurrentPosition
                ); // Route 종료 상태 측정 초기화

                return;
            }

            if (
                externalGameplay != null &&
                !externalGameplay.GameplayInputAllowed
            )
            {
                ResetProgressTracking(
                    player.CurrentPosition
                ); // 경기 잠금 중 Stuck 누적 방지

                return;
            }

            if (!progressTrackingInitialized)
            {
                ResetProgressTracking(
                    player.CurrentPosition
                ); // 최초 Stuck 기준 위치 설정

                return;
            }

            Vector3 currentPosition =
                player.CurrentPosition; // 현재 Bot 위치 조회

            Vector3 progressDelta =
                currentPosition -
                progressAnchorPosition; // 기준 위치 이후 이동량 계산

            progressDelta.y =
                0f; // 수직 점프·낙하 이동을 진행 거리에서 제외

            float progressDistanceSquared =
                progressDelta.sqrMagnitude; // 수평 진행 거리 계산

            if (
                progressDistanceSquared >=
                MinimumProgressDistance *
                MinimumProgressDistance
            )
            {
                ResetProgressTracking(
                    currentPosition
                ); // 충분한 이동 시 정체 시간 초기화

                return;
            }

            float deltaTime =
                player.Runner != null
                    ? player.Runner.DeltaTime
                    : 0f; // Fusion Simulation 시간 조회

            stalledSeconds +=
                Mathf.Max(
                    0f,
                    deltaTime
                ); // 정체 시간 누적

            if (
                !ProjectJBotNavigationPolicy.ShouldRecoverFromStuck(
                    progressAnchorPosition,
                    currentPosition,
                    MinimumProgressDistance,
                    stalledSeconds,
                    StuckTimeoutSeconds
                )
            )
            {
                return; // 아직 Stuck 복구 조건 미충족
            }

            RecoverFromStuck(
                currentPosition
            ); // 다음 Route 범위에서 정체 복구
        }

        private void RecoverFromStuck(
            Vector3 currentPosition
        )
        {
            int checkpointMinimumIndex =
                ResolveCheckpointMinimumRouteIndex(); // 현재 Checkpoint 최소 Route Index 계산

            int nextRouteIndex =
                Mathf.Max(
                    checkpointMinimumIndex,
                    currentRouteIndex + 1
                ); // 현재 막힌 Node와 이전 구간 제외

            if (
                nextRouteIndex <
                routePositions.Count
            )
            {
                currentRouteIndex =
                    ProjectJBotNavigationPolicy.FindNearestRouteIndex(
                        currentPosition,
                        routePositions,
                        nextRouteIndex
                    ); // 다음 Route 범위에서 최근접 Node 선택

                jumpConsumedForCurrentNode =
                    false; // 복구 Route 점프 상태 초기화
            }

            ResetProgressTracking(
                currentPosition
            ); // Stuck 복구 후 측정 기준 초기화
        }

        private int ResolveCheckpointMinimumRouteIndex()
        {
            if (routeOrders.Count == 0)
            {
                return 0; // Route 없음 처리
            }

            int checkpointId =
                externalGameplay != null
                    ? (int)externalGameplay.CurrentCheckpointId
                    : 0; // 현재 최고 Checkpoint ID 조회

            int minimumRouteOrder =
                ProjectJBotNavigationPolicy.ResolveCheckpointMinimumRouteOrder(
                    checkpointId
                ); // Checkpoint별 최소 Route Order 계산

            int minimumRouteIndex =
                ProjectJBotNavigationPolicy.FindFirstRouteIndexAtOrAfterOrder(
                    routeOrders,
                    minimumRouteOrder
                ); // 최소 Route Order 이상 첫 Index 검색

            if (minimumRouteIndex >= 0)
            {
                return minimumRouteIndex; // 허용 Route 시작 Index 반환
            }

            return
                routeOrders.Count - 1; // Route 부족 시 마지막 Node로 제한
        }

        private void ResetProgressTracking(
            Vector3 currentPosition
        )
        {
            progressAnchorPosition =
                currentPosition; // Stuck 기준 위치 갱신

            stalledSeconds =
                0f; // 정체 시간 초기화

            progressTrackingInitialized =
                true; // Stuck 측정 활성화
        }

        private void AdvanceReachedNodes(
            Vector3 currentPosition
        )
        {
            while (
                currentRouteIndex >= 0 &&
                currentRouteIndex < routeNodes.Count
            )
            {
                ProjectJBotRouteNode targetNode =
                    routeNodes[currentRouteIndex]; // 현재 목표 Node 조회

                if (
                    !ProjectJBotNavigationPolicy.HasReached(
                        currentPosition,
                        targetNode.transform.position,
                        targetNode.ArrivalRadius
                    )
                )
                {
                    return; // 아직 목표 Node 미도달
                }

                currentRouteIndex++; // 다음 Route Node 진행

                jumpConsumedForCurrentNode =
                    false; // 다음 Node 점프 상태 초기화

                ResetProgressTracking(
                    currentPosition
                ); // Node 진행 후 Stuck 측정 초기화
            }
        }

        private static int CompareRouteNodes(
            ProjectJBotRouteNode left,
            ProjectJBotRouteNode right
        )
        {
            if (left == null)
            {
                return right == null
                    ? 0
                    : 1; // null Node 뒤로 정렬
            }

            if (right == null)
            {
                return -1; // 유효 Node 앞으로 정렬
            }

            int orderComparison =
                left.RouteOrder.CompareTo(
                    right.RouteOrder
                ); // RouteOrder 우선 비교

            if (orderComparison != 0)
            {
                return orderComparison; // RouteOrder 결과 반환
            }

            return
                left.GetInstanceID().CompareTo(
                    right.GetInstanceID()
                ); // 동일 순서 Instance ID 보조 정렬
        }
    }
}
