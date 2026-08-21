using System.Collections.Generic; // 목록 기능 사용
using ProjectJ.Map; // 맵 시스템 사용
using UnityEditor; // 유니티 에디터 기능 사용
using UnityEditor.SceneManagement; // Scene 에디터 기능 사용
using UnityEngine; // 유니티 기능 사용
using UnityEngine.InputSystem; // Input System 사용
using UnityEngine.SceneManagement; // Scene 기능 사용

namespace ProjectJ.Editor // 프로젝트 에디터 네임스페이스
{
    public static class Day48BranchMergeGreyboxSetup // 48일차 분기 합류 Greybox 설정
    {
        private const string SourceScenePath = "Assets/ProjectJ/Tests/Manual/Phase4/Phase4_InteractionTest.unity"; // 기준 테스트 Scene
        private const string TargetScenePath = "Assets/ProjectJ/Tests/Manual/Phase4/Phase4_BranchMergeGreybox.unity"; // 48일차 Scene
        private const string ModuleFolder = "Assets/ProjectJ/Prefabs/Map/Modules/Day25/"; // Day25 Module 폴더
        private const string StraightPath = ModuleFolder + "PJ_Module_Straight_SouthNorth.prefab"; // 직선 Module
        private const string CornerPath = ModuleFolder + "PJ_Module_Corner_SouthEast.prefab"; // 코너 Module
        private const string BranchPath = ModuleFolder + "PJ_Module_Branch_SouthNorthEast.prefab"; // 분기 Module
        private const string MergePath = ModuleFolder + "PJ_Module_Merge_SouthWestNorth.prefab"; // 합류 Module
        private const string RootName = "=== Day48 Branch Merge Greybox ==="; // 코스 루트 이름
        private const float CourseRootY = 10f; // Module 바닥 높이 보정
        private const float CourseRootX = 300f; // 기존 테스트 구역과 분리할 X 위치
        private const float SocketDirectionDot = 0.99f; // Socket 방향 탐색 오차
        private const float SocketPositionTolerance = 0.05f; // Socket 위치 정렬 오차
        private static readonly Vector3 BaseCenter = new Vector3(CourseRootX, CourseRootY, 0f); // Branch 기준 위치

        [MenuItem("ProjectJ/Day48/Setup Branch Merge Greybox")] // 자동 설정 메뉴 등록
        public static void Setup() // 전체 설정 실행
        {
            if (!PrepareTargetScene()) // 48일차 Scene 준비 검사
            {
                return; // 설정 중단
            }

            Scene scene = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single); // 48일차 Scene 열기
            RemoveExistingRoot(); // 기존 자동 생성 코스 제거
            GameObject root = new GameObject(RootName); // 코스 루트 생성
            SceneManager.MoveGameObjectToScene(root, scene); // 현재 Scene으로 루트 이동

            Transform startRoute = CreateGroup(root.transform, "StartRoute"); // 시작 경로 그룹
            Transform pathA = CreateGroup(root.transform, "Path_A_Short"); // A 경로 그룹
            Transform pathB = CreateGroup(root.transform, "Path_B_Detour"); // B 경로 그룹
            Transform mergeRoute = CreateGroup(root.transform, "MergeRoute"); // 합류 경로 그룹
            Transform markers = CreateGroup(root.transform, "Markers"); // 마커 그룹

            Dictionary<string, MapModule> modules = new Dictionary<string, MapModule>(); // 배치 Module 목록

            modules["Start"] = CreateModule(StraightPath, "Day48_Start_Straight", BaseCenter + new Vector3(0f, 0f, -20f), 0f, startRoute); // 시작 직선
            modules["Branch"] = CreateModule(BranchPath, "Day48_Branch", BaseCenter, 0f, startRoute); // 분기 Module

            modules["A1"] = CreateModule(StraightPath, "Day48_A_Straight", BaseCenter + new Vector3(0f, 0f, 20f), 0f, pathA); // A 경로 직선

