using ProjectJ.MapGeneration; // 맵 모듈 Runtime 기능 참조
using UnityEditor; // Unity Editor 에셋과 메뉴 기능 참조
using UnityEngine; // Unity 오브젝트 생성 기능 참조

namespace ProjectJ.Editor // 프로젝트 Editor 기능 네임스페이스 선언
{ // 프로젝트 Editor 기능 묶음
    internal static class Day34VerticalMapModuleSetupTool // 34일차 수직 상승 모듈 구성 도구 선언
    { // 34일차 수직 상승 모듈 구성 도구 묶음
        private const string MenuPath = ProjectJEditorMenuPaths.MapModules + "/수직 상승 모듈 생성 (Day 34일차)"; // 수직 상승 모듈 생성 메뉴 경로
        private const string ProfileAssetPath = "Assets/_ProjectJ/Data/Definitions/Map/MAP-TRV-001_DefaultTraversal.asset"; // 기본 이동 능력 에셋 경로
        private const string PrefabFolderPath = "Assets/_ProjectJ/Prefabs/Map/Modules"; // 맵 모듈 Prefab 폴더 경로
        private const string StepRisePrefabPath = PrefabFolderPath + "/MAP-006_StepRise.prefab"; // 계단 상승 Prefab 경로
        private const string ZigzagRisePrefabPath = PrefabFolderPath + "/MAP-007_ZigzagRise.prefab"; // 지그재그 상승 Prefab 경로
        private const string JumpRisePrefabPath = PrefabFolderPath + "/MAP-008_JumpRise.prefab"; // 점프 상승 Prefab 경로
        private const float ConnectionWidth = 2f; // 공통 연결부 너비
        private const float ConnectionHeight = 2.2f; // 공통 연결부 높이

        [MenuItem(MenuPath)] // Unity 상단 메뉴에 34일차 구성 항목 등록
        private static void CreateOrUpdateVerticalRiseModules() // 수직 상승 모듈 3종 생성
        { // 수직 상승 모듈 생성 처리
            EnsureFolderExists(PrefabFolderPath); // 맵 Prefab 폴더 존재 보장
            MapTraversalProfile traversalProfile = AssetDatabase.LoadAssetAtPath<MapTraversalProfile>(ProfileAssetPath); // 기본 이동 능력 에셋 조회

            if (traversalProfile == null) // 기본 이동 능력 에셋 누락 확인
            { // 기본 이동 능력 에셋 누락 처리
                Debug.LogError("[ProjectJ][Day34] MAP-TRV-001_DefaultTraversal 에셋이 없습니다. Day 30 메뉴를 먼저 실행하세요."); // 선행 작업 안내 오류 출력
                return; // 수직 모듈 생성 중단
            } // 기본 이동 능력 에셋 누락 처리 종료

            CreateStepRisePrefab(traversalProfile); // 2미터 계단 상승 Prefab 생성 또는 갱신
            CreateZigzagRisePrefab(traversalProfile); // 4미터 지그재그 상승 Prefab 생성 또는 갱신
            CreateJumpRisePrefab(traversalProfile); // 3미터 점프 상승 Prefab 생성 또는 갱신
            AssetDatabase.SaveAssets(); // 생성한 Prefab 저장
            AssetDatabase.Refresh(); // Project 창 에셋 목록 새로고침
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(StepRisePrefabPath); // 생성한 첫 상승 Prefab 선택
            EditorGUIUtility.PingObject(Selection.activeObject); // Project 창에서 첫 상승 Prefab 강조
            Debug.Log("[ProjectJ][Day34] 수직 연결 데이터와 상승 모듈 3종 생성을 완료했습니다."); // 수직 모듈 생성 완료 로그 출력
        } // 수직 상승 모듈 생성 처리 종료

