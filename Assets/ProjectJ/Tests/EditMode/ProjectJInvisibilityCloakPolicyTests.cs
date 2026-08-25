using NUnit.Framework; // EditMode 정책 테스트 사용
using ProjectJ.Items; // 투명 망토 정책 사용

namespace ProjectJ.Tests.EditMode
{
    public sealed class ProjectJInvisibilityCloakPolicyTests
    {
        [Test]
        public void Constants_MatchDesignValues()
        {
            Assert.AreEqual(
                28,
                ProjectJInvisibilityCloakPolicy.NetworkItemId
            );

            Assert.AreEqual(
                5f,
                ProjectJInvisibilityCloakPolicy.DurationSeconds
            );

            Assert.AreEqual(
                2f,
                ProjectJInvisibilityCloakPolicy.ProximityRevealDistance
            );

            Assert.AreEqual(
                0.3f,
                ProjectJInvisibilityCloakPolicy.ShimmerPeriodSeconds
            );

            Assert.AreEqual(
                0.05f,
                ProjectJInvisibilityCloakPolicy.ShimmerVisibleSeconds
            );
        }

        [TestCase(true, true, false, true)]
        [TestCase(false, true, false, false)]
        [TestCase(true, false, false, false)]
        [TestCase(true, true, true, false)]
        public void CanUse_RequiresAuthorityGameplayAndInactiveState(
            bool authorityReady,
            bool gameplayAllowed,
            bool alreadyActive,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJInvisibilityCloakPolicy.CanUse(
                    authorityReady,
                    gameplayAllowed,
                    alreadyActive
                )
            );
        }

        [TestCase(false, false, 100f, ProjectJInvisibilityPresentationMode.Visible)]
        [TestCase(true, false, 100f, ProjectJInvisibilityPresentationMode.Visible)]
        [TestCase(false, true, 2.01f, ProjectJInvisibilityPresentationMode.Hidden)]
        [TestCase(false, true, 2f, ProjectJInvisibilityPresentationMode.ProximityShimmer)]
        [TestCase(false, true, 1.99f, ProjectJInvisibilityPresentationMode.ProximityShimmer)]
        [TestCase(false, true, 0f, ProjectJInvisibilityPresentationMode.ProximityShimmer)]
        public void ResolvePresentationMode_FollowsOwnerAndDistanceRules(
            bool isLocalOwner,
            bool invisible,
            float viewerDistance,
            ProjectJInvisibilityPresentationMode expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJInvisibilityCloakPolicy.ResolvePresentationMode(
                    isLocalOwner,
                    invisible,
                    viewerDistance
                )
            );
        }

        [TestCase(false, true)]
        [TestCase(true, false)]
        public void IsAutoTargetTrackable_IsInverseOfInvisibility(
            bool invisible,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJInvisibilityCloakPolicy.IsAutoTargetTrackable(
                    invisible
                )
            );
        }

        [TestCase(false, false)]
        [TestCase(true, true)]
        public void ShouldBreakForPush_UsesActiveState(
            bool invisible,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJInvisibilityCloakPolicy.ShouldBreakForPush(
                    invisible
                )
            );
        }

        [TestCase(false, true, false, false)]
        [TestCase(true, false, false, false)]
        [TestCase(true, true, true, false)]
        [TestCase(true, true, false, true)]
        public void ShouldBreakForSuccessfulItemUse_OnlyOtherSuccessfulItemsBreak(
            bool invisible,
            bool success,
            bool usedInvisibilityCloak,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJInvisibilityCloakPolicy.ShouldBreakForSuccessfulItemUse(
                    invisible,
                    success,
                    usedInvisibilityCloak
                )
            );
        }

        [TestCase(0f, true)]
        [TestCase(0.049f, true)]
        [TestCase(0.05f, false)]
        [TestCase(0.299f, false)]
        [TestCase(0.3f, true)]
        [TestCase(0.349f, true)]
        [TestCase(0.35f, false)]
        public void IsShimmerVisible_UsesPeriodicBriefReveal(
            float timeSeconds,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJInvisibilityCloakPolicy.IsShimmerVisible(
                    timeSeconds
                )
            );
        }

        [TestCase(0f, 0f)]
        [TestCase(0.075f, 0.035f)]
        [TestCase(0.15f, 0f)]
        [TestCase(0.225f, -0.035f)]
        [TestCase(0.3f, 0f)]
        public void CalculateShimmerOffset_UsesSmallHorizontalWave(
            float timeSeconds,
            float expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJInvisibilityCloakPolicy.CalculateShimmerOffset(
                    timeSeconds
                ),
                0.001f
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
                ProjectJInvisibilityCloakPolicy.HasDurationExpired(
                    elapsedSeconds
                )
            );
        }
    }
}
