using System.Collections.Generic; // 진행 목표 목록 사용
using Fusion; // NetworkButtons 사용
using ProjectJ.AI; // 자율 이동 센서 사용
using ProjectJ.Finish; // 결승선 목표 사용
using UnityEngine; // Unity 기본 타입 사용

namespace ProjectJ.Networking.Fusion // Fusion 네트워크 Namespace
{
    [DisallowMultipleComponent] // Controller 중복 방지
    [RequireComponent(typeof(ProjectJNetworkPlayer))] // Network Player 보장
    [RequireComponent(typeof(ProjectJNetworkBotMarker))] // Bot Marker 보장
    public sealed class ProjectJNetworkBotController : MonoBehaviour // State Authority Bot 입력 Controller
    {
        private const float SensorIntervalSeconds = 0.2f; // 지형 재탐색 간격
        private const float MinimumProgressDistance = 0.25f; // 정상 진행 최소 거리
        private const float SoftStuckTimeoutSeconds = 1.5f; // 방향 실패 판정 시간
        private const float HardStuckTimeoutSeconds = 8f; // 최후 부활 판정 시간
        private const float RecoveryRespawnCooldownSeconds = 10f; // 반복 부활 방지 시간
        private const float DefaultColliderRadius = 0.4f; // 기본 몸통 반경

        private readonly List<global::ProjectJ.Checkpoint.Checkpoint> checkpoints = new List<global::ProjectJ.Checkpoint.Checkpoint>(); // 정렬된 체크포인트 목록
        private readonly List<int> checkpointIds = new List<int>(); // 정책용 체크포인트 ID 목록
        private ProjectJNetworkExternalGameplay externalGameplay; // 체크포인트와 부활 상태
        private ProjectJNetworkBotActionController actionController; // Push와 Item 판단 대상
        private ProjectJBotTraversalSensor traversalSensor; // 주변 지형 물리 센서
        private FinishTrigger finishTrigger; // 마지막 결승선 목표
        private CapsuleCollider bodyCollider; // 현재 몸통 크기
        private ProjectJBotTraversalDecision currentDecision; // 유지 중인 이동 판단
        private Vector3 failedDirection; // 최근 정체 방향
        private Vector3 progressAnchorPosition; // 진행 거리 기준 위치
        private Vector3 lastSafePosition; // 최근 안전 바닥 위치
        private float sensorCooldownSeconds; // 다음 센서 탐색 시간
        private float stalledSeconds; // 현재 방향 정체 시간
        private float hardStalledSeconds; // 전체 정체 시간
        private float recoveryRespawnCooldownSeconds; // 부활 재사용 시간
        private float configuredStartDelaySeconds; // Bot별 출발 지연
        private float startDelayRemainingSeconds; // 남은 출발 지연
        private int currentRouteIndex; // 현재 진행 목표 Index
        private int observedRespawnCount; // 마지막 부활 횟수
        private int observedCheckpointId; // 마지막 체크포인트 ID
        private bool initialized; // 내부 초기화 여부
        private bool progressTrackingInitialized; // 진행 측정 여부
        private bool jumpConsumed; // 현재 점프 소비 여부
        private bool startDelayReleased; // 출발 지연 종료 여부

        public int CurrentRouteIndex => currentRouteIndex; // 호환용 현재 목표 Index

        public int RouteCount => checkpoints.Count + (finishTrigger != null ? 1 : 0); // 호환용 전체 진행 목표 수

        public bool HasRoute => RouteCount > 0; // 호환용 진행 목표 존재 여부

        public float StalledSeconds => hardStalledSeconds; // 전체 정체 시간 조회

        public void ConfigureStartDelay( // Bot별 출발 지연 설정
            float delaySeconds // 요청 지연 시간
        )
        {
            configuredStartDelaySeconds = Mathf.Max(0f, delaySeconds); // 음수 없는 지연 저장
            startDelayRemainingSeconds = configuredStartDelaySeconds; // 남은 지연 초기화
            startDelayReleased = false; // 최초 출발 잠금
        }