        [MenuItem(MenuPath, true)] // 34일차 구성 메뉴 활성 조건 등록
        private static bool ValidateCreateOrUpdateVerticalRiseModules() // Play Mode가 아닐 때만 메뉴 실행 허용
        { // 메뉴 실행 조건 검사 처리
            return !EditorApplication.isPlayingOrWillChangePlaymode; // Edit Mode 실행 가능 여부 반환
        } // 메뉴 실행 조건 검사 종료

        private static void CreateStepRisePrefab(MapTraversalProfile traversalProfile) // 2미터 계단 상승 Prefab 구성
        { // 계단 상승 Prefab 구성 처리
            GameObject root = CreateModuleRoot("MAP-006_StepRise"); // 계단 상승 모듈 루트 생성
            const int stepCount = 8; // 계단 상승 구간 개수
            const float stepHeight = 0.25f; // 단일 계단 상승 높이
            const float stepDepth = 1.5f; // 단일 계단 앞뒤 길이
            const float firstStepCenterZ = -6f; // 첫 계단 중앙 Z 위치

            for (int stepIndex = 0; stepIndex <= stepCount; stepIndex++) // 시작 발판부터 마지막 발판까지 순회
            { // 단일 계단 발판 생성 처리
                float surfaceHeight = stepIndex * stepHeight; // 현재 계단 표면 높이 계산
                float centerZ = firstStepCenterZ + stepIndex * stepDepth; // 현재 계단 중앙 Z 위치 계산
                Vector3 platformPosition = new Vector3(0f, surfaceHeight - 0.25f, centerZ); // 현재 계단 Cube 중앙 위치 계산
                CreateCube(root.transform, $"Step_{stepIndex:00}", platformPosition, new Vector3(4f, 0.5f, stepDepth)); // 현재 계단 발판 생성
            } // 단일 계단 발판 생성 처리 종료

            CreateConnectionPoint(root.transform, "Entrance", new Vector3(0f, 0f, -6.75f), MapConnectionRole.Entrance, MapConnectionDirection.South); // 0미터 남쪽 입구 생성
            CreateConnectionPoint(root.transform, "Exit", new Vector3(0f, 2f, 6.75f), MapConnectionRole.Exit, MapConnectionDirection.North); // 2미터 북쪽 출구 생성
            MapVerticalTraversalSegment[] segments = CreateRepeatedSegments("Step", stepCount, MapTraversalRequirement.Walk, stepHeight, stepDepth); // 여덟 계단 이동 데이터 생성
            ConfigureModule(root, "MAP-006", MapModuleKind.StepRise, MapTraversalRequirement.LedgeClimb, new Vector3(0f, 2f, 0f), new Vector3(4f, 4.5f, 13.5f), 2.2f, stepDepth, stepHeight, MapVerticalLayoutKind.StepRise, 2f, segments, traversalProfile); // 계단 상승 모듈 데이터 적용
            SaveAndDestroyPrefab(root, StepRisePrefabPath); // 계단 상승 Prefab 저장
        } // 계단 상승 Prefab 구성 처리 종료

