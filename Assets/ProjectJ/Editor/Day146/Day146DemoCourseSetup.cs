using System; // 공통 C# 기능 사용
using System.Collections.Generic; // Dictionary와 HashSet 기능 사용
using ProjectJ.Checkpoint; // 기존 체크포인트 기능 사용
using ProjectJ.Finish; // 기존 결승선 기능 사용
using ProjectJ.Map; // 10m 맵 모듈 기능 사용
using UnityEditor; // Unity Editor 자산과 Prefab 기능 사용
using UnityEditor.SceneManagement; // Game Scene 자동 배치와 저장 기능 사용
using UnityEngine; // Unity 오브젝트와 Vector 기능 사용
using UnityEngine.SceneManagement; // Scene 기능 사용
using CheckpointComponent = ProjectJ.Checkpoint.Checkpoint; // Checkpoint 네임스페이스와 타입 이름 충돌 방지

namespace ProjectJ.Editor // Project J Editor 기능 네임스페이스
{
    public static class Day146DemoCourseSetup // 146일차 시연용 고정 코스 생성 도구
    {
        private const float ModuleSize = 10f; // 144일차에서 확정한 정육면체 한 변 크기
        private const float FloorTopY = -4.6f; // 10m 모듈 기본 바닥 윗면 높이
        private const string Day144RootPath = "Assets/ProjectJ/Prefabs/Map/Modules/Day144"; // 기본 모듈 경로
        private const string Day145RootPath = "Assets/ProjectJ/Prefabs/Map/Modules/Day145"; // 장애물 모듈 경로
        private const string CoursesRootPath = "Assets/ProjectJ/Prefabs/Map/Courses"; // 시연 코스 저장 폴더
        private const string CoursePrefabPath = CoursesRootPath + "/PJ146_DemoCourse.prefab"; // 시연 코스 Prefab 경로
        private const string GameScenePath = "Assets/ProjectJ/Scenes/Game.unity"; // 실제 Game Scene 경로
        private const string CourseRootName = "PJ146_DemoCourse"; // Scene과 Prefab 공통 루트 이름
        private const int ExpectedModuleCount = 34; // 시연 코스 전체 모듈 수
        private static readonly Vector3Int FinishModuleCell = new Vector3Int(0, 2, 28); // 결승선 직전 마지막 모듈 Cell

