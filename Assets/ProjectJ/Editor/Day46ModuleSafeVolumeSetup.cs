using ProjectJ.Map; // 맵 시스템 사용
using UnityEditor; // 유니티 에디터 기능 사용
using UnityEngine; // 유니티 기능 사용

namespace ProjectJ.Editor // 프로젝트 에디터 네임스페이스
{
    public static class Day46ModuleSafeVolumeSetup // 46일차 Safe Volume 설정 도구
    {
        private const string ModulePrefabFolder = "Assets/ProjectJ/Prefabs/Map/Modules/Day25"; // 25일차 Module 폴더
        private const string SafeVolumeName = "Day46_SafeVolume_Main"; // 기본 설치 가능 영역 이름
        private const string NoSpawnPrefix = "Day46_NoSpawn_"; // 자동 금지 영역 이름 접두사
        private const float InteriorMargin = 1f; // Module 내부 여백
        private const float SocketReserveWidth = 6f; // Socket 금지 영역 폭
        private const float SocketReserveHeight = 6f; // Socket 금지 영역 높이
        private const float SocketReserveDepth = 4f; // Socket 금지 영역 깊이

        [MenuItem("ProjectJ/Day46/Setup Module Safe Volumes")] // 자동 설정 메뉴 등록
        public static void SetupModuleSafeVolumes() // 모든 Day25 Module 설정
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new string[] { ModulePrefabFolder }); // Module Prefab 검색
            int updatedCount = 0; // 수정 Prefab 수 초기화