        private static void CreateZigzagRisePrefab(MapTraversalProfile traversalProfile) // 4미터 지그재그 상승 Prefab 구성
        { // 지그재그 상승 Prefab 구성 처리
            GameObject root = CreateModuleRoot("MAP-007_ZigzagRise"); // 지그재그 상승 모듈 루트 생성
            CreateCube(root.transform, "StartPlatform", new Vector3(0f, -0.25f, -6f), new Vector3(4f, 0.5f, 3f)); // 시작 발판 생성
            CreateCube(root.transform, "ZigzagPlatform_01", new Vector3(-1.2f, 0.75f, -3f), new Vector3(3f, 0.5f, 3f)); // 왼쪽 1미터 발판 생성
            CreateCube(root.transform, "ZigzagPlatform_02", new Vector3(1.2f, 1.75f, 0f), new Vector3(3f, 0.5f, 3f)); // 오른쪽 2미터 발판 생성
            CreateCube(root.transform, "ZigzagPlatform_03", new Vector3(-1.2f, 2.75f, 3f), new Vector3(3f, 0.5f, 3f)); // 왼쪽 3미터 발판 생성
            CreateCube(root.transform, "EndPlatform", new Vector3(0f, 3.75f, 6f), new Vector3(4f, 0.5f, 3f)); // 4미터 종료 발판 생성
            CreateConnectionPoint(root.transform, "Entrance", new Vector3(0f, 0f, -7.5f), MapConnectionRole.Entrance, MapConnectionDirection.South); // 0미터 남쪽 입구 생성
            CreateConnectionPoint(root.transform, "Exit", new Vector3(0f, 4f, 7.5f), MapConnectionRole.Exit, MapConnectionDirection.North); // 4미터 북쪽 출구 생성
            MapVerticalTraversalSegment[] segments = CreateRepeatedSegments("ZigzagJump", 4, MapTraversalRequirement.Jump, 1f, 3.85f); // 네 지그재그 점프 이동 데이터 생성
            ConfigureModule(root, "MAP-007", MapModuleKind.ZigzagRise, MapTraversalRequirement.Jump, new Vector3(0f, 3f, 0f), new Vector3(5.4f, 6.5f, 15f), 2.2f, 3.85f, 1f, MapVerticalLayoutKind.ZigzagRise, 4f, segments, traversalProfile); // 지그재그 상승 모듈 데이터 적용
            SaveAndDestroyPrefab(root, ZigzagRisePrefabPath); // 지그재그 상승 Prefab 저장
        } // 지그재그 상승 Prefab 구성 처리 종료

        private static void CreateJumpRisePrefab(MapTraversalProfile traversalProfile) // 3미터 점프 상승 Prefab 구성
        { // 점프 상승 Prefab 구성 처리
            GameObject root = CreateModuleRoot("MAP-008_JumpRise"); // 점프 상승 모듈 루트 생성
            CreateCube(root.transform, "StartPlatform", new Vector3(0f, -0.25f, -6f), new Vector3(4f, 0.5f, 6f)); // 시작 점프 발판 생성
            CreateCube(root.transform, "MiddlePlatform", new Vector3(0f, 1.25f, 0f), new Vector3(4f, 0.5f, 3f)); // 중간 1.5미터 발판 생성
            CreateCube(root.transform, "EndPlatform", new Vector3(0f, 2.75f, 6f), new Vector3(4f, 0.5f, 6f)); // 종료 3미터 발판 생성
            CreateConnectionPoint(root.transform, "Entrance", new Vector3(0f, 0f, -9f), MapConnectionRole.Entrance, MapConnectionDirection.South); // 0미터 남쪽 입구 생성
            CreateConnectionPoint(root.transform, "Exit", new Vector3(0f, 3f, 9f), MapConnectionRole.Exit, MapConnectionDirection.North); // 3미터 북쪽 출구 생성
            MapVerticalTraversalSegment[] segments = CreateRepeatedSegments("Jump", 2, MapTraversalRequirement.Jump, 1.5f, 1.5f); // 두 점프 상승 이동 데이터 생성
            ConfigureModule(root, "MAP-008", MapModuleKind.JumpRise, MapTraversalRequirement.Jump, new Vector3(0f, 2.5f, 0f), new Vector3(4f, 5.5f, 18f), 2.2f, 1.5f, 1.5f, MapVerticalLayoutKind.JumpRise, 3f, segments, traversalProfile); // 점프 상승 모듈 데이터 적용
            SaveAndDestroyPrefab(root, JumpRisePrefabPath); // 점프 상승 Prefab 저장
        } // 점프 상승 Prefab 구성 처리 종료

