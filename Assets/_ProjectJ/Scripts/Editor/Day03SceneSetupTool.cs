using System.Collections.Generic; // Build Settings 씬 목록 기능 참조
using ProjectJ.Core.SceneFlow; // 프로젝트 씬 흐름 런타임 형식 참조
using UnityEditor; // Unity 에디터 기능 참조
using UnityEditor.SceneManagement; // Unity 에디터 씬 관리 기능 참조
using UnityEngine; // Unity 게임 오브젝트 기능 참조
using UnityEngine.SceneManagement; // Unity 씬 형식 참조

namespace ProjectJ.Editor // 프로젝트 에디터 전용 네임스페이스 선언
{
    internal static class Day03SceneSetupTool // 3일차 씬 구조 자동 생성 도구 선언
    {
        private const string MenuPath = "Project J/Day 03/Create Scene Flow Skeleton"; // Unity 메뉴 경로 선언
        private const string BootstrapObjectName = "ProjectJ_Bootstrap"; // Bootstrap 루트 게임 오브젝트 이름 선언
        private const string DebugObjectName = "ProjectJ_SceneFlowDebug"; // 개발용 씬 이동 패널 게임 오브젝트 이름 선언

        [MenuItem(MenuPath)] // Unity 상단 메뉴에 실행 항목 등록
        private static void CreateSceneFlowSkeleton() // 기본 씬 생성과 Build Settings 등록 실행
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) // 열린 씬 저장 또는 취소 결과 확인
            {
                return; // 사용자가 취소한 경우 작업 중단
            }

            EnsureFolderExists(GameSceneCatalog.SceneFolderPath); // 게임 씬 폴더 존재 상태 보장

            GameSceneId[] buildOrder = GameSceneCatalog.GetBuildOrder(); // 빌드 등록 순서 조회

            foreach (GameSceneId sceneId in buildOrder) // 모든 기본 씬 식별자 순회
            {
                CreateOrRepairScene(sceneId); // 씬 생성 또는 필수 오브젝트 보완
            }

            RegisterScenesInBuildSettings(buildOrder); // 생성된 씬을 빌드 목록에 순서대로 등록
            SetBootstrapAsPlayModeStartScene(); // Play Mode 시작 씬을 Bootstrap으로 설정
            AssetDatabase.SaveAssets(); // 변경된 에셋 저장
            AssetDatabase.Refresh(); // Project 창 에셋 목록 새로고침

