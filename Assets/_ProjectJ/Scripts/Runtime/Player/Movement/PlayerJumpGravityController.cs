using UnityEngine; // Unity 수학 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스 선언
{ // 플레이어 기능 범위
    public enum PlayerJumpState // 플레이어 수직 이동 상태 종류
    { // 수직 이동 상태 범위
        Grounded, // 지상 정지 상태
        Rising, // 점프 상승 상태
        Falling // 공중 하강 상태
    } // 수직 이동 상태 범위 종료

    public sealed class PlayerJumpGravityController // 점프와 중력 통합 제어기 선언
    { // 점프와 중력 제어 범위
        private readonly float jumpHeight; // 목표 점프 높이
        private readonly float coyoteTime; // 최대 코요테 시간
        private readonly float jumpBufferTime; // 최대 점프 입력 보관 시간
        private readonly float gravityAcceleration; // 중력 가속도
        private readonly float maximumFallSpeed; // 최대 낙하 속도
        private readonly float groundedGravity; // 접지 유지 중력

        public float VerticalVelocity { get; private set; } // 현재 수직 속도
        public float CoyoteTimeRemaining { get; private set; } // 남은 코요테 시간
        public float JumpBufferTimeRemaining { get; private set; } // 남은 점프 입력 보관 시간
        public bool JumpedThisTick { get; private set; } // 현재 갱신의 점프 실행 여부
        public PlayerJumpState CurrentState { get; private set; } // 현재 수직 이동 상태

        public PlayerJumpGravityController(float jumpHeight, float coyoteTime, float jumpBufferTime, float gravityAcceleration, float maximumFallSpeed, float groundedGravity) // 데이터 수치 기반 통합 제어기 생성
        { // 생성 처리 범위
            this.jumpHeight = Mathf.Max(0f, jumpHeight); // 음수가 아닌 점프 높이 저장
            this.coyoteTime = Mathf.Max(0f, coyoteTime); // 음수가 아닌 코요테 시간 저장
            this.jumpBufferTime = Mathf.Max(0f, jumpBufferTime); // 음수가 아닌 입력 보관 시간 저장
            this.gravityAcceleration = Mathf.Min(0f, gravityAcceleration); // 아래 방향 중력 저장
            this.maximumFallSpeed = Mathf.Max(0f, maximumFallSpeed); // 음수가 아닌 최대 낙하 속도 저장
            this.groundedGravity = Mathf.Min(0f, groundedGravity); // 아래 방향 접지 중력 저장
            Reset(); // 초기 수직 상태 적용
        } // 생성 처리 범위 종료

        public void Tick(float deltaTime, bool isGrounded, bool jumpPressedThisTick, bool canJump) // 한 프레임 점프와 중력 통합 갱신
        { // 프레임 갱신 범위
            float safeDeltaTime = Mathf.Max(0f, deltaTime); // 음수가 아닌 프레임 시간 보정
            JumpedThisTick = false; // 현재 프레임 점프 결과 초기화
            UpdateCoyoteTime(safeDeltaTime, isGrounded); // 접지 상태 기반 코요테 시간 갱신
            UpdateJumpBuffer(safeDeltaTime, jumpPressedThisTick, canJump); // 입력과 자세 기반 점프 버퍼 갱신
            ApplyGravity(safeDeltaTime, isGrounded); // 접지와 공중 중력 적용
            TryStartJump(canJump); // 코요테 시간과 버퍼 기반 점프 시도
            UpdateState(isGrounded && !JumpedThisTick); // 점프 실행을 반영한 수직 상태 갱신
        } // 프레임 갱신 범위 종료

        public void SynchronizeGroundedState(bool isGrounded) // 실제 이동 뒤 접지 결과 동기화
        { // 접지 동기화 범위
            UpdateState(isGrounded); // 충돌 이동 결과 기반 상태 보정
        } // 접지 동기화 범위 종료

        public void CancelUpwardVelocity() // 천장 충돌에 따른 상승 취소
        { // 상승 취소 범위
            if (VerticalVelocity <= 0f) // 상승 중이 아닌 상태 확인
            { // 상승 없음 범위
                return; // 수직 속도 변경 생략
            } // 상승 없음 범위 종료

            VerticalVelocity = 0f; // 남은 상승 속도 제거
            CurrentState = PlayerJumpState.Falling; // 하강 전환 상태 적용
        } // 상승 취소 범위 종료

        public void Reset() // 점프와 중력 상태 초기화
        { // 상태 초기화 범위
            VerticalVelocity = groundedGravity; // 접지 유지 수직 속도 적용
            CoyoteTimeRemaining = 0f; // 코요테 시간 제거
            JumpBufferTimeRemaining = 0f; // 점프 입력 버퍼 제거
            JumpedThisTick = false; // 점프 실행 결과 제거
            CurrentState = PlayerJumpState.Grounded; // 지상 상태 복원
        } // 상태 초기화 범위 종료

        private void UpdateCoyoteTime(float deltaTime, bool isGrounded) // 접지 상태 기반 코요테 시간 계산
        { // 코요테 시간 계산 범위
            if (isGrounded) // 현재 접지 상태 확인
            { // 접지 시간 복원 범위
                CoyoteTimeRemaining = coyoteTime; // 최대 코요테 시간 복원
                return; // 공중 시간 감소 생략
            } // 접지 시간 복원 범위 종료

            CoyoteTimeRemaining = Mathf.Max(0f, CoyoteTimeRemaining - deltaTime); // 공중 경과 시간만큼 코요테 시간 감소
        } // 코요테 시간 계산 범위 종료

        private void UpdateJumpBuffer(float deltaTime, bool jumpPressedThisTick, bool canJump) // 점프 입력 보관 시간 계산
        { // 점프 입력 보관 범위
            if (!canJump) // 현재 자세의 점프 제한 확인
            { // 점프 제한 범위
                JumpBufferTimeRemaining = 0f; // 제한 상태의 기존 입력 제거
                return; // 새 점프 입력 저장 생략
            } // 점프 제한 범위 종료

            if (jumpPressedThisTick) // 현재 프레임 새 점프 입력 확인
            { // 새 입력 저장 범위
                JumpBufferTimeRemaining = jumpBufferTime; // 최대 입력 보관 시간 저장
                return; // 기존 입력 시간 감소 생략
            } // 새 입력 저장 범위 종료

            JumpBufferTimeRemaining = Mathf.Max(0f, JumpBufferTimeRemaining - deltaTime); // 경과 시간만큼 입력 보관 시간 감소
        } // 점프 입력 보관 범위 종료

        private void ApplyGravity(float deltaTime, bool isGrounded) // 접지와 공중 수직 속도 계산
        { // 중력 계산 범위
            if (isGrounded && VerticalVelocity < 0f) // 접지 중 하강 속도 확인
            { // 접지 중력 범위
                VerticalVelocity = groundedGravity; // 접지 유지용 수직 속도 적용
                return; // 공중 중력 계산 생략
            } // 접지 중력 범위 종료

            VerticalVelocity += gravityAcceleration * deltaTime; // 프레임 시간 기반 중력 가속 적용
            VerticalVelocity = Mathf.Max(VerticalVelocity, -maximumFallSpeed); // 최대 낙하 속도 제한
        } // 중력 계산 범위 종료

        private void TryStartJump(bool canJump) // 저장된 입력과 코요테 시간 기반 점프 실행
        { // 점프 실행 판정 범위
            if (!canJump || CoyoteTimeRemaining <= 0f || JumpBufferTimeRemaining <= 0f) // 점프 실행 조건 확인
            { // 점프 불가 범위
                return; // 점프 실행 생략
            } // 점프 불가 범위 종료

            float gravityMagnitude = Mathf.Abs(gravityAcceleration); // 중력 가속도 절댓값 계산
            VerticalVelocity = Mathf.Sqrt(2f * gravityMagnitude * jumpHeight); // 목표 높이에 필요한 초기 점프 속도 계산
            CoyoteTimeRemaining = 0f; // 사용한 코요테 시간 제거
            JumpBufferTimeRemaining = 0f; // 사용한 점프 입력 제거
            JumpedThisTick = true; // 현재 프레임 점프 실행 기록
        } // 점프 실행 판정 범위 종료

        private void UpdateState(bool isGrounded) // 접지와 수직 속도 기반 상태 결정
        { // 수직 상태 결정 범위
            if (isGrounded && VerticalVelocity <= 0f) // 접지 중 비상승 상태 확인
            { // 지상 상태 범위
                CurrentState = PlayerJumpState.Grounded; // 지상 상태 적용
                return; // 공중 상태 판정 생략
            } // 지상 상태 범위 종료

            CurrentState = VerticalVelocity > 0f ? PlayerJumpState.Rising : PlayerJumpState.Falling; // 수직 속도 방향 기반 공중 상태 적용
        } // 수직 상태 결정 범위 종료
    } // 점프와 중력 제어 범위 종료
} // 플레이어 기능 범위 종료
