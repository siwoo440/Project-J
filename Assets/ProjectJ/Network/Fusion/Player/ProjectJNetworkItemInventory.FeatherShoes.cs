using Fusion; // Networked와 TickTimer 사용
using ProjectJ.Items; // 깃털 신발 공통 정책 사용

namespace ProjectJ.Networking.Fusion // Fusion 네트워크 네임스페이스
{
    public sealed partial class ProjectJNetworkItemInventory // 깃털 신발 네트워크 효과
    {
        [Networked] // 깃털 신발 남은 시간 동기화
        private TickTimer NetworkFeatherShoesTimer
        {
            get;
            set;
        }

        public bool IsFeatherShoesActive =>
            IsTimerActive(NetworkFeatherShoesTimer); // 깃털 신발 활성 여부

        public float FeatherShoesRemaining =>
            GetRemainingTime(NetworkFeatherShoesTimer); // 깃털 신발 남은 시간

        private void InitializeFeatherShoesAuthority()
        {
            NetworkFeatherShoesTimer = TickTimer.None; // 최초 효과 상태 초기화
        }

        private bool UseFeatherShoesAuthority()
        {
            if (Runner == null)
            {
                return false; // Runner 없음 처리
            }

            NetworkFeatherShoesTimer = TickTimer.CreateFromSeconds(
                Runner,
                ProjectJFeatherShoesPolicy.DurationSeconds
            ); // 중첩 없이 남은 시간을 7초로 갱신

            return true; // 사용 성공 반환
        }

        private void ClearFeatherShoesAuthority()
        {
            NetworkFeatherShoesTimer = TickTimer.None; // 부활·초기화 시 효과 제거
        }
    }
}
