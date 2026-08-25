using Fusion; // Networked와 TickTimer 사용
using ProjectJ.Items; // 제트팩 공통 정책 사용

namespace ProjectJ.Networking.Fusion // Fusion 네트워크 네임스페이스
{
    public sealed partial class ProjectJNetworkItemInventory // 제트팩 네트워크 효과
    {
        [Networked] // 제트팩 남은 연료 시간 동기화
        private TickTimer NetworkJetpackTimer
        {
            get; // Networked 타이머 조회
            set; // State Authority 타이머 갱신
        }

        public bool IsJetpackActive =>
            IsTimerActive(NetworkJetpackTimer); // 제트팩 활성 여부

        public float JetpackRemaining =>
            GetRemainingTime(NetworkJetpackTimer); // 제트팩 남은 연료 시간

        private void InitializeJetpackAuthority()
        {
            NetworkJetpackTimer = TickTimer.None; // 최초 제트팩 상태 초기화
        }

        private bool UseJetpackAuthority()
        {
            if (
                Runner == null ||
                IsGiantBalloonActive
            )
            {
                return false; // Runner 없음 또는 거대 풍선과 상승 효과 중첩 차단
            }

            NetworkJetpackTimer = TickTimer.CreateFromSeconds( // 서버 기준 연료 타이머 생성
                Runner, // 현재 Fusion Runner 사용
                ProjectJJetpackPolicy.DurationSeconds // 확정 5초 지속 시간 사용
            );

            return true; // 사용 성공 반환
        }

        private void ClearJetpackAuthority()
        {
            NetworkJetpackTimer = TickTimer.None; // 초기화·부활 시 제트팩 제거
        }
    }
}
