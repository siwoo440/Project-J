using NUnit.Framework; // Unity EditMode 테스트 기능
using ProjectJ.Items; // 트램폴린 정책 사용
using UnityEngine; // Vector3 테스트 사용

namespace ProjectJ.Tests.EditMode
{
    public sealed class ProjectJTrampolinePolicyTests
    {
        [Test]
        public void LifetimeSeconds_ReturnsTwelveSeconds()
        {
            Assert.AreEqual(
                12f,
                ProjectJTrampolinePolicy.LifetimeSeconds
            );
        }

        [Test]
        public void MaximumUseCount_ReturnsThree()
        {
            Assert.AreEqual(
                3,
                ProjectJTrampolinePolicy.MaximumUseCount
            );
        }

        [TestCase(0, 7f)]
        [TestCase(1, 9f)]
        [TestCase(2, 11f)]
        [TestCase(3, 0f)]
        [TestCase(4, 0f)]
        [TestCase(-1, 7f)]
        public void GetLaunchSpeed_WithUseCount_ReturnsExpected(
            int useCount,
            float expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJTrampolinePolicy.GetLaunchSpeed(useCount)
            );
        }

        [TestCase(0, 1)]
        [TestCase(1, 2)]
        [TestCase(2, 3)]
        [TestCase(3, 3)]
        [TestCase(99, 3)]
        [TestCase(-1, 1)]
        public void GetNextUseCount_WithCurrentCount_ReturnsExpected(
            int useCount,
            int expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJTrampolinePolicy.GetNextUseCount(useCount)
            );
        }

        [TestCase(0, false)]
        [TestCase(1, false)]
        [TestCase(2, false)]
        [TestCase(3, true)]
        [TestCase(4, true)]
        public void HasConsumedAllUses_WithUseCount_ReturnsExpected(
            int useCount,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJTrampolinePolicy.HasConsumedAllUses(useCount)
            );
        }

        [TestCase(true, true, true)]
        [TestCase(false, true, false)]
        [TestCase(true, false, false)]
        [TestCase(false, false, false)]
        public void CanInstall_WithState_ReturnsExpected(
            bool runnerReady,
            bool gameplayAllowed,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJTrampolinePolicy.CanInstall(
                    runnerReady,
                    gameplayAllowed
                )
            );
        }

        [TestCase(1f, 1f, true)]
        [TestCase(0.65f, 1f, true)]
        [TestCase(0.649f, 1f, false)]
        [TestCase(1f, 2.5f, true)]
        [TestCase(1f, 2.501f, false)]
        [TestCase(1f, -0.1f, false)]
        public void IsValidInstallSurface_WithNormalAndDistance_ReturnsExpected(
            float normalY,
            float distance,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJTrampolinePolicy.IsValidInstallSurface(
                    normalY,
                    distance
                )
            );
        }

        [TestCase(0f, 0f, true)]
        [TestCase(0.9f, 0f, true)]
        [TestCase(0.901f, 0f, false)]
        [TestCase(0f, -0.25f, true)]
        [TestCase(0f, 0.75f, true)]
        [TestCase(0f, 0.751f, false)]
        public void IsWithinActivationArea_WithOffsets_ReturnsExpected(
            float horizontalDistance,
            float verticalOffset,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJTrampolinePolicy.IsWithinActivationArea(
                    horizontalDistance,
                    verticalOffset
                )
            );
        }

        [TestCase(true, true, 0, true, 0f, 0f, 0f, true)]
        [TestCase(false, true, 0, true, 0f, 0f, 0f, false)]
        [TestCase(true, false, 0, true, 0f, 0f, 0f, false)]
        [TestCase(true, true, 3, true, 0f, 0f, 0f, false)]
        [TestCase(true, true, 0, false, 1f, 0f, 0f, false)]
        [TestCase(true, true, 0, false, -0.1f, 0f, 0f, true)]
        [TestCase(true, true, 0, true, 0f, 1f, 0f, false)]
        public void CanActivateOwner_WithState_ReturnsExpected(
            bool ownerValid,
            bool gameplayAllowed,
            int useCount,
            bool grounded,
            float verticalVelocity,
            float horizontalDistance,
            float verticalOffset,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJTrampolinePolicy.CanActivateOwner(
                    ownerValid,
                    gameplayAllowed,
                    useCount,
                    grounded,
                    verticalVelocity,
                    horizontalDistance,
                    verticalOffset
                )
            );
        }

        [TestCase(true, false, 0, false)]
        [TestCase(false, false, 0, true)]
        [TestCase(true, true, 0, true)]
        [TestCase(true, false, 3, true)]
        public void ShouldDespawn_WithState_ReturnsExpected(
            bool lifetimeActive,
            bool ownerMissing,
            int useCount,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJTrampolinePolicy.ShouldDespawn(
                    lifetimeActive,
                    ownerMissing,
                    useCount
                )
            );
        }

        [Test]
        public void ResolveLaunchVelocity_SetsVerticalAndPreservesHorizontal()
        {
            Vector3 result =
                ProjectJTrampolinePolicy.ResolveLaunchVelocity(
                    new Vector3(3f, -5f, 4f),
                    9f
                );

            Assert.AreEqual(3f, result.x, 0.0001f);
            Assert.AreEqual(9f, result.y, 0.0001f);
            Assert.AreEqual(4f, result.z, 0.0001f);
        }
    }
}
