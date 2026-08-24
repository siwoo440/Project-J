using System; // 수치 최대값 계산 기능

namespace ProjectJ.Debugging // Runtime 진단 정책 네임스페이스
{
    public static class ProjectJMovementQualityPolicy // 이동 품질 진단 공통 정책
    {
        public static string GetRoleLabel( // 현재 PC 기준 Player 역할 표시 계산
            bool hasInputAuthority, // Input Authority 보유 여부
            bool hasStateAuthority // State Authority 보유 여부
        )
        {
            if (hasInputAuthority) // 직접 조작 Player 확인
            {
                return "LOCAL INPUT"; // 로컬 입력 역할 반환
            }

            if (hasStateAuthority) // Host 상태 권한 Player 확인
            {
                return "STATE AUTHORITY"; // 상태 권한 역할 반환
            }

            return "REMOTE PROXY"; // 원격 Proxy 역할 반환
        }

        public static float AccumulatePeak( // 측정 구간 최대값 누적
            float currentPeak, // 기존 최대값
            float sample // 새 측정값
        )
        {
            float safeCurrentPeak = // 음수 방지 기존 최대값
                Math.Max( // 0과 기존값 비교
                    0f, // 최소 허용값
                    currentPeak // 기존 최대값
                );

            float safeSample = // 음수 방지 새 측정값
                Math.Max( // 0과 표본값 비교
                    0f, // 최소 허용값
                    sample // 새 측정값
                );

            return Math.Max( // 두 정상값 중 큰 값 반환
                safeCurrentPeak, // 기존 최대값
                safeSample // 새 측정값
            );
        }

        public static float CalculateElapsed( // 측정 구간 경과 시간 계산
            float startedAt, // 측정 시작 시각
            float currentTime // 현재 시각
        )
        {
            return Math.Max( // 음수 시간 차이 방지
                0f, // 최소 경과 시간
                currentTime - startedAt // 실제 시간 차이
            );
        }
    }
}
