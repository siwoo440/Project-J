using ProjectJ.MapGeneration; // 맵 생성 Runtime 기능 참조
using UnityEditor; // Unity Editor 메뉴와 Undo 기능 참조
using UnityEditor.SceneManagement; // Scene 변경 상태 기능 참조
using UnityEngine; // Unity 오브젝트 검색과 로그 기능 참조

namespace ProjectJ.Editor // 프로젝트 Editor 기능 네임스페이스 선언
{ // 프로젝트 Editor 기능 묶음
    internal static class Day37MapPlayabilitySetupTool // 37일차 플레이 가능성 검사 설정 도구 선언
    { // 37일차 플레이 가능성 검사 설정 도구 묶음
        private const string MenuPath = ProjectJEditorMenuPaths.MapValidation + "/플레이 가능 경로 검증 구성 (Day 37일차)"; // 플레이 가능성 검사 구성 메뉴 경로

        [MenuItem(MenuPath)] // Unity 상단 메뉴에 37일차 구성 항목 등록
        private static void ConfigurePlayabilityValidation() // 생성기에 경로 검사 시각화 연결
        { // 플레이 가능성 검사 설정 처리
            ProceduralMapGenerator generator = Object.FindFirstObjectByType<ProceduralMapGenerator>(); // 현재 Scene의 절차적 맵 생성기 조회

            if (generator == null) // 절차적 맵 생성기 누락 확인
            { // 절차적 맵 생성기 누락 처리
                Debug.LogError("[ProjectJ][Day37] 현재 Scene에서 ProceduralMapGenerator를 찾지 못했습니다. Game Scene과 31일차 생성기 오브젝트를 확인하세요."); // 생성기 누락 안내 오류 출력
                return; // 플레이 가능성 검사 설정 중단
            } // 절차적 맵 생성기 누락 처리 종료

            MapGenerationDebugVisualizer visualizer = generator.GetComponent<MapGenerationDebugVisualizer>(); // 기존 경로 디버그 시각화 조회

            if (visualizer == null) // 경로 디버그 시각화 누락 확인
            { // 경로 디버그 시각화 추가 처리
                visualizer = Undo.AddComponent<MapGenerationDebugVisualizer>(generator.gameObject); // Undo 가능한 디버그 시각화 컴포넌트 추가
            } // 경로 디버그 시각화 추가 처리 종료

            Undo.RecordObject(visualizer, "Configure Day 37 Map Playability Visualizer"); // 시각화 설정 변경 Undo 기록
            visualizer.ConfigureForEditor(generator, true, true, 0.3f, 0.35f); // 선택 중 표시와 노드 문자 기본값 적용
            EditorUtility.SetDirty(visualizer); // 시각화 컴포넌트 변경 상태 표시
            EditorSceneManager.MarkSceneDirty(generator.gameObject.scene); // 현재 Scene 저장 필요 상태 표시
            Selection.activeGameObject = generator.gameObject; // 생성기 오브젝트 선택
            EditorGUIUtility.PingObject(generator.gameObject); // Hierarchy에서 생성기 오브젝트 강조

            if (generator.GeneratedModuleCount > 0) // 기존 생성 결과 존재 확인
            { // 기존 생성 결과 검사 처리
                generator.ValidatePlayableRoutes(); // 현재 맵의 시작부터 종료 경로 즉시 검사
            } // 기존 생성 결과 검사 처리 종료

            Debug.Log("[ProjectJ][Day37] 플레이 가능 경로 검사와 Scene 디버그 시각화를 구성했습니다. 생성기 오브젝트를 선택하면 정상 경로는 청록색, 실패 위치는 빨간색으로 표시됩니다.", generator); // 37일차 설정 완료 로그 출력
        } // 플레이 가능성 검사 설정 처리 종료

        [MenuItem(MenuPath, true)] // 37일차 구성 메뉴 활성 조건 등록
        private static bool ValidateConfigurePlayabilityValidation() // Play Mode가 아닐 때만 메뉴 실행 허용
        { // 메뉴 실행 조건 검사 처리
            return !EditorApplication.isPlayingOrWillChangePlaymode; // Edit Mode 실행 가능 여부 반환
        } // 메뉴 실행 조건 검사 처리 종료
    } // 37일차 플레이 가능성 검사 설정 도구 묶음 종료
} // 프로젝트 Editor 기능 묶음 종료
