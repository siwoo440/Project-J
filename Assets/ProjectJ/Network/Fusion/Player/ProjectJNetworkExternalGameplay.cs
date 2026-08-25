using System.Collections.Generic; // 활성 Player Registry 사용
using Fusion; // NetworkBehaviour와 TickTimer 사용
using ProjectJ.Checkpoint; // 체크포인트와 낙하 한계 사용
using ProjectJ.Debugging; // 네트워크 디버그 단축키 정책 사용
using ProjectJ.Finish; // FINISH 공통 수신 계약 사용
using ProjectJ.Items; // 눈덩이 적중 정책 사용
using UnityEngine; // Unity 기본 타입 사용
using UnityEngine.InputSystem; // 개발 테스트 키 사용
using UnityEngine.SceneManagement; // Lobby / Game Scene 상태 확인

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent] // 동일 컴포넌트 중복 방지
    [RequireComponent(typeof(ProjectJNetworkPlayer))] // Network Player 보장
    [RequireComponent(typeof(NetworkTransform))] // 네트워크 순간이동 보장
    public sealed partial class ProjectJNetworkExternalGameplay :
        NetworkBehaviour,
        ICheckpointReceiver,
        IFinishReceiver
    {
        private const float ExternalVelocityDecayPerSecond = 12f; // 외력 초당 감속량
        private const float ExternalVelocityStopThreshold = 0.05f; // 외력 정지 임계값
        private const float PushSearchRange = 2.5f; // 밀치기 최대 거리
        private const float PushSearchHalfAngle = 45f; // 90도 전방 범위 절반 각도
        private const float PushForce = 12f; // 기본 밀치기 힘
        private const float PushCooldownSeconds = 1.5f; // 밀치기 재사용 대기시간
        private const float RespawnProtectionSeconds = 3f; // 부활 보호 시간
        private const float CountdownSeconds = 3f; // 경기 시작 카운트다운
        private const float MatchDurationSeconds = 600f; // 최신 기획 기준 10분 경기
        private const int LobbyMatchMinimumPlayerCount = 2; // 74일차 Ready 경기 시작 최소 인원
        private const string LobbySceneName = "Lobby"; // Ready를 입력하는 Lobby Scene
        private const string GameSceneName = "Game"; // 실제 경기 Scene
        private const string Day49AllSystemsTestSceneName =
            "Day49_AllSystemsTest"; // Phase 6 멀티플레이 통합 테스트 Scene

        private static readonly HashSet<ProjectJNetworkExternalGameplay> ActivePlayers =
            new HashSet<ProjectJNetworkExternalGameplay>(); // 현재 프로세스 Player Registry

        internal static void CollectActivePlayers( // 현재 Runner Player 목록 복사
            NetworkRunner runner, // 조회할 Runner
            List<ProjectJNetworkExternalGameplay> results // 재사용 결과 목록
        )
        {
            results.Clear(); // 이전 Tick 후보 제거

            if (runner == null) // Runner 누락 확인
            {
                return; // 조회 차단
            }

            foreach (ProjectJNetworkExternalGameplay candidate in ActivePlayers) // 활성 Player Registry 순회
            {
                if (
                    candidate == null || // Player 누락 조건
                    candidate.Runner != runner || // 다른 Runner Player 제외
                    candidate.Object == null || // NetworkObject 누락 조건
                    !candidate.Object.IsValid // NetworkObject 무효 조건
                )
                {
                    continue; // 결과 목록 제외
                }

                results.Add(candidate); // 현재 Runner Player 추가
            }
        }

        private ProjectJNetworkPlayer networkPlayer; // 이동 상태 초기화 대상
        private ProjectJNetworkItemInventory itemInventory; // 73일차 젤리 보호막 상태 조회
        private NetworkTransform networkTransform; // 순간이동 동기화 대상
        private CheckpointFallLimitSet fallLimitSet; // 체크포인트별 낙하 한계

        [Networked] // 외부 속도 동기화
        private Vector3 NetworkExternalVelocity
        {
            get;
            set;
        }

        [Networked] // 마지막 외력 원인 동기화
        private int NetworkLastExternalForceSource
        {
            get;
            set;
        }

        [Networked] // 외력 적용 횟수 동기화
        private int NetworkExternalForceApplyCount
        {
            get;
            set;
        }

        [Networked] // 밀치기 기준 방향 동기화
        private Vector3 NetworkPushForward
        {
            get;
            set;
        }

        [Networked] // 밀치기 쿨타임 동기화
        private TickTimer NetworkPushCooldown
        {
            get;
            set;
        }

        [Networked] // 마지막 밀치기 결과 동기화
        private int NetworkLastPushResult
        {
            get;
            set;
        }

        [Networked] // 마지막 밀치기 대상 동기화
        private int NetworkLastPushTargetIndex
        {
            get;
            set;
        }

        [Networked] // 밀치기 시도 횟수 동기화
        private int NetworkPushAttemptCount
        {
            get;
            set;
        }

        [Networked] // 밀치기 성공 횟수 동기화
        private int NetworkPushSuccessCount
        {
            get;
            set;
        }

        [Networked] // 최고 체크포인트 ID 동기화
        private int NetworkCheckpointId
        {
            get;
            set;
        }

        [Networked] // 부활 위치 동기화
        private Vector3 NetworkRespawnPosition
        {
            get;
            set;
        }

        [Networked] // 부활 회전 동기화
        private Vector3 NetworkRespawnEulerAngles
        {
            get;
            set;
        }

        [Networked] // 체크포인트 활성화 횟수 동기화
        private int NetworkCheckpointActivationCount
        {
            get;
            set;
        }

        [Networked] // 부활 보호 종료 Tick 동기화
        private TickTimer NetworkRespawnProtectionTimer
        {
            get;
            set;
        }

        [Networked] // 누적 부활 횟수 동기화
        private int NetworkRespawnCount
        {
            get;
            set;
        }

        [Networked] // 마지막 부활 원인 동기화
        private int NetworkLastRespawnReason
        {
            get;
            set;
        }

        [Networked] // 현재 발 높이 동기화
        private float NetworkRaceHeight
        {
            get;
            set;
        }

        [Networked] // 경기 중 최고 높이 동기화
        private float NetworkBestRaceHeight
        {
            get;
            set;
        }

        [Networked] // 현재 경쟁 순위 동기화
        private int NetworkRaceRank
        {
            get;
            set;
        }

        [Networked] // 개인 FINISH 여부 동기화
        private NetworkBool NetworkIsFinished
        {
            get;
            set;
        }

        [Networked] // 개인 최종 결과 고정 여부 동기화
        private NetworkBool NetworkResultLocked
        {
            get;
            set;
        }

        [Networked] // 개인 최종 순위 동기화
        private int NetworkFinalRank
        {
            get;
            set;
        }

        [Networked] // 정상 도달 경과 시간 동기화
        private float NetworkFinishElapsedSeconds
        {
            get;
            set;
        }

        [Networked] // 74일차 Lobby Ready 상태 동기화
        private NetworkBool NetworkLobbyReady
        {
            get;
            set;
        }

        [Networked] // 경기 전체 상태 동기화
        private int NetworkMatchStateValue
        {
            get;
            set;
        }

        [Networked] // 시작 카운트다운 동기화
        private TickTimer NetworkCountdownTimer
        {
            get;
            set;
        }

        [Networked] // 경기 제한 시간 동기화
        private TickTimer NetworkMatchTimer
        {
            get;
            set;
        }

        [Networked] // 경기 종료 원인 동기화
        private int NetworkMatchEndReasonValue
        {
            get;
            set;
        }

        public Vector3 ExternalVelocity =>
            NetworkExternalVelocity; // 현재 외부 속도 조회

        public ProjectJExternalForceSource LastExternalForceSource =>
            (ProjectJExternalForceSource)NetworkLastExternalForceSource; // 마지막 외력 원인 조회

        public int ExternalForceApplyCount =>
            NetworkExternalForceApplyCount; // 외력 적용 횟수 조회

        public ProjectJNetworkPushResult LastPushResult =>
            (ProjectJNetworkPushResult)NetworkLastPushResult; // 마지막 밀치기 결과 조회

        public int LastPushTargetIndex =>
            NetworkLastPushTargetIndex; // 마지막 밀치기 대상 조회

        public int PushAttemptCount =>
            NetworkPushAttemptCount; // 밀치기 시도 횟수 조회

        public int PushSuccessCount =>
            NetworkPushSuccessCount; // 밀치기 성공 횟수 조회

        public CheckpointId CurrentCheckpointId =>
            (CheckpointId)NetworkCheckpointId; // 현재 최고 체크포인트 조회

        public Vector3 RespawnPosition =>
            NetworkRespawnPosition; // 현재 부활 위치 조회

        public Quaternion RespawnRotation =>
            Quaternion.Euler(NetworkRespawnEulerAngles); // 현재 부활 회전 조회

        public int CheckpointActivationCount =>
            NetworkCheckpointActivationCount; // 체크포인트 활성화 횟수 조회

        public int RespawnCount =>
            NetworkRespawnCount; // 누적 부활 횟수 조회

        public ProjectJNetworkRespawnReason LastRespawnReason =>
            (ProjectJNetworkRespawnReason)NetworkLastRespawnReason; // 마지막 부활 원인 조회

        public float RaceHeight =>
            NetworkRaceHeight; // 현재 발 높이 조회

        public float BestRaceHeight =>
            NetworkBestRaceHeight; // 경기 중 최고 높이 조회

        public int RaceRank =>
            NetworkRaceRank; // 현재 또는 고정 순위 조회

        public bool IsFinished =>
            NetworkIsFinished; // 정상 도달 여부 조회

        public bool IsResultLocked =>
            NetworkResultLocked; // 개인 결과 확정 여부 조회

        public int FinalRank =>
            NetworkFinalRank; // 개인 최종 순위 조회

        public float FinishElapsedSeconds =>
            NetworkFinishElapsedSeconds; // 정상 도달 경과 시간 조회

        public bool LobbyReady =>
            NetworkLobbyReady; // Lobby Ready 상태 조회

        public int LobbyPlayerIndex =>
            Object != null && Object.IsValid
                ? Object.InputAuthority.AsIndex
                : -1; // Lobby Player 번호 조회

        public ProjectJNetworkMatchState MatchState
        {
            get
            {
                ProjectJNetworkExternalGameplay coordinator =
                    GetMatchCoordinator(); // 경기 기준 Player 조회

                if (coordinator == null)
                {
                    return ProjectJNetworkMatchState.Preparing; // 기준 Player 없음 처리
                }

                return (ProjectJNetworkMatchState)coordinator.NetworkMatchStateValue; // 기준 Player 경기 상태 반환
            }
        }

        public ProjectJNetworkMatchEndReason MatchEndReason
        {
            get
            {
                ProjectJNetworkExternalGameplay coordinator =
                    GetMatchCoordinator(); // 경기 기준 Player 조회

                if (coordinator == null)
                {
                    return ProjectJNetworkMatchEndReason.None; // 기준 Player 없음 처리
                }

                return (ProjectJNetworkMatchEndReason)coordinator.NetworkMatchEndReasonValue; // 경기 종료 원인 반환
            }
        }

        public bool GameplayInputAllowed
        {
            get
            {
                if (NetworkResultLocked) // 개인 결과 확정 확인
                {
                    return false; // 완주·종료 Player 입력 차단
                }

                ProjectJNetworkExternalGameplay coordinator =
                    GetMatchCoordinator(); // 경기 기준 Player 조회

                if (
                    coordinator == null ||
                    coordinator.Runner == null
                )
                {
                    return false; // 경기 기준 없음 처리
                }

                ProjectJNetworkMatchState state =
                    (ProjectJNetworkMatchState)coordinator.NetworkMatchStateValue; // 기준 경기 상태 조회

                if (state == ProjectJNetworkMatchState.Playing)
                {
                    return true; // Playing 입력 허용
                }

                return
                    state == ProjectJNetworkMatchState.Countdown &&
                    coordinator.NetworkCountdownTimer.ExpiredOrNotRunning(coordinator.Runner); // 카운트다운 종료 Tick부터 동시 허용
            }
        }

        public bool IsRespawnProtected
        {
            get
            {
                if (Runner == null)
                {
                    return false;
                }

                return !NetworkRespawnProtectionTimer.ExpiredOrNotRunning(Runner); // 보호 Timer 실행 여부 반환
            }
        }

        public float RespawnProtectionRemaining =>
            GetRemainingTime(NetworkRespawnProtectionTimer, Runner); // 남은 부활 보호 시간 조회

        public float PushCooldownRemaining =>
            GetRemainingTime(NetworkPushCooldown, Runner); // 남은 밀치기 쿨타임 조회

        public float CountdownRemaining
        {
            get
            {
                ProjectJNetworkExternalGameplay coordinator =
                    GetMatchCoordinator(); // 경기 기준 Player 조회

                return coordinator == null
                    ? 0f
                    : GetRemainingTime(coordinator.NetworkCountdownTimer, coordinator.Runner); // 남은 시작 시간 조회
            }
        }

        public float MatchTimeRemaining
        {
            get
            {
                ProjectJNetworkExternalGameplay coordinator =
                    GetMatchCoordinator(); // 경기 기준 Player 조회

                return coordinator == null
                    ? 0f
                    : GetRemainingTime(coordinator.NetworkMatchTimer, coordinator.Runner); // 남은 경기 시간 조회
            }
        }

        public override void Spawned()
        {
            ActivePlayers.Add(this); // Player Registry 등록
            ResolveReferences(); // 필수 참조 조회

            if (!Object.HasStateAuthority)
            {
                return;
            }

            NetworkExternalVelocity = Vector3.zero; // 외부 속도 초기화
            NetworkLastExternalForceSource = (int)ProjectJExternalForceSource.None; // 외력 원인 초기화
            NetworkExternalForceApplyCount = 0; // 외력 횟수 초기화
            NetworkPushForward = Vector3.forward; // 기본 밀치기 방향 초기화
            NetworkPushCooldown = TickTimer.None; // 밀치기 쿨타임 초기화
            NetworkLastPushResult = (int)ProjectJNetworkPushResult.None; // 밀치기 결과 초기화
            NetworkLastPushTargetIndex = -1; // 밀치기 대상 초기화
            NetworkPushAttemptCount = 0; // 밀치기 시도 횟수 초기화
            NetworkPushSuccessCount = 0; // 밀치기 성공 횟수 초기화
            NetworkCheckpointId = (int)CheckpointId.Start; // 체크포인트 초기화
            NetworkRespawnPosition = transform.position; // 최초 부활 위치 저장
            NetworkRespawnEulerAngles = transform.rotation.eulerAngles; // 최초 부활 회전 저장
            NetworkCheckpointActivationCount = 0; // 체크포인트 횟수 초기화
            NetworkRespawnProtectionTimer = TickTimer.None; // 최초 Spawn 보호 없음
            NetworkRespawnCount = 0; // 부활 횟수 초기화
            NetworkLastRespawnReason = (int)ProjectJNetworkRespawnReason.None; // 부활 원인 초기화
            NetworkRaceHeight = TruncateHeight(transform.position.y); // 최초 높이 저장
            NetworkBestRaceHeight = NetworkRaceHeight; // 최초 최고 높이 저장
            NetworkRaceRank = 1; // 최초 순위 초기화
            NetworkIsFinished = false; // FINISH 상태 초기화
            NetworkResultLocked = false; // 결과 잠금 초기화
            NetworkFinalRank = 0; // 최종 순위 초기화
            NetworkFinishElapsedSeconds = -1f; // FINISH 시간 초기화
            NetworkLobbyReady = false; // Lobby 최초 Ready 해제
            NetworkMatchStateValue = (int)ProjectJNetworkMatchState.Preparing; // 경기 상태 초기화
            NetworkCountdownTimer = TickTimer.None; // 카운트다운 초기화
            NetworkMatchTimer = TickTimer.None; // 경기 타이머 초기화
            NetworkMatchEndReasonValue = (int)ProjectJNetworkMatchEndReason.None; // 종료 원인 초기화
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            ActivePlayers.Remove(this); // Player Registry 제거
        }

        private void Update()
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasInputAuthority
            )
            {
                return;
            }

            Keyboard keyboard = Keyboard.current; // 현재 키보드 조회

            if (keyboard == null)
            {
                return;
            }

            if (
                keyboard.rKey.wasPressedThisFrame &&
                IsLobbySceneActive()
            )
            {
                RequestToggleLobbyReady(); // Lobby에서는 R로 Ready 전환
                return; // 같은 입력을 수동 부활로 사용하지 않음
            }

            if (
                keyboard.rKey.wasPressedThisFrame &&
                GameplayInputAllowed
            )
            {
                RequestManualRespawn(); // Game에서는 R 직접 부활 테스트
            }

            if (
                keyboard[ // 단독 경기 시작 키 조회
                    ProjectJNetworkDebugHotkeyPolicy.GetKey( // 공통 단축키 정책 호출
                        ProjectJNetworkDebugAction.SoloStart // 단독 경기 시작 기능 전달
                    )
                ].wasPressedThisFrame && // 현재 프레임 F5 입력 확인
                IsGameSceneActive() &&
                Object.HasStateAuthority &&
                GetMatchCoordinator() == this
            )
            {
                BeginCountdownAuthority(); // F5 단독 Game Scene 테스트 경기 시작
            }

            if (
                keyboard[ // 강제 경기 종료 키 조회
                    ProjectJNetworkDebugHotkeyPolicy.GetKey( // 공통 단축키 정책 호출
                        ProjectJNetworkDebugAction.ForceMatchEnd // 강제 경기 종료 기능 전달
                    )
                ].wasPressedThisFrame && // 현재 프레임 F11 입력 확인
                ProjectJNetworkDebugHotkeyPolicy.CanForceMatchEnd( // 강제 경기 종료 조건 검사
                    IsGameSceneActive(), // Game Scene 활성 여부 전달
                    Object.HasStateAuthority, // State Authority 보유 여부 전달
                    GetMatchCoordinator() == this, // Match Coordinator 일치 여부 전달
                    GameplayInputAllowed // 실제 경기 입력 허용 여부 전달
                )
            )
            {
                FinishMatchAuthority(ProjectJNetworkMatchEndReason.TimeExpired); // F11 제한 시간 종료 테스트
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority)
            {
                return;
            }

            ResolveReferences(); // 런타임 참조 보정

            ProjectJNetworkExternalGameplay coordinator =
                GetMatchCoordinator(); // 경기 기준 Player 조회

            if (coordinator == this)
            {
                UpdateMatchStateAuthority(); // 경기 전체 상태는 기준 Player만 확정
            }

            if (!GameplayInputAllowed)
            {
                NetworkExternalVelocity = Vector3.zero; // 경기 잠금 중 외력 이동 정지
                UpdateRaceHeightAndBest(); // 현재 결과용 높이 유지

                if (NetworkResultLocked)
                {
                    NetworkRaceRank = NetworkFinalRank; // 확정 순위 유지
                }

                return;
            }

            if (GetInput<ProjectJNetworkInput>(out ProjectJNetworkInput input))
            {
                UpdatePushForward(input.Move); // 마지막 이동 방향 갱신

                if (input.Buttons.IsSet(ProjectJNetworkButton.Push))
                {
                    ProcessPush(); // State Authority 밀치기 판정
                }
            }

            SimulateExternalVelocity(); // 외부 속도 이동과 감속 처리

            if (EvaluateFallRespawn())
            {
                UpdateRaceHeightAndBest(); // 부활 위치 높이 즉시 갱신
                UpdateRaceRank(); // 부활 직후 순위 갱신
                return;
            }

            UpdateRaceHeightAndBest(); // 현재 높이와 최고 높이 갱신
            UpdateRaceRank(); // 실시간 순위 갱신
        }

        public bool TryApplyExternalVelocityChange(
            ProjectJExternalForceSource source,
            Vector3 velocityChange
        )
        {
            return TryApplyExternalVelocityChangeInternal( // 기존 수평 외력 처리 위임
                source, // 외력 원인 전달
                velocityChange, // 외력 속도 전달
                false // 수직 성분 제거
            );
        }

        internal bool TryApplyExternalVelocityChange3D( // 지뢰용 3차원 외력 적용
            ProjectJExternalForceSource source, // 외력 원인
            Vector3 velocityChange // 수직 성분 포함 외력 속도
        )
        {
            return TryApplyExternalVelocityChangeInternal( // 공통 보호 판정 처리 위임
                source, // 외력 원인 전달
                velocityChange, // 3차원 외력 전달
                true // 수직 성분 유지
            );
        }

        private bool TryApplyExternalVelocityChangeInternal( // 공통 외력 적용 처리
            ProjectJExternalForceSource source, // 외력 원인
            Vector3 velocityChange, // 적용할 속도 변화
            bool allowVertical // 수직 성분 허용 여부
        )
        {
            if (!Object.HasStateAuthority)
            {
                return false;
            }

            if (!GameplayInputAllowed)
            {
                return false; // 경기 전·완주 후·경기 종료 외력 차단
            }

            ResolveReferences(); // 아이템 보호 상태 참조 보정

            if (
                itemInventory != null &&
                itemInventory.BlocksExternalForce(source)
            )
            {
                return false; // 젤리 보호막이 Push·Item 외력 차단
            }

            if (
                IsRespawnProtected &&
                IsHostileExternalForce(source)
            )
            {
                return false; // 보호 중 적대 외력 차단
            }

            if (!allowVertical) // 기존 수평 외력 확인
            {
                velocityChange.y = 0f; // 기존 아이템·밀치기 수직 성분 제거
            }

            if (velocityChange.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            NetworkExternalVelocity += velocityChange; // 외부 속도 합산
            NetworkLastExternalForceSource = (int)source; // 외력 원인 저장
            NetworkExternalForceApplyCount++; // 외력 횟수 증가

            return true;
        }

        internal bool TryApplySnowballSlowAuthority( // 눈덩이 적중 감속 적용
            PlayerRef sourceOwner // 투척 사용자
        )
        {
            ResolveReferences(); // Target 아이템 상태 참조 보정

            bool runnerReady =
                Runner != null &&
                Object != null &&
                Object.IsValid &&
                Object.HasStateAuthority; // Target 서버 권한 준비 여부

            bool isOwner =
                Object != null &&
                Object.IsValid &&
                Object.InputAuthority == sourceOwner; // 투척 사용자 자기 적중 여부

            bool isShielded =
                itemInventory != null &&
                itemInventory.BlocksExternalForce(ProjectJExternalForceSource.Item); // 젤리 보호막 상태 조회

            bool canAffect = ProjectJSnowballPolicy.CanAffectTarget(
                runnerReady,
                GameplayInputAllowed,
                isOwner,
                IsFinished,
                IsRespawnProtected,
                isShielded
            ); // 눈덩이 적중 조건 계산

            if (!canAffect || itemInventory == null)
            {
                return false; // 보호·완주·누락 Target 차단
            }

            return itemInventory.ApplySnowballSlowAuthority(); // Target 감속 Timer 적용
        }

        internal bool CanReceiveMineExplosionAuthority( // 지뢰 폭발 Target 사전 판정
            PlayerRef sourceOwner // 지뢰 설치 사용자
        )
        {
            ResolveReferences(); // Target 아이템 상태 참조 보정

            bool runnerReady =
                Runner != null && // Runner 존재 조건
                Object != null && // NetworkObject 존재 조건
                Object.IsValid && // NetworkObject 유효 조건
                Object.HasStateAuthority; // Target 서버 권한 조건

            bool isOwner =
                Object != null && // NetworkObject 존재 조건
                Object.IsValid && // NetworkObject 유효 조건
                Object.InputAuthority == sourceOwner; // 설치 사용자 자기 판정

            bool isShielded =
                itemInventory != null && // 인벤토리 존재 조건
                itemInventory.BlocksExternalForce(ProjectJExternalForceSource.Item); // Jelly 보호막 판정

            return ProjectJMinePolicy.CanAffectTarget( // 공통 지뢰 보호 정책 적용
                runnerReady, // 서버 권한 상태 전달
                GameplayInputAllowed, // 경기 입력 상태 전달
                isOwner, // 소유자 상태 전달
                IsFinished, // 완주 상태 전달
                IsRespawnProtected, // 부활 보호 상태 전달
                isShielded // Jelly 보호막 상태 전달
            );
        }

        public void RequestToggleLobbyReady()
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasInputAuthority ||
                !IsLobbySceneActive() ||
                MatchState != ProjectJNetworkMatchState.Preparing
            )
            {
                return; // Lobby의 Preparing 상태에서만 Ready 변경 허용
            }

            if (Object.HasStateAuthority)
            {
                ToggleLobbyReadyAuthority(); // Host 자신의 Ready 직접 변경
                return;
            }

            RPC_RequestToggleLobbyReady(); // Client Ready 변경 요청
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_RequestToggleLobbyReady()
        {
            if (
                !IsLobbySceneActive() ||
                MatchState != ProjectJNetworkMatchState.Preparing
            )
            {
                return; // State Authority에서 Lobby 상태 재검증
            }

            ToggleLobbyReadyAuthority(); // Client의 Ready 상태 확정
        }

        public bool PrepareForGameSceneAuthority(
            Vector3 spawnPosition,
            Quaternion spawnRotation
        )
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority ||
                !IsGameSceneActive()
            )
            {
                return false; // Host의 Game Scene 준비에서만 허용
            }

            ResolveReferences(); // 이동·아이템 참조 보정

            NetworkExternalVelocity = Vector3.zero; // Lobby에서 남은 외력 제거
            NetworkLastExternalForceSource =
                (int)ProjectJExternalForceSource.None; // 외력 원인 초기화
            NetworkPushCooldown = TickTimer.None; // Push 쿨타임 초기화
            NetworkRespawnProtectionTimer = TickTimer.None; // 보호 상태 초기화
            NetworkCheckpointId = (int)CheckpointId.Start; // 시작 체크포인트로 복원
            NetworkRespawnPosition = spawnPosition; // 시작 부활 위치 저장
            NetworkRespawnEulerAngles = spawnRotation.eulerAngles; // 시작 회전 저장
            NetworkLobbyReady = false; // Game 진입 후 Lobby Ready 제거

            if (itemInventory != null)
            {
                itemInventory.ClearAuthority(); // 경기 시작 전 아이템 상태 초기화
            }

            if (networkPlayer != null)
            {
                networkPlayer.ResetMotionForRespawn(); // 이동 상태 초기화
            }

            if (networkTransform != null)
            {
                networkTransform.Teleport(
                    spawnPosition,
                    spawnRotation
                ); // Network Player를 경기 시작 위치로 이동
            }
            else
            {
                transform.SetPositionAndRotation(
                    spawnPosition,
                    spawnRotation
                ); // NetworkTransform 누락 대비
            }

            NetworkRaceHeight = TruncateHeight(spawnPosition.y); // 시작 높이 저장
            NetworkBestRaceHeight = NetworkRaceHeight; // 최고 높이 초기화

            return true;
        }

        public bool TryBeginCountdownFromLobbyFlowAuthority()
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority ||
                !IsGameSceneActive() ||
                GetActivePlayerCount() < LobbyMatchMinimumPlayerCount
            )
            {
                return false; // 최소 인원과 Game Scene 조건 확인
            }

            ProjectJNetworkExternalGameplay coordinator =
                GetMatchCoordinator(); // 경기 Coordinator 조회

            if (
                coordinator == null ||
                !coordinator.Object.HasStateAuthority
            )
            {
                return false; // Host Coordinator 누락
            }

            coordinator.BeginCountdownAuthority(); // Ready Flow 승인 후 Countdown 시작

            return
                coordinator.MatchState ==
                ProjectJNetworkMatchState.Countdown; // 실제 시작 여부 반환
        }

        private void ToggleLobbyReadyAuthority()
        {
            if (
                !Object.HasStateAuthority ||
                !IsLobbySceneActive() ||
                MatchState != ProjectJNetworkMatchState.Preparing
            )
            {
                return; // Host의 유효한 Lobby 상태만 변경
            }

            NetworkLobbyReady = !NetworkLobbyReady; // Ready 토글

            Debug.Log(
                "[Project J/Fusion] 74일차 Lobby Ready / P" +
                Object.InputAuthority.AsIndex +
                " / " +
                (NetworkLobbyReady ? "READY" : "NOT READY")
            );
        }

        public void RequestManualRespawn()
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasInputAuthority ||
                !GameplayInputAllowed
            )
            {
                return;
            }

            if (Object.HasStateAuthority)
            {
                PerformRespawn(ProjectJNetworkRespawnReason.Manual); // Host 직접 부활
                return;
            }

            RPC_RequestManualRespawn(); // Client 부활 요청
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_RequestManualRespawn()
        {
            if (!GameplayInputAllowed)
            {
                return; // State Authority에서 경기 상태 재검증
            }

            PerformRespawn(ProjectJNetworkRespawnReason.Manual); // State Authority 부활 실행
        }

        public void ReceiveCheckpoint(
            global::ProjectJ.Checkpoint.Checkpoint checkpoint
        )
        {
            if (
                !Object.HasStateAuthority ||
                !GameplayInputAllowed ||
                checkpoint == null
            )
            {
                return;
            }

            int nextCheckpointId = (int)checkpoint.Id; // 접촉 Checkpoint 값 변환

            if (nextCheckpointId <= NetworkCheckpointId)
            {
                return;
            }

            NetworkCheckpointId = nextCheckpointId; // 최고 Checkpoint 저장
            NetworkRespawnPosition = checkpoint.RespawnPosition; // 부활 위치 저장
            NetworkRespawnEulerAngles = checkpoint.RespawnRotation.eulerAngles; // 부활 회전 저장
            NetworkCheckpointActivationCount++; // 활성화 횟수 증가

            Debug.Log(
                "[Project J/Fusion] 71일차 Checkpoint 저장 / P" +
                Object.InputAuthority.AsIndex +
                " / " +
                checkpoint.Id
            );
        }

        public void ReceiveFinish()
        {
            if (
                !Object.HasStateAuthority ||
                !GameplayInputAllowed ||
                NetworkResultLocked
            )
            {
                return;
            }

            ConfirmFinishAuthority(); // State Authority FINISH 확정
        }

        private void UpdateMatchStateAuthority()
        {
            ProjectJNetworkMatchState state =
                (ProjectJNetworkMatchState)NetworkMatchStateValue; // 현재 경기 상태 조회

            if (state == ProjectJNetworkMatchState.Preparing)
            {
                return; // 74일차부터 Lobby Ready Flow가 Countdown 시작을 승인
            }

            if (
                state == ProjectJNetworkMatchState.Countdown &&
                NetworkCountdownTimer.ExpiredOrNotRunning(Runner)
            )
            {
                BeginPlayingAuthority(); // 카운트다운 종료 후 경기 시작
                return;
            }

            if (
                state == ProjectJNetworkMatchState.Playing &&
                NetworkMatchTimer.ExpiredOrNotRunning(Runner)
            )
            {
                FinishMatchAuthority(ProjectJNetworkMatchEndReason.TimeExpired); // 10분 종료 처리
            }
        }

        private void BeginCountdownAuthority()
        {
            if (
                !Object.HasStateAuthority ||
                GetMatchCoordinator() != this ||
                (ProjectJNetworkMatchState)NetworkMatchStateValue != ProjectJNetworkMatchState.Preparing
            )
            {
                return;
            }

            ResetAllPlayersForMatchAuthority(); // 참가 Player 경기 상태 초기화
            NetworkMatchStateValue = (int)ProjectJNetworkMatchState.Countdown; // 카운트다운 상태 확정
            NetworkCountdownTimer = TickTimer.CreateFromSeconds(Runner, CountdownSeconds); // 3초 카운트다운 시작
            NetworkMatchTimer = TickTimer.None; // 경기 타이머 대기
            NetworkMatchEndReasonValue = (int)ProjectJNetworkMatchEndReason.None; // 종료 원인 초기화

            Debug.Log("[Project J/Fusion] 71일차 Countdown 시작 / 3초");
        }

        private void BeginPlayingAuthority()
        {
            NetworkMatchStateValue = (int)ProjectJNetworkMatchState.Playing; // 경기 진행 상태 확정
            NetworkCountdownTimer = TickTimer.None; // 카운트다운 종료
            NetworkMatchTimer = TickTimer.CreateFromSeconds(Runner, MatchDurationSeconds); // 10분 경기 타이머 시작

            foreach (ProjectJNetworkExternalGameplay candidate in ActivePlayers)
            {
                if (!IsValidPlayer(candidate))
                {
                    continue;
                }

                candidate.UpdateRaceHeightAndBest(); // 시작 높이 갱신
                candidate.UpdateRaceRank(); // 시작 순위 갱신
            }

            Debug.Log("[Project J/Fusion] 71일차 Match 시작 / 10분");
        }

        private void FinishMatchAuthority(
            ProjectJNetworkMatchEndReason reason
        )
        {
            if (
                !Object.HasStateAuthority ||
                GetMatchCoordinator() != this
            )
            {
                return;
            }

            ProjectJNetworkMatchState state =
                (ProjectJNetworkMatchState)NetworkMatchStateValue; // 현재 경기 상태 조회

            if (state == ProjectJNetworkMatchState.Finished)
            {
                return;
            }

            foreach (ProjectJNetworkExternalGameplay candidate in ActivePlayers)
            {
                if (!IsValidPlayer(candidate))
                {
                    continue;
                }

                candidate.UpdateRaceHeightAndBest(); // 종료 Tick 높이 확정 준비
            }

            LockUnfinishedResultsByHeightAuthority(); // 미완주 Player 최종 순위 확정
            NetworkMatchStateValue = (int)ProjectJNetworkMatchState.Finished; // 경기 종료 상태 확정
            NetworkMatchEndReasonValue = (int)reason; // 종료 원인 저장
            NetworkCountdownTimer = TickTimer.None; // 카운트다운 제거
            NetworkMatchTimer = TickTimer.None; // 경기 타이머 제거

            Debug.Log(
                "[Project J/Fusion] 71일차 Match 종료 / " +
                reason
            );
        }

        private void ResetAllPlayersForMatchAuthority()
        {
            foreach (ProjectJNetworkExternalGameplay candidate in ActivePlayers)
            {
                if (
                    !IsValidPlayer(candidate) ||
                    !candidate.Object.HasStateAuthority
                )
                {
                    continue;
                }

                candidate.NetworkExternalVelocity = Vector3.zero; // 시작 전 외력 제거
                candidate.NetworkLastExternalForceSource = (int)ProjectJExternalForceSource.None; // 외력 원인 초기화
                candidate.NetworkPushCooldown = TickTimer.None; // 밀치기 쿨타임 초기화
                candidate.NetworkLastPushResult = (int)ProjectJNetworkPushResult.None; // 밀치기 결과 초기화
                candidate.NetworkLastPushTargetIndex = -1; // 밀치기 대상 초기화
                candidate.NetworkRespawnProtectionTimer = TickTimer.None; // 보호 상태 초기화
                candidate.NetworkRaceHeight = TruncateHeight(candidate.transform.position.y); // 시작 높이 저장
                candidate.NetworkBestRaceHeight = candidate.NetworkRaceHeight; // 최고 높이 초기화
                candidate.NetworkRaceRank = 1; // 실시간 순위 초기화
                candidate.NetworkIsFinished = false; // FINISH 상태 초기화
                candidate.NetworkResultLocked = false; // 결과 잠금 초기화
                candidate.NetworkFinalRank = 0; // 최종 순위 초기화
                candidate.NetworkFinishElapsedSeconds = -1f; // FINISH 시간 초기화
                candidate.NetworkLobbyReady = false; // 경기 진입 후 Lobby Ready 초기화
                candidate.ResolveReferences(); // Player 참조 보정

                if (candidate.networkPlayer != null)
                {
                    candidate.networkPlayer.StopMotionForMatchLock(); // 카운트다운 시작 시 이동 정지
                }
            }
        }

        private void ConfirmFinishAuthority()
        {
            UpdateRaceHeightAndBest(); // FINISH 순간 높이와 최고 높이 확정

            int finishOrder =
                GetFinishedPlayerCount() + 1; // 현재까지 도착한 인원 다음 순서

            NetworkIsFinished = true; // 정상 도달 저장
            NetworkResultLocked = true; // 개인 결과 즉시 고정
            NetworkFinalRank = finishOrder; // 도착 순서를 최종 순위로 저장
            NetworkRaceRank = finishOrder; // 표시 순위를 최종 순위로 교체
            NetworkFinishElapsedSeconds = Mathf.Clamp(
                MatchDurationSeconds - MatchTimeRemaining,
                0f,
                MatchDurationSeconds
            ); // 서버 경기 타이머 기준 도착 시간 저장
            NetworkExternalVelocity = Vector3.zero; // FINISH 후 외력 제거
            NetworkRespawnProtectionTimer = TickTimer.None; // FINISH 후 보호 Timer 제거

            ResolveReferences();

            if (networkPlayer != null)
            {
                networkPlayer.StopMotionForMatchLock(); // FINISH Player 이동 정지
            }

            Debug.Log(
                "[Project J/Fusion] 71일차 FINISH / P" +
                Object.InputAuthority.AsIndex +
                " / Rank " +
                NetworkFinalRank +
                " / " +
                NetworkFinishElapsedSeconds.ToString("F2") +
                "s"
            );

            RefreshAllLiveRanksAuthority(); // 남은 Player 순위에 완주자 수 반영

            ProjectJNetworkExternalGameplay coordinator =
                GetMatchCoordinator(); // 경기 기준 Player 조회

            if (
                coordinator != null &&
                coordinator.Object.HasStateAuthority &&
                GetUnfinishedPlayerCount() == 0
            )
            {
                coordinator.FinishMatchAuthority(ProjectJNetworkMatchEndReason.AllFinished); // 전원 완주 즉시 경기 종료
            }
        }

        private void LockUnfinishedResultsByHeightAuthority()
        {
            int finishedCount =
                GetFinishedPlayerCount(); // 이미 정상 도달한 인원 수

            foreach (ProjectJNetworkExternalGameplay player in ActivePlayers)
            {
                if (
                    !IsValidPlayer(player) ||
                    player.NetworkResultLocked
                )
                {
                    continue;
                }

                int higherUnfinishedCount = 0; // 자신보다 높은 미완주 Player 수

                foreach (ProjectJNetworkExternalGameplay candidate in ActivePlayers)
                {
                    if (
                        !IsValidPlayer(candidate) ||
                        candidate == player ||
                        candidate.NetworkIsFinished
                    )
                    {
                        continue;
                    }

                    if (candidate.NetworkRaceHeight > player.NetworkRaceHeight)
                    {
                        higherUnfinishedCount++; // 높이가 더 높은 미완주 Player 계산
                    }
                }

                player.NetworkFinalRank =
                    finishedCount + 1 + higherUnfinishedCount; // 완주자 뒤에서 경쟁 순위 확정
                player.NetworkRaceRank = player.NetworkFinalRank; // 표시 순위 고정
                player.NetworkResultLocked = true; // 결과 확정
                player.NetworkExternalVelocity = Vector3.zero; // 종료 후 외력 제거
                player.ResolveReferences();

                if (player.networkPlayer != null)
                {
                    player.networkPlayer.StopMotionForMatchLock(); // 종료 Player 이동 정지
                }
            }
        }

        private void RefreshAllLiveRanksAuthority()
        {
            foreach (ProjectJNetworkExternalGameplay candidate in ActivePlayers)
            {
                if (
                    !IsValidPlayer(candidate) ||
                    candidate.NetworkResultLocked
                )
                {
                    continue;
                }

                candidate.UpdateRaceHeightAndBest(); // 현재 높이 보정
                candidate.UpdateRaceRank(); // 완주자 Offset 반영
            }
        }

        private void ResolveReferences()
        {
            if (networkPlayer == null)
            {
                networkPlayer = GetComponent<ProjectJNetworkPlayer>(); // 같은 오브젝트 Network Player 조회
            }

            if (itemInventory == null)
            {
                itemInventory = GetComponent<ProjectJNetworkItemInventory>(); // 젤리 보호막 상태 조회
            }

            if (networkTransform == null)
            {
                networkTransform = GetComponent<NetworkTransform>(); // 같은 오브젝트 NetworkTransform 조회
            }

            if (fallLimitSet == null)
            {
                fallLimitSet = FindFirstObjectByType<CheckpointFallLimitSet>(); // 현재 Scene 낙하 한계 조회
            }
        }

        private bool EvaluateFallRespawn()
        {
            if (
                fallLimitSet == null ||
                NetworkResultLocked
            )
            {
                return false;
            }

            float fallLimitY =
                fallLimitSet.GetFallLimitY(CurrentCheckpointId); // 현재 Checkpoint 낙하 기준 조회

            if (transform.position.y >= fallLimitY)
            {
                return false;
            }

            PerformRespawn(ProjectJNetworkRespawnReason.Fall); // 낙하 부활 실행
            return true;
        }

        private void PerformRespawn(
            ProjectJNetworkRespawnReason reason
        )
        {
            if (
                !Object.HasStateAuthority ||
                NetworkResultLocked ||
                !GameplayInputAllowed
            )
            {
                return;
            }

            ResolveReferences();
            itemInventory?.HandleRespawnAuthority(); // 부활 직전 지속·준비 효과 정리
            NetworkExternalVelocity = Vector3.zero; // 이전 외력 제거
            NetworkLastExternalForceSource = (int)ProjectJExternalForceSource.None; // 외력 원인 초기화

            if (networkPlayer != null)
            {
                networkPlayer.ResetMotionForRespawn(); // 수직 이동 상태 초기화
            }

            Quaternion respawnRotation =
                RespawnRotation; // 저장된 부활 회전 조회

            if (networkTransform != null)
            {
                networkTransform.Teleport(
                    NetworkRespawnPosition,
                    respawnRotation
                ); // 네트워크 순간이동
            }
            else
            {
                transform.SetPositionAndRotation(
                    NetworkRespawnPosition,
                    respawnRotation
                ); // NetworkTransform 누락 대비
            }

            NetworkRespawnProtectionTimer = TickTimer.CreateFromSeconds(
                Runner,
                RespawnProtectionSeconds
            ); // 3초 부활 보호 시작

            NetworkRespawnCount++; // 부활 횟수 증가
            NetworkLastRespawnReason = (int)reason; // 부활 원인 저장
            NetworkRaceHeight = TruncateHeight(NetworkRespawnPosition.y); // 부활 높이 저장

            Debug.Log(
                "[Project J/Fusion] 71일차 Respawn / P" +
                Object.InputAuthority.AsIndex +
                " / " +
                reason
            );
        }

        private void UpdatePushForward(
            Vector2 moveInput
        )
        {
            if (moveInput.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Vector3 nextForward =
                new Vector3(
                    moveInput.x,
                    0f,
                    moveInput.y
                ); // XZ 진행 방향 생성

            if (nextForward.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            NetworkPushForward = nextForward.normalized; // 마지막 진행 방향 저장
        }

        private void ProcessPush()
        {
            ResolveReferences(); // 되감기 상태 참조 보정

            if (
                itemInventory != null &&
                itemInventory.IsRewindActive // 되감기 중 자신의 Push 입력 차단
            )
            {
                return;
            }

            if (
                NetworkResultLocked ||
                MatchState != ProjectJNetworkMatchState.Playing
            )
            {
                return; // 경기 외 Push 차단
            }

            NetworkPushAttemptCount++; // 밀치기 시도 횟수 증가
            NetworkLastPushTargetIndex = -1; // 이전 Target 초기화

            if (!NetworkPushCooldown.ExpiredOrNotRunning(Runner))
            {
                NetworkLastPushResult = (int)ProjectJNetworkPushResult.Cooldown; // 쿨타임 결과 저장
                return;
            }

            NetworkPushCooldown = TickTimer.CreateFromSeconds(
                Runner,
                CurrentPushCooldownSeconds
            ); // 현재 Push 재사용 시간 적용

            ProjectJNetworkExternalGameplay target =
                FindClosestPushTarget(); // 최근접 유효 Target 검색

            if (target == null)
            {
                NetworkLastPushResult = (int)ProjectJNetworkPushResult.Miss; // 대상 없음
                return;
            }

            NetworkLastPushTargetIndex =
                target.Object.InputAuthority.AsIndex; // Target 저장

            target.ResolveReferences(); // Target 보호 상태 참조 보정

            if (
                target.itemInventory != null &&
                target.itemInventory.BlocksExternalForce(ProjectJExternalForceSource.Push)
            )
            {
                NetworkLastPushResult = (int)ProjectJNetworkPushResult.Shielded; // 젤리 보호막 차단
                return;
            }

            if (target.IsRespawnProtected)
            {
                NetworkLastPushResult = (int)ProjectJNetworkPushResult.Protected; // 부활 보호 차단
                return;
            }

            Vector3 pushDirection =
                target.transform.position -
                transform.position; // Target 방향 계산

            pushDirection.y = 0f; // 수평 방향 제한

            if (pushDirection.sqrMagnitude <= 0.0001f)
            {
                pushDirection = NetworkPushForward; // 마지막 이동 방향 사용
            }

            if (pushDirection.sqrMagnitude <= 0.0001f)
            {
                pushDirection = Vector3.forward; // 기본 전방 사용
            }

            bool applied =
                target.TryApplyExternalVelocityChange(
                    ProjectJExternalForceSource.Push,
                    pushDirection.normalized * CurrentPushForce
                ); // Target 외력 적용

            if (!applied)
            {
                NetworkLastPushResult = target.IsRespawnProtected
                    ? (int)ProjectJNetworkPushResult.Protected
                    : (int)ProjectJNetworkPushResult.Invalid; // 실패 원인 저장
                return;
            }

            NetworkLastPushResult = (int)ProjectJNetworkPushResult.Success; // 성공 결과 저장
            NetworkPushSuccessCount++; // 성공 횟수 증가
        }

        private ProjectJNetworkExternalGameplay FindClosestPushTarget()
        {
            ProjectJNetworkExternalGameplay closestTarget = null; // 최근접 Target 초기화
            float closestDistanceSquared = float.PositiveInfinity; // 최근접 거리 초기화
            Vector3 forward = NetworkPushForward.sqrMagnitude > 0.0001f
                ? NetworkPushForward.normalized
                : Vector3.forward; // Push 기준 방향 선택

            foreach (ProjectJNetworkExternalGameplay candidate in ActivePlayers)
            {
                if (
                    !IsValidPlayer(candidate) ||
                    candidate == this ||
                    candidate.NetworkResultLocked ||
                    !candidate.Object.HasStateAuthority
                )
                {
                    continue; // 잘못된 Target 제외
                }

                Vector3 toTarget =
                    candidate.transform.position -
                    transform.position; // Target 방향 계산

                float distanceSquared =
                    toTarget.sqrMagnitude; // 거리 제곱 계산

                if (distanceSquared > CurrentPushSearchRange * CurrentPushSearchRange)
                {
                    continue;
                }

                Vector3 horizontalDirection =
                    new Vector3(
                        toTarget.x,
                        0f,
                        toTarget.z
                    ); // 수평 방향 계산

                if (horizontalDirection.sqrMagnitude > 0.0001f)
                {
                    float angle =
                        Vector3.Angle(
                            forward,
                            horizontalDirection.normalized
                        ); // 전방 각도 계산

                    if (angle > PushSearchHalfAngle)
                    {
                        continue;
                    }
                }

                if (distanceSquared >= closestDistanceSquared)
                {
                    continue;
                }

                closestTarget = candidate; // 최근접 Target 갱신
                closestDistanceSquared = distanceSquared; // 최근접 거리 갱신
            }

            return closestTarget;
        }

        private void SimulateExternalVelocity()
        {
            Vector3 externalVelocity =
                NetworkExternalVelocity; // 현재 외부 속도 조회

            if (externalVelocity.sqrMagnitude <= 0.0001f)
            {
                NetworkExternalVelocity = Vector3.zero; // 미세 외력 정리
                return;
            }

            transform.position +=
                externalVelocity *
                Runner.DeltaTime; // 외부 속도 위치 반영

            externalVelocity = Vector3.MoveTowards(
                externalVelocity,
                Vector3.zero,
                ExternalVelocityDecayPerSecond * Runner.DeltaTime
            ); // 외력 Tick 감속

            if (externalVelocity.magnitude <= ExternalVelocityStopThreshold)
            {
                externalVelocity = Vector3.zero; // 임계값 이하 외력 제거
            }

            NetworkExternalVelocity = externalVelocity; // 감속 결과 저장
        }

        private void UpdateRaceHeightAndBest()
        {
            NetworkRaceHeight =
                TruncateHeight(transform.position.y); // 발 기준 World Y 저장

            if (NetworkRaceHeight > NetworkBestRaceHeight)
            {
                NetworkBestRaceHeight = NetworkRaceHeight; // 최고 높이 갱신
            }
        }

        private void UpdateRaceRank()
        {
            if (NetworkResultLocked)
            {
                NetworkRaceRank = NetworkFinalRank; // 확정 순위 유지
                return;
            }

            int finishedCount = 0; // 자신보다 앞에서 확정된 완주자 수
            int higherUnfinishedCount = 0; // 자신보다 높은 미완주 Player 수

            foreach (ProjectJNetworkExternalGameplay candidate in ActivePlayers)
            {
                if (!IsValidPlayer(candidate))
                {
                    continue;
                }

                if (candidate.NetworkIsFinished)
                {
                    finishedCount++; // 완주자는 항상 미완주자보다 앞 순위
                    continue;
                }

                if (
                    candidate == this ||
                    candidate.NetworkResultLocked
                )
                {
                    continue;
                }

                if (candidate.NetworkRaceHeight > NetworkRaceHeight)
                {
                    higherUnfinishedCount++; // 높은 미완주 Player 수 계산
                }
            }

            NetworkRaceRank =
                finishedCount + 1 + higherUnfinishedCount; // 완주자 Offset 포함 경쟁 순위 저장
        }

        private static float TruncateHeight(
            float worldY
        )
        {
            int scaledHeight =
                (int)(worldY * 100f); // 셋째 자리 이하를 0 방향으로 버림

            return scaledHeight / 100f; // 0.00 높이 반환
        }

        private static bool IsHostileExternalForce(
            ProjectJExternalForceSource source
        )
        {
            return
                source == ProjectJExternalForceSource.Push ||
                source == ProjectJExternalForceSource.Item; // Player 간 적대 외력 판정
        }

        private static bool IsValidPlayer(
            ProjectJNetworkExternalGameplay player
        )
        {
            return
                player != null &&
                player.Object != null &&
                player.Object.IsValid; // Registry 유효 Player 판정
        }

        private static int GetActivePlayerCount()
        {
            int count = 0;

            foreach (ProjectJNetworkExternalGameplay candidate in ActivePlayers)
            {
                if (IsValidPlayer(candidate))
                {
                    count++;
                }
            }

            return count;
        }

        private static int GetFinishedPlayerCount()
        {
            int count = 0;

            foreach (ProjectJNetworkExternalGameplay candidate in ActivePlayers)
            {
                if (
                    IsValidPlayer(candidate) &&
                    candidate.NetworkIsFinished
                )
                {
                    count++;
                }
            }

            return count;
        }

        private static int GetUnfinishedPlayerCount()
        {
            int count = 0;

            foreach (ProjectJNetworkExternalGameplay candidate in ActivePlayers)
            {
                if (
                    IsValidPlayer(candidate) &&
                    !candidate.NetworkIsFinished
                )
                {
                    count++;
                }
            }

            return count;
        }

        private static ProjectJNetworkExternalGameplay GetMatchCoordinator()
        {
            ProjectJNetworkExternalGameplay coordinator = null; // 가장 낮은 PlayerRef를 경기 기준으로 사용

            foreach (ProjectJNetworkExternalGameplay candidate in ActivePlayers)
            {
                if (!IsValidPlayer(candidate))
                {
                    continue;
                }

                if (
                    coordinator == null ||
                    candidate.Object.InputAuthority.AsIndex <
                    coordinator.Object.InputAuthority.AsIndex
                )
                {
                    coordinator = candidate; // 더 낮은 PlayerRef 선택
                }
            }

            return coordinator;
        }

        private static bool IsLobbySceneActive()
        {
            Scene activeScene = SceneManager.GetActiveScene();

            return
                activeScene.IsValid() &&
                activeScene.name == LobbySceneName; // Lobby Scene 여부
        }

        private static bool IsGameSceneActive()
        {
            Scene activeScene = SceneManager.GetActiveScene();

            return
                activeScene.IsValid() &&
                (
                    activeScene.name == GameSceneName ||
                    activeScene.name == Day49AllSystemsTestSceneName
                ); // Game 또는 Day49 통합 테스트 Scene 여부
        }

        private static float GetRemainingTime(
            TickTimer timer,
            NetworkRunner runner
        )
        {
            if (runner == null)
            {
                return 0f;
            }

            float? remaining =
                timer.RemainingTime(runner); // Fusion TickTimer 남은 시간 조회

            if (!remaining.HasValue)
            {
                return 0f;
            }

            return Mathf.Max(
                0f,
                remaining.Value
            ); // 음수 방지 후 반환
        }

        private void OnGUI()
        {
            if (!ProjectJDebugOverlayController.IsVisible) // 통합 패널 선택 상태 확인
            {
                return; // 독립 진단창 출력 차단
            }

            if (!Application.isEditor && !Debug.isDebugBuild)
            {
                return;
            }

            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasInputAuthority
            )
            {
                return;
            }

            string protectionText = IsRespawnProtected
                ? RespawnProtectionRemaining.ToString("F2") + "s"
                : "OFF"; // 보호 상태 문자열

            string matchText = MatchState.ToString();

            if (MatchState == ProjectJNetworkMatchState.Countdown)
            {
                matchText +=
                    " / " +
                    Mathf.CeilToInt(CountdownRemaining); // 3·2·1 표시
            }
            else if (MatchState == ProjectJNetworkMatchState.Playing)
            {
                int remainingSeconds =
                    Mathf.CeilToInt(MatchTimeRemaining); // 남은 경기 시간 초 단위 계산

                matchText +=
                    " / " +
                    (remainingSeconds / 60).ToString("00") +
                    ":" +
                    (remainingSeconds % 60).ToString("00"); // MM:SS 표시
            }
            else if (MatchState == ProjectJNetworkMatchState.Finished)
            {
                matchText +=
                    " / " +
                    MatchEndReason; // 종료 원인 표시
            }

            string resultText = NetworkResultLocked
                ? (
                    "Final #" +
                    NetworkFinalRank +
                    " / FINISH " +
                    NetworkIsFinished +
                    (
                        NetworkIsFinished
                            ? " / " + NetworkFinishElapsedSeconds.ToString("F2") + "s"
                            : string.Empty
                    )
                )
                : "RUNNING"; // 개인 결과 문자열

            string debugText =
                "DAY 71 NETWORK\n" +
                "Match: " + matchText + "\n" +
                "Height: " + RaceHeight.ToString("F2") +
                " / Best: " + BestRaceHeight.ToString("F2") +
                " / Rank: " + RaceRank + "\n" +
                "Result: " + resultText + "\n" +
                "Respawn: " + LastRespawnReason +
                " x" + RespawnCount +
                " / Protection: " + protectionText + "\n" +
                "Push: " + LastPushResult +
                " / Target P" + LastPushTargetIndex +
                " / CD " + PushCooldownRemaining.ToString("F2") + "\n" +
                "Checkpoint: " + CurrentCheckpointId + "\n" +
                "R: Respawn / F5: Solo Start / F11: Force End"; // 102일차 디버그 단축키 상태

            GUI.Box(
                new Rect(
                    20f,
                    Screen.height - 205f,
                    650f,
                    185f
                ),
                debugText
            );
        }
    }
}
