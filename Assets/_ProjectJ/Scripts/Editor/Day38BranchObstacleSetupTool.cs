using System.Collections.Generic; // 중복 Prefab 경로 집합 기능 참조
using ProjectJ.Data; // 장애물 데이터와 버전 형식 참조
using ProjectJ.MapGeneration; // 맵 생성과 장애물 계획 기능 참조
using UnityEditor; // Unity Editor 메뉴와 에셋 기능 참조
using UnityEditor.SceneManagement; // Scene 변경 상태 기능 참조
using UnityEngine; // Unity 오브젝트 생성과 로그 기능 참조

namespace ProjectJ.Editor // 프로젝트 Editor 기능 네임스페이스 선언
{ // 프로젝트 Editor 기능 묶음
    internal static class Day38BranchObstacleSetupTool // 38일차 분기 장애물 설정 도구 선언
    { // 38일차 분기 장애물 설정 도구 묶음
        private const string MenuPath = "Project J/Day 38/Configure Branch Obstacles"; // 분기 장애물 구성 메뉴 경로
        private const string ObstacleDataFolder = "Assets/_ProjectJ/Data/Definitions/Obstacle"; // 장애물 데이터 에셋 폴더 경로
        private const string ObstaclePrefabFolder = "Assets/_ProjectJ/Prefabs/Obstacles"; // 장애물 Prefab 폴더 경로
        private const string ObstacleDataPath = ObstacleDataFolder + "/OBS-038_PrototypeBlock.asset"; // 38일차 장애물 데이터 경로
        private const string ObstaclePrefabPath = ObstaclePrefabFolder + "/OBS-038_PrototypeBlock.prefab"; // 38일차 장애물 Prefab 경로

