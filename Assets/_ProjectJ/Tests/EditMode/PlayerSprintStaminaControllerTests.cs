using NUnit.Framework; // EditMode 테스트 기능 참조
using ProjectJ.Data; // 플레이어 스태미나 설정 참조
using ProjectJ.Player; // 달리기와 스태미나 제어기 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{ // 네임스페이스 범위 시작
    public sealed class PlayerSprintStaminaControllerTests // 달리기와 스태미나 규칙 테스트 선언
    { // 클래스 범위 시작
        private const float ComparisonTolerance = 0.0001f; // 부동소수점 비교 허용 오차

        private PlayerSprintStaminaController controller; // 테스트 대상 상태 제어기

        [SetUp] // 각 테스트 실행 전 준비 메서드 지정
        public void SetUp() // 기본 스태미나 설정의 상태 제어기 생성
        { // 메서드 범위 시작
            controller = new PlayerSprintStaminaController(PlayerStaminaSettings.CreateDefault()); // 기본값 기반 테스트 대상 생성
        } // 메서드 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void ValidInputStartsSprintAndConsumesStamina() // 올바른 입력의 달리기 시작과 소비 검증
        { // 메서드 범위 시작
            controller.Tick(1f, true, true, true, false); // 1초 동안 정상 달리기 입력 처리

            Assert.IsTrue(controller.IsSprinting); // 달리기 상태 시작 확인
            Assert.That(controller.CurrentStamina, Is.EqualTo(80f).Within(ComparisonTolerance)); // 초당 20 스태미나 소비 확인
        } // 메서드 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void SprintInputReleaseCancelsSprintAndStartsDelay() // Shift 해제의 달리기 취소와 회복 대기 검증
        { // 메서드 범위 시작
            controller.Tick(0.1f, true, true, true, false); // 정상 달리기 시작
            controller.Tick(0.1f, false, true, true, false); // Shift 해제 프레임 처리

            Assert.IsFalse(controller.IsSprinting); // 달리기 취소 확인
            Assert.AreEqual(PlayerSprintCancelReason.SprintInputReleased, controller.LastCancelReason); // 입력 해제 취소 원인 확인
            Assert.That(controller.RecoveryDelayRemaining, Is.EqualTo(0.75f).Within(ComparisonTolerance)); // 전체 회복 대기 시간 시작 확인
        } // 메서드 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void MissingMoveInputCancelsSprint() // 이동 입력 해제의 달리기 취소 검증
        { // 메서드 범위 시작
            controller.Tick(0.1f, true, true, true, false); // 정상 달리기 시작
            controller.Tick(0.1f, true, false, true, false); // 이동 입력 해제 프레임 처리

            Assert.IsFalse(controller.IsSprinting); // 달리기 취소 확인
            Assert.AreEqual(PlayerSprintCancelReason.MovementInputReleased, controller.LastCancelReason); // 이동 입력 해제 원인 확인
        } // 메서드 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void CrouchingCancelsSprint() // 앉기 자세의 달리기 취소 검증
        { // 메서드 범위 시작
            controller.Tick(0.1f, true, true, true, false); // 정상 달리기 시작
            controller.Tick(0.1f, true, true, true, true); // 앉기 자세 전환 프레임 처리

            Assert.IsFalse(controller.IsSprinting); // 달리기 취소 확인
            Assert.AreEqual(PlayerSprintCancelReason.Crouched, controller.LastCancelReason); // 앉기 취소 원인 확인
        } // 메서드 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void LeavingGroundCancelsSprint() // 지면 이탈의 달리기 취소 검증
        { // 메서드 범위 시작
            controller.Tick(0.1f, true, true, true, false); // 정상 달리기 시작
            controller.Tick(0.1f, true, true, false, false); // 공중 전환 프레임 처리

            Assert.IsFalse(controller.IsSprinting); // 달리기 취소 확인
            Assert.AreEqual(PlayerSprintCancelReason.LeftGround, controller.LastCancelReason); // 지면 이탈 취소 원인 확인
        } // 메서드 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void RecoveryWaitsForConfiguredDelay() // 설정된 대기 후 스태미나 회복 검증
        { // 메서드 범위 시작
            controller.Tick(1f, true, true, true, false); // 스태미나 20 소비
            controller.Tick(0.1f, false, true, true, false); // 달리기 취소와 대기 시작
            controller.Tick(0.5f, false, false, true, false); // 회복 대기 0.5초 진행

            Assert.That(controller.CurrentStamina, Is.EqualTo(80f).Within(ComparisonTolerance)); // 대기 중 회복 없음 확인
            Assert.That(controller.RecoveryDelayRemaining, Is.EqualTo(0.25f).Within(ComparisonTolerance)); // 남은 회복 대기 확인

            controller.Tick(0.25f, false, false, true, false); // 남은 회복 대기 종료
            controller.Tick(0.4f, false, false, true, false); // 스태미나 회복 0.4초 진행

            Assert.That(controller.CurrentStamina, Is.EqualTo(90f).Within(ComparisonTolerance)); // 초당 25 기준 10 회복 확인
        } // 메서드 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void DepletionBlocksRestartUntilSprintInputReleased() // 소진 후 Shift 재입력 전 달리기 차단 검증
        { // 메서드 범위 시작
            controller.Tick(5f, true, true, true, false); // 스태미나 전체 소진

            Assert.IsFalse(controller.IsSprinting); // 소진 즉시 달리기 취소 확인
            Assert.IsTrue(controller.IsSprintBlockedUntilRelease); // Shift 해제 전 재시작 차단 확인
            Assert.AreEqual(PlayerSprintCancelReason.StaminaDepleted, controller.LastCancelReason); // 소진 취소 원인 확인

            controller.Tick(1f, true, true, true, false); // Shift 유지 상태의 회복 대기 종료
            controller.Tick(1f, true, true, true, false); // Shift 유지 상태의 스태미나 회복

            Assert.IsFalse(controller.IsSprinting); // 스태미나 회복 후에도 자동 재시작 차단 확인
            Assert.Greater(controller.CurrentStamina, 5f); // 시작 최소값 이상 회복 확인

            controller.Tick(0f, false, true, true, false); // Shift 입력 해제로 재시작 차단 해제
            controller.Tick(0f, true, true, true, false); // Shift 재입력 처리

            Assert.IsTrue(controller.IsSprinting); // 명시적 재입력 후 달리기 시작 확인
        } // 메서드 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void ResetRestoresFullStaminaAndClearsStates() // 부활 초기화 상태 검증
        { // 메서드 범위 시작
            controller.Tick(5f, true, true, true, false); // 스태미나 소진과 재시작 차단 생성
            controller.Reset(); // 달리기와 스태미나 전체 초기화

            Assert.That(controller.CurrentStamina, Is.EqualTo(100f).Within(ComparisonTolerance)); // 최대 스태미나 복원 확인
            Assert.IsFalse(controller.IsSprinting); // 달리기 상태 해제 확인
            Assert.IsFalse(controller.IsRecoveryDelayed); // 회복 대기 제거 확인
            Assert.IsFalse(controller.IsSprintBlockedUntilRelease); // 재시작 차단 제거 확인
            Assert.AreEqual(PlayerSprintCancelReason.None, controller.LastCancelReason); // 취소 원인 초기화 확인
        } // 메서드 범위 종료
    } // 클래스 범위 종료
} // 네임스페이스 범위 종료
