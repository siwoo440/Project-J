using NUnit.Framework; // EditMode 정책 테스트 사용
using ProjectJ.Items; // 가시 갑옷 정책 사용
using UnityEngine; // Vector3 사용

namespace ProjectJ.Tests.EditMode
{
    public sealed class ProjectJSpikedArmorPolicyTests
    {
        [Test]
        public void Constants_MatchDesignValues()
        {
            Assert.AreEqual(
                5f,
                ProjectJSpikedArmorPolicy.DurationSeconds
            );

            Assert.AreEqual(
                1.2f,
                ProjectJSpikedArmorPolicy.DetectionRadius
            );

            Assert.AreEqual(
                6f,
                ProjectJSpikedArmorPolicy.PushSpeedMetersPerSecond
            );

            Assert.AreEqual(
                1f,
                ProjectJSpikedArmorPolicy.PerTargetCooldownSeconds
            );
        }

        [TestCase(false, true, true, true)]
        [TestCase(true, true, true, false)]
        [TestCase(false, false, true, false)]
        [TestCase(false, true, false, false)]
        public void CanActivate_RequiresInactiveGameplayAuthority(
            bool active,
            bool gameplayAllowed,
            bool authorityReady,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJSpikedArmorPolicy.CanActivate(
                    active,
                    gameplayAllowed,
                    authorityReady
                )
            );
        }

        [TestCase(false, false, true, true)]
        [TestCase(true, false, true, false)]
        [TestCase(false, true, true, false)]
        [TestCase(false, false, false, false)]
        public void CanTriggerTarget_RejectsSelfCooldownAndInvalidGameplay(
            bool isSelf,
            bool cooldownActive,
            bool gameplayAllowed,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJSpikedArmorPolicy.CanTriggerTarget(
                    isSelf,
                    cooldownActive,
                    gameplayAllowed
                )
            );
        }

        [TestCase(0f, true)]
        [TestCase(1f, true)]
        [TestCase(1.19f, true)]
        [TestCase(1.2f, true)]
        [TestCase(1.21f, false)]
        [TestCase(2f, false)]
        [TestCase(-1f, true)]
        public void IsInsideDetectionRadius_UsesOnePointTwoMeterBoundary(
            float distance,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJSpikedArmorPolicy.IsInsideDetectionRadius(
                    distance
                )
            );
        }

        [Test]
        public void ResolvePushVelocity_UsesHorizontalOutwardDirection()
        {
            Vector3 result =
                ProjectJSpikedArmorPolicy.ResolvePushVelocity(
                    new Vector3(4f, 10f, 0f),
                    Vector3.forward
                );

            Assert.AreEqual(
                6f,
                result.x,
                0.0001f
            );

            Assert.AreEqual(
                0f,
                result.y,
                0.0001f
            );

            Assert.AreEqual(
                0f,
                result.z,
                0.0001f
            );
        }

        [Test]
        public void ResolvePushVelocity_NormalizesDiagonalDirection()
        {
            Vector3 result =
                ProjectJSpikedArmorPolicy.ResolvePushVelocity(
                    new Vector3(1f, 4f, 1f),
                    Vector3.forward
                );

            Assert.AreEqual(
                6f,
                result.magnitude,
                0.0001f
            );

            Assert.AreEqual(
                0f,
                result.y,
                0.0001f
            );

            Assert.Greater(
                result.x,
                0f
            );

            Assert.Greater(
                result.z,
                0f
            );
        }

        [Test]
        public void ResolvePushVelocity_UsesFallbackWhenPositionsOverlap()
        {
            Vector3 result =
                ProjectJSpikedArmorPolicy.ResolvePushVelocity(
                    Vector3.zero,
                    new Vector3(2f, 3f, 0f)
                );

            Assert.AreEqual(
                6f,
                result.x,
                0.0001f
            );

            Assert.AreEqual(
                0f,
                result.y,
                0.0001f
            );

            Assert.AreEqual(
                0f,
                result.z,
                0.0001f
            );
        }

        [Test]
        public void ResolvePushVelocity_UsesWorldForwardWhenFallbackInvalid()
        {
            Vector3 result =
                ProjectJSpikedArmorPolicy.ResolvePushVelocity(
                    Vector3.zero,
                    Vector3.up
                );

            Assert.AreEqual(
                0f,
                result.x,
                0.0001f
            );

            Assert.AreEqual(
                0f,
                result.y,
                0.0001f
            );

            Assert.AreEqual(
                6f,
                result.z,
                0.0001f
            );
        }

        [TestCase(0f, false)]
        [TestCase(0.99f, false)]
        [TestCase(1f, true)]
        [TestCase(1.01f, true)]
        [TestCase(2f, true)]
        public void HasCooldownExpired_UsesOneSecondBoundary(
            float elapsedSeconds,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJSpikedArmorPolicy.HasCooldownExpired(
                    elapsedSeconds
                )
            );
        }

        [TestCase(0f, false)]
        [TestCase(4.99f, false)]
        [TestCase(5f, true)]
        [TestCase(5.01f, true)]
        public void HasDurationExpired_UsesFiveSecondBoundary(
            float elapsedSeconds,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJSpikedArmorPolicy.HasDurationExpired(
                    elapsedSeconds
                )
            );
        }
    }
}