        [MenuItem(MenuPath)] // Unity 상단 메뉴에 38일차 구성 항목 등록
        private static void ConfigureBranchObstacles() // 장애물 데이터와 모듈 지점과 생성기 계획 구성
        { // 분기 장애물 설정 처리
            ProceduralMapGenerator generator = Object.FindFirstObjectByType<ProceduralMapGenerator>(); // 현재 Scene의 절차적 맵 생성기 조회

            if (generator == null) // 절차적 맵 생성기 누락 확인
            { // 절차적 맵 생성기 누락 처리
                Debug.LogError("[ProjectJ][Day38] 현재 Scene에서 ProceduralMapGenerator를 찾지 못했습니다. Game Scene과 생성기 오브젝트를 확인하세요."); // 생성기 누락 안내 오류 출력
                return; // 분기 장애물 설정 중단
            } // 절차적 맵 생성기 누락 처리 종료

            EnsureFolderExists(ObstacleDataFolder); // 장애물 데이터 폴더 존재 보장
            EnsureFolderExists(ObstaclePrefabFolder); // 장애물 Prefab 폴더 존재 보장
            GameObject obstaclePrefab = CreatePrototypeObstaclePrefab(); // 38일차 임시 장애물 Prefab 생성 또는 조회
            ObstacleDataDefinition obstacleData = CreateOrUpdateObstacleData(obstaclePrefab); // 임시 장애물 데이터 생성 또는 갱신
            MapGenerationSettings settings = FindGenerationSettings(generator); // 생성기에 연결된 맵 생성 설정 조회

            if (settings == null) // 맵 생성 설정 누락 확인
            { // 맵 생성 설정 누락 처리
                Debug.LogError("[ProjectJ][Day38] ProceduralMapGenerator에 연결된 MapGenerationSettings를 찾지 못했습니다.", generator); // 생성 설정 누락 안내 오류 출력
                return; // 분기 장애물 설정 중단
            } // 맵 생성 설정 누락 처리 종료

            int configuredPrefabCount = ConfigureModuleSpawnPoints(settings); // 생성 후보 모듈 Prefab의 안전 배치 지점 구성
            MapBranchObstaclePlanner planner = generator.GetComponent<MapBranchObstaclePlanner>(); // 기존 분기 장애물 계획기 조회

            if (planner == null) // 분기 장애물 계획기 누락 확인
            { // 분기 장애물 계획기 추가 처리
                planner = Undo.AddComponent<MapBranchObstaclePlanner>(generator.gameObject); // Undo 가능한 분기 장애물 계획기 추가
            } // 분기 장애물 계획기 추가 처리 종료

            Undo.RecordObject(planner, "Configure Day 38 Branch Obstacle Planner"); // 장애물 계획 설정 변경 Undo 기록
            planner.ConfigureForEditor(generator, true, new[] { obstacleData }, 6, 12, 18, 30, 8, 2); // 안전과 고위험 경로 기본 위험도 설정 적용
            EditorUtility.SetDirty(planner); // 장애물 계획기 변경 상태 표시
            MapObstacleDebugVisualizer visualizer = generator.GetComponent<MapObstacleDebugVisualizer>(); // 기존 장애물 디버그 시각화 조회

            if (visualizer == null) // 장애물 디버그 시각화 누락 확인
            { // 장애물 디버그 시각화 추가 처리
                visualizer = Undo.AddComponent<MapObstacleDebugVisualizer>(generator.gameObject); // Undo 가능한 장애물 디버그 시각화 추가
            } // 장애물 디버그 시각화 추가 처리 종료

            Undo.RecordObject(visualizer, "Configure Day 38 Obstacle Debug Visualizer"); // 장애물 시각화 설정 변경 Undo 기록
            visualizer.ConfigureForEditor(generator, planner, true, true, 0.18f); // 선택 중 표시와 문자 기본값 적용
            EditorUtility.SetDirty(visualizer); // 장애물 시각화 변경 상태 표시
            AssetDatabase.SaveAssets(); // 생성과 수정된 에셋 저장
            AssetDatabase.Refresh(); // Project 창 에셋 목록 새로고침
            generator.GenerateMap(); // 새 배치 지점을 포함한 맵과 장애물 즉시 재생성
            EditorSceneManager.MarkSceneDirty(generator.gameObject.scene); // 현재 Scene 저장 필요 상태 표시
            Selection.activeGameObject = generator.gameObject; // 생성기 오브젝트 선택
            EditorGUIUtility.PingObject(generator.gameObject); // Hierarchy에서 생성기 오브젝트 강조
            Debug.Log($"[ProjectJ][Day38] 분기 장애물 설정을 완료했습니다. 모듈 Prefab {configuredPrefabCount}개에 좌우 배치 지점을 구성하고 안전 6~12, 고위험 18~30 기준으로 장애물을 생성했습니다.", generator); // 38일차 설정 완료 로그 출력
        } // 분기 장애물 설정 처리 종료

        [MenuItem(MenuPath, true)] // 38일차 구성 메뉴 활성 조건 등록
        private static bool ValidateConfigureBranchObstacles() // Play Mode가 아닐 때만 메뉴 실행 허용
        { // 메뉴 실행 조건 검사 처리
            return !EditorApplication.isPlayingOrWillChangePlaymode; // Edit Mode 실행 가능 여부 반환
        } // 메뉴 실행 조건 검사 처리 종료

        private static MapGenerationSettings FindGenerationSettings(ProceduralMapGenerator generator) // 생성기가 사용하는 맵 생성 설정 조회
        { // 맵 생성 설정 조회 처리
            SerializedObject serializedGenerator = new SerializedObject(generator); // 생성기 직렬화 표현 생성
            SerializedProperty settingsProperty = serializedGenerator.FindProperty("settings"); // 비공개 생성 설정 필드 조회
            return settingsProperty != null ? settingsProperty.objectReferenceValue as MapGenerationSettings : null; // 연결된 생성 설정 반환
        } // 맵 생성 설정 조회 처리 종료

