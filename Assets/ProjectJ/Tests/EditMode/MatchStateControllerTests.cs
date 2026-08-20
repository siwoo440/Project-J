using NUnit.Framework;
using ProjectJ.Match;
using UnityEngine;

namespace ProjectJ.Tests.EditMode
{
    public sealed class MatchStateControllerTests
    {
        private GameObject root;
        private MatchStateController controller;

        [SetUp]
        public void SetUp()
        {
            root =
                new GameObject(
                    "MatchStateControllerTests"
                );

            controller =
                root.AddComponent<
                    MatchStateController
                >();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(
                root
            );
        }

        [Test]
        public void NewController_IsPreparing()
        {
            Assert.AreEqual(
                MatchState.Preparing,
                controller.CurrentState
            );
        }

        [Test]
        public void ReadySignal_DoesNotImmediatelyStartCountdown()
        {
            controller.NotifyAllPlayersReady();

            Assert.AreEqual(
                MatchState.Preparing,
                controller.CurrentState
            );

            Assert.IsTrue(
                controller.IsReadySignalReceived
            );
        }

        [Test]
        public void ReadySettle_ThenCountdownStartsAtThree()
        {
            controller.NotifyAllPlayersReady();

            controller.AdvanceReadySettle(
                0.5f
            );

            Assert.AreEqual(
                MatchState.Countdown,
                controller.CurrentState
            );

            Assert.AreEqual(
                3,
                controller.CountdownDisplayNumber
            );
        }

        [Test]
        public void FirstCountdownAdvance_KeepsThreeVisible()
        {
            controller.NotifyAllPlayersReady();

            controller.AdvanceReadySettle(
                0.5f
            );

            controller.AdvanceCountdown(
                10f
            );

            Assert.AreEqual(
                3,
                controller.CountdownDisplayNumber
            );
        }

        [Test]
        public void Countdown_AlwaysProgressesThreeTwoOneThenPlaying()
        {
            controller.NotifyAllPlayersReady();

            controller.AdvanceReadySettle(
                0.5f
            );

            Assert.AreEqual(
                3,
                controller.CountdownDisplayNumber
            );

            controller.AdvanceCountdown(
                0f
            );

            Assert.AreEqual(
                3,
                controller.CountdownDisplayNumber
            );

            controller.AdvanceCountdown(
                1.25f
            );

            Assert.AreEqual(
                2,
                controller.CountdownDisplayNumber
            );

            controller.AdvanceCountdown(
                1.25f
            );

            Assert.AreEqual(
                1,
                controller.CountdownDisplayNumber
            );

            controller.AdvanceCountdown(
                1.25f
            );

            Assert.AreEqual(
                MatchState.Playing,
                controller.CurrentState
            );
        }

        [Test]
        public void LargeFrameDelay_CannotSkipThreeTwoOne()
        {
            controller.NotifyAllPlayersReady();

            controller.AdvanceReadySettle(
                10f
            );

            controller.AdvanceCountdown(
                10f
            );

            Assert.AreEqual(
                3,
                controller.CountdownDisplayNumber
            );

            controller.AdvanceCountdown(
                10f
            );

            Assert.AreEqual(
                2,
                controller.CountdownDisplayNumber
            );

            controller.AdvanceCountdown(
                10f
            );

            Assert.AreEqual(
                1,
                controller.CountdownDisplayNumber
            );

            controller.AdvanceCountdown(
                10f
            );

            Assert.AreEqual(
                MatchState.Playing,
                controller.CurrentState
            );
        }

        [Test]
        public void CancelReadySignal_StopsPreparingProgress()
        {
            controller.NotifyAllPlayersReady();

            controller.CancelReadySignal();

            controller.AdvanceReadySettle(
                10f
            );

            Assert.AreEqual(
                MatchState.Preparing,
                controller.CurrentState
            );

            Assert.IsFalse(
                controller.IsReadySignalReceived
            );
        }

        [Test]
        public void FinishMatch_OnlyWorksFromPlaying()
        {
            Assert.IsFalse(
                controller.FinishMatch()
            );

            controller.NotifyAllPlayersReady();

            controller.AdvanceReadySettle(
                0.5f
            );

            controller.AdvanceCountdown(
                0f
            );

            controller.AdvanceCountdown(
                1.25f
            );

            controller.AdvanceCountdown(
                1.25f
            );

            controller.AdvanceCountdown(
                1.25f
            );

            Assert.IsTrue(
                controller.FinishMatch()
            );

            Assert.AreEqual(
                MatchState.Finished,
                controller.CurrentState
            );
        }
    }
}
