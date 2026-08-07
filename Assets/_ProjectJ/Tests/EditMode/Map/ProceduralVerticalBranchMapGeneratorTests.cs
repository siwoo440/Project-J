using System.Collections.Generic; // 목록 기능 참조
using NUnit.Framework; // NUnit 자동 테스트 기능 참조
using ProjectJ.MapGeneration; // 맵 생성 Runtime 기능 참조
using UnityEngine; // Unity 오브젝트와 벡터 기능 참조

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스 선언
{ // EditMode 테스트 묶음
    public sealed class ProceduralVerticalBranchMapGeneratorTests // 수직 분기 맵 생성기 통합 자동 테스트 선언
    { // 수직 분기 맵 생성기 통합 테스트 묶음
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
        public void FixedSeedCreatesReachableVerticalBranchGraph() // 고정 시드의 8개 수직 분기 그래프 생성 확인
        { // 고정 시드 수직 분기 생성 테스트 처리
            ProceduralMapGenerator generator = CreateConfiguredGenerator(); // 테스트용 수직 분기 생성기 구성
            generator.GenerateMap(); // 수직 분기 맵 생성 실행
            Assert.IsTrue(generator.LastGenerationSucceeded, generator.LastValidationReport.BuildDetailedMessage()); // 전체 생성과 종합 검사 성공 확인
            Assert.AreEqual(8, generator.GeneratedModuleCount); // 목표 모듈 8개 생성 확인
            Assert.AreEqual(8, generator.GraphNodes.Count); // 그래프 노드 8개 확인
            Assert.AreEqual(8, generator.GraphEdges.Count); // 분기와 합류를 포함한 간선 8개 확인
            Assert.IsTrue(MapGenerationGraphRules.AreAllNodesReachable(generator.GraphNodes.Count, generator.GraphEdges, 0)); // 시작점 기준 전체 노드 도달 확인
            Assert.AreEqual(8f, generator.GeneratedHeight, 0.02f); // 단일 경로 최종 높이 8미터 확인
            Assert.GreaterOrEqual(generator.AscendingModuleCount, 3); // 단일 경로 최소 상승 모듈 수 확인
        } // 고정 시드 수직 분기 생성 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void BothBranchesReachSameMergeHeight() // 좌우 분기의 합류 높이와 상승 수 일치 확인
        { // 좌우 분기 합류 테스트 처리
            ProceduralMapGenerator generator = CreateConfiguredGenerator(); // 테스트용 수직 분기 생성기 구성
            generator.GenerateMap(); // 수직 분기 맵 생성 실행
            Assert.AreEqual(generator.LeftBranchHeight, generator.RightBranchHeight, 0.02f); // 좌우 누적 분기 높이 일치 확인
            Assert.AreEqual(4f, generator.LeftBranchHeight, 0.02f); // 왼쪽 분기 4미터 상승 확인
            Assert.AreEqual(2, generator.LeftBranchAscendingModuleCount); // 왼쪽 상승 모듈 2개 확인
            Assert.AreEqual(2, generator.RightBranchAscendingModuleCount); // 오른쪽 상승 모듈 2개 확인
            Assert.AreEqual(0, generator.BranchCombinationRetryCount); // 첫 수직 분기 조합 성공 확인
        } // 좌우 분기 합류 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void SameSeedCreatesSameVerticalBranchSignature() // 동일 시드 수직 분기 결과 재현 확인
        { // 동일 시드 수직 분기 재현 테스트 처리
            ProceduralMapGenerator generator = CreateConfiguredGenerator(); // 테스트용 수직 분기 생성기 구성
            generator.GenerateMap(); // 첫 수직 분기 맵 생성 실행
            string firstSignature = generator.GenerationSignature; // 첫 수직 분기 생성 서명 저장
            generator.GenerateMap(); // 같은 설정으로 둘째 수직 분기 맵 생성 실행
            string secondSignature = generator.GenerationSignature; // 둘째 수직 분기 생성 서명 저장
            Assert.IsNotEmpty(firstSignature); // 첫 수직 분기 생성 서명 존재 확인
            Assert.AreEqual(firstSignature, secondSignature); // 동일 시드 수직 분기 구조 일치 확인
        } // 동일 시드 수직 분기 재현 테스트 처리 종료

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
            return generator; // 구성된 수직 분기 생성기 반환
        } // 테스트용 수직 분기 생성기 구성 처리 종료

        private MapModuleDefinition CreateRiseModule(MapTraversalProfile traversalProfile) // 테스트용 2미터 상승 모듈 생성
        { // 테스트 상승 모듈 생성 처리
            GameObject root = CreateModuleRoot("TestRise", true); // 수직 데이터 포함 모듈 루트 생성
            CreateConnection(root.transform, "Entrance", new Vector3(0f, 0f, -4f), MapConnectionRole.Entrance, MapConnectionDirection.South); // 0미터 남쪽 입구 생성
            CreateConnection(root.transform, "Exit", new Vector3(0f, 2f, 4f), MapConnectionRole.Exit, MapConnectionDirection.North); // 2미터 북쪽 출구 생성
            MapModuleDefinition definition = root.GetComponent<MapModuleDefinition>(); // 상승 모듈 정의 조회
            definition.ConfigureForEditor("MAP-VB01", MapModuleKind.StepRise, MapTraversalRequirement.LedgeClimb, MapRotationOptions.Degrees0, new Vector3(0f, 1.25f, 0f), new Vector3(4f, 2.5f, 8f), 2.2f, 1f, 0.25f, traversalProfile); // 상승 모듈 기본 데이터 적용
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
    } // 수직 분기 맵 생성기 통합 자동 테스트 묶음 종료
} // EditMode 테스트 묶음 종료
