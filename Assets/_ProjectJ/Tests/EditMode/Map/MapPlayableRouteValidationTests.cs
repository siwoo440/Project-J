using System.Collections.Generic; // 목록 기능 참조
using NUnit.Framework; // NUnit 자동 테스트 기능 참조
using ProjectJ.MapGeneration; // 맵 생성 Runtime 기능 참조
using UnityEngine; // Unity 오브젝트와 벡터 기능 참조

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스 선언
{ // EditMode 테스트 묶음
    public sealed class MapPlayableRouteValidationTests // 플레이 가능 경로 검사 자동 테스트 선언
    { // 플레이 가능 경로 검사 테스트 묶음
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
        public void BranchedGraphCreatesTwoPlayableRoutes() // 정상 수직 분기 그래프의 좌우 경로 두 개 확인
        { // 정상 수직 분기 경로 테스트 처리
            List<MapModuleDefinition> modules = CreateValidModules(8); // 정상 모듈 8개 생성
            List<MapGenerationGraphNode> nodes = CreateVerticalBranchNodes(); // 좌우 분기 노드 8개 생성
            List<MapGenerationGraphEdge> edges = CreateVerticalBranchEdges(); // 좌우 분기 간선 8개 생성
            MapPlayableRouteReport report = MapPlayableRouteValidator.Validate(modules, nodes, edges, 0, 16, true); // 좌우 분기 경로 전체 검사
            Assert.IsTrue(report.IsValid, report.BuildDetailedMessage()); // 전체 플레이 가능성 성공 확인
            Assert.AreEqual(2, report.RouteCount); // 왼쪽과 오른쪽 경로 두 개 확인
            Assert.AreEqual("0>1>2>4>6>7", report.Routes[0].BuildSignature()); // 첫 경로 노드 순서 확인
            Assert.AreEqual("0>1>3>5>6>7", report.Routes[1].BuildSignature()); // 둘째 경로 노드 순서 확인
        } // 정상 수직 분기 경로 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void MultipleFinishNodesAreRejected() // 합류되지 않은 막다른 종료 두 개 검출 확인
        { // 다중 종료 노드 테스트 처리
            List<MapModuleDefinition> modules = CreateValidModules(4); // 정상 모듈 4개 생성
            List<MapGenerationGraphNode> nodes = CreateNodes(new[] { 0, 0, -1, 1 }); // 중앙과 좌우 노드 생성
            List<MapGenerationGraphEdge> edges = new List<MapGenerationGraphEdge> // 합류 없는 분기 간선 목록 생성
            { // 합류 없는 분기 간선 묶음
                CreateEdge(0, 1), // 시작에서 분기 연결
                CreateEdge(1, 2), // 분기에서 왼쪽 종료 연결
                CreateEdge(1, 3) // 분기에서 오른쪽 종료 연결
            }; // 합류 없는 분기 간선 묶음 종료
            MapPlayableRouteReport report = MapPlayableRouteValidator.Validate(modules, nodes, edges, 0, 16, true); // 합류 없는 분기 경로 검사
            Assert.IsFalse(report.IsValid); // 다중 종료 구조 실패 확인
            Assert.IsTrue(report.Contains(MapPlayableRouteIssueCode.MultipleFinishNodes)); // 다중 종료 문제 포함 확인
        } // 다중 종료 노드 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void CycleInsideRouteIsRejected() // 종료 경로 옆 순환 간선 검출 확인
        { // 순환 경로 테스트 처리
            List<MapModuleDefinition> modules = CreateValidModules(4); // 정상 모듈 4개 생성
            List<MapGenerationGraphNode> nodes = CreateNodes(new[] { 0, 0, -1, 0 }); // 순환 검사용 노드 생성
            List<MapGenerationGraphEdge> edges = new List<MapGenerationGraphEdge> // 순환 포함 간선 목록 생성
            { // 순환 포함 간선 묶음
                CreateEdge(0, 1), // 시작에서 중간 노드 연결
                CreateEdge(1, 2), // 중간에서 순환 노드 연결
                CreateEdge(2, 1), // 순환 노드에서 중간 노드 역연결
                CreateEdge(1, 3) // 중간에서 정상 종료 연결
            }; // 순환 포함 간선 묶음 종료
            MapPlayableRouteReport report = MapPlayableRouteValidator.Validate(modules, nodes, edges, 0, 16, false); // 순환 포함 경로 검사
            Assert.IsFalse(report.IsValid); // 순환 구조 실패 확인
            Assert.IsTrue(report.Contains(MapPlayableRouteIssueCode.CycleDetected)); // 순환 경로 문제 포함 확인
        } // 순환 경로 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void MissingRightBranchIsRejected() // 오른쪽 분기 경로 누락 검출 확인
        { // 오른쪽 분기 누락 테스트 처리
            List<MapModuleDefinition> modules = CreateValidModules(3); // 정상 모듈 3개 생성
            List<MapGenerationGraphNode> nodes = CreateNodes(new[] { 0, -1, 0 }); // 왼쪽만 포함한 노드 생성
            List<MapGenerationGraphEdge> edges = new List<MapGenerationGraphEdge> // 단일 왼쪽 경로 간선 목록 생성
            { // 단일 왼쪽 경로 간선 묶음
                CreateEdge(0, 1), // 시작에서 왼쪽 연결
                CreateEdge(1, 2) // 왼쪽에서 종료 연결
            }; // 단일 왼쪽 경로 간선 묶음 종료
            MapPlayableRouteReport report = MapPlayableRouteValidator.Validate(modules, nodes, edges, 0, 16, true); // 좌우 필수 조건 경로 검사
            Assert.IsFalse(report.IsValid); // 오른쪽 경로 누락 실패 확인
            Assert.IsTrue(report.Contains(MapPlayableRouteIssueCode.MissingRightBranch)); // 오른쪽 분기 누락 문제 포함 확인
        } // 오른쪽 분기 누락 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void MissingModuleInsideRouteIsRejected() // 경로 중간 모듈 누락 검출 확인
        { // 경로 모듈 누락 테스트 처리
            List<MapModuleDefinition> modules = CreateValidModules(3); // 정상 모듈 3개 생성
            modules[1] = null; // 중간 노드 모듈 참조 제거
            List<MapGenerationGraphNode> nodes = CreateNodes(new[] { 0, 0, 0 }); // 선형 노드 3개 생성
            List<MapGenerationGraphEdge> edges = new List<MapGenerationGraphEdge> // 선형 간선 목록 생성
            { // 선형 간선 묶음
                CreateEdge(0, 1), // 첫 노드에서 중간 노드 연결
                CreateEdge(1, 2) // 중간 노드에서 종료 연결
            }; // 선형 간선 묶음 종료
            MapPlayableRouteReport report = MapPlayableRouteValidator.Validate(modules, nodes, edges, 0, 16, false); // 모듈 누락 경로 검사
            Assert.IsFalse(report.IsValid); // 모듈 누락 경로 실패 확인
            Assert.IsTrue(report.Contains(MapPlayableRouteIssueCode.MissingModule)); // 모듈 누락 문제 포함 확인
        } // 경로 모듈 누락 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void RouteCountOverLimitIsRejected() // 과도한 분기 경로 수 제한 확인
        { // 경로 탐색 제한 테스트 처리
            List<MapModuleDefinition> modules = CreateValidModules(5); // 정상 모듈 5개 생성
            List<MapGenerationGraphNode> nodes = CreateNodes(new[] { 0, -1, 0, 1, 0 }); // 세 갈래 분기 노드 생성
            List<MapGenerationGraphEdge> edges = new List<MapGenerationGraphEdge> // 세 갈래 경로 간선 목록 생성
            { // 세 갈래 경로 간선 묶음
                CreateEdge(0, 1), // 시작에서 첫 분기 연결
                CreateEdge(0, 2), // 시작에서 둘째 분기 연결
                CreateEdge(0, 3), // 시작에서 셋째 분기 연결
                CreateEdge(1, 4), // 첫 분기에서 종료 연결
                CreateEdge(2, 4), // 둘째 분기에서 종료 연결
                CreateEdge(3, 4) // 셋째 분기에서 종료 연결
            }; // 세 갈래 경로 간선 묶음 종료
            MapPlayableRouteReport report = MapPlayableRouteValidator.Validate(modules, nodes, edges, 0, 2, false); // 최대 두 경로 제한으로 검사
            Assert.IsFalse(report.IsValid); // 경로 제한 초과 실패 확인
            Assert.IsTrue(report.Contains(MapPlayableRouteIssueCode.RouteLimitExceeded)); // 경로 제한 문제 포함 확인
            Assert.AreEqual(2, report.RouteCount); // 제한 안의 첫 두 경로만 기록 확인
        } // 경로 탐색 제한 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void SameGraphCreatesSameRouteSignatures() // 같은 그래프의 경로 탐색 순서 재현 확인
        { // 경로 순서 재현 테스트 처리
            List<MapModuleDefinition> modules = CreateValidModules(8); // 정상 모듈 8개 생성
            List<MapGenerationGraphNode> nodes = CreateVerticalBranchNodes(); // 좌우 분기 노드 8개 생성
            List<MapGenerationGraphEdge> edges = CreateVerticalBranchEdges(); // 좌우 분기 간선 8개 생성
            MapPlayableRouteReport firstReport = MapPlayableRouteValidator.Validate(modules, nodes, edges, 0, 16, true); // 첫 경로 검사 실행
            MapPlayableRouteReport secondReport = MapPlayableRouteValidator.Validate(modules, nodes, edges, 0, 16, true); // 둘째 경로 검사 실행
            Assert.AreEqual(firstReport.Routes[0].BuildSignature(), secondReport.Routes[0].BuildSignature()); // 첫 경로 서명 재현 확인
            Assert.AreEqual(firstReport.Routes[1].BuildSignature(), secondReport.Routes[1].BuildSignature()); // 둘째 경로 서명 재현 확인
        } // 경로 순서 재현 테스트 처리 종료

        private List<MapModuleDefinition> CreateValidModules(int count) // 테스트용 정상 이동 모듈 목록 생성
        { // 정상 이동 모듈 목록 생성 처리
            MapTraversalProfile traversalProfile = ScriptableObject.CreateInstance<MapTraversalProfile>(); // 테스트 이동 능력 에셋 생성
            traversalProfile.ConfigureForEditor(2f, 1.2f, 0.45f, 6f, 2.4f, 25f, 3f, 0.8f, 0.1f); // 기본 이동 능력 수치 적용
            temporaryObjects.Add(traversalProfile); // 정리 대상 이동 능력 에셋 등록
            List<MapModuleDefinition> modules = new List<MapModuleDefinition>(); // 정상 모듈 결과 목록 생성

            for (int moduleIndex = 0; moduleIndex < count; moduleIndex++) // 요청 개수만큼 모듈 순회 생성
            { // 단일 정상 모듈 생성 처리
                GameObject root = new GameObject($"TestModule_{moduleIndex}"); // 빈 테스트 모듈 루트 생성
                temporaryObjects.Add(root); // 정리 대상 테스트 모듈 등록
                MapModuleDefinition definition = root.AddComponent<MapModuleDefinition>(); // 맵 모듈 정의 컴포넌트 추가
                CreateConnection(root.transform, "Entrance", new Vector3(0f, 0f, -4f), MapConnectionRole.Entrance, MapConnectionDirection.South); // 남쪽 입구 생성
                CreateConnection(root.transform, "Exit", new Vector3(0f, 0f, 4f), MapConnectionRole.Exit, MapConnectionDirection.North); // 북쪽 출구 생성
                definition.ConfigureForEditor($"MAP-RT{moduleIndex:00}", MapModuleKind.FixedPlatform, MapTraversalRequirement.Walk, MapRotationOptions.Degrees0, new Vector3(0f, 1f, 0f), new Vector3(4f, 2f, 8f), 2.2f, 0f, 0f, traversalProfile); // 정상 걷기 모듈 데이터 적용
                definition.RefreshConnectionPoints(); // 모듈 연결 지점 목록 갱신
                modules.Add(definition); // 정상 모듈 결과 목록 추가
            } // 단일 정상 모듈 생성 처리 종료

            return modules; // 완성된 정상 모듈 목록 반환
        } // 정상 이동 모듈 목록 생성 처리 종료

        private List<MapGenerationGraphNode> CreateVerticalBranchNodes() // 표준 8개 수직 분기 그래프 노드 생성
        { // 표준 수직 분기 노드 생성 처리
            return CreateNodes(new[] { 0, 0, -1, 1, -1, 1, 0, 0 }); // 중앙과 좌우 경로 번호가 적용된 노드 반환
        } // 표준 수직 분기 노드 생성 처리 종료

        private List<MapGenerationGraphNode> CreateNodes(int[] laneIndices) // 지정 경로 번호 기반 그래프 노드 목록 생성
        { // 그래프 노드 목록 생성 처리
            List<MapGenerationGraphNode> nodes = new List<MapGenerationGraphNode>(); // 그래프 노드 결과 목록 생성

            for (int nodeIndex = 0; nodeIndex < laneIndices.Length; nodeIndex++) // 모든 경로 번호 순회
            { // 단일 그래프 노드 생성 처리
                nodes.Add(new MapGenerationGraphNode(nodeIndex, $"MAP-RT{nodeIndex:00}", laneIndices[nodeIndex], new Vector3(laneIndices[nodeIndex] * 6f, nodeIndex, nodeIndex * 8f))); // 번호와 경로와 위치가 적용된 노드 추가
            } // 단일 그래프 노드 생성 처리 종료

            return nodes; // 완성된 그래프 노드 목록 반환
        } // 그래프 노드 목록 생성 처리 종료

        private List<MapGenerationGraphEdge> CreateVerticalBranchEdges() // 표준 8개 수직 분기 그래프 간선 생성
        { // 표준 수직 분기 간선 생성 처리
            return new List<MapGenerationGraphEdge> // 수직 분기 간선 목록 반환
            { // 수직 분기 간선 묶음
                CreateEdge(0, 1), // 시작에서 분기 연결
                CreateEdge(1, 2), // 분기에서 왼쪽 첫 노드 연결
                CreateEdge(1, 3), // 분기에서 오른쪽 첫 노드 연결
                CreateEdge(2, 4), // 왼쪽 첫 노드에서 둘째 노드 연결
                CreateEdge(3, 5), // 오른쪽 첫 노드에서 둘째 노드 연결
                CreateEdge(4, 6), // 왼쪽 둘째 노드에서 합류 연결
                CreateEdge(5, 6), // 오른쪽 둘째 노드에서 합류 연결
                CreateEdge(6, 7) // 합류에서 종료 연결
            }; // 수직 분기 간선 묶음 종료
        } // 표준 수직 분기 간선 생성 처리 종료

        private MapGenerationGraphEdge CreateEdge(int fromNodeIndex, int toNodeIndex) // 테스트용 단일 그래프 간선 생성
        { // 단일 그래프 간선 생성 처리
            return new MapGenerationGraphEdge(fromNodeIndex, toNodeIndex, "Exit", "Entrance"); // 공통 연결 ID가 적용된 간선 반환
        } // 단일 그래프 간선 생성 처리 종료

        private MapModuleConnectionPoint CreateConnection(Transform parent, string connectionId, Vector3 localPosition, MapConnectionRole role, MapConnectionDirection direction) // 테스트용 연결 지점 생성
        { // 테스트 연결 지점 생성 처리
            GameObject connectionObject = new GameObject(connectionId); // 빈 테스트 연결 오브젝트 생성
            connectionObject.transform.SetParent(parent, false); // 테스트 모듈 아래 연결 지점 배치
            connectionObject.transform.localPosition = localPosition; // 연결 지점 로컬 위치 적용
            MapModuleConnectionPoint point = connectionObject.AddComponent<MapModuleConnectionPoint>(); // 연결 지점 컴포넌트 추가
            point.ConfigureForEditor(connectionId, role, direction, 2f, 2.2f); // 연결 지점 공통 데이터 적용
            return point; // 생성된 테스트 연결 지점 반환
        } // 테스트 연결 지점 생성 처리 종료
    } // 플레이 가능 경로 검사 자동 테스트 묶음 종료
} // EditMode 테스트 묶음 종료
