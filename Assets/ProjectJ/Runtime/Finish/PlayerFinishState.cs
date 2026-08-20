using System;
using ProjectJ.Ranking;
using UnityEngine;

namespace ProjectJ.Finish
{
    [DisallowMultipleComponent]
    [RequireComponent(
        typeof(PlayerRankingParticipant)
    )]
    public sealed class PlayerFinishState :
        MonoBehaviour
    {
        [SerializeField]
        private PlayerRankingParticipant
            rankingParticipant;

        [SerializeField]
        private bool isFinished;

        [SerializeField]
        private int finishOrder;

        [SerializeField]
        private double finishTime;

        public event Action<PlayerFinishState>
            Finished;

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

        public PlayerRankingParticipant
            RankingParticipant
        {
            get
            {
                return rankingParticipant;
            }
        }

        private void Awake()
        {
            ResolveReferences();
        }

        public void Configure(
            PlayerRankingParticipant participant
        )
        {
            rankingParticipant =
                participant;

            ResolveReferences();
        }

        public bool TryConfirmFinish(
            int order,
            double timestamp
        )
        {
            if (
                isFinished ||
                order <= 0
            )
            {
                return false;
            }

            ResolveReferences();

            isFinished = true;
            finishOrder = order;
            finishTime = timestamp;

            if (rankingParticipant != null)
            {
                rankingParticipant
                    .SetHeightRankingEligible(
                        false
                    );

                rankingParticipant
                    .SetCurrentRank(
                        finishOrder
                    );
            }

            Finished?.Invoke(
                this
            );

            return true;
        }

        private void ResolveReferences()
        {
            if (rankingParticipant != null)
            {
                return;
            }

            rankingParticipant =
                GetComponent<
                    PlayerRankingParticipant
                >();
        }
    }
}
