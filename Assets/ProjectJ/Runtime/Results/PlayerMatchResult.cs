using System;
using ProjectJ.Checkpoint;
using ProjectJ.Player;

namespace ProjectJ.Results
{
    [Serializable]
    public sealed class PlayerMatchResult
    {
        public const double NoFinishTime = -1d;

        private readonly int playerId;
        private readonly int finalRank;
        private readonly bool isFinished;
        private readonly int finishOrder;
        private readonly double finishTime;
        private readonly int highestHeightCentimeters;
        private readonly CheckpointId highestCheckpoint;

        public int PlayerId
        {
            get
            {
                return playerId;
            }
        }

        public int FinalRank
        {
            get
            {
                return finalRank;
            }
        }

        public bool IsFinished
        {
            get
            {
                return isFinished;
            }
        }

        public int FinishOrder
        {
            get
            {
                return finishOrder;
            }
        }

        public double FinishTime
        {
            get
            {
                return finishTime;
            }
        }

        public bool HasFinishTime
        {
            get
            {
                return
                    isFinished &&
                    finishTime >= 0d;
            }
        }

        public int HighestHeightCentimeters
        {
            get
            {
                return highestHeightCentimeters;
            }
        }

        public float HighestHeight
        {
            get
            {
                return
                    PlayerHeightTracker
                        .CentimetersToMeters(
                            highestHeightCentimeters
                        );
            }
        }

        public CheckpointId HighestCheckpoint
        {
            get
            {
                return highestCheckpoint;
            }
        }

        public PlayerMatchResult(
            int playerId,
            int finalRank,
            bool isFinished,
            int finishOrder,
            double finishTime,
            int highestHeightCentimeters,
            CheckpointId highestCheckpoint
        )
        {
            this.playerId =
                playerId;

            this.finalRank =
                Math.Max(
                    1,
                    finalRank
                );

            this.isFinished =
                isFinished;

            this.finishOrder =
                isFinished
                    ? Math.Max(
                        1,
                        finishOrder
                    )
                    : 0;

            this.finishTime =
                isFinished
                    ? finishTime
                    : NoFinishTime;

            this.highestHeightCentimeters =
                highestHeightCentimeters;

            this.highestCheckpoint =
                highestCheckpoint;
        }
    }
}
