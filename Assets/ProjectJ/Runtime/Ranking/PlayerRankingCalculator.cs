using System.Collections.Generic;

namespace ProjectJ.Ranking
{
    public static class PlayerRankingCalculator
    {
        public static int CalculateRank(
            int currentHeightCentimeters,
            IReadOnlyList<int> allHeightsCentimeters
        )
        {
            if (
                allHeightsCentimeters == null ||
                allHeightsCentimeters.Count == 0
            )
            {
                return 1;
            }

            int rank = 1;

            for (
                int i = 0;
                i < allHeightsCentimeters.Count;
                i++
            )
            {
                if (
                    allHeightsCentimeters[i] >
                    currentHeightCentimeters
                )
                {
                    rank++;
                }
            }

            return rank;
        }

        public static int[] CalculateRanks(
            IReadOnlyList<int> heightsCentimeters
        )
        {
            if (
                heightsCentimeters == null ||
                heightsCentimeters.Count == 0
            )
            {
                return new int[0];
            }

            int[] ranks =
                new int[
                    heightsCentimeters.Count
                ];

            for (
                int i = 0;
                i < heightsCentimeters.Count;
                i++
            )
            {
                ranks[i] =
                    CalculateRank(
                        heightsCentimeters[i],
                        heightsCentimeters
                    );
            }

            return ranks;
        }
    }
}
