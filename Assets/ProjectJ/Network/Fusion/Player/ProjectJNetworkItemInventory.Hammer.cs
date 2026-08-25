using Fusion; // Networked와 TickTimer 사용
using ProjectJ.Items; // 망치 정책 사용

namespace ProjectJ.Networking.Fusion // Fusion 네트워크 네임스페이스
{
    public sealed partial class ProjectJNetworkItemInventory // 망치 Networked 상태
    {
        [Networked] // 망치 지속 시간 동기화
        private TickTimer NetworkHammerTimer
        {
            get; // Networked 상태 조회
            set; // State Authority 상태 갱신
        }

        public bool IsHammerActive =>
            IsTimerActive(NetworkHammerTimer); // 망치 활성 여부 조회

        public float HammerRemaining =>
            GetRemainingTime(NetworkHammerTimer); // 망치 남은 시간 조회

        private void InitializeHammerAuthority()
        {
            NetworkHammerTimer = TickTimer.None; // 최초 망치 상태 초기화
        }

        private bool UseHammerAuthority()
        {
            if (
                Runner == null || // Runner 존재 확인
                !ProjectJHammerPolicy.CanActivate(IsHammerActive) // 중복 사용 차단
            )
            {
                return false; // 사용 실패 처리
            }

            NetworkHammerTimer = TickTimer.CreateFromSeconds( // 서버 기준 지속 타이머 생성
                Runner, // 현재 Fusion Runner 사용
                ProjectJHammerPolicy.DurationSeconds // 6초 지속 시간 사용
            );

            return true; // 사용 성공 반환
        }

        private void ClearHammerAuthority()
        {
            NetworkHammerTimer = TickTimer.None; // 초기화·부활 시 망치 효과 제거
        }
    }
}