        private static readonly CourseModuleSpec[] ModuleSpecs = // START부터 FINISH까지 사용할 고정 코스 정의
        {
            new CourseModuleSpec("START", "PJ144_Module_Straight_SouthNorth", new Vector3Int(0, 0, 0)), // 출발 안전 직선
            new CourseModuleSpec("Section_01", "PJ144_Module_Straight_Slalom", new Vector3Int(0, 0, 1)), // 기본 방향 전환 감각
            new CourseModuleSpec("Section_01", "PJ145_Module_MovingPlatform_SouthNorth", new Vector3Int(0, 0, 2)), // 이동 발판
            new CourseModuleSpec("Section_01", "PJ144_Module_Jump_Single", new Vector3Int(0, 0, 3)), // 단일 점프
            new CourseModuleSpec("Section_01", "PJ144_Module_Straight_Pillars", new Vector3Int(0, 0, 4)), // 기둥 우회
            new CourseModuleSpec("CP1", "PJ144_Module_Straight_SouthNorth", new Vector3Int(0, 0, 5)), // CP1 안전 공간

            new CourseModuleSpec("Section_02", "PJ145_Module_GhostPlatform_SouthNorth", new Vector3Int(0, 0, 6)), // 유령 발판
            new CourseModuleSpec("Section_02", "PJ145_Module_RotatingPlatform_SouthNorth", new Vector3Int(0, 0, 7)), // 회전 발판
            new CourseModuleSpec("Section_02", "PJ144_Module_Bridge_Narrow", new Vector3Int(0, 0, 8)), // 좁은 다리
            new CourseModuleSpec("Section_02", "PJ145_Module_AirBag_SouthNorth", new Vector3Int(0, 0, 9)), // 에어백 방해
            new CourseModuleSpec("CP2", "PJ144_Module_Straight_SouthNorth", new Vector3Int(0, 0, 10)), // CP2 안전 공간

            new CourseModuleSpec("Section_03", "PJ144_Module_ClimbTurn_SouthUp", new Vector3Int(0, 0, 11)), // 첫 수직 상승 진입
            new CourseModuleSpec("Section_03", "PJ144_Module_VerticalPlatforms_DownUp", new Vector3Int(0, 1, 11)), // 수직 발판 구간
            new CourseModuleSpec("Section_03", "PJ144_Module_LandingTurn_DownNorth", new Vector3Int(0, 2, 11)), // 상단 수평 복귀
            new CourseModuleSpec("Section_03", "PJ145_Module_SpringPlatform_SouthNorth", new Vector3Int(0, 2, 12)), // 스프링 발판
            new CourseModuleSpec("CP3", "PJ144_Module_Straight_SouthNorth", new Vector3Int(0, 2, 13)), // CP3 안전 공간

            new CourseModuleSpec("Section_04", "PJ144_Module_Branch2_SouthNorthEast", new Vector3Int(0, 2, 14)), // 두 갈래 분기 시작
            new CourseModuleSpec("Section_04_Main", "PJ145_Module_IceSurface_SouthNorth", new Vector3Int(0, 2, 15)), // 메인 루트 빙판
            new CourseModuleSpec("Section_04_Side", "PJ144_Module_Merge2_SouthWestNorth", new Vector3Int(1, 2, 14)), // East 분기에서 West Entrance 수신
            new CourseModuleSpec("Section_04_Side", "PJ145_Module_GhostPlatform_SouthNorth", new Vector3Int(1, 2, 15)), // 보조 루트 유령 발판
            new CourseModuleSpec("Section_04_Side", "PJ144_Module_Corner_SouthWest", new Vector3Int(1, 2, 16)), // 보조 루트를 서쪽 Merge로 회수
            new CourseModuleSpec("Section_04", "PJ144_Module_Merge2_SouthEastNorth", new Vector3Int(0, 2, 16)), // 두 경로 합류
            new CourseModuleSpec("Section_04", "PJ144_Module_Drop_SouthNorth_DoubleSide", new Vector3Int(0, 2, 17)), // 양쪽 낙하 위험
            new CourseModuleSpec("Section_04", "PJ144_Module_Straight_Pillars", new Vector3Int(0, 2, 18)), // 합류 후 안정화 구간
            new CourseModuleSpec("Section_04", "PJ145_Module_RotatingPlatform_SouthNorth", new Vector3Int(0, 2, 19)), // CP 직전 회전 발판
            new CourseModuleSpec("CP4", "PJ144_Module_Straight_SouthNorth", new Vector3Int(0, 2, 20)), // CP4 안전 공간

            new CourseModuleSpec("Section_05", "PJ144_Module_Jump_Double", new Vector3Int(0, 2, 21)), // 종합 2연속 점프
            new CourseModuleSpec("Section_05", "PJ145_Module_MovingPlatform_SouthNorth", new Vector3Int(0, 2, 22)), // 종합 이동 발판
            new CourseModuleSpec("Section_05", "PJ145_Module_GhostPlatform_SouthNorth", new Vector3Int(0, 2, 23)), // 종합 유령 발판
            new CourseModuleSpec("Section_05", "PJ145_Module_AirBag_SouthNorth", new Vector3Int(0, 2, 24)), // 종합 에어백
            new CourseModuleSpec("Section_05", "PJ144_Module_Jump_SteppingStones", new Vector3Int(0, 2, 25)), // 종합 징검다리
            new CourseModuleSpec("Section_05", "PJ144_Module_Straight_Narrow", new Vector3Int(0, 2, 26)), // 결승 전 좁은 길
            new CourseModuleSpec("Section_05", "PJ144_Module_Straight_Slalom", new Vector3Int(0, 2, 27)), // 결승 전 마지막 회피
            new CourseModuleSpec("FINISH", "PJ144_Module_Straight_SouthNorth", FinishModuleCell) // 결승 안전 직선
        }; // 코스 정의 종료

        [MenuItem("ProjectJ/Day146/1. Build Demo Course Prefab")] // 코스 Prefab 생성 메뉴 등록
        public static void BuildDemoCoursePrefabMenu() // 메뉴에서 코스 Prefab 생성
        {
            BuildDemoCoursePrefab(); // 코스 생성 실행
        }

        [MenuItem("ProjectJ/Day146/2. Validate Demo Course Prefab")] // 코스 Prefab 검증 메뉴 등록
        public static void ValidateDemoCoursePrefab() // 생성된 코스 구조와 연결 검사
        {
            GameObject coursePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CoursePrefabPath); // 코스 Prefab 로드

            if (coursePrefab == null) // 코스 Prefab 누락 검사
            {
                Debug.LogError("[ProjectJ][Day146] Demo Course Prefab이 없습니다. 먼저 1번 메뉴를 실행하세요."); // 누락 오류 출력
                return; // 검증 종료
            }

            int errorCount = ValidateCourse(coursePrefab); // 전체 코스 검증 수행

            if (errorCount == 0) // 검증 성공 여부 확인
            {
                Debug.Log("[ProjectJ][Day146] Validation PASS - 34 Module / 10m Grid / Start+CP1~CP4 / Branch+Merge / Vertical / Finish.", coursePrefab); // 검증 성공 로그 출력
            }
            else // 검증 실패 처리
            {
                Debug.LogError("[ProjectJ][Day146] Validation FAIL - 오류 " + errorCount + "개.", coursePrefab); // 검증 실패 로그 출력
            }
        }

