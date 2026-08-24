using NUnit.Framework; // Unity EditMode 테스트 기능
using ProjectJ.Items; // 풀 공 공통 정책 사용

namespace ProjectJ.Tests.EditMode // Project J EditMode 테스트 네임스페이스
{
    public sealed class ProjectJPoolBallPolicyTests // 111일차 풀 공 정책 테스트
    {
        [Test] // 최대 Stack 수량 검증
        public void MaximumStackCount_IsFive()
        {
            Assert.That(ProjectJPoolBallPolicy.MaximumStackCount, Is.EqualTo(5)); // 한 슬롯 최대 5개 확인
        }

        [TestCase(0, 1)] // 빈 Stack 획득
        [TestCase(1, 2)] // 1개에서 2개 획득
        [TestCase(4, 5)] // 4개에서 최대치 획득
        [TestCase(5, 5)] // 최대치 초과 차단
        [TestCase(8, 5)] // 잘못된 초과 값 보정
        public void AddOne_ClampsToMaximum(int currentCount, int expectedCount)
        {
            int actualCount = ProjectJPoolBallPolicy.AddOne(currentCount); // Pickup 1개 합산 결과 계산
            Assert.That(actualCount, Is.EqualTo(expectedCount)); // 예상 Stack 수량 확인
        }

        [TestCase(5, 4)] // 최대 Stack 1개 소비
        [TestCase(2, 1)] // 중간 Stack 1개 소비
        [TestCase(1, 0)] // 마지막 1개 소비
        [TestCase(0, 0)] // 빈 Stack 음수 차단
        [TestCase(-3, 0)] // 잘못된 음수 값 보정
        public void ConsumeOne_DecreasesExactlyOne(int currentCount, int expectedCount)
        {
            int actualCount = ProjectJPoolBallPolicy.ConsumeOne(currentCount); // 투척 1회 소비 결과 계산
            Assert.That(actualCount, Is.EqualTo(expectedCount)); // 예상 남은 수량 확인
        }

        [TestCase(0, true)] // 빈 Stack 합산 가능
        [TestCase(4, true)] // 4개 Stack 합산 가능
        [TestCase(5, false)] // 최대 Stack 합산 차단
        [TestCase(9, false)] // 잘못된 초과 값 합산 차단
        public void CanAddOne_RejectsMaximumStack(int currentCount, bool expected)
        {
            bool actual = ProjectJPoolBallPolicy.CanAddOne(currentCount); // Pickup 합산 가능 여부 계산
            Assert.That(actual, Is.EqualTo(expected)); // 최대 Stack 규칙 확인
        }

        [TestCase(0, false)] // 빈 Stack 투척 차단
        [TestCase(1, true)] // 1개 Stack 투척 가능
        [TestCase(5, true)] // 최대 Stack 투척 가능
        [TestCase(-1, false)] // 잘못된 음수 Stack 차단
        public void CanConsumeOne_RequiresPositiveStack(int currentCount, bool expected)
        {
            bool actual = ProjectJPoolBallPolicy.CanConsumeOne(currentCount); // 투척 가능 여부 계산
            Assert.That(actual, Is.EqualTo(expected)); // 보유 수량 조건 확인
        }

        [Test] // 기획 수치 검증
        public void ProjectileValues_MatchPlannedPrototype()
        {
            Assert.That(ProjectJPoolBallPolicy.HitForce, Is.EqualTo(4f)); // 기획 외력 4 확인
            Assert.That(ProjectJPoolBallPolicy.MaximumTravelDistance, Is.EqualTo(28f)); // 기획 사거리 28m 확인
            Assert.That(ProjectJPoolBallPolicy.CollisionRadius, Is.EqualTo(0.24f)); // 기획 반경 0.24m 확인
            Assert.That(ProjectJPoolBallPolicy.ProjectileSpeed, Is.EqualTo(16f)); // 기획 속도 16m/s 확인
        }

        [TestCase(27.99f, false)] // 최대 거리 직전 유지
        [TestCase(28f, true)] // 정확한 최대 거리 제거
        [TestCase(40f, true)] // 최대 거리 초과 제거
        public void HasReachedTravelLimit_UsesTwentyEightMeters(float distance, bool expected)
        {
            bool actual = ProjectJPoolBallPolicy.HasReachedTravelLimit(distance); // 최대 이동 거리 판정
            Assert.That(actual, Is.EqualTo(expected)); // 28m 기준 확인
        }
    }
}
