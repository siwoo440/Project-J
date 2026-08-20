using System;
using ProjectJ.Checkpoint;
using ProjectJ.Finish;
using ProjectJ.Match;
using ProjectJ.Player;
using ProjectJ.Ranking;
using UnityEngine;

namespace ProjectJ.Results
{
    [DisallowMultipleComponent]
    [RequireComponent(
        typeof(PlayerFinishState)
    )]
    [RequireComponent(
        typeof(PlayerRankingParticipant)
    )]
    [RequireComponent(
        typeof(PlayerHeightTracker)
    )]
    [RequireComponent(
        typeof(PlayerCheckpointTracker)
    )]
    public sealed class PlayerMatchResultCollector :
        MonoBehaviour
    {
        [SerializeField]
        private PlayerFinishState finishState;

        [SerializeField]
        private PlayerRankingParticipant
            rankingParticipant;

        [SerializeField]
        private PlayerHeightTracker heightTracker;

        [SerializeField]
        private PlayerCheckpointTracker
            checkpointTracker;

        [SerializeField]
        private MatchTimer matchTimer;

        [SerializeField]
        private bool hasResult;

        private PlayerMatchResult currentResult;

        private bool finishSubscribed;
        private bool timerSubscribed;

        public event Action<PlayerMatchResult>
            ResultCreated;

        public bool HasResult
        {
            get
            {
                return hasResult;
            }
        }

        public PlayerMatchResult CurrentResult
        {
            get
            {
                return currentResult;
            }
        }

        public PlayerFinishState FinishState
        {
            get
            {
                return finishState;
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

        public PlayerHeightTracker HeightTracker
        {
            get
            {
                return heightTracker;
            }
        }

        public PlayerCheckpointTracker
            CheckpointTracker
        {
            get
            {
                return checkpointTracker;
            }
        }

        public MatchTimer MatchTimer
        {
            get
            {
                return matchTimer;
            }
        }

        private void Awake()
        {
            ResolveLocalReferences();
        }

        private void OnEnable()
        {
            ResolveLocalReferences();
            SubscribeFinish();
            ResolveTimerAndSubscribe();
        }

        private void Start()
        {
            ResolveTimerAndSubscribe();
        }

        private void OnDisable()
        {
            UnsubscribeFinish();
            UnsubscribeTimer();
        }

        public void Configure(
            PlayerFinishState newFinishState,
            PlayerRankingParticipant
                newRankingParticipant,
            PlayerHeightTracker newHeightTracker,
            PlayerCheckpointTracker
                newCheckpointTracker,
            MatchTimer newMatchTimer
        )
        {
            UnsubscribeFinish();
            UnsubscribeTimer();

            finishState =
                newFinishState;

            rankingParticipant =
                newRankingParticipant;

            heightTracker =
                newHeightTracker;

            checkpointTracker =
                newCheckpointTracker;

            matchTimer =
                newMatchTimer;

            ResolveLocalReferences();

            if (isActiveAndEnabled)
            {
                SubscribeFinish();
                ResolveTimerAndSubscribe();
            }
        }

        public bool TryCreateResult()
        {
            if (hasResult)
            {
                return false;
            }

            ResolveLocalReferences();

            if (
                finishState == null ||
                rankingParticipant == null ||
                heightTracker == null ||
                checkpointTracker == null
            )
            {
                return false;
            }

            heightTracker.RefreshHeight();

            bool finished =
                finishState.IsFinished;

            int finalRank =
                finished
                    ? finishState.FinishOrder
                    : rankingParticipant
                        .CurrentRank;

            int finishOrder =
                finished
                    ? finishState.FinishOrder
                    : 0;

            double finishTime =
                finished
                    ? finishState.FinishTime
                    : PlayerMatchResult
                        .NoFinishTime;

            currentResult =
                new PlayerMatchResult(
                    rankingParticipant.PlayerId,
                    finalRank,
                    finished,
                    finishOrder,
                    finishTime,
                    heightTracker
                        .HighestHeightCentimeters,
                    checkpointTracker
                        .CurrentCheckpointId
                );

            hasResult = true;

            ResultCreated?.Invoke(
                currentResult
            );

            return true;
        }

        public bool TryCreateTimeExpiredResult()
        {
            if (
                finishState != null &&
                finishState.IsFinished
            )
            {
                return TryCreateResult();
            }

            return TryCreateResult();
        }

        private void HandleFinished(
            PlayerFinishState player
        )
        {
            TryCreateResult();
        }

        private void HandleTimeExpired()
        {
            TryCreateTimeExpiredResult();
        }

        private void ResolveLocalReferences()
        {
            if (finishState == null)
            {
                finishState =
                    GetComponent<
                        PlayerFinishState
                    >();
            }

            if (rankingParticipant == null)
            {
                rankingParticipant =
                    GetComponent<
                        PlayerRankingParticipant
                    >();
            }

            if (heightTracker == null)
            {
                heightTracker =
                    GetComponent<
                        PlayerHeightTracker
                    >();
            }

            if (checkpointTracker == null)
            {
                checkpointTracker =
                    GetComponent<
                        PlayerCheckpointTracker
                    >();
            }
        }

        private void ResolveTimerAndSubscribe()
        {
            if (matchTimer == null)
            {
                matchTimer =
                    FindFirstObjectByType<
                        MatchTimer
                    >();
            }

            SubscribeTimer();
        }

        private void SubscribeFinish()
        {
            if (
                finishSubscribed ||
                finishState == null
            )
            {
                return;
            }

            finishState.Finished +=
                HandleFinished;

            finishSubscribed = true;
        }

        private void UnsubscribeFinish()
        {
            if (
                !finishSubscribed ||
                finishState == null
            )
            {
                finishSubscribed = false;
                return;
            }

            finishState.Finished -=
                HandleFinished;

            finishSubscribed = false;
        }

        private void SubscribeTimer()
        {
            if (
                timerSubscribed ||
                matchTimer == null
            )
            {
                return;
            }

            matchTimer.TimeExpired +=
                HandleTimeExpired;

            timerSubscribed = true;
        }

        private void UnsubscribeTimer()
        {
            if (
                !timerSubscribed ||
                matchTimer == null
            )
            {
                timerSubscribed = false;
                return;
            }

            matchTimer.TimeExpired -=
                HandleTimeExpired;

            timerSubscribed = false;
        }
    }
}
