using System.Collections.Generic; // 목록 기능 참조
using NUnit.Framework; // NUnit 자동 테스트 기능 참조
using ProjectJ.Data; // 장애물 데이터와 버전 형식 참조
using ProjectJ.MapGeneration; // 맵 생성 Runtime 기능 참조
using UnityEngine; // Unity 오브젝트와 벡터 기능 참조

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스 선언
{ // EditMode 테스트 묶음
    public sealed class ProceduralMapObstaclePlannerTests // 생성 맵 분기 장애물 계획 통합 자동 테스트 선언
    { // 생성 맵 분기 장애물 계획 통합 테스트 묶음
        private readonly List<Object> temporaryObjects = new List<Object>(); // 테스트 종료 시 제거할 임시 오브젝트 목록

        [TearDown] // 각 테스트 종료 정리 항목 표시
        public void TearDown() // 테스트 임시 오브젝트 제거
        { // 테스트 오브젝트 제거 처리
            for (int objectIndex = temporaryObjects.Count - 1; objectIndex >= 0; objectIndex--) // 임시 오브젝트 역순 순회
            { // 임시 오브젝트 제거 처리
                if (temporaryObjects[objectIndex] != null) // 현재 임시 오브젝트 존재 확인
                { // 현재 임시 오브젝트 제거 처리
                    Object.DestroyImmediate(temporaryObjects[objectIndex]); // 현재 임시 오브젝트 즉시 제거
                } // 현재 임시 오브젝트 제거 처리 종료
            } // 임시 오브젝트 제거 처리 종료

            temporaryObjects.Clear(); // 임시 오브젝트 목록 초기화
        } // 테스트 오브젝트 제거 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void GeneratedMapCreatesSafeAndHighRiskBranches() // 생성된 맵의 안전과 고위험 분기 구성 확인
        { // 안전과 고위험 분기 생성 테스트 처리
            ProceduralMapGenerator generator = CreateConfiguredGenerator(); // 테스트용 수직 분기 생성기 구성
            generator.GenerateMap(); // 수직 분기와 장애물 생성 실행
            MapObstaclePlanReport report = generator.LastObstaclePlanReport; // 최근 장애물 계획 보고서 조회
            Assert.IsTrue(generator.LastGenerationSucceeded, report.BuildDetailedMessage()); // 생성과 경로와 장애물 계획 통합 성공 확인
            Assert.IsTrue(report.IsValid, report.BuildDetailedMessage()); // 장애물 계획 보고서 성공 확인
            Assert.AreNotEqual(report.SafeLaneIndex, report.HighRiskLaneIndex); // 안전과 고위험 분기 번호 차이 확인
            int safeRisk = report.SafeLaneIndex < 0 ? report.LeftBranch.TotalRiskScore : report.RightBranch.TotalRiskScore; // 안전 경로 총 위험도 조회
            int highRisk = report.HighRiskLaneIndex < 0 ? report.LeftBranch.TotalRiskScore : report.RightBranch.TotalRiskScore; // 고위험 경로 총 위험도 조회
            Assert.GreaterOrEqual(safeRisk, 6); // 안전 경로 최소 위험도 확인
            Assert.LessOrEqual(safeRisk, 12); // 안전 경로 최대 위험도 확인
            Assert.GreaterOrEqual(highRisk, 18); // 고위험 경로 최소 위험도 확인
            Assert.LessOrEqual(highRisk, 30); // 고위험 경로 최대 위험도 확인
            Assert.GreaterOrEqual(highRisk - safeRisk, 8); // 안전과 고위험 경로 최소 차이 확인
        } // 안전과 고위험 분기 생성 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void ObstaclePlacementsRemainOnBranchLanes() // 장애물이 공통 경로가 아닌 좌우 분기에만 배치되는지 확인
        { // 장애물 분기 한정 배치 테스트 처리
            ProceduralMapGenerator generator = CreateConfiguredGenerator(); // 테스트용 수직 분기 생성기 구성
            generator.GenerateMap(); // 수직 분기와 장애물 생성 실행
            MapObstaclePlanReport report = generator.LastObstaclePlanReport; // 최근 장애물 계획 보고서 조회
            Assert.Greater(report.PlacementCount, 0); // 한 개 이상의 장애물 배치 확인

            for (int placementIndex = 0; placementIndex < report.Placements.Count; placementIndex++) // 모든 장애물 배치 기록 순회
            { // 단일 장애물 분기 번호 검사 처리
                MapObstaclePlacementRecord placement = report.Placements[placementIndex]; // 현재 장애물 배치 기록 조회
                Assert.AreNotEqual(0, placement.LaneIndex); // 공통 경로 장애물 미배치 확인
                Assert.IsTrue(placement.LaneIndex == -1 || placement.LaneIndex == 1); // 좌우 분기 번호 범위 확인
                Assert.GreaterOrEqual(placement.NodeIndex, 0); // 유효한 그래프 노드 번호 확인
            } // 단일 장애물 분기 번호 검사 처리 종료
        } // 장애물 분기 한정 배치 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void SameSeedCreatesSameObstaclePlanSignature() // 동일 시드 장애물 배치와 위험도 재현 확인
        { // 동일 시드 장애물 계획 재현 테스트 처리
            ProceduralMapGenerator generator = CreateConfiguredGenerator(); // 테스트용 수직 분기 생성기 구성
            generator.GenerateMap(); // 첫 수직 분기와 장애물 생성 실행
            string firstObstacleSignature = generator.LastObstaclePlanReport.BuildSignature(); // 첫 장애물 계획 서명 저장
            string firstGenerationSignature = generator.GenerationSignature; // 첫 전체 생성 서명 저장
            generator.GenerateMap(); // 같은 설정으로 둘째 수직 분기와 장애물 생성 실행
            string secondObstacleSignature = generator.LastObstaclePlanReport.BuildSignature(); // 둘째 장애물 계획 서명 저장
            string secondGenerationSignature = generator.GenerationSignature; // 둘째 전체 생성 서명 저장
            Assert.AreEqual(firstObstacleSignature, secondObstacleSignature); // 동일 시드 장애물 계획 서명 일치 확인
            Assert.AreEqual(firstGenerationSignature, secondGenerationSignature); // 동일 시드 전체 생성 서명 일치 확인
        } // 동일 시드 장애물 계획 재현 테스트 처리 종료

        private ProceduralMapGenerator CreateConfiguredGenerator() // 테스트용 수직 분기 생성기와 모듈 구성
        { // 테스트용 수직 분기 생성기 구성 처리
            MapTraversalProfile traversalProfile = ScriptableObject.CreateInstance<MapTraversalProfile>(); // 테스트 이동 능력 에셋 생성
            traversalProfile.ConfigureForEditor(2f, 1.2f, 0.45f, 6f, 2.4f, 25f, 3f, 0.8f, 0.1f); // 기본 이동 능력 수치 적용
            temporaryObjects.Add(traversalProfile); // 정리 대상 이동 능력 에셋 등록
            MapModuleDefinition riseModule = CreateRiseModule(traversalProfile); // 테스트 2미터 상승 모듈 생성
            MapModuleDefinition branchModule = CreateBranchModule(traversalProfile); // 테스트 분기 모듈 생성
            MapModuleDefinition mergeModule = CreateMergeModule(traversalProfile); // 테스트 합류 모듈 생성
            MapGenerationSettings settings = ScriptableObject.CreateInstance<MapGenerationSettings>(); // 테스트 생성 설정 에셋 생성
            settings.ConfigureVerticalBranchingForEditor(36001, false, 0, 8, 128, 0.05f, 0.05f, 0.02f, 2, 8f, 8f, 3, 2, false, 2, 0.02f, 16, new[] { riseModule, branchModule, mergeModule }); // 고정 목표 수직 분기 생성 설정 적용
            temporaryObjects.Add(settings); // 정리 대상 생성 설정 등록
            GameObject generatorObject = new GameObject("TestVerticalBranchMapGenerator"); // 테스트 수직 분기 생성기 오브젝트 생성
            temporaryObjects.Add(generatorObject); // 정리 대상 수직 분기 생성기 등록
            GameObject generatedRoot = new GameObject("GeneratedMap"); // 테스트 생성 결과 루트 생성
            generatedRoot.transform.SetParent(generatorObject.transform, false); // 생성기 아래 결과 루트 배치
            ProceduralMapGenerator generator = generatorObject.AddComponent<ProceduralMapGenerator>(); // 테스트 맵 생성기 컴포넌트 추가
            generator.ConfigureForEditor(settings, generatedRoot.transform, false, false); // 테스트 생성기 참조 연결
            ObstacleDataDefinition obstacleData = CreateObstacleData(); // 테스트용 위험도 6 장애물 데이터 생성
            MapBranchObstaclePlanner planner = generatorObject.AddComponent<MapBranchObstaclePlanner>(); // 테스트 분기 장애물 계획기 추가
            planner.ConfigureForEditor(generator, true, new[] { obstacleData }, 6, 12, 18, 30, 8, 2); // 안전과 고위험 경로 테스트 예산 적용
            return generator; // 구성된 수직 분기 생성기 반환
        } // 테스트용 수직 분기 생성기 구성 처리 종료

        private MapModuleDefinition CreateRiseModule(MapTraversalProfile traversalProfile) // 테스트용 2미터 상승 모듈 생성
        { // 테스트 상승 모듈 생성 처리
            GameObject root = CreateModuleRoot("TestRise", true); // 수직 데이터 포함 모듈 루트 생성
            CreateConnection(root.transform, "Entrance", new Vector3(0f, 0f, -4f), MapConnectionRole.Entrance, MapConnectionDirection.South); // 0미터 남쪽 입구 생성
            CreateConnection(root.transform, "Exit", new Vector3(0f, 2f, 4f), MapConnectionRole.Exit, MapConnectionDirection.North); // 2미터 북쪽 출구 생성
            MapModuleDefinition definition = root.GetComponent<MapModuleDefinition>(); // 상승 모듈 정의 조회
            definition.ConfigureForEditor("MAP-VB01", MapModuleKind.StepRise, MapTraversalRequirement.LedgeClimb, MapRotationOptions.Degrees0, new Vector3(0f, 1.25f, 0f), new Vector3(4f, 2.5f, 8f), 2.2f, 1f, 0.25f, traversalProfile); // 상승 모듈 기본 데이터 적용
            CreateObstacleSpawnPoint(root.transform, "ObstaclePoint_Left", new Vector3(-1.2f, 0.5f, 0f)); // 상승 모듈 왼쪽 장애물 지점 생성
            CreateObstacleSpawnPoint(root.transform, "ObstaclePoint_Right", new Vector3(1.2f, 0.5f, 0f)); // 상승 모듈 오른쪽 장애물 지점 생성
            definition.RefreshConnectionPoints(); // 상승 모듈 연결 지점 수집
            MapVerticalTraversalSegment[] segments = // 테스트 상승 구간 배열 선언
            { // 테스트 상승 구간 묶음
                new MapVerticalTraversalSegment("Step_01", MapTraversalRequirement.LedgeClimb, 0.25f, 1f), // 첫 계단 상승 구간
                new MapVerticalTraversalSegment("Step_02", MapTraversalRequirement.LedgeClimb, 0.25f, 1f), // 둘째 계단 상승 구간
                new MapVerticalTraversalSegment("Step_03", MapTraversalRequirement.LedgeClimb, 0.25f, 1f), // 셋째 계단 상승 구간
                new MapVerticalTraversalSegment("Step_04", MapTraversalRequirement.LedgeClimb, 0.25f, 1f), // 넷째 계단 상승 구간
                new MapVerticalTraversalSegment("Step_05", MapTraversalRequirement.LedgeClimb, 0.25f, 1f), // 다섯째 계단 상승 구간
                new MapVerticalTraversalSegment("Step_06", MapTraversalRequirement.LedgeClimb, 0.25f, 1f), // 여섯째 계단 상승 구간
                new MapVerticalTraversalSegment("Step_07", MapTraversalRequirement.LedgeClimb, 0.25f, 1f), // 일곱째 계단 상승 구간
                new MapVerticalTraversalSegment("Step_08", MapTraversalRequirement.LedgeClimb, 0.25f, 1f) // 여덟째 계단 상승 구간
            }; // 테스트 상승 구간 묶음 종료
            MapVerticalModuleData verticalData = root.GetComponent<MapVerticalModuleData>(); // 상승 모듈 수직 데이터 조회
            verticalData.ConfigureForEditor(MapVerticalLayoutKind.StepRise, "Entrance", "Exit", 2f, segments, traversalProfile); // 2미터 수직 데이터 적용
            return definition; // 테스트 상승 모듈 반환
        } // 테스트 상승 모듈 생성 처리 종료

        private ObstacleDataDefinition CreateObstacleData() // 테스트용 위험도 6 장애물 데이터 생성
        { // 테스트 장애물 데이터 생성 처리
            GameObject obstaclePrefab = new GameObject("TestObstaclePrefab"); // 빈 테스트 장애물 Prefab 대체 오브젝트 생성
            temporaryObjects.Add(obstaclePrefab); // 정리 대상 장애물 Prefab 대체 오브젝트 등록
            ObstacleDataDefinition obstacleData = ScriptableObject.CreateInstance<ObstacleDataDefinition>(); // 임시 장애물 데이터 인스턴스 생성
            obstacleData.SetEditorIdentity("OBS-TEST", "Test Branch Obstacle", new ProjectDataVersion(1, 0, 0)); // 테스트 장애물 식별 정보 적용
            obstacleData.ConfigureObstacleForEditor(obstaclePrefab, 6, new Vector3(0.8f, 1f, 0.8f), true, ObstacleTraversalEffect.Slow); // 테스트 장애물 위험도와 점유 크기 적용
            temporaryObjects.Add(obstacleData); // 정리 대상 장애물 데이터 등록
            return obstacleData; // 구성된 테스트 장애물 데이터 반환
        } // 테스트 장애물 데이터 생성 처리 종료

        private MapObstacleSpawnPoint CreateObstacleSpawnPoint(Transform parent, string pointId, Vector3 localPosition) // 테스트용 장애물 배치 지점 생성
        { // 테스트 장애물 배치 지점 생성 처리
            GameObject pointObject = new GameObject(pointId); // 빈 테스트 장애물 지점 오브젝트 생성
            pointObject.transform.SetParent(parent, false); // 테스트 모듈 아래 배치 지점 배치
            pointObject.transform.localPosition = localPosition; // 배치 지점 로컬 위치 적용
            MapObstacleSpawnPoint spawnPoint = pointObject.AddComponent<MapObstacleSpawnPoint>(); // 장애물 배치 지점 컴포넌트 추가
            spawnPoint.ConfigureForEditor(pointId, 1.2f, 3f, 1.1f, 1f, true); // 안전한 통로 폭과 위험도 배율 적용
            return spawnPoint; // 구성된 장애물 배치 지점 반환
        } // 테스트 장애물 배치 지점 생성 처리 종료

        private MapModuleDefinition CreateBranchModule(MapTraversalProfile traversalProfile) // 테스트용 평면 분기 모듈 생성
        { // 테스트 분기 모듈 생성 처리
            GameObject root = CreateModuleRoot("TestBranch", false); // 일반 분기 모듈 루트 생성
            CreateConnection(root.transform, "Entrance", new Vector3(0f, 0f, -4f), MapConnectionRole.Entrance, MapConnectionDirection.South); // 중앙 남쪽 입구 생성
            CreateConnection(root.transform, "ExitLeft", new Vector3(-6f, 0f, 4f), MapConnectionRole.Exit, MapConnectionDirection.North); // 왼쪽 북쪽 출구 생성
            CreateConnection(root.transform, "ExitRight", new Vector3(6f, 0f, 4f), MapConnectionRole.Exit, MapConnectionDirection.North); // 오른쪽 북쪽 출구 생성
            MapModuleDefinition definition = root.GetComponent<MapModuleDefinition>(); // 분기 모듈 정의 조회
            definition.ConfigureForEditor("MAP-VB02", MapModuleKind.Branch, MapTraversalRequirement.Walk, MapRotationOptions.Degrees0, new Vector3(0f, 1f, 0f), new Vector3(16f, 2f, 8f), 2.2f, 0f, 0f, traversalProfile); // 분기 모듈 데이터 적용
            definition.RefreshConnectionPoints(); // 분기 모듈 연결 지점 수집
            return definition; // 테스트 분기 모듈 반환
        } // 테스트 분기 모듈 생성 처리 종료

        private MapModuleDefinition CreateMergeModule(MapTraversalProfile traversalProfile) // 테스트용 평면 합류 모듈 생성
        { // 테스트 합류 모듈 생성 처리
            GameObject root = CreateModuleRoot("TestMerge", false); // 일반 합류 모듈 루트 생성
            CreateConnection(root.transform, "EntranceLeft", new Vector3(-6f, 0f, -4f), MapConnectionRole.Entrance, MapConnectionDirection.South); // 왼쪽 남쪽 입구 생성
            CreateConnection(root.transform, "EntranceRight", new Vector3(6f, 0f, -4f), MapConnectionRole.Entrance, MapConnectionDirection.South); // 오른쪽 남쪽 입구 생성
            CreateConnection(root.transform, "Exit", new Vector3(0f, 0f, 4f), MapConnectionRole.Exit, MapConnectionDirection.North); // 중앙 북쪽 출구 생성
            MapModuleDefinition definition = root.GetComponent<MapModuleDefinition>(); // 합류 모듈 정의 조회
            definition.ConfigureForEditor("MAP-VB03", MapModuleKind.Merge, MapTraversalRequirement.Walk, MapRotationOptions.Degrees0, new Vector3(0f, 1f, 0f), new Vector3(16f, 2f, 8f), 2.2f, 0f, 0f, traversalProfile); // 합류 모듈 데이터 적용
            definition.RefreshConnectionPoints(); // 합류 모듈 연결 지점 수집
            return definition; // 테스트 합류 모듈 반환
        } // 테스트 합류 모듈 생성 처리 종료

        private GameObject CreateModuleRoot(string objectName, bool includeVerticalData) // 테스트용 모듈 루트 생성
        { // 테스트 모듈 루트 생성 처리
            GameObject root = new GameObject(objectName); // 빈 테스트 모듈 루트 생성
            root.AddComponent<MapModuleDefinition>(); // 맵 모듈 정의 컴포넌트 추가

            if (includeVerticalData) // 수직 데이터 포함 여부 확인
            { // 수직 데이터 포함 처리
                root.AddComponent<MapVerticalModuleData>(); // 수직 모듈 데이터 컴포넌트 추가
            } // 수직 데이터 포함 처리 종료

            temporaryObjects.Add(root); // 정리 대상 테스트 모듈 등록
            return root; // 생성된 테스트 모듈 루트 반환
        } // 테스트 모듈 루트 생성 처리 종료

        private MapModuleConnectionPoint CreateConnection(Transform parent, string connectionId, Vector3 localPosition, MapConnectionRole role, MapConnectionDirection direction) // 테스트용 연결 지점 생성
        { // 테스트 연결 지점 생성 처리
            GameObject connectionObject = new GameObject(connectionId); // 빈 테스트 연결 오브젝트 생성
            connectionObject.transform.SetParent(parent, false); // 테스트 모듈 아래 연결 지점 배치
            connectionObject.transform.localPosition = localPosition; // 연결 지점 로컬 위치 적용
            MapModuleConnectionPoint point = connectionObject.AddComponent<MapModuleConnectionPoint>(); // 연결 지점 컴포넌트 추가
            point.ConfigureForEditor(connectionId, role, direction, 2f, 2.2f); // 연결 지점 공통 데이터 적용
            return point; // 생성된 테스트 연결 지점 반환
        } // 테스트 연결 지점 생성 처리 종료
    } // 생성 맵 분기 장애물 계획 통합 자동 테스트 묶음 종료
} // EditMode 테스트 묶음 종료
