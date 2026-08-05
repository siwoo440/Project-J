namespace ProjectJ.Core.SceneFlow // 프로젝트 씬 흐름 네임스페이스 선언
{
    public enum GameSceneId // 프로젝트에서 사용하는 씬 식별자 선언
    {
        Bootstrap = 0, // 게임 초기 진입 씬 식별자 선언
        MainMenu = 1, // 메인 메뉴 씬 식별자 선언
        Lobby = 2, // 로비 씬 식별자 선언
        MatchLoading = 3, // 경기 로딩 씬 식별자 선언
        Game = 4, // 실제 경기 씬 식별자 선언
        Tests = 5 // 기능 검증 씬 식별자 선언
    }
}
