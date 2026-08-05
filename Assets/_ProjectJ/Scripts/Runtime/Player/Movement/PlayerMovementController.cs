using ProjectJ.Data; // 플레이어 설정 데이터 참조
using ProjectJ.Diagnostics; // 프로젝트 공통 로그 기능 참조
using UnityEngine; // Unity 이동과 물리 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스 선언
{ // 플레이어 기능 범위
    [DisallowMultipleComponent] // 이동 컴포넌트 중복 방지
    [RequireComponent(typeof(CharacterController))] // 캐릭터 충돌 컴포넌트 보장
    [RequireComponent(typeof(PlayerInputReader))] // 입력 컴포넌트 보장
    [RequireComponent(typeof(PlayerStateController))] // 상태 컴포넌트 보장
    [RequireComponent(typeof(PlayerExternalForceController))] // 외부 힘 컴포넌트 보장
    public sealed class PlayerMovementController : MonoBehaviour // 플레이어 이동 제어 컴포넌트 선언
    { // 이동 제어 범위
        private const float MovementInputThreshold = 0.0001f; // 이동 입력 판정 기준
        private const int StandOverlapCapacity = 16; // 서기 검사 충돌체 최대 수

        [SerializeField] private PlayerDataDefinition playerData; // 플레이어 설정 에셋
        [SerializeField] private Transform movementCamera; // 이동 방향 기준 카메라
        [SerializeField] private Transform visualRoot; // 플레이어 외형 루트
        [Header("Traversal Probes")] // 지형 탐지 설정 구역 제목 표시
        [SerializeField] private LayerMask traversalLayers = ~0; // 경사와 모서리와 끝자락 탐지 대상 레이어
        [SerializeField, Min(0.01f)] private float groundProbeDistance = 0.35f; // 발 아래 지면 탐지 거리
        [SerializeField, Min(0.01f)] private float cornerProbeDistance = 0.2f; // 충돌체 앞쪽 모서리 추가 탐지 거리
        [SerializeField, Range(0f, 1f)] private float cornerCorrectionStrength = 0.75f; // 공중 모서리 방향 보정 강도
        [SerializeField, Min(0.01f)] private float ledgeForwardDistance = 0.2f; // 충돌체 앞쪽 끝자락 추가 탐지 거리
        [SerializeField, Min(0.01f)] private float ledgeTopSearchHeight = 1.5f; // 끝자락 윗면 아래 방향 검색 시작 높이
        [SerializeField] private bool drawTraversalGizmos = true; // 선택 중 지형 탐지 결과 기즈모 표시 여부

        private readonly Collider[] standOverlapBuffer = new Collider[StandOverlapCapacity]; // 서기 공간 검사 버퍼
        private CharacterController characterController; // 캐릭터 충돌 제어기
        private PlayerInputReader inputReader; // 플레이어 입력 제공자
        private PlayerStateController stateController; // 플레이어 상태 관리자
        private PlayerExternalForceController externalForceController; // 플레이어 외부 힘 관리자
        private PlayerSprintStaminaController sprintStaminaController; // 달리기와 스태미나 상태 관리자
        private PlayerCrouchStateController crouchStateController; // 앉기와 자세 상태 관리자
        private PlayerJumpGravityController jumpGravityController; // 점프와 중력 상태 관리자
        private PlayerTraversalProbe traversalProbe; // 경사와 모서리와 끝자락 탐지기
        private Vector3 controlledHorizontalVelocity; // 입력 기반 수평 속도
        private Vector3 standingVisualLocalScale; // 서기 외형 크기
        private Vector3 standingVisualLocalPosition; // 서기 외형 위치
        private float groundedStepOffset; // 접지 상태의 기본 계단 오프셋

        public Vector3 HorizontalVelocity => controlledHorizontalVelocity + externalForceController.HorizontalVelocity; // 실제 수평 속도 반환
        public Vector3 ControlledHorizontalVelocity => controlledHorizontalVelocity; // 입력 기반 수평 속도 반환
        public float VerticalVelocity => jumpGravityController == null ? 0f : jumpGravityController.VerticalVelocity; // 현재 수직 속도 반환
        public float CoyoteTimeRemaining => jumpGravityController == null ? 0f : jumpGravityController.CoyoteTimeRemaining; // 남은 코요테 시간 반환
        public float JumpBufferTimeRemaining => jumpGravityController == null ? 0f : jumpGravityController.JumpBufferTimeRemaining; // 남은 점프 입력 보관 시간 반환
        public float CurrentStamina => sprintStaminaController == null ? 0f : sprintStaminaController.CurrentStamina; // 현재 스태미나 반환
        public float StaminaNormalized => sprintStaminaController == null ? 0f : sprintStaminaController.NormalizedStamina; // 스태미나 비율 반환
        public float StaminaRecoveryDelayRemaining => sprintStaminaController == null ? 0f : sprintStaminaController.RecoveryDelayRemaining; // 남은 회복 대기 시간 반환
        public float GroundSlopeAngle => traversalProbe == null ? 0f : traversalProbe.GroundSlopeAngle; // 현재 지면 경사 각도 반환
        public Vector3 GroundNormal => traversalProbe == null ? Vector3.up : traversalProbe.GroundNormal; // 현재 지면 법선 반환
        public Vector3 DetectedLedgePoint => traversalProbe == null ? Vector3.zero : traversalProbe.LedgePoint; // 감지된 끝자락 윗면 위치 반환
        public Vector3 DetectedLedgeNormal => traversalProbe == null ? Vector3.zero : traversalProbe.LedgeNormal; // 감지된 끝자락 벽 법선 반환
        public bool IsGrounded { get; private set; } // 현재 접지 상태
        public bool JumpedThisFrame => jumpGravityController != null && jumpGravityController.JumpedThisTick; // 현재 프레임 점프 실행 여부 반환
        public bool IsSprinting => sprintStaminaController != null && sprintStaminaController.IsSprinting; // 현재 달리기 상태 반환
        public bool IsStaminaRecoveryDelayed => sprintStaminaController != null && sprintStaminaController.IsRecoveryDelayed; // 스태미나 회복 대기 상태 반환
        public bool IsStaminaRecovering => sprintStaminaController != null && sprintStaminaController.IsRecovering; // 스태미나 회복 상태 반환
        public bool IsSprintBlockedUntilRelease => sprintStaminaController != null && sprintStaminaController.IsSprintBlockedUntilRelease; // Shift 재입력 대기 상태 반환
        public PlayerSprintCancelReason LastSprintCancelReason => sprintStaminaController == null ? PlayerSprintCancelReason.None : sprintStaminaController.LastCancelReason; // 마지막 달리기 취소 원인 반환
        public bool IsCrouching => crouchStateController != null && crouchStateController.IsCrouching; // 현재 앉기 계열 상태 반환
        public bool IsStandingBlocked => crouchStateController != null && crouchStateController.IsStandingBlocked; // 머리 위 장애물로 서기 차단 상태 반환
        public bool IsReceivingExternalForce => externalForceController != null && externalForceController.IsReceivingExternalForce; // 밀치기와 외부 힘 적용 상태 반환
        public bool IsOnWalkableSlope => traversalProbe != null && traversalProbe.IsOnWalkableSlope; // 이동 가능한 경사면 여부 반환
        public bool IsNearCorner => traversalProbe != null && traversalProbe.IsNearCorner; // 이동 방향 모서리 감지 여부 반환
        public bool IsLedgeDetected => traversalProbe != null && traversalProbe.IsLedgeDetected; // 올라올 수 있는 끝자락 감지 여부 반환
        public PlayerPostureState PostureState => crouchStateController == null ? PlayerPostureState.Standing : crouchStateController.CurrentState; // 현재 자세 상태 반환
        public PlayerJumpState JumpState => jumpGravityController == null ? PlayerJumpState.Grounded : jumpGravityController.CurrentState; // 현재 수직 이동 상태 반환
        public bool IsChangingDirection { get; private set; } // 지상 반대 방향 전환 상태

        private void Awake() // 이동 기능 준비
        { // 이동 준비 범위
            characterController = GetComponent<CharacterController>(); // 캐릭터 충돌 제어기 조회
            inputReader = GetComponent<PlayerInputReader>(); // 입력 컴포넌트 조회
            stateController = GetComponent<PlayerStateController>(); // 상태 컴포넌트 조회
            externalForceController = GetComponent<PlayerExternalForceController>(); // 외부 힘 컴포넌트 조회

            if (movementCamera == null && Camera.main != null) // 카메라 자동 검색 조건 확인
            { // 카메라 자동 연결 범위
                movementCamera = Camera.main.transform; // 메인 카메라 자동 연결
            } // 카메라 자동 연결 범위 종료

            if (visualRoot == null) // 외형 자동 검색 조건 확인
            { // 외형 자동 연결 범위
                visualRoot = transform.Find("Visual"); // Visual 자식 자동 연결
            } // 외형 자동 연결 범위 종료

            if (playerData == null) // 플레이어 데이터 누락 확인
            { // 데이터 누락 범위
                ProjectLog.Error(ProjectLogCategory.Gameplay, "PLY-001_DefaultPlayer 에셋이 연결되지 않았습니다.", "PLAYER_DATA_MISSING", this); // 데이터 누락 오류 출력
                enabled = false; // 이동 컴포넌트 비활성화
                return; // 이동 준비 중단
            } // 데이터 누락 범위 종료

            if (movementCamera == null) // 이동 카메라 누락 확인
            { // 카메라 누락 범위
                ProjectLog.Error(ProjectLogCategory.Gameplay, "이동 기준 카메라가 연결되지 않았습니다.", "MOVEMENT_CAMERA_MISSING", this); // 카메라 누락 오류 출력
                enabled = false; // 이동 컴포넌트 비활성화
                return; // 이동 준비 중단
            } // 카메라 누락 범위 종료

            characterController.radius = playerData.Crouch.ControllerRadius; // 데이터 기반 충돌체 반지름 적용
            groundedStepOffset = characterController.stepOffset; // Scene에 설정된 접지 계단 높이 저장
            sprintStaminaController = new PlayerSprintStaminaController(playerData.Stamina); // 데이터 기반 달리기와 스태미나 상태 생성
            crouchStateController = new PlayerCrouchStateController(playerData.Crouch); // 데이터 기반 앉기와 자세 상태 생성
            jumpGravityController = new PlayerJumpGravityController(playerData.Jump.JumpHeight, playerData.Jump.CoyoteTime, playerData.Jump.JumpBufferTime, playerData.Gravity.GravityAcceleration, playerData.Gravity.MaximumFallSpeed, playerData.Gravity.GroundedGravity); // 데이터 기반 점프와 중력 상태 생성
            int effectiveTraversalLayers = traversalLayers.value & ~(1 << gameObject.layer); // 플레이어 자신의 레이어를 제외한 지형 탐지 마스크 계산
            traversalProbe = new PlayerTraversalProbe(transform, characterController, effectiveTraversalLayers, groundProbeDistance, cornerProbeDistance, cornerCorrectionStrength, ledgeForwardDistance, ledgeTopSearchHeight); // Inspector 설정 기반 지형 탐지기 생성
            ApplyControllerHeight(crouchStateController.CurrentHeight); // 초기 서기 충돌체 높이 적용

            if (visualRoot != null) // 외형 연결 확인
            { // 외형 초기값 저장 범위
                standingVisualLocalScale = visualRoot.localScale; // 서기 외형 크기 저장
                standingVisualLocalPosition = visualRoot.localPosition; // 서기 외형 위치 저장
            } // 외형 초기값 저장 범위 종료
        } // 이동 준비 범위 종료

        private void Update() // 매 프레임 이동 처리
        { // 프레임 이동 범위
            float deltaTime = Time.deltaTime; // 현재 프레임 시간 저장
            externalForceController.Tick(deltaTime); // 외부 힘 시간 갱신

            if (!stateController.CanMove || !characterController.enabled) // 이동 가능 상태 확인
            { // 조작 차단 범위
                sprintStaminaController.Cancel(PlayerSprintCancelReason.ControlDisabled); // 조작 차단 시 진행 중인 달리기 취소
                return; // 이동 처리 생략
            } // 조작 차단 범위 종료

            Vector2 moveInput = Vector2.ClampMagnitude(inputReader.MoveValue, 1f); // 이동 입력 크기 제한
            IsGrounded = characterController.isGrounded; // 이동 전 접지 상태 갱신
            UpdateCrouchState(deltaTime); // 앉기와 자세 상태 갱신
            UpdateSprintAndStamina(deltaTime, moveInput); // 달리기와 스태미나 상태 통합 갱신
            UpdateJumpAndGravity(deltaTime); // 점프 판정과 중력 상태 통합 갱신
            Vector3 desiredDirection = CalculateDesiredDirection(moveInput); // 카메라 기준 목표 이동 방향 계산
            traversalProbe.Tick(desiredDirection); // 경사와 끝자락 탐지 상태 갱신
            Vector3 cornerCorrectedDirection = traversalProbe.CorrectDirectionAroundCorner(desiredDirection); // 전방 장애물 기반 모서리 보정 방향 계산
            Vector3 effectiveDirection = IsGrounded ? desiredDirection : cornerCorrectedDirection; // 공중 상태에만 모서리 보정 방향 적용
            UpdateControlledHorizontalVelocity(deltaTime, moveInput, effectiveDirection); // 입력과 공중 제어 기반 수평 속도 갱신
            UpdateStepOffset(); // 접지와 공중 상태 기반 계단 오프셋 갱신

            Vector3 horizontalVelocity = controlledHorizontalVelocity + externalForceController.HorizontalVelocity; // 입력 속도와 외부 힘 수평 속도 결합
            bool canAlignToGround = IsGrounded && !JumpedThisFrame; // 점프 시작이 아닌 접지 프레임의 경사 정렬 허용
            Vector3 traversalVelocity = traversalProbe.AlignVelocityToGround(horizontalVelocity, canAlignToGround); // 경사면을 따르는 이동 속도 계산
            Vector3 frameVelocity = traversalVelocity + Vector3.up * jumpGravityController.VerticalVelocity; // 지형 보정 속도와 수직 속도 결합
            CollisionFlags collisionFlags = characterController.Move(frameVelocity * deltaTime); // 캐릭터 이동 실행

            if ((collisionFlags & CollisionFlags.Above) != 0) // 천장 상승 충돌 확인
            { // 천장 충돌 범위
                jumpGravityController.CancelUpwardVelocity(); // 남은 상승 속도 제거
            } // 천장 충돌 범위 종료

            IsGrounded = (collisionFlags & CollisionFlags.Below) != 0 || characterController.isGrounded; // 이동 후 접지 상태 갱신
            jumpGravityController.SynchronizeGroundedState(IsGrounded); // 실제 이동 결과와 수직 상태 동기화
        } // 프레임 이동 범위 종료

        private void UpdateCrouchState(float deltaTime) // 앉기 충돌체와 자세 상태 갱신
        { // 자세 갱신 범위
            bool crouchRequested = inputReader.IsCrouchPressed; // 현재 앉기 입력 상태 조회
            bool canStandUp = crouchRequested || CanStandUp(); // 입력 해제 시에만 서기 공간 검사
            crouchStateController.Tick(deltaTime, crouchRequested, canStandUp, externalForceController.IsReceivingExternalForce); // 입력과 공간과 외부 힘 기반 자세 갱신
            ApplyControllerHeight(crouchStateController.CurrentHeight); // 자세 제어기의 현재 높이 적용
            UpdateVisualHeight(crouchStateController.CurrentHeight); // 외형 높이 갱신
        } // 자세 갱신 범위 종료

        private void ApplyControllerHeight(float currentHeight) // 충돌체 높이와 중심 적용
        { // 충돌체 높이 적용 범위
            characterController.height = currentHeight; // 현재 충돌체 높이 적용
            characterController.center = Vector3.up * currentHeight * 0.5f; // 발 위치를 유지하는 충돌체 중심 적용
        } // 충돌체 높이 적용 범위 종료

        private void UpdateVisualHeight(float currentHeight) // 충돌체 기반 외형 높이 갱신
        { // 외형 높이 갱신 범위
            if (visualRoot == null) // 외형 누락 확인
            { // 외형 누락 범위
                return; // 외형 갱신 생략
            } // 외형 누락 범위 종료

            float heightRatio = currentHeight / playerData.Crouch.StandingHeight; // 서기 높이 대비 현재 비율 계산
            Vector3 targetScale = standingVisualLocalScale; // 목표 외형 크기 생성
            targetScale.y = standingVisualLocalScale.y * heightRatio; // 외형 세로 크기 적용
            Vector3 targetPosition = standingVisualLocalPosition; // 목표 외형 위치 생성
            targetPosition.y = standingVisualLocalPosition.y - (playerData.Crouch.StandingHeight - currentHeight) * 0.5f; // 외형 발 위치 고정
            visualRoot.localScale = targetScale; // 외형 크기 적용
            visualRoot.localPosition = targetPosition; // 외형 위치 적용
        } // 외형 높이 갱신 범위 종료

        private bool CanStandUp() // 서기 공간 확보 여부 반환
        { // 서기 공간 검사 범위
            if (!IsCrouching) // 이미 완전히 서 있는 상태 확인
            { // 서기 상태 범위
                return true; // 추가 공간 검사 없이 서기 가능 반환
            } // 서기 상태 범위 종료

            float radius = playerData.Crouch.ControllerRadius + playerData.Crouch.StandClearancePadding; // 검사 캡슐 반지름 계산
            float lowerHeight = playerData.Crouch.CrouchingHeight - playerData.Crouch.ControllerRadius; // 검사 캡슐 아래 중심 높이 계산
            float upperHeight = playerData.Crouch.StandingHeight - playerData.Crouch.ControllerRadius; // 검사 캡슐 위 중심 높이 계산
            Vector3 lowerPoint = transform.position + Vector3.up * lowerHeight; // 검사 캡슐 아래 중심 계산
            Vector3 upperPoint = transform.position + Vector3.up * upperHeight; // 검사 캡슐 위 중심 계산
            int overlapCount = Physics.OverlapCapsuleNonAlloc(lowerPoint, upperPoint, radius, standOverlapBuffer, ~0, QueryTriggerInteraction.Ignore); // 서기 공간 충돌 검사

            for (int index = 0; index < overlapCount; index++) // 검사된 충돌체 순회
            { // 충돌체 순회 범위
                Collider overlap = standOverlapBuffer[index]; // 현재 충돌체 조회

                if (overlap == null) // 빈 충돌체 확인
                { // 빈 충돌체 범위
                    continue; // 빈 항목 건너뛰기
                } // 빈 충돌체 범위 종료

                if (overlap.transform == transform || overlap.transform.IsChildOf(transform)) // 플레이어 소유 충돌체 확인
                { // 자기 충돌체 범위
                    continue; // 자기 충돌체 제외
                } // 자기 충돌체 범위 종료

                return false; // 외부 장애물 감지 반환
            } // 충돌체 순회 범위 종료

            return true; // 서기 공간 확보 반환
        } // 서기 공간 검사 범위 종료

        private void UpdateSprintAndStamina(float deltaTime, Vector2 moveInput) // 달리기와 스태미나 상태 통합 갱신
        { // 달리기 갱신 범위
            bool hasMoveInput = moveInput.sqrMagnitude > MovementInputThreshold; // 유효 이동 입력 확인
            sprintStaminaController.Tick(deltaTime, inputReader.IsSprintPressed, hasMoveInput, IsGrounded, IsCrouching); // 입력과 자세 기반 달리기와 스태미나 처리
        } // 달리기 갱신 범위 종료

        private void UpdateJumpAndGravity(float deltaTime) // 점프 입력과 코요테 시간과 중력 통합 갱신
        { // 점프 갱신 범위
            bool jumpPressedThisFrame = inputReader.WasJumpPressedThisFrame(); // 현재 프레임 새 점프 입력 조회
            jumpGravityController.Tick(deltaTime, IsGrounded, jumpPressedThisFrame, crouchStateController.CanJump); // 접지와 자세 기반 점프와 중력 처리
        } // 점프 갱신 범위 종료

        private Vector3 CalculateDesiredDirection(Vector2 moveInput) // 카메라 기준 목표 이동 방향 반환
        { // 이동 방향 계산 범위
            Vector3 cameraForward = Vector3.ProjectOnPlane(movementCamera.forward, Vector3.up); // 카메라 수평 전방 계산

            if (cameraForward.sqrMagnitude < MovementInputThreshold) // 카메라 전방 유효성 확인
            { // 카메라 방향 보정 범위
                cameraForward = transform.forward; // 플레이어 전방 대체 적용
            } // 카메라 방향 보정 범위 종료

            cameraForward.Normalize(); // 카메라 전방 정규화
            Vector3 cameraRight = Vector3.Cross(Vector3.up, cameraForward).normalized; // 카메라 수평 오른쪽 계산
            Vector3 desiredDirection = cameraForward * moveInput.y + cameraRight * moveInput.x; // 목표 이동 방향 계산

            if (desiredDirection.sqrMagnitude > 1f) // 대각선 입력 크기 확인
            { // 대각선 보정 범위
                desiredDirection.Normalize(); // 대각선 속도 증가 방지
            } // 대각선 보정 범위 종료

            return desiredDirection; // 카메라 기준 목표 방향 반환
        } // 이동 방향 계산 범위 종료

        private void UpdateControlledHorizontalVelocity(float deltaTime, Vector2 moveInput, Vector3 desiredDirection) // 지상과 공중 수평 이동 속도 갱신
        { // 수평 속도 갱신 범위
            if (!IsGrounded) // 공중 상태 확인
            { // 공중 제어 범위
                float airGroundSpeed = playerData.Movement.MoveSpeed * moveInput.magnitude; // 입력 크기를 반영한 공중 기준 지상 속도 계산
                controlledHorizontalVelocity = PlayerTraversalMath.CalculateAirVelocity(controlledHorizontalVelocity, desiredDirection, airGroundSpeed, playerData.AirControl.ControlRatio, playerData.AirControl.Acceleration, deltaTime); // 관성 보존과 제한 가속도 기반 공중 속도 계산
                IsChangingDirection = false; // 공중 상태의 지상 방향 전환 표시 제거
            } // 공중 제어 범위 종료
            else // 지상 상태 확인
            { // 지상 제어 범위
                float targetSpeed = GetTargetSpeed() * moveInput.magnitude; // 현재 자세와 달리기 상태의 목표 속도 계산
                Vector3 targetVelocity = desiredDirection * targetSpeed; // 지상 목표 수평 속도 계산
                IsChangingDirection = PlayerGroundMovementSolver.IsOppositeDirection(controlledHorizontalVelocity, targetVelocity); // 지상 반대 방향 전환 상태 갱신
                controlledHorizontalVelocity = PlayerGroundMovementSolver.CalculateNextVelocity(controlledHorizontalVelocity, targetVelocity, IsSprinting ? playerData.Sprint.SprintAcceleration : playerData.Movement.Acceleration, playerData.Movement.Deceleration, deltaTime); // 지상 가속과 감속 기반 수평 속도 계산
            } // 지상 제어 범위 종료

            if (desiredDirection.sqrMagnitude > MovementInputThreshold) // 회전 가능한 방향 확인
            { // 플레이어 회전 범위
                Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up); // 목표 회전 계산
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, playerData.Movement.RotationSpeed * deltaTime); // 이동 방향 회전 적용
            } // 플레이어 회전 범위 종료
        } // 수평 속도 갱신 범위 종료

        private void UpdateStepOffset() // 접지 상태에 따른 CharacterController 계단 이동 설정 갱신
        { // 계단 오프셋 갱신 범위
            if (!IsGrounded || JumpedThisFrame) // 공중 또는 점프 시작 상태 확인
            { // 계단 비활성 범위
                characterController.stepOffset = 0f; // 공중 벽 걸림 방지를 위한 계단 오프셋 제거
                return; // 접지 계단 설정 생략
            } // 계단 비활성 범위 종료

            float maximumOffsetForHeight = Mathf.Max(0f, characterController.height - characterController.radius * 2f); // 현재 충돌체 높이에 허용되는 최대 계단 높이 계산
            characterController.stepOffset = Mathf.Min(groundedStepOffset, maximumOffsetForHeight); // Scene 기본값과 현재 자세를 반영한 계단 높이 적용
        } // 계단 오프셋 갱신 범위 종료

        private float GetTargetSpeed() // 현재 이동 상태 목표 속도 반환
        { // 목표 속도 선택 범위
            if (IsCrouching) // 앉기 또는 자세 전환 상태 확인
            { // 앉기 속도 범위
                return playerData.Crouch.CrouchMoveSpeed; // 앉기 이동 속도 반환
            } // 앉기 속도 범위 종료

            if (IsSprinting) // 달리기 상태 확인
            { // 달리기 속도 범위
                return playerData.Sprint.SprintSpeed; // 달리기 이동 속도 반환
            } // 달리기 속도 범위 종료

            return playerData.Movement.MoveSpeed; // 기본 이동 속도 반환
        } // 목표 속도 선택 범위 종료

        private void OnDrawGizmosSelected() // 선택 중 경사와 모서리와 끝자락 탐지 결과 표시
        { // 지형 탐지 기즈모 범위
            if (!drawTraversalGizmos || traversalProbe == null) // 기즈모 비활성 또는 탐지기 미준비 확인
            { // 기즈모 생략 범위
                return; // 지형 탐지 기즈모 표시 생략
            } // 기즈모 생략 범위 종료

            Vector3 feetPosition = transform.position; // 플레이어 발 위치 조회
            Gizmos.color = IsOnWalkableSlope ? Color.cyan : Color.gray; // 경사 감지 여부 기반 지면 기즈모 색상 선택
            Gizmos.DrawLine(feetPosition, feetPosition + GroundNormal); // 현재 지면 법선 선 표시

            if (IsNearCorner) // 공중 모서리 감지 여부 확인
            { // 모서리 기즈모 범위
                Gizmos.color = Color.yellow; // 모서리 기즈모 색상 적용
                Gizmos.DrawWireSphere(feetPosition + Vector3.up * characterController.height * 0.5f, characterController.radius + cornerProbeDistance); // 몸통 중심 모서리 탐지 범위 표시
            } // 모서리 기즈모 범위 종료

            if (IsLedgeDetected) // 올라올 수 있는 끝자락 감지 여부 확인
            { // 끝자락 기즈모 범위
                Gizmos.color = Color.green; // 끝자락 기즈모 색상 적용
                Gizmos.DrawSphere(DetectedLedgePoint, 0.08f); // 감지된 끝자락 윗면 위치 표시
                Gizmos.DrawLine(DetectedLedgePoint, DetectedLedgePoint + DetectedLedgeNormal * 0.5f); // 끝자락 벽 법선 표시
            } // 끝자락 기즈모 범위 종료
        } // 지형 탐지 기즈모 범위 종료

        public void ResetAfterRespawn() // 부활 직후 이동과 지형 상태 초기화
        { // 부활 초기화 범위
            if (characterController == null || playerData == null) // 필수 참조 준비 여부 확인
            { // 참조 미준비 범위
                return; // 초기화 처리 생략
            } // 참조 미준비 범위 종료

            controlledHorizontalVelocity = Vector3.zero; // 입력 기반 수평 속도 제거
            externalForceController.ResetExternalForce(); // 외부 힘 상태 초기화
            sprintStaminaController.Reset(); // 달리기와 스태미나 상태 초기화
            crouchStateController.Reset(); // 앉기와 자세 상태 초기화
            jumpGravityController.Reset(); // 점프와 중력 상태 초기화
            traversalProbe.Reset(); // 경사와 모서리와 끝자락 탐지 상태 초기화
            IsGrounded = false; // 접지 상태 재검사 준비
            IsChangingDirection = false; // 방향 전환 상태 해제
            characterController.radius = playerData.Crouch.ControllerRadius; // 충돌체 반지름 복원
            characterController.stepOffset = groundedStepOffset; // 접지 계단 오프셋 복원
            ApplyControllerHeight(crouchStateController.CurrentHeight); // 충돌체 서기 높이 복원
            UpdateVisualHeight(crouchStateController.CurrentHeight); // 외형 서기 높이 복원
        } // 부활 초기화 범위 종료
    } // 이동 제어 범위 종료
} // 플레이어 기능 범위 종료
