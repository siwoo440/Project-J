using NUnit.Framework; // Unity EditMode 테스트 기능
using ProjectJ.Items; // 낚시대 정책 사용
using UnityEngine; // Vector3 테스트 사용

namespace ProjectJ.Tests.EditMode
{
    public sealed class ProjectJFishingRodPolicyTests
    {
        [Test]
        public void PullDurationSeconds_ReturnsZeroPointSixSeconds()
        {
            Assert.AreEqual(
                0.6f,
                ProjectJFishingRodPolicy.PullDurationSeconds
            );
        }

        [Test]
        public void MaximumRangeMeters_ReturnsFourteenMeters()
        {
            Assert.AreEqual(
                14f,
                ProjectJFishingRodPolicy.MaximumRangeMeters
            );
        }

        [Test]
        public void PullSpeedMetersPerSecond_ReturnsEightMetersPerSecond()
        {
            Assert.AreEqual(
                8f,
                ProjectJFishingRodPolicy.PullSpeedMetersPerSecond
            );
        }

        [TestCase(0f, true)]
        [TestCase(13.999f, true)]
        [TestCase(14f, true)]
        [TestCase(14.001f, false)]
        [TestCase(-0.1f, false)]
        public void IsWithinRange_WithDistance_ReturnsExpected(
            float distanceMeters,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJFishingRodPolicy.IsWithinRange(distanceMeters)
            );
        }

        [TestCase(true, true, false, false, false, false, true)]
        [TestCase(false, true, false, false, false, false, false)]
        [TestCase(true, false, false, false, false, false, false)]
        [TestCase(true, true, true, false, false, false, false)]
        [TestCase(true, true, false, true, false, false, false)]
        [TestCase(true, true, false, false, true, false, false)]
        [TestCase(true, true, false, false, false, true, false)]
        public void CanAffectTarget_WithState_ReturnsExpected(
            bool runnerReady,
            bool gameplayAllowed,
            bool isOwner,
            bool isFinished,
            bool isRespawnProtected,
            bool isShielded,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJFishingRodPolicy.CanAffectTarget(
                    runnerReady,
                    gameplayAllowed,
                    isOwner,
                    isFinished,
                    isRespawnProtected,
                    isShielded
                )
            );
        }

        [Test]
        public void CalculatePullVelocity_TargetRightOfSource_PullsLeft()
        {
            Vector3 velocity = ProjectJFishingRodPolicy.CalculatePullVelocity(
                Vector3.zero,
                new Vector3(5f, 0f, 0f)
            );

            Assert.AreEqual(-8f, velocity.x, 0.0001f);
            Assert.AreEqual(0f, velocity.y, 0.0001f);
            Assert.AreEqual(0f, velocity.z, 0.0001f);
        }

        [Test]
        public void CalculatePullVelocity_IgnoresVerticalDifference()
        {
            Vector3 velocity = ProjectJFishingRodPolicy.CalculatePullVelocity(
                new Vector3(0f, 10f, 4f),
                new Vector3(0f, 0f, 0f)
            );

            Assert.AreEqual(0f, velocity.x, 0.0001f);
            Assert.AreEqual(0f, velocity.y, 0.0001f);
            Assert.AreEqual(8f, velocity.z, 0.0001f);
        }

        [Test]
        public void CalculatePullVelocity_SameHorizontalPosition_ReturnsZero()
        {
            Vector3 velocity = ProjectJFishingRodPolicy.CalculatePullVelocity(
                new Vector3(0f, 5f, 0f),
                Vector3.zero
            );

            Assert.AreEqual(Vector3.zero, velocity);
        }

        [TestCase(true, true, 5f, true, true)]
        [TestCase(false, true, 5f, true, false)]
        [TestCase(true, false, 5f, true, false)]
        [TestCase(true, true, 14f, true, true)]
        [TestCase(true, true, 14.1f, true, false)]
        [TestCase(true, true, 5f, false, false)]
        public void CanMaintainConnection_WithState_ReturnsExpected(
            bool timerActive,
            bool gameplayAllowed,
            float distanceMeters,
            bool lineClear,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJFishingRodPolicy.CanMaintainConnection(
                    timerActive,
                    gameplayAllowed,
                    distanceMeters,
                    lineClear
                )
            );
        }
    }
}
