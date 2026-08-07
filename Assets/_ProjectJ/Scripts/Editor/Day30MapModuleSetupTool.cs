using ProjectJ.MapGeneration; // 맵 모듈 Runtime 기능 참조
using UnityEditor; // Unity Editor 에셋과 메뉴 기능 참조
using UnityEngine; // Unity 오브젝트 생성 기능 참조

namespace ProjectJ.Editor // 프로젝트 Editor 기능 네임스페이스 선언
{ // 프로젝트 Editor 기능 묶음
    internal static class Day30MapModuleSetupTool // 30일차 기본 맵 모듈 구성 도구 선언
    { // 30일차 기본 맵 모듈 구성 도구 묶음
        private const string MenuPath = ProjectJEditorMenuPaths.MapModules + "/기본 맵 모듈 생성 (Day 30일차)"; // 기본 맵 모듈 생성 메뉴 경로
        private const string DataFolderPath = "Assets/_ProjectJ/Data/Definitions/Map"; // 맵 데이터 폴더 경로
        private const string ProfileAssetPath = DataFolderPath + "/MAP-TRV-001_DefaultTraversal.asset"; // 기본 이동 능력 에셋 경로
        private const string PrefabFolderPath = "Assets/_ProjectJ/Prefabs/Map/Modules"; // 맵 모듈 Prefab 폴더 경로
        private const string FixedPrefabPath = PrefabFolderPath + "/MAP-001_FixedStraight.prefab"; // 고정 발판 Prefab 경로
        private const string LowPassagePrefabPath = PrefabFolderPath + "/MAP-002_LowPassage.prefab"; // 낮은 통로 Prefab 경로
        private const string JumpGapPrefabPath = PrefabFolderPath + "/MAP-003_JumpGap.prefab"; // 점프 간격 Prefab 경로

        [MenuItem(MenuPath)] // Unity 상단 메뉴에 30일차 구성 항목 등록
        private static void CreateOrUpdateBasicModules() // 이동 기준과 기본 모듈 Prefab 생성
        { // 기본 모듈 생성 처리
            EnsureFolderExists(DataFolderPath); // 맵 데이터 폴더 존재 보장
            EnsureFolderExists(PrefabFolderPath); // 맵 Prefab 폴더 존재 보장
            MapTraversalProfile traversalProfile = CreateOrUpdateTraversalProfile(); // 기본 이동 능력 에셋 생성 또는 갱신
            CreateFixedPlatformPrefab(traversalProfile); // 고정 발판 Prefab 생성 또는 갱신
            CreateLowPassagePrefab(traversalProfile); // 낮은 통로 Prefab 생성 또는 갱신
            CreateJumpGapPrefab(traversalProfile); // 점프 간격 Prefab 생성 또는 갱신
            AssetDatabase.SaveAssets(); // 생성한 에셋 저장
            AssetDatabase.Refresh(); // Project 창 에셋 목록 새로고침
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(FixedPrefabPath); // 생성한 첫 Prefab 선택
            EditorGUIUtility.PingObject(Selection.activeObject); // Project 창에서 첫 Prefab 강조
            Debug.Log("[ProjectJ][Day30] 맵 이동 기준과 기본 모듈 3개 생성을 완료했습니다."); // 기본 모듈 생성 완료 로그 출력
        } // 기본 모듈 생성 종료

        [MenuItem(MenuPath, true)] // 30일차 구성 메뉴 활성 조건 등록
        private static bool ValidateCreateOrUpdateBasicModules() // Play Mode가 아닐 때만 메뉴 실행 허용
        { // 메뉴 실행 조건 검사 처리
            return !EditorApplication.isPlayingOrWillChangePlaymode; // Edit Mode 실행 가능 여부 반환
        } // 메뉴 실행 조건 검사 종료

        private static MapTraversalProfile CreateOrUpdateTraversalProfile() // 프로젝트 J 플레이어 수치 기반 이동 능력 에셋 구성
        { // 이동 능력 에셋 구성 처리
            MapTraversalProfile profile = AssetDatabase.LoadAssetAtPath<MapTraversalProfile>(ProfileAssetPath); // 기존 이동 능력 에셋 조회

            if (profile == null) // 이동 능력 에셋 누락 확인
            { // 이동 능력 에셋 생성 처리
                profile = ScriptableObject.CreateInstance<MapTraversalProfile>(); // 새 이동 능력 에셋 인스턴스 생성
                AssetDatabase.CreateAsset(profile, ProfileAssetPath); // 새 이동 능력 에셋 저장
            } // 이동 능력 에셋 생성 종료

            profile.ConfigureForEditor(2f, 1.2f, 0.45f, 6f, 2.4f, 25f, 3f, 0.8f, 0.1f); // 안전 낙하 높이를 포함한 이동 수치 적용
            EditorUtility.SetDirty(profile); // 이동 능력 에셋 변경 상태 표시
            return profile; // 구성된 이동 능력 에셋 반환
        } // 이동 능력 에셋 구성 종료

