using ProjectJ.MapGeneration; // 맵 생성 Runtime 기능 참조
using UnityEditor; // Unity Editor 에셋과 메뉴 기능 참조
using UnityEditor.SceneManagement; // Unity Scene 저장 상태 기능 참조
using UnityEngine; // Unity 오브젝트 기능 참조

namespace ProjectJ.Editor // 프로젝트 Editor 기능 네임스페이스 선언
{ // 프로젝트 Editor 기능 묶음
    internal static class Day32BranchedMapSetupTool // 32일차 분기 맵 구성 도구 선언
    { // 32일차 분기 맵 구성 도구 묶음
        private const string MenuPath = ProjectJEditorMenuPaths.MapGeneration + "/분기·합류 맵 생성 구성 (Day 32일차)"; // 분기 맵 구성 메뉴 경로
        private const string DataFolderPath = "Assets/_ProjectJ/Data/Definitions/Map"; // 맵 데이터 폴더 경로
        private const string SettingsAssetPath = DataFolderPath + "/MAP-GEN-001_DefaultGenerationSettings.asset"; // 기본 생성 설정 에셋 경로
        private const string ProfileAssetPath = DataFolderPath + "/MAP-TRV-001_DefaultTraversal.asset"; // 기본 이동 능력 에셋 경로
        private const string PrefabFolderPath = "Assets/_ProjectJ/Prefabs/Map/Modules"; // 맵 모듈 Prefab 폴더 경로
        private const string FixedPrefabPath = PrefabFolderPath + "/MAP-001_FixedStraight.prefab"; // 고정 발판 Prefab 경로
        private const string LowPassagePrefabPath = PrefabFolderPath + "/MAP-002_LowPassage.prefab"; // 낮은 통로 Prefab 경로
        private const string JumpGapPrefabPath = PrefabFolderPath + "/MAP-003_JumpGap.prefab"; // 점프 간격 Prefab 경로
        private const string BranchPrefabPath = PrefabFolderPath + "/MAP-004_Branch.prefab"; // 분기 Prefab 경로
        private const string MergePrefabPath = PrefabFolderPath + "/MAP-005_Merge.prefab"; // 합류 Prefab 경로
        private static readonly Vector3 GenerationOrigin = new Vector3(50f, 0f, 0f); // 기존 수직 시험 맵과 분리된 생성 원점