        public bool TryBuildInput( // 현재 Tick Bot 입력 생성
            ProjectJNetworkPlayer player, // 입력 대상 Bot
            out ProjectJNetworkInput input // 생성 Network 입력
        )
        {
            input = default; // Bot 입력 초기화

            if (player == null || !player.HasLocalStateAuthority) // State Authority 확인
            {
                return false; // 다른 Peer 판단 차단
            }

            EnsureInitialized(player); // 참조와 목표 초기화
            ObserveProgressState(player); // 진행 상태 변경 관찰

            if (ShouldHoldForInitialStartDelay(player)) // 최초 출발 대기 확인
            {
                input.AimDirection = player.transform.forward; // 현재 방향 유지
                return true; // 입력 소유권 유지
            }

            actionController?.TickActions(player, externalGameplay); // Push와 Item 행동 갱신
            float deltaTime = ResolveDeltaTime(player); // Simulation 시간 조회
            UpdateProgressAndRecovery(player, deltaTime); // 정체와 복구 갱신
            UpdateNavigationDecision(player, deltaTime); // 센서 판단 갱신
            ApplyNavigationInput(player, ref input); // 판단을 입력으로 변환
            return true; // Bot 합성 입력 사용
        }

        public void RefreshRoute( // 기존 호출부 호환용 목표 갱신
            ProjectJNetworkPlayer player // 현재 Bot Player
        )
        {
            checkpoints.Clear(); // 이전 체크포인트 제거
            checkpointIds.Clear(); // 이전 ID 제거
            global::ProjectJ.Checkpoint.Checkpoint[] found = Object.FindObjectsByType<global::ProjectJ.Checkpoint.Checkpoint>( // Scene 체크포인트 수집
                FindObjectsInactive.Exclude, // 비활성 대상 제외
                FindObjectsSortMode.None // 직접 ID 정렬 사용
            );
            checkpoints.AddRange(found); // 수집 체크포인트 추가
            checkpoints.Sort(CompareCheckpoints); // ID 기준 정렬

            for (int index = 0; index < checkpoints.Count; index++) // 체크포인트 순회
            {
                checkpointIds.Add((int)checkpoints[index].Id); // 정책용 ID 저장
            }

            finishTrigger = Object.FindFirstObjectByType<FinishTrigger>(); // Scene 결승선 조회
            ResolveCurrentTargetIndex(); // 현재 다음 목표 선택
            ResetNavigationState(player != null ? player.CurrentPosition : transform.position); // 이동 상태 초기화
        }

        private void EnsureInitialized( // 최초 내부 초기화
            ProjectJNetworkPlayer player // 현재 Bot Player
        )
        {
            if (initialized) // 기존 초기화 확인
            {
                return; // 중복 초기화 차단
            }

            externalGameplay = GetComponent<ProjectJNetworkExternalGameplay>(); // 외부 게임 상태 조회
            actionController = GetComponent<ProjectJNetworkBotActionController>(); // 경쟁 행동 조회
            traversalSensor = GetComponent<ProjectJBotTraversalSensor>(); // 기존 센서 조회
            bodyCollider = GetComponent<CapsuleCollider>(); // 몸통 Collider 조회

            if (traversalSensor == null) // 센서 누락 확인
            {
                traversalSensor = gameObject.AddComponent<ProjectJBotTraversalSensor>(); // 런타임 센서 추가
            }

            observedRespawnCount = externalGameplay != null ? externalGameplay.RespawnCount : 0; // 최초 부활 횟수 저장
            observedCheckpointId = externalGameplay != null ? (int)externalGameplay.CurrentCheckpointId : 0; // 최초 체크포인트 저장
            RefreshRoute(player); // 진행 목표 수집
            initialized = true; // 초기화 완료 표시
        }