        private static void CreateFixedPlatformPrefab(MapTraversalProfile traversalProfile) // 고정 직선 발판 Prefab 구성
        { // 고정 직선 발판 구성 처리
            GameObject root = CreateModuleRoot("MAP-001_FixedStraight", 9); // 고정 발판 루트 생성
            CreateCube(root.transform, "Floor", new Vector3(0f, -0.25f, 0f), new Vector3(4f, 0.5f, 8f), 9); // 고정 발판 바닥 생성
            CreateConnectionPoint(root.transform, "Entrance", new Vector3(0f, 0f, -4f), MapConnectionRole.Entrance, MapConnectionDirection.South); // 남쪽 입구 생성
            CreateConnectionPoint(root.transform, "Exit", new Vector3(0f, 0f, 4f), MapConnectionRole.Exit, MapConnectionDirection.North); // 북쪽 출구 생성
            ConfigureModule(root, "MAP-001", MapModuleKind.FixedPlatform, MapTraversalRequirement.Walk, new Vector3(0f, 1f, 0f), new Vector3(4f, 2f, 8f), 2.2f, 0f, 0f, traversalProfile); // 고정 발판 데이터 적용
            SaveAndDestroyPrefab(root, FixedPrefabPath); // 고정 발판 Prefab 저장
        } // 고정 직선 발판 구성 종료

        private static void CreateLowPassagePrefab(MapTraversalProfile traversalProfile) // 낮은 직선 통로 Prefab 구성
        { // 낮은 직선 통로 구성 처리
            GameObject root = CreateModuleRoot("MAP-002_LowPassage", 9); // 낮은 통로 루트 생성
            CreateCube(root.transform, "Floor", new Vector3(0f, -0.25f, 0f), new Vector3(4f, 0.5f, 8f), 9); // 낮은 통로 바닥 생성
            CreateCube(root.transform, "Ceiling", new Vector3(0f, 1.75f, 0f), new Vector3(4f, 0.5f, 4f), 9); // 바닥부터 1.5미터 높이의 천장 생성
            CreateConnectionPoint(root.transform, "Entrance", new Vector3(0f, 0f, -4f), MapConnectionRole.Entrance, MapConnectionDirection.South); // 남쪽 입구 생성
            CreateConnectionPoint(root.transform, "Exit", new Vector3(0f, 0f, 4f), MapConnectionRole.Exit, MapConnectionDirection.North); // 북쪽 출구 생성
            ConfigureModule(root, "MAP-002", MapModuleKind.LowPassage, MapTraversalRequirement.Crouch, new Vector3(0f, 1.25f, 0f), new Vector3(4f, 2.5f, 8f), 1.5f, 0f, 0f, traversalProfile); // 낮은 통로 데이터 적용
            SaveAndDestroyPrefab(root, LowPassagePrefabPath); // 낮은 통로 Prefab 저장
        } // 낮은 직선 통로 구성 종료

        private static void CreateJumpGapPrefab(MapTraversalProfile traversalProfile) // 안전 거리 점프 간격 Prefab 구성
        { // 점프 간격 구성 처리
            GameObject root = CreateModuleRoot("MAP-003_JumpGap", 9); // 점프 간격 루트 생성
            CreateCube(root.transform, "StartPlatform", new Vector3(0f, -0.25f, -4f), new Vector3(4f, 0.5f, 4f), 9); // 점프 시작 발판 생성
            CreateCube(root.transform, "LandingPlatform", new Vector3(0f, -0.25f, 4f), new Vector3(4f, 0.5f, 4f), 9); // 점프 도착 발판 생성
            CreateConnectionPoint(root.transform, "Entrance", new Vector3(0f, 0f, -6f), MapConnectionRole.Entrance, MapConnectionDirection.South); // 남쪽 입구 생성
            CreateConnectionPoint(root.transform, "Exit", new Vector3(0f, 0f, 6f), MapConnectionRole.Exit, MapConnectionDirection.North); // 북쪽 출구 생성
            ConfigureModule(root, "MAP-003", MapModuleKind.JumpGap, MapTraversalRequirement.Jump, new Vector3(0f, 1f, 0f), new Vector3(4f, 2f, 12f), 2.2f, 4f, 0f, traversalProfile); // 점프 간격 데이터 적용
            SaveAndDestroyPrefab(root, JumpGapPrefabPath); // 점프 간격 Prefab 저장
        } // 안전 거리 점프 간격 구성 종료

        private static GameObject CreateModuleRoot(string rootName, int fallbackLayer) // 모듈 루트 오브젝트 생성
        { // 모듈 루트 생성 처리
            GameObject root = new GameObject(rootName); // 빈 모듈 루트 생성
            int groundLayer = LayerMask.NameToLayer("Ground"); // Ground 레이어 번호 조회
            root.layer = groundLayer >= 0 ? groundLayer : fallbackLayer; // Ground 또는 기본 레이어 적용
            root.AddComponent<MapModuleDefinition>(); // 모듈 정의 컴포넌트 추가
            return root; // 생성된 모듈 루트 반환
        } // 모듈 루트 생성 종료

