using Fusion; // NetworkBool, PlayerRef, TickTimer 사용
using ProjectJ.Items; // 손거울 공통 정책 사용
using UnityEngine; // Vector3 사용

namespace ProjectJ.Networking.Fusion
{
    public sealed partial class ProjectJNetworkItemInventory
    {
        [Networked]
        private NetworkBool NetworkHandMirrorActive
        {
            get;
            set;
        }

        [Networked]
        private TickTimer NetworkHandMirrorTimer
        {
            get;
            set;
        }

        [Networked]
        private int NetworkHandMirrorRevision
        {
            get;
            set;
        }

        public bool IsHandMirrorActive
        {
            get
            {
                if (
                    !NetworkHandMirrorActive ||
                    Runner == null
                )
                {
                    return false;
                }

                return
                    !NetworkHandMirrorTimer.ExpiredOrNotRunning(
                        Runner
                    );
            }
        }

        public float HandMirrorRemaining
        {
            get
            {
                if (
                    Runner == null ||
                    !NetworkHandMirrorActive
                )
                {
                    return 0f;
                }

                float? remaining =
                    NetworkHandMirrorTimer.RemainingTime(
                        Runner
                    );

                return
                    remaining.HasValue
                        ? Mathf.Max(
                            0f,
                            remaining.Value
                        )
                        : 0f;
            }
        }

        public int HandMirrorRevision =>
            NetworkHandMirrorRevision;

        private void InitializeHandMirrorAuthority()
        {
            NetworkHandMirrorActive =
                false;

            NetworkHandMirrorTimer =
                TickTimer.None;

            NetworkHandMirrorRevision =
                0;
        }

        private bool UseHandMirrorAuthority()
        {
            ResolveReferences();

            bool authorityReady =
                Object != null &&
                Object.IsValid &&
                Object.HasStateAuthority;

            bool runnerReady =
                Runner != null;

            bool gameplayAllowed =
                externalGameplay != null &&
                externalGameplay.GameplayInputAllowed;

            if (
                !ProjectJHandMirrorPolicy.CanActivate(
                    authorityReady,
                    runnerReady,
                    gameplayAllowed
                )
            )
            {
                return false;
            }

            NetworkHandMirrorActive =
                true;

            NetworkHandMirrorTimer =
                TickTimer.CreateFromSeconds(
                    Runner,
                    ProjectJHandMirrorPolicy.DurationSeconds
                );

            NetworkHandMirrorRevision++;

            return true;
        }

        private void UpdateHandMirrorAuthority()
        {
            if (!NetworkHandMirrorActive)
            {
                return;
            }

            ResolveReferences();

            bool shouldClear =
                Runner == null ||
                externalGameplay == null ||
                !externalGameplay.GameplayInputAllowed ||
                NetworkHandMirrorTimer.ExpiredOrNotRunning(
                    Runner
                );

            if (shouldClear)
            {
                ClearHandMirrorAuthority();
            }
        }

        private void ClearHandMirrorAuthority()
        {
            if (
                !NetworkHandMirrorActive &&
                (
                    Runner == null ||
                    NetworkHandMirrorTimer.ExpiredOrNotRunning(
                        Runner
                    )
                )
            )
            {
                NetworkHandMirrorTimer =
                    TickTimer.None;

                return;
            }

            NetworkHandMirrorActive =
                false;

            NetworkHandMirrorTimer =
                TickTimer.None;

            NetworkHandMirrorRevision++;
        }

        public bool TryReflectHandMirrorProjectileAuthority(
            PlayerRef incomingOwner,
            Vector3 incomingDirection,
            out PlayerRef reflectedOwner,
            out Vector3 reflectedDirection
        )
        {
            reflectedOwner =
                incomingOwner;

            reflectedDirection =
                ProjectJHandMirrorPolicy.ResolveReflectedDirection(
                    incomingDirection,
                    -transform.forward
                );

            ResolveReferences();

            bool authorityReady =
                Object != null &&
                Object.IsValid &&
                Object.HasStateAuthority &&
                Runner != null;

            bool gameplayAllowed =
                externalGameplay != null &&
                externalGameplay.GameplayInputAllowed;

            bool mirrorActive =
                IsHandMirrorActive;

            bool isIncomingOwner =
                Object != null &&
                Object.IsValid &&
                Object.InputAuthority ==
                incomingOwner;

            bool isRewinding =
                IsRewindActive;

            if (
                !ProjectJHandMirrorPolicy.CanReflect(
                    authorityReady,
                    mirrorActive,
                    gameplayAllowed,
                    isIncomingOwner,
                    isRewinding
                )
            )
            {
                return false;
            }

            reflectedOwner =
                Object.InputAuthority;

            NetworkHandMirrorRevision++;

            return true;
        }
    }
}
