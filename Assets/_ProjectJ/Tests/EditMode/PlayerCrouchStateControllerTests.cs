using NUnit.Framework; // EditMode 테스트 기능 참조
using ProjectJ.Data; // 플레이어 앉기 설정 참조
using ProjectJ.Player; // 플레이어 자세 제어기 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{ // 네임스페이스 범위 시작
    public sealed class PlayerCrouchStateControllerTests // 앉기 자세 규칙 테스트 선언
    { // 클래스 범위 시작
        private const float ComparisonTolerance = 0.0001f; // 부동소수점 비교 허용 오차

        private PlayerCrouchStateController controller; // 테스트 대상 자세 제어기

        [SetUp] // 각 테스트 실행 전 준비 메서드 지정
        public void SetUp() // 기본 앉기 설정의 자세 제어기 생성
        { // 메서드 범위 시작
            controller = new PlayerCrouchStateController(PlayerCrouchSettings.CreateDefault()); // 기본값 기반 테스트 대상 생성
        } // 메서드 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void StartsStandingAndAllowsJump() // 초기 서기 상태와 점프 허용 검증
        { // 메서드 범위 시작
            Assert.AreEqual(PlayerPostureState.Standing, controller.CurrentState); // 초기 서기 상태 확인
            Assert.That(controller.CurrentHeight, Is.EqualTo(2f).Within(ComparisonTolerance)); // 초기 서기 높이 확인
            Assert.IsFalse(controller.IsCrouching); // 초기 앉기 상태 해제 확인
            Assert.IsTrue(controller.CanJump); // 초기 점프 허용 확인
        } // 메서드 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void CrouchRequestLowersColliderAndBlocksJump() // 앉기 입력의 충돌체 축소와 점프 차단 검증
        { // 메서드 범위 시작
            controller.Tick(0.1f, true, true, false); // 앉기 목표까지 0.1초 전환

            Assert.AreEqual(PlayerPostureState.Crouching, controller.CurrentState); // 완전한 앉기 상태 확인
            Assert.That(controller.CurrentHeight, Is.EqualTo(1.2f).Within(ComparisonTolerance)); // 앉기 충돌체 높이 확인
            Assert.IsTrue(controller.IsCrouching); // 앉기 상태 확인
            Assert.IsFalse(controller.CanJump); // 앉은 상태 점프 차단 확인
        } // 메서드 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void CrouchReleaseReturnsToStanding() // 앉기 입력 해제 후 서기 복귀 검증
        { // 메서드 범위 시작
            controller.Tick(0.1f, true, true, false); // 완전한 앉기 상태 전환
            controller.Tick(0.1f, false, true, false); // 머리 위 공간이 있는 서기 전환

            Assert.AreEqual(PlayerPostureState.Standing, controller.CurrentState); // 완전한 서기 상태 확인
            Assert.That(controller.CurrentHeight, Is.EqualTo(2f).Within(ComparisonTolerance)); // 서기 충돌체 높이 확인
            Assert.IsTrue(controller.CanJump); // 서기 완료 후 점프 허용 확인
        } // 메서드 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void BlockedStandKeepsCrouchedHeight() // 천장 차단 시 앉기 높이 유지 검증
        { // 메서드 범위 시작
            controller.Tick(0.1f, true, true, false); // 완전한 앉기 상태 전환
            controller.Tick(1f, false, false, false); // 앉기 해제와 서기 공간 차단 처리

            Assert.AreEqual(PlayerPostureState.StandingBlocked, controller.CurrentState); // 서기 차단 상태 확인
            Assert.That(controller.CurrentHeight, Is.EqualTo(1.2f).Within(ComparisonTolerance)); // 앉기 충돌체 높이 유지 확인
            Assert.IsTrue(controller.IsStandingBlocked); // 서기 차단 표시 확인
            Assert.IsFalse(controller.CanJump); // 서기 차단 중 점프 제한 확인
        } // 메서드 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void ExternalForceDoesNotCancelCrouchedPosture() // 밀치기 중 앉기 자세 유지 검증
        { // 메서드 범위 시작
            controller.Tick(0.1f, true, true, false); // 완전한 앉기 상태 전환
            controller.Tick(0.5f, true, true, true); // 외부 힘 적용 중 앉기 입력 유지

            Assert.AreEqual(PlayerPostureState.Crouching, controller.CurrentState); // 밀치기 중 앉기 상태 유지 확인
            Assert.That(controller.CurrentHeight, Is.EqualTo(1.2f).Within(ComparisonTolerance)); // 밀치기 중 낮은 충돌체 유지 확인
            Assert.IsTrue(controller.IsReceivingExternalForce); // 외부 힘 상태 기록 확인
            Assert.IsFalse(controller.CanJump); // 밀치기 중 앉은 자세 점프 차단 확인
        } // 메서드 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void HeightTransitionBlocksJumpUntilStandingCompletes() // 서기 전환 완료 전 점프 차단 검증
        { // 메서드 범위 시작
            controller.Tick(0.1f, true, true, false); // 완전한 앉기 상태 전환
            controller.Tick(0.05f, false, true, false); // 절반 높이만 서기 전환

            Assert.AreEqual(PlayerPostureState.ExitingCrouch, controller.CurrentState); // 서기 전환 상태 확인
            Assert.IsFalse(controller.CanJump); // 전환 중 점프 차단 확인

            controller.Tick(0.05f, false, true, false); // 남은 높이 서기 전환

            Assert.AreEqual(PlayerPostureState.Standing, controller.CurrentState); // 서기 전환 완료 확인
            Assert.IsTrue(controller.CanJump); // 완전한 서기 뒤 점프 허용 확인
        } // 메서드 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void ResetRestoresStandingState() // 부활 초기화의 서기 자세 복원 검증
        { // 메서드 범위 시작
            controller.Tick(0.1f, true, true, true); // 밀치기 중 앉기 상태 생성
            controller.Reset(); // 자세 상태 초기화

            Assert.AreEqual(PlayerPostureState.Standing, controller.CurrentState); // 서기 상태 복원 확인
            Assert.That(controller.CurrentHeight, Is.EqualTo(2f).Within(ComparisonTolerance)); // 서기 높이 복원 확인
            Assert.IsFalse(controller.IsReceivingExternalForce); // 외부 힘 상태 제거 확인
            Assert.IsTrue(controller.CanJump); // 점프 허용 상태 복원 확인
        } // 메서드 범위 종료
    } // 클래스 범위 종료
} // 네임스페이스 범위 종료
