using Fusion; // Networked 상태와 TickTimer 사용
using ProjectJ.Items; // 소형화 물약 정책 사용
using UnityEngine; // Collider 공간 검사 사용

namespace ProjectJ.Networking.Fusion
{
    public sealed partial class ProjectJNetworkItemInventory
    {
        private readonly Collider[] shrinkRestoreOverlapBuffer =
            new Collider[24]; // 정상 크기 복귀 공간 검사 버퍼

        [Networked]
        private int NetworkShrinkPotionStateValue
        {
            get;
            set;
        }

        [Networked]
        private TickTimer NetworkShrinkPotionTimer
        {
            get;
            set;
        }

        [Networked]
        private int NetworkShrinkPotionRevision
        {
            get;
            set;
        }

        public ProjectJShrinkPotionState ShrinkPotionState =>
            (ProjectJShrinkPotionState)
            NetworkShrinkPotionStateValue;

        public bool IsShrinkPotionActive =>
            ShrinkPotionState ==
            ProjectJShrinkPotionState.Active;

        public bool IsShrinkRestorePending =>
            ShrinkPotionState ==
            ProjectJShrinkPotionState.RestorePending;

        public bool IsShrinkApplied =>
            ProjectJShrinkPotionPolicy.ShouldApplyShrink(
                ShrinkPotionState
            );

        public float ShrinkPotionRemaining
        {
            get
            {
                if (
                    Runner == null ||
                    !IsShrinkPotionActive
                )
                {
                    return 0f;
                }

                float? remaining =
                    NetworkShrinkPotionTimer.RemainingTime(
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

        private bool UseShrinkPotionAuthority()
        {
            ResolveReferences();

            bool runnerReady =
                Runner != null &&
                Runner.IsServer &&
                Object != null &&
                Object.IsValid &&
                Object.HasStateAuthority;

            bool gameplayAllowed =
                externalGameplay != null &&
                externalGameplay.GameplayInputAllowed;

            if (
                !ProjectJShrinkPotionPolicy.CanUse(
                    runnerReady,
                    gameplayAllowed,
                    ShrinkPotionState
                )
            )
            {
                return false;
            }

            NetworkShrinkPotionStateValue =
                (int)ProjectJShrinkPotionState.Active;

            NetworkShrinkPotionTimer =
                TickTimer.CreateFromSeconds(
                    Runner,
                    ProjectJShrinkPotionPolicy.DurationSeconds
                );

            NetworkShrinkPotionRevision++;

            return true;
        }

        private void UpdateShrinkPotionAuthority()
        {
            if (
                Runner == null ||
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority
            )
            {
                return;
            }

            ResolveReferences();

            bool gameplayAllowed =
                externalGameplay != null &&
                externalGameplay.GameplayInputAllowed;

            if (!gameplayAllowed)
            {
                if (
                    ShrinkPotionState !=
                    ProjectJShrinkPotionState.Inactive
                )
                {
                    ClearShrinkPotionAuthority();
                }

                return;
            }

            if (
                ShrinkPotionState ==
                ProjectJShrinkPotionState.Active &&
                NetworkShrinkPotionTimer.ExpiredOrNotRunning(
                    Runner
                )
            )
            {
                ProjectJShrinkPotionState nextState =
                    ProjectJShrinkPotionPolicy.ResolveExpiredState(
                        CanRestoreNormalSizeAuthority()
                    );

                SetShrinkPotionStateAuthority(
                    nextState
                );

                return;
            }

            if (
                ShrinkPotionState ==
                ProjectJShrinkPotionState.RestorePending
            )
            {
                ProjectJShrinkPotionState nextState =
                    ProjectJShrinkPotionPolicy.ResolvePendingState(
                        CanRestoreNormalSizeAuthority()
                    );

                if (
                    nextState !=
                    ShrinkPotionState
                )
                {
                    SetShrinkPotionStateAuthority(
                        nextState
                    );
                }
            }
        }

        private void ClearShrinkPotionAuthority()
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority
            )
            {
                return;
            }

            NetworkShrinkPotionStateValue =
                (int)ProjectJShrinkPotionState.Inactive;

            NetworkShrinkPotionTimer =
                TickTimer.None;

            NetworkShrinkPotionRevision++;
        }

        private void SetShrinkPotionStateAuthority(
            ProjectJShrinkPotionState state
        )
        {
            NetworkShrinkPotionStateValue =
                (int)state;

            if (
                state !=
                ProjectJShrinkPotionState.Active
            )
            {
                NetworkShrinkPotionTimer =
                    TickTimer.None;
            }

            NetworkShrinkPotionRevision++;
        }

        private bool CanRestoreNormalSizeAuthority()
        {
            bool crouching =
                networkPlayer != null &&
                networkPlayer.IsCrouching;

            float targetHeight =
                crouching
                    ? ProjectJShrinkPotionPolicy.CrouchBaseHeight
                    : ProjectJShrinkPotionPolicy.StandingBaseHeight;

            float targetRadius =
                ProjectJShrinkPotionPolicy.BaseRadius;

            float clearanceRadius =
                targetRadius *
                ProjectJShrinkPotionPolicy.RestoreClearanceRadiusScale;

            Vector3 basePosition =
                transform.position;

            Vector3 bottom =
                basePosition +
                Vector3.up *
                targetRadius;

            Vector3 top =
                basePosition +
                Vector3.up *
                Mathf.Max(
                    targetRadius,
                    targetHeight -
                    targetRadius
                );

            int overlapCount =
                Physics.OverlapCapsuleNonAlloc(
                    bottom,
                    top,
                    clearanceRadius,
                    shrinkRestoreOverlapBuffer,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore
                );

            for (
                int index = 0;
                index < overlapCount;
                index++
            )
            {
                Collider candidate =
                    shrinkRestoreOverlapBuffer[index];

                shrinkRestoreOverlapBuffer[index] =
                    null;

                if (candidate == null)
                {
                    continue;
                }

                Transform candidateTransform =
                    candidate.transform;

                if (
                    candidateTransform == transform ||
                    candidateTransform.IsChildOf(
                        transform
                    )
                )
                {
                    continue;
                }

                return false;
            }

            return true;
        }
    }
}
