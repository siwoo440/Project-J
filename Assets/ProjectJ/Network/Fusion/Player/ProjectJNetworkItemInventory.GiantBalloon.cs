using Fusion; // Networked와 TickTimer 사용
using ProjectJ.Items; // 거대 풍선 정책 사용

namespace ProjectJ.Networking.Fusion
{
    public sealed partial class ProjectJNetworkItemInventory
    {
        [Networked]
        private int NetworkGiantBalloonPhaseValue
        {
            get;
            set;
        }

        [Networked]
        private TickTimer NetworkGiantBalloonTimer
        {
            get;
            set;
        }

        public ProjectJGiantBalloonPhase GiantBalloonPhase =>
            (ProjectJGiantBalloonPhase)NetworkGiantBalloonPhaseValue;

        public bool IsGiantBalloonActive =>
            ProjectJGiantBalloonPolicy.IsActive(
                GiantBalloonPhase
            );

        public bool IsGiantBalloonRising =>
            ProjectJGiantBalloonPolicy.IsRising(
                GiantBalloonPhase
            );

        public bool IsGiantBalloonDescending =>
            ProjectJGiantBalloonPolicy.IsDescending(
                GiantBalloonPhase
            );

        public float GiantBalloonRemaining =>
            GetRemainingTime(
                NetworkGiantBalloonTimer
            );

        private void InitializeGiantBalloonAuthority()
        {
            NetworkGiantBalloonPhaseValue =
                (int)ProjectJGiantBalloonPhase.Inactive;

            NetworkGiantBalloonTimer =
                TickTimer.None;
        }

        private bool UseGiantBalloonAuthority()
        {
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
                !ProjectJGiantBalloonPolicy.CanUse(
                    runnerReady,
                    gameplayAllowed,
                    IsJetpackActive,
                    IsGiantBalloonActive
                )
            )
            {
                return false;
            }

            SetGiantBalloonPhaseAuthority(
                ProjectJGiantBalloonPhase.Rising
            );

            return true;
        }

        private void UpdateGiantBalloonAuthority()
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority
            )
            {
                return;
            }

            bool gameplayAllowed =
                externalGameplay != null &&
                externalGameplay.GameplayInputAllowed;

            if (
                ProjectJGiantBalloonPolicy.ShouldClear(
                    gameplayAllowed,
                    Object.IsValid
                )
            )
            {
                ClearGiantBalloonAuthority();
                return;
            }

            if (!IsGiantBalloonActive)
            {
                return;
            }

            if (
                Runner == null ||
                !NetworkGiantBalloonTimer.ExpiredOrNotRunning(
                    Runner
                )
            )
            {
                return;
            }

            ProjectJGiantBalloonPhase nextPhase =
                ProjectJGiantBalloonPolicy.GetNextPhase(
                    GiantBalloonPhase
                );

            SetGiantBalloonPhaseAuthority(
                nextPhase
            );
        }

        private void SetGiantBalloonPhaseAuthority(
            ProjectJGiantBalloonPhase phase
        )
        {
            NetworkGiantBalloonPhaseValue =
                (int)phase;

            float duration =
                ProjectJGiantBalloonPolicy.GetPhaseDuration(
                    phase
                );

            if (
                Runner == null ||
                duration <= 0f
            )
            {
                NetworkGiantBalloonTimer =
                    TickTimer.None;
                return;
            }

            NetworkGiantBalloonTimer =
                TickTimer.CreateFromSeconds(
                    Runner,
                    duration
                );
        }

        private void ClearGiantBalloonAuthority()
        {
            NetworkGiantBalloonPhaseValue =
                (int)ProjectJGiantBalloonPhase.Inactive;

            NetworkGiantBalloonTimer =
                TickTimer.None;
        }
    }
}