            modules["B1"] = CreateModule(StraightPath, "Day48_B01_East", BaseCenter + new Vector3(20f, 0f, 0f), 90f, pathB); // B 경로 동쪽 직선
            modules["B2"] = CreateModule(CornerPath, "Day48_B02_TurnSouth", BaseCenter + new Vector3(40f, 0f, 0f), 90f, pathB); // 동쪽에서 남쪽 회전
            modules["B3"] = CreateModule(StraightPath, "Day48_B03_South", BaseCenter + new Vector3(40f, 0f, -20f), 180f, pathB); // 남쪽 직선
            modules["B4"] = CreateModule(CornerPath, "Day48_B04_TurnWest", BaseCenter + new Vector3(40f, 0f, -40f), 180f, pathB); // 남쪽에서 서쪽 회전
            modules["B5"] = CreateModule(StraightPath, "Day48_B05_West", BaseCenter + new Vector3(20f, 0f, -40f), 270f, pathB); // 서쪽 직선
            modules["B6"] = CreateModule(StraightPath, "Day48_B06_West", BaseCenter + new Vector3(0f, 0f, -40f), 270f, pathB); // 서쪽 직선
            modules["B7"] = CreateModule(StraightPath, "Day48_B07_West", BaseCenter + new Vector3(-20f, 0f, -40f), 270f, pathB); // 서쪽 직선
            modules["B8"] = CreateModule(CornerPath, "Day48_B08_TurnNorth", BaseCenter + new Vector3(-40f, 0f, -40f), 270f, pathB); // 서쪽에서 북쪽 회전
            modules["B9"] = CreateModule(StraightPath, "Day48_B09_North", BaseCenter + new Vector3(-40f, 0f, -20f), 0f, pathB); // 북쪽 직선
            modules["B10"] = CreateModule(StraightPath, "Day48_B10_North", BaseCenter + new Vector3(-40f, 0f, 0f), 0f, pathB); // 북쪽 직선
            modules["B11"] = CreateModule(StraightPath, "Day48_B11_North", BaseCenter + new Vector3(-40f, 0f, 20f), 0f, pathB); // 북쪽 직선
            modules["B12"] = CreateModule(CornerPath, "Day48_B12_TurnEast", BaseCenter + new Vector3(-40f, 0f, 40f), 0f, pathB); // 북쪽에서 동쪽 회전
            modules["B13"] = CreateModule(StraightPath, "Day48_B13_East", BaseCenter + new Vector3(-20f, 0f, 40f), 90f, pathB); // 합류 전 동쪽 직선

            modules["Merge"] = CreateModule(MergePath, "Day48_Merge", BaseCenter + new Vector3(0f, 0f, 40f), 0f, mergeRoute); // 합류 Module
            modules["Finish"] = CreateModule(StraightPath, "Day48_Finish_Straight", BaseCenter + new Vector3(0f, 0f, 60f), 0f, mergeRoute); // 합류 이후 직선

            CreateMarker(markers, "Day48_Start_Marker", BaseCenter + new Vector3(0f, -9.35f, -27f), new Vector3(3f, 0.1f, 3f)); // 시작 위치 마커
            CreateMarker(markers, "Day48_Goal_Marker", BaseCenter + new Vector3(0f, -9.35f, 67f), new Vector3(5f, 0.1f, 3f)); // 목표 위치 마커

            bool valid = ValidateCourse(modules); // 전체 연결 상태 검사
            MovePlayerToStart(); // 플레이어 시작 위치 이동
            Selection.activeGameObject = root; // 생성 코스 선택
            EditorSceneManager.MarkSceneDirty(scene); // Scene 변경 표시
            EditorSceneManager.SaveScene(scene); // Scene 저장
            AssetDatabase.SaveAssets(); // 에셋 변경 저장
            AssetDatabase.Refresh(); // 에셋 갱신

            if (valid) // 연결 검증 성공 검사
            {
                Debug.Log("Day48 Branch Merge Greybox 생성 완료 : 모든 지정 Socket 연결 정상"); // 성공 로그 출력
            }
            else
            {
                Debug.LogError("Day48 Greybox 생성은 완료했지만 Socket 연결 검증 오류가 있습니다."); // 검증 오류 출력
            }
        }

        private static bool PrepareTargetScene() // 48일차 Scene 준비
        {
            SceneAsset sourceScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(SourceScenePath); // 기준 Scene 로드

            if (sourceScene == null) // 기준 Scene 존재 검사
            {
                Debug.LogError($"기준 Scene을 찾을 수 없습니다: {SourceScenePath}"); // Scene 누락 오류 출력
                return false; // 준비 실패 반환
            }

            SceneAsset targetScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(TargetScenePath); // 기존 48일차 Scene 확인

            if (targetScene != null) // 기존 Scene 존재 검사
            {
                return true; // 기존 Scene 재사용
            }

            bool copied = AssetDatabase.CopyAsset(SourceScenePath, TargetScenePath); // 기준 Scene 복사

            if (!copied) // Scene 복사 실패 검사
            {
                Debug.LogError($"48일차 Scene 복사에 실패했습니다: {TargetScenePath}"); // 복사 오류 출력
                return false; // 준비 실패 반환
            }

            AssetDatabase.Refresh(); // 복사 Scene 갱신
            return true; // 준비 성공 반환
        }

