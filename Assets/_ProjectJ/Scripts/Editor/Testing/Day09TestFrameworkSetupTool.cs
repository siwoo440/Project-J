using System.Collections.Generic; // 검증 오류 목록 기능 참조
using System.Linq; // Build Settings 씬 경로 검색 기능 참조
using ProjectJ.Core.SceneFlow; // Tests 씬 식별자와 경로 기능 참조
using ProjectJ.Diagnostics; // 프로젝트 공통 로그 기능 참조
using ProjectJ.Testing; // 테스트 프레임워크 상수와 씬 마커 참조
using UnityEditor; // Unity 에디터 메뉴와 에셋 기능 참조
using UnityEditor.SceneManagement; // Unity 에디터 씬 열기와 저장 기능 참조
using UnityEngine; // Unity GameObject와 Object 기능 참조
using UnityEngine.SceneManagement; // Unity 씬 열기 모드와 루트 오브젝트 기능 참조

namespace ProjectJ.Editor // 프로젝트 에디터 전용 네임스페이스 선언
{
    internal static class Day09TestFrameworkSetupTool // 9일차 테스트 프레임워크 자동 구성과 검증 메뉴 선언
    {
        private const string ConfigureMenuPath = ProjectJEditorMenuPaths.TestFramework + "/테스트 프레임워크 구성 (Day 09일차)"; // 테스트 프레임워크 구성 메뉴 경로 선언
        private const string ValidateMenuPath = ProjectJEditorMenuPaths.TestFramework + "/테스트 프레임워크 검증 (Day 09일차)"; // 테스트 프레임워크 검증 메뉴 경로 선언
        private const string EditModeAssemblyPath = "Assets/_ProjectJ/Tests/EditMode/ProjectJ.Tests.EditMode.asmdef"; // EditMode 테스트 어셈블리 경로 선언
        private const string PlayModeAssemblyPath = "Assets/_ProjectJ/Tests/PlayMode/ProjectJ.Tests.PlayMode.asmdef"; // PlayMode 테스트 어셈블리 경로 선언

        [MenuItem(ConfigureMenuPath)] // Unity 상단 메뉴에 테스트 프레임워크 구성 항목 등록
        private static void ConfigureTestFramework() // 테스트 어셈블리와 Tests 씬 기본 구조 구성
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) // 현재 열린 씬 저장 또는 취소 결과 확인
            {
                return; // 사용자가 취소한 경우 테스트 프레임워크 구성 중단
            }

            List<string> assetErrors = CollectRequiredAssetErrors(); // 테스트 어셈블리와 Tests 씬 에셋 누락 오류 수집

            if (assetErrors.Count > 0) // 필수 에셋 누락 오류 존재 여부 확인
            {
                LogErrors(assetErrors); // 발견된 필수 에셋 오류 전체 출력
                EditorUtility.DisplayDialog("Project J Day 09", "필수 테스트 파일이 누락되었습니다. Console을 확인합니다.", "확인"); // 테스트 구성 실패 대화상자 표시
                return; // Tests 씬 변경 작업 중단
            }

            EnsureTestsSceneInBuildSettings(); // Tests 씬의 Build Settings 등록 상태 보장
            Scene testsScene = OpenTestsScene(); // Tests 씬 단독 열기
            ProjectTestSceneMarker marker = EnsureTestSceneMarker(testsScene); // Tests 씬 루트와 마커 존재 상태 보장

            EditorUtility.SetDirty(marker); // 테스트 씬 마커 직렬화 변경 상태 표시
            EditorSceneManager.MarkSceneDirty(testsScene); // Tests 씬 변경 상태 표시
            EditorSceneManager.SaveScene(testsScene, GameSceneCatalog.GetScenePath(GameSceneId.Tests)); // 변경된 Tests 씬 저장
            AssetDatabase.SaveAssets(); // 변경된 프로젝트 에셋 저장
            AssetDatabase.Refresh(); // Project 창과 에셋 데이터베이스 새로고침

