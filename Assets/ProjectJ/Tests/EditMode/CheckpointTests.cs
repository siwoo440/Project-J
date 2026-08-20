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
        public void ActivateCheckpoint_SavesIdAndRespawnPoint()
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
        public void LaterCheckpoint_ReplacesCurrentCheckpoint()
        {
            CheckpointComponent cp1 =
                CreateCheckpoint(
                    CheckpointId.CP1,
                    Vector3.zero
                );

            CheckpointComponent cp2 =
                CreateCheckpoint(
                    CheckpointId.CP2,
                    Vector3.one
                );

            tracker.ActivateCheckpoint(
                cp1
            );

            tracker.ActivateCheckpoint(
                cp2
            );

            Assert.AreEqual(
                CheckpointId.CP2,
                tracker.CurrentCheckpointId
            );

            Object.DestroyImmediate(
                cp1.gameObject
            );

            Object.DestroyImmediate(
                cp2.gameObject
            );
        }

        [Test]
        public void BasicDay30Rule_AllowsDirectReplacement()
        {
            CheckpointComponent cp4 =
                CreateCheckpoint(
                    CheckpointId.CP4,
                    Vector3.zero
                );

            CheckpointComponent cp1 =
                CreateCheckpoint(
                    CheckpointId.CP1,
                    Vector3.one
                );

            tracker.ActivateCheckpoint(
                cp4
            );

            tracker.ActivateCheckpoint(
                cp1
            );

            Assert.AreEqual(
                CheckpointId.CP1,
                tracker.CurrentCheckpointId
            );

            Object.DestroyImmediate(
                cp4.gameObject
            );

            Object.DestroyImmediate(
                cp1.gameObject
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
