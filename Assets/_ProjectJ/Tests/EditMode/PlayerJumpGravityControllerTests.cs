using NUnit.Framework; // EditMode 테스트 기능 참조
using ProjectJ.Player; // 점프와 중력 제어기 참조
using UnityEngine; // Unity 수학 기능 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{ // EditMode 테스트 범위
    public sealed class PlayerJumpGravityControllerTests // 점프와 중력 통합 규칙 테스트 선언
    { // 점프 테스트 범위
        private const float ComparisonTolerance = 0.0001f; // 부동소수점 비교 허용 오차
        private const float JumpHeight = 2.4f; // 테스트 목표 점프 높이
        private const float CoyoteTime = 0.12f; // 테스트 코요테 시간
        private const float JumpBufferTime = 0.12f; // 테스트 점프 입력 보관 시간
        private const float GravityAcceleration = -25f; // 테스트 중력 가속도
        private const float MaximumFallSpeed = 35f; // 테스트 최대 낙하 속도
        private const float GroundedGravity = -2f; // 테스트 접지 유지 중력

        private PlayerJumpGravityController controller; // 테스트 대상 점프와 중력 제어기

        [SetUp] // 각 테스트 실행 전 준비 메서드 지정
        public void SetUp() // 기본 수치 기반 점프 제어기 생성
        { // 테스트 준비 범위
            controller = new PlayerJumpGravityController(JumpHeight, CoyoteTime, JumpBufferTime, GravityAcceleration, MaximumFallSpeed, GroundedGravity); // 기본 테스트 대상 생성
        } // 테스트 준비 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void ResetStartsGroundedAndClearsTimers() // 초기 지상 상태와 타이머 제거 검증
        { // 초기 상태 검증 범위
            Assert.AreEqual(PlayerJumpState.Grounded, controller.CurrentState); // 초기 지상 상태 확인
            Assert.That(controller.VerticalVelocity, Is.EqualTo(GroundedGravity).Within(ComparisonTolerance)); // 초기 접지 유지 속도 확인
            Assert.That(controller.CoyoteTimeRemaining, Is.EqualTo(0f).Within(ComparisonTolerance)); // 초기 코요테 시간 제거 확인
            Assert.That(controller.JumpBufferTimeRemaining, Is.EqualTo(0f).Within(ComparisonTolerance)); // 초기 점프 버퍼 제거 확인
            Assert.IsFalse(controller.JumpedThisTick); // 초기 점프 실행 없음 확인
        } // 초기 상태 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void GroundedInputStartsJumpWithConfiguredHeight() // 접지 입력의 목표 높이 기반 점프 검증
        { // 기본 점프 검증 범위
            controller.Tick(0f, true, true, true); // 접지 상태의 새 점프 입력 처리

            float expectedVelocity = Mathf.Sqrt(2f * Mathf.Abs(GravityAcceleration) * JumpHeight); // 예상 초기 점프 속도 계산
            Assert.That(controller.VerticalVelocity, Is.EqualTo(expectedVelocity).Within(ComparisonTolerance)); // 목표 높이 기반 초기 속도 확인
            Assert.AreEqual(PlayerJumpState.Rising, controller.CurrentState); // 점프 상승 상태 확인
            Assert.IsTrue(controller.JumpedThisTick); // 현재 갱신의 점프 실행 확인
            Assert.That(controller.CoyoteTimeRemaining, Is.EqualTo(0f).Within(ComparisonTolerance)); // 사용한 코요테 시간 제거 확인
            Assert.That(controller.JumpBufferTimeRemaining, Is.EqualTo(0f).Within(ComparisonTolerance)); // 사용한 입력 버퍼 제거 확인
        } // 기본 점프 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void CoyoteInputStartsJumpInsideWindow() // 지면 이탈 뒤 허용 시간 내 점프 검증
        { // 코요테 점프 검증 범위
            controller.Tick(0f, true, false, true); // 접지 상태에서 코요테 시간 복원
            controller.Tick(0.05f, false, true, true); // 지면 이탈 0.05초 뒤 점프 입력 처리

            Assert.IsTrue(controller.JumpedThisTick); // 코요테 시간 내 점프 실행 확인
            Assert.AreEqual(PlayerJumpState.Rising, controller.CurrentState); // 코요테 점프 상승 상태 확인
        } // 코요테 점프 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void ExpiredCoyoteTimeRejectsJump() // 허용 시간 종료 뒤 점프 차단 검증
        { // 코요테 만료 검증 범위
            controller.Tick(0f, true, false, true); // 접지 상태에서 코요테 시간 복원
            controller.Tick(CoyoteTime + 0.01f, false, false, true); // 코요테 시간보다 긴 공중 시간 처리
            controller.Tick(0f, false, true, true); // 만료 뒤 새 점프 입력 처리

            Assert.IsFalse(controller.JumpedThisTick); // 만료 뒤 점프 미실행 확인
            Assert.That(controller.CoyoteTimeRemaining, Is.EqualTo(0f).Within(ComparisonTolerance)); // 코요테 시간 만료 확인
            Assert.Greater(controller.JumpBufferTimeRemaining, 0f); // 착지 대기 입력 버퍼 유지 확인
        } // 코요테 만료 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void BufferedInputStartsJumpOnLanding() // 착지 직전 입력의 착지 점프 검증
        { // 점프 버퍼 검증 범위
            controller.Tick(0f, false, true, true); // 공중 상태의 새 점프 입력 저장
            controller.Tick(0.05f, false, false, true); // 입력 보관 시간 일부 경과
            controller.Tick(0f, true, false, true); // 입력 보관 시간 안의 착지 처리

            Assert.IsTrue(controller.JumpedThisTick); // 착지 프레임 점프 실행 확인
            Assert.AreEqual(PlayerJumpState.Rising, controller.CurrentState); // 착지 직후 상승 상태 확인
            Assert.That(controller.JumpBufferTimeRemaining, Is.EqualTo(0f).Within(ComparisonTolerance)); // 사용한 입력 버퍼 제거 확인
        } // 점프 버퍼 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void RestrictedPostureClearsBufferedInput() // 앉기 자세의 점프 입력과 버퍼 제거 검증
        { // 자세 제한 검증 범위
            controller.Tick(0f, false, true, false); // 점프 제한 자세의 새 입력 처리
            controller.Tick(0f, true, false, true); // 자세 복원 뒤 접지 처리

            Assert.IsFalse(controller.JumpedThisTick); // 제한 중 입력의 지연 실행 차단 확인
            Assert.That(controller.JumpBufferTimeRemaining, Is.EqualTo(0f).Within(ComparisonTolerance)); // 제한 중 입력 버퍼 제거 확인
            Assert.AreEqual(PlayerJumpState.Grounded, controller.CurrentState); // 자세 복원 뒤 지상 상태 확인
        } // 자세 제한 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void GravityStopsAtMaximumFallSpeed() // 중력의 최대 낙하 속도 제한 검증
        { // 최대 낙하 검증 범위
            controller.Tick(10f, false, false, true); // 긴 공중 경과 시간 처리

            Assert.That(controller.VerticalVelocity, Is.EqualTo(-MaximumFallSpeed).Within(ComparisonTolerance)); // 최대 낙하 속도 제한 확인
            Assert.AreEqual(PlayerJumpState.Falling, controller.CurrentState); // 하강 상태 확인
        } // 최대 낙하 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void CeilingCollisionCancelsUpwardVelocity() // 천장 충돌의 상승 속도 제거 검증
        { // 천장 충돌 검증 범위
            controller.Tick(0f, true, true, true); // 접지 상태 기본 점프 실행
            controller.CancelUpwardVelocity(); // 천장 충돌 상승 취소 처리

            Assert.That(controller.VerticalVelocity, Is.EqualTo(0f).Within(ComparisonTolerance)); // 상승 속도 제거 확인
            Assert.AreEqual(PlayerJumpState.Falling, controller.CurrentState); // 천장 충돌 뒤 하강 상태 확인
        } // 천장 충돌 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void ResetAfterJumpRestoresGroundedState() // 부활 초기화의 점프 상태 복원 검증
        { // 점프 초기화 검증 범위
            controller.Tick(0f, true, true, true); // 점프 실행 상태 생성
            controller.Reset(); // 점프와 중력 상태 초기화

            Assert.AreEqual(PlayerJumpState.Grounded, controller.CurrentState); // 지상 상태 복원 확인
            Assert.That(controller.VerticalVelocity, Is.EqualTo(GroundedGravity).Within(ComparisonTolerance)); // 접지 유지 속도 복원 확인
            Assert.That(controller.CoyoteTimeRemaining, Is.EqualTo(0f).Within(ComparisonTolerance)); // 코요테 시간 제거 확인
            Assert.That(controller.JumpBufferTimeRemaining, Is.EqualTo(0f).Within(ComparisonTolerance)); // 점프 입력 버퍼 제거 확인
            Assert.IsFalse(controller.JumpedThisTick); // 점프 실행 결과 제거 확인
        } // 점프 초기화 검증 범위 종료
    } // 점프 테스트 범위 종료
} // EditMode 테스트 범위 종료
