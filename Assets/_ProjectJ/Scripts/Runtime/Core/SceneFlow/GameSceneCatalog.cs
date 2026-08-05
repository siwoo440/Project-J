using System; // 범위 예외 기능 참조

namespace ProjectJ.Core.SceneFlow // 프로젝트 씬 흐름 네임스페이스 선언
{
    public static class GameSceneCatalog // 씬 이름과 경로 관리 형식 선언
    {
        public const string SceneFolderPath = "Assets/_ProjectJ/Scenes/Game"; // 게임 씬 공통 폴더 경로 선언

        public static GameSceneId[] GetBuildOrder() // 빌드에 등록할 씬 순서 반환
        {
            return new[] // 씬 식별자 배열 생성
            {
                GameSceneId.Bootstrap, // Bootstrap 씬을 첫 번째로 등록
                GameSceneId.MainMenu, // MainMenu 씬을 두 번째로 등록
                GameSceneId.Lobby, // Lobby 씬을 세 번째로 등록
                GameSceneId.MatchLoading, // MatchLoading 씬을 네 번째로 등록
                GameSceneId.Game, // Game 씬을 다섯 번째로 등록
                GameSceneId.Tests // Tests 씬을 여섯 번째로 등록
            };
        }

        public static string GetSceneName(GameSceneId sceneId) // 씬 식별자에 대응하는 씬 이름 반환
        {
            switch (sceneId) // 전달된 씬 식별자 분기
            {
                case GameSceneId.Bootstrap: // Bootstrap 식별자 처리
                    return "Bootstrap"; // Bootstrap 씬 이름 반환

                case GameSceneId.MainMenu: // MainMenu 식별자 처리
                    return "MainMenu"; // MainMenu 씬 이름 반환

                case GameSceneId.Lobby: // Lobby 식별자 처리
                    return "Lobby"; // Lobby 씬 이름 반환

                case GameSceneId.MatchLoading: // MatchLoading 식별자 처리
                    return "MatchLoading"; // MatchLoading 씬 이름 반환

                case GameSceneId.Game: // Game 식별자 처리
                    return "Game"; // Game 씬 이름 반환

                case GameSceneId.Tests: // Tests 식별자 처리
                    return "Tests"; // Tests 씬 이름 반환

                default: // 정의되지 않은 씬 식별자 처리
                    throw new ArgumentOutOfRangeException(nameof(sceneId), sceneId, "정의되지 않은 씬 식별자입니다."); // 잘못된 씬 식별자 예외 발생
            }
        }

        public static string GetScenePath(GameSceneId sceneId) // 씬 식별자에 대응하는 에셋 경로 반환
        {
            string sceneName = GetSceneName(sceneId); // 씬 식별자에서 씬 이름 조회
            return $"{SceneFolderPath}/{sceneName}.unity"; // 완성된 Unity 씬 에셋 경로 반환
        }
    }
}