        private void ObserveProgressState( // 체크포인트와 부활 변경 관찰
            ProjectJNetworkPlayer player // 현재 Bot Player
        )
        {
            if (externalGameplay == null) // 외부 상태 누락 확인
            {
                return; // 관찰 생략
            }

            int respawnCount = externalGameplay.RespawnCount; // 현재 부활 횟수
            int checkpointId = (int)externalGameplay.CurrentCheckpointId; // 현재 체크포인트 ID

            if (respawnCount == observedRespawnCount && checkpointId == observedCheckpointId) // 변화 없음 확인
            {
                return; // 기존 판단 유지
            }

            observedRespawnCount = respawnCount; // 부활 횟수 갱신
            observedCheckpointId = checkpointId; // 체크포인트 갱신
            ResolveCurrentTargetIndex(); // 다음 목표 선택
            ResetNavigationState(player.CurrentPosition); // 새 구간 상태 초기화
        }

        private void ResolveCurrentTargetIndex() // 다음 진행 목표 Index 계산
        {
            int checkpointId = externalGameplay != null ? (int)externalGameplay.CurrentCheckpointId : 0; // 현재 체크포인트 ID
            int nextIndex = ProjectJBotNavigationPolicy.FindNextCheckpointIndex(checkpointId, checkpointIds); // 다음 체크포인트 검색
            currentRouteIndex = nextIndex >= 0 ? nextIndex : (finishTrigger != null ? checkpoints.Count : -1); // 이후 결승선 선택
        }

        private bool TryGetTargetPosition( // 현재 장거리 목표 위치 조회
            out Vector3 targetPosition // 결과 목표 위치
        )
        {
            if (currentRouteIndex >= 0 && currentRouteIndex < checkpoints.Count) // 체크포인트 범위 확인
            {
                global::ProjectJ.Checkpoint.Checkpoint target = checkpoints[currentRouteIndex]; // 현재 체크포인트 조회

                if (target != null) // 체크포인트 유효성 확인
                {
                    targetPosition = target.transform.position; // Trigger 위치 반환
                    return true; // 목표 존재 반환
                }
            }

            if (currentRouteIndex == checkpoints.Count && finishTrigger != null) // 결승선 목표 확인
            {
                targetPosition = finishTrigger.transform.position; // 결승선 위치 반환
                return true; // 목표 존재 반환
            }

            targetPosition = default; // 목표 위치 초기화
            return false; // 목표 없음 반환
        }

        private void UpdateNavigationDecision( // 센서 이동 판단 갱신
            ProjectJNetworkPlayer player, // 현재 Bot Player
            float deltaTime // Simulation 경과 시간
        )
        {
            sensorCooldownSeconds = Mathf.Max(0f, sensorCooldownSeconds - deltaTime); // 탐색 대기시간 감소

            if (!player.IsGrounded) // 공중 상태 확인
            {
                jumpConsumed = false; // 다음 착지 점프 준비
                return; // 공중에서 기존 방향 유지
            }

            if (sensorCooldownSeconds > 0f && currentDecision.IsValid) // 기존 판단 유지 조건 확인
            {
                return; // 잦은 방향 전환 방지
            }

            if (!TryGetTargetPosition(out Vector3 targetPosition)) // 다음 목표 확인
            {
                currentDecision = default; // 이동 판단 제거
                return; // 목표 없는 이동 차단
            }

            Vector3 goalDirection = targetPosition - player.CurrentPosition; // 장거리 목표 방향 계산
            float safetyFloorY = externalGameplay != null ? externalGameplay.RespawnPosition.y : player.CurrentPosition.y; // 안전 높이 계산
            float radius = bodyCollider != null ? bodyCollider.radius : DefaultColliderRadius; // 몸통 반경 조회
            bool selected = traversalSensor.TrySelectTraversal( // 주변 최적 이동 탐색
                player.CurrentPosition, // 현재 발 위치
                goalDirection, // 장거리 목표 방향
                safetyFloorY, // 안전 높이
                player.WalkSpeed, // 기존 걷기 속도
                player.JumpSpeed, // 기존 점프 속도
                player.GravityAcceleration, // 기존 중력
                radius, // 몸통 반경
                player.ColliderHeight, // 몸통 높이
                failedDirection, // 최근 실패 방향
                out currentDecision // 최종 판단 수신
            );

            if (!selected && (lastSafePosition - player.CurrentPosition).sqrMagnitude > MinimumProgressDistance * MinimumProgressDistance) // 안전 위치 후퇴 가능 확인
            {
                traversalSensor.TrySelectTraversal( // 안전 위치 방향 재탐색
                    player.CurrentPosition, // 현재 발 위치
                    lastSafePosition - player.CurrentPosition, // 후퇴 목표 방향
                    safetyFloorY, // 안전 높이
                    player.WalkSpeed, // 기존 걷기 속도
                    player.JumpSpeed, // 기존 점프 속도
                    player.GravityAcceleration, // 기존 중력
                    radius, // 몸통 반경
                    player.ColliderHeight, // 몸통 높이
                    failedDirection, // 실패 방향
                    out currentDecision // 후퇴 판단 수신
                );
            }

            sensorCooldownSeconds = SensorIntervalSeconds; // 다음 탐색 간격 설정
        }

