using System; // 공통 C# 기능 사용
using System.Collections.Generic; // 모듈 규격 목록 기능 사용
using ProjectJ.Map; // 10m 맵 모듈 기능 사용
using ProjectJ.Obstacles; // 기존 AirBag 장애물 기능 사용
using ProjectJ.Platforms; // 기존 발판 기능 사용
using UnityEditor; // Unity Editor 자산 생성 기능 사용
using UnityEngine; // Unity 오브젝트와 수학 기능 사용

namespace ProjectJ.Editor // Project J Editor 기능 네임스페이스
{
    public static class Day145ObstacleModuleSetup // 145일차 기존 발판·장애물 모듈 생성 도구
    {
        private const float ModuleSize = 10f; // 정육면체 모듈 한 변 크기
        private const float FloorThickness = 0.4f; // 기본 바닥 두께
        private const float FloorCenterY = -4.8f; // 기본 바닥 중심 높이
        private const float FloorTopY = -4.6f; // 기본 바닥 윗면 높이
        private const int ExpectedModuleCount = 6; // 생성 대상 모듈 개수
        private const string PrefabSearchRoot = "Assets/ProjectJ/Prefabs"; // 기존 Prefab 검색 루트
        private const string Day145RootPath = "Assets/ProjectJ/Prefabs/Map/Modules/Day145"; // Day145 생성 Prefab 경로

        private static readonly MapModuleFaceDirection[] AllDirections = // 6방향 Socket 순서
        {
            MapModuleFaceDirection.North, // 북쪽 방향
            MapModuleFaceDirection.South, // 남쪽 방향
            MapModuleFaceDirection.East, // 동쪽 방향
            MapModuleFaceDirection.West, // 서쪽 방향
            MapModuleFaceDirection.Up, // 위쪽 방향
            MapModuleFaceDirection.Down // 아래쪽 방향
        }; // 6방향 Socket 순서 종료

        private static readonly ModuleSpec[] ModuleSpecs = // 생성할 기존 기능 기반 모듈 목록
        {
            new ModuleSpec("PJ145_Module_MovingPlatform_SouthNorth", ModuleFeature.MovingPlatform), // 이동 발판 모듈 규격
            new ModuleSpec("PJ145_Module_RotatingPlatform_SouthNorth", ModuleFeature.RotatingPlatform), // 회전 발판 모듈 규격
            new ModuleSpec("PJ145_Module_GhostPlatform_SouthNorth", ModuleFeature.GhostPlatform), // 유령 발판 모듈 규격
            new ModuleSpec("PJ145_Module_IceSurface_SouthNorth", ModuleFeature.IceSurface), // 빙판 모듈 규격
            new ModuleSpec("PJ145_Module_SpringPlatform_SouthNorth", ModuleFeature.SpringPlatform), // 스프링 발판 모듈 규격
            new ModuleSpec("PJ145_Module_AirBag_SouthNorth", ModuleFeature.AirBag) // 에어백 장애물 모듈 규격
        }; // 생성 대상 목록 종료

        [MenuItem("ProjectJ/Day145/1. Rebuild Existing Platform Obstacle Modules")] // 전체 재생성 메뉴 등록
        public static void RebuildExistingPlatformObstacleModules() // 기존 기능을 10m 모듈로 재생성
        {
            DeleteDay145ModulesInternal(); // 기존 Day145 생성물 정리
            EnsureFolder(Day145RootPath); // Day145 출력 폴더 생성

            int reusedPrefabCount = 0; // 기존 Prefab 재사용 개수 초기화
            int fallbackCount = 0; // 기능 기반 대체 생성 개수 초기화

            for (int index = 0; index < ModuleSpecs.Length; index++) // 생성 규격 전체 순회
            {
                ModuleSpec spec = ModuleSpecs[index]; // 현재 생성 규격 조회
                bool reusedExistingPrefab = CreateModulePrefab(spec); // 모듈 생성과 기존 Prefab 재사용 여부 조회

                if (reusedExistingPrefab) // 기존 Prefab 재사용 여부 검사
                {
                    reusedPrefabCount++; // 기존 Prefab 재사용 개수 증가
                }
                else // 기존 Prefab을 직접 재사용하지 못한 경우 처리
                {
                    fallbackCount++; // 기능 기반 대체 생성 개수 증가
                }
            }

            AssetDatabase.SaveAssets(); // 생성 자산 저장
            AssetDatabase.Refresh(); // Project 창 갱신
            ValidateDay145Modules(); // 생성 결과 자동 검증
            Debug.Log("[ProjectJ][Day145] 장애물 모듈 6종 생성 완료. 기존 Prefab 직접 재사용: " + reusedPrefabCount + " / 기존 Runtime 기능 기반 생성: " + fallbackCount + "."); // 생성 결과 출력
        }

