using ProjectJ.MapGeneration; // 맵 생성 Runtime 기능 참조
using UnityEditor; // Unity Editor 에셋과 메뉴 기능 참조
using UnityEngine; // Unity 오브젝트 기능 참조

namespace ProjectJ.Editor // 프로젝트 Editor 기능 네임스페이스 선언
{ // 프로젝트 Editor 기능 묶음
    internal static class Day36VerticalBranchGenerationSetupTool // 36일차 수직 분기 설정 도구 선언
    { // 36일차 수직 분기 설정 도구 묶음
        private const string MenuPath = ProjectJEditorMenuPaths.MapGeneration + "/수직 분기 맵 생성 구성 (Day 36일차)"; // 수직 분기 구성 메뉴 경로
        private const string SettingsAssetPath = "Assets/_ProjectJ/Data/Definitions/Map/MAP-GEN-001_DefaultGenerationSettings.asset"; // 기본 생성 설정 에셋 경로

        private static readonly string[] ModulePrefabPaths = // 기본 생성 후보 Prefab 경로 목록 선언
        { // 기본 생성 후보 Prefab 경로 묶음
            "Assets/_ProjectJ/Prefabs/Map/Modules/MAP-001_FixedStraight.prefab", // 고정 직선 모듈 경로
            "Assets/_ProjectJ/Prefabs/Map/Modules/MAP-002_LowPassage.prefab", // 낮은 통로 모듈 경로
            "Assets/_ProjectJ/Prefabs/Map/Modules/MAP-003_JumpGap.prefab", // 점프 간격 모듈 경로
            "Assets/_ProjectJ/Prefabs/Map/Modules/MAP-004_Branch.prefab", // 분기 모듈 경로
            "Assets/_ProjectJ/Prefabs/Map/Modules/MAP-005_Merge.prefab", // 합류 모듈 경로
            "Assets/_ProjectJ/Prefabs/Map/Modules/MAP-006_StepRise.prefab", // 계단 상승 모듈 경로
            "Assets/_ProjectJ/Prefabs/Map/Modules/MAP-007_ZigzagRise.prefab", // 지그재그 상승 모듈 경로
            "Assets/_ProjectJ/Prefabs/Map/Modules/MAP-008_JumpRise.prefab" // 점프 상승 모듈 경로
        }; // 기본 생성 후보 Prefab 경로 묶음 종료

        [MenuItem(MenuPath)] // Unity 상단 메뉴에 36일차 구성 항목 등록
        private static void ConfigureVerticalBranchGeneration() // 수직 분기 생성 설정과 후보 목록 적용
        { // 수직 분기 설정 적용 처리
            MapGenerationSettings settings = AssetDatabase.LoadAssetAtPath<MapGenerationSettings>(SettingsAssetPath); // 기본 생성 설정 에셋 조회

            if (settings == null) // 기본 생성 설정 누락 확인
            { // 기본 생성 설정 누락 처리
                Debug.LogError("[ProjectJ][Day36] MAP-GEN-001_DefaultGenerationSettings 에셋이 없습니다. Day 31 메뉴를 먼저 실행하세요."); // 선행 설정 안내 오류 출력
                return; // 수직 분기 설정 중단
            } // 기본 생성 설정 누락 처리 종료

            MapModuleDefinition[] modulePrefabs = LoadModulePrefabs(); // 기본 모듈 Prefab 8종 조회

            if (modulePrefabs == null) // 모듈 Prefab 누락 확인
            { // 모듈 Prefab 누락 처리
                return; // 수직 분기 설정 중단
            } // 모듈 Prefab 누락 처리 종료

            if (!ValidateRequiredPrefabs(modulePrefabs)) // 분기·합류·상승 Prefab 데이터 확인
            { // 필수 Prefab 데이터 오류 처리
                return; // 수직 분기 설정 중단
            } // 필수 Prefab 데이터 오류 처리 종료

            settings.ConfigureVerticalBranchingForEditor(36001, false, 0, 8, 128, 0.05f, 0.05f, 0.02f, 2, 8f, 16f, 3, 2, false, 1, 0.02f, 64, modulePrefabs); // 36일차 권장 수직 분기 생성값 적용
            EditorUtility.SetDirty(settings); // 생성 설정 변경 상태 표시
            AssetDatabase.SaveAssets(); // 변경된 생성 설정 저장
            AssetDatabase.Refresh(); // Project 창 에셋 목록 새로고침
            Selection.activeObject = settings; // 갱신된 생성 설정 선택
            EditorGUIUtility.PingObject(settings); // Project 창에서 생성 설정 강조
            Debug.Log("[ProjectJ][Day36] 수직 분기·합류 생성 설정을 완료했습니다. Seed 36001, 모듈 8개, 분기 단계 2개, 분기별 최소 상승 1개, 합류 높이 오차 0.02m, 조합 재시도 64회를 적용했습니다."); // 수직 분기 설정 완료 로그 출력
        } // 수직 분기 설정 적용 처리 종료