        private static MapVerticalTraversalSegment[] CreateRepeatedSegments(string idPrefix, int count, MapTraversalRequirement requirement, float heightGain, float horizontalDistance) // 동일 규격 수직 이동 구간 배열 생성
        { // 수직 이동 구간 배열 생성 처리
            MapVerticalTraversalSegment[] segments = new MapVerticalTraversalSegment[count]; // 요청 개수 이동 구간 배열 생성

            for (int segmentIndex = 0; segmentIndex < count; segmentIndex++) // 모든 이동 구간 순회
            { // 단일 이동 구간 생성 처리
                string segmentId = $"{idPrefix}_{segmentIndex + 1:00}"; // 순번이 포함된 구간 ID 생성
                segments[segmentIndex] = new MapVerticalTraversalSegment(segmentId, requirement, heightGain, horizontalDistance); // 현재 이동 구간 데이터 저장
            } // 단일 이동 구간 생성 처리 종료

            return segments; // 완성된 이동 구간 배열 반환
        } // 수직 이동 구간 배열 생성 처리 종료

        private static GameObject CreateModuleRoot(string rootName) // 수직 모듈 루트 오브젝트 생성
        { // 수직 모듈 루트 생성 처리
            GameObject root = new GameObject(rootName); // 빈 수직 모듈 루트 생성
            root.layer = ResolveGroundLayer(); // Ground 레이어 적용
            root.AddComponent<MapModuleDefinition>(); // 기본 모듈 정의 컴포넌트 추가
            root.AddComponent<MapVerticalModuleData>(); // 수직 모듈 데이터 컴포넌트 추가
            return root; // 생성된 수직 모듈 루트 반환
        } // 수직 모듈 루트 생성 처리 종료

        private static GameObject CreateCube(Transform parent, string objectName, Vector3 localPosition, Vector3 localScale) // 수직 모듈용 Cube 생성
        { // 수직 모듈용 Cube 생성 처리
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube); // 기본 Cube 오브젝트 생성
            cube.name = objectName; // Cube 이름 적용
            cube.transform.SetParent(parent, false); // 모듈 루트 자식으로 설정
            cube.transform.localPosition = localPosition; // 로컬 위치 적용
            cube.transform.localRotation = Quaternion.identity; // 로컬 회전 초기화
            cube.transform.localScale = localScale; // 로컬 크기 적용
            cube.layer = ResolveGroundLayer(); // Ground 레이어 적용
            return cube; // 생성된 Cube 반환
        } // 수직 모듈용 Cube 생성 처리 종료

        private static MapModuleConnectionPoint CreateConnectionPoint(Transform parent, string pointName, Vector3 localPosition, MapConnectionRole role, MapConnectionDirection direction) // 수직 입구 또는 출구 연결 지점 생성
        { // 수직 연결 지점 생성 처리
            GameObject pointObject = new GameObject(pointName); // 빈 연결 지점 오브젝트 생성
            pointObject.transform.SetParent(parent, false); // 모듈 루트 자식으로 설정
            pointObject.transform.localPosition = localPosition; // 로컬 연결 위치 적용
            pointObject.transform.localRotation = Quaternion.identity; // 로컬 연결 회전 초기화
            MapModuleConnectionPoint point = pointObject.AddComponent<MapModuleConnectionPoint>(); // 연결 지점 컴포넌트 추가
            point.ConfigureForEditor(pointName, role, direction, ConnectionWidth, ConnectionHeight); // 연결 지점 공통 규격 적용
            return point; // 생성된 연결 지점 반환
        } // 수직 연결 지점 생성 처리 종료

