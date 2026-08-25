using NUnit.Framework; // Unity EditMode 테스트 기능
using ProjectJ.Items; // 복어 풍선옷 정책 사용

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스
{
    public sealed class ProjectJPufferBalloonSuitPolicyTests // 복어 풍선옷 정책 테스트
    {
        [Test] // 지속 시간 검증
        public void DurationSeconds_ReturnsFiveSeconds()
        {
            Assert.AreEqual(5f, ProjectJPufferBalloonSuitPolicy.DurationSeconds); // 5초 지속 확인
        }

        [Test] // 감지 반경 검증
        public void DetectionRadius_ReturnsOnePointTwoMeters()
        {
            Assert.AreEqual(1.2f, ProjectJPufferBalloonSuitPolicy.DetectionRadiusMeters); // 1.2m 확인
        }

        [Test] // 밀치기 외력 검증
        public void PushSpeed_ReturnsSixMetersPerSecond()
        {
            Assert.AreEqual(6f, ProjectJPufferBalloonSuitPolicy.PushSpeedMetersPerSecond); // 6m/s 확인
        }

        [Test] // 대상별 재발동 시간 검증
        public void PerTargetCooldown_ReturnsOneSecond()
        {
            Assert.AreEqual(1f, ProjectJPufferBalloonSuitPolicy.PerTargetCooldownSeconds); // 1초 확인
        }

        [TestCase(false, true, true, true)] // 정상 사용 허용
        [TestCase(true, true, true, false)] // 이미 활성 상태 차단
        [TestCase(false, false, true, false)] // 경기 입력 잠금 차단
        [TestCase(false, true, false, false)] // Runner 준비 실패 차단
        public void CanActivate_WithCurrentState_ReturnsExpected(
            bool isAlreadyActive,
            bool gameplayAllowed,
            bool runnerReady,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJPufferBalloonSuitPolicy.CanActivate(
                    isAlreadyActive,
                    gameplayAllowed,
                    runnerReady
                )
            );
        }

        [TestCase(0f, true)] // 사용자와 같은 위치
        [TestCase(1.2f, true)] // 경계 포함
        [TestCase(1.2001f, false)] // 경계 밖 제외
        [TestCase(-1f, true)] // 음수 입력은 거리 절댓값 보정
        public void IsInsideDetectionRadius_WithDistance_ReturnsExpected(
            float distanceMeters,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJPufferBalloonSuitPolicy.IsInsideDetectionRadius(distanceMeters)
            );
        }

        [TestCase(false, false, true)] // 유효한 다른 대상
        [TestCase(true, false, false)] // 자기 자신 제외
        [TestCase(false, true, false)] // 대상 쿨타임 중 제외
        public void CanTriggerTarget_WithState_ReturnsExpected(
            bool isSelf,
            bool isTargetCooldownActive,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJPufferBalloonSuitPolicy.CanTriggerTarget(
                    isSelf,
                    isTargetCooldownActive
                )
            );
        }
    }
}
