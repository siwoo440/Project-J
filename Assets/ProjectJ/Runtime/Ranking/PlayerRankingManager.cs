using System.Collections.Generic;
using UnityEngine;

namespace ProjectJ.Ranking
{
    [DisallowMultipleComponent]
    public sealed class PlayerRankingManager : MonoBehaviour
    {
        [SerializeField]
        private List<PlayerRankingParticipant> participants =
            new List<PlayerRankingParticipant>();

        private readonly List<int> heightBuffer =
            new List<int>();

        private int nextRuntimePlayerId;

        public IReadOnlyList<PlayerRankingParticipant> Participants
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

            for (
                int i = 0;
                i < participants.Count;
                i++
            )
            {
                heightBuffer.Add(
                    participants[i]
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
                i < participants.Count;
                i++
            )
            {
                participants[i]
                    .SetCurrentRank(
                        ranks[i]
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
