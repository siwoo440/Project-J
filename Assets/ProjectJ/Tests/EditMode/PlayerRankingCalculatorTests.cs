using NUnit.Framework;
using ProjectJ.Player;
using ProjectJ.Ranking;

namespace ProjectJ.Tests.EditMode
{
    public sealed class PlayerRankingCalculatorTests
    {
        [Test]
        public void DistinctHeights_ProduceSequentialRanks()
        {
            int[] heights =
            {
                100000,
                90000,
                80000,
                70000
            };

            int[] ranks =
                PlayerRankingCalculator
                    .CalculateRanks(
                        heights
                    );

            CollectionAssert.AreEqual(
                new[]
                {
                    1,
                    2,
                    3,
                    4
                },
                ranks
            );
        }

        [Test]
        public void TwoEqualHeights_UseCompetitionRanking()
        {
            int[] heights =
            {
                100000,
                90000,
                90000,
                70000
            };

            int[] ranks =
                PlayerRankingCalculator
                    .CalculateRanks(
                        heights
                    );

            CollectionAssert.AreEqual(
                new[]
                {
                    1,
                    2,
                    2,
                    4
                },
                ranks
            );
        }

        [Test]
        public void ThreeEqualTopHeights_ShareFirstPlace()
        {
            int[] heights =
            {
                100000,
                100000,
                100000,
                50000
            };

            int[] ranks =
                PlayerRankingCalculator
                    .CalculateRanks(
                        heights
                    );

            CollectionAssert.AreEqual(
                new[]
                {
                    1,
                    1,
                    1,
                    4
                },
                ranks
            );
        }

        [Test]
        public void AllEqualHeights_AllShareFirstPlace()
        {
            int[] heights =
            {
                50000,
                50000,
                50000,
                50000
            };

            int[] ranks =
                PlayerRankingCalculator
                    .CalculateRanks(
                        heights
                    );

            CollectionAssert.AreEqual(
                new[]
                {
                    1,
                    1,
                    1,
                    1
                },
                ranks
            );
        }

        [Test]
        public void InputOrder_DoesNotChangeHeightBasedRank()
        {
            int[] heights =
            {
                10000,
                20000,
                30000
            };

            int[] ranks =
                PlayerRankingCalculator
                    .CalculateRanks(
                        heights
                    );

            CollectionAssert.AreEqual(
                new[]
                {
                    3,
                    2,
                    1
                },
                ranks
            );
        }

        [Test]
        public void SameTwoDecimalHeight_GivesSameRank()
        {
            int playerAHeight =
                PlayerHeightTracker
                    .TruncateToCentimeters(
                        123.4591f
                    );

            int playerBHeight =
                PlayerHeightTracker
                    .TruncateToCentimeters(
                        123.4501f
                    );

            int playerCHeight =
                PlayerHeightTracker
                    .TruncateToCentimeters(
                        120f
                    );

            int[] heights =
            {
                playerAHeight,
                playerBHeight,
                playerCHeight
            };

            int[] ranks =
                PlayerRankingCalculator
                    .CalculateRanks(
                        heights
                    );

            Assert.That(
                playerAHeight,
                Is.EqualTo(
                    playerBHeight
                )
            );

            CollectionAssert.AreEqual(
                new[]
                {
                    1,
                    1,
                    3
                },
                ranks
            );
        }

        [Test]
        public void FallingBelowAnotherPlayer_ChangesRank()
        {
            int[] before =
            {
                50000,
                48000
            };

            int[] after =
            {
                45000,
                48000
            };

            CollectionAssert.AreEqual(
                new[]
                {
                    1,
                    2
                },
                PlayerRankingCalculator
                    .CalculateRanks(
                        before
                    )
            );

            CollectionAssert.AreEqual(
                new[]
                {
                    2,
                    1
                },
                PlayerRankingCalculator
                    .CalculateRanks(
                        after
                    )
            );
        }

        [Test]
        public void NegativeHeights_AreRankedNormally()
        {
            int[] heights =
            {
                -100,
                -200,
                -200,
                -500
            };

            int[] ranks =
                PlayerRankingCalculator
                    .CalculateRanks(
                        heights
                    );

            CollectionAssert.AreEqual(
                new[]
                {
                    1,
                    2,
                    2,
                    4
                },
                ranks
            );
        }

        [Test]
        public void SinglePlayer_IsAlwaysFirst()
        {
            int[] ranks =
                PlayerRankingCalculator
                    .CalculateRanks(
                        new[]
                        {
                            12345
                        }
                    );

            CollectionAssert.AreEqual(
                new[]
                {
                    1
                },
                ranks
            );
        }

        [Test]
        public void EmptyInput_ReturnsEmptyRanks()
        {
            int[] ranks =
                PlayerRankingCalculator
                    .CalculateRanks(
                        new int[0]
                    );

            Assert.That(
                ranks,
                Is.Empty
            );
        }
    }
}
