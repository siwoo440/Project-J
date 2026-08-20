using ProjectJ.Player;
using UnityEngine;

namespace ProjectJ.Ranking
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerHeightTracker))]
    public sealed class PlayerRankingParticipant :
        MonoBehaviour
    {
        [SerializeField]
        private int playerId = -1;

        [SerializeField]
        private PlayerHeightTracker heightTracker;

        [SerializeField]
        private int currentRank = 1;

        [SerializeField]
        private bool heightRankingEligible = true;

        public int PlayerId
        {
            get
            {
                return playerId;
            }
        }

        public PlayerHeightTracker HeightTracker
        {
            get
            {
                return heightTracker;
            }
        }

        public int CurrentRank
        {
            get
            {
                return currentRank;
            }
        }

        public bool HeightRankingEligible
        {
            get
            {
                return heightRankingEligible;
            }
        }

        public int CurrentHeightCentimeters
        {
            get
            {
                if (heightTracker == null)
                {
                    return 0;
                }

                return
                    heightTracker
                        .CurrentHeightCentimeters;
            }
        }

        public float CurrentHeight
        {
            get
            {
                if (heightTracker == null)
                {
                    return 0f;
                }

                return
                    heightTracker.CurrentHeight;
            }
        }

        private void Awake()
        {
            ResolveHeightTracker();
        }

        private void OnEnable()
        {
            ResolveHeightTracker();

            PlayerRankingManager manager =
                FindFirstObjectByType<
                    PlayerRankingManager
                >();

            if (manager != null)
            {
                manager.Register(
                    this
                );
            }
        }

        private void OnDisable()
        {
            PlayerRankingManager manager =
                FindFirstObjectByType<
                    PlayerRankingManager
                >();

            if (manager != null)
            {
                manager.Unregister(
                    this
                );
            }
        }

        public void Configure(
            int newPlayerId,
            PlayerHeightTracker newHeightTracker
        )
        {
            playerId =
                newPlayerId;

            heightTracker =
                newHeightTracker;
        }

        public void AssignPlayerIdIfUnset(
            int newPlayerId
        )
        {
            if (playerId >= 0)
            {
                return;
            }

            playerId =
                newPlayerId;
        }

        public void SetCurrentRank(
            int newRank
        )
        {
            currentRank =
                Mathf.Max(
                    1,
                    newRank
                );
        }

        public void SetHeightRankingEligible(
            bool eligible
        )
        {
            heightRankingEligible =
                eligible;
        }

        private void ResolveHeightTracker()
        {
            if (heightTracker != null)
            {
                return;
            }

            heightTracker =
                GetComponent<
                    PlayerHeightTracker
                >();
        }
    }
}
