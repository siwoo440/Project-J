using NUnit.Framework;
using ProjectJ.Checkpoint;
using UnityEngine;

namespace ProjectJ.Tests.EditMode
{
    public sealed class PlayerRespawnProtectionTests
    {
        private GameObject playerObject;
        private GameObject fallLimitObject;

        private Rigidbody body;
        private PlayerCheckpointTracker
            checkpointTracker;
        private CheckpointFallLimitSet
            fallLimitSet;
        private PlayerFallTracker
            fallTracker;
        private PlayerRespawnController
            respawnController;
        private PlayerRespawnProtection
            protection;

        [SetUp]
        public void SetUp()
        {
            fallLimitObject =
                new GameObject(
                    "Fall Limits"
                );

            fallLimitSet =
                fallLimitObject.AddComponent<
                    CheckpointFallLimitSet
                >();

            fallLimitSet.Configure(
                -20f,
                180f,
                380f,
                580f,
                780f
            );

            playerObject =
                new GameObject(
                    "Player"
                );

            playerObject.transform.position =
                new Vector3(
                    0f,
                    5f,
                    0f
                );

            body =
                playerObject.AddComponent<
                    Rigidbody
                >();

            body.useGravity = false;

            checkpointTracker =
                playerObject.AddComponent<
                    PlayerCheckpointTracker
                >();

            checkpointTracker
                .CaptureStartPoint();

            fallTracker =
                playerObject.AddComponent<
                    PlayerFallTracker
                >();

            fallTracker.Configure(
                checkpointTracker,
                fallLimitSet
            );

            respawnController =
                playerObject.AddComponent<
                    PlayerRespawnController
                >();

            respawnController.Configure(
                body,
                checkpointTracker,
                fallTracker
            );

            protection =
                playerObject.AddComponent<
                    PlayerRespawnProtection
                >();

            protection.Configure(
                respawnController,
                3f
            );
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(
                playerObject
            );

            Object.DestroyImmediate(
                fallLimitObject
            );
        }

        [Test]
        public void RespawnEvent_StartsThreeSecondProtection()
        {
            bool respawned =
                respawnController
                    .RequestRespawn();

            Assert.IsTrue(
                respawned
            );

            Assert.IsTrue(
                protection.IsProtected
            );

            Assert.AreEqual(
                3f,
                protection
                    .ProtectionDuration
            );

            Assert.Greater(
                protection
                    .RemainingProtectionTime,
                0f
            );
        }

        [Test]
        public void Protection_IsActiveBeforeThreeSeconds()
        {
            protection.StartProtectionAt(
                100d
            );

            bool active =
                protection
                    .EvaluateProtectionAt(
                        102.99d
                    );

            Assert.IsTrue(
                active
            );

            Assert.IsTrue(
                protection.IsProtected
            );

            Assert.Greater(
                protection
                    .RemainingProtectionTime,
                0f
            );
        }

        [Test]
        public void Protection_EndsAtThreeSeconds()
        {
            protection.StartProtectionAt(
                100d
            );

            bool active =
                protection
                    .EvaluateProtectionAt(
                        103d
                    );

            Assert.IsFalse(
                active
            );

            Assert.IsFalse(
                protection.IsProtected
            );

            Assert.AreEqual(
                0f,
                protection
                    .RemainingProtectionTime
            );
        }

        [Test]
        public void HostileEffect_IsBlockedDuringProtection()
        {
            protection.StartProtectionAt(
                10d
            );

            bool accepted =
                protection
                    .TryAcceptHostileEffect();

            Assert.IsFalse(
                accepted
            );

            Assert.IsFalse(
                protection
                    .CanReceiveHostileEffect
            );
        }

        [Test]
        public void HostileEffect_IsAcceptedAfterProtection()
        {
            protection.StartProtectionAt(
                10d
            );

            protection.EvaluateProtectionAt(
                13d
            );

            bool accepted =
                protection
                    .TryAcceptHostileEffect();

            Assert.IsTrue(
                accepted
            );

            Assert.IsTrue(
                protection
                    .CanReceiveHostileEffect
            );
        }

        [Test]
        public void RepeatedRespawn_RestartsProtectionFromThreeSeconds()
        {
            protection.StartProtectionAt(
                10d
            );

            protection.EvaluateProtectionAt(
                12d
            );

            protection.StartProtectionAt(
                12d
            );

            bool activeAtFourPointNineSeconds =
                protection
                    .EvaluateProtectionAt(
                        14.9d
                    );

            Assert.IsTrue(
                activeAtFourPointNineSeconds
            );

            bool activeAtFiveSeconds =
                protection
                    .EvaluateProtectionAt(
                        15d
                    );

            Assert.IsFalse(
                activeAtFiveSeconds
            );
        }

        [Test]
        public void ProtectionEndedEvent_FiresOnce()
        {
            int endedCount = 0;

            protection.ProtectionEnded +=
                () =>
                {
                    endedCount++;
                };

            protection.StartProtectionAt(
                20d
            );

            protection.EvaluateProtectionAt(
                23d
            );

            protection.EvaluateProtectionAt(
                30d
            );

            Assert.AreEqual(
                1,
                endedCount
            );
        }

        [Test]
        public void Protection_DoesNotChangeRigidbodyMotion()
        {
            Vector3 expectedVelocity =
                new Vector3(
                    2f,
                    3f,
                    4f
                );

            Vector3 expectedAngularVelocity =
                new Vector3(
                    1f,
                    2f,
                    3f
                );

            body.linearVelocity =
                expectedVelocity;

            body.angularVelocity =
                expectedAngularVelocity;

            protection.StartProtectionAt(
                50d
            );

            Assert.AreEqual(
                expectedVelocity,
                body.linearVelocity
            );

            Assert.AreEqual(
                expectedAngularVelocity,
                body.angularVelocity
            );

            Assert.IsTrue(
                protection.IsProtected
            );
        }
    }
}
