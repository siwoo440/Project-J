using Fusion;
using ProjectJ.Items; // 깃털 신발 수치 정책 사용
using UnityEngine;

namespace ProjectJ.Networking.Fusion
{
    [DisallowMultipleComponent]
    public sealed class ProjectJNetworkPlayer :
        NetworkBehaviour,
        IBeforeAllTicks,
        IAfterAllTicks
    {
        private const float InputPulseDuration = 0.35f;
        private const float BaseMoveSpeed = 5f;
        private const float JumpVelocity = 7f;
        private const float Gravity = -20f;
        private const float GroundProbeStartHeight = 0.15f;
        private const float GroundProbeDistance = 0.25f;
        private const float SprintMoveSpeed = 8f;
        private const float MaxStamina = 100f;
        private const float SprintStaminaDrainPerSecond = 25f;
        private const float StaminaRecoveryPerSecond = 20f;
        private const float SprintRestartStamina = 20f;
        private const float StandingColliderHeight = 2f;
        private const float CrouchColliderHeight = 1f;
        private const float BodyColliderRadius = 0.4f;
        private const float StandingVisualY = 1f;
        private const float CrouchVisualY = 0.5f;
        private const float StandingVisualScaleY = 1f;
        private const float CrouchVisualScaleY = 0.5f;
        private const float StandClearanceRadiusScale = 0.95f;

        private Renderer visualRenderer;
        private Transform visualTransform;
        private Camera authorityCamera;
        private NetworkTransform networkTransform;
        private CapsuleCollider bodyCollider;
        private ProjectJNetworkExternalGameplay externalGameplay; // 경기 상태 입력 잠금 조회
        private ProjectJNetworkItemInventory itemInventory; // 이동 아이템 효과 상태 조회

        private readonly RaycastHit[] groundHitBuffer = new RaycastHit[16];
        private readonly RaycastHit[] jetpackCeilingHitBuffer = new RaycastHit[16]; // 제트팩 천장 충돌 후보 버퍼
        private readonly Collider[] standOverlapBuffer = new Collider[16];

        private Material runtimeMaterial;
        private float inputPulseUntil;
        private bool hasForwardPosition;
        private Vector3 lastForwardPosition;
        private Vector3 predictedPositionBeforeResimulation;
        private bool hasRenderPosition;
        private Vector3 previousRenderPosition;

        [Networked]
        private float NetworkVerticalVelocity
        {
            get;
            set;
        }

        [Networked]
        private NetworkBool NetworkGrounded
        {
            get;
            set;
        }

        [Networked]
        private float NetworkStamina
        {
            get;
            set;
        }

        [Networked]
        private NetworkBool NetworkIsSprinting
        {
            get;
            set;
        }

        [Networked]
        private NetworkBool NetworkSprintExhausted
        {
            get;
            set;
        }

        [Networked]
        private NetworkBool NetworkIsCrouching
        {
            get;
            set;
        }

        public PlayerRef Owner =>
            Object != null && Object.IsValid
                ? Object.InputAuthority
                : default;

        public bool HasLocalStateAuthority =>
            Object != null && Object.IsValid && Object.HasStateAuthority;

        public bool HasLocalInputAuthority =>
            Object != null && Object.IsValid && Object.HasInputAuthority;

        public bool IsRemoteView =>
            Object != null && Object.IsValid && !Object.HasInputAuthority;

        public bool IsRemoteProxy =>
            Object != null && Object.IsValid && !Object.HasInputAuthority && !Object.HasStateAuthority;

        public bool HasNetworkTransform =>
            networkTransform != null;

        public bool NetworkTransformHasPhysicsBody => // NetworkTransform Physics Body 적용 여부
            networkTransform != null && // NetworkTransform 존재 확인
            networkTransform.HasPhysicsBody; // 실제 Physics Body 상태 반환

        public bool NetworkTransformHasForecastEnabled => // 실제 Forecast Physics 활성 여부
            networkTransform != null && // NetworkTransform 존재 확인
            networkTransform.HasForecastEnabled; // 전역·로컬 Forecast 적용 결과 반환

        public bool ForceRemoteRenderTimeframe => // Remote Timeframe 강제 여부
            Object != null && // NetworkObject 존재 확인
            Object.IsValid && // NetworkObject 유효성 확인
            Object.ForceRemoteRenderTimeframe; // 강제 Remote 설정 반환

        public bool UsesRemoteRenderTimeframe => // 실제 Remote Timeframe 사용 여부
            Object != null && // NetworkObject 존재 확인
            Object.IsValid && // NetworkObject 유효성 확인
            Object.RenderTimeframe == RenderTimeframe.Remote; // 실제 Render Timeframe 비교

        public string RenderTimeframeLabel => // 현재 Render Timeframe 표시 문자열
            Object != null && Object.IsValid // NetworkObject 유효성 확인
                ? Object.RenderTimeframe.ToString().ToUpperInvariant() // 실제 Timeframe 이름 반환
                : "INVALID"; // 유효하지 않은 객체 상태 반환

        public bool RemoteInterpolationExpected =>
            IsRemoteView && HasNetworkTransform;

        public bool AuthorityCameraEnabled =>
            authorityCamera != null && authorityCamera.enabled;

        public bool HasReceivedInput
        {
            get;
            private set;
        }

        public Vector2 LastReceivedMove
        {
            get;
            private set;
        }

        public bool LastReceivedJump
        {
            get;
            private set;
        }

        public bool LastReceivedSprint
        {
            get;
            private set;
        }

        public bool LastReceivedCrouch
        {
            get;
            private set;
        }

        public string LastReceivedTick
        {
            get;
            private set;
        } = "-";

        public Vector3 CurrentPosition => transform.position;

        public Vector3 LastSimulationPosition
        {
            get;
            private set;
        }

        public Vector3 LastRenderPosition
        {
            get;
            private set;
        }

        public float RenderSimulationOffset
        {
            get;
            private set;
        }

        public float LastRenderStepDistance
        {
            get;
            private set;
        }

        public int RenderSampleCount
        {
            get;
            private set;
        }

        public float MovementSpeed => CurrentMoveSpeed;
        public float WalkSpeed => BaseMoveSpeed;
        public float SprintSpeed => SprintMoveSpeed;
        public bool IsFeatherShoesActive =>
            itemInventory != null && itemInventory.IsFeatherShoesActive; // 깃털 신발 활성 여부

        public bool IsJetpackActive =>
            itemInventory != null && itemInventory.IsJetpackActive; // 제트팩 Networked 연료 활성 여부

        public bool IsSnowballSlowed =>
            itemInventory != null && itemInventory.IsSnowballSlowed; // 눈덩이 감속 활성 여부

        public float CurrentMoveSpeed
        {
            get
            {
                float baseSpeed = NetworkIsSprinting
                    ? SprintMoveSpeed
                    : BaseMoveSpeed; // 달리기 상태의 기본 속도 선택

                float featherShoesSpeed = ProjectJFeatherShoesPolicy.CalculateMovementSpeed(
                    baseSpeed,
                    IsFeatherShoesActive
                ); // 깃털 신발 속도 배율 적용

                float snowballSpeed = ProjectJSnowballPolicy.CalculateMovementSpeed( // 눈덩이 적용 후 속도 계산
                    featherShoesSpeed,
                    IsSnowballSlowed
                ); // 눈덩이 감속 배율 적용

                return ProjectJJetpackPolicy.CalculateHorizontalMovementSpeed( // 제트팩 수평 조정 배율 적용
                    snowballSpeed, // 기존 이동·아이템 보정 속도 유지
                    IsJetpackActive // 제트팩 활성 상태 전달
                );
            }
        }

        public float CurrentSprintStaminaDrainPerSecond =>
            ProjectJFeatherShoesPolicy.CalculateSprintStaminaDrain(
                SprintStaminaDrainPerSecond,
                IsFeatherShoesActive
            ); // 깃털 신발 추가 소모 적용

        public float Stamina => NetworkStamina;
        public float StaminaMaximum => MaxStamina;
        public bool IsSprinting => NetworkIsSprinting;
        public bool IsSprintExhausted => NetworkSprintExhausted;
        public bool IsCrouching => NetworkIsCrouching;

        public float ColliderHeight =>
            bodyCollider != null
                ? bodyCollider.height
                : (NetworkIsCrouching ? CrouchColliderHeight : StandingColliderHeight);

        public bool CanStandUp =>
            !NetworkIsCrouching || HasStandingClearance();

        public float VerticalVelocity => NetworkVerticalVelocity;
        public bool IsGrounded => NetworkGrounded;
        public float JumpSpeed => JumpVelocity;
        public float GravityAcceleration => Gravity;
        public bool InputSeenRecently => Time.unscaledTime < inputPulseUntil;

        public int ResimulationBatchCount
        {
            get;
            private set;
        }

        public int ResimulationTickCount
        {
            get;
            private set;
        }

        public int LastResimulationTickCount
        {
            get;
            private set;
        }

        public int LastForwardTickCount
        {
            get;
            private set;
        }

        public float LastRollbackDistance
        {
            get;
            private set;
        }

        public float LastCorrectionDistance
        {
            get;
            private set;
        }

        public float MaxCorrectionDistance
        {
            get;
            private set;
        }

        public Vector3 PredictionPositionBeforeResimulation
        {
            get;
            private set;
        }

        public Vector3 RollbackPosition
        {
            get;
            private set;
        }

        public Vector3 CorrectedPositionAfterResimulation
        {
            get;
            private set;
        }

        public override void Spawned()
        {
            CachePresentation();
            ApplyAuthorityPresentation();

            lastForwardPosition = transform.position;
            hasForwardPosition = true;
            LastSimulationPosition = transform.position;
            LastRenderPosition = transform.position;
            previousRenderPosition = transform.position;
            hasRenderPosition = true;

            if (Object.HasStateAuthority)
            {
                NetworkStamina = MaxStamina;
                NetworkIsSprinting = false;
                NetworkSprintExhausted = false;
                NetworkIsCrouching = false;
            }

            ApplyColliderPosture();
            ApplyCrouchPresentation();

            if (Object.HasInputAuthority)
            {
                ProjectJLocalPlayerPresentationController.BindLocalPlayer(this);
            }

            Debug.Log(
                "[Project J/Fusion] Network Player 연결 / Owner: " +
                Object.InputAuthority.AsIndex +
                " / State Authority: " + Object.HasStateAuthority +
                " / Input Authority: " + Object.HasInputAuthority +
                " / NetworkTransform: " + HasNetworkTransform
            );
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            ProjectJLocalPlayerPresentationController.UnbindLocalPlayer(this);
        }

        public override void FixedUpdateNetwork()
        {
            LastSimulationPosition = transform.position;

            ProjectJNetworkInput input =
                default;

            bool hasInput =
                GetInput<ProjectJNetworkInput>(
                    out input
                );

            Vector2 moveInput =
                Vector2.zero;

            if (hasInput)
            {
                HasReceivedInput =
                    true;

                moveInput =
                    input.Move;

                if (moveInput.sqrMagnitude > 1f)
                {
                    moveInput.Normalize();
                }

                LastReceivedMove =
                    moveInput;

                LastReceivedJump =
                    input.Buttons.IsSet(
                        ProjectJNetworkButton.Jump
                    );

                LastReceivedSprint =
                    input.Buttons.IsSet(
                        ProjectJNetworkButton.Sprint
                    );

                LastReceivedCrouch =
                    input.Buttons.IsSet(
                        ProjectJNetworkButton.Crouch
                    );

                LastReceivedTick =
                    Runner.Tick.ToString();
            }
            else
            {
                LastReceivedMove =
                    Vector2.zero;

                LastReceivedJump =
                    false;

                LastReceivedSprint =
                    false;

                LastReceivedCrouch =
                    false;
            }

            if (
                externalGameplay != null && // 경기 상태 컴포넌트 확인
                !externalGameplay.GameplayInputAllowed // 경기 조작 허용 여부 확인
            )
            {
                NetworkVerticalVelocity = 0f; // 잠금 중 수직 이동 정지
                NetworkIsSprinting = false; // 잠금 중 달리기 정지
                LastSimulationPosition = transform.position; // 현재 위치를 Simulation 기준으로 유지
                return; // 이동·점프·달리기·앉기 입력 처리 차단
            }
            if (
            ProjectJSoapBubblePolicy.ShouldRestrictLocomotion(
            itemInventory != null &&
            itemInventory.IsSoapBubbleActive
                    )
                )
            {
                moveInput = Vector2.zero;
                LastReceivedSprint = false;
                LastReceivedCrouch = false;
            }
            float deltaTime = Runner.DeltaTime;
            bool hasMoveInput = moveInput.sqrMagnitude > 0.0001f;

            UpdateCrouchState();
            ApplyColliderPosture();
            UpdateSprintState(hasMoveInput, deltaTime);

            bool groundedBeforeMove = TryGetGroundHeight(
                transform.position,
                GroundProbeDistance,
                out float groundHeight
            );

            if (groundedBeforeMove && NetworkVerticalVelocity <= 0f)
            {
                Vector3 groundedPosition = transform.position;
                groundedPosition.y = groundHeight;
                transform.position = groundedPosition;
                NetworkVerticalVelocity = 0f;
                NetworkGrounded = true;
            }
            else
            {
                NetworkGrounded = false;
            }

            if (LastReceivedJump && NetworkGrounded && !NetworkIsCrouching)
            {
                NetworkVerticalVelocity = JumpVelocity;
                NetworkGrounded = false;
            }

            if (!NetworkGrounded)
            {
                NetworkVerticalVelocity += Gravity * deltaTime;
            }

            bool jetpackActive = IsJetpackActive; // Networked 제트팩 연료 상태 조회
            bool jetpackMovementAllowed = ProjectJJetpackPolicy.CanApplyMovement( // 제트팩 이동 허용 상태 계산
                jetpackActive, // Networked 활성 상태 전달
                true // 기존 Gameplay Lock 검사 통과 상태
            );
            bool jetpackCeilingBlocked = false; // 기본 천장 차단 상태

            if (jetpackMovementAllowed)
            {
                float candidateUpwardVelocity = Mathf.Max( // 이번 Tick 예상 상승 속도 계산
                    NetworkVerticalVelocity, // 기존 중력 반영 수직 속도
                    ProjectJJetpackPolicy.PrototypeAscentSpeedMetersPerSecond // 최소 제트팩 상승 속도
                );
                float upwardProbeDistance = // 이번 Tick 예상 상승 거리 계산
                    Mathf.Max(0f, candidateUpwardVelocity) * deltaTime; // 위쪽 이동 거리만 사용

                jetpackCeilingBlocked = IsJetpackCeilingBlocked( // 천장 충돌 여부 검사
                    upwardProbeDistance // 예상 상승 거리 전달
                );
            }

            NetworkVerticalVelocity = ProjectJJetpackPolicy.ResolveVerticalVelocity( // 제트팩 수직 속도 반영
                NetworkVerticalVelocity, // 기존 중력 계산 결과 전달
                jetpackActive, // Networked 활성 상태 전달
                true, // 기존 Gameplay Lock 검사 통과 상태
                jetpackCeilingBlocked // 천장 차단 상태 전달
            );

            if (jetpackActive && NetworkVerticalVelocity > 0f)
            {
                NetworkGrounded = false; // 제트팩 상승 중 공중 상태 유지
            }

            float horizontalMoveSpeed = CurrentMoveSpeed;
            Vector3 currentPosition = transform.position;
            Vector3 nextPosition = currentPosition;

            nextPosition.x += moveInput.x * horizontalMoveSpeed * deltaTime;
            nextPosition.z += moveInput.y * horizontalMoveSpeed * deltaTime;
            nextPosition.y += NetworkVerticalVelocity * deltaTime;

            if (
                NetworkVerticalVelocity <= 0f &&
                TryGetLandingGroundHeight(currentPosition, nextPosition, out float landingHeight)
            )
            {
                nextPosition.y = landingHeight;
                NetworkVerticalVelocity = 0f;
                NetworkGrounded = true;
            }

            transform.position = nextPosition;
            LastSimulationPosition = transform.position;

            bool hasActivity =
                hasMoveInput ||
                LastReceivedJump ||
                LastReceivedSprint ||
                LastReceivedCrouch;

            if (hasActivity)
            {
                inputPulseUntil = Time.unscaledTime + InputPulseDuration;
            }
        }

        public void ResetMotionForRespawn()
        {
            if (
                Object == null || // NetworkObject 존재 확인
                !Object.IsValid || // NetworkObject 유효 확인
                !Object.HasStateAuthority // State Authority 확인
            )
            {
                return; // Client 상태 변경 차단
            }

            NetworkVerticalVelocity = 0f; // 이전 점프·낙하 수직 속도 제거
            NetworkGrounded = false; // 부활 위치에서 Ground를 다시 판정
            LastSimulationPosition = transform.position; // Simulation 기준 위치 갱신
            lastForwardPosition = transform.position; // Prediction 기준 위치 갱신
            hasForwardPosition = true; // Prediction 기준 위치 활성화
        }

        public void ResetMovementDiagnostics() // 현재 PC의 이동 진단 누적값 초기화
        {
            Vector3 currentPosition = // 초기화 시점 Player 위치
                transform.position; // 현재 Transform 위치 사용

            LastSimulationPosition = // Simulation 기준 위치 초기화
                currentPosition; // 현재 위치 저장

            LastRenderPosition = // Render 기준 위치 초기화
                currentPosition; // 현재 위치 저장

            RenderSimulationOffset = // Simulation·Render 차이 초기화
                0f; // 측정 시작값 사용

            LastRenderStepDistance = // 최근 Render 이동 거리 초기화
                0f; // 측정 시작값 사용

            RenderSampleCount = // Render 표본 수 초기화
                0; // 새 측정 구간 시작

            previousRenderPosition = // 다음 Render 거리 기준 위치 갱신
                currentPosition; // 현재 위치 사용

            hasRenderPosition = // Render 기준 위치 활성화
                true; // 다음 프레임부터 거리 측정

            ResimulationBatchCount = // 누적 Resimulation Batch 초기화
                0; // 새 측정 구간 시작

            ResimulationTickCount = // 누적 Resimulation Tick 초기화
                0; // 새 측정 구간 시작

            LastResimulationTickCount = // 최근 Resimulation Tick 초기화
                0; // 기록 없음 상태

            LastForwardTickCount = // 최근 Forward Tick 초기화
                0; // 기록 없음 상태

            LastRollbackDistance = // 최근 Rollback 거리 초기화
                0f; // 측정 시작값 사용

            LastCorrectionDistance = // 최근 Correction 거리 초기화
                0f; // 측정 시작값 사용

            MaxCorrectionDistance = // 최대 Correction 거리 초기화
                0f; // 새 측정 구간 시작

            predictedPositionBeforeResimulation = // 내부 Prediction 기준 위치 초기화
                currentPosition; // 현재 위치 사용

            PredictionPositionBeforeResimulation = // 외부 표시 Prediction 위치 초기화
                currentPosition; // 현재 위치 사용

            RollbackPosition = // 외부 표시 Rollback 위치 초기화
                currentPosition; // 현재 위치 사용

            CorrectedPositionAfterResimulation = // 외부 표시 Correction 위치 초기화
                currentPosition; // 현재 위치 사용

            lastForwardPosition = // 다음 Resimulation 비교 기준 갱신
                currentPosition; // 현재 위치 사용

            hasForwardPosition = // Forward 비교 기준 활성화
                true; // 현재 위치 기준 사용
        }

        public void StopMotionForMatchLock()
        {
            if (
                Object == null || // NetworkObject 존재 확인
                !Object.IsValid || // NetworkObject 유효 확인
                !Object.HasStateAuthority // State Authority 확인
            )
            {
                return; // Client 직접 상태 확정 차단
            }

            NetworkVerticalVelocity = 0f; // 경기 잠금 시 수직 속도 제거
            NetworkGrounded = false; // 다음 허용 시 Ground 재판정
            NetworkIsSprinting = false; // 경기 잠금 시 달리기 종료
            LastSimulationPosition = transform.position; // Simulation 기준 위치 고정
            lastForwardPosition = transform.position; // Prediction 기준 위치 고정
            hasForwardPosition = true; // Prediction 기준 유지
        }

        public bool ApplyPlatformPassengerPositionAuthority(
            Vector3 targetPosition
        )
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority ||
                externalGameplay == null ||
                !externalGameplay.GameplayInputAllowed
            )
            {
                return false; // Host의 경기 중 Network Player만 플랫폼 이동 적용
            }

