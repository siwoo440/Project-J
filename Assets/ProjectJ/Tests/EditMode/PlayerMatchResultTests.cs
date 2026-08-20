using NUnit.Framework;
using ProjectJ.Checkpoint;
using ProjectJ.Finish;
using ProjectJ.Player;
using ProjectJ.Ranking;
using ProjectJ.Results;
using UnityEngine;
using CheckpointComponent =
    ProjectJ.Checkpoint.Checkpoint;

namespace ProjectJ.Tests.EditMode
{
    public sealed class PlayerMatchResultTests
    {
        private GameObject playerObject;

        private PlayerHeightTracker heightTracker;
        private PlayerRankingParticipant ranking;
        private PlayerCheckpointTracker checkpointTracker;
        private PlayerFinishState finishState;
        private PlayerMatchResultCollector collector;

        [SetUp]
        public void SetUp()
        {
            playerObject =
                new GameObject(
                    "Player"
                );

            CapsuleCollider capsule =
                playerObject.AddComponent<
                    CapsuleCollider
                >();

            capsule.height = 2f;

            capsule.center =
                new Vector3(
                    0f,
                    1f,
                    0f
                );

            heightTracker =
                playerObject.AddComponent<
                    PlayerHeightTracker
                >();

            heightTracker.Configure(
                null
            );

            ranking =
                playerObject.AddComponent<
                    PlayerRankingParticipant
                >();

            ranking.Configure(
                7,
                heightTracker
            );

            checkpointTracker =
                playerObject.AddComponent<
                    PlayerCheckpointTracker
                >();

            checkpointTracker
                .CaptureStartPoint();

            finishState =
                playerObject.AddComponent<
                    PlayerFinishState
                >();

            finishState.Configure(
                ranking
            );

            collector =
                playerObject.AddComponent<
                    PlayerMatchResultCollector
                >();

            collector.Configure(
                finishState,
                ranking,
                heightTracker,
                checkpointTracker,
                null
            );
        }

        [TearDown]
        public void TearDown()
        {
            CheckpointComponent[] checkpoints =
                Object.FindObjectsByType<
                    CheckpointComponent
                >(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            for (
                int i = 0;
                i < checkpoints.Length;
                i++
            )
            {
                if (checkpoints[i] != null)
                {
                    Object.DestroyImmediate(
                        checkpoints[i]
                            .gameObject
                    );
                }
            }

            if (playerObject != null)
            {
                Object.DestroyImmediate(
                    playerObject
                );
            }
        }

        [Test]
        public void FinishedPlayer_ResultMatchesFinishRecords()
        {
            MovePlayerAndRefresh(
                1000.42f
            );

            int expectedHighestHeight =
                heightTracker
                    .HighestHeightCentimeters;

            ActivateCheckpoint(
                CheckpointId.CP4
            );

            bool finished =
                finishState
                    .TryConfirmFinish(
                        2,
                        521.45d
                    );

            Assert.IsTrue(
                finished
            );

            Assert.IsTrue(
                collector.HasResult
            );

            PlayerMatchResult result =
                collector.CurrentResult;

            Assert.AreEqual(
                7,
                result.PlayerId
            );

            Assert.AreEqual(
                2,
                result.FinalRank
            );

            Assert.IsTrue(
                result.IsFinished
            );

            Assert.AreEqual(
                2,
                result.FinishOrder
            );

            Assert.AreEqual(
                521.45d,
                result.FinishTime
            );

            Assert.AreEqual(
                expectedHighestHeight,
                result
                    .HighestHeightCentimeters
            );

            Assert.AreEqual(
                CheckpointId.CP4,
                result.HighestCheckpoint
            );
        }

        [Test]
        public void TimeExpiredResult_UsesCurrentRankAndNoFinishTime()
        {
            ranking.SetCurrentRank(
                4
            );

            MovePlayerAndRefresh(
                742.31f
            );

            int expectedHighestHeight =
                heightTracker
                    .HighestHeightCentimeters;

            ActivateCheckpoint(
                CheckpointId.CP3
            );

            bool created =
                collector
                    .TryCreateTimeExpiredResult();

            Assert.IsTrue(
                created
            );

            PlayerMatchResult result =
                collector.CurrentResult;

            Assert.AreEqual(
                4,
                result.FinalRank
            );

            Assert.IsFalse(
                result.IsFinished
            );

            Assert.AreEqual(
                0,
                result.FinishOrder
            );

            Assert.IsFalse(
                result.HasFinishTime
            );

            Assert.AreEqual(
                PlayerMatchResult.NoFinishTime,
                result.FinishTime
            );

            Assert.AreEqual(
                expectedHighestHeight,
                result
                    .HighestHeightCentimeters
            );

            Assert.AreEqual(
                CheckpointId.CP3,
                result.HighestCheckpoint
            );
        }

        [Test]
        public void Result_IsCreatedOnlyOnce()
        {
            ranking.SetCurrentRank(
                3
            );

            MovePlayerAndRefresh(
                400f
            );

            bool first =
                collector
                    .TryCreateTimeExpiredResult();

            PlayerMatchResult firstResult =
                collector.CurrentResult;

            ranking.SetCurrentRank(
                8
            );

            MovePlayerAndRefresh(
                900f
            );

            bool second =
                collector
                    .TryCreateTimeExpiredResult();

            Assert.IsTrue(
                first
            );

            Assert.IsFalse(
                second
            );

            Assert.AreSame(
                firstResult,
                collector.CurrentResult
            );

            Assert.AreEqual(
                3,
                collector
                    .CurrentResult
                    .FinalRank
            );

            Assert.AreEqual(
                40000,
                collector
                    .CurrentResult
                    .HighestHeightCentimeters
            );
        }

        [Test]
        public void FinishedResult_UsesFinishOrderInsteadOfLiveRank()
        {
            ranking.SetCurrentRank(
                6
            );

            finishState
                .TryConfirmFinish(
                    1,
                    100d
                );

            Assert.IsTrue(
                collector.HasResult
            );

            Assert.AreEqual(
                1,
                collector
                    .CurrentResult
                    .FinalRank
            );
        }

        [Test]
        public void ResultCreatedEvent_FiresOnce()
        {
            int eventCount = 0;

            collector.ResultCreated +=
                result =>
                {
                    eventCount++;
                };

            collector
                .TryCreateTimeExpiredResult();

            collector
                .TryCreateTimeExpiredResult();

            Assert.AreEqual(
                1,
                eventCount
            );
        }

        [Test]
        public void HighestCheckpoint_IsSnapshotValue()
        {
            ActivateCheckpoint(
                CheckpointId.CP2
            );

            collector
                .TryCreateTimeExpiredResult();

            ActivateCheckpoint(
                CheckpointId.CP4
            );

            Assert.AreEqual(
                CheckpointId.CP2,
                collector
                    .CurrentResult
                    .HighestCheckpoint
            );
        }

        private void MovePlayerAndRefresh(
            float worldY
        )
        {
            playerObject.transform.position =
                new Vector3(
                    0f,
                    worldY,
                    0f
                );

            heightTracker.RefreshHeight();
        }

        private void ActivateCheckpoint(
            CheckpointId id
        )
        {
            GameObject checkpointObject =
                new GameObject(
                    id.ToString()
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

            checkpointTracker
                .ActivateCheckpoint(
                    checkpoint
                );
        }
    }
}