        [MenuItem(MenuPath)] // Unity 상단 메뉴에 32일차 구성 항목 등록
        private static void CreateOrUpdateBranchedMapGeneration() // 분기와 합류 Prefab 및 생성기 구성
        { // 분기 맵 구성 처리
            MapTraversalProfile traversalProfile = AssetDatabase.LoadAssetAtPath<MapTraversalProfile>(ProfileAssetPath); // 기존 이동 능력 에셋 조회

            if (traversalProfile == null) // 이동 능력 에셋 누락 확인
            { // 이동 능력 에셋 누락 처리
                Debug.LogError("[ProjectJ][Day32] MAP-TRV-001_DefaultTraversal 에셋이 없습니다. Day 30 생성 메뉴를 먼저 실행하세요."); // 이동 능력 누락 오류 출력
                return; // 분기 맵 구성 중단
            } // 이동 능력 에셋 누락 처리

            if (!BasicModulePrefabsExist()) // 30일차 기본 Prefab 존재 여부 확인
            { // 기본 Prefab 누락 처리
                Debug.LogError("[ProjectJ][Day32] 30일차 기본 맵 모듈 Prefab 3개가 없습니다. Day 30 생성 메뉴를 먼저 실행하세요."); // 기본 Prefab 누락 오류 출력
                return; // 분기 맵 구성 중단
            } // 기본 Prefab 누락 처리

            CreateBranchPrefab(traversalProfile); // 분기 모듈 Prefab 생성 또는 갱신
            CreateMergePrefab(traversalProfile); // 합류 모듈 Prefab 생성 또는 갱신
            AssetDatabase.SaveAssets(); // 새 특수 Prefab 저장
            AssetDatabase.Refresh(); // Project 창 에셋 목록 새로고침
            MapModuleDefinition[] modulePrefabs = LoadAllModulePrefabs(); // 기본과 특수 모듈 전체 조회

            if (ContainsMissingPrefab(modulePrefabs)) // 전체 모듈 Prefab 누락 확인
            { // 전체 Prefab 누락 처리
                Debug.LogError("[ProjectJ][Day32] 생성 후보 모듈 5개 중 누락된 Prefab이 있습니다."); // 전체 Prefab 누락 오류 출력
                return; // 분기 맵 구성 중단
            } // 전체 Prefab 누락 처리

            MapGenerationSettings settings = CreateOrUpdateSettings(modulePrefabs); // 32일차 생성 설정 에셋 구성
            ProceduralMapGenerator generator = FindOrCreateGenerator(); // 현재 Scene 생성기 조회 또는 생성
            generator.transform.position = GenerationOrigin; // 기존 수직 시험 맵과 분리된 생성 원점 강제 적용
            Transform generatedRoot = FindOrCreateGeneratedRoot(generator.transform); // 생성 모듈 보관 루트 조회 또는 생성
            generator.ConfigureForEditor(settings, generatedRoot, true, true); // 생성기에 32일차 설정 연결
            generator.ClearGeneratedMap(); // Scene에 남은 이전 미리보기 결과 제거
            EditorUtility.SetDirty(settings); // 생성 설정 변경 상태 표시
            EditorUtility.SetDirty(generator); // 생성기 변경 상태 표시
            EditorSceneManager.MarkSceneDirty(generator.gameObject.scene); // 현재 Scene 저장 필요 상태 표시
            AssetDatabase.SaveAssets(); // 변경된 설정 에셋 저장
            Selection.activeGameObject = generator.gameObject; // 구성된 생성기 오브젝트 선택
            EditorGUIUtility.PingObject(generator.gameObject); // Hierarchy에서 생성기 강조
            Debug.Log("[ProjectJ][Day32] 분기·병렬 경로·합류 맵 생성 구성을 완료했습니다.", generator); // 분기 맵 구성 완료 로그 출력
        } // 분기 맵 구성 처리

        [MenuItem(MenuPath, true)] // 32일차 구성 메뉴 활성 조건 등록
        private static bool ValidateCreateOrUpdateBranchedMapGeneration() // Play Mode가 아닐 때만 메뉴 실행 허용
        { // 메뉴 실행 조건 검사 처리
            return !EditorApplication.isPlayingOrWillChangePlaymode; // Edit Mode 실행 가능 여부 반환
        } // 메뉴 실행 조건 검사 처리

        private static bool BasicModulePrefabsExist() // 30일차 기본 모듈 Prefab 존재 여부 검사
        { // 기본 모듈 Prefab 존재 검사 처리
            bool fixedExists = AssetDatabase.LoadAssetAtPath<GameObject>(FixedPrefabPath) != null; // 고정 발판 Prefab 존재 여부 계산
            bool lowPassageExists = AssetDatabase.LoadAssetAtPath<GameObject>(LowPassagePrefabPath) != null; // 낮은 통로 Prefab 존재 여부 계산
            bool jumpGapExists = AssetDatabase.LoadAssetAtPath<GameObject>(JumpGapPrefabPath) != null; // 점프 간격 Prefab 존재 여부 계산
            return fixedExists && lowPassageExists && jumpGapExists; // 기본 Prefab 전체 존재 여부 반환
        } // 기본 모듈 Prefab 존재 검사 처리

