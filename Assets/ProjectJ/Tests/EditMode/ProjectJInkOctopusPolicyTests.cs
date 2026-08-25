using NUnit.Framework; // Unity EditMode 테스트 기능
using ProjectJ.Items; // 먹물 문어 정책 사용

namespace ProjectJ.Tests.EditMode
{
    public sealed class ProjectJInkOctopusPolicyTests
    {
        [Test]
        public void DurationSeconds_ReturnsThreePointFiveSeconds()
        {
            Assert.AreEqual(3.5f, ProjectJInkOctopusPolicy.DurationSeconds);
        }

        [Test]
        public void ProjectileSpeed_ReturnsSixteenMetersPerSecond()
        {
            Assert.AreEqual(16f, ProjectJInkOctopusPolicy.ProjectileSpeed);
        }

        [Test]
        public void MaximumTravelDistance_ReturnsEighteenMeters()
        {
            Assert.AreEqual(18f, ProjectJInkOctopusPolicy.MaximumTravelDistance);
        }

        [Test]
        public void CollisionRadius_ReturnsPrototypeRadius()
        {
            Assert.AreEqual(0.3f, ProjectJInkOctopusPolicy.CollisionRadius);
        }

        [Test]
        public void OverlayCoverage_IsApproximatelySixtyFivePercent()
        {
            float coverage =
                ProjectJInkOctopusPolicy.OverlayWidthNormalized *
                ProjectJInkOctopusPolicy.OverlayHeightNormalized;

            Assert.AreEqual(0.675f, coverage, 0.0001f);
        }

        [TestCase(true, true, false, false, false, true)]
        [TestCase(false, true, false, false, false, false)]
        [TestCase(true, false, false, false, false, false)]
        [TestCase(true, true, true, false, false, false)]
        [TestCase(true, true, false, true, false, false)]
        [TestCase(true, true, false, false, true, false)]
        public void CanAffectTarget_WithCurrentState_ReturnsExpected(
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
                ProjectJInkOctopusPolicy.CanAffectTarget(
                    runnerReady,
                    gameplayAllowed,
                    isOwner,
                    isFinished,
                    isRespawnProtected
                )
            );
        }

        [TestCase(0f, 3.5f)]
        [TestCase(1.2f, 3.5f)]
        [TestCase(3.4f, 3.5f)]
        [TestCase(-5f, 3.5f)]
        public void GetRefreshedDuration_DoesNotStackAndRefreshesToFullDuration(
            float currentRemaining,
            float expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJInkOctopusPolicy.GetRefreshedDuration(currentRemaining)
            );
        }

        [TestCase(0f, false)]
        [TestCase(17.999f, false)]
        [TestCase(18f, true)]
        [TestCase(20f, true)]
        [TestCase(-1f, false)]
        public void HasReachedTravelLimit_WithDistance_ReturnsExpected(
            float travelledDistance,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJInkOctopusPolicy.HasReachedTravelLimit(travelledDistance)
            );
        }
    }
}
