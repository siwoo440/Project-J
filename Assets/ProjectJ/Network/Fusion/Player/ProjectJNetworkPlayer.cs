using Fusion;
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

        private readonly RaycastHit[] groundHitBuffer = new RaycastHit[16];
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
        public float CurrentMoveSpeed => NetworkIsSprinting ? SprintMoveSpeed : BaseMoveSpeed;
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

            if (!GetInput<ProjectJNetworkInput>(out ProjectJNetworkInput input))
            {
                return;
            }

            HasReceivedInput = true;

            Vector2 moveInput = input.Move;

            if (moveInput.sqrMagnitude > 1f)
            {
                moveInput.Normalize();
            }

            LastReceivedMove = moveInput;
            LastReceivedJump = input.Buttons.IsSet(ProjectJNetworkButton.Jump);
            LastReceivedSprint = input.Buttons.IsSet(ProjectJNetworkButton.Sprint);
            LastReceivedCrouch = input.Buttons.IsSet(ProjectJNetworkButton.Crouch);
            LastReceivedTick = Runner.Tick.ToString();

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
                    stamina - SprintStaminaDrainPerSecond * deltaTime
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
