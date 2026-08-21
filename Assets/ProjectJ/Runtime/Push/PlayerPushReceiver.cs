using ProjectJ.Checkpoint;
using ProjectJ.Finish;
using UnityEngine;

namespace ProjectJ.Push
{
    [DisallowMultipleComponent]
    [RequireComponent(
        typeof(Rigidbody)
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

        public Rigidbody Body
        {
            get
            {
                return body;
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

                return true;
            }
        }

        private void Awake()
        {
            ResolveReferences();
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

            Vector3 currentVelocity =
                body.linearVelocity;

            Vector3 horizontalVelocityChange =
                new Vector3(
                    velocityChange.x,
                    0f,
                    velocityChange.z
                );

            body.linearVelocity =
                new Vector3(
                    currentVelocity.x +
                        horizontalVelocityChange.x,
                    currentVelocity.y,
                    currentVelocity.z +
                        horizontalVelocityChange.z
                );

            return true;
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
        }
    }
}