        private static GameObject CreatePrototypeObstaclePrefab() // 38일차 임시 장애물 Prefab 생성 또는 조회
        { // 임시 장애물 Prefab 생성 처리
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ObstaclePrefabPath); // 기존 임시 장애물 Prefab 조회

            if (existingPrefab != null) // 기존 임시 장애물 Prefab 존재 확인
            { // 기존 임시 장애물 Prefab 처리
                return existingPrefab; // 기존 Prefab 그대로 반환
            } // 기존 임시 장애물 Prefab 처리 종료

            GameObject prototypeObject = GameObject.CreatePrimitive(PrimitiveType.Cube); // Collider가 포함된 임시 큐브 생성
            prototypeObject.name = "OBS-038_PrototypeBlock"; // 임시 장애물 오브젝트 이름 적용
            prototypeObject.transform.localScale = new Vector3(0.8f, 1f, 0.8f); // 통로를 완전히 막지 않는 임시 크기 적용
            int obstacleLayer = LayerMask.NameToLayer("Obstacle"); // 프로젝트 장애물 레이어 번호 조회

            if (obstacleLayer >= 0) // 장애물 레이어 존재 확인
            { // 장애물 레이어 적용 처리
                prototypeObject.layer = obstacleLayer; // 임시 장애물에 장애물 레이어 적용
            } // 장애물 레이어 적용 처리 종료

            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(prototypeObject, ObstaclePrefabPath); // 임시 장애물 Prefab 에셋 저장
            Object.DestroyImmediate(prototypeObject); // 임시 Scene 오브젝트 제거
            return savedPrefab; // 저장된 임시 장애물 Prefab 반환
        } // 임시 장애물 Prefab 생성 처리 종료

        private static ObstacleDataDefinition CreateOrUpdateObstacleData(GameObject obstaclePrefab) // 38일차 장애물 데이터 생성 또는 갱신
        { // 장애물 데이터 생성 또는 갱신 처리
            ObstacleDataDefinition obstacleData = AssetDatabase.LoadAssetAtPath<ObstacleDataDefinition>(ObstacleDataPath); // 기존 38일차 장애물 데이터 조회

            if (obstacleData == null) // 38일차 장애물 데이터 누락 확인
            { // 38일차 장애물 데이터 생성 처리
                obstacleData = ScriptableObject.CreateInstance<ObstacleDataDefinition>(); // 새 장애물 데이터 인스턴스 생성
                AssetDatabase.CreateAsset(obstacleData, ObstacleDataPath); // 새 장애물 데이터 에셋 저장
            } // 38일차 장애물 데이터 생성 처리 종료

            obstacleData.SetEditorIdentity("OBS-038", "Prototype Branch Block", new ProjectDataVersion(1, 0, 0)); // 장애물 영구 ID와 표시 이름 설정
            obstacleData.ConfigureObstacleForEditor(obstaclePrefab, 6, new Vector3(0.8f, 1f, 0.8f), true, ObstacleTraversalEffect.Slow); // 임시 장애물 위험도와 점유 크기 설정
            EditorUtility.SetDirty(obstacleData); // 장애물 데이터 변경 상태 표시
            return obstacleData; // 구성된 장애물 데이터 반환
        } // 장애물 데이터 생성 또는 갱신 처리 종료

