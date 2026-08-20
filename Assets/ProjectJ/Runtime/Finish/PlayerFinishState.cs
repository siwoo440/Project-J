using System;
using ProjectJ.Player;
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

        [SerializeField]
        private bool finishDepartureApplied;

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

        public bool FinishDepartureApplied
        {
            get
            {
                return finishDepartureApplied;
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

            ApplyFinishedPlayerDeparture();

            return true;
        }

        public void ApplyFinishedPlayerDeparture()
        {
            if (finishDepartureApplied)
            {
                return;
            }

            finishDepartureApplied = true;

            PlayerCameraRelativeMovement movement =
                GetComponent<
                    PlayerCameraRelativeMovement
                >();

            if (movement != null)
            {
                movement.enabled =
                    false;
            }

            PlayerLedgeClimber ledgeClimber =
                GetComponent<
                    PlayerLedgeClimber
                >();

            if (ledgeClimber != null)
            {
                ledgeClimber.enabled =
                    false;
            }

            PlayerLedgeDetector ledgeDetector =
                GetComponent<
                    PlayerLedgeDetector
                >();

            if (ledgeDetector != null)
            {
                ledgeDetector.enabled =
                    false;
            }

            Rigidbody body =
                GetComponent<Rigidbody>();

            if (body != null)
            {
                body.linearVelocity =
                    Vector3.zero;

                body.angularVelocity =
                    Vector3.zero;

                body.detectCollisions =
                    false;

                body.isKinematic =
                    true;
            }

            Collider[] colliders =
                GetComponentsInChildren<
                    Collider
                >(
                    true
                );

            for (
                int i = 0;
                i < colliders.Length;
                i++
            )
            {
                colliders[i].enabled =
                    false;
            }

            Animator[] animators =
                GetComponentsInChildren<
                    Animator
                >(
                    true
                );

            for (
                int i = 0;
                i < animators.Length;
                i++
            )
            {
                animators[i].enabled =
                    false;
            }

            Renderer[] renderers =
                GetComponentsInChildren<
                    Renderer
                >(
                    true
                );

            for (
                int i = 0;
                i < renderers.Length;
                i++
            )
            {
                renderers[i].enabled =
                    false;
            }
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
