using System.Collections.Generic;
using NUnit.Framework;
using ProjectJ.Match;
using UnityEngine;

namespace ProjectJ.Tests.EditMode
{
    public sealed class MatchTimerTests
    {
        private GameObject root;
        private MatchStateController controller;
        private MatchTimer timer;

        [SetUp]
        public void SetUp()
        {
            root =
                new GameObject(
                    "MatchTimerTests"
                );

            controller =
                root.AddComponent<
                    MatchStateController
                >();

            timer =
                root.AddComponent<
                    MatchTimer
                >();

            timer.Configure(
                controller,
                MatchTimer
                    .DefaultMatchDurationSeconds
            );
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(
                root
            );
        }

        [Test]
        public void DefaultDuration_IsFifteenMinutes()
        {
            Assert.AreEqual(
                900f,
                MatchTimer
                    .DefaultMatchDurationSeconds
            );

            Assert.AreEqual(
                900f,
                timer.RemainingSeconds,
                0.0001f
            );

            Assert.AreEqual(
                "15:00",
                timer.FormattedRemainingTime
            );
        }

        [Test]
        public void Preparing_DoesNotDecreaseTimer()
        {
            timer.AdvanceTimer(
                10f
            );

            Assert.AreEqual(
                900f,
                timer.RemainingSeconds,
                0.0001f
            );
        }

        [Test]
        public void Countdown_DoesNotDecreaseTimer()
        {
            controller
                .NotifyAllPlayersReady();

            controller
                .AdvanceReadySettle(
                    10f
                );

            Assert.AreEqual(
                MatchState.Countdown,
                controller.CurrentState
            );

            timer.AdvanceTimer(
                10f
            );

            Assert.AreEqual(
                900f,
                timer.RemainingSeconds,
                0.0001f
            );
        }

        [Test]
        public void Playing_DecreasesTimer()
        {
            MoveControllerToPlaying();

            timer.AdvanceTimer(
                10f
            );

            Assert.AreEqual(
                890f,
                timer.RemainingSeconds,
                0.0001f
            );
        }

        [Test]
        public void Warnings_FireAtSixtyThirtyAndTenSeconds()
        {
            timer.Configure(
                controller,
                65f
            );

            MoveControllerToPlaying();

            List<int> warnings =
                new List<int>();

            timer.WarningReached +=
                warnings.Add;

            timer.AdvanceTimer(
                5.1f
            );

            timer.AdvanceTimer(
                30.1f
            );

            timer.AdvanceTimer(
                20.1f
            );

            CollectionAssert.AreEqual(
                new[]
                {
                    60,
                    30,
                    10
                },
                warnings
            );
        }

        [Test]
        public void Warning_IsOnlyRaisedOnce()
        {
            timer.Configure(
                controller,
                65f
            );

            MoveControllerToPlaying();

            int warningCount = 0;

            timer.WarningReached +=
                seconds =>
                {
                    if (
                        seconds ==
                        MatchTimer
                            .OneMinuteWarningSeconds
                    )
                    {
                        warningCount++;
                    }
                };

            timer.AdvanceTimer(
                5.1f
            );

            timer.AdvanceTimer(
                1f
            );

            timer.AdvanceTimer(
                1f
            );

            Assert.AreEqual(
                1,
                warningCount
            );
        }

        [Test]
        public void ZeroSeconds_FinishesMatch()
        {
            timer.Configure(
                controller,
                5f
            );

            MoveControllerToPlaying();

            timer.AdvanceTimer(
                5f
            );

            Assert.AreEqual(
                0f,
                timer.RemainingSeconds,
                0.0001f
            );

            Assert.IsTrue(
                timer.IsExpired
            );

            Assert.AreEqual(
                MatchState.Finished,
                controller.CurrentState
            );
        }

        [Test]
        public void Finished_DoesNotContinueBelowZero()
        {
            timer.Configure(
                controller,
                1f
            );

            MoveControllerToPlaying();

            timer.AdvanceTimer(
                2f
            );

            timer.AdvanceTimer(
                100f
            );

            Assert.AreEqual(
                0f,
                timer.RemainingSeconds,
                0.0001f
            );
        }

        [TestCase(900f, "15:00")]
        [TestCase(60f, "01:00")]
        [TestCase(59.9f, "01:00")]
        [TestCase(30f, "00:30")]
        [TestCase(10f, "00:10")]
        [TestCase(0f, "00:00")]
        public void FormatTime_ReturnsExpectedText(
            float seconds,
            string expected
        )
        {
            Assert.AreEqual(
                expected,
                MatchTimer.FormatTime(
                    seconds
                )
            );
        }

        private void MoveControllerToPlaying()
        {
            controller
                .NotifyAllPlayersReady();

            controller
                .AdvanceReadySettle(
                    10f
                );

            controller
                .AdvanceCountdown(
                    0f
                );

            controller
                .AdvanceCountdown(
                    10f
                );

            controller
                .AdvanceCountdown(
                    10f
                );

            controller
                .AdvanceCountdown(
                    10f
                );

            Assert.AreEqual(
                MatchState.Playing,
                controller.CurrentState
            );
        }
    }
}
