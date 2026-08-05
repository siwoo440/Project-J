namespace ProjectJ.Testing // 프로젝트 테스트 프레임워크 네임스페이스 선언
{
    public static class ProjectTestFramework // Project J 테스트 프레임워크 공통 상수 관리 형식 선언
    {
        public const int FrameworkVersion = 1; // 현재 테스트 프레임워크 구조 버전 선언
        public const string SceneRootName = "ProjectJ_TestSceneRoot"; // Tests 씬 전용 루트 오브젝트 이름 선언
        public const string SceneMarkerReadyMessage = "Test scene marker initialized."; // Tests 씬 마커 초기화 로그 문구 선언
        public const string SmokeCategory = "Smoke"; // 빠른 기본 검증용 테스트 Category 이름 선언
        public const string SceneCategory = "Scene"; // 씬 로드 검증용 테스트 Category 이름 선언
        public const string LoggingCategory = "Logging"; // 공통 로그 규칙 검증용 테스트 Category 이름 선언
    }
}