        [MenuItem(MenuPath, true)] // 36일차 구성 메뉴 활성 조건 등록
        private static bool ValidateConfigureVerticalBranchGeneration() // Play Mode가 아닐 때만 메뉴 실행 허용
        { // 메뉴 실행 조건 검사 처리
            return !EditorApplication.isPlayingOrWillChangePlaymode; // Edit Mode 실행 가능 여부 반환
        } // 메뉴 실행 조건 검사 처리 종료

        private static MapModuleDefinition[] LoadModulePrefabs() // 기본 모듈 Prefab 정의 8종 조회
        { // 기본 모듈 Prefab 조회 처리
            MapModuleDefinition[] definitions = new MapModuleDefinition[ModulePrefabPaths.Length]; // 모듈 정의 결과 배열 생성

            for (int pathIndex = 0; pathIndex < ModulePrefabPaths.Length; pathIndex++) // 모든 Prefab 경로 순회
            { // 단일 Prefab 조회 처리
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ModulePrefabPaths[pathIndex]); // 현재 경로 Prefab 조회

                if (prefab == null) // 현재 Prefab 누락 확인
                { // 현재 Prefab 누락 처리
                    Debug.LogError($"[ProjectJ][Day36] 필요한 Prefab이 없습니다: {ModulePrefabPaths[pathIndex]}"); // 누락 Prefab 경로 출력
                    return null; // 모듈 Prefab 조회 실패 반환
                } // 현재 Prefab 누락 처리 종료

                MapModuleDefinition definition = prefab.GetComponent<MapModuleDefinition>(); // 현재 Prefab 모듈 정의 조회

                if (definition == null) // 모듈 정의 누락 확인
                { // 모듈 정의 누락 처리
                    Debug.LogError($"[ProjectJ][Day36] MapModuleDefinition이 없습니다: {ModulePrefabPaths[pathIndex]}", prefab); // 모듈 정의 누락 오류 출력
                    return null; // 모듈 Prefab 조회 실패 반환
                } // 모듈 정의 누락 처리 종료

                definitions[pathIndex] = definition; // 현재 모듈 정의 결과 저장
            } // 단일 Prefab 조회 처리 종료

            return definitions; // 완성된 모듈 정의 배열 반환
        } // 기본 모듈 Prefab 조회 처리 종료

        private static bool ValidateRequiredPrefabs(MapModuleDefinition[] modulePrefabs) // 분기·합류·상승 Prefab 필수 데이터 검사
        { // 필수 Prefab 데이터 검사 처리
            if (modulePrefabs[3].ModuleKind != MapModuleKind.Branch || modulePrefabs[4].ModuleKind != MapModuleKind.Merge) // 분기와 합류 종류 확인
            { // 분기 또는 합류 종류 오류 처리
                Debug.LogError("[ProjectJ][Day36] MAP-004는 Branch, MAP-005는 Merge 종류여야 합니다."); // 특수 모듈 종류 오류 출력
                return false; // 필수 Prefab 검사 실패 반환
            } // 분기 또는 합류 종류 오류 처리 종료

            for (int moduleIndex = 5; moduleIndex < modulePrefabs.Length; moduleIndex++) // MAP-006부터 MAP-008까지 순회
            { // 단일 상승 모듈 검사 처리
                MapVerticalModuleData verticalData = modulePrefabs[moduleIndex].GetComponent<MapVerticalModuleData>(); // 현재 상승 모듈 수직 데이터 조회

                if (verticalData == null) // 수직 데이터 누락 확인
                { // 수직 데이터 누락 처리
                    Debug.LogError($"[ProjectJ][Day36] {modulePrefabs[moduleIndex].name}에 MapVerticalModuleData가 없습니다.", modulePrefabs[moduleIndex]); // 수직 데이터 누락 오류 출력
                    return false; // 필수 Prefab 검사 실패 반환
                } // 수직 데이터 누락 처리 종료

                if (!verticalData.TryValidate(out string reason)) // 수직 데이터 유효성 확인
                { // 수직 데이터 오류 처리
                    Debug.LogError($"[ProjectJ][Day36] {modulePrefabs[moduleIndex].name} 수직 데이터 오류: {reason}", modulePrefabs[moduleIndex]); // 수직 데이터 오류 출력
                    return false; // 필수 Prefab 검사 실패 반환
                } // 수직 데이터 오류 처리 종료
            } // 단일 상승 모듈 검사 처리 종료

            return true; // 필수 Prefab 검사 성공 반환
        } // 필수 Prefab 데이터 검사 처리 종료
    } // 36일차 수직 분기 설정 도구 묶음 종료
} // 프로젝트 Editor 기능 묶음 종료
