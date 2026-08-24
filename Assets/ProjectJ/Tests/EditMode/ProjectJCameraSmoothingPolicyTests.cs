using NUnit.Framework; // Unity EditMode 테스트 기능
using ProjectJ.CameraSystem; // 카메라 보간 정책 사용
using UnityEngine; // Vector3 수학 기능

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스
{
    public sealed class ProjectJCameraSmoothingPolicyTests // 카메라 위치 보간 정책 테스트
    {
        [Test] // 프레임별 보간 결과 검증
        public void CalculateNextPosition_MovesTowardTargetWithoutOvershoot() // 목표 위치 방향 이동과 초과 방지 확인
        {
            Vector3 currentPosition = // 현재 카메라 위치
                Vector3.zero; // 원점 위치 사용

            Vector3 targetPosition = // 목표 카메라 위치
                new Vector3(10f, 0f, 0f); // X축 10미터 위치 사용

            Vector3 nextPosition = // 다음 카메라 위치 계산
                ProjectJCameraSmoothingPolicy.CalculateNextPosition( // 프레임 독립 보간 호출
                    currentPosition, // 현재 위치 전달
                    targetPosition, // 목표 위치 전달
                    10f, // 초당 보간 속도 전달
                    0.1f // 프레임 시간 전달
                );

            Assert.That( // 계산 위치 오차 검증
                nextPosition.x, // 계산된 X 위치
                Is.EqualTo(6.321206f).Within(0.00001f) // 지수 보간 예상값 비교
            );

            Assert.That( // 목표 위치 초과 여부 검증
                nextPosition.x, // 계산된 X 위치
                Is.LessThan(targetPosition.x) // 목표 위치 미만 확인
            );
        }

        [Test] // 서로 다른 FPS 결과 검증
        public void CalculateNextPosition_ProducesSameResultAcrossFrameRates() // 30FPS와 60FPS 보간 차이 방지
        {
            Vector3 targetPosition = // 공통 목표 위치
                new Vector3(10f, 3f, -5f); // 세 축 목표 위치 사용

            Vector3 thirtyFpsPosition = // 30FPS 누적 위치
                Vector3.zero; // 원점에서 시작

            Vector3 sixtyFpsPosition = // 60FPS 누적 위치
                Vector3.zero; // 원점에서 시작

            for (int index = 0; index < 30; index++) // 30FPS 1초 반복
            {
                thirtyFpsPosition = // 30FPS 다음 위치 저장
                    ProjectJCameraSmoothingPolicy.CalculateNextPosition( // 프레임 독립 보간 호출
                        thirtyFpsPosition, // 현재 누적 위치 전달
                        targetPosition, // 공통 목표 위치 전달
                        8f, // 동일 보간 속도 전달
                        1f / 30f // 30FPS 프레임 시간 전달
                    );
            }

            for (int index = 0; index < 60; index++) // 60FPS 1초 반복
            {
                sixtyFpsPosition = // 60FPS 다음 위치 저장
                    ProjectJCameraSmoothingPolicy.CalculateNextPosition( // 프레임 독립 보간 호출
                        sixtyFpsPosition, // 현재 누적 위치 전달
                        targetPosition, // 공통 목표 위치 전달
                        8f, // 동일 보간 속도 전달
                        1f / 60f // 60FPS 프레임 시간 전달
                    );
            }

            Assert.That( // FPS별 위치 차이 검증
                Vector3.Distance( // 두 누적 위치 거리 계산
                    thirtyFpsPosition, // 30FPS 결과 위치
                    sixtyFpsPosition // 60FPS 결과 위치
                ),
                Is.LessThan(0.0001f) // 허용 오차 미만 확인
            );
        }

        [Test] // 순간이동 경계값 검증
        public void ShouldSnap_AtThreshold_ReturnsTrue() // 경계 거리에서 즉시 이동 확인
        {
            bool shouldSnap = // 즉시 이동 여부 계산
                ProjectJCameraSmoothingPolicy.ShouldSnap( // 순간이동 판정 호출
                    Vector3.zero, // 현재 위치 전달
                    new Vector3(4f, 0f, 0f), // 4미터 목표 위치 전달
                    4f // 4미터 경계값 전달
                );

            Assert.IsTrue( // 즉시 이동 결과 검증
                shouldSnap // 실제 판정 결과
            );
        }

        [Test] // 일반 추적 거리 검증
        public void ShouldSnap_BelowThreshold_ReturnsFalse() // 짧은 거리에서 보간 유지 확인
        {
            bool shouldSnap = // 즉시 이동 여부 계산
                ProjectJCameraSmoothingPolicy.ShouldSnap( // 순간이동 판정 호출
                    Vector3.zero, // 현재 위치 전달
                    new Vector3(3.99f, 0f, 0f), // 경계 미만 목표 위치 전달
                    4f // 4미터 경계값 전달
                );

            Assert.IsFalse( // 보간 유지 결과 검증
                shouldSnap // 실제 판정 결과
            );
        }

        [Test] // 정지 시간 처리 검증
        public void CalculateNextPosition_ZeroDeltaTime_KeepsCurrentPosition() // 프레임 진행 없음 상태 유지 확인
        {
            Vector3 currentPosition = // 현재 카메라 위치
                new Vector3(1f, 2f, 3f); // 비교용 위치 사용

            Vector3 nextPosition = // 다음 카메라 위치 계산
                ProjectJCameraSmoothingPolicy.CalculateNextPosition( // 프레임 독립 보간 호출
                    currentPosition, // 현재 위치 전달
                    new Vector3(9f, 8f, 7f), // 다른 목표 위치 전달
                    10f, // 보간 속도 전달
                    0f // 정지된 프레임 시간 전달
                );

            Assert.AreEqual( // 위치 유지 결과 검증
                currentPosition, // 예상 현재 위치
                nextPosition // 실제 계산 위치
            );
        }
    }
}
