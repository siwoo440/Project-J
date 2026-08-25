using NUnit.Framework; // EditMode 정책 테스트 사용
using ProjectJ.Items; // 되감기 시계 정책 사용
using UnityEngine; // Vector3 사용

namespace ProjectJ.Tests.EditMode
{
    public sealed class ProjectJRewindClockPolicyTests
    {
        [Test]
        public void Constants_MatchDesignValues()
        {
            Assert.AreEqual(
                5f,
                ProjectJRewindClockPolicy.HistoryDurationSeconds
            );
            Assert.AreEqual(
                0.8f,
                ProjectJRewindClockPolicy.RewindDurationSeconds
            );
            Assert.AreEqual(
                0.5f,
                ProjectJRewindClockPolicy.HistoryRetentionSlackSeconds
            );
        }

        [Test]
        public void CanUse_WhenAllConditionsAreValid_ReturnsTrue()
        {
            Assert.IsTrue(
                ProjectJRewindClockPolicy.CanUse(
                    true,
                    true,
                    true,
                    true,
                    false,
                    false,
                    false
                )
            );
        }

        [TestCase(false, true, true, true, false, false, false)]
        [TestCase(true, false, true, true, false, false, false)]
        [TestCase(true, true, false, true, false, false, false)]
        [TestCase(true, true, true, false, false, false, false)]
        [TestCase(true, true, true, true, true, false, false)]
        [TestCase(true, true, true, true, false, true, false)]
        [TestCase(true, true, true, true, false, false, true)]
        public void CanUse_WhenAnyBlockingConditionExists_ReturnsFalse(
            bool runnerReady,
            bool gameplayAllowed,
            bool hasFullHistory,
            bool targetSafe,
            bool cartRiding,
            bool grapplingHookActive,
            bool rewindActive
        )
        {
            Assert.IsFalse(
                ProjectJRewindClockPolicy.CanUse(
                    runnerReady,
                    gameplayAllowed,
                    hasFullHistory,
                    targetSafe,
                    cartRiding,
                    grapplingHookActive,
                    rewindActive
                )
            );
        }

        [TestCase(true, false, true)]
        [TestCase(true, true, false)]
        [TestCase(false, false, false)]
        [TestCase(false, true, false)]
        public void ShouldRecord_UsesGameplayAndRewindState(
            bool gameplayAllowed,
            bool rewindActive,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJRewindClockPolicy.ShouldRecord(
                    gameplayAllowed,
                    rewindActive
                )
            );
        }

        [TestCase(-1f, 0f)]
        [TestCase(0f, 0f)]
        [TestCase(0.4f, 0.5f)]
        [TestCase(0.8f, 1f)]
        [TestCase(2f, 1f)]
        public void CalculatePlaybackNormalized_ClampsToZeroAndOne(
            float elapsedSeconds,
            float expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJRewindClockPolicy.CalculatePlaybackNormalized(
                    elapsedSeconds
                ),
                0.0001f
            );
        }

        [TestCase(12f, 0f, 12f)]
        [TestCase(12f, 0.5f, 9.5f)]
        [TestCase(12f, 1f, 7f)]
        [TestCase(12f, 2f, 7f)]
        public void CalculatePlaybackHistoryTime_ReversesFiveSeconds(
            float startHistoryTime,
            float normalized,
            float expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJRewindClockPolicy.CalculatePlaybackHistoryTime(
                    startHistoryTime,
                    normalized
                ),
                0.0001f
            );
        }

        [TestCase(10f, 10f, true, true)]
        [TestCase(10.01f, 10f, true, true)]
        [TestCase(9.99f, 10f, true, false)]
        [TestCase(10f, 10f, false, false)]
        public void IsTargetSafe_RequiresFinitePositionAndFallLimit(
            float targetY,
            float fallLimitY,
            bool finitePosition,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJRewindClockPolicy.IsTargetSafe(
                    targetY,
                    fallLimitY,
                    finitePosition
                )
            );
        }

        [Test]
        public void IsFinitePosition_RejectsInvalidCoordinates()
        {
            Assert.IsTrue(
                ProjectJRewindClockPolicy.IsFinitePosition(
                    new Vector3(1f, 2f, 3f)
                )
            );

            Assert.IsFalse(
                ProjectJRewindClockPolicy.IsFinitePosition(
                    new Vector3(float.NaN, 2f, 3f)
                )
            );

            Assert.IsFalse(
                ProjectJRewindClockPolicy.IsFinitePosition(
                    new Vector3(1f, float.PositiveInfinity, 3f)
                )
            );
        }

        [TestCase(0f, false)]
        [TestCase(0.79f, false)]
        [TestCase(0.8f, true)]
        [TestCase(1.2f, true)]
        public void IsPlaybackComplete_UsesPointEightSecondBoundary(
            float elapsedSeconds,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJRewindClockPolicy.IsPlaybackComplete(
                    elapsedSeconds
                )
            );
        }
    }
}
