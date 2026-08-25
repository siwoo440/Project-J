using NUnit.Framework; // Unity EditMode 테스트 기능
using ProjectJ.Items; // 거대 풍선 정책 사용

namespace ProjectJ.Tests.EditMode
{
    public sealed class ProjectJGiantBalloonPolicyTests
    {
        [Test]
        public void RisingDurationSeconds_ReturnsSixSeconds()
        {
            Assert.AreEqual(
                6f,
                ProjectJGiantBalloonPolicy.RisingDurationSeconds
            );
        }

        [Test]
        public void DescendingDurationSeconds_ReturnsOnePointFiveSeconds()
        {
            Assert.AreEqual(
                1.5f,
                ProjectJGiantBalloonPolicy.DescendingDurationSeconds
            );
        }

        [Test]
        public void RisingSpeed_ReturnsFourMetersPerSecond()
        {
            Assert.AreEqual(
                4f,
                ProjectJGiantBalloonPolicy.RisingSpeed
            );
        }

        [Test]
        public void HorizontalControlMultiplier_ReturnsSixtyPercent()
        {
            Assert.AreEqual(
                0.6f,
                ProjectJGiantBalloonPolicy.HorizontalControlMultiplier
            );
        }

        [Test]
        public void DescendingSpeed_ReturnsMinusTwoMetersPerSecond()
        {
            Assert.AreEqual(
                -2f,
                ProjectJGiantBalloonPolicy.DescendingSpeed
            );
        }

        [TestCase(true, true, false, false, true)]
        [TestCase(false, true, false, false, false)]
        [TestCase(true, false, false, false, false)]
        [TestCase(true, true, true, false, false)]
        [TestCase(true, true, false, true, false)]
        public void CanUse_WithState_ReturnsExpected(
            bool runnerReady,
            bool gameplayAllowed,
            bool jetpackActive,
            bool giantBalloonActive,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJGiantBalloonPolicy.CanUse(
                    runnerReady,
                    gameplayAllowed,
                    jetpackActive,
                    giantBalloonActive
                )
            );
        }

        [TestCase(ProjectJGiantBalloonPhase.Inactive, false)]
        [TestCase(ProjectJGiantBalloonPhase.Rising, true)]
        [TestCase(ProjectJGiantBalloonPhase.Descending, true)]
        public void IsActive_WithPhase_ReturnsExpected(
            ProjectJGiantBalloonPhase phase,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJGiantBalloonPolicy.IsActive(phase)
            );
        }

        [TestCase(ProjectJGiantBalloonPhase.Inactive, false)]
        [TestCase(ProjectJGiantBalloonPhase.Rising, true)]
        [TestCase(ProjectJGiantBalloonPhase.Descending, false)]
        public void IsRising_WithPhase_ReturnsExpected(
            ProjectJGiantBalloonPhase phase,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJGiantBalloonPolicy.IsRising(phase)
            );
        }

        [TestCase(ProjectJGiantBalloonPhase.Inactive, false)]
        [TestCase(ProjectJGiantBalloonPhase.Rising, false)]
        [TestCase(ProjectJGiantBalloonPhase.Descending, true)]
        public void IsDescending_WithPhase_ReturnsExpected(
            ProjectJGiantBalloonPhase phase,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJGiantBalloonPolicy.IsDescending(phase)
            );
        }

        [TestCase(5f, ProjectJGiantBalloonPhase.Inactive, 5f)]
        [TestCase(5f, ProjectJGiantBalloonPhase.Rising, 3f)]
        [TestCase(8f, ProjectJGiantBalloonPhase.Rising, 4.8f)]
        [TestCase(5f, ProjectJGiantBalloonPhase.Descending, 3f)]
        [TestCase(-5f, ProjectJGiantBalloonPhase.Rising, 0f)]
        public void CalculateHorizontalMovementSpeed_WithPhase_ReturnsExpected(
            float baseSpeed,
            ProjectJGiantBalloonPhase phase,
            float expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJGiantBalloonPolicy.CalculateHorizontalMovementSpeed(
                    baseSpeed,
                    phase
                ),
                0.0001f
            );
        }

        [TestCase(
            -5f,
            ProjectJGiantBalloonPhase.Rising,
            true,
            false,
            false,
            4f
        )]
        [TestCase(
            7f,
            ProjectJGiantBalloonPhase.Rising,
            true,
            false,
            false,
            7f
        )]
        [TestCase(
            7f,
            ProjectJGiantBalloonPhase.Rising,
            true,
            true,
            false,
            0f
        )]
        [TestCase(
            -5f,
            ProjectJGiantBalloonPhase.Descending,
            true,
            false,
            false,
            -2f
        )]
        [TestCase(
            5f,
            ProjectJGiantBalloonPhase.Descending,
            true,
            false,
            false,
            -2f
        )]
        [TestCase(
            -5f,
            ProjectJGiantBalloonPhase.Descending,
            true,
            false,
            true,
            0f
        )]
        [TestCase(
            3f,
            ProjectJGiantBalloonPhase.Inactive,
            true,
            false,
            false,
            3f
        )]
        [TestCase(
            -3f,
            ProjectJGiantBalloonPhase.Rising,
            false,
            false,
            false,
            -3f
        )]
        public void ResolveVerticalVelocity_WithPhase_ReturnsExpected(
            float currentVelocity,
            ProjectJGiantBalloonPhase phase,
            bool gameplayAllowed,
            bool ceilingBlocked,
            bool grounded,
            float expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJGiantBalloonPolicy.ResolveVerticalVelocity(
                    currentVelocity,
                    phase,
                    gameplayAllowed,
                    ceilingBlocked,
                    grounded
                ),
                0.0001f
            );
        }

        [TestCase(
            ProjectJGiantBalloonPhase.Inactive,
            ProjectJGiantBalloonPhase.Inactive
        )]
        [TestCase(
            ProjectJGiantBalloonPhase.Rising,
            ProjectJGiantBalloonPhase.Descending
        )]
        [TestCase(
            ProjectJGiantBalloonPhase.Descending,
            ProjectJGiantBalloonPhase.Inactive
        )]
        public void GetNextPhase_WithCurrentPhase_ReturnsExpected(
            ProjectJGiantBalloonPhase current,
            ProjectJGiantBalloonPhase expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJGiantBalloonPolicy.GetNextPhase(current)
            );
        }

        [TestCase(ProjectJGiantBalloonPhase.Rising, 6f)]
        [TestCase(ProjectJGiantBalloonPhase.Descending, 1.5f)]
        [TestCase(ProjectJGiantBalloonPhase.Inactive, 0f)]
        public void GetPhaseDuration_WithPhase_ReturnsExpected(
            ProjectJGiantBalloonPhase phase,
            float expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJGiantBalloonPolicy.GetPhaseDuration(phase)
            );
        }

        [TestCase(true, true, false)]
        [TestCase(false, true, true)]
        [TestCase(true, false, true)]
        [TestCase(false, false, true)]
        public void ShouldClear_WithState_ReturnsExpected(
            bool gameplayAllowed,
            bool objectValid,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJGiantBalloonPolicy.ShouldClear(
                    gameplayAllowed,
                    objectValid
                )
            );
        }
    }
}
