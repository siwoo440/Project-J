using System;
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
    public sealed class PlayerExternalForceReceiver :
        MonoBehaviour
    {
        [SerializeField]
        private Rigidbody body;

        [SerializeField]
        private PlayerFinishState finishState;

        [SerializeField]
        private PlayerExternalForceAccumulator
            externalForceAccumulator;

        public event Action<
            ExternalForceSource,
            Vector3
        > ExternalForceApplied;

        public Rigidbody Body
        {
            get
            {
                ResolveReferences();

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

        public bool CanReceiveExternalForce
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

                return
                    externalForceAccumulator != null;
            }
        }

        private void Awake()
        {
            ResolveReferences();
        }

        public void Configure(
            Rigidbody newBody,
            PlayerFinishState newFinishState,
            PlayerExternalForceAccumulator
                newExternalForceAccumulator
        )
        {
            body =
                newBody;

            finishState =
                newFinishState;

            externalForceAccumulator =
                newExternalForceAccumulator;

            ResolveReferences();
        }

        public bool TryApplyVelocityChange(
            ExternalForceSource source,
            Vector3 velocityChange
        )
        {
            if (!CanReceiveExternalForce)
            {
                return false;
            }

            Vector3 horizontalChange =
                new Vector3(
                    velocityChange.x,
                    0f,
                    velocityChange.z
                );

            bool applied =
                externalForceAccumulator
                    .AddVelocityChange(
                        horizontalChange
                    );

            if (applied)
            {
                ExternalForceApplied?.Invoke(
                    source,
                    horizontalChange
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
