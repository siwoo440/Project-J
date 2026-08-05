using System.IO; // 빌드 출력 경로 확장자 검사 기능 참조
using System.Linq; // 씬 경로와 스크립팅 정의 검색 기능 참조
using NUnit.Framework; // EditMode 테스트 기능 참조
using ProjectJ.Build; // Project J 빌드 경로와 씬 목록 참조
using UnityEditor; // Unity 에셋 데이터베이스와 빌드 설정 기능 참조
using UnityEditor.Build.Profile; // Unity Build Profile 기능 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{
    [Category("Build")] // Build Profile 관련 테스트 Category 지정
    public sealed class DevelopmentBuildProfileTests // Windows 개발 Build Profile 경로와 설정 검증 테스트 형식 선언
    {
        [Test] // Unity Test Runner 테스트 지정
        public void DevelopmentBuildPathsUseExpectedProjectFolders() // 개발 빌드 출력과 로그 경로 규칙 검증
        {
            Assert.AreEqual("Builds/Windows/Development/ProjectJ_Development.exe", ProjectBuildConfiguration.DevelopmentBuildPath); // Windows 개발 실행 파일 출력 경로 검증
            Assert.AreEqual(".exe", Path.GetExtension(ProjectBuildConfiguration.DevelopmentBuildPath)); // Windows 개발 실행 파일 확장자 검증
            Assert.AreEqual("Logs/Builds/Windows/DevelopmentBuildSummary.log", ProjectBuildConfiguration.DevelopmentBuildSummaryPath); // 개발 빌드 요약 로그 경로 검증
            Assert.AreEqual("Logs/Builds/Windows/DevelopmentBuild.log", ProjectBuildConfiguration.DevelopmentUnityLogPath); // Unity 명령행 빌드 로그 경로 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void DevelopmentSceneListUsesExpectedOrder() // 개발 클라이언트 씬 목록의 개수와 순서 검증
        {
            string[] expectedScenes = // 개발 클라이언트에 필요한 기준 씬 순서 선언
            {
                ProjectBuildConfiguration.BootstrapScenePath, // Bootstrap 기준 씬 경로 추가
                ProjectBuildConfiguration.MainMenuScenePath, // MainMenu 기준 씬 경로 추가
                ProjectBuildConfiguration.LobbyScenePath, // Lobby 기준 씬 경로 추가
                ProjectBuildConfiguration.MatchLoadingScenePath, // MatchLoading 기준 씬 경로 추가
                ProjectBuildConfiguration.GameScenePath // Game 기준 씬 경로 추가
            };

            CollectionAssert.AreEqual(expectedScenes, ProjectBuildConfiguration.DevelopmentScenePaths); // 실제 개발 씬 순서와 기준 순서 일치 여부 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void DevelopmentSceneListExcludesTestsScene() // 개발 클라이언트 씬 목록의 Tests 씬 제외 여부 검증
        {
            CollectionAssert.DoesNotContain(ProjectBuildConfiguration.DevelopmentScenePaths, ProjectBuildConfiguration.TestsScenePath); // Tests 씬이 개발 클라이언트 씬 목록에 없는지 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void RequiredDevelopmentScenesExist() // 개발 클라이언트에 필요한 모든 씬 에셋 존재 여부 검증
        {
            foreach (string scenePath in ProjectBuildConfiguration.DevelopmentScenePaths) // 모든 개발 클라이언트 씬 경로 순회
            {
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath); // 현재 경로의 Unity 씬 에셋 불러오기
                Assert.IsNotNull(sceneAsset, $"필수 개발 빌드 씬이 없습니다: {scenePath}"); // 현재 필수 씬 에셋 존재 여부 검증
            }
        }

        [Test] // Unity Test Runner 테스트 지정
        public void DevelopmentBuildProfileAssetExistsAtExpectedPath() // Windows 개발 Build Profile 에셋 고정 경로 존재 여부 검증
        {
            BuildProfile profile = LoadDevelopmentProfile(); // 고정 경로의 Windows 개발 Build Profile 불러오기

            Assert.IsNotNull(profile, $"개발 Build Profile이 없습니다: {ProjectBuildConfiguration.DevelopmentProfileAssetPath}"); // Windows 개발 Build Profile 에셋 존재 여부 검증
            Assert.AreEqual(ProjectBuildConfiguration.DevelopmentProfileName, profile.name); // Windows 개발 Build Profile 에셋 이름 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void DevelopmentBuildProfileOverridesGlobalScenes() // Windows 개발 Build Profile의 전용 씬 목록 사용 여부 검증
        {
            BuildProfile profile = LoadRequiredDevelopmentProfile(); // 존재가 보장된 Windows 개발 Build Profile 불러오기

            Assert.IsTrue(profile.overrideGlobalScenes); // 글로벌 씬 목록 오버라이드 활성화 여부 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void DevelopmentBuildProfileSceneOrderMatchesExpected() // Windows 개발 Build Profile의 활성 씬 목록과 순서 검증
        {
            BuildProfile profile = LoadRequiredDevelopmentProfile(); // 존재가 보장된 Windows 개발 Build Profile 불러오기
            EditorBuildSettingsScene[] profileScenes = profile.scenes ?? System.Array.Empty<EditorBuildSettingsScene>(); // Build Profile에 저장된 씬 목록 또는 빈 배열 조회
            string[] enabledScenes = profileScenes // Build Profile에 저장된 씬 목록 조회
                .Where(scene => scene.enabled) // 활성화된 씬만 선택
                .Select(scene => scene.path) // 활성 씬 경로 선택
                .ToArray(); // 활성 씬 경로 배열 생성

            CollectionAssert.AreEqual(ProjectBuildConfiguration.DevelopmentScenePaths, enabledScenes); // Build Profile 활성 씬과 기준 씬 순서 일치 여부 검증
            CollectionAssert.DoesNotContain(enabledScenes, ProjectBuildConfiguration.TestsScenePath); // Build Profile에서 Tests 씬 제외 여부 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void DevelopmentBuildProfileContainsDevelopmentDefine() // Windows 개발 Build Profile의 전용 스크립팅 정의 포함 여부 검증
        {
            BuildProfile profile = LoadRequiredDevelopmentProfile(); // 존재가 보장된 Windows 개발 Build Profile 불러오기

            Assert.IsNotNull(profile.scriptingDefines); // Build Profile 스크립팅 정의 배열 존재 여부 검증
            CollectionAssert.Contains(profile.scriptingDefines, ProjectBuildConfiguration.DevelopmentScriptingDefine); // Project J 개발 전용 정의 포함 여부 검증
        }

        private static BuildProfile LoadDevelopmentProfile() // 고정 경로의 Windows 개발 Build Profile 불러오기
        {
            return AssetDatabase.LoadAssetAtPath<BuildProfile>(ProjectBuildConfiguration.DevelopmentProfileAssetPath); // Windows 개발 Build Profile 에셋 반환
        }

        private static BuildProfile LoadRequiredDevelopmentProfile() // 존재하지 않으면 테스트를 즉시 실패시키는 Windows 개발 Build Profile 불러오기
        {
            BuildProfile profile = LoadDevelopmentProfile(); // 고정 경로의 Windows 개발 Build Profile 불러오기
            Assert.IsNotNull(profile, $"개발 Build Profile이 없습니다: {ProjectBuildConfiguration.DevelopmentProfileAssetPath}"); // Build Profile 누락 시 현재 테스트 실패 처리
            return profile; // 존재가 확인된 Windows 개발 Build Profile 반환
        }
    }
}
