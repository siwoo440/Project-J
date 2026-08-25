using NUnit.Framework; // Unity EditMode 테스트 기능
using ProjectJ.Items; // 망치 정책 사용

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스
{
    public sealed class ProjectJHammerPolicyTests // 망치 정책 테스트
    {
        [Test] // 지속 시간 검증
        public void DurationSeconds_ReturnsSixSeconds()
        {
            Assert.AreEqual(6f, ProjectJHammerPolicy.DurationSeconds); // 6초 지속 시간 확인
        }

        [Test] // 망치 사거리 검증
        public void HammerPushRange_ReturnsThreePointTwoMeters()
        {
            Assert.AreEqual(3.2f, ProjectJHammerPolicy.HammerPushRangeMeters); // 3.2m 사거리 확인
        }

        [Test] // 망치 외력 검증
        public void HammerPushForce_ReturnsElevenMetersPerSecond()
        {
            Assert.AreEqual(11f, ProjectJHammerPolicy.HammerPushForceMetersPerSecond); // 11m/s 외력 확인
        }

        [Test] // 망치 재사용 시간 검증
        public void HammerPushCooldown_ReturnsOnePointFourSeconds()
        {
            Assert.AreEqual(1.4f, ProjectJHammerPolicy.HammerPushCooldownSeconds); // 1.4초 재사용 시간 확인
        }

        [TestCase(false, true)] // 비활성 상태 사용 허용
        [TestCase(true, false)] // 이미 활성 상태 중첩 차단
        public void CanActivate_WithCurrentState_ReturnsExpected( // 중첩 사용 규칙 검증
            bool isAlreadyActive, // 현재 활성 상태
            bool expected // 예상 사용 가능 여부
        )
        {
            Assert.AreEqual( // 예상 결과 비교
                expected, // 예상값 전달
                ProjectJHammerPolicy.CanActivate(isAlreadyActive) // 실제 정책 계산
            );
        }

        [TestCase(2.5f, false, 2.5f)] // 비활성 기존 사거리 유지
        [TestCase(2.5f, true, 3.2f)] // 활성 망치 사거리 적용
        [TestCase(-2f, false, 0f)] // 잘못된 음수 기본값 보정
        public void ResolvePushRange_WithState_ReturnsExpected( // Push 사거리 정책 검증
            float baseRange, // 기존 사거리
            bool isHammerActive, // 망치 활성 상태
            float expected // 예상 최종 사거리
        )
        {
            float range = ProjectJHammerPolicy.ResolvePushRange( // 최종 사거리 계산
                baseRange, // 기존값 전달
                isHammerActive // 활성 상태 전달
            );

            Assert.AreEqual(expected, range, 0.0001f); // 계산 결과 검증
        }

        [TestCase(12f, false, 12f)] // 비활성 기존 외력 유지
        [TestCase(12f, true, 11f)] // 활성 망치 데이터값 적용
        [TestCase(-2f, false, 0f)] // 잘못된 음수 기본값 보정
        public void ResolvePushForce_WithState_ReturnsExpected( // Push 외력 정책 검증
            float baseForce, // 기존 외력
            bool isHammerActive, // 망치 활성 상태
            float expected // 예상 최종 외력
        )
        {
            float force = ProjectJHammerPolicy.ResolvePushForce( // 최종 외력 계산
                baseForce, // 기존값 전달
                isHammerActive // 활성 상태 전달
            );

            Assert.AreEqual(expected, force, 0.0001f); // 계산 결과 검증
        }

        [TestCase(1.5f, false, 1.5f)] // 비활성 기존 재사용 시간 유지
        [TestCase(1.5f, true, 1.4f)] // 활성 망치 재사용 시간 적용
        [TestCase(-2f, false, 0f)] // 잘못된 음수 기본값 보정
        public void ResolvePushCooldown_WithState_ReturnsExpected( // Push 재사용 시간 정책 검증
            float baseCooldown, // 기존 재사용 시간
            bool isHammerActive, // 망치 활성 상태
            float expected // 예상 최종 재사용 시간
        )
        {
            float cooldown = ProjectJHammerPolicy.ResolvePushCooldown( // 최종 재사용 시간 계산
                baseCooldown, // 기존값 전달
                isHammerActive // 활성 상태 전달
            );

            Assert.AreEqual(expected, cooldown, 0.0001f); // 계산 결과 검증
        }
    }
}