        private static void CreateBranchPrefab(MapTraversalProfile traversalProfile) // 중앙 경로를 좌우 두 경로로 나누는 Prefab 구성
        { // 분기 Prefab 구성 처리
            GameObject root = CreateModuleRoot("MAP-004_Branch"); // 분기 모듈 루트 생성
            CreateCube(root.transform, "EntranceStem", new Vector3(0f, -0.25f, -2f), new Vector3(4f, 0.5f, 4f)); // 중앙 진입 발판 생성
            CreateCube(root.transform, "CrossPlatform", new Vector3(0f, -0.25f, 1f), new Vector3(16f, 0.5f, 2f)); // 좌우 연결 발판 생성
            CreateCube(root.transform, "LeftExitPlatform", new Vector3(-6f, -0.25f, 3f), new Vector3(4f, 0.5f, 2f)); // 왼쪽 출구 발판 생성
            CreateCube(root.transform, "RightExitPlatform", new Vector3(6f, -0.25f, 3f), new Vector3(4f, 0.5f, 2f)); // 오른쪽 출구 발판 생성
            CreateConnectionPoint(root.transform, "Entrance", new Vector3(0f, 0f, -4f), MapConnectionRole.Entrance, MapConnectionDirection.South); // 중앙 남쪽 입구 생성
            CreateConnectionPoint(root.transform, "ExitLeft", new Vector3(-6f, 0f, 4f), MapConnectionRole.Exit, MapConnectionDirection.North); // 왼쪽 북쪽 출구 생성
            CreateConnectionPoint(root.transform, "ExitRight", new Vector3(6f, 0f, 4f), MapConnectionRole.Exit, MapConnectionDirection.North); // 오른쪽 북쪽 출구 생성
            ConfigureModule(root, "MAP-004", MapModuleKind.Branch, new Vector3(0f, 1f, 0f), new Vector3(16f, 2f, 8f), traversalProfile); // 분기 모듈 데이터 적용
            SaveAndDestroyPrefab(root, BranchPrefabPath); // 분기 Prefab 저장
        } // 분기 Prefab 구성 처리

        private static void CreateMergePrefab(MapTraversalProfile traversalProfile) // 좌우 두 경로를 중앙 경로로 합치는 Prefab 구성
        { // 합류 Prefab 구성 처리
            GameObject root = CreateModuleRoot("MAP-005_Merge"); // 합류 모듈 루트 생성
            CreateCube(root.transform, "LeftEntrancePlatform", new Vector3(-6f, -0.25f, -3f), new Vector3(4f, 0.5f, 2f)); // 왼쪽 입구 발판 생성
            CreateCube(root.transform, "RightEntrancePlatform", new Vector3(6f, -0.25f, -3f), new Vector3(4f, 0.5f, 2f)); // 오른쪽 입구 발판 생성
            CreateCube(root.transform, "CrossPlatform", new Vector3(0f, -0.25f, -1f), new Vector3(16f, 0.5f, 2f)); // 좌우 합류 발판 생성
            CreateCube(root.transform, "ExitStem", new Vector3(0f, -0.25f, 2f), new Vector3(4f, 0.5f, 4f)); // 중앙 출구 발판 생성
            CreateConnectionPoint(root.transform, "EntranceLeft", new Vector3(-6f, 0f, -4f), MapConnectionRole.Entrance, MapConnectionDirection.South); // 왼쪽 남쪽 입구 생성
            CreateConnectionPoint(root.transform, "EntranceRight", new Vector3(6f, 0f, -4f), MapConnectionRole.Entrance, MapConnectionDirection.South); // 오른쪽 남쪽 입구 생성
            CreateConnectionPoint(root.transform, "Exit", new Vector3(0f, 0f, 4f), MapConnectionRole.Exit, MapConnectionDirection.North); // 중앙 북쪽 출구 생성
            ConfigureModule(root, "MAP-005", MapModuleKind.Merge, new Vector3(0f, 1f, 0f), new Vector3(16f, 2f, 8f), traversalProfile); // 합류 모듈 데이터 적용
            SaveAndDestroyPrefab(root, MergePrefabPath); // 합류 Prefab 저장
        } // 합류 Prefab 구성 처리

        private static GameObject CreateModuleRoot(string rootName) // 특수 모듈 루트 오브젝트 생성
        { // 특수 모듈 루트 생성 처리
            GameObject root = new GameObject(rootName); // 빈 특수 모듈 루트 생성
            root.layer = ResolveGroundLayer(); // Ground 레이어 적용
            root.AddComponent<MapModuleDefinition>(); // 모듈 정의 컴포넌트 추가
            return root; // 생성된 특수 모듈 루트 반환
        } // 특수 모듈 루트 생성 처리

