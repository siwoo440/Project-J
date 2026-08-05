using ProjectJ.Data; // 플레이어 설정 데이터 참조
using ProjectJ.Diagnostics; // 프로젝트 공통 로그 기능 참조
using UnityEngine; // Unity 이동과 물리 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스
{ // 네임스페이스 범위 시작
    [DisallowMultipleComponent] // 이동 컴포넌트 중복 방지
    [RequireComponent(typeof(CharacterController))] // 캐릭터 충돌 컴포넌트 보장
    [RequireComponent(typeof(PlayerInputReader))] // 입력 컴포넌트 보장
    [RequireComponent(typeof(PlayerStateController))] // 상태 컴포넌트 보장
    [RequireComponent(typeof(PlayerExternalForceController))] // 외부 힘 컴포넌트 보장
    public sealed class PlayerMovementController : MonoBehaviour // 플레이어 이동 제어 컴포넌트
    { // 클래스 범위 시작
        private const float MovementInputThreshold = 0.0001f; // 이동 입력 판정 기준
        private const float HeightComparisonTolerance = 0.001f; // 높이 비교 오차 기준
        private const int StandOverlapCapacity = 16; // 서기 검사 충돌체 최대 수

        [SerializeField] private PlayerDataDefinition playerData; // 플레이어 설정 에셋
        [SerializeField] private Transform movementCamera; // 이동 방향 기준 카메라
        [SerializeField] private Transform visualRoot; // 플레이어 외형 루트

        private readonly Collider[] standOverlapBuffer = new Collider[StandOverlapCapacity]; // 서기 공간 검사 버퍼
        private CharacterController characterController; // 캐릭터 충돌 제어기
        private PlayerInputReader inputReader; // 플레이어 입력 제공자
        private PlayerStateController stateController; // 플레이어 상태 관리자
        private PlayerExternalForceController externalForceController; // 플레이어 외부 힘 관리자
        private PlayerSprintStaminaController sprintStaminaController; // 달리기와 스태미나 상태 관리자
        private Vector3 controlledHorizontalVelocity; // 입력 기반 수평 속도
        private Vector3 standingVisualLocalScale; // 서기 외형 크기
        private Vector3 standingVisualLocalPosition; // 서기 외형 위치
        private float verticalVelocity; // 현재 수직 속도
        private float coyoteTimeRemaining; // 남은 코요테 시간
        private float jumpBufferTimeRemaining; // 남은 점프 버퍼 시간

        public Vector3 HorizontalVelocity => controlledHorizontalVelocity + externalForceController.HorizontalVelocity; // 실제 수평 속도 반환
        public Vector3 ControlledHorizontalVelocity => controlledHorizontalVelocity; // 입력 기반 수평 속도 반환
        public float VerticalVelocity => verticalVelocity; // 현재 수직 속도 반환
        public float CurrentStamina => sprintStaminaController == null ? 0f : sprintStaminaController.CurrentStamina; // 현재 스태미나 반환
        public float StaminaNormalized => sprintStaminaController == null ? 0f : sprintStaminaController.NormalizedStamina; // 스태미나 비율 반환
        public float StaminaRecoveryDelayRemaining => sprintStaminaController == null ? 0f : sprintStaminaController.RecoveryDelayRemaining; // 남은 회복 대기 시간 반환
        public bool IsGrounded { get; private set; } // 현재 접지 상태
        public bool IsSprinting => sprintStaminaController != null && sprintStaminaController.IsSprinting; // 현재 달리기 상태 반환
        public bool IsStaminaRecoveryDelayed => sprintStaminaController != null && sprintStaminaController.IsRecoveryDelayed; // 스태미나 회복 대기 상태 반환
        public bool IsStaminaRecovering => sprintStaminaController != null && sprintStaminaController.IsRecovering; // 스태미나 회복 상태 반환
        public bool IsSprintBlockedUntilRelease => sprintStaminaController != null && sprintStaminaController.IsSprintBlockedUntilRelease; // Shift 재입력 대기 상태 반환
        public PlayerSprintCancelReason LastSprintCancelReason => sprintStaminaController == null ? PlayerSprintCancelReason.None : sprintStaminaController.LastCancelReason; // 마지막 달리기 취소 원인 반환
        public bool IsCrouching { get; private set; } // 현재 앉기 상태
        public bool IsChangingDirection { get; private set; } // 지상 반대 방향 전환 상태

        private void Awake() // 이동 기능 준비
        { // 메서드 범위 시작
            characterController = GetComponent<CharacterController>(); // 캐릭터 충돌 제어기 조회
            inputReader = GetComponent<PlayerInputReader>(); // 입력 컴포넌트 조회
            stateController = GetComponent<PlayerStateController>(); // 상태 컴포넌트 조회
            externalForceController = GetComponent<PlayerExternalForceController>(); // 외부 힘 컴포넌트 조회

            if (movementCamera == null && Camera.main != null) // 카메라 자동 검색 조건
            { // 조건 범위 시작
                movementCamera = Camera.main.transform; // 메인 카메라 자동 연결
            } // 조건 범위 종료

            if (visualRoot == null) // 외형 자동 검색 조건
            { // 조건 범위 시작
                visualRoot = transform.Find("Visual"); // Visual 자식 자동 연결
            } // 조건 범위 종료

            if (playerData == null) // 플레이어 데이터 누락 확인
            { // 조건 범위 시작
                ProjectLog.Error(ProjectLogCategory.Gameplay, "PLY-001_DefaultPlayer 에셋이 연결되지 않았습니다.", "PLAYER_DATA_MISSING", this); // 데이터 누락 오류 출력
                enabled = false; // 이동 컴포넌트 비활성화
                return; // 이동 준비 중단
            } // 조건 범위 종료

            if (movementCamera == null) // 이동 카메라 누락 확인
            { // 조건 범위 시작
                ProjectLog.Error(ProjectLogCategory.Gameplay, "이동 기준 카메라가 연결되지 않았습니다.", "MOVEMENT_CAMERA_MISSING", this); // 카메라 누락 오류 출력
                enabled = false; // 이동 컴포넌트 비활성화
                return; // 이동 준비 중단
            } // 조건 범위 종료

            characterController.radius = playerData.Crouch.ControllerRadius; // 데이터 기반 충돌체 반지름 적용
            characterController.height = playerData.Crouch.StandingHeight; // 데이터 기반 서기 높이 적용
            characterController.center = Vector3.up * playerData.Crouch.StandingHeight * 0.5f; // 발 위치 고정 중심 적용
            sprintStaminaController = new PlayerSprintStaminaController(playerData.Stamina); // 데이터 기반 달리기와 스태미나 상태 생성

            if (visualRoot != null) // 외형 연결 확인
            { // 조건 범위 시작
                standingVisualLocalScale = visualRoot.localScale; // 서기 외형 크기 저장
                standingVisualLocalPosition = visualRoot.localPosition; // 서기 외형 위치 저장
            } // 조건 범위 종료
        } // 메서드 범위 종료

        private void Update() // 매 프레임 이동 처리
        { // 메서드 범위 시작
            float deltaTime = Time.deltaTime; // 현재 프레임 시간
            externalForceController.Tick(deltaTime); // 외부 힘 시간 갱신

            if (!stateController.CanMove || !characterController.enabled) // 이동 가능 상태 확인
            { // 조건 범위 시작
                sprintStaminaController.Cancel(PlayerSprintCancelReason.ControlDisabled); // 조작 차단 시 진행 중인 달리기 취소
                return; // 이동 처리 생략
            } // 조건 범위 종료

            Vector2 moveInput = Vector2.ClampMagnitude(inputReader.MoveValue, 1f); // 이동 입력 크기 제한
            IsGrounded = characterController.isGrounded; // 이동 전 접지 상태 갱신
            UpdateCrouchState(deltaTime); // 앉기 상태 갱신
            UpdateSprintAndStamina(deltaTime, moveInput); // 달리기와 스태미나 상태 통합 갱신
            UpdateJumpTimers(deltaTime); // 점프 보조 시간 갱신
            UpdateControlledHorizontalVelocity(deltaTime, moveInput); // 입력 기반 수평 속도 갱신
            UpdateVerticalVelocity(deltaTime); // 수직 속도 갱신
            TryConsumeJump(); // 점프 입력 소비

            Vector3 frameVelocity = controlledHorizontalVelocity + externalForceController.HorizontalVelocity + Vector3.up * verticalVelocity; // 입력과 외부 힘을 합친 이동 속도
            CollisionFlags collisionFlags = characterController.Move(frameVelocity * deltaTime); // 캐릭터 이동 실행

            if ((collisionFlags & CollisionFlags.Above) != 0 && verticalVelocity > 0f) // 천장 상승 충돌 확인
            { // 조건 범위 시작
                verticalVelocity = 0f; // 상승 속도 제거
            } // 조건 범위 종료

            IsGrounded = (collisionFlags & CollisionFlags.Below) != 0 || characterController.isGrounded; // 이동 후 접지 상태 갱신
        } // 메서드 범위 종료

        private void UpdateCrouchState(float deltaTime) // 앉기 높이 상태 갱신
        { // 메서드 범위 시작
            bool crouchRequested = inputReader.IsCrouchPressed; // 앉기 입력 상태
            bool standingBlocked = !crouchRequested && !CanStandUp(); // 서기 공간 차단 상태
            float targetHeight = crouchRequested || standingBlocked ? playerData.Crouch.CrouchingHeight : playerData.Crouch.StandingHeight; // 목표 충돌체 높이
            float currentHeight = Mathf.MoveTowards(characterController.height, targetHeight, playerData.Crouch.HeightTransitionSpeed * deltaTime); // 부드러운 높이 전환
            characterController.height = currentHeight; // 현재 충돌체 높이 적용
            characterController.center = Vector3.up * currentHeight * 0.5f; // 발 위치 고정 중심 적용
            IsCrouching = targetHeight < playerData.Crouch.StandingHeight || currentHeight < playerData.Crouch.StandingHeight - HeightComparisonTolerance; // 현재 앉기 상태 판정
            UpdateVisualHeight(currentHeight); // 외형 높이 갱신
        } // 메서드 범위 종료

        private void UpdateVisualHeight(float currentHeight) // 충돌체 기반 외형 높이 갱신
        { // 메서드 범위 시작
            if (visualRoot == null) // 외형 누락 확인
            { // 조건 범위 시작
                return; // 외형 갱신 생략
            } // 조건 범위 종료

            float heightRatio = currentHeight / playerData.Crouch.StandingHeight; // 서기 높이 대비 현재 비율
            Vector3 targetScale = standingVisualLocalScale; // 목표 외형 크기 생성
            targetScale.y = standingVisualLocalScale.y * heightRatio; // 외형 세로 크기 적용
            Vector3 targetPosition = standingVisualLocalPosition; // 목표 외형 위치 생성
            targetPosition.y = standingVisualLocalPosition.y - (playerData.Crouch.StandingHeight - currentHeight) * 0.5f; // 외형 발 위치 고정
            visualRoot.localScale = targetScale; // 외형 크기 적용
            visualRoot.localPosition = targetPosition; // 외형 위치 적용
        } // 메서드 범위 종료

        private bool CanStandUp() // 서기 공간 확보 여부 반환
        { // 메서드 범위 시작
            if (characterController.height >= playerData.Crouch.StandingHeight - HeightComparisonTolerance) // 이미 서기 높이 확인
            { // 조건 범위 시작
                return true; // 서기 가능 반환
            } // 조건 범위 종료

            float radius = playerData.Crouch.ControllerRadius + playerData.Crouch.StandClearancePadding; // 검사 캡슐 반지름 계산
            float lowerHeight = playerData.Crouch.CrouchingHeight - playerData.Crouch.ControllerRadius; // 검사 캡슐 아래 중심 높이
            float upperHeight = playerData.Crouch.StandingHeight - playerData.Crouch.ControllerRadius; // 검사 캡슐 위 중심 높이
            Vector3 lowerPoint = transform.position + Vector3.up * lowerHeight; // 검사 캡슐 아래 중심
            Vector3 upperPoint = transform.position + Vector3.up * upperHeight; // 검사 캡슐 위 중심
            int overlapCount = Physics.OverlapCapsuleNonAlloc(lowerPoint, upperPoint, radius, standOverlapBuffer, ~0, QueryTriggerInteraction.Ignore); // 서기 공간 충돌 검사

            for (int index = 0; index < overlapCount; index++) // 검사된 충돌체 순회
            { // 반복 범위 시작
                Collider overlap = standOverlapBuffer[index]; // 현재 충돌체 조회

                if (overlap == null) // 빈 충돌체 확인
                { // 조건 범위 시작
                    continue; // 빈 항목 건너뛰기
                } // 조건 범위 종료

                if (overlap.transform == transform || overlap.transform.IsChildOf(transform)) // 플레이어 소유 충돌체 확인
                { // 조건 범위 시작
                    continue; // 자기 충돌체 제외
                } // 조건 범위 종료

                return false; // 외부 장애물 감지 반환
            } // 반복 범위 종료

            return true; // 서기 공간 확보 반환
        } // 메서드 범위 종료

        private void UpdateSprintAndStamina(float deltaTime, Vector2 moveInput) // 달리기와 스태미나 상태 통합 갱신
        { // 메서드 범위 시작
            bool hasMoveInput = moveInput.sqrMagnitude > MovementInputThreshold; // 유효 이동 입력 확인
            sprintStaminaController.Tick(deltaTime, inputReader.IsSprintPressed, hasMoveInput, IsGrounded, IsCrouching); // 입력과 자세 기반 달리기와 스태미나 처리
        } // 메서드 범위 종료

        private void UpdateJumpTimers(float deltaTime) // 점프 보조 시간 갱신
        { // 메서드 범위 시작
            if (IsGrounded) // 접지 상태 확인
            { // 조건 범위 시작
                coyoteTimeRemaining = playerData.Jump.CoyoteTime; // 코요테 시간 복원
            } // 조건 범위 종료
            else // 공중 상태 분기
            { // 분기 범위 시작
                coyoteTimeRemaining = Mathf.Max(0f, coyoteTimeRemaining - deltaTime); // 코요테 시간 감소
            } // 분기 범위 종료

            if (inputReader.WasJumpPressedThisFrame()) // 새 점프 입력 확인
            { // 조건 범위 시작
                jumpBufferTimeRemaining = playerData.Jump.JumpBufferTime; // 점프 버퍼 시간 저장
            } // 조건 범위 종료
            else // 새 점프 입력 없음 분기
            { // 분기 범위 시작
                jumpBufferTimeRemaining = Mathf.Max(0f, jumpBufferTimeRemaining - deltaTime); // 점프 버퍼 시간 감소
            } // 분기 범위 종료
        } // 메서드 범위 종료

        private void UpdateControlledHorizontalVelocity(float deltaTime, Vector2 moveInput) // 입력 기반 수평 이동 속도 갱신
        { // 메서드 범위 시작
            Vector3 cameraForward = Vector3.ProjectOnPlane(movementCamera.forward, Vector3.up); // 카메라 수평 전방 계산

            if (cameraForward.sqrMagnitude < MovementInputThreshold) // 카메라 전방 유효성 확인
            { // 조건 범위 시작
                cameraForward = transform.forward; // 플레이어 전방 대체 적용
            } // 조건 범위 종료

            cameraForward.Normalize(); // 카메라 전방 정규화
            Vector3 cameraRight = Vector3.Cross(Vector3.up, cameraForward).normalized; // 카메라 수평 오른쪽 계산
            Vector3 desiredDirection = cameraForward * moveInput.y + cameraRight * moveInput.x; // 목표 이동 방향 계산

            if (desiredDirection.sqrMagnitude > 1f) // 대각선 입력 크기 확인
            { // 조건 범위 시작
                desiredDirection.Normalize(); // 대각선 속도 증가 방지
            } // 조건 범위 종료

            float targetSpeed = GetTargetSpeed() * moveInput.magnitude; // 현재 상태 목표 속도 계산

            if (!IsGrounded) // 공중 상태 확인
            { // 조건 범위 시작
                targetSpeed *= playerData.AirControl.ControlRatio; // 공중 제어 비율 적용
            } // 조건 범위 종료

            Vector3 targetVelocity = desiredDirection * targetSpeed; // 목표 수평 속도 계산
            IsChangingDirection = IsGrounded && PlayerGroundMovementSolver.IsOppositeDirection(controlledHorizontalVelocity, targetVelocity); // 지상 반대 방향 전환 상태 갱신
            float acceleration = GetHorizontalAcceleration(targetVelocity); // 현재 가속도 조회
            controlledHorizontalVelocity = Vector3.MoveTowards(controlledHorizontalVelocity, targetVelocity, acceleration * deltaTime); // 선택된 가속도를 이용한 수평 속도 전환

            if (desiredDirection.sqrMagnitude > MovementInputThreshold) // 회전 가능한 방향 확인
            { // 조건 범위 시작
                Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up); // 목표 회전 계산
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, playerData.Movement.RotationSpeed * deltaTime); // 이동 방향 회전 적용
            } // 조건 범위 종료
        } // 메서드 범위 종료

        private float GetTargetSpeed() // 현재 이동 상태 목표 속도 반환
        { // 메서드 범위 시작
            if (IsCrouching) // 앉기 상태 확인
            { // 조건 범위 시작
                return playerData.Crouch.CrouchMoveSpeed; // 앉기 이동 속도 반환
            } // 조건 범위 종료

            if (IsSprinting) // 달리기 상태 확인
            { // 조건 범위 시작
                return playerData.Sprint.SprintSpeed; // 달리기 이동 속도 반환
            } // 조건 범위 종료

            return playerData.Movement.MoveSpeed; // 기본 이동 속도 반환
        } // 메서드 범위 종료

        private float GetHorizontalAcceleration(Vector3 targetVelocity) // 현재 수평 가속도 반환
        { // 메서드 범위 시작
            if (!IsGrounded) // 공중 상태 확인
            { // 조건 범위 시작
                return playerData.AirControl.Acceleration; // 공중 가속도 반환
            } // 조건 범위 종료

            float baseAcceleration = IsSprinting ? playerData.Sprint.SprintAcceleration : playerData.Movement.Acceleration; // 걷기 또는 달리기 기본 가속도 선택
            return PlayerGroundMovementSolver.SelectAcceleration(controlledHorizontalVelocity, targetVelocity, baseAcceleration, playerData.Movement.Deceleration); // 입력 해제와 방향 전환을 반영한 지상 가속도 반환
        } // 메서드 범위 종료

        private void UpdateVerticalVelocity(float deltaTime) // 수직 속도 갱신
        { // 메서드 범위 시작
            if (IsGrounded && verticalVelocity < 0f) // 접지 중 하강 상태 확인
            { // 조건 범위 시작
                verticalVelocity = playerData.Gravity.GroundedGravity; // 접지 유지 중력 적용
                return; // 공중 중력 계산 생략
            } // 조건 범위 종료

            verticalVelocity += playerData.Gravity.GravityAcceleration * deltaTime; // 중력 가속도 적용
            verticalVelocity = Mathf.Max(verticalVelocity, -playerData.Gravity.MaximumFallSpeed); // 최대 낙하 속도 제한
        } // 메서드 범위 종료

        private void TryConsumeJump() // 점프 입력 실행
        { // 메서드 범위 시작
            if (coyoteTimeRemaining <= 0f || jumpBufferTimeRemaining <= 0f) // 점프 허용 조건 확인
            { // 조건 범위 시작
                return; // 점프 실행 생략
            } // 조건 범위 종료

            float gravityMagnitude = Mathf.Abs(playerData.Gravity.GravityAcceleration); // 중력 절댓값 계산
            verticalVelocity = Mathf.Sqrt(2f * gravityMagnitude * playerData.Jump.JumpHeight); // 점프 초기 속도 계산
            coyoteTimeRemaining = 0f; // 코요테 시간 소비
            jumpBufferTimeRemaining = 0f; // 점프 버퍼 소비
        } // 메서드 범위 종료

        public void ResetAfterRespawn() // 부활 직후 이동과 자세 상태 초기화
        { // 메서드 범위 시작
            if (characterController == null || playerData == null) // 필수 참조 준비 여부 확인
            { // 조건 범위 시작
                return; // 초기화 처리 생략
            } // 조건 범위 종료

            controlledHorizontalVelocity = Vector3.zero; // 입력 기반 수평 속도 제거
            externalForceController.ResetExternalForce(); // 외부 힘 상태 초기화
            sprintStaminaController.Reset(); // 달리기와 스태미나 상태 초기화
            verticalVelocity = playerData.Gravity.GroundedGravity; // 접지 유지용 수직 속도 적용
            coyoteTimeRemaining = 0f; // 코요테 시간 초기화
            jumpBufferTimeRemaining = 0f; // 점프 버퍼 초기화
            IsGrounded = false; // 접지 상태 재검사 준비
            IsCrouching = false; // 앉기 상태 해제
            IsChangingDirection = false; // 방향 전환 상태 해제
            characterController.radius = playerData.Crouch.ControllerRadius; // 충돌체 반지름 복원
            characterController.height = playerData.Crouch.StandingHeight; // 충돌체 서기 높이 복원
            characterController.center = Vector3.up * playerData.Crouch.StandingHeight * 0.5f; // 충돌체 중심 복원
            UpdateVisualHeight(playerData.Crouch.StandingHeight); // 외형 서기 높이 복원
        } // 메서드 범위 종료
    } // 클래스 범위 종료
} // 네임스페이스 범위 종료
