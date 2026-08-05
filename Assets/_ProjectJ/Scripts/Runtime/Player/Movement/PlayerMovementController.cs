using ProjectJ.Data; // 플레이어 설정 데이터 참조
using UnityEngine; // Unity 이동과 수학 기능 참조
namespace ProjectJ.Player // 플레이어 기능 네임스페이스 선언
{ // 플레이어 기능 코드 묶음
    [DisallowMultipleComponent] // 동일 오브젝트의 이동 컴포넌트 중복 방지
    [RequireComponent(typeof(CharacterController), typeof(PlayerInputReader))] // 필수 이동과 입력 컴포넌트 자동 보장
    public sealed class PlayerMovementController : MonoBehaviour // 기본 이동과 점프 처리 컴포넌트 선언
    { // 플레이어 이동 코드 묶음
        [SerializeField] private PlayerDataDefinition playerData; // 플레이어 이동 설정 에셋 참조
        [SerializeField] private Transform movementCamera; // 이동 방향 기준 카메라 참조
        private CharacterController characterController; // CharacterController 컴포넌트 저장
        private PlayerInputReader inputReader; // 플레이어 입력 읽기 컴포넌트 저장
        private Vector3 horizontalVelocity; // 현재 수평 이동 속도 저장
        private float verticalVelocity; // 현재 수직 이동 속도 저장
        private float coyoteTimeRemaining; // 남은 코요테 시간 저장
        private float jumpBufferTimeRemaining; // 남은 점프 입력 보관 시간 저장
        public Vector3 HorizontalVelocity => horizontalVelocity; // 현재 수평 이동 속도 반환
        public float VerticalVelocity => verticalVelocity; // 현재 수직 이동 속도 반환
        public bool IsGrounded { get; private set; } // 현재 접지 상태 저장
        private void Awake() // 플레이어 이동 필수 참조 준비
        { // 이동 준비 처리 묶음
            characterController = GetComponent<CharacterController>(); // CharacterController 컴포넌트 조회
            inputReader = GetComponent<PlayerInputReader>(); // 입력 읽기 컴포넌트 조회
            if (movementCamera == null && Camera.main != null) // 이동 기준 카메라 자동 검색 조건 확인
            { // 카메라 자동 연결 처리 묶음
                movementCamera = Camera.main.transform; // Main Camera를 이동 방향 기준으로 연결
            } // 카메라 자동 연결 처리 종료
            if (playerData == null) // 플레이어 데이터 연결 여부 확인
            { // 플레이어 데이터 누락 처리 묶음
                Debug.LogError("[ProjectJ][Gameplay][PLAYER_DATA_MISSING] PLY-001_DefaultPlayer 에셋이 연결되지 않았습니다.", this); // 플레이어 데이터 누락 오류 출력
                enabled = false; // 이동 컴포넌트 비활성화
                return; // 이동 준비 처리 중단
            } // 플레이어 데이터 누락 처리 종료
            if (movementCamera == null) // 이동 기준 카메라 연결 여부 확인
            { // 이동 기준 카메라 누락 처리 묶음
                Debug.LogError("[ProjectJ][Gameplay][MOVEMENT_CAMERA_MISSING] 이동 기준 카메라가 연결되지 않았습니다.", this); // 이동 기준 카메라 누락 오류 출력
                enabled = false; // 이동 컴포넌트 비활성화
            } // 이동 기준 카메라 누락 처리 종료
        } // 이동 준비 처리 종료
        private void Update() // 매 프레임 이동과 점프 처리
        { // 프레임 이동 처리 묶음
            float deltaTime = Time.deltaTime; // 현재 프레임 경과 시간 저장
            IsGrounded = characterController.isGrounded; // 이동 전 접지 상태 갱신
            UpdateJumpTimers(deltaTime); // 코요테 시간과 점프 입력 버퍼 갱신
            UpdateHorizontalVelocity(deltaTime); // 카메라 기준 수평 이동 속도 갱신
            UpdateVerticalVelocity(deltaTime); // 중력 기반 수직 이동 속도 갱신
            TryConsumeJump(); // 가능한 점프 입력 소비
            Vector3 frameVelocity = horizontalVelocity + Vector3.up * verticalVelocity; // 현재 프레임 전체 이동 속도 계산
            CollisionFlags collisionFlags = characterController.Move(frameVelocity * deltaTime); // CharacterController를 사용한 실제 이동 실행
            if ((collisionFlags & CollisionFlags.Above) != 0 && verticalVelocity > 0f) // 머리 충돌 중 상승 상태 확인
            { // 머리 충돌 처리 묶음
                verticalVelocity = 0f; // 천장 관통 방지를 위한 상승 속도 제거
            } // 머리 충돌 처리 종료
            IsGrounded = (collisionFlags & CollisionFlags.Below) != 0 || characterController.isGrounded; // 이동 후 접지 상태 갱신
        } // 프레임 이동 처리 종료
        private void UpdateJumpTimers(float deltaTime) // 점프 허용 시간과 입력 보관 시간 갱신
        { // 점프 시간 갱신 처리 묶음
            if (IsGrounded) // 현재 접지 상태 확인
            { // 접지 코요테 시간 처리 묶음
                coyoteTimeRemaining = playerData.Jump.CoyoteTime; // 코요테 시간을 최대값으로 복원
            } // 접지 코요테 시간 처리 종료
            else // 공중 상태 처리 분기
            { // 공중 코요테 시간 처리 묶음
                coyoteTimeRemaining = Mathf.Max(0f, coyoteTimeRemaining - deltaTime); // 남은 코요테 시간 감소
            } // 공중 코요테 시간 처리 종료
            if (inputReader.WasJumpPressedThisFrame()) // 현재 프레임 점프 입력 확인
            { // 점프 입력 저장 처리 묶음
                jumpBufferTimeRemaining = playerData.Jump.JumpBufferTime; // 점프 입력 보관 시간을 최대값으로 설정
            } // 점프 입력 저장 처리 종료
            else // 새 점프 입력 없음 처리 분기
            { // 점프 입력 시간 감소 처리 묶음
                jumpBufferTimeRemaining = Mathf.Max(0f, jumpBufferTimeRemaining - deltaTime); // 남은 점프 입력 보관 시간 감소
            } // 점프 입력 시간 감소 처리 종료
        } // 점프 시간 갱신 처리 종료
        private void UpdateHorizontalVelocity(float deltaTime) // 카메라 기준 수평 이동 속도 갱신
        { // 수평 이동 계산 처리 묶음
            Vector2 moveInput = Vector2.ClampMagnitude(inputReader.MoveValue, 1f); // 이동 입력 크기를 최대 1로 제한
            Vector3 cameraForward = Vector3.ProjectOnPlane(movementCamera.forward, Vector3.up); // 카메라 전방의 수평 방향 계산
            if (cameraForward.sqrMagnitude < 0.0001f) // 카메라 수평 전방 방향 유효 여부 확인
            { // 카메라 전방 보정 처리 묶음
                cameraForward = transform.forward; // 플레이어 전방을 대체 방향으로 사용
            } // 카메라 전방 보정 처리 종료
            cameraForward.Normalize(); // 카메라 수평 전방 방향 정규화
            Vector3 cameraRight = Vector3.Cross(Vector3.up, cameraForward).normalized; // 카메라 수평 오른쪽 방향 계산
            Vector3 desiredDirection = cameraForward * moveInput.y + cameraRight * moveInput.x; // 입력에 따른 월드 이동 방향 계산
            if (desiredDirection.sqrMagnitude > 1f) // 대각선 이동 방향 크기 확인
            { // 대각선 이동 보정 처리 묶음
                desiredDirection.Normalize(); // 대각선 이동 속도 증가 방지
            } // 대각선 이동 보정 처리 종료
            float speedRatio = IsGrounded ? 1f : playerData.AirControl.ControlRatio; // 접지 상태에 따른 이동 속도 비율 계산
            float targetSpeed = playerData.Movement.MoveSpeed * speedRatio * moveInput.magnitude; // 현재 입력 크기에 따른 목표 속도 계산
            Vector3 targetVelocity = desiredDirection * targetSpeed; // 목표 수평 이동 속도 계산
            float acceleration = GetHorizontalAcceleration(moveInput); // 현재 상태에 맞는 수평 가속도 조회
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, targetVelocity, acceleration * deltaTime); // 목표값을 향한 수평 속도 변경
            if (desiredDirection.sqrMagnitude > 0.0001f) // 회전 가능한 이동 방향 존재 여부 확인
            { // 플레이어 회전 처리 묶음
                Quaternion targetRotation = Quaternion.LookRotation(desiredDirection, Vector3.up); // 이동 방향 기준 목표 회전 계산
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, playerData.Movement.RotationSpeed * deltaTime); // 목표 이동 방향으로 부드러운 회전 적용
            } // 플레이어 회전 처리 종료
        } // 수평 이동 계산 처리 종료
        private float GetHorizontalAcceleration(Vector2 moveInput) // 현재 상태의 수평 가속도 반환
        { // 수평 가속도 선택 처리 묶음
            if (!IsGrounded) // 공중 상태 확인
            { // 공중 가속도 처리 묶음
                return playerData.AirControl.Acceleration; // 공중 방향 제어 가속도 반환
            } // 공중 가속도 처리 종료
            if (moveInput.sqrMagnitude > 0.0001f) // 지상 이동 입력 존재 여부 확인
            { // 지상 가속 처리 묶음
                return playerData.Movement.Acceleration; // 지상 이동 가속도 반환
            } // 지상 가속 처리 종료
            return playerData.Movement.Deceleration; // 지상 입력 해제 감속도 반환
        } // 수평 가속도 선택 처리 종료
        private void UpdateVerticalVelocity(float deltaTime) // 접지와 중력 기반 수직 속도 갱신
        { // 수직 속도 계산 처리 묶음
            if (IsGrounded && verticalVelocity < 0f) // 접지 상태의 하강 속도 확인
            { // 접지 수직 속도 처리 묶음
                verticalVelocity = playerData.Gravity.GroundedGravity; // 접지 유지용 작은 하향 속도 적용
                return; // 공중 중력 계산 생략
            } // 접지 수직 속도 처리 종료
            verticalVelocity += playerData.Gravity.GravityAcceleration * deltaTime; // 현재 수직 속도에 중력 가속도 적용
            verticalVelocity = Mathf.Max(verticalVelocity, -playerData.Gravity.MaximumFallSpeed); // 최대 낙하 속도 제한
        } // 수직 속도 계산 처리 종료
        private void TryConsumeJump() // 코요테 시간과 버퍼를 사용한 점프 실행
        { // 점프 실행 판단 처리 묶음
            if (coyoteTimeRemaining <= 0f || jumpBufferTimeRemaining <= 0f) // 점프 허용 조건 확인
            { // 점프 불가 처리 묶음
                return; // 점프 실행 생략
            } // 점프 불가 처리 종료
            float gravityMagnitude = Mathf.Abs(playerData.Gravity.GravityAcceleration); // 중력 가속도 절댓값 계산
            verticalVelocity = Mathf.Sqrt(2f * gravityMagnitude * playerData.Jump.JumpHeight); // 목표 높이에 필요한 초기 점프 속도 계산
            coyoteTimeRemaining = 0f; // 중복 점프 방지를 위한 코요테 시간 소비
            jumpBufferTimeRemaining = 0f; // 사용한 점프 입력 버퍼 소비
        } // 점프 실행 판단 처리 종료
    } // 플레이어 이동 코드 종료
} // 플레이어 기능 코드 종료
