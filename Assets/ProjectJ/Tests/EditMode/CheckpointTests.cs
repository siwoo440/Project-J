using NUnit.Framework;
using ProjectJ.Checkpoint;
using CheckpointComponent =
    ProjectJ.Checkpoint.Checkpoint;
using UnityEngine;

namespace ProjectJ.Tests.EditMode
{
    public sealed class CheckpointTests
    {
        private GameObject playerObject;
        private PlayerCheckpointTracker tracker;

        [SetUp]
        public void SetUp()
        {
            playerObject =
                new GameObject(
                    "Player"
                );

            playerObject.transform.position =
                new Vector3(
                    1f,
                    2f,
                    3f
                );

            tracker =
                playerObject.AddComponent<
                    PlayerCheckpointTracker
                >();

            tracker.CaptureStartPoint();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(
                playerObject
            );
        }

        [Test]
        public void NewTracker_StartsAtStart()
        {
            Assert.AreEqual(
                CheckpointId.Start,
                tracker.CurrentCheckpointId
            );

            Assert.AreEqual(
                playerObject.transform.position,
                tracker.RespawnPosition
            );
        }

        [Test]
        public void HigherCheckpoint_SavesIdAndRespawnPoint()
        {
            CheckpointComponent checkpoint =
                CreateCheckpoint(
                    CheckpointId.CP1,
                    new Vector3(
                        10f,
                        20f,
                        30f
                    )
                );

            bool activated =
                tracker.ActivateCheckpoint(
                    checkpoint
                );

            Assert.IsTrue(
                activated
            );

            Assert.AreEqual(
                CheckpointId.CP1,
                tracker.CurrentCheckpointId
            );

            Assert.AreEqual(
                checkpoint.RespawnPosition,
                tracker.RespawnPosition
            );

            Object.DestroyImmediate(
                checkpoint.gameObject
            );
        }

        [Test]
        public void SkippingLowerCheckpoints_IsAllowed()
        {
            CheckpointComponent cp3 =
                CreateCheckpoint(
                    CheckpointId.CP3,
                    new Vector3(
                        0f,
                        30f,
                        0f
                    )
                );

            bool activated =
                tracker.ActivateCheckpoint(
                    cp3
                );

            Assert.IsTrue(
                activated
            );

            Assert.AreEqual(
                CheckpointId.CP3,
                tracker.CurrentCheckpointId
            );

            Assert.AreEqual(
                cp3.RespawnPosition,
                tracker.RespawnPosition
            );

            Object.DestroyImmediate(
                cp3.gameObject
            );
        }

        [Test]
        public void HigherCheckpoint_ReplacesCurrentCheckpoint()
        {
            CheckpointComponent cp1 =
                CreateCheckpoint(
                    CheckpointId.CP1,
                    Vector3.zero
                );

            CheckpointComponent cp3 =
                CreateCheckpoint(
                    CheckpointId.CP3,
                    Vector3.one
                );

            tracker.ActivateCheckpoint(
                cp1
            );

            bool activated =
                tracker.ActivateCheckpoint(
                    cp3
                );

            Assert.IsTrue(
                activated
            );

            Assert.AreEqual(
                CheckpointId.CP3,
                tracker.CurrentCheckpointId
            );

            Object.DestroyImmediate(
                cp1.gameObject
            );

            Object.DestroyImmediate(
                cp3.gameObject
            );
        }

        [Test]
        public void LowerCheckpoint_CannotReplaceHighestCheckpoint()
        {
            CheckpointComponent cp4 =
                CreateCheckpoint(
                    CheckpointId.CP4,
                    new Vector3(
                        0f,
                        40f,
                        0f
                    )
                );

            CheckpointComponent cp1 =
                CreateCheckpoint(
                    CheckpointId.CP1,
                    new Vector3(
                        0f,
                        10f,
                        0f
                    )
                );

            tracker.ActivateCheckpoint(
                cp4
            );

            Vector3 savedRespawnPosition =
                tracker.RespawnPosition;

            Quaternion savedRespawnRotation =
                tracker.RespawnRotation;

            bool activated =
                tracker.ActivateCheckpoint(
                    cp1
                );

            Assert.IsFalse(
                activated
            );

            Assert.AreEqual(
                CheckpointId.CP4,
                tracker.CurrentCheckpointId
            );

            Assert.AreSame(
                cp4,
                tracker.CurrentCheckpoint
            );

            Assert.AreEqual(
                savedRespawnPosition,
                tracker.RespawnPosition
            );

            Assert.AreEqual(
                savedRespawnRotation,
                tracker.RespawnRotation
            );

            Object.DestroyImmediate(
                cp4.gameObject
            );

            Object.DestroyImmediate(
                cp1.gameObject
            );
        }

        [Test]
        public void SameCheckpoint_DoesNotActivateTwice()
        {
            CheckpointComponent cp2 =
                CreateCheckpoint(
                    CheckpointId.CP2,
                    Vector3.zero
                );

            int eventCount = 0;

            tracker.CheckpointChanged +=
                id =>
                {
                    eventCount++;
                };

            bool firstActivation =
                tracker.ActivateCheckpoint(
                    cp2
                );

            bool secondActivation =
                tracker.ActivateCheckpoint(
                    cp2
                );

            Assert.IsTrue(
                firstActivation
            );

            Assert.IsFalse(
                secondActivation
            );

            Assert.AreEqual(
                1,
                eventCount
            );

            Assert.AreEqual(
                CheckpointId.CP2,
                tracker.CurrentCheckpointId
            );

            Object.DestroyImmediate(
                cp2.gameObject
            );
        }

        [Test]
        public void DirectStartToCp4_IsAllowed()
        {
            CheckpointComponent cp4 =
                CreateCheckpoint(
                    CheckpointId.CP4,
                    Vector3.zero
                );

            bool activated =
                tracker.ActivateCheckpoint(
                    cp4
                );

            Assert.IsTrue(
                activated
            );

            Assert.AreEqual(
                CheckpointId.CP4,
                tracker.CurrentCheckpointId
            );

            Object.DestroyImmediate(
                cp4.gameObject
            );
        }

        [Test]
        public void NullCheckpoint_IsRejected()
        {
            bool activated =
                tracker.ActivateCheckpoint(
                    null
                );

            Assert.IsFalse(
                activated
            );

            Assert.AreEqual(
                CheckpointId.Start,
                tracker.CurrentCheckpointId
            );
        }

        [TestCase(
            CheckpointId.CP1,
            CheckpointId.Start,
            true
        )]
        [TestCase(
            CheckpointId.CP3,
            CheckpointId.CP1,
            true
        )]
        [TestCase(
            CheckpointId.CP4,
            CheckpointId.CP3,
            true
        )]
        [TestCase(
            CheckpointId.CP2,
            CheckpointId.CP2,
            false
        )]
        [TestCase(
            CheckpointId.CP1,
            CheckpointId.CP3,
            false
        )]
        [TestCase(
            CheckpointId.Start,
            CheckpointId.CP1,
            false
        )]
        public void IsHigherCheckpoint_ReturnsExpectedResult(
            CheckpointId candidate,
            CheckpointId current,
            bool expected
        )
        {
            Assert.AreEqual(
                expected,
                PlayerCheckpointTracker
                    .IsHigherCheckpoint(
                        candidate,
                        current
                    )
            );
        }

        private static CheckpointComponent
            CreateCheckpoint(
                CheckpointId id,
                Vector3 position
            )
        {
            GameObject checkpointObject =
                new GameObject(
                    id.ToString()
                );

            checkpointObject.transform.position =
                position;

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

            respawnObject.transform.localPosition =
                Vector3.up;

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
