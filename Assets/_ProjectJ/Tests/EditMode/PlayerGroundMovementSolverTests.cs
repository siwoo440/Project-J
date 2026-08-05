using NUnit.Framework; // EditMode 테스트 기능 참조
using ProjectJ.Player; // 지상 이동 계산 기능 참조
using UnityEngine; // Unity 벡터 기능 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{ // 네임스페이스 범위 시작
    public sealed class PlayerGroundMovementSolverTests // 지상 가속과 감속과 방향 전환 테스트 선언
    { // 클래스 범위 시작
        private const float ComparisonTolerance = 0.0001f; // 부동소수점 비교 허용 오차

        [Test] // Unity Test Runner 테스트 지정
        public void ForwardInputUsesGroundAcceleration() // 정지 상태에서 일반 지상 가속도 적용 여부 검증
        { // 메서드 범위 시작
            Vector3 currentVelocity = Vector3.zero; // 초기 정지 속도 설정
            Vector3 targetVelocity = Vector3.forward * 6f; // 전방 목표 속도 설정
            Vector3 nextVelocity = PlayerGroundMovementSolver.CalculateNextVelocity(currentVelocity, targetVelocity, 24f, 30f, 0.1f); // 0.1초 뒤 수평 속도 계산

            Assert.That(nextVelocity.z, Is.EqualTo(2.4f).Within(ComparisonTolerance)); // 일반 가속도 기반 전방 속도 확인
        } // 메서드 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void ReleasedInputUsesGroundDeceleration() // 이동 입력 해제 시 지상 감속도 적용 여부 검증
        { // 메서드 범위 시작
            Vector3 currentVelocity = Vector3.forward * 6f; // 초기 전방 이동 속도 설정
            Vector3 targetVelocity = Vector3.zero; // 정지 목표 속도 설정
            Vector3 nextVelocity = PlayerGroundMovementSolver.CalculateNextVelocity(currentVelocity, targetVelocity, 24f, 30f, 0.1f); // 0.1초 뒤 수평 속도 계산

            Assert.That(nextVelocity.z, Is.EqualTo(3f).Within(ComparisonTolerance)); // 감속도 기반 남은 전방 속도 확인
        } // 메서드 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void OppositeInputUsesCombinedDirectionChangeAcceleration() // 반대 입력 시 감속과 가속을 합친 응답 적용 여부 검증
        { // 메서드 범위 시작
            Vector3 currentVelocity = Vector3.forward * 6f; // 초기 전방 이동 속도 설정
            Vector3 targetVelocity = Vector3.back * 6f; // 후방 목표 속도 설정
            Vector3 nextVelocity = PlayerGroundMovementSolver.CalculateNextVelocity(currentVelocity, targetVelocity, 24f, 30f, 0.1f); // 0.1초 뒤 방향 전환 속도 계산

            Assert.That(nextVelocity.z, Is.EqualTo(0.6f).Within(ComparisonTolerance)); // 합산 응답으로 빠르게 감소한 전방 속도 확인
        } // 메서드 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void SecondOppositeInputStepCrossesIntoNewDirection() // 반대 입력 유지 시 빠른 새 방향 진입 여부 검증
        { // 메서드 범위 시작
            Vector3 firstVelocity = Vector3.forward * 6f; // 초기 전방 이동 속도 설정
            Vector3 targetVelocity = Vector3.back * 6f; // 후방 목표 속도 설정
            Vector3 secondVelocity = PlayerGroundMovementSolver.CalculateNextVelocity(firstVelocity, targetVelocity, 24f, 30f, 0.1f); // 첫 번째 방향 전환 프레임 계산
            Vector3 thirdVelocity = PlayerGroundMovementSolver.CalculateNextVelocity(secondVelocity, targetVelocity, 24f, 30f, 0.1f); // 두 번째 방향 전환 프레임 계산

            Assert.Less(thirdVelocity.z, 0f); // 두 번째 계산에서 후방 이동 진입 여부 확인
        } // 메서드 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void SideInputIsNotClassifiedAsOpposite() // 직각 측면 입력의 반대 방향 오판 방지 여부 검증
        { // 메서드 범위 시작
            bool isOpposite = PlayerGroundMovementSolver.IsOppositeDirection(Vector3.forward, Vector3.right); // 전방 속도와 오른쪽 목표 방향 비교

            Assert.IsFalse(isOpposite); // 직각 방향의 일반 가속 처리 여부 확인
        } // 메서드 범위 종료
    } // 클래스 범위 종료
} // 네임스페이스 범위 종료