            for (int i = 0; i < prefabGuids.Length; i++) // 모든 Prefab 반복
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuids[i]); // GUID 경로 변환

                if (SetupPrefab(prefabPath)) // Prefab 설정 성공 검사
                {
                    updatedCount++; // 수정 수 증가
                }
            }

            AssetDatabase.SaveAssets(); // 변경 에셋 저장
            AssetDatabase.Refresh(); // 에셋 데이터 갱신
            Debug.Log($"Day46 장애물 Safe Volume 설정 완료: {updatedCount}개 Module"); // 완료 로그 출력
        }

        [MenuItem("GameObject/ProjectJ/Day46/Add Safe Volume", false, 10)] // Safe Volume 추가 메뉴 등록
        public static void AddSafeVolumeToSelection() // 선택 오브젝트에 Safe Volume 추가
        {
            CreateVolumeForSelection(MapObstacleVolumeType.Safe, "SafeVolume"); // 설치 가능 영역 생성
        }

        [MenuItem("GameObject/ProjectJ/Day46/Add No Spawn Volume", false, 11)] // No Spawn Volume 추가 메뉴 등록
        public static void AddNoSpawnVolumeToSelection() // 선택 오브젝트에 No Spawn Volume 추가
        {
            CreateVolumeForSelection(MapObstacleVolumeType.NoSpawn, "NoSpawnVolume"); // 설치 금지 영역 생성
        }

        private static bool SetupPrefab(string prefabPath) // 단일 Prefab 설정
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath); // Prefab 편집 인스턴스 열기

            if (prefabRoot == null) // Prefab 로드 실패 검사
            {
                return false; // 설정 실패 반환
            }

            try // Prefab 안전 처리 시작
            {
                MapModule module = prefabRoot.GetComponent<MapModule>(); // Module 컴포넌트 찾기

                if (module == null) // Module 누락 검사
                {
                    return false; // 대상 아님 반환
                }

                Transform obstacleSpawnRoot = prefabRoot.transform.Find("Gameplay/ObstacleSpawnAreas"); // 설치 가능 영역 루트 찾기
                Transform noSpawnRoot = prefabRoot.transform.Find("Gameplay/NoSpawnAreas"); // 설치 금지 영역 루트 찾기

                if (obstacleSpawnRoot == null || noSpawnRoot == null) // Gameplay 루트 누락 검사
                {
                    Debug.LogWarning($"Day46 Gameplay 영역 루트 누락: {prefabPath}"); // 누락 경고 출력
                    return false; // 설정 실패 반환
                }

                RemoveGeneratedChildren(obstacleSpawnRoot, SafeVolumeName); // 기존 자동 Safe Volume 제거
                RemoveGeneratedChildren(noSpawnRoot, NoSpawnPrefix); // 기존 자동 No Spawn 제거
                CreateDefaultSafeVolume(module, obstacleSpawnRoot); // 기본 Safe Volume 생성
                CreateSocketNoSpawnVolumes(module, noSpawnRoot); // Socket 금지 영역 생성
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath); // Prefab 변경 저장
                return true; // 설정 성공 반환
            }
            finally // Prefab 정리 처리
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot); // Prefab 편집 인스턴스 닫기
            }
        }

        private static void CreateDefaultSafeVolume(MapModule module, Transform parent) // 기본 설치 가능 영역 생성
        {
            GameObject volumeObject = new GameObject(SafeVolumeName); // Safe Volume 오브젝트 생성
            volumeObject.transform.SetParent(parent, false); // 지정 루트에 배치
            volumeObject.transform.localPosition = Vector3.zero; // Module 중심 배치
            volumeObject.transform.localRotation = Quaternion.identity; // 기본 회전 적용
            float safeSize = Mathf.Max(1f, module.ModuleSize - InteriorMargin * 2f); // 내부 여백 적용 크기 계산
            volumeObject.transform.localScale = new Vector3(safeSize, safeSize, safeSize); // 기본 설치 가능 영역 크기 적용
            MapObstaclePlacementVolume volume = volumeObject.AddComponent<MapObstaclePlacementVolume>(); // 영역 컴포넌트 추가
            volume.Configure(MapObstacleVolumeType.Safe, true); // 설치 가능 영역 설정
        }

        private static void CreateSocketNoSpawnVolumes(MapModule module, Transform parent) // Socket 주변 금지 영역 생성
        {
            for (int i = 0; i < module.Sockets.Count; i++) // 모든 Socket 반복
            {
                MapModuleSocket socket = module.Sockets[i]; // 현재 Socket 저장

                if (socket == null || socket.State == MapModuleFaceState.Closed) // 닫힌 Socket 제외 검사
                {
                    continue; // 다음 Socket 진행
                }

                GameObject volumeObject = new GameObject(NoSpawnPrefix + socket.Direction); // Socket 금지 영역 생성
                volumeObject.transform.SetParent(parent, true); // 금지 영역 루트에 배치
                volumeObject.transform.position = socket.transform.position - socket.transform.forward * (SocketReserveDepth * 0.5f); // Module 안쪽으로 중심 이동
                volumeObject.transform.rotation = socket.transform.rotation; // Socket 방향 적용
                volumeObject.transform.localScale = new Vector3(SocketReserveWidth, SocketReserveHeight, SocketReserveDepth); // 금지 영역 크기 적용
                MapObstaclePlacementVolume volume = volumeObject.AddComponent<MapObstaclePlacementVolume>(); // 영역 컴포넌트 추가
                volume.Configure(MapObstacleVolumeType.NoSpawn, true); // 설치 금지 영역 설정
            }
        }

        private static void RemoveGeneratedChildren(Transform parent, string namePrefix) // 자동 생성 영역 정리
        {
            for (int i = parent.childCount - 1; i >= 0; i--) // 자식 역순 반복
            {
                Transform child = parent.GetChild(i); // 현재 자식 저장

                if (!child.name.StartsWith(namePrefix)) // 자동 생성 이름 검사
                {
                    continue; // 사용자 영역 유지
                }

                Object.DestroyImmediate(child.gameObject); // 기존 자동 영역 삭제
            }
        }

        private static void CreateVolumeForSelection(MapObstacleVolumeType type, string defaultName) // 선택 위치에 새 영역 생성
        {
            Transform parent = Selection.activeTransform; // 현재 선택 Transform 저장

            if (parent == null) // 선택 대상 누락 검사
            {
                Debug.LogWarning("Day46 Volume을 추가할 부모 오브젝트를 먼저 선택하세요."); // 선택 안내 출력
                return; // 생성 중단
            }

            GameObject volumeObject = new GameObject(defaultName); // 새 영역 오브젝트 생성
            Undo.RegisterCreatedObjectUndo(volumeObject, "Create Day46 Placement Volume"); // Undo 기록 등록
            volumeObject.transform.SetParent(parent, false); // 선택 오브젝트 하위 배치
            volumeObject.transform.localPosition = Vector3.zero; // 부모 중심 배치
            volumeObject.transform.localRotation = Quaternion.identity; // 기본 회전 적용
            volumeObject.transform.localScale = new Vector3(4f, 4f, 4f); // 편집용 기본 크기 적용
            MapObstaclePlacementVolume volume = volumeObject.AddComponent<MapObstaclePlacementVolume>(); // 영역 컴포넌트 추가
            volume.Configure(type, true); // 영역 종류 설정
            Selection.activeGameObject = volumeObject; // 생성 영역 선택
        }
    }
}
