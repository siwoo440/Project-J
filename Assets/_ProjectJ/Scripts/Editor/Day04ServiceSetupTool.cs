using ProjectJ.Core.SceneFlow; // Bootstrap 씬 경로와 필수 컴포넌트 형식 참조
using ProjectJ.Core.Services; // 공통 서비스 초기화 컴포넌트 형식 참조
using UnityEditor; // Unity 에디터 메뉴와 에셋 기능 참조
using UnityEditor.SceneManagement; // Unity 에디터 씬 열기와 저장 기능 참조
using UnityEngine; // Unity 게임 오브젝트와 로그 기능 참조
using UnityEngine.SceneManagement; // Unity 씬 열기 모드 기능 참조

namespace ProjectJ.Editor // 프로젝트 에디터 전용 네임스페이스 선언
{
    internal static class Day04ServiceSetupTool // 4일차 공통 서비스 Bootstrap 구성 도구 선언
    {
        private const string MenuPath = ProjectJEditorMenuPaths.ProjectSettingsServices + "/공통 서비스 구성 (Day 04일차)"; // Unity 상단 메뉴 경로 선언
        private const string BootstrapObjectName = "ProjectJ_Bootstrap"; // Bootstrap 루트 게임 오브젝트 이름 선언

        [MenuItem(MenuPath)] // Unity 상단 메뉴에 공통 서비스 구성 항목 등록
        private static void ConfigureCommonServices() // Bootstrap 씬에 공통 서비스 초기화 구성 적용
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) // 열린 씬 저장 또는 취소 결과 확인
            {
                return; // 사용자가 취소한 경우 작업 중단
            }

            string bootstrapScenePath = GameSceneCatalog.GetScenePath(GameSceneId.Bootstrap); // Bootstrap 씬 에셋 경로 조회
            SceneAsset bootstrapSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(bootstrapScenePath); // Bootstrap 씬 에셋 조회

            if (bootstrapSceneAsset == null) // Bootstrap 씬 에셋 존재 여부 확인
            {
                Debug.LogError($"[Day04] Bootstrap 씬을 찾을 수 없습니다: {bootstrapScenePath}"); // Bootstrap 씬 누락 오류 출력
                return; // 공통 서비스 구성 작업 중단
            }

            Scene bootstrapScene = EditorSceneManager.OpenScene(bootstrapScenePath, OpenSceneMode.Single); // Bootstrap 씬 단독 열기
            GameObject bootstrapObject = GameObject.Find(BootstrapObjectName); // Bootstrap 루트 게임 오브젝트 검색

            if (bootstrapObject == null) // Bootstrap 루트 게임 오브젝트 존재 여부 확인
            {
                bootstrapObject = new GameObject(BootstrapObjectName); // 누락된 Bootstrap 루트 게임 오브젝트 생성
            }

            EnsureComponent<SceneFlowManager>(bootstrapObject); // 씬 전환 관리자 컴포넌트 존재 상태 보장
            EnsureComponent<CommonServiceInitializer>(bootstrapObject); // 공통 서비스 초기화 컴포넌트 존재 상태 보장
            EnsureComponent<BootstrapEntryPoint>(bootstrapObject); // Bootstrap 진입 컴포넌트 존재 상태 보장
            EditorSceneManager.MarkSceneDirty(bootstrapScene); // Bootstrap 씬 변경 상태 표시
            EditorSceneManager.SaveScene(bootstrapScene, bootstrapScenePath); // 변경된 Bootstrap 씬 저장
            EditorSceneManager.playModeStartScene = bootstrapSceneAsset; // Play Mode 시작 씬을 Bootstrap으로 유지
            AssetDatabase.SaveAssets(); // 변경된 에셋과 에디터 설정 저장
            Debug.Log("[Day04] Bootstrap 공통 서비스 초기화 구성을 완료했습니다."); // 공통 서비스 구성 완료 로그 출력
        }

        [MenuItem(MenuPath, true)] // 공통 서비스 구성 메뉴 활성 조건 등록
        private static bool ValidateConfigureCommonServices() // Play Mode가 아닐 때만 메뉴 실행 허용
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode; // Play Mode 진입 또는 실행 중이 아닌 경우 활성화
        }

        private static void EnsureComponent<T>(GameObject targetObject) where T : Component // 지정 게임 오브젝트의 필수 컴포넌트 존재 상태 보장
        {
            if (targetObject.GetComponent<T>() != null) // 지정 컴포넌트가 이미 존재하는지 확인
            {
                return; // 중복 컴포넌트 추가 없이 메서드 종료
            }

            targetObject.AddComponent<T>(); // 누락된 지정 컴포넌트 추가
        }
    }
}
