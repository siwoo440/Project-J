using System.Collections.Generic; // 목록 기능 참조
using NUnit.Framework; // NUnit 자동 테스트 기능 참조
using ProjectJ.MapGeneration; // 맵 생성 Runtime 기능 참조
using UnityEngine; // Unity 오브젝트와 벡터 기능 참조

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스 선언
{ // EditMode 테스트 묶음
    public sealed class MapGenerationResultValidatorTests // 생성 결과 종합 검사기 자동 테스트 선언
    { // 생성 결과 종합 검사기 자동 테스트 묶음
        private readonly List<Object> temporaryObjects = new List<Object>(); // 테스트 종료 시 제거할 임시 오브젝트 목록

        private sealed class ValidationFixture // 테스트용 생성 결과 묶음 선언
        { // 테스트용 생성 결과 묶음
            public List<MapModuleDefinition> Modules = new List<MapModuleDefinition>(); // 테스트 모듈 목록
            public List<MapGenerationGraphNode> Nodes = new List<MapGenerationGraphNode>(); // 테스트 그래프 노드 목록
            public List<MapGenerationGraphEdge> Edges = new List<MapGenerationGraphEdge>(); // 테스트 그래프 간선 목록
        } // 테스트용 생성 결과 묶음 종료

        [TearDown] // 각 테스트 종료 정리 항목 표시
        public void TearDown() // 테스트 임시 오브젝트 제거
        { // 테스트 임시 오브젝트 제거 처리
            for (int objectIndex = temporaryObjects.Count - 1; objectIndex >= 0; objectIndex--) // 임시 오브젝트 역순 순회
            { // 임시 오브젝트 제거 처리
                if (temporaryObjects[objectIndex] != null) // 현재 임시 오브젝트 존재 확인
                { // 현재 임시 오브젝트 제거 처리
                    Object.DestroyImmediate(temporaryObjects[objectIndex]); // 현재 임시 오브젝트 즉시 제거
                } // 현재 임시 오브젝트 제거 처리 종료
            } // 임시 오브젝트 제거 처리 종료

            temporaryObjects.Clear(); // 임시 오브젝트 목록 초기화
        } // 테스트 임시 오브젝트 제거 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void ValidConnectedMapPassesAllChecks() // 정상 연결 맵 전체 검사 통과 확인
        { // 정상 연결 맵 검사 테스트 처리
            ValidationFixture fixture = CreateValidFixture(); // 정상 테스트 생성 결과 구성
            MapGenerationValidationReport report = ValidateFixture(fixture); // 정상 생성 결과 종합 검사 실행
            Assert.IsTrue(report.IsCompleted); // 검사 완료 상태 확인
            Assert.IsTrue(report.IsValid, report.BuildDetailedMessage()); // 전체 검사 성공 확인
            Assert.AreEqual(0, report.IssueCount); // 발견 문제 없음 확인
        } // 정상 연결 맵 검사 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void DisconnectedMapReportsReachabilityFailure() // 끊어진 맵 도달 실패 보고 확인
        { // 끊어진 맵 검사 테스트 처리
            ValidationFixture fixture = CreateValidFixture(); // 정상 테스트 생성 결과 구성
            fixture.Edges.Clear(); // 모듈 사이 간선 제거
            MapGenerationValidationReport report = ValidateFixture(fixture); // 끊어진 생성 결과 종합 검사 실행
            Assert.IsFalse(report.IsValid); // 전체 검사 실패 확인
            Assert.IsTrue(report.Contains(MapGenerationValidationIssueCode.DisconnectedGraph)); // 끊어진 경로 문제 포함 확인
        } // 끊어진 맵 검사 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void MissingConnectionIdReportsExactFailure() // 잘못된 연결 ID 오류 보고 확인
        { // 잘못된 연결 ID 검사 테스트 처리
            ValidationFixture fixture = CreateValidFixture(); // 정상 테스트 생성 결과 구성
            fixture.Edges[0] = new MapGenerationGraphEdge(0, 1, "MissingExit", "Entrance"); // 존재하지 않는 출구 ID 적용
            MapGenerationValidationReport report = ValidateFixture(fixture); // 잘못된 연결 ID 생성 결과 검사
            Assert.IsFalse(report.IsValid); // 전체 검사 실패 확인
            Assert.IsTrue(report.Contains(MapGenerationValidationIssueCode.MissingExitConnection)); // 출구 누락 문제 포함 확인
        } // 잘못된 연결 ID 검사 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void OverlappingModulesReportBoundsFailure() // 겹친 모듈 영역 오류 보고 확인
        { // 겹친 모듈 검사 테스트 처리
            ValidationFixture fixture = CreateValidFixture(); // 정상 테스트 생성 결과 구성
            fixture.Modules[1].transform.position = new Vector3(0f, 0f, 7f); // 둘째 모듈을 첫 모듈 영역 안으로 이동
            fixture.Nodes[1] = new MapGenerationGraphNode(1, fixture.Modules[1].ModuleId, 0, fixture.Modules[1].transform.position); // 이동된 둘째 노드 위치 갱신
            MapGenerationValidationReport report = ValidateFixture(fixture); // 겹친 생성 결과 종합 검사 실행
            Assert.IsFalse(report.IsValid); // 전체 검사 실패 확인
            Assert.IsTrue(report.Contains(MapGenerationValidationIssueCode.ModuleOverlap)); // 모듈 겹침 문제 포함 확인
        } // 겹친 모듈 검사 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void UnsafeHeightDifferenceReportsTraversalFailure() // 이동 불가능 높이 차이 오류 보고 확인
        { // 이동 불가능 높이 검사 테스트 처리
            ValidationFixture fixture = CreateValidFixture(); // 정상 테스트 생성 결과 구성
            fixture.Modules[1].transform.position = new Vector3(0f, 4f, 8f); // 둘째 모듈을 안전 상승 높이 밖으로 이동
            fixture.Nodes[1] = new MapGenerationGraphNode(1, fixture.Modules[1].ModuleId, 0, fixture.Modules[1].transform.position); // 이동된 둘째 노드 위치 갱신
            MapGenerationValidationReport report = ValidateFixture(fixture); // 높이 차이 생성 결과 종합 검사 실행
            Assert.IsFalse(report.IsValid); // 전체 검사 실패 확인
            Assert.IsTrue(report.Contains(MapGenerationValidationIssueCode.TraversalHeightExceeded)); // 이동 높이 초과 문제 포함 확인
        } // 이동 불가능 높이 검사 테스트 처리 종료

        private ValidationFixture CreateValidFixture() // 정상 두 모듈 생성 결과 구성
        { // 정상 두 모듈 생성 결과 구성 처리
            MapTraversalProfile traversalProfile = ScriptableObject.CreateInstance<MapTraversalProfile>(); // 테스트 이동 능력 에셋 생성
            traversalProfile.ConfigureForEditor(2f, 1.2f, 0.45f, 6f, 2.4f, 25f, 3f, 0.8f, 0.1f); // 기본 이동 능력 수치 적용
            temporaryObjects.Add(traversalProfile); // 정리 대상 이동 능력 에셋 등록
            MapModuleDefinition firstModule = CreateModule("FirstModule", "MAP-V01", Vector3.zero, traversalProfile); // 첫 테스트 모듈 생성
            MapModuleDefinition secondModule = CreateModule("SecondModule", "MAP-V02", new Vector3(0f, 0f, 8f), traversalProfile); // 둘째 테스트 모듈 생성
            ValidationFixture fixture = new ValidationFixture(); // 새 테스트 생성 결과 묶음 생성
            fixture.Modules.Add(firstModule); // 첫 모듈 목록 등록
            fixture.Modules.Add(secondModule); // 둘째 모듈 목록 등록
            fixture.Nodes.Add(new MapGenerationGraphNode(0, firstModule.ModuleId, 0, firstModule.transform.position)); // 첫 그래프 노드 등록
            fixture.Nodes.Add(new MapGenerationGraphNode(1, secondModule.ModuleId, 0, secondModule.transform.position)); // 둘째 그래프 노드 등록
            fixture.Edges.Add(new MapGenerationGraphEdge(0, 1, "Exit", "Entrance")); // 첫 모듈에서 둘째 모듈 간선 등록
            return fixture; // 완성된 테스트 생성 결과 반환
        } // 정상 두 모듈 생성 결과 구성 처리 종료

        private MapModuleDefinition CreateModule(string objectName, string moduleId, Vector3 worldPosition, MapTraversalProfile traversalProfile) // 테스트용 직선 모듈 생성
        { // 테스트용 직선 모듈 생성 처리
            GameObject root = new GameObject(objectName); // 빈 테스트 모듈 루트 생성
            root.transform.position = worldPosition; // 테스트 모듈 월드 위치 적용
            MapModuleDefinition definition = root.AddComponent<MapModuleDefinition>(); // 맵 모듈 정의 컴포넌트 추가
            temporaryObjects.Add(root); // 정리 대상 테스트 모듈 등록
            CreateConnection(root.transform, "Entrance", new Vector3(0f, 0f, -4f), MapConnectionRole.Entrance, MapConnectionDirection.South); // 남쪽 입구 생성
            CreateConnection(root.transform, "Exit", new Vector3(0f, 0f, 4f), MapConnectionRole.Exit, MapConnectionDirection.North); // 북쪽 출구 생성
            definition.ConfigureForEditor(moduleId, MapModuleKind.FixedPlatform, MapTraversalRequirement.Walk, MapRotationOptions.All, new Vector3(0f, 1f, 0f), new Vector3(4f, 2f, 8f), 2.2f, 0f, 0f, traversalProfile); // 직선 모듈 공통 데이터 적용
            definition.RefreshConnectionPoints(); // 직선 모듈 연결 지점 수집
            return definition; // 완성된 테스트 모듈 반환
        } // 테스트용 직선 모듈 생성 처리 종료

        private MapModuleConnectionPoint CreateConnection(Transform parent, string connectionId, Vector3 localPosition, MapConnectionRole role, MapConnectionDirection direction) // 테스트용 연결 지점 생성
        { // 테스트용 연결 지점 생성 처리
            GameObject connectionObject = new GameObject(connectionId); // 빈 테스트 연결 오브젝트 생성
            connectionObject.transform.SetParent(parent, false); // 테스트 모듈 아래 연결 지점 배치
            connectionObject.transform.localPosition = localPosition; // 연결 지점 로컬 위치 적용
            MapModuleConnectionPoint point = connectionObject.AddComponent<MapModuleConnectionPoint>(); // 연결 지점 컴포넌트 추가
            point.ConfigureForEditor(connectionId, role, direction, 2f, 2.2f); // 연결 지점 공통 데이터 적용
            return point; // 완성된 테스트 연결 지점 반환
        } // 테스트용 연결 지점 생성 처리 종료

        private MapGenerationValidationReport ValidateFixture(ValidationFixture fixture) // 테스트 생성 결과 공통 검사
        { // 테스트 생성 결과 공통 검사 처리
            return MapGenerationResultValidator.Validate(fixture.Modules, fixture.Nodes, fixture.Edges, 2, 0.05f, 0.05f, 0.02f); // 공통 허용 오차 기반 검사 결과 반환
        } // 테스트 생성 결과 공통 검사 처리 종료
    } // 생성 결과 종합 검사기 자동 테스트 묶음 종료
} // EditMode 테스트 묶음 종료
