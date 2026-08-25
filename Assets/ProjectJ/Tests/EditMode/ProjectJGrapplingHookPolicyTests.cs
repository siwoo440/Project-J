using NUnit.Framework; // Unity EditMode 테스트 기능
using ProjectJ.Items; // 갈고리 정책 사용
using UnityEngine; // Vector3 테스트 사용

namespace ProjectJ.Tests.EditMode
{
    public sealed class ProjectJGrapplingHookPolicyTests
    {
        [Test]
        public void DurationSeconds_ReturnsOnePointFiveSeconds()
        {
            Assert.AreEqual(
                1.5f,
                ProjectJGrapplingHookPolicy.DurationSeconds
            );
        }

        [Test]
        public void MaximumRangeMeters_ReturnsTwentyMeters()
        {
            Assert.AreEqual(
                20f,
                ProjectJGrapplingHookPolicy.MaximumRangeMeters
            );
        }

        [Test]
        public void PullSpeedMetersPerSecond_ReturnsTwelveMetersPerSecond()
        {
            Assert.AreEqual(
                12f,
                ProjectJGrapplingHookPolicy.PullSpeedMetersPerSecond
            );
        }

        [Test]
        public void ArrivalDistanceMeters_ReturnsZeroPointSevenFiveMeters()
        {
            Assert.AreEqual(
                0.75f,
                ProjectJGrapplingHookPolicy.ArrivalDistanceMeters
            );
        }

        [TestCase(0f, true)]
        [TestCase(19.999f, true)]
        [TestCase(20f, true)]
        [TestCase(20.001f, false)]
        [TestCase(-0.1f, false)]
        public void IsWithinInitialRange_WithDistance_ReturnsExpected(
            float distanceMeters,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJGrapplingHookPolicy.IsWithinInitialRange(
                    distanceMeters
                )
            );
        }

        [TestCase(0f, true)]
        [TestCase(0.5f, true)]
        [TestCase(0.75f, true)]
        [TestCase(0.751f, false)]
        [TestCase(-0.1f, true)]
        public void HasArrived_WithDistance_ReturnsExpected(
            float distanceMeters,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJGrapplingHookPolicy.HasArrived(
                    distanceMeters
                )
            );
        }

        [TestCase(true, true, true, 10f, true)]
        [TestCase(false, true, true, 10f, false)]
        [TestCase(true, false, true, 10f, false)]
        [TestCase(true, true, false, 10f, false)]
        [TestCase(true, true, true, 20f, true)]
        [TestCase(true, true, true, 20.1f, false)]
        public void CanActivate_WithState_ReturnsExpected(
            bool runnerReady,
            bool gameplayAllowed,
            bool isGrappleSurface,
            float distanceMeters,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJGrapplingHookPolicy.CanActivate(
                    runnerReady,
                    gameplayAllowed,
                    isGrappleSurface,
                    distanceMeters
                )
            );
        }

        [Test]
        public void CalculatePullVelocity_UsesFullThreeDimensionalDirection()
        {
            Vector3 velocity =
                ProjectJGrapplingHookPolicy.CalculatePullVelocity(
                    Vector3.zero,
                    new Vector3(0f, 3f, 4f)
                );

            Assert.AreEqual(0f, velocity.x, 0.0001f);
            Assert.AreEqual(7.2f, velocity.y, 0.0001f);
            Assert.AreEqual(9.6f, velocity.z, 0.0001f);
        }

        [Test]
        public void CalculatePullVelocity_SamePosition_ReturnsZero()
        {
            Vector3 velocity =
                ProjectJGrapplingHookPolicy.CalculatePullVelocity(
                    Vector3.one,
                    Vector3.one
                );

            Assert.AreEqual(Vector3.zero, velocity);
        }

        [TestCase(true, true, 5f, true)]
        [TestCase(false, true, 5f, false)]
        [TestCase(true, false, 5f, false)]
        [TestCase(true, true, 0.75f, false)]
        [TestCase(true, true, 0.2f, false)]
        public void CanMaintainConnection_WithState_ReturnsExpected(
            bool timerActive,
            bool gameplayAllowed,
            float distanceToAnchor,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJGrapplingHookPolicy.CanMaintainConnection(
                    timerActive,
                    gameplayAllowed,
                    distanceToAnchor
                )
            );
        }
    }
}
