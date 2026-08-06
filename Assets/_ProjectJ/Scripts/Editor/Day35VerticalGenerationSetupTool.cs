using ProjectJ.MapGeneration; // 맵 생성 Runtime 기능 참조
using UnityEditor; // Unity Editor 에셋과 메뉴 기능 참조
using UnityEngine; // Unity 오브젝트 기능 참조

namespace ProjectJ.Editor // 프로젝트 Editor 기능 네임스페이스 선언
{ // 프로젝트 Editor 기능 묶음
    internal static class Day35VerticalGenerationSetupTool // 35일차 수직 생성 설정 도구 선언
    { // 35일차 수직 생성 설정 도구 묶음
        private const string MenuPath = "Project J/Day 35/Configure Vertical Generation"; // 수직 생성 구성 메뉴 경로
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

        [MenuItem(MenuPath)] // Unity 상단 메뉴에 35일차 구성 항목 등록
        private static void ConfigureVerticalGeneration() // 수직 생성 설정과 후보 목록 적용
        { // 수직 생성 설정 적용 처리
            MapGenerationSettings settings = AssetDatabase.LoadAssetAtPath<MapGenerationSettings>(SettingsAssetPath); // 기본 생성 설정 에셋 조회

            if (settings == null) // 기본 생성 설정 누락 확인
            { // 기본 생성 설정 누락 처리
                Debug.LogError("[ProjectJ][Day35] MAP-GEN-001_DefaultGenerationSettings 에셋이 없습니다. Day 31 메뉴를 먼저 실행하세요."); // 선행 설정 안내 오류 출력
                return; // 수직 생성 설정 중단
            } // 기본 생성 설정 누락 처리 종료

            MapModuleDefinition[] modulePrefabs = LoadModulePrefabs(); // 기본 모듈 Prefab 8종 조회

            if (modulePrefabs == null) // 모듈 Prefab 누락 확인
            { // 모듈 Prefab 누락 처리
                return; // 수직 생성 설정 중단
            } // 모듈 Prefab 누락 처리 종료

            if (!ValidateVerticalPrefabs(modulePrefabs)) // 상승 모듈 수직 데이터 확인
            { // 상승 모듈 수직 데이터 오류 처리
                return; // 수직 생성 설정 중단
            } // 상승 모듈 수직 데이터 오류 처리 종료

            settings.ConfigureVerticalForEditor(35001, false, 0, 8, 128, 0.05f, 0.05f, 0.02f, 8f, 16f, 3, 2, false, modulePrefabs); // 35일차 권장 수직 생성값 적용
            EditorUtility.SetDirty(settings); // 생성 설정 변경 상태 표시
            AssetDatabase.SaveAssets(); // 변경된 생성 설정 저장
            AssetDatabase.Refresh(); // Project 창 에셋 목록 새로고침
            Selection.activeObject = settings; // 갱신된 생성 설정 선택
            EditorGUIUtility.PingObject(settings); // Project 창에서 생성 설정 강조
            Debug.Log("[ProjectJ][Day35] XYZ 연결과 목표 높이 기반 수직 생성 설정을 완료했습니다. Seed 35001, 모듈 8개, 목표 8~16m, 최소 상승 모듈 3개, 최대 연속 평지 2개를 적용했습니다."); // 수직 생성 설정 완료 로그 출력
        } // 수직 생성 설정 적용 처리 종료

        [MenuItem(MenuPath, true)] // 35일차 구성 메뉴 활성 조건 등록
        private static bool ValidateConfigureVerticalGeneration() // Play Mode가 아닐 때만 메뉴 실행 허용
        { // 메뉴 실행 조건 검사 처리
            return !EditorApplication.isPlayingOrWillChangePlaymode; // Edit Mode 실행 가능 여부 반환
        } // 메뉴 실행 조건 검사 종료

        private static MapModuleDefinition[] LoadModulePrefabs() // 기본 모듈 Prefab 정의 8종 조회
        { // 기본 모듈 Prefab 조회 처리
            MapModuleDefinition[] definitions = new MapModuleDefinition[ModulePrefabPaths.Length]; // 모듈 정의 결과 배열 생성

            for (int pathIndex = 0; pathIndex < ModulePrefabPaths.Length; pathIndex++) // 모든 Prefab 경로 순회
            { // 단일 Prefab 조회 처리
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ModulePrefabPaths[pathIndex]); // 현재 경로 Prefab 조회

                if (prefab == null) // 현재 Prefab 누락 확인
                { // 현재 Prefab 누락 처리
                    Debug.LogError($"[ProjectJ][Day35] 필요한 Prefab이 없습니다: {ModulePrefabPaths[pathIndex]}"); // 누락 Prefab 경로 출력
                    return null; // 모듈 Prefab 조회 실패 반환
                } // 현재 Prefab 누락 처리 종료

                MapModuleDefinition definition = prefab.GetComponent<MapModuleDefinition>(); // 현재 Prefab 모듈 정의 조회

                if (definition == null) // 모듈 정의 누락 확인
                { // 모듈 정의 누락 처리
                    Debug.LogError($"[ProjectJ][Day35] MapModuleDefinition이 없습니다: {ModulePrefabPaths[pathIndex]}", prefab); // 모듈 정의 누락 오류 출력
                    return null; // 모듈 Prefab 조회 실패 반환
                } // 모듈 정의 누락 처리 종료

                definitions[pathIndex] = definition; // 현재 모듈 정의 결과 저장
            } // 단일 Prefab 조회 처리 종료

            return definitions; // 완성된 모듈 정의 배열 반환
        } // 기본 모듈 Prefab 조회 처리 종료

        private static bool ValidateVerticalPrefabs(MapModuleDefinition[] modulePrefabs) // 상승 모듈 3종 수직 데이터 검사
        { // 상승 모듈 수직 데이터 검사 처리
            for (int moduleIndex = 5; moduleIndex < modulePrefabs.Length; moduleIndex++) // MAP-006부터 MAP-008까지 순회
            { // 단일 상승 모듈 검사 처리
                MapVerticalModuleData verticalData = modulePrefabs[moduleIndex].GetComponent<MapVerticalModuleData>(); // 현재 상승 모듈 수직 데이터 조회

                if (verticalData == null) // 수직 데이터 누락 확인
                { // 수직 데이터 누락 처리
                    Debug.LogError($"[ProjectJ][Day35] {modulePrefabs[moduleIndex].name}에 MapVerticalModuleData가 없습니다. Day 34 메뉴를 다시 실행하세요.", modulePrefabs[moduleIndex]); // 수직 데이터 누락 오류 출력
                    return false; // 상승 모듈 검사 실패 반환
                } // 수직 데이터 누락 처리 종료

                if (!verticalData.TryValidate(out string reason)) // 수직 데이터 유효성 확인
                { // 잘못된 수직 데이터 처리
                    Debug.LogError($"[ProjectJ][Day35] {modulePrefabs[moduleIndex].name} 수직 데이터 오류: {reason}", modulePrefabs[moduleIndex]); // 수직 데이터 오류 출력
                    return false; // 상승 모듈 검사 실패 반환
                } // 잘못된 수직 데이터 처리 종료
            } // 단일 상승 모듈 검사 처리 종료

            return true; // 상승 모듈 검사 성공 반환
        } // 상승 모듈 수직 데이터 검사 처리 종료
    } // 35일차 수직 생성 설정 도구 묶음 종료
} // 프로젝트 Editor 기능 묶음 종료