        private static GameObject CreateCube(Transform parent, string objectName, Vector3 localPosition, Vector3 localScale) // 특수 모듈용 Cube 생성
        { // 특수 모듈용 Cube 생성 처리
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube); // 기본 Cube 오브젝트 생성
            cube.name = objectName; // Cube 이름 적용
            cube.transform.SetParent(parent, false); // 모듈 루트 자식으로 설정
            cube.transform.localPosition = localPosition; // 로컬 위치 적용
            cube.transform.localRotation = Quaternion.identity; // 로컬 회전 초기화
            cube.transform.localScale = localScale; // 로컬 크기 적용
            cube.layer = ResolveGroundLayer(); // Ground 레이어 적용
            return cube; // 생성된 Cube 반환
        } // 특수 모듈용 Cube 생성 처리

        private static MapModuleConnectionPoint CreateConnectionPoint(Transform parent, string pointName, Vector3 localPosition, MapConnectionRole role, MapConnectionDirection direction) // 특수 모듈 연결 지점 생성
        { // 특수 모듈 연결 지점 생성 처리
            GameObject pointObject = new GameObject(pointName); // 빈 연결 지점 오브젝트 생성
            pointObject.transform.SetParent(parent, false); // 모듈 루트 자식으로 설정
            pointObject.transform.localPosition = localPosition; // 로컬 연결 위치 적용
            pointObject.transform.localRotation = Quaternion.identity; // 로컬 연결 회전 초기화
            MapModuleConnectionPoint point = pointObject.AddComponent<MapModuleConnectionPoint>(); // 연결 지점 컴포넌트 추가
            point.ConfigureForEditor(pointName, role, direction, 2f, 2.2f); // 연결 지점 공통 크기 적용
            return point; // 생성된 연결 지점 반환
        } // 특수 모듈 연결 지점 생성 처리

        private static void ConfigureModule(GameObject root, string moduleId, MapModuleKind moduleKind, Vector3 boundsCenter, Vector3 boundsSize, MapTraversalProfile traversalProfile) // 특수 모듈 공통 데이터 적용
        { // 특수 모듈 공통 데이터 적용 처리
            MapModuleDefinition definition = root.GetComponent<MapModuleDefinition>(); // 루트 모듈 정의 조회
            definition.ConfigureForEditor(moduleId, moduleKind, MapTraversalRequirement.Walk, MapRotationOptions.All, boundsCenter, boundsSize, 2.2f, 0f, 0f, traversalProfile); // 특수 모듈 데이터 설정
            definition.RefreshConnectionPoints(); // 생성된 입구와 출구 목록 수집

            if (!definition.TryValidate(out string reason)) // 생성된 특수 모듈 유효성 확인
            { // 특수 모듈 오류 처리
                Debug.LogError($"[ProjectJ][Day32] {moduleId} 검증 실패: {reason}", root); // 특수 모듈 검증 오류 출력
            } // 특수 모듈 오류 처리
        } // 특수 모듈 공통 데이터 적용 처리

