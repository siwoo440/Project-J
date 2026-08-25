using NUnit.Framework; // EditMode 정책 테스트 사용
using ProjectJ.Items; // 유도탄 정책 사용
using UnityEngine; // Vector3 사용

namespace ProjectJ.Tests.EditMode
{
    public sealed class ProjectJHomingMissilePolicyTests
    {
        [Test]
        public void Constants_MatchDesignValues()
        {
            Assert.AreEqual(35f, ProjectJHomingMissilePolicy.SearchRadius);
            Assert.AreEqual(11f, ProjectJHomingMissilePolicy.Speed);
            Assert.AreEqual(10f, ProjectJHomingMissilePolicy.LifetimeSeconds);
            Assert.AreEqual(8f, ProjectJHomingMissilePolicy.HitExternalSpeed);
            Assert.AreEqual(1, ProjectJHomingMissilePolicy.MaximumReacquireCount);
        }

        [TestCase(0f, true)]
        [TestCase(34.99f, true)]
        [TestCase(35f, true)]
        [TestCase(35.01f, false)]
        [TestCase(-1f, true)]
        public void IsWithinSearchRadius_UsesThirtyFiveMeterBoundary(
            float distance,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJHomingMissilePolicy.IsWithinSearchRadius(distance)
            );
        }

        [TestCase(0f, 0f)]
        [TestCase(0.02f, 0.22f)]
        [TestCase(0.05f, 0.55f)]
        [TestCase(0.1f, 1.1f)]
        [TestCase(-1f, 0f)]
        public void CalculateStepDistance_UsesElevenMetersPerSecond(
            float deltaTime,
            float expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJHomingMissilePolicy.CalculateStepDistance(deltaTime),
                0.0001f
            );
        }

        [TestCase(0, true)]
        [TestCase(1, false)]
        [TestCase(2, false)]
        [TestCase(-1, true)]
        public void CanReacquire_AllowsOnlyOneRetry(
            int currentCount,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJHomingMissilePolicy.CanReacquire(currentCount)
            );
        }

        [TestCase(true, false, true, true, true)]
        [TestCase(false, false, true, true, false)]
        [TestCase(true, true, true, true, false)]
        [TestCase(true, false, false, true, false)]
        [TestCase(true, false, true, false, false)]
        public void CanTarget_ValidatesState(
            bool objectValid,
            bool owner,
            bool gameplayAllowed,
            bool visible,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJHomingMissilePolicy.CanTarget(
                    objectValid,
                    owner,
                    gameplayAllowed,
                    visible
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
                ProjectJHomingMissilePolicy.HasReachedRouteNode(distance)
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
                ProjectJHomingMissilePolicy.IsWithinRouteNodeSearchRadius(
                    distance
                )
            );
        }

        [Test]
        public void ResolveHitVelocity_NormalizesDirection()
        {
            Vector3 result =
                ProjectJHomingMissilePolicy.ResolveHitVelocity(
                    new Vector3(10f, 0f, 0f)
                );

            Assert.AreEqual(8f, result.x, 0.0001f);
            Assert.AreEqual(0f, result.y, 0.0001f);
            Assert.AreEqual(0f, result.z, 0.0001f);
        }

        [Test]
        public void ResolveHitVelocity_RemovesVerticalComponent()
        {
            Vector3 result =
                ProjectJHomingMissilePolicy.ResolveHitVelocity(
                    new Vector3(1f, 1f, 0f)
                );

            Assert.AreEqual(
                8f,
                result.magnitude,
                0.0001f
            );

            Assert.AreEqual(
                0f,
                result.y,
                0.0001f
            );
        }

        [Test]
        public void ResolveHitVelocity_ZeroDirectionReturnsZero()
        {
            Assert.AreEqual(
                Vector3.zero,
                ProjectJHomingMissilePolicy.ResolveHitVelocity(
                    Vector3.zero
                )
            );
        }

        [TestCase(true, true)]
        [TestCase(false, false)]
        public void ShouldTrackDirectly_FollowsLineOfSight(
            bool clearLine,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJHomingMissilePolicy.ShouldTrackDirectly(clearLine)
            );
        }

        [TestCase(0f, false)]
        [TestCase(9.99f, false)]
        [TestCase(10f, true)]
        [TestCase(10.01f, true)]
        public void HasLifetimeExpired_UsesTenSecondBoundary(
            float elapsedSeconds,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJHomingMissilePolicy.HasLifetimeExpired(
                    elapsedSeconds
                )
            );
        }

        [TestCase(true, true, true)]
        [TestCase(false, true, false)]
        [TestCase(true, false, false)]
        public void CanSpawn_RequiresServerStateAndTarget(
            bool runnerReady,
            bool targetFound,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJHomingMissilePolicy.CanSpawn(
                    runnerReady,
                    targetFound
                )
            );
        }
    }
}
