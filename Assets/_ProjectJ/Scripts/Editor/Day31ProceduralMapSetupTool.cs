using ProjectJ.MapGeneration; // 맵 생성 Runtime 기능 참조
using UnityEditor; // Unity Editor 에셋과 메뉴 기능 참조
using UnityEditor.SceneManagement; // Unity Scene 저장 상태 기능 참조
using UnityEngine; // Unity 오브젝트 기능 참조

namespace ProjectJ.Editor // 프로젝트 Editor 기능 네임스페이스 선언
{ // 프로젝트 Editor 기능 묶음
    internal static class Day31ProceduralMapSetupTool // 31일차 맵 생성기 구성 도구 선언
    { // 31일차 맵 생성기 구성 도구 묶음
        private const string MenuPath = "Project J/Day 31/Create Or Update Procedural Map Generator"; // 맵 생성기 구성 메뉴 경로
        private const string SettingsAssetPath = "Assets/_ProjectJ/Data/Definitions/Map/MAP-GEN-001_DefaultGenerationSettings.asset"; // 기본 생성 설정 에셋 경로
        private const string FixedPrefabPath = "Assets/_ProjectJ/Prefabs/Map/Modules/MAP-001_FixedStraight.prefab"; // 고정 발판 Prefab 경로
        private const string LowPassagePrefabPath = "Assets/_ProjectJ/Prefabs/Map/Modules/MAP-002_LowPassage.prefab"; // 낮은 통로 Prefab 경로
        private const string JumpGapPrefabPath = "Assets/_ProjectJ/Prefabs/Map/Modules/MAP-003_JumpGap.prefab"; // 점프 간격 Prefab 경로

        [MenuItem(MenuPath)] // Unity 상단 메뉴에 31일차 구성 항목 등록
        private static void CreateOrUpdateProceduralMapGenerator() // 생성 설정과 Scene 생성기 구성
        { // 생성 설정과 Scene 생성기 구성 처리
            MapModuleDefinition[] modulePrefabs = LoadModulePrefabs(); // 기본 맵 모듈 Prefab 세 개 조회

            if (ContainsMissingPrefab(modulePrefabs)) // 기본 모듈 Prefab 누락 확인
            { // 기본 모듈 Prefab 누락 처리
                Debug.LogError("[ProjectJ][Day31] 30일차 기본 맵 모듈 Prefab이 없습니다. Day 30 생성 메뉴를 먼저 실행하세요."); // 기본 Prefab 누락 오류 출력
                return; // 생성기 구성 중단
            } // 기본 모듈 Prefab 누락 처리

            MapGenerationSettings settings = CreateOrUpdateSettings(modulePrefabs); // 기본 생성 설정 에셋 생성 또는 갱신
            ProceduralMapGenerator generator = FindOrCreateGenerator(); // 현재 Scene 생성기 조회 또는 생성
            Transform generatedRoot = FindOrCreateGeneratedRoot(generator.transform); // 생성 모듈 보관 루트 조회 또는 생성
            generator.ConfigureForEditor(settings, generatedRoot, true, true); // 생성기에 기본 설정 연결
            EditorUtility.SetDirty(settings); // 생성 설정 변경 상태 표시
            EditorUtility.SetDirty(generator); // 생성기 변경 상태 표시
            EditorSceneManager.MarkSceneDirty(generator.gameObject.scene); // 현재 Scene 저장 필요 상태 표시
            AssetDatabase.SaveAssets(); // 생성 설정 에셋 저장
            Selection.activeGameObject = generator.gameObject; // 구성된 생성기 오브젝트 선택
            EditorGUIUtility.PingObject(generator.gameObject); // Hierarchy에서 생성기 강조
            Debug.Log("[ProjectJ][Day31] 시드 기반 선형 맵 생성기 구성을 완료했습니다.", generator); // 생성기 구성 완료 로그 출력
        } // 생성 설정과 Scene 생성기 구성 처리

        [MenuItem(MenuPath, true)] // 31일차 구성 메뉴 활성 조건 등록
        private static bool ValidateCreateOrUpdateProceduralMapGenerator() // Play Mode가 아닐 때만 메뉴 실행 허용
        { // 메뉴 실행 조건 검사 처리
            return !EditorApplication.isPlayingOrWillChangePlaymode; // Edit Mode 실행 가능 여부 반환
        } // 메뉴 실행 조건 검사 처리

        private static MapModuleDefinition[] LoadModulePrefabs() // 기본 맵 모듈 Prefab 목록 조회
        { // 기본 맵 모듈 Prefab 조회 처리
            MapModuleDefinition[] prefabs = new MapModuleDefinition[3]; // 세 개 모듈 결과 배열 생성
            prefabs[0] = LoadModuleDefinition(FixedPrefabPath); // 고정 발판 모듈 조회
            prefabs[1] = LoadModuleDefinition(LowPassagePrefabPath); // 낮은 통로 모듈 조회
            prefabs[2] = LoadModuleDefinition(JumpGapPrefabPath); // 점프 간격 모듈 조회
            return prefabs; // 기본 모듈 목록 반환
        } // 기본 맵 모듈 Prefab 조회 처리