        private static void SaveAndDestroyPrefab(GameObject root, string prefabPath) // 임시 루트를 Prefab으로 저장하고 정리
        { // Prefab 저장과 정리 처리
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath); // 지정 경로에 Prefab 저장 또는 갱신
            Object.DestroyImmediate(root); // Scene에 남은 임시 루트 제거
        } // Prefab 저장과 정리 처리

        private static int ResolveGroundLayer() // Ground 레이어 또는 안전 기본값 조회
        { // Ground 레이어 조회 처리
            int groundLayer = LayerMask.NameToLayer("Ground"); // Ground 레이어 번호 조회
            return groundLayer >= 0 ? groundLayer : 9; // Ground 또는 기존 기본 번호 반환
        } // Ground 레이어 조회 처리

        private static MapModuleDefinition[] LoadAllModulePrefabs() // 기본과 특수 맵 모듈 전체 조회
        { // 전체 맵 모듈 조회 처리
            MapModuleDefinition[] prefabs = new MapModuleDefinition[5]; // 다섯 모듈 결과 배열 생성
            prefabs[0] = LoadModuleDefinition(FixedPrefabPath); // 고정 발판 모듈 조회
            prefabs[1] = LoadModuleDefinition(LowPassagePrefabPath); // 낮은 통로 모듈 조회
            prefabs[2] = LoadModuleDefinition(JumpGapPrefabPath); // 점프 간격 모듈 조회
            prefabs[3] = LoadModuleDefinition(BranchPrefabPath); // 분기 모듈 조회
            prefabs[4] = LoadModuleDefinition(MergePrefabPath); // 합류 모듈 조회
            return prefabs; // 전체 모듈 목록 반환
        } // 전체 맵 모듈 조회 처리

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

        private static MapGenerationSettings CreateOrUpdateSettings(MapModuleDefinition[] modulePrefabs) // 32일차 맵 생성 설정 에셋 구성
        { // 32일차 생성 설정 구성 처리
            MapGenerationSettings settings = AssetDatabase.LoadAssetAtPath<MapGenerationSettings>(SettingsAssetPath); // 기존 생성 설정 에셋 조회

            if (settings == null) // 생성 설정 에셋 누락 확인
            { // 생성 설정 에셋 생성 처리
                settings = ScriptableObject.CreateInstance<MapGenerationSettings>(); // 새 생성 설정 인스턴스 생성
                AssetDatabase.CreateAsset(settings, SettingsAssetPath); // 새 생성 설정 에셋 저장
            } // 생성 설정 에셋 생성 처리

            settings.ConfigureForEditor(31001, false, 0, 8, 32, 0.05f, 0.05f, 0.02f, true, 2, modulePrefabs); // 32일차 기본 생성 수치 적용
            return settings; // 구성된 생성 설정 반환
        } // 32일차 생성 설정 구성 처리

        private static ProceduralMapGenerator FindOrCreateGenerator() // 현재 Scene 생성기 조회 또는 생성
        { // Scene 생성기 조회 또는 생성 처리
            ProceduralMapGenerator generator = Object.FindFirstObjectByType<ProceduralMapGenerator>(); // 현재 Scene의 기존 생성기 조회

            if (generator != null) // 기존 생성기 확인
            { // 기존 생성기 처리
                return generator; // 기존 생성기 반환
            } // 기존 생성기 처리

            GameObject generatorObject = new GameObject("ProceduralMapGenerator"); // 새 생성기 오브젝트 생성
            generatorObject.transform.position = GenerationOrigin; // 기존 수직 시험 맵과 분리된 생성 원점 적용
            Undo.RegisterCreatedObjectUndo(generatorObject, "Create Procedural Map Generator"); // 생성 작업 Undo 등록
            return generatorObject.AddComponent<ProceduralMapGenerator>(); // 새 생성기 컴포넌트 반환
        } // Scene 생성기 조회 또는 생성 처리

        private static Transform FindOrCreateGeneratedRoot(Transform generatorTransform) // 생성 모듈 보관 루트 조회 또는 생성
        { // 생성 루트 조회 또는 생성 처리
            Transform generatedRoot = generatorTransform.Find("GeneratedMap"); // 기존 생성 루트 자식 조회

            if (generatedRoot != null) // 기존 생성 루트 확인
            { // 기존 생성 루트 처리
                generatedRoot.localPosition = Vector3.zero; // 생성 루트 로컬 위치 보정
                generatedRoot.localRotation = Quaternion.identity; // 생성 루트 로컬 회전 보정
                return generatedRoot; // 기존 생성 루트 반환
            } // 기존 생성 루트 처리

            GameObject rootObject = new GameObject("GeneratedMap"); // 새 생성 루트 오브젝트 생성
            Undo.RegisterCreatedObjectUndo(rootObject, "Create Generated Map Root"); // 생성 루트 Undo 등록
            rootObject.transform.SetParent(generatorTransform, false); // 생성기를 부모로 설정
            return rootObject.transform; // 새 생성 루트 반환
        } // 생성 루트 조회 또는 생성 처리
    } // 32일차 분기 맵 구성 도구 묶음
} // 프로젝트 Editor 기능 묶음