            if (!IsGrounded)
            {
                return false; // 점프·낙하 중인 Player는 플랫폼에 끌려가지 않음
            }

            transform.position = targetPosition; // 플랫폼 이동량을 Player 위치에 적용
            LastSimulationPosition = targetPosition; // Simulation 디버그 기준 위치 갱신

            return true;
        }

        public bool TrySetItemVerticalVelocityAuthority(
            float verticalVelocity
        )
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority ||
                externalGameplay == null ||
                !externalGameplay.GameplayInputAllowed ||
                verticalVelocity <= 0f
            )
            {
                return false; // State Authority의 유효한 경기 중 아이템 점프만 허용
            }

            NetworkVerticalVelocity = verticalVelocity; // 스프링 신발 추가 점프 속도 적용
            NetworkGrounded = false; // 공중 상태 유지
            LastSimulationPosition = transform.position; // Simulation 기준 갱신
            return true;
        }

        private void UpdateCrouchState()
        {
            if (LastReceivedCrouch)
            {
                NetworkIsCrouching = true;
                return;
            }

            if (NetworkIsCrouching && !HasStandingClearance())
            {
                return;
            }

            NetworkIsCrouching = false;
        }

        private void UpdateSprintState(bool hasMoveInput, float deltaTime)
        {
            float stamina = Mathf.Clamp(NetworkStamina, 0f, MaxStamina);

            if (NetworkSprintExhausted)
            {
                NetworkIsSprinting = false;
                stamina = Mathf.Min(
                    MaxStamina,
                    stamina + StaminaRecoveryPerSecond * deltaTime
                );

                if (!LastReceivedSprint && stamina >= SprintRestartStamina)
                {
                    NetworkSprintExhausted = false;
                }

                NetworkStamina = stamina;
                return;
            }

            bool sprintRequested =
                LastReceivedSprint &&
                hasMoveInput &&
                !NetworkIsCrouching &&
                stamina > 0f;

            if (sprintRequested)
            {
                NetworkIsSprinting = true;
                stamina = Mathf.Max(
                    0f,
                    stamina - CurrentSprintStaminaDrainPerSecond * deltaTime
                );

                if (stamina <= 0f)
                {
                    stamina = 0f;
                    NetworkIsSprinting = false;
                    NetworkSprintExhausted = true;
                }
            }
            else
            {
                NetworkIsSprinting = false;
                stamina = Mathf.Min(
                    MaxStamina,
                    stamina + StaminaRecoveryPerSecond * deltaTime
                );
            }

            NetworkStamina = stamina;
        }

        private bool TryGetGroundHeight(
            Vector3 position,
            float probeDistance,
            out float groundHeight
        )
        {
            Vector3 origin = position + Vector3.up * GroundProbeStartHeight;
            float castDistance = GroundProbeStartHeight + probeDistance;

            return TryFindGroundHit(origin, castDistance, out groundHeight);
        }

        private bool TryGetLandingGroundHeight(
            Vector3 currentPosition,
            Vector3 nextPosition,
            out float groundHeight
        )
        {
            float downwardTravel = Mathf.Max(
                0f,
                currentPosition.y - nextPosition.y
            );

            Vector3 origin = new Vector3(
                nextPosition.x,
                currentPosition.y + GroundProbeStartHeight,
                nextPosition.z
            );

            float castDistance =
                GroundProbeStartHeight +
                downwardTravel +
                GroundProbeDistance;

            return TryFindGroundHit(origin, castDistance, out groundHeight);
        }

        private bool TryFindGroundHit(
            Vector3 origin,
            float castDistance,
            out float groundHeight
        )
        {
            int hitCount = Physics.RaycastNonAlloc(
                origin,
                Vector3.down,
                groundHitBuffer,
                castDistance,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore
            );

            float closestDistance = float.PositiveInfinity;
            bool foundGround = false;
            groundHeight = 0f;

            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = groundHitBuffer[i];
                Collider hitCollider = hit.collider;

                if (
                    hitCollider == null ||
                    IsOwnCollider(hitCollider) ||
                    hit.distance >= closestDistance
                )
                {
                    continue;
                }

                closestDistance = hit.distance;
                groundHeight = hit.point.y;
                foundGround = true;
            }

            return foundGround;
        }

        private bool IsJetpackCeilingBlocked( // 제트팩 위쪽 이동 충돌 검사
            float upwardTravelDistance // 이번 Tick 예상 상승 거리
        )
        {
            if (upwardTravelDistance <= 0f)
            {
                return false; // 위쪽 이동이 없으면 천장 검사 생략
            }

            float colliderHeight = NetworkIsCrouching // 현재 자세 높이 선택
                ? CrouchColliderHeight // 앉기 높이 사용
                : StandingColliderHeight; // 서기 높이 사용
            float probeRadius = // 몸통 반경보다 약간 작은 천장 검사 반경
                BodyColliderRadius * StandClearanceRadiusScale; // 기존 여유 배율 재사용
            Vector3 probeOrigin = // 현재 캡슐 상단 구 중심 계산
                transform.position + // Player 발 기준 위치 사용
                Vector3.up * (colliderHeight - probeRadius); // 상단 구 중심 높이 적용
            float castDistance = // 실제 천장 검사 거리 계산
                upwardTravelDistance + ProjectJJetpackPolicy.CeilingProbeSkinMeters; // 이동 거리와 5cm 여유 합산

            int hitCount = Physics.SphereCastNonAlloc( // 할당 없는 위쪽 SphereCast 실행
                probeOrigin, // 캡슐 상단 구 중심 사용
                probeRadius, // 현재 몸통 기준 반경 사용
                Vector3.up, // 위쪽 방향 검사
                jetpackCeilingHitBuffer, // 재사용 충돌 버퍼 사용
                castDistance, // 이번 Tick 검사 거리 사용
                Physics.AllLayers, // 기존 Player 충돌 범위와 동일한 전체 레이어 검사
                QueryTriggerInteraction.Ignore // Trigger는 천장으로 취급하지 않음
            );

            for (int index = 0; index < hitCount; index++)
            {
                Collider hitCollider = jetpackCeilingHitBuffer[index].collider; // 충돌 Collider 조회

                if (hitCollider == null || IsOwnCollider(hitCollider))
                {
                    continue; // 빈 결과·자기 Collider 제외
                }

                return true; // 첫 외부 Collider를 천장 차단으로 판정
            }

            return false; // 위쪽 외부 Collider 없음
        }

        private bool HasStandingClearance()
        {
            float clearanceRadius = BodyColliderRadius * StandClearanceRadiusScale;

            Vector3 bottomPoint = transform.position + Vector3.up *
                (CrouchColliderHeight + clearanceRadius);

            Vector3 topPoint = transform.position + Vector3.up *
                (StandingColliderHeight - clearanceRadius);

            int overlapCount = Physics.OverlapCapsuleNonAlloc(
                bottomPoint,
                topPoint,
                clearanceRadius,
                standOverlapBuffer,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore
            );

            for (int i = 0; i < overlapCount; i++)
            {
                Collider candidate = standOverlapBuffer[i];

                if (candidate == null || IsOwnCollider(candidate))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        private bool IsOwnCollider(Collider candidate)
        {
            Transform candidateTransform = candidate.transform;

            return
                candidateTransform == transform ||
                candidateTransform.IsChildOf(transform);
        }

        private void ApplyColliderPosture()
        {
            if (bodyCollider == null)
            {
                return;
            }

            bool crouching = NetworkIsCrouching;
            float height = crouching ? CrouchColliderHeight : StandingColliderHeight;

            bodyCollider.height = height;
            bodyCollider.radius = BodyColliderRadius;
            bodyCollider.center = new Vector3(0f, height * 0.5f, 0f);
        }

        private void ApplyCrouchPresentation()
        {
            if (visualTransform == null)
            {
                return;
            }

            bool crouching = NetworkIsCrouching;
            Vector3 localPosition = visualTransform.localPosition;
            localPosition.y = crouching ? CrouchVisualY : StandingVisualY;
            visualTransform.localPosition = localPosition;

            Vector3 localScale = visualTransform.localScale;
            localScale.y = crouching ? CrouchVisualScaleY : StandingVisualScaleY;
            visualTransform.localScale = localScale;
        }

        private void LateUpdate()
        {
            if (Object == null || !Object.IsValid)
            {
                return;
            }

            ApplyColliderPosture();
            ApplyCrouchPresentation();

            Vector3 renderPosition = transform.position;
            LastRenderPosition = renderPosition;
            RenderSimulationOffset = Vector3.Distance(
                LastSimulationPosition,
                renderPosition
            );

            if (hasRenderPosition)
            {
                LastRenderStepDistance = Vector3.Distance(
                    previousRenderPosition,
                    renderPosition
                );
            }
            else
            {
                LastRenderStepDistance = 0f;
                hasRenderPosition = true;
            }

            previousRenderPosition = renderPosition;
            RenderSampleCount++;
        }

        public void BeforeAllTicks(bool resimulation, int tickCount)
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasInputAuthority
            )
            {
                return;
            }

            if (!resimulation)
            {
                LastForwardTickCount = tickCount;
                return;
            }

            ResimulationBatchCount++;
            ResimulationTickCount += tickCount;
            LastResimulationTickCount = tickCount;

            predictedPositionBeforeResimulation = hasForwardPosition
                ? lastForwardPosition
                : transform.position;

            PredictionPositionBeforeResimulation = predictedPositionBeforeResimulation;
            RollbackPosition = transform.position;
            LastRollbackDistance = Vector3.Distance(
                predictedPositionBeforeResimulation,
                RollbackPosition
            );
        }

        public void AfterAllTicks(bool resimulation, int tickCount)
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasInputAuthority
            )
            {
                return;
            }

            if (resimulation)
            {
                CorrectedPositionAfterResimulation = transform.position;
                LastCorrectionDistance = Vector3.Distance(
                    predictedPositionBeforeResimulation,
                    CorrectedPositionAfterResimulation
                );

                if (LastCorrectionDistance > MaxCorrectionDistance)
                {
                    MaxCorrectionDistance = LastCorrectionDistance;
                }

                return;
            }

            LastForwardTickCount = tickCount;
            lastForwardPosition = transform.position;
            hasForwardPosition = true;
        }

        private void CachePresentation()
        {
            visualRenderer = GetComponentInChildren<Renderer>(true);
            visualTransform = visualRenderer != null ? visualRenderer.transform : null;
            authorityCamera = GetComponentInChildren<Camera>(true);
            bodyCollider = GetComponent<CapsuleCollider>();
            networkTransform = GetComponent<NetworkTransform>();
            externalGameplay = GetComponent<ProjectJNetworkExternalGameplay>(); // 경기 상태 컴포넌트 조회
            itemInventory = GetComponent<ProjectJNetworkItemInventory>(); // 이동 아이템 효과 컴포넌트 조회
        }

        private void ApplyAuthorityPresentation()
        {
            bool isLocalOwner = Object.HasInputAuthority;

            if (visualRenderer != null)
            {
                runtimeMaterial = visualRenderer.material;
                runtimeMaterial.color = isLocalOwner
                    ? new Color(0.2f, 0.9f, 0.35f, 1f)
                    : new Color(1f, 0.55f, 0.15f, 1f);
            }

            if (authorityCamera == null)
            {
                return;
            }

            authorityCamera.enabled = false;
            authorityCamera.targetTexture = null;
        }

        private void OnDestroy()
        {
            ProjectJLocalPlayerPresentationController.UnbindLocalPlayer(this);

            if (authorityCamera != null)
            {
                authorityCamera.enabled = false;
                authorityCamera.targetTexture = null;
            }

            if (runtimeMaterial != null)
            {
                Destroy(runtimeMaterial);
                runtimeMaterial = null;
            }
        }
    }
}