        private void ApplyNavigationInput( // 판단을 Fusion 입력으로 변환
            ProjectJNetworkPlayer player, // 현재 Bot Player
            ref ProjectJNetworkInput input // 수정할 Network 입력
        )
        {
            if (!currentDecision.IsValid) // 유효 판단 확인
            {
                input.Move = Vector2.zero; // 이동 입력 제거
                input.AimDirection = player.transform.forward; // 현재 방향 유지
                return; // 입력 변환 종료
            }

            input.Move = Vector2.up; // 전진 입력 설정
            input.AimDirection = currentDecision.Direction; // 센서 방향 설정
            bool pulseJump = currentDecision.Action == ProjectJBotTraversalAction.Jump && player.IsGrounded && !jumpConsumed; // 점프 1회 입력 판정
            input.Buttons.Set(ProjectJNetworkButton.Jump, pulseJump); // 점프 버튼 설정

            if (pulseJump) // 점프 입력 확인
            {
                jumpConsumed = true; // 점프 소비 표시
            }
        }

        private void UpdateProgressAndRecovery( // 진행과 정체 복구 갱신
            ProjectJNetworkPlayer player, // 현재 Bot Player
            float deltaTime // Simulation 경과 시간
        )
        {
            recoveryRespawnCooldownSeconds = Mathf.Max(0f, recoveryRespawnCooldownSeconds - deltaTime); // 부활 대기시간 감소

            if (externalGameplay != null && !externalGameplay.GameplayInputAllowed) // 경기 입력 잠금 확인
            {
                ResetProgressTracking(player.CurrentPosition); // 정체 누적 제거
                return; // 복구 생략
            }

            if (!progressTrackingInitialized) // 진행 측정 미초기화 확인
            {
                ResetProgressTracking(player.CurrentPosition); // 기준 위치 설정
                return; // 최초 측정 종료
            }

            Vector3 delta = player.CurrentPosition - progressAnchorPosition; // 기준 이후 이동량 계산
            delta.y = 0f; // 수직 이동 제외

            if (delta.sqrMagnitude >= MinimumProgressDistance * MinimumProgressDistance) // 충분한 진행 확인
            {
                ResetProgressTracking(player.CurrentPosition); // 정체 측정 초기화

                if (player.IsGrounded && IsAtOrAboveSafetyFloor(player.CurrentPosition.y)) // 안전한 바닥 위치 확인
                {
                    lastSafePosition = player.CurrentPosition; // 최근 안전 위치 저장
                }

                hardStalledSeconds = 0f; // 전체 정체 시간 초기화
                return; // 정상 진행 종료
            }

            stalledSeconds += deltaTime; // 방향 정체 시간 누적
            hardStalledSeconds += deltaTime; // 전체 정체 시간 누적

            if (stalledSeconds >= SoftStuckTimeoutSeconds) // 방향 실패 시간 확인
            {
                failedDirection = currentDecision.IsValid ? currentDecision.Direction : failedDirection; // 실패 방향 기억
                currentDecision = default; // 기존 판단 폐기
                sensorCooldownSeconds = 0f; // 즉시 재탐색 허용
                stalledSeconds = 0f; // 방향 정체 초기화
                progressAnchorPosition = player.CurrentPosition; // 측정 기준 갱신
            }

            if (hardStalledSeconds < HardStuckTimeoutSeconds || recoveryRespawnCooldownSeconds > 0f) // 최후 복구 조건 확인
            {
                return; // 부활 복구 대기
            }

            externalGameplay?.RequestBotRecoveryRespawn(); // State Authority 체크포인트 부활 요청
            recoveryRespawnCooldownSeconds = RecoveryRespawnCooldownSeconds; // 반복 부활 방지 설정
            hardStalledSeconds = 0f; // 전체 정체 초기화
        }