        private static GameObject CreateCube(Transform parent, string objectName, Vector3 localPosition, Vector3 localScale, int fallbackLayer) // 모듈용 Cube 생성
        { // 모듈용 Cube 생성 처리
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube); // 기본 Cube 오브젝트 생성
            cube.name = objectName; // Cube 이름 적용
            cube.transform.SetParent(parent, false); // 모듈 루트 자식으로 설정
            cube.transform.localPosition = localPosition; // 로컬 위치 적용
            cube.transform.localRotation = Quaternion.identity; // 로컬 회전 초기화
            cube.transform.localScale = localScale; // 로컬 크기 적용
            int groundLayer = LayerMask.NameToLayer("Ground"); // Ground 레이어 번호 조회
            cube.layer = groundLayer >= 0 ? groundLayer : fallbackLayer; // Ground 또는 기본 레이어 적용
            return cube; // 생성된 Cube 반환
        } // 모듈용 Cube 생성 종료

        private static MapModuleConnectionPoint CreateConnectionPoint(Transform parent, string pointName, Vector3 localPosition, MapConnectionRole role, MapConnectionDirection direction) // 입구 또는 출구 연결 지점 생성
        { // 연결 지점 생성 처리
            GameObject pointObject = new GameObject(pointName); // 빈 연결 지점 오브젝트 생성
            pointObject.transform.SetParent(parent, false); // 모듈 루트 자식으로 설정
            pointObject.transform.localPosition = localPosition; // 로컬 연결 위치 적용
            pointObject.transform.localRotation = Quaternion.identity; // 로컬 연결 회전 초기화
            MapModuleConnectionPoint point = pointObject.AddComponent<MapModuleConnectionPoint>(); // 연결 지점 컴포넌트 추가
            point.ConfigureForEditor(pointName, role, direction, 2f, 2.2f); // 연결 지점 기본 데이터 적용
            return point; // 생성된 연결 지점 반환
        } // 연결 지점 생성 종료

        private static void ConfigureModule(GameObject root, string moduleId, MapModuleKind moduleKind, MapTraversalRequirement requirement, Vector3 boundsCenter, Vector3 boundsSize, float clearanceHeight, float jumpDistance, float jumpRise, MapTraversalProfile traversalProfile) // 모듈 공통 데이터 적용
        { // 모듈 공통 데이터 적용 처리
            MapModuleDefinition definition = root.GetComponent<MapModuleDefinition>(); // 루트 모듈 정의 조회
            definition.ConfigureForEditor(moduleId, moduleKind, requirement, MapRotationOptions.All, boundsCenter, boundsSize, clearanceHeight, jumpDistance, jumpRise, traversalProfile); // 공통 모듈 데이터 설정
            definition.RefreshConnectionPoints(); // 생성된 입구와 출구 목록 수집

            if (!definition.TryValidate(out string reason)) // 생성된 모듈 유효성 확인
            { // 생성된 모듈 오류 처리
                Debug.LogError($"[ProjectJ][Day30] {moduleId} 검증 실패: {reason}", root); // 모듈 검증 오류 출력
            } // 생성된 모듈 오류 처리 종료
        } // 모듈 공통 데이터 적용 종료

        private static void SaveAndDestroyPrefab(GameObject root, string prefabPath) // 임시 루트를 Prefab으로 저장하고 정리
        { // Prefab 저장과 정리 처리
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath); // 지정 경로에 Prefab 저장 또는 갱신
            Object.DestroyImmediate(root); // Scene에 남은 임시 루트 제거
        } // Prefab 저장과 정리 종료

        private static void EnsureFolderExists(string folderPath) // 지정된 Unity 에셋 폴더 존재 보장
        { // 에셋 폴더 생성 처리
            if (AssetDatabase.IsValidFolder(folderPath)) // 대상 폴더 존재 확인
            { // 기존 폴더 처리
                return; // 폴더 생성 생략
            } // 기존 폴더 처리 종료

            string[] pathParts = folderPath.Split('/'); // 전체 경로를 단계별 이름으로 분리
            string currentPath = pathParts[0]; // Assets 루트 경로 저장

            for (int index = 1; index < pathParts.Length; index++) // 하위 폴더 단계 순회
            { // 하위 폴더 생성 처리
                string nextPath = $"{currentPath}/{pathParts[index]}"; // 다음 단계 전체 경로 계산

                if (!AssetDatabase.IsValidFolder(nextPath)) // 다음 단계 폴더 누락 확인
                { // 누락 폴더 생성 처리
                    AssetDatabase.CreateFolder(currentPath, pathParts[index]); // 현재 경로 아래에 폴더 생성
                } // 누락 폴더 생성 종료

                currentPath = nextPath; // 현재 경로를 다음 단계로 갱신
            } // 하위 폴더 생성 종료
        } // 에셋 폴더 생성 종료
    } // 30일차 기본 맵 모듈 구성 도구 묶음 종료
} // 프로젝트 Editor 기능 묶음 종료

