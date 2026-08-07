using NUnit.Framework; // EditMode 테스트 기능 참조
using ProjectJ.Player; // 플레이어 올라오기 기능 참조
using UnityEngine; // Unity 벡터 기능 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{ // 끝자락 올라오기 테스트 범위
    public sealed class PlayerLedgeClimbControllerTests // 끝자락 올라오기 경로 규칙 테스트 선언
    { // 올라오기 경로 테스트 범위
        private const float ComparisonTolerance = 0.0001f; // 부동소수점 비교 허용 오차

        [Test] // Unity Test Runner 테스트 지정
        public void HeightInsideRangeIsReachable() // 허용 범위 안쪽 끝자락 높이 판정 검증
        { // 허용 높이 검증 범위
            bool result = PlayerLedgeClimbController.IsHeightReachable(1f, 2.5f, 0.35f, 2.2f); // 높이 차이 1.5미터 판정 실행

            Assert.IsTrue(result); // 허용 범위 높이 판정 확인
        } // 허용 높이 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void HeightAboveMaximumIsRejected() // 최대 높이를 넘는 끝자락 판정 검증
        { // 최대 높이 검증 범위
            bool result = PlayerLedgeClimbController.IsHeightReachable(1f, 3.5f, 0.35f, 2.2f); // 높이 차이 2.5미터 판정 실행

            Assert.IsFalse(result); // 최대 높이 초과 판정 확인
        } // 최대 높이 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void InputTowardWallPassesApproachCheck() // 벽을 향하는 입력 판정 검증
        { // 벽 접근 입력 검증 범위
            bool result = PlayerLedgeClimbController.IsApproachingWall(Vector3.forward, Vector3.back, 0.5f); // 전방 입력과 바깥 벽 법선 판정 실행

            Assert.IsTrue(result); // 벽 접근 입력 판정 확인
        } // 벽 접근 입력 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void InputAwayFromWallFailsApproachCheck() // 벽 반대 방향 입력 판정 검증
        { // 벽 이탈 입력 검증 범위
            bool result = PlayerLedgeClimbController.IsApproachingWall(Vector3.back, Vector3.back, 0.5f); // 후방 입력과 바깥 벽 법선 판정 실행

            Assert.IsFalse(result); // 벽 이탈 입력 판정 확인
        } // 벽 이탈 입력 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void BeginCreatesLiftingAndLandingTargets() // 올라오기 시작의 두 단계 목표 생성 검증
        { // 올라오기 목표 생성 범위
            PlayerLedgeClimbController controller = new PlayerLedgeClimbController(); // 테스트용 올라오기 제어기 생성
            bool started = controller.TryBegin(Vector3.zero, new Vector3(0f, 1.5f, 0f), Vector3.back, 0.5f, 0.55f, 0.05f, 0.3f, 0.2f); // 끝자락 감지 결과 기반 시작 시도

            Assert.IsTrue(started); // 올라오기 시작 성공 확인
            Assert.That(controller.CurrentState, Is.EqualTo(PlayerLedgeClimbState.Lifting)); // 몸 올리기 상태 적용 확인
            Assert.That(controller.LiftPosition.y, Is.EqualTo(1.55f).Within(ComparisonTolerance)); // 몸 올리기 목표 높이 확인
            Assert.That(controller.LandingPosition.z, Is.EqualTo(0.55f).Within(ComparisonTolerance)); // 벽 안쪽 착지 거리 확인
        } // 올라오기 목표 생성 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void LiftingTickMovesPlayerUpward() // 몸 올리기 단계의 상승 이동 검증
        { // 몸 올리기 이동 검증 범위
            PlayerLedgeClimbController controller = CreateStartedController(); // 시작된 테스트 제어기 생성
            Vector3 movement = controller.Tick(0.15f, Vector3.zero); // 몸 올리기 단계 절반 이동 계산

            Assert.Greater(movement.y, 0f); // 위쪽 이동량 존재 확인
            Assert.That(controller.CurrentState, Is.EqualTo(PlayerLedgeClimbState.Lifting)); // 몸 올리기 상태 유지 확인
        } // 몸 올리기 이동 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void LiftingCompletionStartsAdvancing() // 몸 올리기 완료 뒤 발판 진입 전환 검증
        { // 발판 진입 전환 검증 범위
            PlayerLedgeClimbController controller = CreateStartedController(); // 시작된 테스트 제어기 생성
            controller.Tick(0.3f, Vector3.zero); // 몸 올리기 단계 전체 시간 진행

            Assert.That(controller.CurrentState, Is.EqualTo(PlayerLedgeClimbState.Advancing)); // 발판 안쪽 이동 상태 전환 확인
            Assert.IsFalse(controller.CompletedThisTick); // 최종 올라오기 미완료 확인
        } // 발판 진입 전환 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void AdvancingCompletionReturnsIdle() // 발판 진입 완료 뒤 대기 상태 복귀 검증
        { // 올라오기 완료 검증 범위
            PlayerLedgeClimbController controller = CreateStartedController(); // 시작된 테스트 제어기 생성
            Vector3 liftMovement = controller.Tick(0.3f, Vector3.zero); // 몸 올리기 단계 완료 이동 계산
            controller.Tick(0.2f, liftMovement); // 발판 진입 단계 완료 이동 계산

            Assert.That(controller.CurrentState, Is.EqualTo(PlayerLedgeClimbState.Idle)); // 대기 상태 복귀 확인
            Assert.IsTrue(controller.CompletedThisTick); // 현재 갱신 완료 결과 확인
        } // 올라오기 완료 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void CancelClearsActiveClimb() // 진행 중 올라오기 취소 상태 검증
        { // 올라오기 취소 검증 범위
            PlayerLedgeClimbController controller = CreateStartedController(); // 시작된 테스트 제어기 생성
            controller.Cancel(); // 진행 중 올라오기 취소 실행

            Assert.That(controller.CurrentState, Is.EqualTo(PlayerLedgeClimbState.Idle)); // 대기 상태 복귀 확인
            Assert.IsFalse(controller.IsClimbing); // 올라오기 진행 상태 해제 확인
            Assert.That(controller.LandingPosition, Is.EqualTo(Vector3.zero)); // 최종 착지 목표 제거 확인
        } // 올라오기 취소 검증 범위 종료

        private static PlayerLedgeClimbController CreateStartedController() // 공통 시작 상태의 테스트 제어기 생성
        { // 테스트 제어기 생성 범위
            PlayerLedgeClimbController controller = new PlayerLedgeClimbController(); // 새 올라오기 제어기 생성
            controller.TryBegin(Vector3.zero, new Vector3(0f, 1.5f, 0f), Vector3.back, 0.5f, 0.55f, 0.05f, 0.3f, 0.2f); // 공통 끝자락 경로 시작
            return controller; // 시작된 테스트 제어기 반환
        } // 테스트 제어기 생성 범위 종료
    } // 올라오기 경로 테스트 범위 종료
} // 끝자락 올라오기 테스트 범위 종료
