using Fusion; // Networked와 TickTimer 사용
using ProjectJ.Items; // 투명 망토 정책 사용
using UnityEngine; // Mathf 사용

namespace ProjectJ.Networking.Fusion
{
    public sealed partial class ProjectJNetworkItemInventory
    {
        [Networked]
        private NetworkBool NetworkInvisibilityCloakActive
        {
            get;
            set;
        }

        [Networked]
        private TickTimer NetworkInvisibilityCloakTimer
        {
            get;
            set;
        }

        [Networked]
        private int NetworkInvisibilityCloakRevision
        {
            get;
            set;
        }

        public bool IsInvisibilityCloakActive =>
            NetworkInvisibilityCloakActive;

        public bool IsAutoTargetTrackable =>
            ProjectJInvisibilityCloakPolicy.IsAutoTargetTrackable(
                IsInvisibilityCloakActive
            );

        public int InvisibilityCloakRevision =>
            NetworkInvisibilityCloakRevision;

        public float InvisibilityCloakRemaining
        {
            get
            {
                if (
                    Runner == null ||
                    !IsInvisibilityCloakActive
                )
                {
                    return 0f;
                }

                float? remaining =
                    NetworkInvisibilityCloakTimer.RemainingTime(
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

        private void EnsureInvisibilityPresentation()
        {
            if (
                GetComponent<ProjectJNetworkInvisibilityPresentation>() ==
                null
            )
            {
                gameObject.AddComponent<ProjectJNetworkInvisibilityPresentation>();
            }
        }

        private void InitializeInvisibilityCloakAuthority()
        {
            NetworkInvisibilityCloakActive =
                false;

            NetworkInvisibilityCloakTimer =
                TickTimer.None;

            NetworkInvisibilityCloakRevision =
                0;
        }

        private bool UseInvisibilityCloakAuthority()
        {
            ResolveReferences();

            bool authorityReady =
                Runner != null &&
                Object != null &&
                Object.IsValid &&
                Object.HasStateAuthority;

            bool gameplayAllowed =
                externalGameplay != null &&
                externalGameplay.GameplayInputAllowed;

            if (
                !ProjectJInvisibilityCloakPolicy.CanUse(
                    authorityReady,
                    gameplayAllowed,
                    IsInvisibilityCloakActive
                )
            )
            {
                return false;
            }

            NetworkInvisibilityCloakActive =
                true;

            NetworkInvisibilityCloakTimer =
                TickTimer.CreateFromSeconds(
                    Runner,
                    ProjectJInvisibilityCloakPolicy.DurationSeconds
                );

            NetworkInvisibilityCloakRevision++;

            return true;
        }

        private void UpdateInvisibilityCloakAuthority()
        {
            if (
                Runner == null ||
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority ||
                !IsInvisibilityCloakActive
            )
            {
                return;
            }

            ResolveReferences();

            if (
                externalGameplay == null ||
                !externalGameplay.GameplayInputAllowed ||
                NetworkInvisibilityCloakTimer.ExpiredOrNotRunning(
                    Runner
                )
            )
            {
                ClearInvisibilityCloakAuthority();
            }
        }

        internal void BreakInvisibilityCloakForPushAuthority()
        {
            if (
                !ProjectJInvisibilityCloakPolicy.ShouldBreakForPush(
                    IsInvisibilityCloakActive
                )
            )
            {
                return;
            }

            ClearInvisibilityCloakAuthority();
        }

        private void BreakInvisibilityCloakForSuccessfulItemUseAuthority(
            int usedItemId
        )
        {
            bool usedInvisibilityCloak =
                usedItemId ==
                ProjectJInvisibilityCloakPolicy.NetworkItemId;

            if (
                !ProjectJInvisibilityCloakPolicy.ShouldBreakForSuccessfulItemUse(
                    IsInvisibilityCloakActive,
                    true,
                    usedInvisibilityCloak
                )
            )
            {
                return;
            }

            ClearInvisibilityCloakAuthority();
        }

        private void ClearInvisibilityCloakAuthority()
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority
            )
            {
                return;
            }

            if (
                !NetworkInvisibilityCloakActive &&
                (
                    Runner == null ||
                    NetworkInvisibilityCloakTimer.ExpiredOrNotRunning(
                        Runner
                    )
                )
            )
            {
                return;
            }

            NetworkInvisibilityCloakActive =
                false;

            NetworkInvisibilityCloakTimer =
                TickTimer.None;

            NetworkInvisibilityCloakRevision++;
        }
    }
}