        private static int ConfigureModuleSpawnPoints(MapGenerationSettings settings) // 생성 후보 모듈 Prefab에 장애물 배치 지점 구성
        { // 모듈 배치 지점 구성 처리
            MapModuleDefinition[] modulePrefabs = settings.ModulePrefabs; // 생성 설정의 모듈 Prefab 목록 조회
            HashSet<string> configuredPaths = new HashSet<string>(); // 중복 Prefab 경로 방지 집합 생성
            int configuredCount = 0; // 구성 완료 Prefab 수 초기화

            if (modulePrefabs == null) // 모듈 Prefab 목록 누락 확인
            { // 모듈 Prefab 목록 누락 처리
                return configuredCount; // 구성 수 0 반환
            } // 모듈 Prefab 목록 누락 처리 종료

            for (int prefabIndex = 0; prefabIndex < modulePrefabs.Length; prefabIndex++) // 모든 생성 후보 Prefab 순회
            { // 단일 모듈 Prefab 배치 지점 구성 처리
                MapModuleDefinition modulePrefab = modulePrefabs[prefabIndex]; // 현재 모듈 Prefab 조회

                if (modulePrefab == null || modulePrefab.ModuleKind == MapModuleKind.Branch || modulePrefab.ModuleKind == MapModuleKind.Merge) // 장애물 지점 제외 모듈 확인
                { // 장애물 지점 제외 모듈 처리
                    continue; // 현재 모듈 Prefab 제외
                } // 장애물 지점 제외 모듈 처리 종료

                string prefabPath = AssetDatabase.GetAssetPath(modulePrefab.gameObject); // 현재 모듈 Prefab 에셋 경로 조회

                if (string.IsNullOrWhiteSpace(prefabPath) || !configuredPaths.Add(prefabPath)) // 잘못됐거나 이미 처리한 Prefab 경로 확인
                { // 잘못됐거나 중복 Prefab 경로 처리
                    continue; // 현재 모듈 Prefab 제외
                } // 잘못됐거나 중복 Prefab 경로 처리 종료

                ConfigureSinglePrefabSpawnPoints(prefabPath); // 현재 모듈 Prefab의 좌우 배치 지점 구성
                configuredCount++; // 구성 완료 Prefab 수 증가
            } // 단일 모듈 Prefab 배치 지점 구성 처리 종료

            return configuredCount; // 구성 완료 Prefab 수 반환
        } // 모듈 배치 지점 구성 처리 종료

        private static void ConfigureSinglePrefabSpawnPoints(string prefabPath) // 단일 모듈 Prefab의 장애물 배치 지점 구성
        { // 단일 Prefab 배치 지점 구성 처리
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath); // 편집 가능한 Prefab 내용 로드