        private static MapModuleDefinition LoadModuleDefinition(string prefabPath) // 단일 Prefab의 모듈 정의 조회
        { // 단일 모듈 정의 조회 처리
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath); // 지정 경로 Prefab 에셋 조회
            return prefab != null ? prefab.GetComponent<MapModuleDefinition>() : null; // 모듈 정의 또는 빈 결과 반환
        } // 단일 모듈 정의 조회 처리

        private static bool ContainsMissingPrefab(MapModuleDefinition[] prefabs) // 후보 목록의 빈 Prefab 검사
        { // 빈 Prefab 검사 처리
            for (int prefabIndex = 0; prefabIndex < prefabs.Length; prefabIndex++) // 모든 후보 Prefab 순회
            { // 후보 Prefab 누락 검사 처리
                if (prefabs[prefabIndex] == null) // 현재 Prefab 누락 확인
                { // 현재 Prefab 누락 처리
                    return true; // 누락 있음 반환
                } // 현재 Prefab 누락 처리
            } // 후보 Prefab 누락 검사 처리

            return false; // 누락 없음 반환
        } // 빈 Prefab 검사 처리

        private static MapGenerationSettings CreateOrUpdateSettings(MapModuleDefinition[] modulePrefabs) // 기본 맵 생성 설정 에셋 구성
        { // 맵 생성 설정 에셋 구성 처리
            MapGenerationSettings settings = AssetDatabase.LoadAssetAtPath<MapGenerationSettings>(SettingsAssetPath); // 기존 생성 설정 에셋 조회

            if (settings == null) // 생성 설정 에셋 누락 확인
            { // 생성 설정 에셋 생성 처리
                settings = ScriptableObject.CreateInstance<MapGenerationSettings>(); // 새 생성 설정 인스턴스 생성
                AssetDatabase.CreateAsset(settings, SettingsAssetPath); // 새 생성 설정 에셋 저장
            } // 생성 설정 에셋 생성 처리

            settings.ConfigureForEditor(31001, false, 0, 8, 32, 0.05f, modulePrefabs); // 31일차 기본 생성 수치 적용
            return settings; // 구성된 생성 설정 반환
        } // 맵 생성 설정 에셋 구성 처리

        private static ProceduralMapGenerator FindOrCreateGenerator() // 현재 Scene 생성기 조회 또는 생성
        { // Scene 생성기 조회 또는 생성 처리
            ProceduralMapGenerator generator = Object.FindFirstObjectByType<ProceduralMapGenerator>(); // 현재 Scene의 기존 생성기 조회

            if (generator != null) // 기존 생성기 확인
            { // 기존 생성기 처리
                return generator; // 기존 생성기 반환
            } // 기존 생성기 처리

            GameObject generatorObject = new GameObject("ProceduralMapGenerator"); // 새 생성기 오브젝트 생성
            generatorObject.transform.position = new Vector3(50f, 0f, 0f); // 기존 수직 테스트 맵과 분리된 시험 위치 적용
            Undo.RegisterCreatedObjectUndo(generatorObject, "Create Procedural Map Generator"); // 생성 작업 Undo 등록
            return generatorObject.AddComponent<ProceduralMapGenerator>(); // 새 생성기 컴포넌트 반환
        } // Scene 생성기 조회 또는 생성 처리

        private static Transform FindOrCreateGeneratedRoot(Transform generatorTransform) // 생성 모듈 보관 루트 조회 또는 생성
        { // 생성 모듈 보관 루트 조회 또는 생성 처리
            Transform generatedRoot = generatorTransform.Find("GeneratedMap"); // 기존 생성 루트 자식 조회

            if (generatedRoot != null) // 기존 생성 루트 확인
            { // 기존 생성 루트 처리
                return generatedRoot; // 기존 생성 루트 반환
            } // 기존 생성 루트 처리

            GameObject rootObject = new GameObject("GeneratedMap"); // 새 생성 루트 오브젝트 생성
            Undo.RegisterCreatedObjectUndo(rootObject, "Create Generated Map Root"); // 생성 루트 Undo 등록
            rootObject.transform.SetParent(generatorTransform, false); // 생성기를 부모로 설정
            return rootObject.transform; // 새 생성 루트 반환
        } // 생성 모듈 보관 루트 조회 또는 생성 처리
    } // 31일차 맵 생성기 구성 도구 묶음
} // 프로젝트 Editor 기능 묶음
