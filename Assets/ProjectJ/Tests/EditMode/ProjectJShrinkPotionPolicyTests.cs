using NUnit.Framework; // EditMode 정책 테스트 사용
using ProjectJ.Items; // 소형화 물약 정책 사용

namespace ProjectJ.Tests.EditMode
{
    public sealed class ProjectJShrinkPotionPolicyTests
    {
        [Test]
        public void Constants_MatchDesignValues()
        {
            Assert.AreEqual(
                6f,
                ProjectJShrinkPotionPolicy.DurationSeconds
            );

            Assert.AreEqual(
                0.8f,
                ProjectJShrinkPotionPolicy.ScaleMultiplier
            );
        }

        [TestCase(2f, 1.6f)]
        [TestCase(1f, 0.8f)]
        [TestCase(0f, 0f)]
        [TestCase(-1f, 0f)]
        public void CalculateColliderHeight_AppliesEightyPercent(
            float baseHeight,
            float expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJShrinkPotionPolicy.CalculateColliderHeight(
                    baseHeight,
                    true
                ),
                0.0001f
            );
        }

        [TestCase(0.4f, 0.32f)]
        [TestCase(1f, 0.8f)]
        [TestCase(0f, 0f)]
        [TestCase(-1f, 0f)]
        public void CalculateColliderRadius_AppliesEightyPercent(
            float baseRadius,
            float expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJShrinkPotionPolicy.CalculateColliderRadius(
                    baseRadius,
                    true
                ),
                0.0001f
            );
        }

        [TestCase(2f, 2f)]
        [TestCase(1f, 1f)]
        [TestCase(0.4f, 0.4f)]
        public void ColliderSize_WhenInactive_RemainsUnchanged(
            float baseValue,
            float expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJShrinkPotionPolicy.CalculateColliderHeight(
                    baseValue,
                    false
                ),
                0.0001f
            );

            Assert.AreEqual(
                expected,
                ProjectJShrinkPotionPolicy.CalculateColliderRadius(
                    baseValue,
                    false
                ),
                0.0001f
            );
        }

        [TestCase(ProjectJShrinkPotionState.Inactive, false)]
        [TestCase(ProjectJShrinkPotionState.Active, true)]
        [TestCase(ProjectJShrinkPotionState.RestorePending, true)]
        public void ShouldApplyShrink_ReturnsExpected(
            ProjectJShrinkPotionState state,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJShrinkPotionPolicy.ShouldApplyShrink(
                    state
                )
            );
        }

        [TestCase(true, true, ProjectJShrinkPotionState.Inactive, true)]
        [TestCase(false, true, ProjectJShrinkPotionState.Inactive, false)]
        [TestCase(true, false, ProjectJShrinkPotionState.Inactive, false)]
        [TestCase(true, true, ProjectJShrinkPotionState.Active, false)]
        [TestCase(true, true, ProjectJShrinkPotionState.RestorePending, false)]
        public void CanUse_OnlyAllowsInactiveGameplayState(
            bool runnerReady,
            bool gameplayAllowed,
            ProjectJShrinkPotionState state,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJShrinkPotionPolicy.CanUse(
                    runnerReady,
                    gameplayAllowed,
                    state
                )
            );
        }

        [TestCase(true, ProjectJShrinkPotionState.Inactive)]
        [TestCase(false, ProjectJShrinkPotionState.RestorePending)]
        public void ResolveExpiredState_UsesRestoreClearance(
            bool canRestore,
            ProjectJShrinkPotionState expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJShrinkPotionPolicy.ResolveExpiredState(
                    canRestore
                )
            );
        }

        [TestCase(true, ProjectJShrinkPotionState.Inactive)]
        [TestCase(false, ProjectJShrinkPotionState.RestorePending)]
        public void ResolvePendingState_WaitsUntilSpaceIsSafe(
            bool canRestore,
            ProjectJShrinkPotionState expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJShrinkPotionPolicy.ResolvePendingState(
                    canRestore
                )
            );
        }

        [TestCase(5f, 5f)]
        [TestCase(8f, 8f)]
        [TestCase(0f, 0f)]
        public void MovementSpeed_IsNotChanged(
            float baseSpeed,
            float expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJShrinkPotionPolicy.CalculateMovementSpeed(
                    baseSpeed
                ),
                0.0001f
            );
        }

        [TestCase(7f, 7f)]
        [TestCase(10f, 10f)]
        [TestCase(0f, 0f)]
        public void JumpSpeed_IsNotChanged(
            float baseSpeed,
            float expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJShrinkPotionPolicy.CalculateJumpSpeed(
                    baseSpeed
                ),
                0.0001f
            );
        }

        [TestCase(1f, true, 0.8f)]
        [TestCase(0.5f, true, 0.4f)]
        [TestCase(1.6f, true, 1.28f)]
        [TestCase(1f, false, 1f)]
        public void CalculatePresentationValue_UsesShrinkState(
            float baseValue,
            bool shrinkApplied,
            float expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJShrinkPotionPolicy.CalculatePresentationValue(
                    baseValue,
                    shrinkApplied
                ),
                0.0001f
            );
        }
    }
}
