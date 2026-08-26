using Fusion; // TickTimer 사용

namespace ProjectJ.Networking.Fusion
{
    public sealed partial class ProjectJNetworkExternalGameplay
    {
        public bool TryBeginCountdownForFilledRosterAuthority(
            int expectedParticipantCount
        )
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority ||
                Runner == null ||
                expectedParticipantCount <= 0 ||
                !IsGameSceneActive() ||
                !ProjectJNetworkBotRosterManager.IsCountdownAllowed(
                    Runner
                )
            )
            {
                return false; // Host·Game·Roster 안정화 조건 미충족 차단
            }

            ProjectJNetworkExternalGameplay coordinator =
                GetMatchCoordinator(); // 현재 Human Match Coordinator 조회

            if (
                coordinator == null ||
                coordinator.Object == null ||
                !coordinator.Object.IsValid ||
                !coordinator.Object.HasStateAuthority
            )
            {
                return false; // 유효한 Host Coordinator 없음 처리
            }

            ProjectJNetworkMatchState state =
                (ProjectJNetworkMatchState)coordinator.NetworkMatchStateValue; // 현재 전체 경기 상태 조회

            if (
                state == ProjectJNetworkMatchState.Countdown ||
                state == ProjectJNetworkMatchState.Playing
            )
            {
                return true; // 이미 Countdown 또는 경기 시작 상태 유지
            }

            if (state != ProjectJNetworkMatchState.Preparing)
            {
                return false; // 시작 가능한 Preparing 외 상태 차단
            }

            coordinator.BeginCountdownAuthority(); // 공통 3초 Countdown 시작 처리 재사용

            return
                (ProjectJNetworkMatchState)coordinator.NetworkMatchStateValue ==
                ProjectJNetworkMatchState.Countdown; // 실제 Countdown 진입 여부 반환
        }

        public bool CancelCountdownForRosterAuthority()
        {
            if (
                Object == null ||
                !Object.IsValid ||
                !Object.HasStateAuthority ||
                Runner == null ||
                !IsGameSceneActive()
            )
            {
                return false; // Host Game Scene 외 Countdown 취소 차단
            }

            ProjectJNetworkExternalGameplay coordinator =
                GetMatchCoordinator(); // 현재 Human Match Coordinator 조회

            if (
                coordinator == null ||
                coordinator.Object == null ||
                !coordinator.Object.IsValid ||
                !coordinator.Object.HasStateAuthority ||
                (ProjectJNetworkMatchState)coordinator.NetworkMatchStateValue !=
                ProjectJNetworkMatchState.Countdown
            )
            {
                return false; // 실제 Countdown 상태에서만 취소 허용
            }

            coordinator.NetworkMatchStateValue =
                (int)ProjectJNetworkMatchState.Preparing; // 인원 부족으로 Preparing 복귀

            coordinator.NetworkCountdownTimer =
                TickTimer.None; // 진행 중 Countdown 제거

            coordinator.NetworkMatchTimer =
                TickTimer.None; // 경기 Timer 대기 상태 복원

            return true; // Roster 부족 Countdown 취소 완료
        }
    }
}
