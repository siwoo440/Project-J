using NUnit.Framework; // EditMode 정책 테스트 사용
using ProjectJ.Items; // 드론 정책 사용
using UnityEngine; // Vector3 사용

namespace ProjectJ.Tests.EditMode
{
    public sealed class ProjectJDronePolicyTests
    {
        [Test]
        public void Constants_MatchDesignValues()
        {
            Assert.AreEqual(12f, ProjectJDronePolicy.LifetimeSeconds);
            Assert.AreEqual(9f, ProjectJDronePolicy.Speed);
            Assert.AreEqual(7f, ProjectJDronePolicy.AttackExternalSpeed);
            Assert.AreEqual(1, ProjectJDronePolicy.MaximumReacquireCount);
            Assert.AreEqual(1f, ProjectJDronePolicy.AttackDistance);
        }

        [TestCase(true, true, 2, true, true)]
        [TestCase(false, true, 2, true, false)]
        [TestCase(true, false, 2, true, false)]
        [TestCase(true, true, 1, true, false)]
        [TestCase(true, true, 2, false, false)]
        public void CanUse_RejectsOwnerInFirstPlaceAndMissingTarget(
            bool runnerReady,
            bool gameplayAllowed,
            int ownerRaceRank,
            bool targetFound,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJDronePolicy.CanUse(
                    runnerReady,
                    gameplayAllowed,
                    ownerRaceRank,
                    targetFound
                )
            );
        }

        [TestCase(true, false, true, true, true)]
        [TestCase(false, false, true, true, false)]
        [TestCase(true, true, true, true, false)]
        [TestCase(true, false, false, true, false)]
        [TestCase(true, false, true, false, false)]
        public void CanTarget_ValidatesNetworkGameplayAndTrackingState(
            bool objectValid,
            bool isOwner,
            bool gameplayAllowed,
            bool trackable,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJDronePolicy.CanTarget(
                    objectValid,
                    isOwner,
                    gameplayAllowed,
                    trackable
                )
            );
        }

        [TestCase(1, true)]
        [TestCase(2, false)]
        [TestCase(0, false)]
        [TestCase(-1, false)]
        public void IsInitialLeaderRank_RequiresRankOne(
            int raceRank,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJDronePolicy.IsInitialLeaderRank(
                    raceRank
                )
            );
        }

        [TestCase(1, 3, 5, 2, true)]
        [TestCase(3, 1, 1, 9, false)]
        [TestCase(2, 2, 1, 5, true)]
        [TestCase(2, 2, 8, 5, false)]
        [TestCase(2, int.MaxValue, 8, int.MaxValue, true)]
        public void IsBetterReacquireCandidate_PrefersLowerRankThenLowerPlayerIndex(
            int candidateRank,
            int bestRank,
            int candidateIndex,
            int bestIndex,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJDronePolicy.IsBetterReacquireCandidate(
                    candidateRank,
                    bestRank,
                    candidateIndex,
                    bestIndex
                )
            );
        }

        [TestCase(0f, 0f)]
        [TestCase(0.02f, 0.18f)]
        [TestCase(0.05f, 0.45f)]
        [TestCase(0.1f, 0.9f)]
        [TestCase(-1f, 0f)]
        public void CalculateStepDistance_UsesNineMetersPerSecond(
            float deltaTime,
            float expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJDronePolicy.CalculateStepDistance(
                    deltaTime
                ),
                0.0001f
            );
        }

        [TestCase(0f, true)]
        [TestCase(0.99f, true)]
        [TestCase(1f, true)]
        [TestCase(1.01f, false)]
        public void HasReachedAttackDistance_UsesOneMeterBoundary(
            float distance,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJDronePolicy.HasReachedAttackDistance(
                    distance
                )
            );
        }

        [TestCase(0, true)]
        [TestCase(1, false)]
        [TestCase(2, false)]
        [TestCase(-1, true)]
        public void CanReacquire_AllowsExactlyOneRetry(
            int currentCount,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJDronePolicy.CanReacquire(
                    currentCount
                )
            );
        }

        [Test]
        public void ResolveAttackVelocity_UsesHorizontalDirection()
        {
            Vector3 result =
                ProjectJDronePolicy.ResolveAttackVelocity(
                    new Vector3(10f, 5f, 0f)
                );

            Assert.AreEqual(7f, result.x, 0.0001f);
            Assert.AreEqual(0f, result.y, 0.0001f);
            Assert.AreEqual(0f, result.z, 0.0001f);
        }

        [Test]
        public void ResolveAttackVelocity_UsesWorldForwardForZeroHorizontalDirection()
        {
            Vector3 result =
                ProjectJDronePolicy.ResolveAttackVelocity(
                    Vector3.up
                );

            Assert.AreEqual(0f, result.x, 0.0001f);
            Assert.AreEqual(0f, result.y, 0.0001f);
            Assert.AreEqual(7f, result.z, 0.0001f);
        }

        [TestCase(0f, false)]
        [TestCase(11.99f, false)]
        [TestCase(12f, true)]
        [TestCase(12.01f, true)]
        public void HasLifetimeExpired_UsesTwelveSecondBoundary(
            float elapsedSeconds,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJDronePolicy.HasLifetimeExpired(
                    elapsedSeconds
                )
            );
        }

        [TestCase(0f, true)]
        [TestCase(11.99f, true)]
        [TestCase(12f, true)]
        [TestCase(12.01f, false)]
        public void IsWithinRouteNodeSearchRadius_UsesTwelveMeters(
            float distance,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJDronePolicy.IsWithinRouteNodeSearchRadius(
                    distance
                )
            );
        }

        [TestCase(0f, true)]
        [TestCase(0.39f, true)]
        [TestCase(0.4f, true)]
        [TestCase(0.41f, false)]
        public void HasReachedRouteNode_UsesPointFourMeterBoundary(
            float distance,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJDronePolicy.HasReachedRouteNode(
                    distance
                )
            );
        }
    }
}
