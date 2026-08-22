using System.Collections.Generic; // 활성 플레이어 Registry 사용
using Fusion; // NetworkBehaviour와 Networked 상태 사용
using ProjectJ.Checkpoint; // 체크포인트와 낙하 한계 사용
using UnityEngine; // Unity 기본 타입 사용
using UnityEngine.InputSystem; // 70일차 직접 부활 테스트 입력 사용

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent] // 동일 컴포넌트 중복 방지
    [RequireComponent(typeof(ProjectJNetworkPlayer))] // Network Player 보장
    [RequireComponent(typeof(NetworkTransform))] // 네트워크 순간이동 보장
    public sealed class ProjectJNetworkExternalGameplay :
        NetworkBehaviour,
        ICheckpointReceiver
    {
        private const float ExternalVelocityDecayPerSecond = 12f; // 외력 초당 감속량
        private const float ExternalVelocityStopThreshold = 0.05f; // 외력 정지 임계값
        private const float PushSearchRange = 2.5f; // 밀치기 최대 거리
        private const float PushSearchHalfAngle = 45f; // 90도 전방 범위 절반 각도
        private const float PushForce = 12f; // 기본 밀치기 힘
        private const float PushCooldownSeconds = 1.5f; // 밀치기 재사용 대기시간
        private const float RespawnProtectionSeconds = 3f; // 부활 보호 시간

        private static readonly HashSet<ProjectJNetworkExternalGameplay> ActivePlayers =
            new HashSet<ProjectJNetworkExternalGameplay>(); // 현재 세션 Player Registry

        private ProjectJNetworkPlayer networkPlayer; // 이동 상태 초기화 대상
        private NetworkTransform networkTransform; // 순간이동 동기화 대상
        private CheckpointFallLimitSet fallLimitSet; // 현재 체크포인트별 낙하 한계

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

        [Networked] // 부활 회전 Euler 동기화
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

        [Networked] // 소수점 둘째 자리까지 버린 발 높이 동기화
        private float NetworkRaceHeight
        {
            get;
            set;
        }

        [Networked] // 실시간 경쟁 순위 동기화
        private int NetworkRaceRank
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
            NetworkRaceHeight; // 현재 경쟁 높이 조회

        public int RaceRank =>
            NetworkRaceRank; // 현재 실시간 순위 조회

        public bool IsRespawnProtected
        {
            get
            {
                if (Runner == null) // Runner 존재 확인
                {
                    return false; // Runner 없음 처리
                }

                return !NetworkRespawnProtectionTimer.ExpiredOrNotRunning(Runner); // 보호 Timer 실행 여부 반환
            }
        }

        public float RespawnProtectionRemaining
        {
            get
            {
                if (Runner == null) // Runner 존재 확인
                {
                    return 0f; // Runner 없음 처리
                }

                float? remaining = NetworkRespawnProtectionTimer.RemainingTime(Runner); // 남은 보호 시간 조회

                if (!remaining.HasValue) // Timer 없음 확인
                {
                    return 0f; // 보호 시간 없음 처리
                }

                return Mathf.Max(0f, remaining.Value); // 음수 방지 후 반환
            }
        }

        public float PushCooldownRemaining
        {
            get
            {
                if (Runner == null) // Runner 존재 확인
                {
                    return 0f; // Runner 없음 처리
                }

                float? remaining = NetworkPushCooldown.RemainingTime(Runner); // 남은 쿨타임 조회

                if (!remaining.HasValue) // Timer 없음 확인
                {
                    return 0f; // 쿨타임 없음 처리
                }

                return Mathf.Max(0f, remaining.Value); // 음수 방지 후 반환
            }
        }

        public override void Spawned()
        {
            ActivePlayers.Add(this); // Player Registry 등록
            ResolveReferences(); // 필수 참조 조회

            if (!Object.HasStateAuthority) // State Authority 확인
            {
                return; // 초기 네트워크 상태 쓰기 차단
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
            NetworkRespawnPosition = transform.position; // Spawn 위치를 최초 부활 위치로 저장
            NetworkRespawnEulerAngles = transform.rotation.eulerAngles; // Spawn 회전을 최초 부활 회전으로 저장
            NetworkCheckpointActivationCount = 0; // 체크포인트 횟수 초기화
            NetworkRespawnProtectionTimer = TickTimer.None; // 최초 Spawn 보호 없음
            NetworkRespawnCount = 0; // 부활 횟수 초기화
            NetworkLastRespawnReason = (int)ProjectJNetworkRespawnReason.None; // 부활 원인 초기화
            NetworkRaceHeight = TruncateHeight(transform.position.y); // 최초 발 높이 저장
            NetworkRaceRank = 1; // 최초 순위 초기화
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            ActivePlayers.Remove(this); // Player Registry 제거
        }

        private void Update()
        {
            if (
                Object == null || // NetworkObject 존재 확인
                !Object.IsValid || // NetworkObject 유효 확인
                !Object.HasInputAuthority // 로컬 소유 Player 확인
            )
            {
                return; // 원격 Player 입력 차단
            }

            Keyboard keyboard = Keyboard.current; // 현재 키보드 조회

            if (keyboard == null) // 키보드 연결 확인
            {
                return; // 키보드 없음 처리
            }

            if (keyboard.rKey.wasPressedThisFrame) // 70일차 직접 부활 테스트 키 확인
            {
                RequestManualRespawn(); // State Authority 부활 요청
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasStateAuthority) // Host State Authority 확인
            {
                return; // Client 직접 판정 차단
            }

            ResolveReferences(); // 런타임 참조 보정

            if (GetInput<ProjectJNetworkInput>(out ProjectJNetworkInput input)) // Player 입력 수신 확인
            {
                UpdatePushForward(input.Move); // 마지막 이동 방향 갱신

                if (input.Buttons.IsSet(ProjectJNetworkButton.Push)) // 밀치기 단발 입력 확인
                {
                    ProcessPush(); // State Authority 밀치기 판정
                }
            }

            SimulateExternalVelocity(); // 외부 속도 이동과 감속 처리

            if (EvaluateFallRespawn()) // 낙하 부활 발생 확인
            {
                UpdateRaceHeight(); // 부활 위치 높이 즉시 갱신
                UpdateRaceRank(); // 부활 직후 순위 즉시 갱신
                return; // 현재 Tick 추가 처리 종료
            }

            UpdateRaceHeight(); // 현재 발 높이 저장
            UpdateRaceRank(); // 경쟁 순위 저장
        }

        public bool TryApplyExternalVelocityChange(
            ProjectJExternalForceSource source,
            Vector3 velocityChange
        )
        {
            if (!Object.HasStateAuthority) // State Authority 확인
            {
                return false; // Client 외력 쓰기 차단
            }

            if (
                IsRespawnProtected && // 부활 보호 상태 확인
                IsHostileExternalForce(source) // 적대적 외력 확인
            )
            {
                return false; // Push와 Item 외력 차단
            }

            velocityChange.y = 0f; // 70일차 수평 외력 유지

            if (velocityChange.sqrMagnitude <= 0.0001f) // 유효 외력 확인
            {
                return false; // 너무 작은 외력 거부
            }

            NetworkExternalVelocity += velocityChange; // 외부 속도 합산
            NetworkLastExternalForceSource = (int)source; // 외력 원인 저장
            NetworkExternalForceApplyCount++; // 외력 적용 횟수 증가

            return true; // 외력 적용 성공
        }

        public void RequestManualRespawn()
        {
            if (
                Object == null || // NetworkObject 존재 확인
                !Object.IsValid || // NetworkObject 유효 확인
                !Object.HasInputAuthority // 요청 권한 확인
            )
            {
                return; // 원격 Player 요청 차단
            }

            if (Object.HasStateAuthority) // Host 자신의 Player 확인
            {
                PerformRespawn(ProjectJNetworkRespawnReason.Manual); // Host 직접 부활 실행
                return; // RPC 중복 호출 방지
            }

            RPC_RequestManualRespawn(); // Client에서 State Authority로 요청
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        private void RPC_RequestManualRespawn()
        {
            PerformRespawn(ProjectJNetworkRespawnReason.Manual); // State Authority 직접 부활 실행
        }

        public void ReceiveCheckpoint(global::ProjectJ.Checkpoint.Checkpoint checkpoint)
        {
            if (!Object.HasStateAuthority) // State Authority 확인
            {
                return; // Client 체크포인트 쓰기 차단
            }

            if (checkpoint == null) // 체크포인트 참조 확인
            {
                return; // 잘못된 참조 차단
            }

            int nextCheckpointId = (int)checkpoint.Id; // 접촉 체크포인트 값 변환

            if (nextCheckpointId <= NetworkCheckpointId) // 최고값 갱신 여부 확인
            {
                return; // 같은 값 또는 낮은 값 무시
            }

            NetworkCheckpointId = nextCheckpointId; // 최고 체크포인트 저장
            NetworkRespawnPosition = checkpoint.RespawnPosition; // 부활 위치 저장
            NetworkRespawnEulerAngles = checkpoint.RespawnRotation.eulerAngles; // 부활 회전 저장
            NetworkCheckpointActivationCount++; // 체크포인트 횟수 증가

            Debug.Log(
                "[Project J/Fusion] 70일차 Checkpoint 저장 / P" +
                Object.InputAuthority.AsIndex +
                " / " +
                checkpoint.Id +
                " / Respawn " +
                NetworkRespawnPosition
            ); // 체크포인트 확인 로그
        }

        private void ResolveReferences()
        {
            if (networkPlayer == null) // Network Player 참조 확인
            {
                networkPlayer = GetComponent<ProjectJNetworkPlayer>(); // 같은 오브젝트에서 조회
            }

            if (networkTransform == null) // NetworkTransform 참조 확인
            {
                networkTransform = GetComponent<NetworkTransform>(); // 같은 오브젝트에서 조회
            }

            if (fallLimitSet == null) // 낙하 한계 참조 확인
            {
                fallLimitSet = FindFirstObjectByType<CheckpointFallLimitSet>(); // 현재 Scene 설정 조회
            }
        }

        private bool EvaluateFallRespawn()
        {
            if (fallLimitSet == null) // 낙하 한계 설정 존재 확인
            {
                return false; // 자동 낙하 부활 생략
            }

            float fallLimitY = fallLimitSet.GetFallLimitY(CurrentCheckpointId); // 현재 구간 낙하 기준 조회

            if (transform.position.y >= fallLimitY) // 낙하 기준 통과 여부 확인
            {
                return false; // 정상 높이 유지
            }

            PerformRespawn(ProjectJNetworkRespawnReason.Fall); // 낙하 부활 실행
            return true; // 부활 발생 반환
        }

        private void PerformRespawn(ProjectJNetworkRespawnReason reason)
        {
            if (!Object.HasStateAuthority) // State Authority 확인
            {
                return; // Client 직접 부활 차단
            }

            ResolveReferences(); // 필수 참조 보정
            NetworkExternalVelocity = Vector3.zero; // 이전 외력 제거
            NetworkLastExternalForceSource = (int)ProjectJExternalForceSource.None; // 외력 원인 초기화

            if (networkPlayer != null) // Network Player 존재 확인
            {
                networkPlayer.ResetMotionForRespawn(); // 수직 속도와 Ground 상태 초기화
            }

            Quaternion respawnRotation = RespawnRotation; // 저장된 부활 회전 조회

            if (networkTransform != null) // NetworkTransform 존재 확인
            {
                networkTransform.Teleport(NetworkRespawnPosition, respawnRotation); // 모든 Peer에 순간이동 전파
            }
            else // NetworkTransform 누락 대비
            {
                transform.SetPositionAndRotation(NetworkRespawnPosition, respawnRotation); // 로컬 Transform 부활 처리
            }

            NetworkRespawnProtectionTimer = TickTimer.CreateFromSeconds(
                Runner,
                RespawnProtectionSeconds
            ); // 3초 보호 시작

            NetworkRespawnCount++; // 부활 횟수 증가
            NetworkLastRespawnReason = (int)reason; // 마지막 부활 원인 저장
            NetworkRaceHeight = TruncateHeight(NetworkRespawnPosition.y); // 부활 높이 즉시 저장

            Debug.Log(
                "[Project J/Fusion] 70일차 Respawn / P" +
                Object.InputAuthority.AsIndex +
                " / " +
                reason +
                " / " +
                NetworkRespawnPosition +
                " / Protection " +
                RespawnProtectionSeconds.ToString("F1") +
                "s"
            ); // 부활 확인 로그
        }

        private void UpdatePushForward(Vector2 moveInput)
        {
            if (moveInput.sqrMagnitude <= 0.0001f) // 이동 입력 존재 확인
            {
                return; // 마지막 방향 유지
            }

            Vector3 nextForward = new Vector3(moveInput.x, 0f, moveInput.y); // XZ 진행 방향 생성

            if (nextForward.sqrMagnitude <= 0.0001f) // 방향 유효성 확인
            {
                return; // 잘못된 방향 차단
            }

            NetworkPushForward = nextForward.normalized; // 마지막 이동 방향 저장
        }

        private void ProcessPush()
        {
            NetworkPushAttemptCount++; // 밀치기 시도 횟수 증가
            NetworkLastPushTargetIndex = -1; // 이전 대상 초기화

            if (!NetworkPushCooldown.ExpiredOrNotRunning(Runner)) // 쿨타임 확인
            {
                NetworkLastPushResult = (int)ProjectJNetworkPushResult.Cooldown; // 쿨타임 결과 저장
                return; // 밀치기 처리 종료
            }

            NetworkPushCooldown = TickTimer.CreateFromSeconds(Runner, PushCooldownSeconds); // 시도 즉시 쿨타임 시작

            ProjectJNetworkExternalGameplay target = FindClosestPushTarget(); // 가장 가까운 Target 검색

            if (target == null) // Target 존재 확인
            {
                NetworkLastPushResult = (int)ProjectJNetworkPushResult.Miss; // 빗나감 결과 저장
                return; // 밀치기 처리 종료
            }

            NetworkLastPushTargetIndex = target.Object.InputAuthority.AsIndex; // 찾은 Target 저장

            if (target.IsRespawnProtected) // Target 부활 보호 확인
            {
                NetworkLastPushResult = (int)ProjectJNetworkPushResult.Protected; // 보호 차단 결과 저장
                return; // 외력 적용 차단
            }

            Vector3 pushDirection = target.transform.position - transform.position; // 실행자에서 Target 방향 계산
            pushDirection.y = 0f; // 수평 방향 제한

            if (pushDirection.sqrMagnitude <= 0.0001f) // 위치 중첩 확인
            {
                pushDirection = NetworkPushForward; // 마지막 진행 방향 사용
            }

            if (pushDirection.sqrMagnitude <= 0.0001f) // 최종 방향 확인
            {
                pushDirection = Vector3.forward; // 기본 전방 사용
            }

            bool applied = target.TryApplyExternalVelocityChange(
                ProjectJExternalForceSource.Push,
                pushDirection.normalized * PushForce
            ); // Target 외력 적용 요청

            if (!applied) // 외력 적용 실패 확인
            {
                NetworkLastPushResult = target.IsRespawnProtected
                    ? (int)ProjectJNetworkPushResult.Protected
                    : (int)ProjectJNetworkPushResult.Invalid; // 실패 원인 저장
                return; // 밀치기 처리 종료
            }

            NetworkLastPushResult = (int)ProjectJNetworkPushResult.Success; // 성공 결과 저장
            NetworkPushSuccessCount++; // 성공 횟수 증가

            Debug.Log(
                "[Project J/Fusion] 70일차 Push 성공 / P" +
                Object.InputAuthority.AsIndex +
                " -> P" +
                NetworkLastPushTargetIndex +
                " / Force " +
                PushForce
            ); // 밀치기 확인 로그
        }

        private ProjectJNetworkExternalGameplay FindClosestPushTarget()
        {
            ProjectJNetworkExternalGameplay closestTarget = null; // 최근접 Target 초기화
            float closestDistanceSquared = float.PositiveInfinity; // 최근접 거리 초기화
            Vector3 forward = NetworkPushForward.sqrMagnitude > 0.0001f
                ? NetworkPushForward.normalized
                : Vector3.forward; // 밀치기 기준 방향 선택

            foreach (ProjectJNetworkExternalGameplay candidate in ActivePlayers) // 전체 활성 Player 순회
            {
                if (candidate == null) // 삭제된 Player 확인
                {
                    continue; // 다음 후보 이동
                }

                if (candidate == this) // 자기 자신 확인
                {
                    continue; // 자기 자신 제외
                }

                if (
                    candidate.Object == null || // NetworkObject 존재 확인
                    !candidate.Object.IsValid || // NetworkObject 유효 확인
                    !candidate.Object.HasStateAuthority // 같은 Host 권한 확인
                )
                {
                    continue; // 잘못된 후보 제외
                }

                Vector3 toTarget = candidate.transform.position - transform.position; // Target 방향 계산
                float distanceSquared = toTarget.sqrMagnitude; // Target 거리 제곱 계산

                if (distanceSquared > PushSearchRange * PushSearchRange) // 최대 거리 확인
                {
                    continue; // 범위 밖 후보 제외
                }

                Vector3 horizontalDirection = new Vector3(toTarget.x, 0f, toTarget.z); // 수평 Target 방향 계산

                if (horizontalDirection.sqrMagnitude > 0.0001f) // 방향 계산 가능 확인
                {
                    float angle = Vector3.Angle(forward, horizontalDirection.normalized); // 전방 각도 계산

                    if (angle > PushSearchHalfAngle) // 90도 전방 범위 확인
                    {
                        continue; // 전방 범위 밖 후보 제외
                    }
                }

                if (distanceSquared >= closestDistanceSquared) // 최근접 여부 확인
                {
                    continue; // 더 먼 후보 제외
                }

                closestTarget = candidate; // 최근접 Target 갱신
                closestDistanceSquared = distanceSquared; // 최근접 거리 갱신
            }

            return closestTarget; // 최종 Target 반환
        }

        private void SimulateExternalVelocity()
        {
            Vector3 externalVelocity = NetworkExternalVelocity; // 현재 외부 속도 읽기

            if (externalVelocity.sqrMagnitude <= 0.0001f) // 외력 존재 확인
            {
                NetworkExternalVelocity = Vector3.zero; // 미세 외력 정리
                return; // 이동 처리 종료
            }

            transform.position += externalVelocity * Runner.DeltaTime; // 외부 속도 위치 반영
            externalVelocity = Vector3.MoveTowards(
                externalVelocity,
                Vector3.zero,
                ExternalVelocityDecayPerSecond * Runner.DeltaTime
            ); // Tick 기반 외력 감속

            if (externalVelocity.magnitude <= ExternalVelocityStopThreshold) // 정지 임계값 확인
            {
                externalVelocity = Vector3.zero; // 외력 완전 정지
            }

            NetworkExternalVelocity = externalVelocity; // 감속 결과 동기화
        }

        private void UpdateRaceHeight()
        {
            NetworkRaceHeight = TruncateHeight(transform.position.y); // 발 기준 World Y를 0.00 단위로 저장
        }

        private void UpdateRaceRank()
        {
            int rank = 1; // 기본 1위로 시작

            foreach (ProjectJNetworkExternalGameplay candidate in ActivePlayers) // 전체 활성 Player 순회
            {
                if (
                    candidate == null || // 삭제된 Player 확인
                    candidate == this || // 자기 자신 제외
                    candidate.Object == null || // NetworkObject 존재 확인
                    !candidate.Object.IsValid // NetworkObject 유효 확인
                )
                {
                    continue; // 순위 후보 제외
                }

                if (candidate.NetworkRaceHeight > NetworkRaceHeight) // 자신보다 높은 Player 확인
                {
                    rank++; // 높은 Player 수만큼 순위 증가
                }
            }

            NetworkRaceRank = rank; // 경쟁 순위 저장
        }

        private static float TruncateHeight(float worldY)
        {
            int scaledHeight = (int)(worldY * 100f); // 셋째 자리 이하를 0 방향으로 버림
            return scaledHeight / 100f; // 소수점 둘째 자리 값 반환
        }

        private static bool IsHostileExternalForce(ProjectJExternalForceSource source)
        {
            return
                source == ProjectJExternalForceSource.Push || // 플레이어 밀치기 차단 대상
                source == ProjectJExternalForceSource.Item; // 적대 아이템 차단 대상
        }

        private void OnGUI()
        {
            if (!Application.isEditor && !Debug.isDebugBuild) // 개발 환경 확인
            {
                return; // Release 디버그 표시 차단
            }

            if (
                Object == null || // NetworkObject 존재 확인
                !Object.IsValid || // NetworkObject 유효 확인
                !Object.HasInputAuthority // 자신의 Player 확인
            )
            {
                return; // 원격 Player 표시 차단
            }

            string protectionText = IsRespawnProtected
                ? RespawnProtectionRemaining.ToString("F2") + "s"
                : "OFF"; // 보호 상태 문자열 생성

            string debugText =
                "DAY 70 NETWORK\n" +
                "Height: " + RaceHeight.ToString("F2") + " / Rank: " + RaceRank + "\n" +
                "Respawn: " + LastRespawnReason + " x" + RespawnCount + " / Protection: " + protectionText + "\n" +
                "External: " + ExternalVelocity.ToString("F2") + " / " + LastExternalForceSource + "\n" +
                "Push: " + LastPushResult + " / Target P" + LastPushTargetIndex + " / CD " + PushCooldownRemaining.ToString("F2") + "\n" +
                "Checkpoint: " + CurrentCheckpointId + " / Respawn " + RespawnPosition.ToString("F2") + "\n" +
                "R: Manual Respawn"; // 70일차 상태 문자열 생성

            GUI.Box(
                new Rect(
                    20f,
                    Screen.height - 185f,
                    580f,
                    165f
                ),
                debugText
            ); // 로컬 디버그 박스 표시
        }
    }
}