            Selection.activeGameObject = marker.gameObject; // 구성된 테스트 씬 루트 오브젝트 선택
            EditorGUIUtility.PingObject(marker.gameObject); // Hierarchy에서 테스트 씬 루트 위치 강조

            List<string> validationErrors = CollectValidationErrors(testsScene); // 구성 직후 테스트 프레임워크 전체 검증

            if (validationErrors.Count > 0) // 구성 후 검증 오류 존재 여부 확인
            {
                LogErrors(validationErrors); // 발견된 검증 오류 전체 출력
                EditorUtility.DisplayDialog("Project J Day 09", $"테스트 프레임워크 구성 후 오류 {validationErrors.Count}개를 발견했습니다.", "확인"); // 구성 후 검증 실패 대화상자 표시
                return; // 성공 로그와 대화상자 표시 생략
            }

            ProjectLog.Info(ProjectLogCategory.Test, "EditMode·PlayMode 테스트 프레임워크 구성을 완료했습니다.", "TEST_FRAMEWORK_READY"); // 테스트 프레임워크 구성 완료 로그 출력
            EditorUtility.DisplayDialog("Project J Day 09", "EditMode·PlayMode 테스트와 Tests 씬 구성을 완료했습니다.", "확인"); // 테스트 프레임워크 구성 성공 대화상자 표시
        }

        [MenuItem(ValidateMenuPath)] // Unity 상단 메뉴에 테스트 프레임워크 검증 항목 등록
        private static void ValidateTestFramework() // 현재 테스트 어셈블리와 Tests 씬 구성 검증
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) // 현재 열린 씬 저장 또는 취소 결과 확인
            {
                return; // 사용자가 취소한 경우 테스트 프레임워크 검증 중단
            }

            List<string> assetErrors = CollectRequiredAssetErrors(); // 테스트 어셈블리와 Tests 씬 에셋 누락 오류 수집

            if (assetErrors.Count > 0) // 필수 에셋 누락 오류 존재 여부 확인
            {
                LogErrors(assetErrors); // 발견된 필수 에셋 오류 전체 출력
                EditorUtility.DisplayDialog("Project J Day 09", $"필수 테스트 파일 오류 {assetErrors.Count}개를 발견했습니다.", "확인"); // 필수 에셋 검증 실패 대화상자 표시
                return; // Tests 씬 세부 검증 중단
            }

            Scene testsScene = OpenTestsScene(); // Tests 씬 단독 열기
            List<string> validationErrors = CollectValidationErrors(testsScene); // 테스트 프레임워크 전체 검증 오류 수집

            if (validationErrors.Count == 0) // 테스트 프레임워크 검증 오류가 없는지 확인
            {
                ProjectLog.Info(ProjectLogCategory.Test, "테스트 프레임워크 검증을 통과했습니다.", "TEST_FRAMEWORK_VALID"); // 테스트 프레임워크 검증 성공 로그 출력
                EditorUtility.DisplayDialog("Project J Day 09", "테스트 어셈블리와 Tests 씬 구성이 정상입니다.", "확인"); // 테스트 프레임워크 검증 성공 대화상자 표시
                return; // 오류 로그 처리 생략
            }

            LogErrors(validationErrors); // 발견된 테스트 프레임워크 검증 오류 전체 출력
            EditorUtility.DisplayDialog("Project J Day 09", $"테스트 프레임워크 오류 {validationErrors.Count}개를 발견했습니다.", "확인"); // 테스트 프레임워크 검증 실패 대화상자 표시
        }

        [MenuItem(ConfigureMenuPath, true)] // 테스트 프레임워크 구성 메뉴 활성 조건 등록
        [MenuItem(ValidateMenuPath, true)] // 테스트 프레임워크 검증 메뉴 활성 조건 등록
        private static bool ValidateEditorMenu() // Play Mode가 아닐 때만 9일차 메뉴 실행 허용
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode; // Play Mode 진입 또는 실행 중이 아닌 경우 활성화
        }

        private static List<string> CollectRequiredAssetErrors() // 필수 테스트 어셈블리와 Tests 씬 에셋 누락 오류 수집
        {
            List<string> errors = new List<string>(); // 필수 에셋 검증 오류 목록 생성

            if (AssetDatabase.LoadMainAssetAtPath(EditModeAssemblyPath) == null) // EditMode 테스트 어셈블리 존재 여부 확인
            {
                errors.Add($"EditMode 테스트 어셈블리가 없습니다: {EditModeAssemblyPath}"); // EditMode 테스트 어셈블리 누락 오류 추가
            }

            if (AssetDatabase.LoadMainAssetAtPath(PlayModeAssemblyPath) == null) // PlayMode 테스트 어셈블리 존재 여부 확인
            {
                errors.Add($"PlayMode 테스트 어셈블리가 없습니다: {PlayModeAssemblyPath}"); // PlayMode 테스트 어셈블리 누락 오류 추가
            }

            string testsScenePath = GameSceneCatalog.GetScenePath(GameSceneId.Tests); // Tests 씬 에셋 경로 조회

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(testsScenePath) == null) // Tests 씬 에셋 존재 여부 확인
            {
                errors.Add($"Tests 씬이 없습니다: {testsScenePath}"); // Tests 씬 누락 오류 추가
            }

            return errors; // 수집된 필수 에셋 오류 목록 반환
        }

        private static Scene OpenTestsScene() // Tests 씬을 에디터에서 단독 열기
        {
            string testsScenePath = GameSceneCatalog.GetScenePath(GameSceneId.Tests); // Tests 씬 에셋 경로 조회
            return EditorSceneManager.OpenScene(testsScenePath, OpenSceneMode.Single); // Tests 씬 단독 열기와 결과 반환
        }

        private static ProjectTestSceneMarker EnsureTestSceneMarker(Scene testsScene) // Tests 씬 루트와 테스트 씬 마커 존재 상태 보장
        {
            GameObject rootObject = testsScene.GetRootGameObjects() // Tests 씬의 모든 루트 게임 오브젝트 조회
                .FirstOrDefault(root => root.name == ProjectTestFramework.SceneRootName); // 고정 이름의 테스트 씬 루트 검색

            if (rootObject == null) // 테스트 씬 루트 존재 여부 확인
            {
                rootObject = new GameObject(ProjectTestFramework.SceneRootName); // 누락된 테스트 씬 루트 게임 오브젝트 생성
                SceneManager.MoveGameObjectToScene(rootObject, testsScene); // 새 테스트 씬 루트를 Tests 씬으로 이동
            }

            ProjectTestSceneMarker marker = rootObject.GetComponent<ProjectTestSceneMarker>(); // 기존 테스트 씬 마커 컴포넌트 조회

            if (marker == null) // 테스트 씬 마커 컴포넌트 존재 여부 확인
            {
                marker = rootObject.AddComponent<ProjectTestSceneMarker>(); // 누락된 테스트 씬 마커 컴포넌트 추가
            }

            marker.Configure(ProjectTestFramework.FrameworkVersion); // 현재 테스트 프레임워크 버전 적용
            return marker; // 구성된 테스트 씬 마커 반환
        }

        private static void EnsureTestsSceneInBuildSettings() // Tests 씬의 Build Settings 등록 상태 보장
        {
            string testsScenePath = GameSceneCatalog.GetScenePath(GameSceneId.Tests); // Tests 씬 에셋 경로 조회
            EditorBuildSettingsScene[] currentScenes = EditorBuildSettings.scenes; // 현재 Build Settings 씬 목록 조회

            if (currentScenes.Any(scene => scene.path == testsScenePath && scene.enabled)) // 활성화된 Tests 씬 등록 여부 확인
            {
                return; // 기존 Build Settings 구성을 유지하고 메서드 종료
            }

            List<EditorBuildSettingsScene> updatedScenes = currentScenes.ToList(); // 수정 가능한 Build Settings 씬 목록 생성
            int existingIndex = updatedScenes.FindIndex(scene => scene.path == testsScenePath); // 비활성 상태를 포함한 Tests 씬 등록 위치 검색

            if (existingIndex >= 0) // 비활성 상태 Tests 씬 등록 여부 확인
            {
                updatedScenes[existingIndex] = new EditorBuildSettingsScene(testsScenePath, true); // 기존 Tests 씬 항목을 활성 상태로 교체
            }
            else // Build Settings에 Tests 씬이 없는 경우 처리
            {
                updatedScenes.Add(new EditorBuildSettingsScene(testsScenePath, true)); // 활성화된 Tests 씬 항목 추가
            }

            EditorBuildSettings.scenes = updatedScenes.ToArray(); // 변경된 Build Settings 씬 목록 적용
        }

        private static List<string> CollectValidationErrors(Scene testsScene) // 현재 테스트 프레임워크 설정 오류 목록 수집
        {
            List<string> errors = CollectRequiredAssetErrors(); // 필수 테스트 에셋 오류 목록으로 검증 시작
            string testsScenePath = GameSceneCatalog.GetScenePath(GameSceneId.Tests); // Tests 씬 경로 조회

            if (!EditorBuildSettings.scenes.Any(scene => scene.path == testsScenePath && scene.enabled)) // Tests 씬의 활성 Build Settings 등록 여부 확인
            {
                errors.Add("Tests 씬이 Build Settings에 활성 상태로 등록되지 않았습니다."); // Tests 씬 Build Settings 누락 오류 추가
            }

            GameObject[] matchingRoots = testsScene.GetRootGameObjects() // Tests 씬의 모든 루트 게임 오브젝트 조회
                .Where(root => root.name == ProjectTestFramework.SceneRootName) // 고정 테스트 씬 루트 이름과 일치하는 오브젝트 선택
                .ToArray(); // 검색 결과 배열 생성

            if (matchingRoots.Length != 1) // 테스트 씬 루트가 정확히 하나인지 확인
            {
                errors.Add($"{ProjectTestFramework.SceneRootName} 루트는 정확히 1개여야 합니다. 현재 개수: {matchingRoots.Length}"); // 테스트 씬 루트 개수 오류 추가
                return errors; // 마커 세부 검사 없이 현재 오류 목록 반환
            }

            ProjectTestSceneMarker marker = matchingRoots[0].GetComponent<ProjectTestSceneMarker>(); // 테스트 씬 루트의 마커 컴포넌트 조회

            if (marker == null) // 테스트 씬 마커 컴포넌트 존재 여부 확인
            {
                errors.Add("ProjectTestSceneMarker 컴포넌트가 없습니다."); // 테스트 씬 마커 누락 오류 추가
                return errors; // 마커 버전 검사 없이 현재 오류 목록 반환
            }

            if (marker.FrameworkVersion != ProjectTestFramework.FrameworkVersion) // 씬 마커 버전과 현재 프레임워크 버전 일치 여부 확인
            {
                errors.Add($"테스트 프레임워크 버전이 다릅니다. 예상: {ProjectTestFramework.FrameworkVersion}, 현재: {marker.FrameworkVersion}"); // 테스트 프레임워크 버전 불일치 오류 추가
            }

            return errors; // 수집된 테스트 프레임워크 오류 목록 반환
        }

        private static void LogErrors(IReadOnlyList<string> errors) // 테스트 프레임워크 검증 오류 전체 출력
        {
            for (int index = 0; index < errors.Count; index++) // 모든 테스트 프레임워크 오류 순회
            {
                ProjectLog.Error(ProjectLogCategory.Test, errors[index], "TEST_FRAMEWORK_INVALID"); // 공통 로그 규칙을 사용하는 테스트 프레임워크 오류 출력
            }
        }
    }
}
