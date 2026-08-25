using NUnit.Framework; // EditMode 정책 테스트 사용
using ProjectJ.Items; // 저격 물총 정책 사용
using UnityEngine; // Vector3 사용

namespace ProjectJ.Tests.EditMode
{
    public sealed class ProjectJSniperWaterGunPolicyTests
    {
        [Test]
        public void Constants_MatchItemDesign()
        {
            Assert.AreEqual(29, ProjectJSniperWaterGunPolicy.NetworkItemId);
            Assert.AreEqual(0.8f, ProjectJSniperWaterGunPolicy.PreparationSeconds);
            Assert.AreEqual(50f, ProjectJSniperWaterGunPolicy.RangeMeters);
            Assert.AreEqual(12f, ProjectJSniperWaterGunPolicy.HorizontalVelocityChange);
            Assert.AreEqual(2, ProjectJSniperWaterGunPolicy.Zoom2X);
            Assert.AreEqual(4, ProjectJSniperWaterGunPolicy.Zoom4X);
        }

        [TestCase(true, true, false, true, true)]
        [TestCase(false, true, false, true, false)]
        [TestCase(true, false, false, true, false)]
        [TestCase(true, true, true, true, false)]
        [TestCase(true, true, false, false, false)]
        public void CanBeginAim_RequiresValidAuthorityGameplayAndSelectedItem(
            bool authorityReady,
            bool gameplayAllowed,
            bool alreadyAiming,
            bool slotHasSniper,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJSniperWaterGunPolicy.CanBeginAim(
                    authorityReady,
                    gameplayAllowed,
                    alreadyAiming,
                    slotHasSniper
                )
            );
        }

        [TestCase(true, true, true, true, true, false)]
        [TestCase(false, true, true, true, true, true)]
        [TestCase(true, false, true, true, true, true)]
        [TestCase(true, true, false, true, true, true)]
        [TestCase(true, true, true, false, true, true)]
        [TestCase(true, true, true, true, false, true)]
        public void ShouldCancelAim_UsesGameplayHoldSlotItemAndRespawnState(
            bool gameplayAllowed,
            bool useHeld,
            bool selectedSlotMatches,
            bool slotStillContainsSniper,
            bool sameRespawnLife,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJSniperWaterGunPolicy.ShouldCancelAim(
                    gameplayAllowed,
                    useHeld,
                    selectedSlotMatches,
                    slotStillContainsSniper,
                    sameRespawnLife
                )
            );
        }

        [TestCase(0f, true)]
        [TestCase(49.999f, true)]
        [TestCase(50f, true)]
        [TestCase(50.001f, false)]
        [TestCase(100f, false)]
        public void IsInRange_UsesInclusiveFiftyMeterBoundary(
            float distance,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJSniperWaterGunPolicy.IsInRange(distance)
            );
        }

        [TestCase(0.8f, 0f)]
        [TestCase(0.4f, 0.5f)]
        [TestCase(0f, 1f)]
        [TestCase(-0.1f, 1f)]
        public void CalculatePreparationProgress_MapsRemainingTimeToZeroOne(
            float remainingSeconds,
            float expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJSniperWaterGunPolicy.CalculatePreparationProgress(
                    remainingSeconds
                ),
                0.0001f
            );
        }

        [Test]
        public void ResolveAimDirection_NormalizesValidDirection()
        {
            Vector3 result =
                ProjectJSniperWaterGunPolicy.ResolveAimDirection(
                    new Vector3(3f, 4f, 0f),
                    Vector3.forward
                );

            Assert.AreEqual(1f, result.magnitude, 0.0001f);
            Assert.AreEqual(0.6f, result.x, 0.0001f);
            Assert.AreEqual(0.8f, result.y, 0.0001f);
        }

        [Test]
        public void ResolveAimDirection_UsesFallbackForZeroDirection()
        {
            Vector3 result =
                ProjectJSniperWaterGunPolicy.ResolveAimDirection(
                    Vector3.zero,
                    Vector3.right
                );

            Assert.AreEqual(Vector3.right, result);
        }

        [Test]
        public void ResolveAimDirection_UsesFallbackForNaNDirection()
        {
            Vector3 result =
                ProjectJSniperWaterGunPolicy.ResolveAimDirection(
                    new Vector3(float.NaN, 0f, 1f),
                    Vector3.forward
                );

            Assert.AreEqual(Vector3.forward, result);
        }

        [Test]
        public void ResolveAimDirection_UsesForwardWhenFallbackIsInvalid()
        {
            Vector3 result =
                ProjectJSniperWaterGunPolicy.ResolveAimDirection(
                    Vector3.zero,
                    new Vector3(float.PositiveInfinity, 0f, 0f)
                );

            Assert.AreEqual(Vector3.forward, result);
        }

        [Test]
        public void CreateHorizontalVelocityChange_RemovesVerticalComponent()
        {
            Vector3 result =
                ProjectJSniperWaterGunPolicy.CreateHorizontalVelocityChange(
                    new Vector3(1f, 5f, 1f),
                    Vector3.forward
                );

            Assert.AreEqual(0f, result.y, 0.0001f);
            Assert.AreEqual(
                ProjectJSniperWaterGunPolicy.HorizontalVelocityChange,
                result.magnitude,
                0.0001f
            );
        }

        [Test]
        public void CreateHorizontalVelocityChange_UsesFallbackForVerticalAim()
        {
            Vector3 result =
                ProjectJSniperWaterGunPolicy.CreateHorizontalVelocityChange(
                    Vector3.up,
                    Vector3.right
                );

            Assert.AreEqual(
                new Vector3(
                    ProjectJSniperWaterGunPolicy.HorizontalVelocityChange,
                    0f,
                    0f
                ),
                result
            );
        }

        [TestCase(60f, 2, 30f)]
        [TestCase(60f, 4, 15f)]
        [TestCase(68f, 2, 34f)]
        [TestCase(68f, 4, 17f)]
        public void CalculateZoomedFieldOfView_DividesBaseFov(
            float baseFov,
            int zoomMultiplier,
            float expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJSniperWaterGunPolicy.CalculateZoomedFieldOfView(
                    baseFov,
                    zoomMultiplier
                ),
                0.0001f
            );
        }

        [TestCase(2, 1f, 4)]
        [TestCase(2, -1f, 4)]
        [TestCase(4, 1f, 2)]
        [TestCase(4, -1f, 2)]
        [TestCase(2, 0f, 2)]
        [TestCase(4, 0f, 4)]
        public void ResolveZoomMultiplier_TogglesBetweenTwoAndFour(
            int current,
            float scrollDelta,
            int expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJSniperWaterGunPolicy.ResolveZoomMultiplier(
                    current,
                    scrollDelta
                )
            );
        }

        [TestCase(0f, false)]
        [TestCase(0.00001f, false)]
        [TestCase(0.001f, true)]
        [TestCase(1f, true)]
        public void IsMeaningfulScroll_UsesSmallDeadZone(
            float scrollDelta,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJSniperWaterGunPolicy.IsMeaningfulScroll(
                    scrollDelta
                )
            );
        }
    }
}
