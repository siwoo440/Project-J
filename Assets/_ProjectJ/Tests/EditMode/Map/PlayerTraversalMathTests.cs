using NUnit.Framework; // EditMode 테스트 기능 참조
using ProjectJ.Player; // 플레이어 지형 계산 기능 참조
using UnityEngine; // Unity 벡터와 수학 기능 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{ // EditMode 테스트 범위
    public sealed class PlayerTraversalMathTests // 공중 제어와 지형 보정 규칙 테스트 선언
    { // 지형 계산 테스트 범위
        private const float ComparisonTolerance = 0.0001f; // 부동소수점 비교 허용 오차

        [Test] // Unity Test Runner 테스트 지정
        public void AirWithoutInputPreservesHorizontalMomentum() // 입력 없는 공중 상태의 수평 관성 보존 검증
        { // 공중 관성 검증 범위
            Vector3 currentVelocity = new Vector3(8f, 4f, 0f); // 수직 속도가 포함된 현재 속도 생성
            Vector3 result = PlayerTraversalMath.CalculateAirVelocity(currentVelocity, Vector3.zero, 6f, 0.65f, 12f, 0.1f); // 입력 없는 공중 속도 계산

            Assert.That(result.x, Is.EqualTo(8f).Within(ComparisonTolerance)); // 기존 수평 속력 보존 확인
            Assert.That(result.y, Is.EqualTo(0f).Within(ComparisonTolerance)); // 수직 성분 제외 확인
            Assert.That(result.z, Is.EqualTo(0f).Within(ComparisonTolerance)); // 불필요한 측면 속도 없음 확인
        } // 공중 관성 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void AirFromRestUsesConfiguredControlRatio() // 정지 상태 공중 입력의 제어 비율 적용 검증
        { // 공중 시작 속도 검증 범위
            Vector3 result = PlayerTraversalMath.CalculateAirVelocity(Vector3.zero, Vector3.forward, 6f, 0.65f, 100f, 1f); // 충분한 가속도의 공중 속도 계산

            Assert.That(result.magnitude, Is.EqualTo(3.9f).Within(ComparisonTolerance)); // 지상 속도의 65퍼센트 적용 확인
            Assert.That(result.normalized.z, Is.EqualTo(1f).Within(ComparisonTolerance)); // 입력 방향 적용 확인
        } // 공중 시작 속도 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void AirDirectionChangeRespectsAccelerationLimit() // 공중 방향 전환의 가속도 제한 검증
        { // 공중 가속도 검증 범위
            Vector3 currentVelocity = Vector3.forward * 6f; // 기존 전방 관성 생성
            Vector3 result = PlayerTraversalMath.CalculateAirVelocity(currentVelocity, Vector3.right, 6f, 0.65f, 12f, 0.1f); // 오른쪽 입력의 공중 전환 계산

            Assert.That(Vector3.Distance(currentVelocity, result), Is.EqualTo(1.2f).Within(ComparisonTolerance)); // 한 프레임 최대 속도 변화량 확인
        } // 공중 가속도 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void WalkableSlopeInsideLimitReturnsTrue() // 제한 각도 이내 경사 이동 가능 판정 검증
        { // 이동 가능 경사 검증 범위
            Vector3 groundNormal = Quaternion.AngleAxis(30f, Vector3.right) * Vector3.up; // 30도 경사 법선 생성

            Assert.IsTrue(PlayerTraversalMath.IsWalkableSlope(groundNormal, 45f)); // 45도 제한 이내 판정 확인
        } // 이동 가능 경사 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void SteepSlopeOutsideLimitReturnsFalse() // 제한 각도를 넘는 경사 이동 불가 판정 검증
        { // 가파른 경사 검증 범위
            Vector3 groundNormal = Quaternion.AngleAxis(60f, Vector3.right) * Vector3.up; // 60도 경사 법선 생성

            Assert.IsFalse(PlayerTraversalMath.IsWalkableSlope(groundNormal, 45f)); // 45도 제한 초과 판정 확인
        } // 가파른 경사 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void GroundAlignmentPreservesMovementSpeed() // 경사 투영 뒤 이동 속력 보존 검증
        { // 경사 속력 검증 범위
            Vector3 groundNormal = Quaternion.AngleAxis(30f, Vector3.right) * Vector3.up; // 30도 경사 법선 생성
            Vector3 result = PlayerTraversalMath.AlignVelocityToGround(Vector3.forward * 6f, groundNormal, 45f); // 경사면 이동 속도 계산

            Assert.That(result.magnitude, Is.EqualTo(6f).Within(ComparisonTolerance)); // 기존 이동 속력 보존 확인
            Assert.That(Vector3.Dot(result, groundNormal), Is.EqualTo(0f).Within(ComparisonTolerance)); // 경사면과 평행한 속도 확인
        } // 경사 속력 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void SteepGroundDoesNotChangeVelocity() // 가파른 경사의 속도 투영 제외 검증
        { // 가파른 경사 속도 검증 범위
            Vector3 originalVelocity = Vector3.forward * 6f; // 기존 이동 속도 생성
            Vector3 groundNormal = Quaternion.AngleAxis(60f, Vector3.right) * Vector3.up; // 60도 경사 법선 생성
            Vector3 result = PlayerTraversalMath.AlignVelocityToGround(originalVelocity, groundNormal, 45f); // 가파른 경사 속도 계산

            Assert.That(result, Is.EqualTo(originalVelocity)); // 기존 이동 속도 유지 확인
        } // 가파른 경사 속도 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void CornerCorrectionMovesDirectionAlongWall() // 대각선 입력의 벽 표면 방향 보정 검증
        { // 모서리 보정 검증 범위
            Vector3 desiredDirection = new Vector3(1f, 0f, 1f).normalized; // 벽을 향하는 대각선 입력 생성
            Vector3 wallNormal = Vector3.back; // 전방 벽 법선 생성
            Vector3 result = PlayerTraversalMath.CalculateCornerCorrectedDirection(desiredDirection, wallNormal, 1f); // 최대 강도의 모서리 방향 계산

            Assert.That(result.x, Is.EqualTo(1f).Within(ComparisonTolerance)); // 벽을 따르는 오른쪽 방향 확인
            Assert.That(result.z, Is.EqualTo(0f).Within(ComparisonTolerance)); // 벽을 향하는 전방 성분 제거 확인
        } // 모서리 보정 검증 범위 종료
    } // 지형 계산 테스트 범위 종료
} // EditMode 테스트 범위 종료
