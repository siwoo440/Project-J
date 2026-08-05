using NUnit.Framework; // EditMode 테스트 기능 참조
using ProjectJ.Player; // 플레이어 외부 힘 요청 기능 참조
using UnityEngine; // Unity 벡터 기능 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{ // 외부 힘 요청 테스트 범위
    public sealed class ExternalForceRequestTests // 외부 힘 원인과 결합 방식 테스트 선언
    { // 외부 힘 요청 테스트 범위
        private const float ComparisonTolerance = 0.0001f; // 부동소수점 비교 허용 오차

        [Test] // Unity Test Runner 테스트 지정
        public void PushUsesHorizontalReplacementAndImmunity() // 밀치기 요청의 수평 교체와 면역 설정 검증
        { // 밀치기 요청 검증 범위
            ExternalForceRequest request = ExternalForceRequest.CreatePush(new Vector3(1f, 1f, 0f), 6f); // 수직 성분이 포함된 밀치기 요청 생성

            Assert.That(request.Velocity.y, Is.EqualTo(0f).Within(ComparisonTolerance)); // 밀치기 수직 성분 제거 확인
            Assert.That(request.Velocity.magnitude, Is.EqualTo(6f).Within(ComparisonTolerance)); // 밀치기 속력 적용 확인
            Assert.That(request.Source, Is.EqualTo(ExternalForceSource.Push)); // 밀치기 원인 저장 확인
            Assert.That(request.Application, Is.EqualTo(ExternalForceApplication.ReplaceImpulse)); // 순간 힘 교체 방식 확인
            Assert.IsTrue(request.StartsHitImmunity); // 밀치기 면역 시작 확인
        } // 밀치기 요청 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void PlatformPreservesFullCarrierVelocity() // 이동 발판 요청의 전체 전달 속도 보존 검증
        { // 발판 요청 검증 범위
            Vector3 velocity = new Vector3(2f, 1f, -3f); // 테스트용 발판 속도 생성
            ExternalForceRequest request = ExternalForceRequest.CreatePlatform(velocity); // 발판 전달 속도 요청 생성

            Assert.That(request.Velocity, Is.EqualTo(velocity)); // 전체 발판 속도 보존 확인
            Assert.That(request.Source, Is.EqualTo(ExternalForceSource.Platform)); // 발판 원인 저장 확인
            Assert.That(request.Application, Is.EqualTo(ExternalForceApplication.SetCarrierVelocity)); // 전달 속도 갱신 방식 확인
            Assert.IsFalse(request.StartsHitImmunity); // 발판 이동의 피격 면역 미사용 확인
        } // 발판 요청 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void ObstacleUsesAdditiveImpulseWithoutImmunity() // 장애물 요청의 누적 순간 힘 설정 검증
        { // 장애물 요청 검증 범위
            ExternalForceRequest request = ExternalForceRequest.CreateObstacle(Vector3.up, 8f); // 위쪽 장애물 순간 힘 요청 생성

            Assert.That(request.Velocity, Is.EqualTo(Vector3.up * 8f)); // 장애물 방향과 속력 적용 확인
            Assert.That(request.Source, Is.EqualTo(ExternalForceSource.Obstacle)); // 장애물 원인 저장 확인
            Assert.That(request.Application, Is.EqualTo(ExternalForceApplication.AddImpulse)); // 순간 힘 누적 방식 확인
            Assert.IsFalse(request.StartsHitImmunity); // 장애물 힘의 피격 면역 미사용 확인
        } // 장애물 요청 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void NegativeForceProducesZeroVelocity() // 음수 힘 입력의 영 속도 보정 검증
        { // 음수 힘 보정 검증 범위
            ExternalForceRequest pushRequest = ExternalForceRequest.CreatePush(Vector3.forward, -1f); // 음수 밀치기 요청 생성
            ExternalForceRequest obstacleRequest = ExternalForceRequest.CreateObstacle(Vector3.up, -1f); // 음수 장애물 요청 생성

            Assert.That(pushRequest.Velocity, Is.EqualTo(Vector3.zero)); // 음수 밀치기 영 속도 보정 확인
            Assert.That(obstacleRequest.Velocity, Is.EqualTo(Vector3.zero)); // 음수 장애물 영 속도 보정 확인
        } // 음수 힘 보정 검증 범위 종료
    } // 외부 힘 요청 테스트 범위 종료
} // 외부 힘 요청 테스트 범위 종료
