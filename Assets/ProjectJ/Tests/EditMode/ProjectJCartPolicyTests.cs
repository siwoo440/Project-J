using NUnit.Framework; // Unity EditMode 테스트 기능
using ProjectJ.Items; // 카트 정책 사용
using UnityEngine; // Vector3 테스트 사용

namespace ProjectJ.Tests.EditMode
{
    public sealed class ProjectJCartPolicyTests
    {
        [Test]
        public void LifetimeSeconds_ReturnsEightSeconds()
        {
            Assert.AreEqual(8f, ProjectJCartPolicy.LifetimeSeconds);
        }

        [Test]
        public void MovementSpeed_ReturnsTenMetersPerSecond()
        {
            Assert.AreEqual(10f, ProjectJCartPolicy.MovementSpeed);
        }

        [Test]
        public void MaximumRouteNodes_ReturnsThree()
        {
            Assert.AreEqual(3, ProjectJCartPolicy.MaximumRouteNodes);
        }

        [Test]
        public void SidePushSpeed_ReturnsSixMetersPerSecond()
        {
            Assert.AreEqual(6f, ProjectJCartPolicy.SidePushSpeed);
        }

        [Test]
        public void RehitCooldownSeconds_ReturnsHalfSecond()
        {
            Assert.AreEqual(0.5f, ProjectJCartPolicy.RehitCooldownSeconds);
        }

        [TestCase(true, true, false, true, false, true)]
        [TestCase(false, true, false, true, false, false)]
        [TestCase(true, false, false, true, false, false)]
        [TestCase(true, true, true, true, false, false)]
        [TestCase(true, true, false, false, false, false)]
        [TestCase(true, true, false, true, true, false)]
        public void CanUse_WithState_ReturnsExpected(
            bool runnerReady,
            bool gameplayAllowed,
            bool alreadyRiding,
            bool hasStartNode,
            bool hasExistingCart,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJCartPolicy.CanUse(
                    runnerReady,
                    gameplayAllowed,
                    alreadyRiding,
                    hasStartNode,
                    hasExistingCart
                )
            );
        }

        [TestCase(0f, 0f)]
        [TestCase(0.02f, 0.2f)]
        [TestCase(0.05f, 0.5f)]
        [TestCase(0.1f, 1f)]
        [TestCase(-1f, 0f)]
        public void CalculateTravelDistance_WithDeltaTime_ReturnsExpected(
            float deltaTime,
            float expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJCartPolicy.CalculateTravelDistance(deltaTime),
                0.0001f
            );
        }

        [TestCase(0f, true)]
        [TestCase(0.399f, true)]
        [TestCase(0.4f, true)]
        [TestCase(0.401f, false)]
        [TestCase(-1f, true)]
        public void HasReachedNode_WithDistance_ReturnsExpected(
            float distance,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJCartPolicy.HasReachedNode(distance)
            );
        }

        [TestCase(0, true, true)]
        [TestCase(1, true, true)]
        [TestCase(2, true, true)]
        [TestCase(3, true, false)]
        [TestCase(4, true, false)]
        [TestCase(1, false, false)]
        public void CanAdvanceToNextNode_WithState_ReturnsExpected(
            int visitedNodeCount,
            bool hasNextNode,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJCartPolicy.CanAdvanceToNextNode(
                    visitedNodeCount,
                    hasNextNode
                )
            );
        }

        [TestCase(true, true, true, false, false)]
        [TestCase(false, true, true, false, true)]
        [TestCase(true, false, true, false, true)]
        [TestCase(true, true, false, false, true)]
        [TestCase(true, true, true, true, true)]
        public void ShouldFinishRide_WithState_ReturnsExpected(
            bool lifetimeActive,
            bool gameplayAllowed,
            bool ownerValid,
            bool routeEnded,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJCartPolicy.ShouldFinishRide(
                    lifetimeActive,
                    gameplayAllowed,
                    ownerValid,
                    routeEnded
                )
            );
        }

        [TestCase(0f, false)]
        [TestCase(0.49f, false)]
        [TestCase(0.5f, true)]
        [TestCase(0.51f, true)]
        [TestCase(10f, true)]
        [TestCase(-1f, false)]
        public void IsRehitReady_WithElapsedTime_ReturnsExpected(
            float elapsedSeconds,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJCartPolicy.IsRehitReady(elapsedSeconds)
            );
        }

        [Test]
        public void ResolveSidePushDirection_TargetOnRight_ReturnsRight()
        {
            Vector3 result = ProjectJCartPolicy.ResolveSidePushDirection(
                Vector3.right,
                Vector3.zero,
                new Vector3(3f, 2f, 0f)
            );

            Assert.AreEqual(1f, result.x, 0.0001f);
            Assert.AreEqual(0f, result.y, 0.0001f);
            Assert.AreEqual(0f, result.z, 0.0001f);
        }

        [Test]
        public void ResolveSidePushDirection_TargetOnLeft_ReturnsLeft()
        {
            Vector3 result = ProjectJCartPolicy.ResolveSidePushDirection(
                Vector3.right,
                Vector3.zero,
                new Vector3(-3f, 2f, 0f)
            );

            Assert.AreEqual(-1f, result.x, 0.0001f);
            Assert.AreEqual(0f, result.y, 0.0001f);
            Assert.AreEqual(0f, result.z, 0.0001f);
        }

        [Test]
        public void ResolveSidePushDirection_RemovesVerticalComponent()
        {
            Vector3 result = ProjectJCartPolicy.ResolveSidePushDirection(
                new Vector3(1f, 5f, 0f),
                Vector3.zero,
                Vector3.right
            );

            Assert.AreEqual(1f, result.x, 0.0001f);
            Assert.AreEqual(0f, result.y, 0.0001f);
            Assert.AreEqual(0f, result.z, 0.0001f);
        }

        [Test]
        public void ResolveSidePushDirection_ZeroRightUsesWorldRight()
        {
            Vector3 result = ProjectJCartPolicy.ResolveSidePushDirection(
                Vector3.zero,
                Vector3.zero,
                Vector3.zero
            );

            Assert.AreEqual(1f, result.x, 0.0001f);
            Assert.AreEqual(0f, result.y, 0.0001f);
            Assert.AreEqual(0f, result.z, 0.0001f);
        }

        [TestCase(0f, true)]
        [TestCase(3.999f, true)]
        [TestCase(4f, true)]
        [TestCase(4.001f, false)]
        [TestCase(-1f, true)]
        public void IsWithinStartNodeSearchRadius_WithDistance_ReturnsExpected(
            float distance,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                ProjectJCartPolicy.IsWithinStartNodeSearchRadius(distance)
            );
        }
    }
}
