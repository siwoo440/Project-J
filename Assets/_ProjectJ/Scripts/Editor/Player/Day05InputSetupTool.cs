using ProjectJ.Core.SceneFlow; // Tests와 Bootstrap 씬 경로 관리 형식 참조
using ProjectJ.Input; // 프로젝트 입력 이름과 디버그 컴포넌트 참조
using UnityEditor; // Unity 에디터 메뉴와 에셋 기능 참조
using UnityEditor.SceneManagement; // Unity 에디터 씬 열기와 Play Mode 시작 씬 기능 참조
using UnityEngine; // Unity 게임 오브젝트와 로그 기능 참조
using UnityEngine.InputSystem; // Unity Input System 액션 에셋 기능 참조
using UnityEngine.SceneManagement; // Unity 씬 열기 모드 기능 참조

namespace ProjectJ.Editor // 프로젝트 에디터 전용 네임스페이스 선언
{
    internal static class Day05InputSetupTool // 5일차 입력 검증 씬 자동 구성 도구 선언
    {
        private const string ConfigureMenuPath = ProjectJEditorMenuPaths.PlayerInput + "/입력 디버그 구성 (Day 05일차)"; // 입력 검증 구성 메뉴 경로 선언
        private const string UseTestsStartMenuPath = ProjectJEditorMenuPaths.PlayerPlayMode + "/Tests 씬을 Play Mode 시작 씬으로 설정 (Day 05일차)"; // Tests Play Mode 시작 설정 메뉴 경로 선언
        private const string RestoreBootstrapMenuPath = ProjectJEditorMenuPaths.PlayerPlayMode + "/Bootstrap 씬을 Play Mode 시작 씬으로 복원 (Day 05일차)"; // Bootstrap Play Mode 시작 복원 메뉴 경로 선언
        private const string DebugObjectName = "ProjectJ_InputDebug"; // 입력 검증 게임 오브젝트 이름 선언

        [MenuItem(ConfigureMenuPath)] // Unity 상단 메뉴에 입력 검증 구성 항목 등록
        private static void ConfigureInputDebug() // 입력 에셋 검사와 Tests 씬 디버그 오브젝트 구성
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) // 열린 씬 저장 또는 취소 결과 확인
            {
                return; // 사용자가 취소한 경우 입력 구성 작업 중단
            }

            InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ProjectInputNames.AssetPath); // 프로젝트 입력 액션 에셋 불러오기

            if (inputActions == null) // 프로젝트 입력 액션 에셋 존재 여부 확인
            {
                Debug.LogError($"[Day05] 입력 액션 에셋을 찾을 수 없습니다: {ProjectInputNames.AssetPath}"); // 입력 에셋 누락 오류 출력
                return; // 입력 구성 작업 중단
            }

            if (!ValidateInputAsset(inputActions)) // 필수 액션 맵과 액션 구성 검사
            {
                return; // 입력 에셋 구성이 올바르지 않으면 작업 중단
            }

            string testsScenePath = GameSceneCatalog.GetScenePath(GameSceneId.Tests); // Tests 씬 에셋 경로 조회
            SceneAsset testsSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(testsScenePath); // Tests 씬 에셋 불러오기

            if (testsSceneAsset == null) // Tests 씬 에셋 존재 여부 확인
            {
                Debug.LogError($"[Day05] Tests 씬을 찾을 수 없습니다: {testsScenePath}"); // Tests 씬 누락 오류 출력
                return; // 입력 구성 작업 중단
            }

            Scene testsScene = EditorSceneManager.OpenScene(testsScenePath, OpenSceneMode.Single); // Tests 씬 단독 열기
            GameObject debugObject = GameObject.Find(DebugObjectName); // 기존 입력 검증 게임 오브젝트 검색

            if (debugObject == null) // 입력 검증 게임 오브젝트 존재 여부 확인
            {
                debugObject = new GameObject(DebugObjectName); // 누락된 입력 검증 게임 오브젝트 생성
            }

            InputActionDebugMonitor monitor = debugObject.GetComponent<InputActionDebugMonitor>(); // 기존 입력 액션 모니터 컴포넌트 조회

            if (monitor == null) // 입력 액션 모니터 컴포넌트 존재 여부 확인
            {
                monitor = debugObject.AddComponent<InputActionDebugMonitor>(); // 누락된 입력 액션 모니터 컴포넌트 추가
            }

            monitor.Configure(inputActions, InputDebugMap.Gameplay); // 입력 에셋과 기본 Gameplay 검증 맵 설정
            EditorUtility.SetDirty(monitor); // 입력 액션 모니터 직렬화 변경 상태 표시
            EditorSceneManager.MarkSceneDirty(testsScene); // Tests 씬 변경 상태 표시
            EditorSceneManager.SaveScene(testsScene, testsScenePath); // 변경된 Tests 씬 저장
            Selection.activeGameObject = debugObject; // 구성 완료 후 입력 검증 오브젝트 선택
            AssetDatabase.SaveAssets(); // 변경된 에셋 저장
            Debug.Log("[Day05] Input Actions 검사와 Tests 씬 입력 검증 구성을 완료했습니다."); // 입력 구성 완료 로그 출력
        }

        [MenuItem(UseTestsStartMenuPath)] // Tests 씬을 Play Mode 시작 씬으로 설정하는 메뉴 등록
        private static void UseTestsAsPlayModeStartScene() // 입력 수동 검증을 위한 Tests 시작 씬 설정
        {
            SetPlayModeStartScene(GameSceneId.Tests, "[Day05] Play Mode 시작 씬을 Tests로 설정했습니다."); // Tests 씬을 Play Mode 시작 씬으로 지정
        }

        [MenuItem(RestoreBootstrapMenuPath)] // Bootstrap 씬을 Play Mode 시작 씬으로 복원하는 메뉴 등록
        private static void RestoreBootstrapAsPlayModeStartScene() // 입력 검증 종료 후 Bootstrap 시작 씬 복원
        {
            SetPlayModeStartScene(GameSceneId.Bootstrap, "[Day05] Play Mode 시작 씬을 Bootstrap으로 복원했습니다."); // Bootstrap 씬을 Play Mode 시작 씬으로 지정
        }

        [MenuItem(ConfigureMenuPath, true)] // 입력 검증 구성 메뉴 활성 조건 등록
        [MenuItem(UseTestsStartMenuPath, true)] // Tests 시작 설정 메뉴 활성 조건 등록
        [MenuItem(RestoreBootstrapMenuPath, true)] // Bootstrap 복원 메뉴 활성 조건 등록
        private static bool ValidateEditorMenu() // Play Mode가 아닐 때만 5일차 메뉴 실행 허용
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode; // Play Mode 진입 또는 실행 중이 아닌 경우 활성화
        }

        private static void SetPlayModeStartScene(GameSceneId sceneId, string successMessage) // 지정 씬을 Play Mode 시작 씬으로 설정
        {
            string scenePath = GameSceneCatalog.GetScenePath(sceneId); // 지정 씬 에셋 경로 조회
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath); // 지정 씬 에셋 불러오기

            if (sceneAsset == null) // 지정 씬 에셋 존재 여부 확인
            {
                Debug.LogError($"[Day05] Play Mode 시작 씬을 찾을 수 없습니다: {scenePath}"); // 시작 씬 누락 오류 출력
                return; // Play Mode 시작 씬 설정 중단
            }

            EditorSceneManager.playModeStartScene = sceneAsset; // 지정 씬을 Play Mode 시작 씬으로 설정
            Debug.Log(successMessage); // Play Mode 시작 씬 설정 완료 로그 출력
        }

        private static bool ValidateInputAsset(InputActionAsset inputActions) // 필수 액션 맵과 액션 존재 여부 검사
        {
            string[] gameplayActions = // Gameplay 필수 액션 이름 배열 선언
            {
                ProjectInputNames.Gameplay.Move, // 이동 액션 이름 추가
                ProjectInputNames.Gameplay.Look, // 시점 액션 이름 추가
                ProjectInputNames.Gameplay.Jump, // 점프 액션 이름 추가
                ProjectInputNames.Gameplay.Sprint, // 달리기 액션 이름 추가
                ProjectInputNames.Gameplay.Crouch, // 앉기 액션 이름 추가
                ProjectInputNames.Gameplay.Push, // 밀치기 액션 이름 추가
                ProjectInputNames.Gameplay.UseItem, // 아이템 사용 액션 이름 추가
                ProjectInputNames.Gameplay.SelectPreviousItem, // 이전 아이템 선택 액션 이름 추가
                ProjectInputNames.Gameplay.SelectNextItem, // 다음 아이템 선택 액션 이름 추가
                ProjectInputNames.Gameplay.ShowItem, // 아이템 보여주기 액션 이름 추가
                ProjectInputNames.Gameplay.DropItem, // 아이템 버리기 액션 이름 추가
                ProjectInputNames.Gameplay.Interact, // 상호작용 액션 이름 추가
                ProjectInputNames.Gameplay.Scoreboard, // 순위표 액션 이름 추가
                ProjectInputNames.Gameplay.Pause // 일시정지 액션 이름 추가
            };

            string[] uiActions = // UI 필수 액션 이름 배열 선언
            {
                ProjectInputNames.UI.Navigate, // UI 이동 액션 이름 추가
                ProjectInputNames.UI.Submit, // UI 확인 액션 이름 추가
                ProjectInputNames.UI.Cancel, // UI 취소 액션 이름 추가
                ProjectInputNames.UI.Point, // UI 포인터 액션 이름 추가
                ProjectInputNames.UI.Click, // UI 클릭 액션 이름 추가
                ProjectInputNames.UI.ScrollWheel // UI 스크롤 액션 이름 추가
            };

            return ValidateActionMap(inputActions, ProjectInputNames.Gameplay.Map, gameplayActions) // Gameplay 액션 맵 검사 결과 확인
                && ValidateActionMap(inputActions, ProjectInputNames.UI.Map, uiActions); // UI 액션 맵 검사 결과 확인
        }

        private static bool ValidateActionMap(InputActionAsset inputActions, string mapName, string[] requiredActions) // 지정 액션 맵의 필수 액션 존재 여부 검사
        {
            InputActionMap actionMap = inputActions.FindActionMap(mapName, false); // 입력 에셋에서 지정 액션 맵 검색

            if (actionMap == null) // 지정 액션 맵 존재 여부 확인
            {
                Debug.LogError($"[Day05] {mapName} 액션 맵이 없습니다."); // 액션 맵 누락 오류 출력
                return false; // 액션 맵 검사 실패 반환
            }

            foreach (string actionName in requiredActions) // 모든 필수 액션 이름 순회
            {
                if (actionMap.FindAction(actionName, false) != null) // 현재 필수 액션 존재 여부 확인
                {
                    continue; // 존재하는 액션은 다음 액션 검사로 이동
                }

                Debug.LogError($"[Day05] {mapName}/{actionName} 액션이 없습니다."); // 필수 액션 누락 오류 출력
                return false; // 액션 맵 검사 실패 반환
            }

            return true; // 액션 맵과 모든 필수 액션 검사 성공 반환
        }
    }
}
