using NUnit.Framework; // Unity EditMode 테스트 기능
using ProjectJ.Items; // 연막탄 정책 사용
using UnityEngine; // Vector3 테스트 사용

namespace ProjectJ.Tests.EditMode
{
    public sealed class ProjectJSmokeGrenadePolicyTests
    {
        [Test]
        public void SmokeDurationSeconds_ReturnsSixSeconds()
        {
            Assert.AreEqual(
                6f,
                ProjectJSmokeGrenadePolicy.SmokeDurationSeconds
            );
        }

        [Test]
        public void MaximumThrowDistance_ReturnsFourteenMeters()
        {
            Assert.AreEqual(
                14f,
                ProjectJSmokeGrenadePolicy.MaximumThrowDistance
            );
        }

        [Test]
        public void SmokeRadius_ReturnsFiveMeters()
        {
            Assert.AreEqual(
                5f,
                ProjectJSmokeGrenadePolicy.SmokeRadius
            );
        }

        [Test]
        public void OverlayAlpha_ReturnsSixtyPercent()
        {
            Assert.AreEqual(
                0.6f,
                ProjectJSmokeGrenadePolicy.OverlayAlpha
            );
        }

        [Test]
        public void MaximumActiveZonesPerOwner_ReturnsTwo()
        {
            Assert.AreEqual(
                2,
                ProjectJSmokeGrenadePolicy.MaximumActiveZonesPerOwner
            );
        }

        [TestCase(true, true, true)]
        [TestCase(false, true, false)]
        [TestCase(true, false, false)]
        [TestCase(false, false, false)]
        public void CanThrow_WithState_ReturnsExpected(
            bool runnerReady,
            bool gameplayAllowed,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJSmokeGrenadePolicy.CanThrow(
                    runnerReady,
                    gameplayAllowed
                )
            );
        }

        [TestCase(0f, true)]
        [TestCase(4.999f, true)]
        [TestCase(5f, true)]
        [TestCase(5.001f, false)]
        [TestCase(-1f, true)]
        public void IsWithinSmokeRadius_WithDistance_ReturnsExpected(
            float distance,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJSmokeGrenadePolicy.IsWithinSmokeRadius(distance)
            );
        }

        [TestCase(0, 0f)]
        [TestCase(1, 0.6f)]
        [TestCase(2, 0.6f)]
        [TestCase(10, 0.6f)]
        [TestCase(-1, 0f)]
        public void ResolveOverlayAlpha_DoesNotStackDensity(
            int activeZoneCount,
            float expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJSmokeGrenadePolicy.ResolveOverlayAlpha(
                    activeZoneCount
                )
            );
        }

        [TestCase(-21f, -20f, true)]
        [TestCase(-20.001f, -20f, true)]
        [TestCase(-20f, -20f, false)]
        [TestCase(10f, -20f, false)]
        public void IsBelowFallLimit_WithHeight_ReturnsExpected(
            float currentY,
            float fallLimitY,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJSmokeGrenadePolicy.IsBelowFallLimit(
                    currentY,
                    fallLimitY
                )
            );
        }

        [Test]
        public void CreateInitialVelocity_UsesConfiguredArc()
        {
            Vector3 velocity =
                ProjectJSmokeGrenadePolicy.CreateInitialVelocity(
                    Vector3.forward
                );

            Assert.AreEqual(0f, velocity.x, 0.0001f);
            Assert.AreEqual(6f, velocity.y, 0.0001f);
            Assert.AreEqual(12f, velocity.z, 0.0001f);
        }

        [Test]
        public void CreateInitialVelocity_IgnoresForwardVerticalComponent()
        {
            Vector3 velocity =
                ProjectJSmokeGrenadePolicy.CreateInitialVelocity(
                    new Vector3(0f, 10f, 1f)
                );

            Assert.AreEqual(0f, velocity.x, 0.0001f);
            Assert.AreEqual(6f, velocity.y, 0.0001f);
            Assert.AreEqual(12f, velocity.z, 0.0001f);
        }

        [Test]
        public void GetHorizontalDistance_IgnoresHeight()
        {
            float distance =
                ProjectJSmokeGrenadePolicy.GetHorizontalDistance(
                    new Vector3(0f, 100f, 0f),
                    new Vector3(3f, -100f, 4f)
                );

            Assert.AreEqual(5f, distance, 0.0001f);
        }

        [TestCase(true, true, true)]
        [TestCase(false, true, false)]
        [TestCase(true, false, false)]
        [TestCase(false, false, false)]
        public void ShouldKeepSmokeZone_WithState_ReturnsExpected(
            bool lifetimeActive,
            bool anyGameplayActive,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJSmokeGrenadePolicy.ShouldKeepSmokeZone(
                    lifetimeActive,
                    anyGameplayActive
                )
            );
        }
    }
}