        [MenuItem("ProjectJ/Day146/3. Rebuild And Place In Game Scene")] // 코스 생성 후 Game Scene 배치 메뉴 등록
        public static void RebuildAndPlaceInGameScene() // 코스를 생성하고 실제 Game Scene에 배치
        {
            if (!BuildDemoCoursePrefab()) // 코스 Prefab 생성 실패 검사
            {
                return; // Scene 배치 중단
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) // 현재 Scene의 미저장 변경 보호
            {
                Debug.LogWarning("[ProjectJ][Day146] 현재 Scene 저장이 취소되어 Game Scene 배치를 중단했습니다."); // 중단 이유 출력
                return; // Scene 배치 중단
            }

            Scene gameScene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single); // 실제 Game Scene 열기
            RemoveCourseInstanceFromScene(gameScene); // 이전 Day146 코스 인스턴스만 제거
            GameObject coursePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CoursePrefabPath); // 생성된 코스 Prefab 로드
            GameObject courseInstance = PrefabUtility.InstantiatePrefab(coursePrefab) as GameObject; // Prefab 연결을 유지한 Scene 인스턴스 생성

            if (courseInstance == null) // Scene 인스턴스 생성 실패 검사
            {
                Debug.LogError("[ProjectJ][Day146] Game Scene에 Demo Course를 배치하지 못했습니다."); // 배치 실패 출력
                return; // Scene 배치 중단
            }

            courseInstance.name = CourseRootName; // Scene 인스턴스 이름 통일
            courseInstance.transform.position = Vector3.zero; // 월드 원점에 코스 배치
            courseInstance.transform.rotation = Quaternion.identity; // 회전 초기화
            EditorSceneManager.MarkSceneDirty(gameScene); // Scene 변경 상태 기록
            EditorSceneManager.SaveScene(gameScene); // Game Scene 자동 저장
            Selection.activeGameObject = courseInstance; // 생성 코스를 선택 상태로 변경
            EditorGUIUtility.PingObject(courseInstance); // Hierarchy에서 생성 코스 강조
            Debug.Log("[ProjectJ][Day146] Game Scene에 PJ146_DemoCourse 배치 완료. 기존 Day146 인스턴스만 교체했습니다.", courseInstance); // 배치 완료 로그 출력
        }

        [MenuItem("ProjectJ/Day146/4. Delete Demo Course From Game Scene")] // Game Scene에서 Day146 코스만 삭제 메뉴 등록
        public static void DeleteDemoCourseFromGameScene() // 실제 Game Scene의 Day146 인스턴스 제거
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) // 현재 Scene의 미저장 변경 보호
            {
                Debug.LogWarning("[ProjectJ][Day146] 현재 Scene 저장이 취소되어 삭제를 중단했습니다."); // 삭제 중단 이유 출력
                return; // 삭제 중단
            }

            Scene gameScene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single); // 실제 Game Scene 열기
            bool removed = RemoveCourseInstanceFromScene(gameScene); // 기존 Day146 코스 제거 시도

            if (removed) // 실제 삭제 여부 확인
            {
                EditorSceneManager.MarkSceneDirty(gameScene); // Scene 변경 상태 기록
                EditorSceneManager.SaveScene(gameScene); // 삭제 상태 자동 저장
                Debug.Log("[ProjectJ][Day146] Game Scene에서 PJ146_DemoCourse만 삭제했습니다."); // 삭제 완료 로그 출력
            }
            else // 삭제 대상 없음 처리
            {
                Debug.Log("[ProjectJ][Day146] Game Scene에 삭제할 PJ146_DemoCourse가 없습니다."); // 대상 없음 로그 출력
            }
        }

        [MenuItem("ProjectJ/Day146/5. Delete Demo Course Prefab")] // 생성 Prefab만 삭제 메뉴 등록
        public static void DeleteDemoCoursePrefab() // Day146 생성 Prefab 삭제
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(CoursePrefabPath) == null) // 삭제 대상 존재 검사
            {
                Debug.Log("[ProjectJ][Day146] 삭제할 Demo Course Prefab이 없습니다."); // 대상 없음 로그 출력
                return; // 삭제 종료
            }

            AssetDatabase.DeleteAsset(CoursePrefabPath); // Day146 코스 Prefab만 삭제
            AssetDatabase.SaveAssets(); // 삭제 상태 저장
            AssetDatabase.Refresh(); // Project 창 갱신
            Debug.Log("[ProjectJ][Day146] PJ146_DemoCourse Prefab만 삭제했습니다. Day144/Day145 원본 모듈은 유지됩니다."); // 안전 삭제 완료 로그 출력
        }

        private static bool BuildDemoCoursePrefab() // 전체 시연 코스 Prefab 생성
        {
            if (!ValidateSourceModules()) // 필요한 Day144/145 원본 모듈 존재 검사
            {
                Debug.LogError("[ProjectJ][Day146] 필요한 원본 모듈이 누락되어 Demo Course 생성을 중단했습니다."); // 원본 누락 오류 출력
                return false; // 생성 실패 반환
            }

            EnsureFolder(CoursesRootPath); // 코스 저장 폴더 생성
            GameObject courseRoot = new GameObject(CourseRootName); // 임시 코스 루트 생성
            Dictionary<string, Transform> sectionRoots = CreateSectionRoots(courseRoot.transform); // 구간별 그룹 생성

            for (int index = 0; index < ModuleSpecs.Length; index++) // 전체 코스 모듈 순회
            {
                CourseModuleSpec spec = ModuleSpecs[index]; // 현재 모듈 정의 조회
                GameObject sourcePrefab = LoadModulePrefab(spec.PrefabName); // 원본 모듈 Prefab 로드
                Transform sectionRoot = sectionRoots[spec.SectionName]; // 현재 구간 부모 조회
                GameObject instance = PrefabUtility.InstantiatePrefab(sourcePrefab) as GameObject; // 원본 모듈 Prefab 인스턴스 생성

                if (instance == null) // 인스턴스 생성 실패 검사
                {
                    UnityEngine.Object.DestroyImmediate(courseRoot); // 임시 루트 정리
                    Debug.LogError("[ProjectJ][Day146] 모듈 인스턴스 생성 실패: " + spec.PrefabName); // 실패 모듈 출력
                    return false; // 생성 실패 반환
                }

                instance.name = index.ToString("D2") + "_" + spec.PrefabName; // 코스 순서가 보이도록 이름 설정
                instance.transform.SetParent(sectionRoot, false); // 구간 그룹 아래 배치
                instance.transform.localPosition = CellToLocalPosition(spec.Cell); // 10m Grid 위치 적용
                instance.transform.localRotation = Quaternion.identity; // 모듈 방향 데이터와 일치하도록 회전 금지
            }

            Transform gameplayRoot = CreateRoot(courseRoot.transform, "Gameplay"); // 체크포인트와 Finish 그룹 생성
            CreateStartMarker(gameplayRoot); // 출발 위치 Marker 생성
            CreateCheckpoint(gameplayRoot, "Checkpoint_Start", CheckpointId.Start, new Vector3Int(0, 0, 0)); // START 체크포인트 생성
            CreateCheckpoint(gameplayRoot, "Checkpoint_CP1", CheckpointId.CP1, new Vector3Int(0, 0, 5)); // CP1 생성
            CreateCheckpoint(gameplayRoot, "Checkpoint_CP2", CheckpointId.CP2, new Vector3Int(0, 0, 10)); // CP2 생성
            CreateCheckpoint(gameplayRoot, "Checkpoint_CP3", CheckpointId.CP3, new Vector3Int(0, 2, 13)); // CP3 생성
            CreateCheckpoint(gameplayRoot, "Checkpoint_CP4", CheckpointId.CP4, new Vector3Int(0, 2, 20)); // CP4 생성
            CreateFinish(gameplayRoot); // 결승 Trigger와 순위 Manager 생성
            PrefabUtility.SaveAsPrefabAsset(courseRoot, CoursePrefabPath); // 코스 전체를 Prefab으로 저장
            UnityEngine.Object.DestroyImmediate(courseRoot); // 임시 Hierarchy 오브젝트 제거
            AssetDatabase.SaveAssets(); // 생성 자산 저장
            AssetDatabase.Refresh(); // Project 창 갱신
            GameObject savedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(CoursePrefabPath); // 저장된 코스 Prefab 다시 로드

            if (savedPrefab == null) // Prefab 저장 실패 검사
            {
                Debug.LogError("[ProjectJ][Day146] PJ146_DemoCourse Prefab 저장에 실패했습니다."); // 저장 실패 출력
                return false; // 생성 실패 반환
            }

            int errorCount = ValidateCourse(savedPrefab); // 생성 직후 자동 검증

            if (errorCount > 0) // 자동 검증 실패 검사
            {
                Debug.LogError("[ProjectJ][Day146] Demo Course는 생성됐지만 검증 오류가 " + errorCount + "개 있습니다.", savedPrefab); // 검증 오류 출력
                return false; // 후속 Scene 자동 배치 차단
            }

            Debug.Log("[ProjectJ][Day146] PJ146_DemoCourse 생성 완료 - 34 Module / START / CP1~CP4 / 수직 상승 / Branch+Merge / FINISH.", savedPrefab); // 생성 완료 로그 출력
            return true; // 생성 성공 반환
        }

        private static Dictionary<string, Transform> CreateSectionRoots(Transform courseRoot) // 코스 구간별 그룹 생성
        {
            string[] sectionNames = // 고정 구간 이름 목록
            {
                "START", // 출발 구간
                "Section_01", // 기본 이동 구간
                "CP1", // 첫 체크포인트 구간
                "Section_02", // 타이밍 장애물 구간
                "CP2", // 두 번째 체크포인트 구간
                "Section_03", // 수직 이동 구간
                "CP3", // 세 번째 체크포인트 구간
                "Section_04", // 분기와 합류 공통 구간
                "Section_04_Main", // 분기 메인 루트
                "Section_04_Side", // 분기 보조 루트
                "CP4", // 네 번째 체크포인트 구간
                "Section_05", // 최종 종합 구간
                "FINISH" // 결승 구간
            }; // 구간 이름 목록 종료

            Dictionary<string, Transform> sectionRoots = new Dictionary<string, Transform>(); // 구간 검색용 Dictionary 생성
            Transform modulesRoot = CreateRoot(courseRoot, "Modules"); // 전체 모듈 그룹 생성

            for (int index = 0; index < sectionNames.Length; index++) // 구간 이름 전체 순회
            {
                Transform sectionRoot = CreateRoot(modulesRoot, sectionNames[index]); // 구간 그룹 생성
                sectionRoots.Add(sectionNames[index], sectionRoot); // 이름으로 구간 등록
            }

            return sectionRoots; // 완성된 구간 Dictionary 반환
        }

        private static void CreateStartMarker(Transform gameplayRoot) // 플레이어 Spawn 연결용 Marker 생성
        {
            GameObject marker = new GameObject("StartSpawnPoint"); // 출발 위치 Marker 생성
            marker.transform.SetParent(gameplayRoot, false); // Gameplay 아래 배치
            marker.transform.localPosition = CellToLocalPosition(new Vector3Int(0, 0, 0)) + new Vector3(0f, FloorTopY + 1.1f, -2.5f); // 첫 모듈 남쪽 안전 공간에 배치
            marker.transform.localRotation = Quaternion.identity; // 북쪽 진행 방향 유지
        }

        private static void CreateCheckpoint(Transform gameplayRoot, string objectName, CheckpointId id, Vector3Int cell) // 기존 Checkpoint 기반 Gate 생성
        {
            Vector3 cellPosition = CellToLocalPosition(cell); // 대상 Cell 중심 위치 계산
            GameObject checkpointObject = new GameObject(objectName); // 체크포인트 오브젝트 생성
            checkpointObject.transform.SetParent(gameplayRoot, false); // Gameplay 아래 배치
            checkpointObject.transform.localPosition = cellPosition + new Vector3(0f, FloorTopY + 1.5f, 0f); // 안전 바닥 위 Trigger 중심 배치
            BoxCollider trigger = checkpointObject.AddComponent<BoxCollider>(); // 체크포인트 Trigger Collider 추가
            trigger.isTrigger = true; // 물리 통과형 Trigger 설정
            trigger.size = new Vector3(8f, 3f, 1.2f); // 넓은 통과 Gate 크기 적용
            Rigidbody triggerBody = checkpointObject.AddComponent<Rigidbody>(); // Trigger 물리 이벤트용 Rigidbody 추가
            triggerBody.isKinematic = true; // 고정 Trigger 설정
            triggerBody.useGravity = false; // 중력 비활성화
            CheckpointComponent checkpoint = checkpointObject.AddComponent<CheckpointComponent>(); // 기존 체크포인트 기능 부착
            GameObject respawnObject = new GameObject("RespawnPoint"); // 부활 위치 생성
            respawnObject.transform.SetParent(checkpointObject.transform, false); // 체크포인트 아래 배치
            respawnObject.transform.localPosition = new Vector3(0f, -0.4f, -2f); // Trigger 직전 안전 바닥 위 부활 위치 설정
            respawnObject.transform.localRotation = Quaternion.identity; // 북쪽 방향으로 부활
            checkpoint.Configure(id, respawnObject.transform); // 기존 Checkpoint에 ID와 부활 지점 연결
        }

        private static void CreateFinish(Transform gameplayRoot) // 기존 Finish 시스템으로 결승선 생성
        {
            GameObject finishSystemObject = new GameObject("FinishSystem"); // 결승 시스템 그룹 생성
            finishSystemObject.transform.SetParent(gameplayRoot, false); // Gameplay 아래 배치
            FinishOrderManager finishManager = finishSystemObject.AddComponent<FinishOrderManager>(); // 기존 결승 순위 Manager 추가
            GameObject finishTriggerObject = new GameObject("FinishTrigger"); // 결승 Trigger 생성
            finishTriggerObject.transform.SetParent(gameplayRoot, false); // Gameplay 아래 배치
            finishTriggerObject.transform.localPosition = CellToLocalPosition(FinishModuleCell) + new Vector3(0f, FloorTopY + 1.5f, 3.4f); // 마지막 모듈 북쪽에 결승 Gate 배치
            BoxCollider trigger = finishTriggerObject.AddComponent<BoxCollider>(); // 결승 Trigger Collider 추가
            trigger.isTrigger = true; // 물리 통과형 Trigger 설정
            trigger.size = new Vector3(8f, 3f, 1.5f); // 넓은 결승 Gate 크기 적용
            Rigidbody triggerBody = finishTriggerObject.AddComponent<Rigidbody>(); // Trigger 물리 이벤트용 Rigidbody 추가
            triggerBody.isKinematic = true; // 고정 Trigger 설정
            triggerBody.useGravity = false; // 중력 비활성화
            FinishTrigger finishTrigger = finishTriggerObject.AddComponent<FinishTrigger>(); // 기존 FinishTrigger 기능 부착
            finishTrigger.Configure(finishManager); // 기존 FinishOrderManager 연결
        }

        private static bool ValidateSourceModules() // 코스에 필요한 원본 Prefab 존재 여부 검사
        {
            HashSet<string> checkedNames = new HashSet<string>(StringComparer.Ordinal); // 중복 Prefab 검사를 막기 위한 이름 집합
            bool valid = true; // 전체 원본 상태 초기화

            for (int index = 0; index < ModuleSpecs.Length; index++) // 코스 정의 전체 순회
            {
                string prefabName = ModuleSpecs[index].PrefabName; // 현재 Prefab 이름 조회

                if (!checkedNames.Add(prefabName)) // 이미 확인한 Prefab인지 검사
                {
                    continue; // 중복 검사 생략
                }

                if (LoadModulePrefab(prefabName) != null) // 원본 Prefab 존재 확인
                {
                    continue; // 다음 Prefab 검사
                }

                Debug.LogError("[ProjectJ][Day146] 필요한 모듈 Prefab 누락: " + prefabName); // 누락 원본 출력
                valid = false; // 전체 원본 상태 실패 기록
            }

            return valid; // 원본 준비 상태 반환
        }

        private static GameObject LoadModulePrefab(string prefabName) // Day144 또는 Day145에서 원본 모듈 로드
        {
            string day144Path = Day144RootPath + "/" + prefabName + ".prefab"; // Day144 예상 경로 계산
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(day144Path); // Day144에서 우선 검색

            if (prefab != null) // Day144 검색 성공 검사
            {
                return prefab; // Day144 Prefab 반환
            }

            string day145Path = Day145RootPath + "/" + prefabName + ".prefab"; // Day145 예상 경로 계산
            return AssetDatabase.LoadAssetAtPath<GameObject>(day145Path); // Day145 Prefab 반환 또는 null 반환
        }

        private static int ValidateCourse(GameObject coursePrefab) // 생성된 전체 코스 검증
        {
            int errorCount = 0; // 전체 오류 개수 초기화
            MapModule[] modules = coursePrefab.GetComponentsInChildren<MapModule>(true); // 모든 모듈 조회

            if (modules.Length != ExpectedModuleCount) // 전체 모듈 수 검사
            {
                Debug.LogError("[ProjectJ][Day146] 예상 Module " + ExpectedModuleCount + "개 / 현재 " + modules.Length + "개.", coursePrefab); // 개수 오류 출력
                errorCount++; // 오류 개수 증가
            }

            Dictionary<Vector3Int, MapModule> modulesByCell = new Dictionary<Vector3Int, MapModule>(); // Cell별 모듈 Dictionary 생성

            for (int index = 0; index < modules.Length; index++) // 모든 모듈 순회
            {
                MapModule module = modules[index]; // 현재 모듈 조회
                Vector3 localPosition = coursePrefab.transform.InverseTransformPoint(module.transform.position); // 코스 기준 로컬 위치 계산
                Vector3Int cell = PositionToCell(localPosition); // 10m Cell 좌표 계산
                Vector3 snappedPosition = CellToLocalPosition(cell); // 기대 Grid 위치 계산

                if (Vector3.Distance(localPosition, snappedPosition) > 0.001f) // 10m Grid 정렬 검사
                {
                    Debug.LogError("[ProjectJ][Day146] 10m Grid에서 벗어난 Module: " + module.name, module); // Grid 오류 출력
                    errorCount++; // 오류 개수 증가
                }

                if (modulesByCell.ContainsKey(cell)) // 동일 Cell 중복 배치 검사
                {
                    Debug.LogError("[ProjectJ][Day146] 같은 Cell에 Module이 중복 배치됨: " + cell, module); // Cell 중복 오류 출력
                    errorCount++; // 오류 개수 증가
                    continue; // Dictionary 중복 추가 방지
                }

                modulesByCell.Add(cell, module); // Cell과 모듈 연결 저장

                if (!Mathf.Approximately(module.ModuleSize, ModuleSize) || !module.IsDefinitionValid()) // 10m 규격과 Socket 정의 검사
                {
                    Debug.LogError("[ProjectJ][Day146] Module 규격 오류: " + module.name, module); // 규격 오류 출력
                    errorCount++; // 오류 개수 증가
                }
            }

            foreach (KeyValuePair<Vector3Int, MapModule> pair in modulesByCell) // Cell별 모듈 전체 순회
            {
                MapModule module = pair.Value; // 현재 모듈 조회
                IReadOnlyList<MapModuleSocket> sockets = module.Sockets; // 현재 Socket 목록 조회

                if (sockets == null || sockets.Count != 6) // Socket 목록 자체가 잘못된 경우 검사
                {
                    Debug.LogError("[ProjectJ][Day146] Socket 6개 규칙 위반: " + module.name, module); // Socket 목록 오류 출력
                    errorCount++; // 오류 개수 증가
                    continue; // 연결 검사는 다음 모듈로 진행
                }

                for (int socketIndex = 0; socketIndex < sockets.Count; socketIndex++) // Socket 전체 순회
                {
                    MapModuleSocket socket = sockets[socketIndex]; // 현재 Socket 조회

                    if (socket == null || socket.State != MapModuleFaceState.Exit) // 정상 진행 Exit만 검사
                    {
                        continue; // 다음 Socket 검사
                    }

                    Vector3Int neighborCell = pair.Key + MapModule.GetDirectionCellOffset(socket.Direction); // Exit가 가리키는 다음 Cell 계산

                    if (!modulesByCell.TryGetValue(neighborCell, out MapModule neighborModule)) // 다음 Cell 모듈 존재 검사
                    {
                        if (pair.Key == FinishModuleCell && socket.Direction == MapModuleFaceDirection.North) // FINISH Gate로 끝나는 마지막 Exit인지 검사
                        {
                            continue; // 유일하게 허용되는 미연결 Exit 처리
                        }

                        Debug.LogError("[ProjectJ][Day146] Exit 다음 Cell에 Module이 없음: " + pair.Key + " / " + socket.Direction, module); // 연결 누락 출력
                        errorCount++; // 오류 개수 증가
                        continue; // 다음 Socket 검사
                    }

                    MapModuleFaceDirection opposite = MapModule.GetOppositeDirection(socket.Direction); // 반대 방향 Entrance 계산

                    if (!neighborModule.TryGetSocket(opposite, out MapModuleSocket neighborSocket)) // 다음 모듈의 반대 Socket 존재 검사
                    {
                        Debug.LogError("[ProjectJ][Day146] 연결 대상 반대 Socket 누락: " + neighborModule.name, neighborModule); // Socket 누락 오류 출력
                        errorCount++; // 오류 개수 증가
                        continue; // 다음 Socket 검사
                    }

                    if (!MapModule.CanConnect(socket.Direction, socket.State, neighborSocket.Direction, neighborSocket.State)) // Exit와 Entrance 연결 규칙 검사
                    {
                        Debug.LogError("[ProjectJ][Day146] Module 연결 상태 불일치: " + module.name + " -> " + neighborModule.name, module); // 연결 상태 오류 출력
                        errorCount++; // 오류 개수 증가
                    }
                }
            }

            errorCount += ValidateCheckpoints(coursePrefab); // START와 CP1~CP4 검증 합산
            errorCount += ValidateFinish(coursePrefab); // FINISH 시스템 검증 합산
            return errorCount; // 전체 오류 개수 반환
        }

        private static int ValidateCheckpoints(GameObject coursePrefab) // 체크포인트 전체 검증
        {
            int errorCount = 0; // 체크포인트 오류 개수 초기화
            CheckpointComponent[] checkpoints = coursePrefab.GetComponentsInChildren<CheckpointComponent>(true); // 모든 체크포인트 조회

            if (checkpoints.Length != 5) // START + CP1~CP4 총 5개 검사
            {
                Debug.LogError("[ProjectJ][Day146] Checkpoint는 Start 포함 5개여야 합니다. 현재 " + checkpoints.Length + "개.", coursePrefab); // 개수 오류 출력
                errorCount++; // 오류 개수 증가
            }

            HashSet<CheckpointId> ids = new HashSet<CheckpointId>(); // Checkpoint ID 중복 검사 집합 생성

            for (int index = 0; index < checkpoints.Length; index++) // 체크포인트 전체 순회
            {
                CheckpointComponent checkpoint = checkpoints[index]; // 현재 체크포인트 조회

                if (!ids.Add(checkpoint.Id)) // ID 중복 검사
                {
                    Debug.LogError("[ProjectJ][Day146] Checkpoint ID 중복: " + checkpoint.Id, checkpoint); // ID 중복 오류 출력
                    errorCount++; // 오류 개수 증가
                }
            }

            CheckpointId[] requiredIds = // 필수 체크포인트 ID 목록
            {
                CheckpointId.Start, // 출발
                CheckpointId.CP1, // 첫 번째 체크포인트
                CheckpointId.CP2, // 두 번째 체크포인트
                CheckpointId.CP3, // 세 번째 체크포인트
                CheckpointId.CP4 // 네 번째 체크포인트
            }; // 필수 ID 목록 종료

            for (int index = 0; index < requiredIds.Length; index++) // 필수 ID 전체 순회
            {
                if (ids.Contains(requiredIds[index])) // 필수 ID 존재 확인
                {
                    continue; // 다음 ID 검사
                }

                Debug.LogError("[ProjectJ][Day146] 필수 Checkpoint 누락: " + requiredIds[index], coursePrefab); // 누락 ID 출력
                errorCount++; // 오류 개수 증가
            }

            return errorCount; // 체크포인트 오류 개수 반환
        }

        private static int ValidateFinish(GameObject coursePrefab) // 결승선 시스템 검증
        {
            int errorCount = 0; // 결승선 오류 개수 초기화
            FinishOrderManager[] managers = coursePrefab.GetComponentsInChildren<FinishOrderManager>(true); // 결승 순위 Manager 조회
            FinishTrigger[] triggers = coursePrefab.GetComponentsInChildren<FinishTrigger>(true); // 결승 Trigger 조회

            if (managers.Length != 1) // FinishOrderManager 하나 규칙 검사
            {
                Debug.LogError("[ProjectJ][Day146] FinishOrderManager는 1개여야 합니다. 현재 " + managers.Length + "개.", coursePrefab); // Manager 개수 오류 출력
                errorCount++; // 오류 개수 증가
            }

            if (triggers.Length != 1) // FinishTrigger 하나 규칙 검사
            {
                Debug.LogError("[ProjectJ][Day146] FinishTrigger는 1개여야 합니다. 현재 " + triggers.Length + "개.", coursePrefab); // Trigger 개수 오류 출력
                errorCount++; // 오류 개수 증가
            }
            else if (managers.Length == 1 && triggers[0].FinishManager != managers[0]) // Trigger와 Manager 연결 검사
            {
                Debug.LogError("[ProjectJ][Day146] FinishTrigger와 FinishOrderManager 참조가 일치하지 않습니다.", triggers[0]); // 참조 오류 출력
                errorCount++; // 오류 개수 증가
            }

            return errorCount; // 결승선 오류 개수 반환
        }

        private static bool RemoveCourseInstanceFromScene(Scene scene) // 특정 Scene에서 Day146 코스 인스턴스만 제거
        {
            GameObject[] roots = scene.GetRootGameObjects(); // Scene 루트 오브젝트 조회
            bool removed = false; // 삭제 여부 초기화

            for (int index = roots.Length - 1; index >= 0; index--) // 루트 전체 역순 순회
            {
                if (!string.Equals(roots[index].name, CourseRootName, StringComparison.Ordinal)) // Day146 코스 이름 검사
                {
                    continue; // 다른 Scene 오브젝트 보존
                }

                UnityEngine.Object.DestroyImmediate(roots[index]); // Day146 코스 인스턴스만 삭제
                removed = true; // 삭제 여부 기록
            }

            return removed; // 실제 삭제 여부 반환
        }

        private static Vector3 CellToLocalPosition(Vector3Int cell) // 10m Cell 좌표를 실제 로컬 위치로 변환
        {
            return new Vector3(cell.x * ModuleSize, cell.y * ModuleSize, cell.z * ModuleSize); // 10m Grid 위치 반환
        }

        private static Vector3Int PositionToCell(Vector3 position) // 실제 위치를 가장 가까운 10m Cell로 변환
        {
            return new Vector3Int(Mathf.RoundToInt(position.x / ModuleSize), Mathf.RoundToInt(position.y / ModuleSize), Mathf.RoundToInt(position.z / ModuleSize)); // Grid Cell 좌표 반환
        }

        private static Transform CreateRoot(Transform parent, string rootName) // 빈 그룹 루트 생성
        {
            GameObject root = new GameObject(rootName); // 그룹 오브젝트 생성
            root.transform.SetParent(parent, false); // 부모 연결
            return root.transform; // 생성한 Transform 반환
        }

        private static void EnsureFolder(string folderPath) // 중첩 Unity 폴더 생성
        {
            string[] parts = folderPath.Split('/'); // 경로 단계 분리
            string currentPath = parts[0]; // Assets 루트에서 시작

            for (int index = 1; index < parts.Length; index++) // 하위 폴더 단계 순회
            {
                string nextPath = currentPath + "/" + parts[index]; // 다음 폴더 전체 경로 계산

                if (!AssetDatabase.IsValidFolder(nextPath)) // 폴더 누락 검사
                {
                    AssetDatabase.CreateFolder(currentPath, parts[index]); // 누락 폴더 생성
                }

                currentPath = nextPath; // 현재 경로 갱신
            }
        }

        private readonly struct CourseModuleSpec // 고정 코스의 단일 모듈 배치 정보
        {
            public CourseModuleSpec(string sectionName, string prefabName, Vector3Int cell) // 배치 정보 생성자
            {
                SectionName = sectionName; // 구간 이름 저장
                PrefabName = prefabName; // 원본 Prefab 이름 저장
                Cell = cell; // 10m Cell 좌표 저장
            }

            public string SectionName { get; } // 구간 이름 반환
            public string PrefabName { get; } // Prefab 이름 반환
            public Vector3Int Cell { get; } // Grid Cell 좌표 반환
        }
    }
}
