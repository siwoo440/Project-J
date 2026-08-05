namespace ProjectJ.Core.Services // 프로젝트 공통 서비스 네임스페이스 선언
{
    public enum GameServiceState // 공통 서비스 초기화 상태 선언
    {
        NotInitialized = 0, // 아직 초기화되지 않은 상태 선언
        Initializing = 1, // 현재 초기화가 진행 중인 상태 선언
        Initialized = 2, // 초기화가 정상 완료된 상태 선언
        Failed = 3 // 초기화 과정에서 실패한 상태 선언
    }
}
