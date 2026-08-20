using NUnit.Framework;
using ProjectJ.Checkpoint;
using CheckpointComponent =
    ProjectJ.Checkpoint.Checkpoint;
using UnityEngine;

namespace ProjectJ.Tests.EditMode
{
    public sealed class PlayerFallTrackerTests
    {
        private GameObject playerObject;
        private GameObject limitsObject;
        private PlayerCheckpointTracker
            checkpointTracker;
        private PlayerFallTracker
            fallTracker;
        private CheckpointFallLimitSet
            fallLimitSet;

        [SetUp]
        public void SetUp()
        {
            limitsObject =
                new GameObject(
                    "Fall Limits"
                );

            fallLimitSet =
                limitsObject.AddComponent<
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
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(
                playerObject
            );

            Object.DestroyImmediate(
                limitsObject
            );
        }

        [Test]
        public void FallLimitSet_ReturnsExpectedLimits()
        {
            Assert.AreEqual(
                -20f,
                fallLimitSet.GetFallLimitY(
                    CheckpointId.Start
                )
            );

            Assert.AreEqual(
                180f,
                fallLimitSet.GetFallLimitY(
                    CheckpointId.CP1
                )
            );

            Assert.AreEqual(
                380f,
                fallLimitSet.GetFallLimitY(
                    CheckpointId.CP2
                )
            );

            Assert.AreEqual(
                580f,
                fallLimitSet.GetFallLimitY(
                    CheckpointId.CP3
                )
            );

            Assert.AreEqual(
                780f,
                fallLimitSet.GetFallLimitY(
                    CheckpointId.CP4
                )
            );

            Assert.IsTrue(
                fallLimitSet
                    .HasAscendingLimits()
            );
        }

        [Test]
        public void StartLimit_OnlyFallsBelowLimit()
        {
            bool atLimit =
                fallTracker.EvaluateHeight(
                    -20f
                );

            Assert.IsFalse(
                atLimit
            );

            bool belowLimit =
                fallTracker.EvaluateHeight(
                    -20.01f
                );

            Assert.IsTrue(
                belowLimit
            );

            Assert.IsTrue(
                fallTracker.IsFallen
            );
        }

        [Test]
        public void SkippedCheckpoint_UsesHigherFallLimit()
        {
            CheckpointComponent cp3 =
                CreateCheckpoint(
                    CheckpointId.CP3,
                    600f
                );

            checkpointTracker
                .ActivateCheckpoint(
                    cp3
                );

            fallTracker
                .RefreshActiveFallLimit();

            Assert.AreEqual(
                580f,
                fallTracker.ActiveFallLimitY
            );

            Object.DestroyImmediate(
                cp3.gameObject
            );
        }

        [Test]
        public void LowerCheckpoint_DoesNotLowerFallLimit()
        {
            CheckpointComponent cp3 =
                CreateCheckpoint(
                    CheckpointId.CP3,
                    600f
                );

            CheckpointComponent cp1 =
                CreateCheckpoint(
                    CheckpointId.CP1,
                    200f
                );

            checkpointTracker
                .ActivateCheckpoint(
                    cp3
                );

            checkpointTracker
                .ActivateCheckpoint(
                    cp1
                );

            fallTracker
                .RefreshActiveFallLimit();

            Assert.AreEqual(
                CheckpointId.CP3,
                checkpointTracker
                    .CurrentCheckpointId
            );

            Assert.AreEqual(
                580f,
                fallTracker.ActiveFallLimitY
            );

            Object.DestroyImmediate(
                cp3.gameObject
            );

            Object.DestroyImmediate(
                cp1.gameObject
            );
        }

        [Test]
        public void SameHeight_CanBeSafeBeforeCheckpointAndFallenAfter()
        {
            fallLimitSet.Configure(
                -5f,
                -4f,
                -3f,
                -2f,
                -1f
            );

            fallTracker
                .ResetFallenState();

            bool beforeCheckpoint =
                fallTracker.EvaluateHeight(
                    -1.5f
                );

            Assert.IsFalse(
                beforeCheckpoint
            );

            CheckpointComponent cp4 =
                CreateCheckpoint(
                    CheckpointId.CP4,
                    0f
                );

            checkpointTracker
                .ActivateCheckpoint(
                    cp4
                );

            fallTracker
                .ResetFallenState();

            bool afterCheckpoint =
                fallTracker.EvaluateHeight(
                    -1.5f
                );

            Assert.IsTrue(
                afterCheckpoint
            );

            Object.DestroyImmediate(
                cp4.gameObject
            );
        }

        [Test]
        public void FellEvent_FiresOnlyOnceUntilReset()
        {
            int eventCount = 0;

            fallTracker.Fell +=
                () =>
                {
                    eventCount++;
                };

            bool first =
                fallTracker.EvaluateHeight(
                    -21f
                );

            bool second =
                fallTracker.EvaluateHeight(
                    -50f
                );

            Assert.IsTrue(
                first
            );

            Assert.IsFalse(
                second
            );

            Assert.AreEqual(
                1,
                eventCount
            );
        }

        [Test]
        public void ResetFallenState_AllowsNewFallDetection()
        {
            int eventCount = 0;

            fallTracker.Fell +=
                () =>
                {
                    eventCount++;
                };

            fallTracker.EvaluateHeight(
                -21f
            );

            fallTracker
                .ResetFallenState();

            bool detectedAgain =
                fallTracker.EvaluateHeight(
                    -21f
                );

            Assert.IsTrue(
                detectedAgain
            );

            Assert.AreEqual(
                2,
                eventCount
            );
        }

        private static CheckpointComponent
            CreateCheckpoint(
                CheckpointId id,
                float worldY
            )
        {
            GameObject checkpointObject =
                new GameObject(
                    id.ToString()
                );

            checkpointObject.transform.position =
                new Vector3(
                    0f,
                    worldY,
                    0f
                );

            BoxCollider collider =
                checkpointObject.AddComponent<
                    BoxCollider
                >();

            collider.isTrigger = true;

            GameObject respawnObject =
                new GameObject(
                    "RespawnPoint"
                );

            respawnObject.transform.SetParent(
                checkpointObject.transform,
                false
            );

            CheckpointComponent checkpoint =
                checkpointObject.AddComponent<
                    CheckpointComponent
                >();

            checkpoint.Configure(
                id,
                respawnObject.transform
            );

            return checkpoint;
        }
    }
}