        private static Transform CreateGroup(Transform parent, string groupName) // Hierarchy 그룹 생성
        {
            GameObject group = new GameObject(groupName); // 그룹 오브젝트 생성
            group.transform.SetParent(parent, false); // 부모 연결
            return group.transform; // 그룹 Transform 반환
        }

        private static MapModule CreateModule(string prefabPath, string objectName, Vector3 position, float yRotation, Transform parent) // Module 배치
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath); // Module Prefab 로드

            if (prefab == null) // Prefab 존재 검사
            {
                Debug.LogError($"Module Prefab을 찾을 수 없습니다: {prefabPath}"); // Prefab 누락 오류 출력
                return null; // 생성 실패 반환
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject; // Prefab 인스턴스 생성

            if (instance == null) // 인스턴스 생성 검사
            {
                Debug.LogError($"Module 생성에 실패했습니다: {prefabPath}"); // 생성 오류 출력
                return null; // 생성 실패 반환
            }

            instance.name = objectName; // 식별 이름 적용
            instance.transform.SetParent(parent, true); // 코스 그룹 연결
            instance.transform.position = position; // 고정 Grid 위치 적용
            instance.transform.rotation = Quaternion.Euler(0f, yRotation, 0f); // 고정 방향 적용

            MapModule module = instance.GetComponent<MapModule>(); // MapModule 컴포넌트 탐색

            if (module == null) // MapModule 누락 검사
            {
                Debug.LogError($"{objectName}에 MapModule이 없습니다.", instance); // 정의 누락 오류 출력
                return null; // 생성 실패 반환
            }

            if (!module.IsDefinitionValid()) // Module 정의 유효성 검사
            {
                Debug.LogError($"{objectName}의 Module 정의가 올바르지 않습니다.", instance); // 정의 오류 출력
            }

            return module; // 생성 Module 반환
        }

        private static void CreateMarker(Transform parent, string objectName, Vector3 position, Vector3 scale) // 테스트 마커 생성
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube); // Cube 마커 생성
            marker.name = objectName; // 마커 이름 적용
            marker.transform.SetParent(parent, true); // 마커 그룹 연결
            marker.transform.position = position; // 마커 위치 적용
            marker.transform.localScale = scale; // 마커 크기 적용

            Collider collider = marker.GetComponent<Collider>(); // 마커 Collider 탐색

            if (collider != null) // Collider 존재 검사
            {
                Object.DestroyImmediate(collider); // 마커 충돌 제거
            }
        }

        private static bool ValidateCourse(Dictionary<string, MapModule> modules) // 전체 고정 코스 연결 검증
        {
            bool valid = true; // 전체 결과 초기화

            valid &= ValidateLink(modules, "Start", Vector3.forward, "Branch", Vector3.back); // 시작에서 분기
            valid &= ValidateLink(modules, "Branch", Vector3.forward, "A1", Vector3.back); // 분기에서 A 경로
            valid &= ValidateLink(modules, "A1", Vector3.forward, "Merge", Vector3.back); // A 경로에서 합류

            valid &= ValidateLink(modules, "Branch", Vector3.right, "B1", Vector3.left); // 분기에서 B 경로
            valid &= ValidateLink(modules, "B1", Vector3.right, "B2", Vector3.left); // B01 B02
            valid &= ValidateLink(modules, "B2", Vector3.back, "B3", Vector3.forward); // B02 B03
            valid &= ValidateLink(modules, "B3", Vector3.back, "B4", Vector3.forward); // B03 B04
            valid &= ValidateLink(modules, "B4", Vector3.left, "B5", Vector3.right); // B04 B05
            valid &= ValidateLink(modules, "B5", Vector3.left, "B6", Vector3.right); // B05 B06
            valid &= ValidateLink(modules, "B6", Vector3.left, "B7", Vector3.right); // B06 B07
            valid &= ValidateLink(modules, "B7", Vector3.left, "B8", Vector3.right); // B07 B08
            valid &= ValidateLink(modules, "B8", Vector3.forward, "B9", Vector3.back); // B08 B09
            valid &= ValidateLink(modules, "B9", Vector3.forward, "B10", Vector3.back); // B09 B10
            valid &= ValidateLink(modules, "B10", Vector3.forward, "B11", Vector3.back); // B10 B11
            valid &= ValidateLink(modules, "B11", Vector3.forward, "B12", Vector3.back); // B11 B12
            valid &= ValidateLink(modules, "B12", Vector3.right, "B13", Vector3.left); // B12 B13
            valid &= ValidateLink(modules, "B13", Vector3.right, "Merge", Vector3.left); // B 경로에서 합류

            valid &= ValidateLink(modules, "Merge", Vector3.forward, "Finish", Vector3.back); // 합류 이후 직선

            return valid; // 전체 연결 결과 반환
        }

        private static bool ValidateLink( // 한 쌍의 Module 연결 검증
            Dictionary<string, MapModule> modules, // Module 목록
            string fromKey, // 출발 Module 키
            Vector3 fromWorldDirection, // 출발 Socket 세계 방향
            string toKey, // 도착 Module 키
            Vector3 toWorldDirection // 도착 Socket 세계 방향
        )
        {
            if (!modules.TryGetValue(fromKey, out MapModule fromModule) || fromModule == null) // 출발 Module 검사
            {
                Debug.LogError($"출발 Module 누락: {fromKey}"); // 출발 누락 로그
                return false; // 검증 실패
            }

            if (!modules.TryGetValue(toKey, out MapModule toModule) || toModule == null) // 도착 Module 검사
            {
                Debug.LogError($"도착 Module 누락: {toKey}"); // 도착 누락 로그
                return false; // 검증 실패
            }

            MapModuleSocket fromSocket = FindSocketByWorldDirection(fromModule, MapModuleFaceState.Exit, fromWorldDirection); // 출발 Exit 탐색
            MapModuleSocket toSocket = FindSocketByWorldDirection(toModule, MapModuleFaceState.Entrance, toWorldDirection); // 도착 Entrance 탐색
            MapModuleConnectionResult result = MapModuleConnectionValidator.Validate(fromSocket, toSocket, SocketPositionTolerance, SocketDirectionDot); // 실제 정렬 검사

            if (result.IsValid) // 연결 성공 검사
            {
                return true; // 정상 반환
            }

            Debug.LogError($"{fromKey} -> {toKey} 연결 실패 : {result.Failure}"); // 연결 실패 로그
            return false; // 연결 실패 반환
        }

        private static MapModuleSocket FindSocketByWorldDirection(MapModule module, MapModuleFaceState state, Vector3 worldDirection) // 상태와 세계 방향으로 Socket 탐색
        {
            if (module == null || module.Sockets == null) // Module 데이터 검사
            {
                return null; // Socket 없음 반환
            }

            Vector3 normalizedDirection = worldDirection.normalized; // 기준 방향 정규화
            MapModuleSocket bestSocket = null; // 최적 Socket 초기화
            float bestDot = -1f; // 최적 방향 점수 초기화

            for (int i = 0; i < module.Sockets.Count; i++) // 모든 Socket 반복
            {
                MapModuleSocket socket = module.Sockets[i]; // 현재 Socket 저장

                if (socket == null || socket.State != state) // 상태 일치 검사
                {
                    continue; // 다른 Socket 제외
                }

                float dot = Vector3.Dot(socket.transform.forward.normalized, normalizedDirection); // 세계 방향 일치도 계산

                if (dot <= bestDot) // 더 좋은 방향인지 검사
                {
                    continue; // 기존 Socket 유지
                }

                bestDot = dot; // 최고 점수 갱신
                bestSocket = socket; // 최고 Socket 갱신
            }

            if (bestDot < SocketDirectionDot) // 방향 일치 기준 검사
            {
                return null; // 적합한 Socket 없음 반환
            }

            return bestSocket; // 최적 Socket 반환
        }

        private static void MovePlayerToStart() // 테스트 플레이어 시작 위치 이동
        {
            PlayerInput playerInput = Object.FindFirstObjectByType<PlayerInput>(); // Scene PlayerInput 탐색

            if (playerInput == null) // 플레이어 존재 검사
            {
                Debug.LogWarning("48일차 Scene에서 PlayerInput을 찾지 못했습니다. 플레이어 위치를 직접 이동하세요."); // 플레이어 누락 경고
                return; // 위치 이동 중단
            }

            Rigidbody body = playerInput.GetComponent<Rigidbody>(); // 플레이어 Rigidbody 탐색

            if (body != null) // Rigidbody 존재 검사
            {
                body.linearVelocity = Vector3.zero; // 선형 속도 초기화
                body.angularVelocity = Vector3.zero; // 회전 속도 초기화
            }

            playerInput.transform.position = BaseCenter + new Vector3(0f, -7.5f, -27f); // 시작 직선 위로 이동
            playerInput.transform.rotation = Quaternion.identity; // 북쪽 방향으로 초기화
            EditorUtility.SetDirty(playerInput.gameObject); // Scene 변경 표시
        }

        private static void RemoveExistingRoot() // 기존 48일차 코스 제거
        {
            GameObject existing = GameObject.Find(RootName); // 기존 루트 탐색

            if (existing == null) // 기존 루트 존재 검사
            {
                return; // 삭제 작업 생략
            }

            Object.DestroyImmediate(existing); // 기존 자동 생성 코스 삭제
        }
    }
}
