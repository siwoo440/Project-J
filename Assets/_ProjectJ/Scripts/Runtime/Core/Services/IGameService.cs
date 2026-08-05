namespace ProjectJ.Core.Services // 프로젝트 공통 서비스 네임스페이스 선언
{
    public interface IGameService // 모든 공통 서비스가 구현할 규칙 선언
    {
        string ServiceName { get; } // 로그와 진단에 사용할 서비스 이름 반환
        int InitializationOrder { get; } // 서비스 초기화 순서 값 반환
        GameServiceState State { get; } // 현재 서비스 초기화 상태 반환
        void Initialize(); // 서비스를 한 번만 초기화
    }
}