        [MenuItem("ProjectJ/Day145/2. Validate Day145 Modules")] // 검증 메뉴 등록
        public static void ValidateDay145Modules() // Day145 모듈 규격 검증
        {
            if (!AssetDatabase.IsValidFolder(Day145RootPath)) // 출력 폴더 존재 검사
            {
                Debug.LogError("[ProjectJ][Day145] 생성 폴더가 없습니다. 먼저 1번 재생성 메뉴를 실행하세요."); // 출력 폴더 누락 오류 출력
                return; // 검증 종료
            }

            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { Day145RootPath }); // Day145 Prefab 검색
            int errorCount = 0; // 전체 오류 개수 초기화

            if (prefabGuids.Length != ExpectedModuleCount) // 생성 개수 검사
            {
                Debug.LogError("[ProjectJ][Day145] 예상 Prefab " + ExpectedModuleCount + "종 / 현재 " + prefabGuids.Length + "종."); // 생성 개수 오류 출력
                errorCount++; // 오류 개수 증가
            }

            for (int index = 0; index < ModuleSpecs.Length; index++) // 대상 모듈 전체 순회
            {
                ModuleSpec spec = ModuleSpecs[index]; // 현재 대상 규격 조회
                string prefabPath = Day145RootPath + "/" + spec.ModuleId + ".prefab"; // 대상 Prefab 경로 계산
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath); // 대상 Prefab 로드

                if (prefab == null) // 대상 Prefab 누락 검사
                {
                    Debug.LogError("[ProjectJ][Day145] Prefab 누락: " + prefabPath); // Prefab 누락 오류 출력
                    errorCount++; // 오류 개수 증가
                    continue; // 다음 모듈 검사
                }

                errorCount += ValidateSingleModule(prefab, spec); // 개별 모듈 오류 합산
            }

