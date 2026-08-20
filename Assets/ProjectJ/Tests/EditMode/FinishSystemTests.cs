using NUnit.Framework;
using ProjectJ.Finish;
using ProjectJ.Player;
using ProjectJ.Ranking;
using UnityEngine;

namespace ProjectJ.Tests.EditMode
{
    public sealed class FinishSystemTests
    {
        private GameObject managerObject;
        private FinishOrderManager finishManager;

        [SetUp]
        public void SetUp()
        {
            managerObject =
                new GameObject(
                    "Finish Manager"
                );

            finishManager =
                managerObject.AddComponent<
                    FinishOrderManager
                >();
        }

        [TearDown]
        public void TearDown()
        {
            PlayerFinishState[] players =
                Object.FindObjectsByType<
                    PlayerFinishState
                >(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None
                );

            for (
                int i = 0;
                i < players.Length;
                i++
            )
            {
                if (players[i] != null)
                {
                    Object.DestroyImmediate(
                        players[i].gameObject
                    );
                }
            }

            if (managerObject != null)
            {
                Object.DestroyImmediate(
                    managerObject
                );
            }

            PlayerRankingManager rankingManager =
                Object.FindFirstObjectByType<
                    PlayerRankingManager
                >();

            if (rankingManager != null)
            {
                Object.DestroyImmediate(
                    rankingManager.gameObject
                );
            }
        }

        [Test]
        public void FirstFinish_AssignsOrderOneAndTime()
        {
            PlayerFinishState player =
                CreatePlayer(
                    "Player A",
                    10f
                );

            bool registered =
                finishManager
                    .TryRegisterFinish(
                        player,
                        123.456d
                    );

            Assert.IsTrue(
                registered
            );

            Assert.IsTrue(
                player.IsFinished
            );

            Assert.AreEqual(
                1,
                player.FinishOrder
            );

            Assert.AreEqual(
                123.456d,
                player.FinishTime
            );

            Assert.AreEqual(
                1,
                finishManager.FinishCount
            );
        }

        [Test]
        public void MultiplePlayers_ReceiveArrivalOrder()
        {
            PlayerFinishState playerA =
                CreatePlayer(
                    "Player A",
                    10f
                );

            PlayerFinishState playerB =
                CreatePlayer(
                    "Player B",
                    20f
                );

            PlayerFinishState playerC =
                CreatePlayer(
                    "Player C",
                    30f
                );

            finishManager.TryRegisterFinish(
                playerB,
                10d
            );

            finishManager.TryRegisterFinish(
                playerA,
                11d
            );

            finishManager.TryRegisterFinish(
                playerC,
                12d
            );

            Assert.AreEqual(
                1,
                playerB.FinishOrder
            );

            Assert.AreEqual(
                2,
                playerA.FinishOrder
            );

            Assert.AreEqual(
                3,
                playerC.FinishOrder
            );
        }

        [Test]
        public void DuplicateFinish_IsRejected()
        {
            PlayerFinishState player =
                CreatePlayer(
                    "Player A",
                    10f
                );

            bool first =
                finishManager
                    .TryRegisterFinish(
                        player,
                        10d
                    );

            bool second =
                finishManager
                    .TryRegisterFinish(
                        player,
                        20d
                    );

            Assert.IsTrue(
                first
            );

            Assert.IsFalse(
                second
            );

            Assert.AreEqual(
                1,
                player.FinishOrder
            );

            Assert.AreEqual(
                10d,
                player.FinishTime
            );

            Assert.AreEqual(
                1,
                finishManager.FinishCount
            );
        }

        [Test]
        public void FinishedPlayer_IsExcludedFromHeightRanking()
        {
            GameObject rankingManagerObject =
                new GameObject(
                    "Ranking Manager"
                );

            PlayerRankingManager rankingManager =
                rankingManagerObject.AddComponent<
                    PlayerRankingManager
                >();

            PlayerFinishState highPlayer =
                CreatePlayer(
                    "High Player",
                    100f
                );

            PlayerFinishState lowPlayer =
                CreatePlayer(
                    "Low Player",
                    50f
                );

            PlayerRankingParticipant
                highParticipant =
                    highPlayer
                        .RankingParticipant;

            PlayerRankingParticipant
                lowParticipant =
                    lowPlayer
                        .RankingParticipant;

            rankingManager.Register(
                highParticipant
            );

            rankingManager.Register(
                lowParticipant
            );

            highParticipant
                .HeightTracker
                .RefreshHeight();

            lowParticipant
                .HeightTracker
                .RefreshHeight();

            rankingManager
                .RecalculateRanks();

            Assert.AreEqual(
                1,
                highParticipant.CurrentRank
            );

            Assert.AreEqual(
                2,
                lowParticipant.CurrentRank
            );

            finishManager
                .TryRegisterFinish(
                    highPlayer,
                    5d
                );

            rankingManager
                .RecalculateRanks();

            Assert.IsFalse(
                highParticipant
                    .HeightRankingEligible
            );

            Assert.IsTrue(
                lowParticipant
                    .HeightRankingEligible
            );

            Assert.AreEqual(
                2,
                lowParticipant.CurrentRank
            );

            Assert.AreEqual(
                1,
                highParticipant.CurrentRank
            );
        }

        [Test]
        public void FinishedRank_DoesNotChangeAfterHeightChanges()
        {
            GameObject rankingManagerObject =
                new GameObject(
                    "Ranking Manager"
                );

            PlayerRankingManager rankingManager =
                rankingManagerObject.AddComponent<
                    PlayerRankingManager
                >();

            PlayerFinishState playerA =
                CreatePlayer(
                    "Player A",
                    100f
                );

            PlayerFinishState playerB =
                CreatePlayer(
                    "Player B",
                    90f
                );

            rankingManager.Register(
                playerA.RankingParticipant
            );

            rankingManager.Register(
                playerB.RankingParticipant
            );

            finishManager.TryRegisterFinish(
                playerA,
                7d
            );

            playerA.transform.position =
                new Vector3(
                    0f,
                    -100f,
                    0f
                );

            playerA.RankingParticipant
                .HeightTracker
                .RefreshHeight();

            playerB.RankingParticipant
                .HeightTracker
                .RefreshHeight();

            rankingManager
                .RecalculateRanks();

            Assert.AreEqual(
                1,
                playerA.FinishOrder
            );

            Assert.AreEqual(
                1,
                playerA.RankingParticipant
                    .CurrentRank
            );

            Assert.AreEqual(
                2,
                playerB.RankingParticipant
                    .CurrentRank
            );
        }

        private static PlayerFinishState
            CreatePlayer(
                string objectName,
                float worldY
            )
        {
            GameObject player =
                new GameObject(
                    objectName
                );

            player.transform.position =
                new Vector3(
                    0f,
                    worldY,
                    0f
                );

            CapsuleCollider capsule =
                player.AddComponent<
                    CapsuleCollider
                >();

            capsule.height = 2f;
            capsule.center =
                new Vector3(
                    0f,
                    1f,
                    0f
                );

            PlayerHeightTracker heightTracker =
                player.AddComponent<
                    PlayerHeightTracker
                >();

            heightTracker.Configure(
                null
            );

            heightTracker.RefreshHeight();

            PlayerRankingParticipant participant =
                player.AddComponent<
                    PlayerRankingParticipant
                >();

            participant.Configure(
                -1,
                heightTracker
            );

            PlayerFinishState finishState =
                player.AddComponent<
                    PlayerFinishState
                >();

            finishState.Configure(
                participant
            );

            return finishState;
        }
    }
}
