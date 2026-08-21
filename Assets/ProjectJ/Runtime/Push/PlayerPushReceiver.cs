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
    [RequireComponent(
        typeof(PlayerExternalForceReceiver)
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

        [SerializeField]
        private PlayerExternalForceReceiver
            externalForceReceiver;

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

                if (
                    externalForceReceiver != null
                )
                {
                    return
                        externalForceReceiver
                            .ExternalForceAccumulator;
                }

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
                    externalForceReceiver == null ||
                    !externalForceReceiver
                        .CanReceiveExternalForce
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

                return true;
            }
        }

        private void Awake()
        {
            ResolveReferences();

            if (
                externalForceReceiver == null
            )
            {
                externalForceReceiver =
                    gameObject.AddComponent<
                        PlayerExternalForceReceiver
                    >();

                externalForceReceiver.Configure(
                    body,
                    finishState,
                    externalForceAccumulator
                );
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

            if (
                externalForceReceiver != null
            )
            {
                externalForceReceiver.Configure(
                    body,
                    finishState,
                    externalForceAccumulator
                );
            }
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
                externalForceReceiver
                    .TryApplyVelocityChange(
                        ExternalForceSource.Push,
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

            if (
                externalForceReceiver == null
            )
            {
                externalForceReceiver =
                    GetComponent<
                        PlayerExternalForceReceiver
                    >();
            }
        }
    }
}
