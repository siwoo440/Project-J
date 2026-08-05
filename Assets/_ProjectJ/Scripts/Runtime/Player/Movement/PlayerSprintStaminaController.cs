using ProjectJ.Data; // 플레이어 스태미나 데이터 참조
using UnityEngine; // Unity 수학 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스 선언
{ // 네임스페이스 범위 시작
    public enum PlayerSprintCancelReason // 달리기 취소 원인 종류 선언
    { // 열거형 범위 시작
        None, // 취소 원인 없음
        SprintInputReleased, // 달리기 입력 해제
        MovementInputReleased, // 이동 입력 해제
        LeftGround, // 지면 이탈
        Crouched, // 앉기 자세 전환
        StaminaDepleted, // 스태미나 소진
        ControlDisabled // 플레이어 조작 차단
    } // 열거형 범위 종료

    public sealed class PlayerSprintStaminaController // 달리기와 스태미나 상태 제어기 선언
    { // 클래스 범위 시작
        private const float StaminaTolerance = 0.0001f; // 스태미나 비교 허용 오차

        private readonly PlayerStaminaSettings settings; // 스태미나 설정 저장

        public float CurrentStamina { get; private set; } // 현재 스태미나
        public float RecoveryDelayRemaining { get; private set; } // 남은 회복 대기 시간
        public float NormalizedStamina => settings.MaximumStamina <= 0f ? 0f : CurrentStamina / settings.MaximumStamina; // 현재 스태미나 비율 반환
        public bool IsSprinting { get; private set; } // 현재 달리기 상태
        public bool IsRecoveryDelayed => RecoveryDelayRemaining > StaminaTolerance; // 회복 대기 상태 반환
        public bool IsRecovering => !IsSprinting && !IsRecoveryDelayed && CurrentStamina < settings.MaximumStamina; // 실제 회복 상태 반환
        public bool IsSprintBlockedUntilRelease { get; private set; } // 입력 해제 전 재달리기 차단 상태
        public PlayerSprintCancelReason LastCancelReason { get; private set; } // 마지막 달리기 취소 원인

        public PlayerSprintStaminaController(PlayerStaminaSettings settings) // 스태미나 설정 기반 상태 제어기 생성
        { // 메서드 범위 시작
            this.settings = settings; // 전달된 스태미나 설정 저장
            Reset(); // 초기 스태미나와 상태 적용
        } // 메서드 범위 종료

        public void Tick(float deltaTime, bool sprintPressed, bool hasMoveInput, bool isGrounded, bool isCrouching) // 한 프레임 달리기와 스태미나 상태 갱신
        { // 메서드 범위 시작
            float safeDeltaTime = Mathf.Max(0f, deltaTime); // 음수가 아닌 프레임 시간 보정

            if (!sprintPressed) // 달리기 입력 해제 확인
            { // 조건 범위 시작
                IsSprintBlockedUntilRelease = false; // 소진 후 재입력 차단 해제
            } // 조건 범위 종료

            if (IsSprinting) // 현재 달리기 상태 확인
            { // 조건 범위 시작
                PlayerSprintCancelReason cancelReason = GetCancelReason(sprintPressed, hasMoveInput, isGrounded, isCrouching); // 현재 입력과 자세의 취소 원인 계산

                if (cancelReason != PlayerSprintCancelReason.None) // 달리기 취소 조건 존재 확인
                { // 조건 범위 시작
                    Cancel(cancelReason); // 달리기 취소와 회복 대기 시작
                    return; // 같은 프레임 회복 방지
                } // 조건 범위 종료

                DrainStamina(safeDeltaTime); // 달리기 중 스태미나 소비
                return; // 비달리기 회복 처리 생략
            } // 조건 범위 종료

            if (CanStartSprint(sprintPressed, hasMoveInput, isGrounded, isCrouching)) // 달리기 시작 가능 여부 확인
            { // 조건 범위 시작
                IsSprinting = true; // 달리기 상태 시작
                LastCancelReason = PlayerSprintCancelReason.None; // 이전 취소 원인 초기화
                DrainStamina(safeDeltaTime); // 시작 프레임 스태미나 소비
                return; // 회복 처리 생략
            } // 조건 범위 종료

            RecoverStamina(safeDeltaTime); // 비달리기 상태의 대기와 회복 처리
        } // 메서드 범위 종료

        public void Cancel(PlayerSprintCancelReason cancelReason) // 외부 상태 변화에 따른 달리기 취소
        { // 메서드 범위 시작
            if (!IsSprinting) // 이미 달리기가 종료된 상태 확인
            { // 조건 범위 시작
                return; // 중복 취소 처리 생략
            } // 조건 범위 종료

            IsSprinting = false; // 달리기 상태 해제
            LastCancelReason = cancelReason; // 달리기 취소 원인 저장
            RecoveryDelayRemaining = settings.RecoveryDelay; // 스태미나 회복 대기 시간 시작

            if (cancelReason == PlayerSprintCancelReason.StaminaDepleted) // 스태미나 소진 취소 확인
            { // 조건 범위 시작
                IsSprintBlockedUntilRelease = true; // Shift 해제 전 달리기 재시작 차단
            } // 조건 범위 종료
        } // 메서드 범위 종료

        public void Reset() // 달리기와 스태미나 상태 초기화
        { // 메서드 범위 시작
            CurrentStamina = settings.MaximumStamina; // 스태미나 최대치 복원
            RecoveryDelayRemaining = 0f; // 회복 대기 시간 제거
            IsSprinting = false; // 달리기 상태 해제
            IsSprintBlockedUntilRelease = false; // 재입력 차단 해제
            LastCancelReason = PlayerSprintCancelReason.None; // 취소 원인 초기화
        } // 메서드 범위 종료

        private bool CanStartSprint(bool sprintPressed, bool hasMoveInput, bool isGrounded, bool isCrouching) // 현재 조건의 달리기 시작 가능 여부 반환
        { // 메서드 범위 시작
            if (!sprintPressed || !hasMoveInput) // 달리기 또는 이동 입력 누락 확인
            { // 조건 범위 시작
                return false; // 입력 부족으로 달리기 시작 차단
            } // 조건 범위 종료

            if (!isGrounded || isCrouching) // 지면과 자세 조건 확인
            { // 조건 범위 시작
                return false; // 공중 또는 앉기 상태의 달리기 시작 차단
            } // 조건 범위 종료

            if (IsSprintBlockedUntilRelease) // 소진 후 입력 해제 대기 확인
            { // 조건 범위 시작
                return false; // Shift 재입력 전 달리기 시작 차단
            } // 조건 범위 종료

            if (CurrentStamina <= StaminaTolerance) // 실제 소비 가능한 스태미나 확인
            { // 조건 범위 시작
                return false; // 빈 스태미나 달리기 시작 차단
            } // 조건 범위 종료

            return CurrentStamina + StaminaTolerance >= settings.MinimumStaminaToStartSprint; // 최소 시작 스태미나 충족 여부 반환
        } // 메서드 범위 종료

        private static PlayerSprintCancelReason GetCancelReason(bool sprintPressed, bool hasMoveInput, bool isGrounded, bool isCrouching) // 현재 입력과 자세의 달리기 취소 원인 반환
        { // 메서드 범위 시작
            if (!sprintPressed) // 달리기 입력 해제 확인
            { // 조건 범위 시작
                return PlayerSprintCancelReason.SprintInputReleased; // 입력 해제 취소 원인 반환
            } // 조건 범위 종료

            if (!hasMoveInput) // 이동 입력 해제 확인
            { // 조건 범위 시작
                return PlayerSprintCancelReason.MovementInputReleased; // 이동 중단 취소 원인 반환
            } // 조건 범위 종료

            if (isCrouching) // 앉기 자세 확인
            { // 조건 범위 시작
                return PlayerSprintCancelReason.Crouched; // 앉기 전환 취소 원인 반환
            } // 조건 범위 종료

            if (!isGrounded) // 지면 이탈 확인
            { // 조건 범위 시작
                return PlayerSprintCancelReason.LeftGround; // 공중 전환 취소 원인 반환
            } // 조건 범위 종료

            return PlayerSprintCancelReason.None; // 달리기 유지 가능 반환
        } // 메서드 범위 종료

        private void DrainStamina(float deltaTime) // 달리기 중 스태미나 소비
        { // 메서드 범위 시작
            CurrentStamina = Mathf.Max(0f, CurrentStamina - settings.SprintDrainPerSecond * deltaTime); // 초당 소비량 기반 스태미나 감소
            RecoveryDelayRemaining = settings.RecoveryDelay; // 마지막 소비 시점 기준 회복 대기 갱신

            if (CurrentStamina <= StaminaTolerance) // 스태미나 소진 확인
            { // 조건 범위 시작
                CurrentStamina = 0f; // 미세한 잔여 스태미나 제거
                Cancel(PlayerSprintCancelReason.StaminaDepleted); // 달리기 강제 취소와 재입력 차단
            } // 조건 범위 종료
        } // 메서드 범위 종료

        private void RecoverStamina(float deltaTime) // 비달리기 상태의 스태미나 회복
        { // 메서드 범위 시작
            if (RecoveryDelayRemaining > 0f) // 회복 대기 시간 존재 확인
            { // 조건 범위 시작
                RecoveryDelayRemaining = Mathf.Max(0f, RecoveryDelayRemaining - deltaTime); // 프레임 시간만큼 회복 대기 감소
                return; // 대기 중 스태미나 회복 차단
            } // 조건 범위 종료

            CurrentStamina = Mathf.Min(settings.MaximumStamina, CurrentStamina + settings.RecoveryPerSecond * deltaTime); // 최대값 이내 스태미나 회복
        } // 메서드 범위 종료
    } // 클래스 범위 종료
} // 네임스페이스 범위 종료
