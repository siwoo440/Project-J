namespace ProjectJ.Editor // 프로젝트 Editor 메뉴 경로 네임스페이스
{ // 기능별 Editor 메뉴 경로 정의
    internal static class ProjectJEditorMenuPaths // Project J 상단 메뉴 공통 경로 선언
    { // 기능별 상단 메뉴 경로 상수 정의
        public const string Root = "Project J"; // Project J 최상위 메뉴 경로

        public const string ProjectSettings = Root + "/01. 프로젝트 설정"; // 프로젝트 설정 대분류 경로
        public const string ProjectSettingsScenes = ProjectSettings + "/씬"; // 씬 설정 하위 경로
        public const string ProjectSettingsServices = ProjectSettings + "/서비스"; // 공통 서비스 하위 경로
        public const string ProjectSettingsPhysics = ProjectSettings + "/물리"; // 물리 설정 하위 경로

        public const string PlayerAndInput = Root + "/02. 플레이어와 입력"; // 플레이어와 입력 대분류 경로
        public const string PlayerInput = PlayerAndInput + "/입력"; // 입력 설정 하위 경로
        public const string PlayerPlayMode = PlayerAndInput + "/Play Mode"; // Play Mode 시작 설정 하위 경로
        public const string PlayerSettings = PlayerAndInput + "/플레이어 설정"; // 플레이어 데이터 설정 하위 경로

        public const string Data = Root + "/03. 데이터"; // 데이터 대분류 경로
        public const string DataBase = Data + "/기본 데이터"; // 기본 데이터 하위 경로
        public const string DataCsv = Data + "/CSV"; // CSV 하위 경로
        public const string DataCatalog = Data + "/카탈로그"; // 런타임 카탈로그 하위 경로

        public const string Tests = Root + "/04. 테스트"; // 테스트 대분류 경로
        public const string TestFramework = Tests + "/테스트 프레임워크"; // 테스트 프레임워크 하위 경로

        public const string Build = Root + "/05. 빌드"; // 빌드 대분류 경로
        public const string DevelopmentBuild = Build + "/개발 빌드"; // 개발 빌드 하위 경로

        public const string Map = Root + "/06. 맵"; // 맵 대분류 경로
        public const string MapModules = Map + "/맵 모듈"; // 맵 모듈 하위 경로
        public const string MapGeneration = Map + "/맵 생성"; // 맵 생성 하위 경로
        public const string MapValidation = Map + "/검증"; // 맵 검증 하위 경로

        public const string Obstacles = Root + "/07. 장애물"; // 장애물 대분류 경로
        public const string MapObstacles = Obstacles + "/맵 장애물"; // 맵 장애물 하위 경로

        public const string Items = Root + "/08. 아이템"; // 아이템 대분류 경로
        public const string ItemInventory = Items + "/인벤토리"; // 아이템 인벤토리 하위 경로
        public const string ItemChests = Items + "/아이템 상자"; // 아이템 상자 하위 경로
        public const string ItemEffects = Items + "/효과"; // 아이템 효과 하위 경로
        public const string ItemValidation = Items + "/통합 검증"; // 아이템 통합 검증 하위 경로

        public const string UI = Root + "/09. UI"; // UI 대분류 경로
        public const string GameUI = UI + "/게임 화면"; // 게임 화면 UI 하위 경로
    } // 기능별 상단 메뉴 경로 상수 정의
} // 프로젝트 Editor 메뉴 경로 네임스페이스