            string bootstrapPath = GameSceneCatalog.GetScenePath(GameSceneId.Bootstrap); // Bootstrap 씬 경로 조회
            EditorSceneManager.OpenScene(bootstrapPath, OpenSceneMode.Single); // 작업 완료 후 Bootstrap 씬 열기
            Debug.Log("[Day03] 기본 씬 생성과 Build Settings 등록을 완료했습니다."); // 작업 완료 로그 출력
        }

        private static void CreateOrRepairScene(GameSceneId sceneId) // 씬 생성 또는 필수 오브젝트 보완
        {
            string scenePath = GameSceneCatalog.GetScenePath(sceneId); // 대상 씬 에셋 경로 조회
            SceneAsset existingSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath); // 기존 씬 에셋 조회
            Scene scene; // 생성 또는 열린 씬 저장 변수 선언

            if (existingSceneAsset == null) // 기존 씬이 없는지 확인
            {
                NewSceneSetup setup = sceneId == GameSceneId.Bootstrap // Bootstrap 씬 여부 확인
                    ? NewSceneSetup.EmptyScene // Bootstrap은 빈 씬으로 생성
                    : NewSceneSetup.DefaultGameObjects; // 나머지 씬은 기본 카메라와 조명 포함 생성

                scene = EditorSceneManager.NewScene(setup, NewSceneMode.Single); // 새 씬 생성
            }
            else // 기존 씬이 있는 경우 처리
            {
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single); // 기존 씬 열기
            }

            if (sceneId == GameSceneId.Bootstrap) // Bootstrap 씬인지 확인
            {
                EnsureBootstrapObjects(); // Bootstrap 필수 컴포넌트 구성
            }
            else // 일반 게임 씬인 경우 처리
            {
                EnsureDebugPanelObject(); // 개발용 씬 이동 패널 구성
            }

            EditorSceneManager.MarkSceneDirty(scene); // 씬 변경 상태 표시
            EditorSceneManager.SaveScene(scene, scenePath); // 지정 경로에 씬 저장
        }

        private static void EnsureBootstrapObjects() // Bootstrap 필수 오브젝트와 컴포넌트 구성
        {
            GameObject bootstrapObject = GameObject.Find(BootstrapObjectName); // 기존 Bootstrap 루트 오브젝트 검색

            if (bootstrapObject == null) // Bootstrap 루트 오브젝트가 없는지 확인
            {
                bootstrapObject = new GameObject(BootstrapObjectName); // Bootstrap 루트 오브젝트 생성
            }

            if (bootstrapObject.GetComponent<SceneFlowManager>() == null) // 씬 전환 관리자 컴포넌트 존재 여부 확인
            {
                bootstrapObject.AddComponent<SceneFlowManager>(); // 씬 전환 관리자 컴포넌트 추가
            }

            if (bootstrapObject.GetComponent<BootstrapEntryPoint>() == null) // Bootstrap 진입 컴포넌트 존재 여부 확인
            {
                bootstrapObject.AddComponent<BootstrapEntryPoint>(); // Bootstrap 진입 컴포넌트 추가
            }
        }

        private static void EnsureDebugPanelObject() // 일반 씬 개발용 이동 패널 구성
        {
            GameObject debugObject = GameObject.Find(DebugObjectName); // 기존 개발용 패널 오브젝트 검색

            if (debugObject == null) // 개발용 패널 오브젝트가 없는지 확인
            {
                debugObject = new GameObject(DebugObjectName); // 개발용 패널 오브젝트 생성
            }

            if (debugObject.GetComponent<SceneFlowDebugPanel>() == null) // 개발용 패널 컴포넌트 존재 여부 확인
            {
                debugObject.AddComponent<SceneFlowDebugPanel>(); // 개발용 패널 컴포넌트 추가
            }
        }

        private static void RegisterScenesInBuildSettings(GameSceneId[] buildOrder) // 생성된 씬을 빌드 목록에 등록
        {
            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>(); // 빌드 씬 목록 생성

            foreach (GameSceneId sceneId in buildOrder) // 모든 씬 식별자 순회
            {
                string scenePath = GameSceneCatalog.GetScenePath(sceneId); // 씬 에셋 경로 조회
                buildScenes.Add(new EditorBuildSettingsScene(scenePath, true)); // 활성화된 빌드 씬 항목 추가
            }

            EditorBuildSettings.scenes = buildScenes.ToArray(); // 현재 Build Settings 또는 Build Profile 씬 목록 교체
        }

        private static void SetBootstrapAsPlayModeStartScene() // Play Mode 시작 씬을 Bootstrap으로 설정
        {
            string bootstrapPath = GameSceneCatalog.GetScenePath(GameSceneId.Bootstrap); // Bootstrap 씬 에셋 경로 조회
            SceneAsset bootstrapScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(bootstrapPath); // Bootstrap 씬 에셋 불러오기
            EditorSceneManager.playModeStartScene = bootstrapScene; // Play Mode 시작 씬 지정
        }

        private static void EnsureFolderExists(string folderPath) // 지정된 Unity 에셋 폴더 존재 상태 보장
        {
            if (AssetDatabase.IsValidFolder(folderPath)) // 대상 폴더가 이미 존재하는지 확인
            {
                return; // 폴더 생성 작업 생략
            }

            string[] pathParts = folderPath.Split('/'); // 폴더 경로를 각 단계로 분리
            string currentPath = pathParts[0]; // 첫 번째 Assets 경로 저장

            for (int index = 1; index < pathParts.Length; index++) // 하위 폴더 경로 순회
            {
                string nextPath = $"{currentPath}/{pathParts[index]}"; // 다음 단계 전체 경로 생성

                if (!AssetDatabase.IsValidFolder(nextPath)) // 다음 단계 폴더 존재 여부 확인
                {
                    AssetDatabase.CreateFolder(currentPath, pathParts[index]); // 누락된 하위 폴더 생성
                }

                currentPath = nextPath; // 현재 경로를 다음 단계로 갱신
            }
        }
    }
}
