using NUnit.Framework; // Unity EditMode 테스트 기능
using ProjectJ.Items; // 비눗방울 정책 사용

namespace ProjectJ.Tests.EditMode
{
    public sealed class ProjectJSoapBubblePolicyTests
    {
        [Test]
        public void DurationSeconds_ReturnsTwoPointFiveSeconds()
        {
            Assert.AreEqual(
                2.5f,
                ProjectJSoapBubblePolicy.DurationSeconds
            );
        }

        [Test]
        public void ProjectileSpeed_ReturnsThirteenMetersPerSecond()
        {
            Assert.AreEqual(
                13f,
                ProjectJSoapBubblePolicy.ProjectileSpeed
            );
        }

        [Test]
        public void MaximumTravelDistance_ReturnsSixteenMeters()
        {
            Assert.AreEqual(
                16f,
                ProjectJSoapBubblePolicy.MaximumTravelDistance
            );
        }

        [Test]
        public void CollisionRadius_ReturnsPrototypeRadius()
        {
            Assert.AreEqual(
                0.3f,
                ProjectJSoapBubblePolicy.CollisionRadius
            );
        }

        [Test]
        public void EscapeJumpPressCount_ReturnsSix()
        {
            Assert.AreEqual(
                6,
                ProjectJSoapBubblePolicy.EscapeJumpPressCount
            );
        }

        [TestCase(true, true, false, false, false, true)]
        [TestCase(false, true, false, false, false, false)]
        [TestCase(true, false, false, false, false, false)]
        [TestCase(true, true, true, false, false, false)]
        [TestCase(true, true, false, true, false, false)]
        [TestCase(true, true, false, false, true, false)]
        public void CanAffectTarget_WithState_ReturnsExpected(
            bool runnerReady,
            bool gameplayAllowed,
            bool isOwner,
            bool isFinished,
            bool isRespawnProtected,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJSoapBubblePolicy.CanAffectTarget(
                    runnerReady,
                    gameplayAllowed,
                    isOwner,
                    isFinished,
                    isRespawnProtected
                )
            );
        }

        [TestCase(true, true)]
        [TestCase(false, false)]
        public void ShouldRestrictLocomotion_WithActiveState_ReturnsExpected(
            bool active,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJSoapBubblePolicy.ShouldRestrictLocomotion(active)
            );
        }

        [TestCase(true, true, false, true)]
        [TestCase(true, true, true, false)]
        [TestCase(true, false, false, false)]
        [TestCase(false, true, false, false)]
        [TestCase(false, false, true, false)]
        public void ShouldCountJumpPress_WithInput_ReturnsExpected(
            bool active,
            bool jumpPressed,
            bool previousJumpPressed,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJSoapBubblePolicy.ShouldCountJumpPress(
                    active,
                    jumpPressed,
                    previousJumpPressed
                )
            );
        }

        [TestCase(0, 1)]
        [TestCase(1, 2)]
        [TestCase(5, 6)]
        [TestCase(6, 6)]
        [TestCase(99, 6)]
        [TestCase(-1, 1)]
        public void GetNextJumpPressCount_WithCurrentCount_ReturnsExpected(
            int currentCount,
            int expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJSoapBubblePolicy.GetNextJumpPressCount(currentCount)
            );
        }

        [TestCase(0, false)]
        [TestCase(5, false)]
        [TestCase(6, true)]
        [TestCase(7, true)]
        public void HasEscaped_WithJumpCount_ReturnsExpected(
            int jumpCount,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJSoapBubblePolicy.HasEscaped(jumpCount)
            );
        }

        [TestCase(0f, false)]
        [TestCase(15.999f, false)]
        [TestCase(16f, true)]
        [TestCase(16.001f, true)]
        public void HasReachedTravelLimit_WithDistance_ReturnsExpected(
            float distance,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJSoapBubblePolicy.HasReachedTravelLimit(distance)
            );
        }

        [TestCase(0f)]
        [TestCase(0.5f)]
        [TestCase(2.4f)]
        [TestCase(10f)]
        public void GetRefreshedDuration_AlwaysReturnsFullDuration(
            float remaining
        )
        {
            Assert.AreEqual(
                2.5f,
                ProjectJSoapBubblePolicy.GetRefreshedDuration(remaining)
            );
        }
    }
}
