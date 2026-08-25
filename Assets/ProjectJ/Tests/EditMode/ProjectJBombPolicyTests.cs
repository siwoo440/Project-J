using NUnit.Framework; // Unity EditMode 테스트 사용
using ProjectJ.Items; // 폭탄 정책 사용
using UnityEngine; // Vector3 검증 사용

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스
{
    public sealed class ProjectJBombPolicyTests // 폭탄 정책 테스트
    {
        [Test]
        public void FuseSeconds_ReturnsTwoPointFiveSeconds()
        {
            Assert.AreEqual(2.5f, ProjectJBombPolicy.FuseSeconds);
        }

        [Test]
        public void MaximumThrowDistance_ReturnsTwelveMeters()
        {
            Assert.AreEqual(12f, ProjectJBombPolicy.MaximumThrowDistance);
        }

        [Test]
        public void ExplosionRadius_ReturnsFiveMeters()
        {
            Assert.AreEqual(5f, ProjectJBombPolicy.ExplosionRadius);
        }

        [Test]
        public void CenterForce_ReturnsTenMetersPerSecond()
        {
            Assert.AreEqual(10f, ProjectJBombPolicy.CenterForce);
        }

        [Test]
        public void EdgeForce_ReturnsFourMetersPerSecond()
        {
            Assert.AreEqual(4f, ProjectJBombPolicy.EdgeForce);
        }

        [TestCase(true, true, false, true)]
        [TestCase(false, true, false, false)]
        [TestCase(true, false, false, false)]
        [TestCase(true, true, true, false)]
        public void CanThrow_WithAuthorityAndActiveBombState_ReturnsExpected(
            bool runnerReady,
            bool gameplayAllowed,
            bool hasActiveBomb,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJBombPolicy.CanThrow(
                    runnerReady,
                    gameplayAllowed,
                    hasActiveBomb
                )
            );
        }

        [TestCase(0f, true)]
        [TestCase(4.999f, true)]
        [TestCase(5f, true)]
        [TestCase(5.001f, false)]
        [TestCase(-1f, true)]
        public void IsWithinExplosionRadius_WithDistance_ReturnsExpected(
            float distance,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJBombPolicy.IsWithinExplosionRadius(distance)
            );
        }

        [TestCase(0f, 10f)]
        [TestCase(1.25f, 8.5f)]
        [TestCase(2.5f, 7f)]
        [TestCase(3.75f, 5.5f)]
        [TestCase(5f, 4f)]
        [TestCase(6f, 0f)]
        [TestCase(-2f, 10f)]
        public void CalculateExplosionForce_WithDistance_ReturnsExpected(
            float distance,
            float expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJBombPolicy.CalculateExplosionForce(distance),
                0.0001f
            );
        }

        [Test]
        public void CreateInitialVelocity_WithForward_UsesPrototypeArcValues()
        {
            Vector3 velocity =
                ProjectJBombPolicy.CreateInitialVelocity(Vector3.forward);

            Assert.AreEqual(0f, velocity.x, 0.0001f);
            Assert.AreEqual(5f, velocity.y, 0.0001f);
            Assert.AreEqual(8f, velocity.z, 0.0001f);
        }

        [Test]
        public void CreateInitialVelocity_WithZeroForward_UsesForwardFallback()
        {
            Vector3 velocity =
                ProjectJBombPolicy.CreateInitialVelocity(Vector3.zero);

            Assert.AreEqual(0f, velocity.x, 0.0001f);
            Assert.AreEqual(5f, velocity.y, 0.0001f);
            Assert.AreEqual(8f, velocity.z, 0.0001f);
        }

        [Test]
        public void GetHorizontalDistance_IgnoresHeightDifference()
        {
            float distance = ProjectJBombPolicy.GetHorizontalDistance(
                new Vector3(0f, 0f, 0f),
                new Vector3(3f, 50f, 4f)
            );

            Assert.AreEqual(5f, distance, 0.0001f);
        }

        [Test]
        public void CreateExplosionVelocityChange_AtCenter_UsesFallbackAndCenterForce()
        {
            Vector3 velocityChange =
                ProjectJBombPolicy.CreateExplosionVelocityChange(
                    Vector3.zero,
                    Vector3.zero,
                    Vector3.forward
                );

            Assert.AreEqual(10f, velocityChange.magnitude, 0.0001f);
            Assert.Greater(velocityChange.z, 0f);
        }

        [Test]
        public void CreateExplosionVelocityChange_AtEdge_UsesEdgeForce()
        {
            Vector3 velocityChange =
                ProjectJBombPolicy.CreateExplosionVelocityChange(
                    Vector3.zero,
                    new Vector3(5f, 0f, 0f),
                    Vector3.forward
                );

            Assert.AreEqual(4f, velocityChange.magnitude, 0.0001f);
            Assert.Greater(velocityChange.x, 0f);
        }

        [Test]
        public void CreateExplosionVelocityChange_OutsideRadius_ReturnsZero()
        {
            Vector3 velocityChange =
                ProjectJBombPolicy.CreateExplosionVelocityChange(
                    Vector3.zero,
                    new Vector3(5.1f, 0f, 0f),
                    Vector3.forward
                );

            Assert.AreEqual(Vector3.zero, velocityChange);
        }
    }
}
