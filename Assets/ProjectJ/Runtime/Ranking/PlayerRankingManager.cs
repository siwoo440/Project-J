using System.Collections.Generic;
using UnityEngine;

namespace ProjectJ.Ranking
{
    [DisallowMultipleComponent]
    public sealed class PlayerRankingManager :
        MonoBehaviour
    {
        [SerializeField]
        private List<PlayerRankingParticipant>
            participants =
                new List<
                    PlayerRankingParticipant
                >();

        private readonly List<int> heightBuffer =
            new List<int>();

        private readonly List<
            PlayerRankingParticipant
        > activeParticipantBuffer =
            new List<
                PlayerRankingParticipant
            >();

        private int nextRuntimePlayerId;

        public IReadOnlyList<
            PlayerRankingParticipant
        > Participants
        {
            get
            {
                return participants;
            }
        }

        private void Awake()
        {
            RegisterExistingParticipants();
        }

        private void LateUpdate()
        {
            RecalculateRanks();
        }

        public void Register(
            PlayerRankingParticipant participant
        )
        {
            if (
                participant == null ||
                participants.Contains(
                    participant
                )
            )
            {
                return;
            }

            participant.AssignPlayerIdIfUnset(
                nextRuntimePlayerId
            );

            nextRuntimePlayerId++;

            participants.Add(
                participant
            );
        }

        public void Unregister(
            PlayerRankingParticipant participant
        )
        {
            if (participant == null)
            {
                return;
            }

            participants.Remove(
                participant
            );
        }

        public void RecalculateRanks()
        {
            RemoveNullParticipants();

            heightBuffer.Clear();
            activeParticipantBuffer.Clear();

            int fixedRankOffset = 0;

            for (
                int i = 0;
                i < participants.Count;
                i++
            )
            {
                PlayerRankingParticipant
                    participant =
                        participants[i];

                if (
                    !participant
                        .HeightRankingEligible
                )
                {
                    fixedRankOffset++;
                    continue;
                }

                activeParticipantBuffer.Add(
                    participant
                );

                heightBuffer.Add(
                    participant
                        .CurrentHeightCentimeters
                );
            }

            if (heightBuffer.Count == 0)
            {
                return;
            }

            int[] ranks =
                PlayerRankingCalculator
                    .CalculateRanks(
                        heightBuffer
                    );

            for (
                int i = 0;
                i <
                activeParticipantBuffer.Count;
                i++
            )
            {
                activeParticipantBuffer[i]
                    .SetCurrentRank(
                        ranks[i] +
                        fixedRankOffset
                    );
            }
        }

        private void RegisterExistingParticipants()
        {
            PlayerRankingParticipant[] existing =
                FindObjectsByType<
                    PlayerRankingParticipant
                >(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None
                );

            for (
                int i = 0;
                i < existing.Length;
                i++
            )
            {
                Register(
                    existing[i]
                );
            }
        }

        private void RemoveNullParticipants()
        {
            for (
                int i =
                    participants.Count - 1;
                i >= 0;
                i--
            )
            {
                if (participants[i] == null)
                {
                    participants.RemoveAt(
                        i
                    );
                }
            }
        }
    }
}
