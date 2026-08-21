using NUnit.Framework;
using ProjectJ.Finish;
using ProjectJ.Player;
using ProjectJ.Push;
using ProjectJ.Ranking;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectJ.Tests.EditMode
{
    public sealed class PlayerPushFeedbackEventTests
    {
        private GameObject player;
        private PlayerPushController controller;

        [SetUp]
        public void SetUp()
        {
            int playerLayer =
                LayerMask.NameToLayer(
                    "Player"
                );

            Assert.GreaterOrEqual(
                playerLayer,
                0
            );

            player =
                new GameObject(
                    "Feedback Event Player"
                );

            player.layer =
                playerLayer;

            player.AddComponent<
                CapsuleCollider
            >();

            PlayerHeightTracker height =
                player.AddComponent<
                    PlayerHeightTracker
                >();

            PlayerRankingParticipant ranking =
                player.AddComponent<
                    PlayerRankingParticipant
                >();

            ranking.Configure(
                -1,
                height
            );

            PlayerFinishState finishState =
                player.AddComponent<
                    PlayerFinishState
                >();

            finishState.Configure(
                ranking
            );

            PlayerPushTargetSelector selector =
                player.AddComponent<
                    PlayerPushTargetSelector
                >();

            selector.Configure(
                2.5f,
                90f,
                1 << playerLayer,
                finishState
            );

            PlayerInput input =
                player.AddComponent<
                    PlayerInput
                >();

            controller =
                player.AddComponent<
                    PlayerPushController
                >();

            controller.Configure(
                selector,
                input,
                finishState,
                12f,
                0f,
                1.5f
            );

            Physics.SyncTransforms();
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
        public void MissRequest_RaisesOneFeedbackEvent()
        {
            int eventCount =
                0;

            PushAttemptResult capturedResult =
                PushAttemptResult.InvalidState;

            controller.PushAttempted +=
                (
                    result,
                    target,
                    velocityChange
                ) =>
                {
                    eventCount++;
                    capturedResult =
                        result;
                };

            PushAttemptResult result =
                controller.TryPushAt(
                    10d
                );

            Assert.AreEqual(
                PushAttemptResult.Miss,
                result
            );

            Assert.AreEqual(
                1,
                eventCount
            );

            Assert.AreEqual(
                PushAttemptResult.Miss,
                capturedResult
            );
        }

        [Test]
        public void CooldownRequest_RaisesOneAdditionalFeedbackEvent()
        {
            int eventCount =
                0;

            controller.PushAttempted +=
                (
                    result,
                    target,
                    velocityChange
                ) =>
                {
                    eventCount++;
                };

            controller.TryPushAt(
                20d
            );

            PushAttemptResult secondResult =
                controller.TryPushAt(
                    20.1d
                );

            Assert.AreEqual(
                PushAttemptResult.Cooldown,
                secondResult
            );

            Assert.AreEqual(
                2,
                eventCount
            );
        }
    }
}
