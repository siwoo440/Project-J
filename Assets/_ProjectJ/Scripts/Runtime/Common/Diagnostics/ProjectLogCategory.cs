namespace ProjectJ.Diagnostics // 프로젝트 공통 로그 네임스페이스 선언
{
    public enum ProjectLogCategory // 프로젝트 로그 분류 선언
    {
        Core = 0, // 공통 초기화와 핵심 서비스 로그 분류 선언
        Scene = 1, // 씬 전환과 씬 상태 로그 분류 선언
        Input = 2, // 입력 시스템 로그 분류 선언
        Data = 3, // 데이터 로드와 검증 로그 분류 선언
        Physics = 4, // 물리와 충돌 규칙 로그 분류 선언
        Gameplay = 5, // 일반 게임 플레이 로그 분류 선언
        UI = 6, // UI와 화면 흐름 로그 분류 선언
        Audio = 7, // 오디오 시스템 로그 분류 선언
        Network = 8, // 네트워크와 서버 통신 로그 분류 선언
        Test = 9 // 자동 테스트와 테스트 씬 로그 분류 선언
    }
}
