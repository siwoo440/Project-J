using NUnit.Framework;
using ProjectJ.Push;
using UnityEngine;

namespace ProjectJ.Tests.EditMode
{
    public sealed class
        PlayerExternalForceAccumulatorTests
    {
        private GameObject player;
        private Rigidbody body;
        private PlayerExternalForceAccumulator
            accumulator;

        [SetUp]
        public void SetUp()
        {
            player =
                new GameObject(
                    "External Force Test Player"
                );

            body =
                player.AddComponent<
                    Rigidbody
                >();

            body.useGravity =
                false;

            accumulator =
                player.AddComponent<
                    PlayerExternalForceAccumulator
                >();

            accumulator.Configure(
                body,
                12f,
                0.05f
            );
        }

        [TearDown]
        public void TearDown()
        {
            if (player != null)
            {
                Object.DestroyImmediate(
                    player
                );
            }
        }

        [Test]
        public void MultipleVelocityChanges_AreSummed()
        {
            accumulator.AddVelocityChange(
                new Vector3(
                    4f,
                    0f,
                    0f
                )
            );

            accumulator.AddVelocityChange(
                new Vector3(
                    0f,
                    0f,
                    3f
                )
            );

            Assert.AreEqual(
                new Vector3(
                    4f,
                    0f,
                    3f
                ),
                accumulator
                    .CurrentExternalVelocity
            );

            Assert.AreEqual(
                new Vector3(
                    4f,
                    0f,
                    3f
                ),
                body.linearVelocity
            );
        }

        [Test]
        public void OppositeVelocityChanges_Cancel()
        {
            accumulator.AddVelocityChange(
                new Vector3(
                    6f,
                    0f,
                    0f
                )
            );

            accumulator.AddVelocityChange(
                new Vector3(
                    -6f,
                    0f,
                    0f
                )
            );

            Assert.AreEqual(
                Vector3.zero,
                accumulator
                    .CurrentExternalVelocity
            );

            Assert.AreEqual(
                Vector3.zero,
                body.linearVelocity
            );
        }

        [Test]
        public void VerticalComponent_IsIgnored()
        {
            body.linearVelocity =
                new Vector3(
                    0f,
                    2f,
                    0f
                );

            accumulator.AddVelocityChange(
                new Vector3(
                    3f,
                    10f,
                    4f
                )
            );

            Assert.AreEqual(
                2f,
                body.linearVelocity.y,
                0.0001f
            );

            Assert.AreEqual(
                0f,
                accumulator
                    .CurrentExternalVelocity.y,
                0.0001f
            );
        }

        [Test]
        public void DecayStep_ReducesSlidingAndPreservesY()
        {
            body.linearVelocity =
                new Vector3(
                    0f,
                    2f,
                    0f
                );

            accumulator.AddVelocityChange(
                new Vector3(
                    6f,
                    0f,
                    0f
                )
            );

            accumulator.ApplyDecayStep(
                0.1f
            );

            Assert.AreEqual(
                4.8f,
                accumulator
                    .CurrentExternalVelocity.x,
                0.0001f
            );

            Assert.AreEqual(
                4.8f,
                body.linearVelocity.x,
                0.0001f
            );

            Assert.AreEqual(
                2f,
                body.linearVelocity.y,
                0.0001f
            );
        }

        [Test]
        public void LargeDecayStep_StopsExternalVelocity()
        {
            accumulator.AddVelocityChange(
                new Vector3(
                    6f,
                    0f,
                    0f
                )
            );

            accumulator.ApplyDecayStep(
                1f
            );

            Assert.AreEqual(
                Vector3.zero,
                accumulator
                    .CurrentExternalVelocity
            );

            Assert.AreEqual(
                Vector3.zero,
                body.linearVelocity
            );
        }

        [Test]
        public void DecayStep_DoesNotReverseAfterCollisionStop()
        {
            accumulator.AddVelocityChange(
                new Vector3(
                    6f,
                    0f,
                    0f
                )
            );

            body.linearVelocity =
                Vector3.zero;

            accumulator.ApplyDecayStep(
                0.1f
            );

            Assert.AreEqual(
                Vector3.zero,
                body.linearVelocity
            );
        }

        [Test]
        public void ClearExternalVelocity_PreservesVerticalSpeed()
        {
            body.linearVelocity =
                new Vector3(
                    0f,
                    3f,
                    0f
                );

            accumulator.AddVelocityChange(
                new Vector3(
                    5f,
                    0f,
                    0f
                )
            );

            accumulator.ClearExternalVelocity(
                true
            );

            Assert.AreEqual(
                0f,
                body.linearVelocity.x,
                0.0001f
            );

            Assert.AreEqual(
                3f,
                body.linearVelocity.y,
                0.0001f
            );

            Assert.AreEqual(
                Vector3.zero,
                accumulator
                    .CurrentExternalVelocity
            );
        }
    }
}
