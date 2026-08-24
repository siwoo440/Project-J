namespace ProjectJ.Networking // 공통 네트워크 실행 정책 네임스페이스
{
    public static class ProjectJNetworkExecutionPolicy // Host·Client와 Server 실행 분리 정책
    {
        public static bool IsDedicatedServerBuild // 현재 Server 빌드 여부
        {
            get // 현재 빌드 상태 반환
            {
#if UNITY_SERVER // Unity Dedicated Server 전용 정의 확인
                return true; // Server 빌드 상태 반환
#else // 일반 Windows·Editor 정의 사용
                return false; // 일반 Windows·Editor 상태 반환
#endif // 빌드 정의 분기 종료
            }
        }

        public static bool ShouldInstallHostClientBootstrap( // 일반 Bootstrap 설치 판단
            bool isDedicatedServerBuild // Server 빌드 여부
        )
        {
            return !isDedicatedServerBuild; // 일반 실행에서만 설치 허용
        }

        public static bool ShouldAutoStartDedicatedServer( // Dedicated 자동 시작 판단
            bool isDedicatedServerBuild, // Server 빌드 여부
            bool startOnPlay // 자동 시작 설정
        )
        {
            return // 자동 시작 결과 반환
                isDedicatedServerBuild && // Server 빌드 조건
                startOnPlay; // 자동 시작 선택 조건
        }
    }
}
