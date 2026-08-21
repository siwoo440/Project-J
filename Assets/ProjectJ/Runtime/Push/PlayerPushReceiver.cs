using System;
using ProjectJ.Checkpoint;
using ProjectJ.Finish;
using UnityEngine;

namespace ProjectJ.Push
{
    [DisallowMultipleComponent]
    [RequireComponent(
        typeof(Rigidbody)
    )]
    [RequireComponent(
        typeof(PlayerExternalForceAccumulator)
    )]
    public sealed class PlayerPushReceiver :
        MonoBehaviour
    {
        [SerializeField]
        private Rigidbody body;

        [SerializeField]
        private PlayerRespawnProtection
            respawnProtection;

        [SerializeField]
        private PlayerFinishState finishState;

        [SerializeField]
        private PlayerExternalForceAccumulator
            externalForceAccumulator;

        public event Action<Vector3> PushReceived;

        public Rigidbody Body
        {
            get
            {
                return body;
            }
        }

        public PlayerExternalForceAccumulator
            ExternalForceAccumulator
        {
            get
            {
                ResolveReferences();

                return
                    externalForceAccumulator;
            }
        }

        public bool IsRespawnProtected
        {
            get
            {
                ResolveReferences();

                return
                    respawnProtection != null &&
                    respawnProtection.IsProtected;
            }
        }

        public bool CanReceivePush
        {
            get
            {
                ResolveReferences();

                if (
                    body == null ||
                    body.isKinematic
                )
                {
                    return false;
                }

                if (
                    finishState != null &&
                    finishState.IsFinished
                )
                {
                    return false;
                }

                if (
                    respawnProtection != null &&
                    !respawnProtection
                        .TryAcceptHostileEffect()
                )
                {
                    return false;
                }

                return
                    externalForceAccumulator != null;
            }
        }

        private void Awake()
        {
            ResolveReferences();

            if (
                externalForceAccumulator == null
            )
            {
                externalForceAccumulator =
                    gameObject.AddComponent<
                        PlayerExternalForceAccumulator
                    >();
            }
        }

        public void Configure(
            Rigidbody newBody,
            PlayerRespawnProtection
                newRespawnProtection,
            PlayerFinishState
                newFinishState
        )
        {
            body =
                newBody;

            respawnProtection =
                newRespawnProtection;

            finishState =
                newFinishState;

            ResolveReferences();
        }

        public bool TryApplyPush(
            Vector3 velocityChange
        )
        {
            if (!CanReceivePush)
            {
                return false;
            }

            bool applied =
                externalForceAccumulator
                    .AddVelocityChange(
                        velocityChange
                    );

            if (applied)
            {
                PushReceived?.Invoke(
                    new Vector3(
                        velocityChange.x,
                        0f,
                        velocityChange.z
                    )
                );
            }

            return applied;
        }

        private void ResolveReferences()
        {
            if (body == null)
            {
                body =
                    GetComponent<Rigidbody>();
            }

            if (respawnProtection == null)
            {
                respawnProtection =
                    GetComponent<
                        PlayerRespawnProtection
                    >();
            }

            if (finishState == null)
            {
                finishState =
                    GetComponent<
                        PlayerFinishState
                    >();
            }

            if (
                externalForceAccumulator == null
            )
            {
                externalForceAccumulator =
                    GetComponent<
                        PlayerExternalForceAccumulator
                    >();
            }
        }
    }
}
