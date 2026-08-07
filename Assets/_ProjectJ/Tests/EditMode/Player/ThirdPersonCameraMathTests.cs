using NUnit.Framework; // EditMode 테스트 기능 참조
using ProjectJ.Player; // 플레이어 카메라 계산 기능 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{ // 3인칭 카메라 계산 테스트 범위
    public sealed class ThirdPersonCameraMathTests // 카메라 충돌 거리와 시야각 계산 테스트 선언
    { // 카메라 계산 테스트 기능 범위
        private const float ComparisonTolerance = 0.0001f; // 부동소수점 비교 허용 오차

        [Test] // Unity Test Runner 테스트 지정
        public void CollisionDistanceSubtractsPadding() // 벽 충돌 거리에서 여유를 빼는지 검증
        { // 충돌 여유 검증 범위
            float result = ThirdPersonCameraMath.CalculateCollisionDistance(3f, 0.1f, 0.35f, 5f); // 충돌 거리와 여유 기반 안전 거리 계산

            Assert.That(result, Is.EqualTo(2.9f).Within(ComparisonTolerance)); // 충돌 여유만큼 앞쪽 거리 적용 확인
        } // 충돌 여유 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void CollisionDistanceNeverFallsBelowMinimum() // 벽이 피벗에 가까울 때 최소 거리 보장 검증
        { // 최소 거리 검증 범위
            float result = ThirdPersonCameraMath.CalculateCollisionDistance(0.1f, 0.08f, 0.35f, 5f); // 최소 거리보다 가까운 충돌 계산

            Assert.That(result, Is.EqualTo(0.35f).Within(ComparisonTolerance)); // 설정된 최소 거리 유지 확인
        } // 최소 거리 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void CollisionDistanceNeverExceedsMaximum() // 잘못된 큰 충돌 거리의 최대 거리 제한 검증
        { // 최대 거리 검증 범위
            float result = ThirdPersonCameraMath.CalculateCollisionDistance(10f, 0f, 0.35f, 5f); // 기본 거리보다 큰 충돌 거리 계산

            Assert.That(result, Is.EqualTo(5f).Within(ComparisonTolerance)); // 기본 최대 거리 제한 확인
        } // 최대 거리 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void CameraRetractsImmediatelyWhenWallGetsCloser() // 벽 접근 시 카메라 즉시 축소 검증
        { // 즉시 축소 검증 범위
            float result = ThirdPersonCameraMath.CalculateSmoothedDistance(5f, 2f, 8f, 0.016f); // 가까운 목표 거리로 갱신 계산

            Assert.That(result, Is.EqualTo(2f).Within(ComparisonTolerance)); // 관통 방지를 위한 즉시 축소 확인
        } // 즉시 축소 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void CameraRecoversByConfiguredSpeed() // 벽 이탈 시 설정 속도 기반 복귀 검증
        { // 거리 복귀 속도 검증 범위
            float result = ThirdPersonCameraMath.CalculateSmoothedDistance(2f, 5f, 8f, 0.25f); // 2미터 복귀 가능한 프레임 계산

            Assert.That(result, Is.EqualTo(4f).Within(ComparisonTolerance)); // 초당 복귀 속도 적용 확인
        } // 거리 복귀 속도 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void CameraRecoveryDoesNotOvershootTarget() // 벽 이탈 복귀의 목표 거리 초과 방지 검증
        { // 거리 초과 방지 검증 범위
            float result = ThirdPersonCameraMath.CalculateSmoothedDistance(4.9f, 5f, 100f, 1f); // 큰 복귀 속도 기반 거리 계산

            Assert.That(result, Is.EqualTo(5f).Within(ComparisonTolerance)); // 목표 거리 초과 없음 확인
        } // 거리 초과 방지 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void SprintUsesExpandedFieldOfView() // 달리기 상태의 확장 시야각 선택 검증
        { // 달리기 시야각 검증 범위
            float result = ThirdPersonCameraMath.CalculateTargetFieldOfView(true, 60f, 68f); // 달리기 목표 시야각 계산

            Assert.That(result, Is.EqualTo(68f).Within(ComparisonTolerance)); // 달리기 시야각 선택 확인
        } // 달리기 시야각 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void WalkingUsesNormalFieldOfView() // 일반 이동 상태의 기본 시야각 선택 검증
        { // 기본 시야각 검증 범위
            float result = ThirdPersonCameraMath.CalculateTargetFieldOfView(false, 60f, 68f); // 일반 이동 목표 시야각 계산

            Assert.That(result, Is.EqualTo(60f).Within(ComparisonTolerance)); // 기본 시야각 선택 확인
        } // 기본 시야각 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void FieldOfViewBlendMovesWithoutOvershoot() // 시야각 전환의 목표값 초과 방지 검증
        { // 시야각 전환 검증 범위
            float firstResult = ThirdPersonCameraMath.CalculateSmoothedFieldOfView(60f, 68f, 4f, 1f); // 1초 동안 시야각 확대 계산
            float secondResult = ThirdPersonCameraMath.CalculateSmoothedFieldOfView(firstResult, 68f, 100f, 1f); // 큰 속도의 나머지 시야각 전환 계산

            Assert.That(firstResult, Is.EqualTo(64f).Within(ComparisonTolerance)); // 설정 속도 기반 중간 시야각 확인
            Assert.That(secondResult, Is.EqualTo(68f).Within(ComparisonTolerance)); // 목표 시야각 초과 없음 확인
        } // 시야각 전환 검증 범위 종료
    } // 카메라 계산 테스트 기능 범위 종료
} // 3인칭 카메라 계산 테스트 범위 종료