            if (errorCount == 0) // 전체 검증 성공 검사
            {
                Debug.Log("[ProjectJ][Day145] Validation PASS - 6종 / 10x10x10 / 6 Socket / 기존 발판·장애물 기능 포함."); // 검증 성공 출력
            }
            else // 전체 검증 실패 처리
            {
                Debug.LogError("[ProjectJ][Day145] Validation FAIL - 오류 " + errorCount + "개."); // 검증 실패 출력
            }
        }

        [MenuItem("ProjectJ/Day145/3. Delete Day145 Modules")] // 생성물 삭제 메뉴 등록
        public static void DeleteDay145Modules() // Day145 생성물만 삭제
        {
            DeleteDay145ModulesInternal(); // Day145 출력 폴더 삭제
            AssetDatabase.SaveAssets(); // 삭제 내용 저장
            AssetDatabase.Refresh(); // Project 창 갱신
            Debug.Log("[ProjectJ][Day145] Day145 생성 모듈만 삭제 완료. 기존 발판·장애물 Prefab과 Runtime 스크립트는 유지됩니다."); // 삭제 완료 출력
        }

        [MenuItem("ProjectJ/Day145/4. Log Existing Platform Obstacle Prefabs")] // 기존 Prefab 확인 메뉴 등록
        public static void LogExistingPlatformObstaclePrefabs() // 재사용 가능한 기존 Prefab 검색 결과 출력
        {
            for (int index = 0; index < ModuleSpecs.Length; index++) // 대상 기능 전체 순회
            {
                ModuleSpec spec = ModuleSpecs[index]; // 현재 기능 규격 조회
                GameObject prefab = FindBestExistingPrefab(spec.Feature, out string prefabPath); // 기존 Prefab 검색

                if (prefab != null) // 기존 Prefab 검색 성공 검사
                {
                    Debug.Log("[ProjectJ][Day145] " + spec.Feature + " 기존 Prefab: " + prefabPath, prefab); // 검색 결과 출력
                }
                else // 기존 Prefab 검색 실패 처리
                {
                    Debug.LogWarning("[ProjectJ][Day145] " + spec.Feature + " 기존 scripted Prefab을 찾지 못했습니다. 재생성 시 기존 Runtime 컴포넌트로 안전한 모듈을 만듭니다."); // 대체 생성 안내 출력
                }
            }
        }

        private static bool CreateModulePrefab(ModuleSpec spec) // 단일 Day145 모듈 Prefab 생성
        {
            GameObject moduleRoot = new GameObject(spec.ModuleId); // 모듈 루트 생성
            MapModule module = moduleRoot.AddComponent<MapModule>(); // MapModule 컴포넌트 부착
            Transform geometryRoot = CreateRoot(moduleRoot.transform, "Geometry"); // 고정 지형 루트 생성
            Transform socketsRoot = CreateRoot(moduleRoot.transform, "Sockets"); // Socket 루트 생성
            Transform gameplayRoot = CreateRoot(moduleRoot.transform, "Gameplay"); // 게임플레이 루트 생성
            Transform obstacleRoot = CreateRoot(gameplayRoot, "ObstacleRoot"); // 기존 발판·장애물 배치 루트 생성
            CreateTraversalGeometry(geometryRoot, spec.Feature); // 기능별 안전 통과 지형 생성
            MapModuleSocket[] sockets = CreateSockets(socketsRoot); // 6방향 Socket 생성
            module.Configure(spec.ModuleId, ModuleSize, sockets); // 10m 모듈 데이터 설정
            bool reusedExistingPrefab = TryPlaceExistingFeaturePrefab(spec.Feature, obstacleRoot, moduleRoot.transform); // 기존 scripted Prefab 우선 배치

            if (!reusedExistingPrefab) // 기존 Prefab을 직접 사용하지 못한 경우 검사
            {
                CreateRuntimeFeatureFallback(spec.Feature, obstacleRoot); // 기존 Runtime 기능으로 안전한 대체 발판 생성
            }

            string prefabPath = Day145RootPath + "/" + spec.ModuleId + ".prefab"; // 저장 경로 계산
            PrefabUtility.SaveAsPrefabAsset(moduleRoot, prefabPath); // Prefab 생성 또는 덮어쓰기
            UnityEngine.Object.DestroyImmediate(moduleRoot); // 임시 Hierarchy 오브젝트 제거
            return reusedExistingPrefab; // 기존 Prefab 재사용 여부 반환
        }

        private static void CreateTraversalGeometry(Transform geometryRoot, ModuleFeature feature) // 기능별 기본 진행 지형 생성
        {
            if (feature == ModuleFeature.AirBag) // 에어백은 낙하 없는 회피형 구조 검사
            {
                CreateBox(geometryRoot, "Floor_Full", new Vector3(0f, FloorCenterY, 0f), new Vector3(ModuleSize, FloorThickness, ModuleSize)); // 전체 안전 바닥 생성
                return; // 지형 생성 종료
            }

            CreateBox(geometryRoot, "Floor_Entry", new Vector3(0f, FloorCenterY, -3.75f), new Vector3(ModuleSize, FloorThickness, 2.5f)); // 남쪽 진입 바닥 생성
            CreateBox(geometryRoot, "Floor_Exit", new Vector3(0f, FloorCenterY, 3.75f), new Vector3(ModuleSize, FloorThickness, 2.5f)); // 북쪽 이탈 바닥 생성

            if (feature == ModuleFeature.IceSurface) // 빙판 기능 검사
            {
                return; // 중앙 빙판이 바닥 역할을 담당하므로 추가 지형 생략
            }
        }

        private static MapModuleSocket[] CreateSockets(Transform socketsRoot) // South Entrance와 North Exit 기반 6 Socket 생성
        {
            MapModuleSocket[] sockets = new MapModuleSocket[AllDirections.Length]; // Socket 배열 생성

            for (int index = 0; index < AllDirections.Length; index++) // 6방향 전체 순회
            {
                MapModuleFaceDirection direction = AllDirections[index]; // 현재 방향 조회
                MapModuleFaceState state = MapModuleFaceState.Closed; // 기본 닫힘 상태 설정

                if (direction == MapModuleFaceDirection.South) // 남쪽 방향 검사
                {
                    state = MapModuleFaceState.Entrance; // 남쪽을 Entrance로 설정
                }
                else if (direction == MapModuleFaceDirection.North) // 북쪽 방향 검사
                {
                    state = MapModuleFaceState.Exit; // 북쪽을 Exit로 설정
                }

                GameObject socketObject = new GameObject("Socket_" + direction); // Socket 오브젝트 생성
                socketObject.transform.SetParent(socketsRoot, false); // Socket 부모 연결
                socketObject.transform.localPosition = MapModule.GetDirectionVector(direction) * (ModuleSize * 0.5f); // ±5m 셀 경계 배치
                MapModuleSocket socket = socketObject.AddComponent<MapModuleSocket>(); // Socket 컴포넌트 부착
                socket.Configure(direction, state); // 방향과 상태 설정
                sockets[index] = socket; // 배열에 Socket 저장
            }

            return sockets; // 완성된 Socket 배열 반환
        }

        private static bool TryPlaceExistingFeaturePrefab(ModuleFeature feature, Transform parent, Transform moduleRoot) // 기존 scripted Prefab을 모듈 내부에 배치
        {
            GameObject sourcePrefab = FindBestExistingPrefab(feature, out string sourcePath); // 기능이 포함된 기존 Prefab 검색

            if (sourcePrefab == null) // 기존 Prefab 없음 검사
            {
                return false; // 대체 생성 요청 반환
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject; // 기존 Prefab 인스턴스 생성

            if (instance == null) // Prefab 인스턴스 생성 실패 검사
            {
                Debug.LogWarning("[ProjectJ][Day145] 기존 Prefab 인스턴스 생성 실패: " + sourcePath); // 인스턴스 실패 경고 출력
                return false; // 대체 생성 요청 반환
            }

            instance.name = "Existing_" + sourcePrefab.name; // 기존 Prefab 재사용 표시 이름 설정
            instance.transform.SetParent(parent, false); // ObstacleRoot 아래 배치
            instance.transform.localPosition = new Vector3(0f, FloorTopY, 0f); // 기존 발판 기준 높이 정렬
            instance.transform.localRotation = Quaternion.identity; // 회전 초기화

            if (!IsInstanceInsideModule(instance, moduleRoot)) // 10m 셀 범위 검사
            {
                Debug.LogWarning("[ProjectJ][Day145] 기존 Prefab이 10m 셀을 벗어나 안전하게 재사용할 수 없어 Runtime 기능 기반으로 대체합니다: " + sourcePath, sourcePrefab); // 범위 초과 경고 출력
                UnityEngine.Object.DestroyImmediate(instance); // 범위 초과 인스턴스 제거
                return false; // 대체 생성 요청 반환
            }

            if (!AreMovingPointsInsideModule(instance, moduleRoot)) // 이동 발판 경로 범위 검사
            {
                Debug.LogWarning("[ProjectJ][Day145] 기존 MovingPlatform 이동점이 10m 셀을 벗어나 Runtime 기능 기반으로 대체합니다: " + sourcePath, sourcePrefab); // 이동 범위 초과 경고 출력
                UnityEngine.Object.DestroyImmediate(instance); // 범위 초과 인스턴스 제거
                return false; // 대체 생성 요청 반환
            }

            Debug.Log("[ProjectJ][Day145] 기존 Prefab 직접 재사용: " + sourcePath); // 기존 Prefab 재사용 로그 출력
            return true; // 기존 Prefab 재사용 성공 반환
        }

        private static GameObject FindBestExistingPrefab(ModuleFeature feature, out string bestPath) // 대상 기능이 포함된 기존 Prefab 검색
        {
            Type targetType = GetFeatureType(feature); // 기능에 대응하는 Component 타입 조회
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { PrefabSearchRoot }); // ProjectJ Prefab 전체 검색
            GameObject bestPrefab = null; // 최적 Prefab 초기화
            bestPath = null; // 최적 경로 초기화
            int bestScore = int.MinValue; // 최적 점수 초기화

            for (int index = 0; index < prefabGuids.Length; index++) // 검색된 Prefab 전체 순회
            {
                string path = AssetDatabase.GUIDToAssetPath(prefabGuids[index]); // Prefab 경로 조회

                if (path.StartsWith(Day145RootPath, StringComparison.Ordinal)) // 현재 생성 폴더 재검색 방지
                {
                    continue; // 다음 Prefab 검사
                }

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path); // Prefab 자산 로드

                if (prefab == null) // Prefab 로드 실패 검사
                {
                    continue; // 다음 Prefab 검사
                }

                if (prefab.GetComponentInChildren<MapModule>(true) != null) // 이전 맵 Wrapper 중첩 방지 검사
                {
                    continue; // MapModule 포함 Prefab 제외
                }

                Component targetComponent = prefab.GetComponentInChildren(targetType, true); // 대상 기능 Component 검색

                if (targetComponent == null) // 대상 기능 누락 검사
                {
                    continue; // 다음 Prefab 검사
                }

                int score = ScorePrefabPath(path); // 기존 Prefab 우선순위 계산

                if (score <= bestScore) // 기존 최적 점수보다 낮은지 검사
                {
                    continue; // 다음 Prefab 검사
                }

                bestScore = score; // 최적 점수 갱신
                bestPrefab = prefab; // 최적 Prefab 갱신
                bestPath = path; // 최적 경로 갱신
            }

            return bestPrefab; // 최적 Prefab 반환
        }

        private static int ScorePrefabPath(string path) // 기존 Prefab 경로 우선순위 계산
        {
            int score = 0; // 기본 점수 초기화

            if (path.IndexOf("Platform", StringComparison.OrdinalIgnoreCase) >= 0) // 발판 관련 경로 검사
            {
                score += 40; // 발판 Prefab 우선점수 추가
            }

            if (path.IndexOf("Obstacle", StringComparison.OrdinalIgnoreCase) >= 0) // 장애물 관련 경로 검사
            {
                score += 40; // 장애물 Prefab 우선점수 추가
            }

            if (path.IndexOf("Gameplay", StringComparison.OrdinalIgnoreCase) >= 0) // 게임플레이 관련 경로 검사
            {
                score += 20; // 게임플레이 Prefab 우선점수 추가
            }

            if (path.IndexOf("Network", StringComparison.OrdinalIgnoreCase) >= 0) // 네트워크 연결 Prefab 검사
            {
                score += 10; // 네트워크 구성 보존 우선점수 추가
            }

            if (path.IndexOf("Imported", StringComparison.OrdinalIgnoreCase) >= 0) // 단순 외형 Prefab 검사
            {
                score -= 20; // scripted 게임플레이 Prefab보다 낮은 우선순위 적용
            }

            return score; // 최종 점수 반환
        }

        private static void CreateRuntimeFeatureFallback(ModuleFeature feature, Transform parent) // 기존 Runtime 기능으로 셀 내부 안전 발판 생성
        {
            switch (feature) // 기능 종류 분기
            {
                case ModuleFeature.MovingPlatform: // 이동 발판 생성
                    CreateMovingPlatform(parent); // 이동 발판 기능 생성
                    break; // 분기 종료
                case ModuleFeature.RotatingPlatform: // 회전 발판 생성
                    CreateRotatingPlatform(parent); // 회전 발판 기능 생성
                    break; // 분기 종료
                case ModuleFeature.GhostPlatform: // 유령 발판 생성
                    CreateGhostPlatform(parent); // 유령 발판 기능 생성
                    break; // 분기 종료
                case ModuleFeature.IceSurface: // 빙판 생성
                    CreateIceSurface(parent); // 빙판 기능 생성
                    break; // 분기 종료
                case ModuleFeature.SpringPlatform: // 스프링 발판 생성
                    CreateSpringPlatform(parent); // 스프링 발판 기능 생성
                    break; // 분기 종료
                case ModuleFeature.AirBag: // 에어백 장애물 생성
                    CreateAirBag(parent); // 에어백 기능 생성
                    break; // 분기 종료
                default: // 정의되지 않은 기능 처리
                    throw new ArgumentOutOfRangeException(nameof(feature), feature, null); // 잘못된 기능 예외 출력
            }
        }

        private static void CreateMovingPlatform(Transform parent) // 기존 MovingPlatform 기능 기반 발판 생성
        {
            Transform pointA = CreateMarker(parent, "MovePoint_A", new Vector3(-1.8f, FloorTopY + 0.25f, 0f)); // 왼쪽 이동점 생성
            Transform pointB = CreateMarker(parent, "MovePoint_B", new Vector3(1.8f, FloorTopY + 0.25f, 0f)); // 오른쪽 이동점 생성
            GameObject body = CreateBox(parent, "MovingPlatform", pointA.localPosition, new Vector3(3.2f, 0.5f, 3.2f)); // 이동 발판 몸체 생성
            MovingPlatform movingPlatform = body.AddComponent<MovingPlatform>(); // 기존 MovingPlatform 기능 부착
            movingPlatform.Configure(pointA, pointB, 2.5f); // 기존 기본 속도 기반 이동 설정
            Rigidbody bodyRigidbody = body.GetComponent<Rigidbody>(); // 자동 추가 Rigidbody 조회
            bodyRigidbody.isKinematic = true; // 물리 밀림 방지 설정
            bodyRigidbody.useGravity = false; // 중력 비활성화 설정
        }

        private static void CreateRotatingPlatform(Transform parent) // 기존 RotatingPlatform 기능 기반 발판 생성
        {
            GameObject body = CreateBox(parent, "RotatingPlatform", new Vector3(0f, FloorTopY + 0.25f, 0f), new Vector3(4f, 0.5f, 4f)); // 회전 발판 몸체 생성
            RotatingPlatform rotatingPlatform = body.AddComponent<RotatingPlatform>(); // 기존 RotatingPlatform 기능 부착
            rotatingPlatform.Configure(Vector3.up, 35f); // 기존 기본 회전 속도 적용
            Rigidbody bodyRigidbody = body.GetComponent<Rigidbody>(); // 자동 추가 Rigidbody 조회
            bodyRigidbody.isKinematic = true; // 물리 밀림 방지 설정
            bodyRigidbody.useGravity = false; // 중력 비활성화 설정
        }

        private static void CreateGhostPlatform(Transform parent) // 기존 GhostPlatform 기능 기반 발판 생성
        {
            GameObject body = CreateBox(parent, "GhostPlatform", new Vector3(0f, FloorTopY + 0.25f, 0f), new Vector3(4f, 0.5f, 4f)); // 유령 발판 몸체 생성
            GhostPlatform ghostPlatform = body.AddComponent<GhostPlatform>(); // 기존 GhostPlatform 기능 부착
            int playerLayer = LayerMask.NameToLayer("Player"); // Player 레이어 조회
            LayerMask playerMask = playerLayer >= 0 ? 1 << playerLayer : 1 << 8; // Player 레이어 누락 시 기존 기본값 사용
            ghostPlatform.Configure(3f, 1f, 2f, 0f, playerMask); // 기존 타이밍 값 적용
        }

        private static void CreateIceSurface(Transform parent) // 기존 IceSurface 기능 기반 중앙 빙판 생성
        {
            GameObject body = CreateBox(parent, "IceSurface", new Vector3(0f, FloorCenterY, 0f), new Vector3(4.5f, FloorThickness, 5f)); // 중앙 빙판 바닥 생성
            IceSurface iceSurface = body.AddComponent<IceSurface>(); // 기존 IceSurface 기능 부착
            iceSurface.Configure(6f, 2.5f, 3f); // 기존 기본 가속·감속 값 적용
        }

        private static void CreateSpringPlatform(Transform parent) // 기존 SpringPlatform 기능 기반 발판 생성
        {
            GameObject body = CreateBox(parent, "SpringPlatform", new Vector3(0f, FloorTopY + 0.2f, 0f), new Vector3(3.2f, 0.4f, 3.2f)); // 중앙 스프링 발판 생성
            SpringPlatform springPlatform = body.AddComponent<SpringPlatform>(); // 기존 SpringPlatform 기능 부착
            springPlatform.Configure(1.5f); // 기존 1.5배 점프 배율 적용
        }

        private static void CreateAirBag(Transform parent) // 기존 AirBagObstacle 기능 기반 장애물 생성
        {
            GameObject body = CreateBox(parent, "AirBagObstacle", new Vector3(2.7f, FloorTopY + 1f, 0f), new Vector3(1.4f, 2f, 1.8f)); // 우측 에어백 장애물 생성
            AirBagObstacle airBag = body.AddComponent<AirBagObstacle>(); // 기존 AirBagObstacle 기능 부착
            airBag.Configure(12f, Vector3.left, 0.35f); // 기존 밀어내기 세기와 분산값 적용
        }

        private static bool IsInstanceInsideModule(GameObject instance, Transform moduleRoot) // 기존 Prefab의 렌더·충돌 영역이 10m 셀 안인지 검사
        {
            Bounds? bounds = CalculateCombinedBounds(instance); // Prefab 전체 표시·충돌 Bounds 계산

            if (!bounds.HasValue) // Bounds를 계산할 요소가 없는지 검사
            {
                return true; // 크기 검사 생략 허용
            }

            Vector3[] corners = GetBoundsCorners(bounds.Value); // Bounds 모서리 좌표 생성

            for (int index = 0; index < corners.Length; index++) // 모서리 전체 순회
            {
                Vector3 localPoint = moduleRoot.InverseTransformPoint(corners[index]); // 모듈 로컬 좌표 변환

                if (Mathf.Abs(localPoint.x) > 5.01f || Mathf.Abs(localPoint.y) > 5.01f || Mathf.Abs(localPoint.z) > 5.01f) // 10m 셀 범위 검사
                {
                    return false; // 셀 범위 초과 반환
                }
            }

            return true; // 셀 범위 내부 반환
        }

        private static bool AreMovingPointsInsideModule(GameObject instance, Transform moduleRoot) // 기존 MovingPlatform의 이동점 범위 검사
        {
            MovingPlatform[] movingPlatforms = instance.GetComponentsInChildren<MovingPlatform>(true); // 이동 발판 전체 검색

            for (int index = 0; index < movingPlatforms.Length; index++) // 이동 발판 전체 순회
            {
                SerializedObject serializedMovingPlatform = new SerializedObject(movingPlatforms[index]); // private 이동점 데이터 읽기 준비
                SerializedProperty pointAProperty = serializedMovingPlatform.FindProperty("pointA"); // A 이동점 속성 검색
                SerializedProperty pointBProperty = serializedMovingPlatform.FindProperty("pointB"); // B 이동점 속성 검색
                Transform pointA = pointAProperty != null ? pointAProperty.objectReferenceValue as Transform : null; // A 이동점 조회
                Transform pointB = pointBProperty != null ? pointBProperty.objectReferenceValue as Transform : null; // B 이동점 조회

                if (!IsTransformInsideModule(pointA, moduleRoot) || !IsTransformInsideModule(pointB, moduleRoot)) // 이동점 셀 범위 검사
                {
                    return false; // 이동 경로 범위 초과 반환
                }
            }

            return true; // 모든 이동점 셀 내부 반환
        }

        private static bool IsTransformInsideModule(Transform target, Transform moduleRoot) // Transform 셀 범위 검사
        {
            if (target == null) // 대상 Transform 누락 검사
            {
                return true; // 누락은 기존 스크립트 검증에 맡기고 범위 검사는 통과
            }

            Vector3 localPoint = moduleRoot.InverseTransformPoint(target.position); // 모듈 로컬 좌표 변환
            return Mathf.Abs(localPoint.x) <= 5.01f && Mathf.Abs(localPoint.y) <= 5.01f && Mathf.Abs(localPoint.z) <= 5.01f; // 셀 범위 결과 반환
        }

        private static Bounds? CalculateCombinedBounds(GameObject root) // Renderer와 Collider를 합친 Bounds 계산
        {
            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true); // Renderer 전체 조회
            Collider[] colliders = root.GetComponentsInChildren<Collider>(true); // Collider 전체 조회
            bool hasBounds = false; // Bounds 존재 여부 초기화
            Bounds combinedBounds = new Bounds(root.transform.position, Vector3.zero); // 합산 Bounds 초기화

            for (int index = 0; index < renderers.Length; index++) // Renderer 전체 순회
            {
                if (!hasBounds) // 첫 Bounds 검사
                {
                    combinedBounds = renderers[index].bounds; // 첫 Renderer Bounds 저장
                    hasBounds = true; // Bounds 존재 기록
                }
                else // 기존 Bounds가 있는 경우 처리
                {
                    combinedBounds.Encapsulate(renderers[index].bounds); // Renderer Bounds 합산
                }
            }

            for (int index = 0; index < colliders.Length; index++) // Collider 전체 순회
            {
                if (!hasBounds) // 첫 Bounds 검사
                {
                    combinedBounds = colliders[index].bounds; // 첫 Collider Bounds 저장
                    hasBounds = true; // Bounds 존재 기록
                }
                else // 기존 Bounds가 있는 경우 처리
                {
                    combinedBounds.Encapsulate(colliders[index].bounds); // Collider Bounds 합산
                }
            }

            return hasBounds ? combinedBounds : (Bounds?)null; // 계산된 Bounds 또는 없음 반환
        }

        private static Vector3[] GetBoundsCorners(Bounds bounds) // Bounds 8개 모서리 생성
        {
            Vector3 min = bounds.min; // 최소 좌표 조회
            Vector3 max = bounds.max; // 최대 좌표 조회
            return new[] // 8개 모서리 배열 반환
            {
                new Vector3(min.x, min.y, min.z), // 최소 XYZ 모서리
                new Vector3(min.x, min.y, max.z), // 최소 XY 최대 Z 모서리
                new Vector3(min.x, max.y, min.z), // 최소 X 최대 Y 최소 Z 모서리
                new Vector3(min.x, max.y, max.z), // 최소 X 최대 YZ 모서리
                new Vector3(max.x, min.y, min.z), // 최대 X 최소 YZ 모서리
                new Vector3(max.x, min.y, max.z), // 최대 X 최소 Y 최대 Z 모서리
                new Vector3(max.x, max.y, min.z), // 최대 XY 최소 Z 모서리
                new Vector3(max.x, max.y, max.z) // 최대 XYZ 모서리
            }; // 모서리 배열 종료
        }

        private static int ValidateSingleModule(GameObject prefab, ModuleSpec spec) // 단일 Day145 Prefab 검증
        {
            int errorCount = 0; // 현재 모듈 오류 개수 초기화
            MapModule[] modules = prefab.GetComponentsInChildren<MapModule>(true); // 중첩 MapModule 검사 목록 조회

            if (modules.Length != 1) // MapModule 하나 규칙 검사
            {
                Debug.LogError("[ProjectJ][Day145] MapModule은 루트에 1개만 있어야 합니다: " + prefab.name, prefab); // MapModule 중첩 오류 출력
                errorCount++; // 오류 개수 증가
                return errorCount; // 중복 정의 세부 검사 종료
            }

            MapModule module = modules[0]; // 단일 MapModule 조회

            if (module.gameObject != prefab) // 루트 MapModule 여부 검사
            {
                Debug.LogError("[ProjectJ][Day145] MapModule이 Prefab 루트에 없습니다: " + prefab.name, prefab); // 루트 오류 출력
                errorCount++; // 오류 개수 증가
            }

            if (!Mathf.Approximately(module.ModuleSize, ModuleSize)) // 10m 규격 검사
            {
                Debug.LogError("[ProjectJ][Day145] Module Size가 10이 아닙니다: " + prefab.name, prefab); // 크기 오류 출력
                errorCount++; // 오류 개수 증가
            }

            if (!module.IsDefinitionValid()) // 6 Socket과 Entrance Exit 규칙 검사
            {
                Debug.LogError("[ProjectJ][Day145] Module 정의 오류: " + prefab.name, prefab); // 정의 오류 출력
                errorCount++; // 오류 개수 증가
            }

            IReadOnlyList<MapModuleSocket> sockets = module.Sockets; // Socket 목록 조회

            if (sockets == null || sockets.Count != 6) // Socket 6개 규칙 검사
            {
                Debug.LogError("[ProjectJ][Day145] Socket 6개 규칙 위반: " + prefab.name, prefab); // Socket 개수 오류 출력
                errorCount++; // 오류 개수 증가
            }
            else // Socket 세부 검사가 가능한 경우 처리
            {
                for (int index = 0; index < sockets.Count; index++) // Socket 전체 순회
                {
                    MapModuleSocket socket = sockets[index]; // 현재 Socket 조회
                    Vector3 expectedPosition = MapModule.GetDirectionVector(socket.Direction) * 5f; // 기대 경계 위치 계산

                    if (Vector3.Distance(socket.transform.localPosition, expectedPosition) > 0.001f) // ±5m 경계 위치 검사
                    {
                        Debug.LogError("[ProjectJ][Day145] Socket 경계 위치 오류: " + prefab.name + " / " + socket.name, prefab); // Socket 위치 오류 출력
                        errorCount++; // 오류 개수 증가
                    }

                    if (socket.Direction == MapModuleFaceDirection.Down && socket.State == MapModuleFaceState.Exit) // 하강 Exit 검사
                    {
                        Debug.LogError("[ProjectJ][Day145] 정상 진행 Down Exit 금지 규칙 위반: " + prefab.name, prefab); // 하강 Exit 오류 출력
                        errorCount++; // 오류 개수 증가
                    }
                }
            }

            if (!HasExpectedFeature(prefab, spec.Feature)) // 기존 기능 포함 여부 검사
            {
                Debug.LogError("[ProjectJ][Day145] 기존 기능 Component 누락: " + spec.Feature + " / " + prefab.name, prefab); // 기능 누락 오류 출력
                errorCount++; // 오류 개수 증가
            }

            if (!IsInstanceInsideModule(prefab, prefab.transform)) // 최종 Prefab 전체 10m 셀 범위 검사
            {
                Debug.LogError("[ProjectJ][Day145] Renderer 또는 Collider가 10m 셀을 벗어납니다: " + prefab.name, prefab); // 범위 오류 출력
                errorCount++; // 오류 개수 증가
            }

            return errorCount; // 현재 모듈 오류 개수 반환
        }

        private static bool HasExpectedFeature(GameObject prefab, ModuleFeature feature) // 기능 Component 포함 여부 검사
        {
            Type featureType = GetFeatureType(feature); // 대상 Component 타입 조회
            return prefab.GetComponentInChildren(featureType, true) != null; // 하위 오브젝트 포함 기능 존재 여부 반환
        }

        private static Type GetFeatureType(ModuleFeature feature) // 모듈 기능과 Runtime Component 타입 연결
        {
            switch (feature) // 기능 종류 분기
            {
                case ModuleFeature.MovingPlatform: return typeof(MovingPlatform); // 이동 발판 타입 반환
                case ModuleFeature.RotatingPlatform: return typeof(RotatingPlatform); // 회전 발판 타입 반환
                case ModuleFeature.GhostPlatform: return typeof(GhostPlatform); // 유령 발판 타입 반환
                case ModuleFeature.IceSurface: return typeof(IceSurface); // 빙판 타입 반환
                case ModuleFeature.SpringPlatform: return typeof(SpringPlatform); // 스프링 발판 타입 반환
                case ModuleFeature.AirBag: return typeof(AirBagObstacle); // 에어백 타입 반환
                default: throw new ArgumentOutOfRangeException(nameof(feature), feature, null); // 잘못된 기능 예외 출력
            }
        }

        private static GameObject CreateBox(Transform parent, string objectName, Vector3 localPosition, Vector3 localScale) // 충돌 가능한 Cube 생성
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube); // Unity Cube 생성
            box.name = objectName; // 오브젝트 이름 설정
            box.transform.SetParent(parent, false); // 부모 연결
            box.transform.localPosition = localPosition; // 로컬 위치 설정
            box.transform.localRotation = Quaternion.identity; // 로컬 회전 초기화
            box.transform.localScale = localScale; // 로컬 크기 설정
            int groundLayer = LayerMask.NameToLayer("Ground"); // Ground 레이어 조회

            if (groundLayer >= 0) // Ground 레이어 존재 검사
            {
                box.layer = groundLayer; // Ground 레이어 적용
            }

            return box; // 생성 Cube 반환
        }

        private static Transform CreateMarker(Transform parent, string objectName, Vector3 localPosition) // 이동점 Marker 생성
        {
            GameObject marker = new GameObject(objectName); // 빈 Marker 생성
            marker.transform.SetParent(parent, false); // 부모 연결
            marker.transform.localPosition = localPosition; // 로컬 위치 설정
            return marker.transform; // Marker Transform 반환
        }

        private static Transform CreateRoot(Transform parent, string rootName) // 빈 그룹 루트 생성
        {
            GameObject root = new GameObject(rootName); // 그룹 오브젝트 생성
            root.transform.SetParent(parent, false); // 부모 연결
            return root.transform; // 그룹 Transform 반환
        }

        private static void EnsureFolder(string folderPath) // 중첩 Unity 폴더 생성
        {
            string[] parts = folderPath.Split('/'); // 경로 단계 분리
            string currentPath = parts[0]; // Assets 루트 시작

            for (int index = 1; index < parts.Length; index++) // 하위 폴더 단계 순회
            {
                string nextPath = currentPath + "/" + parts[index]; // 다음 폴더 전체 경로 계산

                if (!AssetDatabase.IsValidFolder(nextPath)) // 폴더 존재 검사
                {
                    AssetDatabase.CreateFolder(currentPath, parts[index]); // 누락 폴더 생성
                }

                currentPath = nextPath; // 현재 경로 갱신
            }
        }

        private static void DeleteDay145ModulesInternal() // Day145 생성 폴더만 안전 삭제
        {
            if (!AssetDatabase.IsValidFolder(Day145RootPath)) // 생성 폴더 존재 검사
            {
                return; // 삭제 대상 없음 처리
            }

            AssetDatabase.DeleteAsset(Day145RootPath); // Day145 생성 Prefab 폴더 삭제
        }

        private enum ModuleFeature // Day145 모듈 기능 종류
        {
            MovingPlatform, // 이동 발판
            RotatingPlatform, // 회전 발판
            GhostPlatform, // 유령 발판
            IceSurface, // 빙판
            SpringPlatform, // 스프링 발판
            AirBag // 에어백 장애물
        }

        private readonly struct ModuleSpec // 모듈 ID와 기능 연결 데이터
        {
            public ModuleSpec(string moduleId, ModuleFeature feature) // 모듈 규격 생성자
            {
                ModuleId = moduleId; // 모듈 ID 저장
                Feature = feature; // 기능 종류 저장
            }

            public string ModuleId { get; } // 모듈 ID 반환
            public ModuleFeature Feature { get; } // 기능 종류 반환
        }
    }
}