        private bool IsAtOrAboveSafetyFloor( // 체크포인트 높이 이상 판정
            float positionY // 현재 발 높이
        )
        {
            float safetyY = externalGameplay != null ? externalGameplay.RespawnPosition.y : positionY; // 안전 높이 조회
            return positionY >= safetyY - 0.05f; // 작은 오차 포함 판정
        }

        private bool ShouldHoldForInitialStartDelay( // 최초 출발 대기 판정
            ProjectJNetworkPlayer player // 현재 Bot Player
        )
        {
            if (startDelayReleased) // 지연 종료 확인
            {
                return false; // 추가 대기 없음
            }

            if (externalGameplay != null && !externalGameplay.GameplayInputAllowed) // 경기 시작 전 확인
            {
                startDelayRemainingSeconds = configuredStartDelaySeconds; // 설정 지연 유지
                return true; // 이동 차단
            }

            startDelayRemainingSeconds = Mathf.Max(0f, startDelayRemainingSeconds - ResolveDeltaTime(player)); // 남은 지연 감소

            if (startDelayRemainingSeconds > 0f) // 남은 지연 확인
            {
                return true; // 대기 유지
            }

            startDelayReleased = true; // 출발 지연 종료
            return false; // 자율 이동 허용
        }

        private static float ResolveDeltaTime( // Simulation 시간 조회
            ProjectJNetworkPlayer player // 현재 Bot Player
        )
        {
            return player != null && player.Runner != null ? Mathf.Max(0f, player.Runner.DeltaTime) : 0f; // Fusion DeltaTime 반환
        }

        private void ResetNavigationState( // 목표 구간 상태 초기화
            Vector3 currentPosition // 현재 Bot 위치
        )
        {
            currentDecision = default; // 기존 판단 제거
            failedDirection = Vector3.zero; // 실패 방향 제거
            jumpConsumed = false; // 점프 상태 초기화
            sensorCooldownSeconds = 0f; // 즉시 탐색 허용
            lastSafePosition = currentPosition; // 최초 안전 위치 저장
            hardStalledSeconds = 0f; // 전체 정체 초기화
            ResetProgressTracking(currentPosition); // 진행 측정 초기화
        }

        private void ResetProgressTracking( // 진행 측정 초기화
            Vector3 currentPosition // 새 기준 위치
        )
        {
            progressAnchorPosition = currentPosition; // 진행 기준 저장
            stalledSeconds = 0f; // 방향 정체 초기화
            progressTrackingInitialized = true; // 진행 측정 활성화
        }

        private static int CompareCheckpoints( // 체크포인트 정렬 비교
            global::ProjectJ.Checkpoint.Checkpoint left, // 왼쪽 체크포인트
            global::ProjectJ.Checkpoint.Checkpoint right // 오른쪽 체크포인트
        )
        {
            if (left == null) // 왼쪽 누락 확인
            {
                return right == null ? 0 : 1; // 빈 대상 뒤로 정렬
            }

            if (right == null) // 오른쪽 누락 확인
            {
                return -1; // 유효 대상 앞으로 정렬
            }

            int comparison = ((int)left.Id).CompareTo((int)right.Id); // 체크포인트 ID 비교
            return comparison != 0 ? comparison : left.GetInstanceID().CompareTo(right.GetInstanceID()); // Instance ID 보조 정렬
        }
    }
}