        private static void ConfigureModule(GameObject root, string moduleId, MapModuleKind moduleKind, MapTraversalRequirement requirement, Vector3 boundsCenter, Vector3 boundsSize, float clearanceHeight, float jumpDistance, float jumpRise, MapVerticalLayoutKind layoutKind, float expectedHeightGain, MapVerticalTraversalSegment[] segments, MapTraversalProfile traversalProfile) // 수직 모듈 공통 데이터 적용
        { // 수직 모듈 공통 데이터 적용 처리
            MapModuleDefinition definition = root.GetComponent<MapModuleDefinition>(); // 기본 모듈 정의 조회
            MapVerticalModuleData verticalData = root.GetComponent<MapVerticalModuleData>(); // 수직 모듈 데이터 조회
            definition.ConfigureForEditor(moduleId, moduleKind, requirement, MapRotationOptions.All, boundsCenter, boundsSize, clearanceHeight, jumpDistance, jumpRise, traversalProfile); // 기본 모듈 데이터 설정
            definition.RefreshConnectionPoints(); // 생성된 입구와 출구 목록 수집
            verticalData.ConfigureForEditor(layoutKind, "Entrance", "Exit", expectedHeightGain, segments, traversalProfile); // 수직 연결과 이동 구간 데이터 설정

            if (!definition.TryValidate(out string moduleReason)) // 기본 모듈 데이터 유효성 확인
            { // 기본 모듈 데이터 오류 처리
                Debug.LogError($"[ProjectJ][Day34] {moduleId} 기본 검증 실패: {moduleReason}", root); // 기본 모듈 검증 오류 출력
            } // 기본 모듈 데이터 오류 처리 종료

            if (!verticalData.TryValidate(out string verticalReason)) // 수직 모듈 데이터 유효성 확인
            { // 수직 모듈 데이터 오류 처리
                Debug.LogError($"[ProjectJ][Day34] {moduleId} 수직 검증 실패: {verticalReason}", root); // 수직 모듈 검증 오류 출력
            } // 수직 모듈 데이터 오류 처리 종료
        } // 수직 모듈 공통 데이터 적용 처리 종료

        private static void SaveAndDestroyPrefab(GameObject root, string prefabPath) // 임시 루트를 Prefab으로 저장하고 정리
        { // Prefab 저장과 정리 처리
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath); // 지정 경로에 Prefab 저장 또는 갱신
            Object.DestroyImmediate(root); // Scene에 남은 임시 루트 제거
        } // Prefab 저장과 정리 처리 종료

        private static int ResolveGroundLayer() // Ground 레이어 또는 안전 기본값 조회
        { // Ground 레이어 조회 처리
            int groundLayer = LayerMask.NameToLayer("Ground"); // Ground 레이어 번호 조회
            return groundLayer >= 0 ? groundLayer : 9; // Ground 또는 기존 기본 번호 반환
        } // Ground 레이어 조회 처리 종료

        private static void EnsureFolderExists(string folderPath) // 지정된 Unity 에셋 폴더 존재 보장
        { // 에셋 폴더 생성 처리
            if (AssetDatabase.IsValidFolder(folderPath)) // 대상 폴더 존재 확인
            { // 기존 폴더 처리
                return; // 폴더 생성 생략
            } // 기존 폴더 처리 종료

            string[] pathParts = folderPath.Split('/'); // 전체 경로를 단계별 이름으로 분리
            string currentPath = pathParts[0]; // Assets 루트 경로 저장

            for (int pathIndex = 1; pathIndex < pathParts.Length; pathIndex++) // 하위 폴더 단계 순회
            { // 하위 폴더 생성 처리
                string nextPath = $"{currentPath}/{pathParts[pathIndex]}"; // 다음 단계 전체 경로 계산

                if (!AssetDatabase.IsValidFolder(nextPath)) // 다음 단계 폴더 누락 확인
                { // 누락 폴더 생성 처리
                    AssetDatabase.CreateFolder(currentPath, pathParts[pathIndex]); // 현재 경로 아래에 폴더 생성
                } // 누락 폴더 생성 처리 종료

                currentPath = nextPath; // 현재 경로를 다음 단계로 갱신
            } // 하위 폴더 생성 처리 종료
        } // 에셋 폴더 생성 처리 종료
    } // 34일차 수직 상승 모듈 구성 도구 묶음 종료
} // 프로젝트 Editor 기능 묶음 종료
