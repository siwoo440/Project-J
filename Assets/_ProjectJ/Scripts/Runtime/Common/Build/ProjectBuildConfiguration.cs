using System.Collections.Generic; // 읽기 전용 씬 경로 목록 기능 참조

namespace ProjectJ.Build // 프로젝트 빌드 공통 네임스페이스 선언
{
    public static class ProjectBuildConfiguration // Project J 빌드 프로필 경로와 출력 규칙 관리 형식 선언
    {
        public const string DevelopmentProfileName = "ProjectJ_Windows_Development"; // Windows 개발 빌드 프로필 이름 선언
        public const string DevelopmentProfileAssetPath = "Assets/_ProjectJ/Settings/BuildProfiles/ProjectJ_Windows_Development.asset"; // Windows 개발 빌드 프로필 에셋 경로 선언
        public const string DevelopmentScriptingDefine = "PROJECTJ_DEVELOPMENT"; // Windows 개발 프로필 전용 스크립팅 정의 선언

        public const string DevelopmentBuildDirectory = "Builds/Windows/Development"; // Windows 개발 빌드 출력 폴더 경로 선언
        public const string DevelopmentExecutableName = "ProjectJ_Development.exe"; // Windows 개발 클라이언트 실행 파일 이름 선언
        public const string DevelopmentBuildPath = DevelopmentBuildDirectory + "/" + DevelopmentExecutableName; // Windows 개발 클라이언트 전체 출력 경로 선언

        public const string DevelopmentLogDirectory = "Logs/Builds/Windows"; // Windows 개발 빌드 로그 폴더 경로 선언
        public const string DevelopmentBuildSummaryFileName = "DevelopmentBuildSummary.log"; // 개발 빌드 요약 로그 파일 이름 선언
        public const string DevelopmentUnityLogFileName = "DevelopmentBuild.log"; // Unity 명령행 개발 빌드 로그 파일 이름 선언
        public const string DevelopmentBuildSummaryPath = DevelopmentLogDirectory + "/" + DevelopmentBuildSummaryFileName; // 개발 빌드 요약 로그 전체 경로 선언
        public const string DevelopmentUnityLogPath = DevelopmentLogDirectory + "/" + DevelopmentUnityLogFileName; // Unity 명령행 개발 빌드 로그 전체 경로 선언

        public const string BootstrapScenePath = "Assets/_ProjectJ/Scenes/Game/Bootstrap.unity"; // Bootstrap 빌드 씬 경로 선언
        public const string MainMenuScenePath = "Assets/_ProjectJ/Scenes/Game/MainMenu.unity"; // MainMenu 빌드 씬 경로 선언
        public const string LobbyScenePath = "Assets/_ProjectJ/Scenes/Game/Lobby.unity"; // Lobby 빌드 씬 경로 선언
        public const string MatchLoadingScenePath = "Assets/_ProjectJ/Scenes/Game/MatchLoading.unity"; // MatchLoading 빌드 씬 경로 선언
        public const string GameScenePath = "Assets/_ProjectJ/Scenes/Game/Game.unity"; // Game 빌드 씬 경로 선언
        public const string TestsScenePath = "Assets/_ProjectJ/Scenes/Game/Tests.unity"; // 개발 클라이언트에서 제외할 Tests 씬 경로 선언

        private static readonly string[] DevelopmentScenePathValues = // Windows 개발 클라이언트 씬 순서 배열 선언
        {
            BootstrapScenePath, // 첫 번째 Bootstrap 씬 경로 추가
            MainMenuScenePath, // 두 번째 MainMenu 씬 경로 추가
            LobbyScenePath, // 세 번째 Lobby 씬 경로 추가
            MatchLoadingScenePath, // 네 번째 MatchLoading 씬 경로 추가
            GameScenePath // 다섯 번째 Game 씬 경로 추가
        };

        public static IReadOnlyList<string> DevelopmentScenePaths => DevelopmentScenePathValues; // Windows 개발 클라이언트 씬 경로 목록 반환
    }
}
