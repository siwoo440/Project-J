using System.Collections.Generic; // 활성 플레이어 Registry 사용
using Fusion; // NetworkBehaviour와 Networked 상태 사용
using ProjectJ.Checkpoint; // 체크포인트 공통 계약 사용
using UnityEngine; // Vector3와 GUI 사용

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent] // 동일 네트워크 외력 컴포넌트 중복 방지
    [RequireComponent(typeof(ProjectJNetworkPlayer))] // 기존 Network Player 보장
    public sealed class ProjectJNetworkExternalGameplay : // 69일차 외력·밀치기·체크포인트 네트워크 처리
        NetworkBehaviour,
        ICheckpointReceiver
    {
        private const float ExternalVelocityDecayPerSecond = 12f; // 외력 초당 감속량
        private const float ExternalVelocityStopThreshold = 0.05f; // 외력 정지 임계값
        private const float PushSearchRange = 2.5f; // 밀치기 최대 거리
        private const float PushSearchHalfAngle = 45f; // 90도 전방 범위의 절반 각도
        private const float PushForce = 12f; // 기본 밀치기 외력
        private const float PushCooldownSeconds = 1.5f; // 밀치기 재사용 대기시간

        private static readonly HashSet<ProjectJNetworkExternalGameplay> ActivePlayers = // 현재 프로세스 Player Registry
            new HashSet<ProjectJNetworkExternalGameplay>();

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

        public Vector3 ExternalVelocity => // 현재 외부 속도 조회
            NetworkExternalVelocity;

        public ProjectJExternalForceSource LastExternalForceSource => // 마지막 외력 원인 조회
            (ProjectJExternalForceSource)NetworkLastExternalForceSource;

        public int ExternalForceApplyCount => // 외력 적용 횟수 조회
            NetworkExternalForceApplyCount;

        public ProjectJNetworkPushResult LastPushResult => // 마지막 밀치기 결과 조회
            (ProjectJNetworkPushResult)NetworkLastPushResult;

        public int LastPushTargetIndex => // 마지막 밀치기 대상 조회
            NetworkLastPushTargetIndex;

        public int PushAttemptCount => // 밀치기 시도 횟수 조회
            NetworkPushAttemptCount;

        public int PushSuccessCount => // 밀치기 성공 횟수 조회
            NetworkPushSuccessCount;

        public CheckpointId CurrentCheckpointId => // 현재 최고 체크포인트 조회
            (CheckpointId)NetworkCheckpointId;

        public Vector3 RespawnPosition => // 현재 부활 위치 조회
            NetworkRespawnPosition;

        public Quaternion RespawnRotation => // 현재 부활 회전 조회
            Quaternion.Euler(
                NetworkRespawnEulerAngles
            );

        public int CheckpointActivationCount => // 체크포인트 활성화 횟수 조회
            NetworkCheckpointActivationCount;

        public float PushCooldownRemaining // 남은 밀치기 쿨타임 조회
        {
            get
            {
                if (Runner == null) // Runner 존재 확인
                {
                    return 0f; // Runner 없음 처리
                }

                float? remaining = // Fusion TickTimer 남은 시간 조회
                    NetworkPushCooldown.RemainingTime(
                        Runner
                    );

                if (!remaining.HasValue) // 실행 중인 Timer 확인
                {
                    return 0f; // Timer 없음 처리
                }

                return Mathf.Max( // 음수 시간 방지
                    0f,
                    remaining.Value
                );
            }
        }

        public override void Spawned() // Network Object 생성 완료 처리
        {
            ActivePlayers.Add( // Player Registry 등록
                this
            );

            if (!Object.HasStateAuthority) // State Authority 확인
            {
                return; // 초기 상태 쓰기 차단
            }

            NetworkExternalVelocity = Vector3.zero; // 외부 속도 초기화
            NetworkLastExternalForceSource = (int)ProjectJExternalForceSource.None; // 외력 원인 초기화
            NetworkExternalForceApplyCount = 0; // 외력 적용 횟수 초기화
            NetworkPushForward = Vector3.forward; // 기본 밀치기 방향 초기화
            NetworkPushCooldown = TickTimer.None; // 밀치기 쿨타임 초기화
            NetworkLastPushResult = (int)ProjectJNetworkPushResult.None; // 밀치기 결과 초기화
            NetworkLastPushTargetIndex = -1; // 밀치기 대상 초기화
            NetworkPushAttemptCount = 0; // 밀치기 시도 횟수 초기화
            NetworkPushSuccessCount = 0; // 밀치기 성공 횟수 초기화
            NetworkCheckpointId = (int)CheckpointId.Start; // 체크포인트 초기화
            NetworkRespawnPosition = transform.position; // 시작 부활 위치 저장
            NetworkRespawnEulerAngles = transform.rotation.eulerAngles; // 시작 부활 회전 저장
            NetworkCheckpointActivationCount = 0; // 체크포인트 횟수 초기화
        }

        public override void Despawned( // Network Object 제거 처리
            NetworkRunner runner,
            bool hasState
        )
        {
            ActivePlayers.Remove( // Player Registry 제거
                this
            );
        }

        public override void FixedUpdateNetwork() // Fusion Tick 기반 69일차 Simulation
        {
            if (!Object.HasStateAuthority) // Host State Authority 확인
            {
                return; // Client 직접 판정 차단
            }

            if (GetInput<ProjectJNetworkInput>(out ProjectJNetworkInput input)) // Player 입력 수신 확인
            {
                UpdatePushForward( // 마지막 이동 방향 갱신
                    input.Move
                );

                if (input.Buttons.IsSet(ProjectJNetworkButton.Push)) // 밀치기 단발 입력 확인
                {
                    ProcessPush(); // State Authority 밀치기 판정
                }
            }

            SimulateExternalVelocity(); // 외부 속도 이동과 감속 처리
        }

        public bool TryApplyExternalVelocityChange( // 공통 외력 적용 API
            ProjectJExternalForceSource source,
            Vector3 velocityChange
        )
        {
            if (!Object.HasStateAuthority) // State Authority 확인
            {
                return false; // Client 외력 쓰기 차단
            }

            velocityChange.y = 0f; // 69일차 수평 외력만 허용

            if (velocityChange.sqrMagnitude <= 0.0001f) // 유효 외력 확인
            {
                return false; // 너무 작은 외력 거부
            }

            NetworkExternalVelocity += velocityChange; // 기존 외력과 새 외력 합산
            NetworkLastExternalForceSource = (int)source; // 마지막 외력 원인 저장
            NetworkExternalForceApplyCount++; // 외력 적용 횟수 증가

            return true; // 외력 적용 성공
        }

        public void ReceiveCheckpoint( // Runtime Checkpoint 접촉 수신
            global::ProjectJ.Checkpoint.Checkpoint checkpoint
        )
        {
            if (!Object.HasStateAuthority) // State Authority 확인
            {
                return; // Client 체크포인트 쓰기 차단
            }

            if (checkpoint == null) // 체크포인트 참조 확인
            {
                return; // 잘못된 참조 차단
            }

            int nextCheckpointId = // 접촉 체크포인트 값 변환
                (int)checkpoint.Id;

            if (nextCheckpointId <= NetworkCheckpointId) // 최고값 갱신 여부 확인
            {
                return; // 같은 값 또는 낮은 값 무시
            }

            NetworkCheckpointId = nextCheckpointId; // 최고 체크포인트 저장
            NetworkRespawnPosition = checkpoint.RespawnPosition; // 부활 위치 저장
            NetworkRespawnEulerAngles = checkpoint.RespawnRotation.eulerAngles; // 부활 회전 저장
            NetworkCheckpointActivationCount++; // 활성화 횟수 증가

            Debug.Log( // 체크포인트 네트워크 확인 로그
                "[Project J/Fusion] 69일차 Checkpoint 저장 / P" +
                Object.InputAuthority.AsIndex +
                " / " +
                checkpoint.Id +
                " / Respawn " +
                NetworkRespawnPosition
            );
        }

        private void UpdatePushForward( // 이동 입력에서 밀치기 방향 갱신
            Vector2 moveInput
        )
        {
            if (moveInput.sqrMagnitude <= 0.0001f) // 이동 입력 존재 확인
            {
                return; // 마지막 방향 유지
            }

            Vector3 nextForward = // XZ 이동 방향 생성
                new Vector3(
                    moveInput.x,
                    0f,
                    moveInput.y
                );

            if (nextForward.sqrMagnitude <= 0.0001f) // 방향 유효성 확인
            {
                return; // 잘못된 방향 차단
            }

            NetworkPushForward = nextForward.normalized; // 마지막 이동 방향 저장
        }

        private void ProcessPush() // State Authority 밀치기 처리
        {
            NetworkPushAttemptCount++; // 밀치기 시도 횟수 증가
            NetworkLastPushTargetIndex = -1; // 이전 대상 초기화

            if (!NetworkPushCooldown.ExpiredOrNotRunning(Runner)) // 쿨타임 확인
            {
                NetworkLastPushResult = (int)ProjectJNetworkPushResult.Cooldown; // 쿨타임 결과 저장
                return; // 밀치기 처리 중단
            }

            NetworkPushCooldown = TickTimer.CreateFromSeconds( // 시도 즉시 쿨타임 시작
                Runner,
                PushCooldownSeconds
            );

            ProjectJNetworkExternalGameplay target = // 가장 가까운 Target 검색
                FindClosestPushTarget();

            if (target == null) // Target 존재 확인
            {
                NetworkLastPushResult = (int)ProjectJNetworkPushResult.Miss; // 빗나감 결과 저장
                return; // 밀치기 처리 종료
            }

            Vector3 pushDirection = // 실행자에서 Target 방향 계산
                target.transform.position -
                transform.position;

            pushDirection.y = 0f; // 수평 방향 제한

            if (pushDirection.sqrMagnitude <= 0.0001f) // 겹친 위치 확인
            {
                pushDirection = NetworkPushForward; // 마지막 진행 방향 사용
            }

            if (pushDirection.sqrMagnitude <= 0.0001f) // 최종 방향 유효성 확인
            {
                pushDirection = Vector3.forward; // 기본 전방 방향 사용
            }

            bool applied = // Target 외력 적용 요청
                target.TryApplyExternalVelocityChange(
                    ProjectJExternalForceSource.Push,
                    pushDirection.normalized * PushForce
                );

            if (!applied) // 외력 적용 실패 확인
            {
                NetworkLastPushResult = (int)ProjectJNetworkPushResult.Invalid; // 실패 결과 저장
                return; // 밀치기 처리 종료
            }

            NetworkLastPushTargetIndex = target.Object.InputAuthority.AsIndex; // Target PlayerRef 저장
            NetworkLastPushResult = (int)ProjectJNetworkPushResult.Success; // 성공 결과 저장
            NetworkPushSuccessCount++; // 성공 횟수 증가

            Debug.Log( // 밀치기 네트워크 확인 로그
                "[Project J/Fusion] 69일차 Push 성공 / P" +
                Object.InputAuthority.AsIndex +
                " -> P" +
                NetworkLastPushTargetIndex +
                " / Force " +
                PushForce
            );
        }

        private ProjectJNetworkExternalGameplay FindClosestPushTarget() // 전방 최근접 Target 탐색
        {
            ProjectJNetworkExternalGameplay closestTarget = null; // 최근접 Target 초기화
            float closestDistanceSquared = float.PositiveInfinity; // 최근접 거리 초기화

            Vector3 forward = // 밀치기 기준 방향 선택
                NetworkPushForward.sqrMagnitude > 0.0001f
                    ? NetworkPushForward.normalized
                    : Vector3.forward;

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
                    !candidate.Object.IsValid || // NetworkObject 유효성 확인
                    !candidate.Object.HasStateAuthority // 같은 Host 권한 대상 확인
                )
                {
                    continue; // 잘못된 후보 제외
                }

                Vector3 toTarget = // Target 방향 계산
                    candidate.transform.position -
                    transform.position;

                float distanceSquared = // Target 거리 제곱 계산
                    toTarget.sqrMagnitude;

                if (distanceSquared > PushSearchRange * PushSearchRange) // 최대 거리 확인
                {
                    continue; // 범위 밖 후보 제외
                }

                Vector3 horizontalDirection = // 수평 Target 방향 계산
                    new Vector3(
                        toTarget.x,
                        0f,
                        toTarget.z
                    );

                if (horizontalDirection.sqrMagnitude > 0.0001f) // 방향 계산 가능 확인
                {
                    float angle = // 전방 각도 계산
                        Vector3.Angle(
                            forward,
                            horizontalDirection.normalized
                        );

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

        private void SimulateExternalVelocity() // 외부 속도 이동 처리
        {
            Vector3 externalVelocity = // 현재 외부 속도 읽기
                NetworkExternalVelocity;

            if (externalVelocity.sqrMagnitude <= 0.0001f) // 외력 존재 확인
            {
                NetworkExternalVelocity = Vector3.zero; // 미세 외력 정리
                return; // 이동 처리 종료
            }

            transform.position += // 외부 속도 위치 반영
                externalVelocity *
                Runner.DeltaTime;

            externalVelocity = Vector3.MoveTowards( // Tick 기반 외력 감속
                externalVelocity,
                Vector3.zero,
                ExternalVelocityDecayPerSecond * Runner.DeltaTime
            );

            if (externalVelocity.magnitude <= ExternalVelocityStopThreshold) // 정지 임계값 확인
            {
                externalVelocity = Vector3.zero; // 외력 완전 정지
            }

            NetworkExternalVelocity = externalVelocity; // 감속 결과 동기화
        }

        private void OnGUI() // 69일차 로컬 디버그 표시
        {
            if (!Application.isEditor && !Debug.isDebugBuild) // 개발 환경 확인
            {
                return; // Release 디버그 표시 차단
            }

            if (
                Object == null || // NetworkObject 존재 확인
                !Object.IsValid || // NetworkObject 유효성 확인
                !Object.HasInputAuthority // 자신의 Player 확인
            )
            {
                return; // 원격 Player 디버그 표시 차단
            }

            string debugText = // 69일차 상태 문자열 생성
                "DAY 69 NETWORK\n" +
                "External: " + ExternalVelocity.ToString("F2") + " / " + LastExternalForceSource + "\n" +
                "Push: " + LastPushResult + " / Target P" + LastPushTargetIndex + " / CD " + PushCooldownRemaining.ToString("F2") + "\n" +
                "Checkpoint: " + CurrentCheckpointId + " / Respawn " + RespawnPosition.ToString("F2");

            GUI.Box( // 디버그 배경 표시
                new Rect(
                    20f,
                    Screen.height - 125f,
                    540f,
                    105f
                ),
                debugText
            );
        }
    }
}