            try // Prefab 내용 안전 편집 시작
            { // Prefab 내용 안전 편집 처리
                MapModuleDefinition module = prefabRoot.GetComponentInChildren<MapModuleDefinition>(true); // Prefab의 모듈 정의 조회

                if (module == null) // 모듈 정의 누락 확인
                { // 모듈 정의 누락 처리
                    return; // 현재 Prefab 구성 생략
                } // 모듈 정의 누락 처리 종료

                Transform pointsRoot = FindOrCreateChild(module.transform, "Day38_ObstaclePoints"); // 장애물 지점 보관 부모 조회 또는 생성
                float horizontalOffset = Mathf.Max(0.65f, module.BoundsSize.x * 0.32f); // 중앙 통로를 피한 좌우 간격 계산
                float localFloorY = module.BoundsCenter.y - module.BoundsSize.y * 0.5f + 0.5f; // 임시 장애물 중심 높이 계산
                ConfigureSpawnPoint(pointsRoot, "ObstaclePoint_Left", new Vector3(-horizontalOffset, localFloorY, 0f), 1.2f); // 모듈 왼쪽 배치 지점 구성
                ConfigureSpawnPoint(pointsRoot, "ObstaclePoint_Right", new Vector3(horizontalOffset, localFloorY, 0f), 0.9f); // 모듈 오른쪽 배치 지점 구성
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath); // 수정된 Prefab 내용 저장
            } // Prefab 내용 안전 편집 처리 종료
            finally // Prefab 내용 해제 보장
            { // Prefab 내용 해제 처리
                PrefabUtility.UnloadPrefabContents(prefabRoot); // 편집용 Prefab 내용 해제
            } // Prefab 내용 해제 처리 종료
        } // 단일 Prefab 배치 지점 구성 처리 종료

        private static Transform FindOrCreateChild(Transform parent, string childName) // 지정 이름의 직접 자식 조회 또는 생성
        { // 직접 자식 조회 또는 생성 처리
            Transform child = parent.Find(childName); // 지정 이름의 기존 직접 자식 조회

            if (child != null) // 기존 직접 자식 존재 확인
            { // 기존 직접 자식 처리
                return child; // 기존 직접 자식 반환
            } // 기존 직접 자식 처리 종료

            GameObject childObject = new GameObject(childName); // 새 빈 자식 오브젝트 생성
            childObject.transform.SetParent(parent, false); // 지정 부모 아래 로컬 좌표로 배치
            return childObject.transform; // 새 자식 Transform 반환
        } // 직접 자식 조회 또는 생성 처리 종료

        private static void ConfigureSpawnPoint(Transform parent, string pointName, Vector3 localPosition, float riskMultiplier) // 단일 장애물 배치 지점 생성 또는 갱신
        { // 단일 장애물 배치 지점 구성 처리
            Transform pointTransform = FindOrCreateChild(parent, pointName); // 배치 지점 자식 조회 또는 생성
            pointTransform.localPosition = localPosition; // 배치 지점 로컬 위치 적용
            pointTransform.localRotation = Quaternion.identity; // 배치 지점 로컬 회전 초기화
            pointTransform.localScale = Vector3.one; // 배치 지점 로컬 배율 초기화
            MapObstacleSpawnPoint spawnPoint = pointTransform.GetComponent<MapObstacleSpawnPoint>(); // 기존 배치 지점 컴포넌트 조회

            if (spawnPoint == null) // 배치 지점 컴포넌트 누락 확인
            { // 배치 지점 컴포넌트 추가 처리
                spawnPoint = pointTransform.gameObject.AddComponent<MapObstacleSpawnPoint>(); // 새 배치 지점 컴포넌트 추가
            } // 배치 지점 컴포넌트 추가 처리 종료

            spawnPoint.ConfigureForEditor(pointName, 1.2f, 3f, 1.1f, riskMultiplier, true); // 통로 폭과 위험도 배율 기본값 적용
            EditorUtility.SetDirty(spawnPoint); // 배치 지점 변경 상태 표시
        } // 단일 장애물 배치 지점 구성 처리 종료

        private static void EnsureFolderExists(string folderPath) // 지정 Unity 에셋 폴더 존재 보장
        { // 에셋 폴더 존재 보장 처리
            if (AssetDatabase.IsValidFolder(folderPath)) // 전체 폴더 이미 존재 확인
            { // 전체 폴더 이미 존재 처리
                return; // 폴더 생성 생략
            } // 전체 폴더 이미 존재 처리 종료

            string[] pathParts = folderPath.Split('/'); // 전체 폴더 경로 단계별 분리
            string currentPath = pathParts[0]; // 첫 Assets 경로 저장

            for (int partIndex = 1; partIndex < pathParts.Length; partIndex++) // 모든 하위 폴더 단계 순회
            { // 단일 하위 폴더 생성 처리
                string nextPath = $"{currentPath}/{pathParts[partIndex]}"; // 다음 단계 전체 경로 생성

                if (!AssetDatabase.IsValidFolder(nextPath)) // 다음 단계 폴더 누락 확인
                { // 다음 단계 폴더 생성 처리
                    AssetDatabase.CreateFolder(currentPath, pathParts[partIndex]); // 누락된 하위 폴더 생성
                } // 다음 단계 폴더 생성 처리 종료

                currentPath = nextPath; // 현재 경로를 다음 단계로 갱신
            } // 단일 하위 폴더 생성 처리 종료
        } // 에셋 폴더 존재 보장 처리 종료
    } // 38일차 분기 장애물 설정 도구 묶음 종료
} // 프로젝트 Editor 기능 묶음 종료
