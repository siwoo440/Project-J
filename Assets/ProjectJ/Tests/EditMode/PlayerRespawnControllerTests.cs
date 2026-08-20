using NUnit.Framework;
using ProjectJ.Checkpoint;
using CheckpointComponent =
    ProjectJ.Checkpoint.Checkpoint;
using UnityEngine;

namespace ProjectJ.Tests.EditMode
{
    public sealed class PlayerRespawnControllerTests
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

            playerObject.transform
                .SetPositionAndRotation(
                    new Vector3(
                        2f,
                        5f,
                        3f
                    ),
                    Quaternion.Euler(
                        0f,
                        45f,
                        0f
                    )
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
        public void DirectRespawn_WithoutCheckpoint_ReturnsToStart()
        {
            Vector3 startPosition =
                checkpointTracker
                    .RespawnPosition;

            Quaternion startRotation =
                checkpointTracker
                    .RespawnRotation;

            body.position =
                new Vector3(
                    50f,
                    100f,
                    -20f
                );

            body.rotation =
                Quaternion.Euler(
                    20f,
                    120f,
                    10f
                );

            body.linearVelocity =
                new Vector3(
                    4f,
                    -12f,
                    6f
                );

            body.angularVelocity =
                new Vector3(
                    2f,
                    3f,
                    4f
                );

            bool respawned =
                respawnController
                    .RequestRespawn();

            Assert.IsTrue(
                respawned
            );

            Assert.AreEqual(
                startPosition,
                body.position
            );

            Assert.AreEqual(
                startRotation,
                body.rotation
            );

            Assert.AreEqual(
                Vector3.zero,
                body.linearVelocity
            );

            Assert.AreEqual(
                Vector3.zero,
                body.angularVelocity
            );

            Assert.AreEqual(
                1,
                respawnController
                    .RespawnCount
            );
        }

        [Test]
        public void DirectRespawn_AfterCp3_ReturnsToCp3()
        {
            CheckpointComponent cp3 =
                CreateCheckpoint(
                    CheckpointId.CP3,
                    new Vector3(
                        0f,
                        600f,
                        10f
                    ),
                    Quaternion.Euler(
                        0f,
                        180f,
                        0f
                    )
                );

            checkpointTracker
                .ActivateCheckpoint(
                    cp3
                );

            Vector3 expectedPosition =
                cp3.RespawnPosition;

            Quaternion expectedRotation =
                cp3.RespawnRotation;

            body.position =
                Vector3.zero;

            body.rotation =
                Quaternion.identity;

            respawnController
                .RequestRespawn();

            Assert.AreEqual(
                CheckpointId.CP3,
                checkpointTracker
                    .CurrentCheckpointId
            );

            Assert.AreEqual(
                expectedPosition,
                body.position
            );

            Assert.AreEqual(
                expectedRotation,
                body.rotation
            );

            Object.DestroyImmediate(
                cp3.gameObject
            );
        }

        [Test]
        public void LowerCheckpoint_DoesNotChangeRespawnTarget()
        {
            CheckpointComponent cp3 =
                CreateCheckpoint(
                    CheckpointId.CP3,
                    new Vector3(
                        0f,
                        600f,
                        0f
                    ),
                    Quaternion.identity
                );

            CheckpointComponent cp1 =
                CreateCheckpoint(
                    CheckpointId.CP1,
                    new Vector3(
                        0f,
                        200f,
                        0f
                    ),
                    Quaternion.identity
                );

            checkpointTracker
                .ActivateCheckpoint(
                    cp3
                );

            checkpointTracker
                .ActivateCheckpoint(
                    cp1
                );

            Vector3 expectedPosition =
                cp3.RespawnPosition;

            body.position =
                Vector3.zero;

            respawnController
                .RequestRespawn();

            Assert.AreEqual(
                CheckpointId.CP3,
                checkpointTracker
                    .CurrentCheckpointId
            );

            Assert.AreEqual(
                expectedPosition,
                body.position
            );

            Object.DestroyImmediate(
                cp3.gameObject
            );

            Object.DestroyImmediate(
                cp1.gameObject
            );
        }

        [Test]
        public void FallEvent_AutomaticallyRespawnsAndResetsFallState()
        {
            Vector3 startPosition =
                checkpointTracker
                    .RespawnPosition;

            bool detected =
                fallTracker
                    .EvaluateHeight(
                        -21f
                    );

            Assert.IsTrue(
                detected
            );

            Assert.AreEqual(
                startPosition,
                body.position
            );

            Assert.IsFalse(
                fallTracker.IsFallen
            );

            Assert.AreEqual(
                1,
                respawnController
                    .RespawnCount
            );
        }

        [Test]
        public void RepeatedRespawn_ReturnsToSameCheckpoint()
        {
            CheckpointComponent cp2 =
                CreateCheckpoint(
                    CheckpointId.CP2,
                    new Vector3(
                        5f,
                        400f,
                        8f
                    ),
                    Quaternion.Euler(
                        0f,
                        90f,
                        0f
                    )
                );

            checkpointTracker
                .ActivateCheckpoint(
                    cp2
                );

            Vector3 expectedPosition =
                cp2.RespawnPosition;

            Quaternion expectedRotation =
                cp2.RespawnRotation;

            for (
                int i = 0;
                i < 3;
                i++
            )
            {
                body.position =
                    new Vector3(
                        i * 10f,
                        -100f,
                        i * -5f
                    );

                body.rotation =
                    Quaternion.Euler(
                        i * 20f,
                        i * 30f,
                        0f
                    );

                respawnController
                    .RequestRespawn();

                Assert.AreEqual(
                    expectedPosition,
                    body.position
                );

                Assert.AreEqual(
                    expectedRotation,
                    body.rotation
                );
            }

            Assert.AreEqual(
                3,
                respawnController
                    .RespawnCount
            );

            Object.DestroyImmediate(
                cp2.gameObject
            );
        }

        [Test]
        public void RespawnedEvent_ReportsCurrentCheckpoint()
        {
            CheckpointComponent cp4 =
                CreateCheckpoint(
                    CheckpointId.CP4,
                    new Vector3(
                        0f,
                        800f,
                        0f
                    ),
                    Quaternion.identity
                );

            checkpointTracker
                .ActivateCheckpoint(
                    cp4
                );

            CheckpointId reported =
                CheckpointId.Start;

            int eventCount = 0;

            respawnController.Respawned +=
                id =>
                {
                    reported = id;
                    eventCount++;
                };

            respawnController
                .RequestRespawn();

            Assert.AreEqual(
                1,
                eventCount
            );

            Assert.AreEqual(
                CheckpointId.CP4,
                reported
            );

            Object.DestroyImmediate(
                cp4.gameObject
            );
        }

        private static CheckpointComponent
            CreateCheckpoint(
                CheckpointId id,
                Vector3 position,
                Quaternion rotation
            )
        {
            GameObject checkpointObject =
                new GameObject(
                    id.ToString()
                );

            checkpointObject.transform
                .SetPositionAndRotation(
                    position,
                    rotation
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

            respawnObject.transform.localPosition =
                new Vector3(
                    0f,
                    1.1f,
                    0f
                );

            respawnObject.transform.localRotation =
                Quaternion.identity;

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
