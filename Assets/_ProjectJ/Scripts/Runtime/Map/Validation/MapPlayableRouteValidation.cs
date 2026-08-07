using System; // 직렬화 기능 참조
using System.Collections.Generic; // 목록과 탐색 자료구조 참조
using System.Text; // 상세 결과 문자열 기능 참조
using UnityEngine; // Unity 직렬화와 수학 기능 참조

namespace ProjectJ.MapGeneration // 맵 생성 기능 네임스페이스 선언
{ // 맵 생성 기능 묶음
    public enum MapPlayableRouteIssueCode // 플레이 가능 경로 문제 종류 선언
    { // 플레이 가능 경로 문제 종류 묶음
        ValidationNotRun, // 경로 검사 미실행
        InvalidInput, // 필수 검사 입력 오류
        MissingStartNode, // 시작 노드 누락
        MissingFinishNode, // 종료 노드 누락
        MultipleFinishNodes, // 종료 노드 다중 존재
        InvalidNodeIndex, // 잘못된 노드 번호
        InvalidEdge, // 잘못된 그래프 간선
        CycleDetected, // 순환 경로 발견
        UnreachableNode, // 시작점에서 도달 불가
        DeadEnd, // 종료점 이전 막다른 노드
        MissingRoute, // 시작부터 종료까지 경로 없음
        RouteLimitExceeded, // 경로 탐색 제한 초과
        MissingModule, // 경로 모듈 참조 누락
        InvalidModuleTraversal, // 모듈 이동 조건 오류
        InvalidVerticalTraversal, // 수직 구간 이동 조건 오류
        MissingLeftBranch, // 왼쪽 분기 경로 누락
        MissingRightBranch // 오른쪽 분기 경로 누락
    } // 플레이 가능 경로 문제 종류 묶음 종료

    [Serializable] // Unity 직렬화 대상 표시
    public sealed class MapPlayableRouteIssue // 플레이 가능 경로 단일 문제 선언
    { // 플레이 가능 경로 단일 문제 묶음
        [SerializeField] private MapPlayableRouteIssueCode code; // 문제 종류
        [SerializeField] private string message; // 문제 설명
        [SerializeField] private int routeIndex; // 관련 경로 번호
        [SerializeField] private int primaryNodeIndex; // 첫 관련 노드 번호
        [SerializeField] private int secondaryNodeIndex; // 둘째 관련 노드 번호

        public MapPlayableRouteIssueCode Code => code; // 문제 종류 반환
        public string Message => message; // 문제 설명 반환
        public int RouteIndex => routeIndex; // 관련 경로 번호 반환
        public int PrimaryNodeIndex => primaryNodeIndex; // 첫 관련 노드 번호 반환
        public int SecondaryNodeIndex => secondaryNodeIndex; // 둘째 관련 노드 번호 반환

        public MapPlayableRouteIssue(MapPlayableRouteIssueCode newCode, string newMessage, int newRouteIndex, int newPrimaryNodeIndex, int newSecondaryNodeIndex) // 단일 경로 문제 생성
        { // 단일 경로 문제 생성 처리
            code = newCode; // 문제 종류 저장
            message = newMessage; // 문제 설명 저장
            routeIndex = newRouteIndex; // 관련 경로 번호 저장
            primaryNodeIndex = newPrimaryNodeIndex; // 첫 관련 노드 번호 저장
            secondaryNodeIndex = newSecondaryNodeIndex; // 둘째 관련 노드 번호 저장
        } // 단일 경로 문제 생성 처리 종료
    } // 플레이 가능 경로 단일 문제 묶음 종료

    [Serializable] // Unity 직렬화 대상 표시
    public sealed class MapPlayableRoute // 시작부터 종료까지 단일 이동 경로 선언
    { // 단일 이동 경로 묶음
        [SerializeField] private int routeIndex; // 경로 번호
        [SerializeField] private List<int> nodeIndices = new List<int>(); // 경로 노드 순서

        public int RouteIndex => routeIndex; // 경로 번호 반환
        public IReadOnlyList<int> NodeIndices => nodeIndices; // 경로 노드 순서 반환

        public MapPlayableRoute(int newRouteIndex, IReadOnlyList<int> newNodeIndices) // 이동 경로 데이터 생성
        { // 이동 경로 생성 처리
            routeIndex = newRouteIndex; // 경로 번호 저장
            nodeIndices = newNodeIndices != null ? new List<int>(newNodeIndices) : new List<int>(); // 경로 노드 순서 복사
        } // 이동 경로 생성 처리 종료

        public bool ContainsLane(IReadOnlyList<MapGenerationGraphNode> nodes, int laneIndex) // 경로의 지정 분기 포함 여부 검사
        { // 지정 분기 포함 검사 처리
            if (nodes == null) // 그래프 노드 목록 누락 확인
            { // 그래프 노드 목록 누락 처리
                return false; // 지정 분기 없음 반환
            } // 그래프 노드 목록 누락 처리 종료

            for (int pathIndex = 0; pathIndex < nodeIndices.Count; pathIndex++) // 경로의 모든 노드 순회
            { // 단일 경로 노드 검사 처리
                int nodeIndex = nodeIndices[pathIndex]; // 현재 노드 번호 조회

                if (nodeIndex >= 0 && nodeIndex < nodes.Count && nodes[nodeIndex] != null && nodes[nodeIndex].LaneIndex == laneIndex) // 현재 노드 경로 번호 일치 확인
                { // 지정 분기 발견 처리
                    return true; // 지정 분기 포함 반환
                } // 지정 분기 발견 처리 종료
            } // 단일 경로 노드 검사 처리 종료

            return false; // 지정 분기 없음 반환
        } // 지정 분기 포함 검사 처리 종료

        public string BuildSignature() // 경로 재현 확인용 노드 서명 생성
        { // 경로 노드 서명 생성 처리
            return string.Join(">", nodeIndices); // 노드 순서를 화살표 구분 문자열로 반환
        } // 경로 노드 서명 생성 처리 종료
    } // 단일 이동 경로 묶음 종료

    [Serializable] // Unity 직렬화 대상 표시
    public sealed class MapPlayableRouteReport // 플레이 가능 경로 종합 검사 보고서 선언
    { // 플레이 가능 경로 종합 검사 보고서 묶음
        [SerializeField] private bool isCompleted; // 검사 완료 여부
        [SerializeField] private int startNodeIndex = -1; // 시작 노드 번호
        [SerializeField] private int finishNodeIndex = -1; // 종료 노드 번호
        [SerializeField] private List<MapPlayableRoute> routes = new List<MapPlayableRoute>(); // 발견된 시작부터 종료까지 경로 목록
        [SerializeField] private List<MapPlayableRouteIssue> issues = new List<MapPlayableRouteIssue>(); // 발견 문제 목록

        public bool IsCompleted => isCompleted; // 검사 완료 여부 반환
        public bool IsValid => isCompleted && issues.Count == 0 && routes.Count > 0; // 전체 경로 검사 성공 여부 반환
        public int StartNodeIndex => startNodeIndex; // 시작 노드 번호 반환
        public int FinishNodeIndex => finishNodeIndex; // 종료 노드 번호 반환
        public int RouteCount => routes.Count; // 발견 경로 개수 반환
        public int IssueCount => issues.Count; // 발견 문제 개수 반환
        public IReadOnlyList<MapPlayableRoute> Routes => routes; // 발견 경로 목록 반환
        public IReadOnlyList<MapPlayableRouteIssue> Issues => issues; // 발견 문제 목록 반환

        public static MapPlayableRouteReport CreateNotRun() // 미실행 경로 보고서 생성
        { // 미실행 경로 보고서 생성 처리
            return new MapPlayableRouteReport(); // 빈 미완료 경로 보고서 반환
        } // 미실행 경로 보고서 생성 처리 종료

        public bool Contains(MapPlayableRouteIssueCode code) // 특정 경로 문제 포함 여부 검사
        { // 특정 경로 문제 포함 검사 처리
            for (int issueIndex = 0; issueIndex < issues.Count; issueIndex++) // 모든 발견 문제 순회
            { // 발견 문제 종류 비교 처리
                if (issues[issueIndex].Code == code) // 요청 문제 종류 일치 확인
                { // 요청 문제 종류 일치 처리
                    return true; // 특정 문제 포함 반환
                } // 요청 문제 종류 일치 처리 종료
            } // 발견 문제 종류 비교 처리 종료

            return false; // 특정 문제 없음 반환
        } // 특정 경로 문제 포함 검사 처리 종료

        public string BuildSummary() // 경로 검사 결과 한 줄 요약 생성
        { // 경로 검사 결과 한 줄 요약 생성 처리
            if (!isCompleted) // 검사 미완료 확인
            { // 검사 미완료 처리
                return "플레이 가능 경로 검사가 아직 실행되지 않았습니다."; // 검사 미완료 요약 반환
            } // 검사 미완료 처리 종료

            return IsValid ? $"시작부터 종료까지 플레이 가능 경로 {routes.Count}개를 확인했습니다." : $"플레이 가능 경로 문제 {issues.Count}개를 발견했습니다."; // 성공 또는 실패 요약 반환
        } // 경로 검사 결과 한 줄 요약 생성 처리 종료

        public string BuildDetailedMessage() // 경로 검사 결과 상세 문자열 생성
        { // 경로 검사 결과 상세 문자열 생성 처리
            StringBuilder builder = new StringBuilder(); // 상세 문자열 생성기 준비
            builder.Append("[ProjectJ][Day37] "); // 37일차 로그 머리말 추가
            builder.Append(BuildSummary()); // 검사 요약 추가
            builder.Append($" | 시작: {startNodeIndex} | 종료: {finishNodeIndex} | 경로: {routes.Count}"); // 시작과 종료와 경로 수 추가

            for (int routeIndex = 0; routeIndex < routes.Count; routeIndex++) // 모든 발견 경로 순회
            { // 발견 경로 문자열 추가 처리
                builder.AppendLine(); // 다음 경로 줄 이동
                builder.Append($"- 경로 {routes[routeIndex].RouteIndex}: "); // 경로 번호 추가
                builder.Append(routes[routeIndex].BuildSignature()); // 경로 노드 서명 추가
            } // 발견 경로 문자열 추가 처리 종료

            for (int issueIndex = 0; issueIndex < issues.Count; issueIndex++) // 모든 발견 문제 순회
            { // 발견 문제 문자열 추가 처리
                MapPlayableRouteIssue issue = issues[issueIndex]; // 현재 발견 문제 조회
                builder.AppendLine(); // 다음 문제 줄 이동
                builder.Append("- "); // 문제 목록 기호 추가
                builder.Append(issue.Code); // 문제 종류 추가
                builder.Append(" | "); // 문제 구분자 추가
                builder.Append(issue.Message); // 문제 설명 추가

                if (issue.RouteIndex >= 0) // 관련 경로 존재 확인
                { // 관련 경로 표시 처리
                    builder.Append($" | 경로 {issue.RouteIndex}"); // 관련 경로 번호 추가
                } // 관련 경로 표시 처리 종료

                if (issue.PrimaryNodeIndex >= 0) // 첫 관련 노드 존재 확인
                { // 첫 관련 노드 표시 처리
                    builder.Append($" | 노드 {issue.PrimaryNodeIndex}"); // 첫 관련 노드 번호 추가
                } // 첫 관련 노드 표시 처리 종료

                if (issue.SecondaryNodeIndex >= 0) // 둘째 관련 노드 존재 확인
                { // 둘째 관련 노드 표시 처리
                    builder.Append($" → {issue.SecondaryNodeIndex}"); // 둘째 관련 노드 번호 추가
                } // 둘째 관련 노드 표시 처리 종료
            } // 발견 문제 문자열 추가 처리 종료

            return builder.ToString(); // 완성된 상세 문자열 반환
        } // 경로 검사 결과 상세 문자열 생성 처리 종료

        internal void SetEndpoints(int newStartNodeIndex, int newFinishNodeIndex) // 시작과 종료 노드 내부 저장
        { // 시작과 종료 노드 저장 처리
            startNodeIndex = newStartNodeIndex; // 시작 노드 번호 저장
            finishNodeIndex = newFinishNodeIndex; // 종료 노드 번호 저장
        } // 시작과 종료 노드 저장 처리 종료

        internal void AddRoute(IReadOnlyList<int> nodeIndices) // 발견된 이동 경로 내부 등록
        { // 발견 경로 등록 처리
            routes.Add(new MapPlayableRoute(routes.Count, nodeIndices)); // 순서 기반 새 경로 추가
        } // 발견 경로 등록 처리 종료

        internal void AddIssue(MapPlayableRouteIssueCode code, string message, int routeIndex, int primaryNodeIndex, int secondaryNodeIndex) // 발견 경로 문제 내부 등록
        { // 발견 경로 문제 등록 처리
            issues.Add(new MapPlayableRouteIssue(code, message, routeIndex, primaryNodeIndex, secondaryNodeIndex)); // 새 경로 문제 목록 추가
        } // 발견 경로 문제 등록 처리 종료

        internal void Complete() // 경로 검사 완료 상태 내부 적용
        { // 경로 검사 완료 처리
            isCompleted = true; // 검사 완료 상태 저장
        } // 경로 검사 완료 처리 종료
    } // 플레이 가능 경로 종합 검사 보고서 묶음 종료

    public static class MapPlayableRouteValidator // 생성 맵 플레이 가능 경로 검사기 선언
    { // 생성 맵 플레이 가능 경로 검사기 묶음
        public static MapPlayableRouteReport Validate(IReadOnlyList<MapModuleDefinition> modules, IReadOnlyList<MapGenerationGraphNode> nodes, IReadOnlyList<MapGenerationGraphEdge> edges, int startNodeIndex, int maximumRouteCount, bool requireBothBranchLanes) // 시작부터 종료까지 모든 이동 경로 검사
        { // 플레이 가능 경로 전체 검사 처리
            MapPlayableRouteReport report = new MapPlayableRouteReport(); // 새 경로 검사 보고서 생성

            if (modules == null || nodes == null || edges == null) // 필수 검사 목록 누락 확인
            { // 필수 검사 목록 누락 처리
                report.AddIssue(MapPlayableRouteIssueCode.InvalidInput, "모듈·노드·간선 목록이 모두 필요합니다.", -1, -1, -1); // 필수 목록 누락 문제 등록
                report.Complete(); // 검사 완료 상태 적용
                return report; // 필수 목록 누락 보고서 반환
            } // 필수 검사 목록 누락 처리 종료

            if (nodes.Count == 0 || startNodeIndex < 0 || startNodeIndex >= nodes.Count) // 시작 노드 범위 오류 확인
            { // 시작 노드 오류 처리
                report.AddIssue(MapPlayableRouteIssueCode.MissingStartNode, "유효한 시작 노드가 필요합니다.", -1, startNodeIndex, -1); // 시작 노드 누락 문제 등록
                report.Complete(); // 검사 완료 상태 적용
                return report; // 시작 노드 오류 보고서 반환
            } // 시작 노드 오류 처리 종료

            int safeMaximumRouteCount = Mathf.Max(1, maximumRouteCount); // 최소 한 개 이상의 경로 탐색 제한 계산
            List<int>[] outgoingNodes = CreateNodeLists(nodes.Count); // 노드별 다음 노드 목록 생성
            List<int>[] incomingNodes = CreateNodeLists(nodes.Count); // 노드별 이전 노드 목록 생성
            ValidateNodes(nodes, report); // 그래프 노드 번호와 중복 검사
            ValidateAndRegisterEdges(edges, nodes.Count, outgoingNodes, incomingNodes, report); // 간선 검사와 인접 목록 구성
            List<int> finishCandidates = FindFinishCandidates(outgoingNodes); // 출구가 없는 종료 후보 노드 수집
            int finishNodeIndex = finishCandidates.Count > 0 ? finishCandidates[0] : -1; // 첫 종료 후보 번호 계산
            report.SetEndpoints(startNodeIndex, finishNodeIndex); // 보고서 시작과 종료 노드 저장

            if (finishCandidates.Count == 0) // 종료 후보 없음 확인
            { // 종료 후보 없음 처리
                report.AddIssue(MapPlayableRouteIssueCode.MissingFinishNode, "출구가 없는 종료 노드를 찾지 못했습니다.", -1, -1, -1); // 종료 노드 누락 문제 등록
                report.Complete(); // 검사 완료 상태 적용
                return report; // 종료 노드 누락 보고서 반환
            } // 종료 후보 없음 처리 종료

            if (finishCandidates.Count > 1) // 종료 후보 다중 존재 확인
            { // 종료 후보 다중 존재 처리
                report.AddIssue(MapPlayableRouteIssueCode.MultipleFinishNodes, $"종료 후보가 {finishCandidates.Count}개입니다. 단일 합류 종료가 필요합니다.", -1, finishCandidates[0], finishCandidates[1]); // 다중 종료 문제 등록
            } // 종료 후보 다중 존재 처리 종료

            bool[] reachableFromStart = CollectReachableNodes(startNodeIndex, outgoingNodes); // 시작점에서 도달 가능한 노드 계산
            bool[] canReachFinish = CollectReachableNodes(finishNodeIndex, incomingNodes); // 종료점에 도달 가능한 노드 역방향 계산
            ValidateReachability(reachableFromStart, canReachFinish, finishNodeIndex, report); // 도달 불가와 막다른 노드 검사
            List<int> currentPath = new List<int>(); // 현재 깊이 우선 탐색 경로 생성
            HashSet<int> currentPathNodes = new HashSet<int>(); // 현재 경로 순환 검사용 노드 집합 생성
            bool routeLimitExceeded = false; // 경로 탐색 제한 초과 여부 초기화
            CollectRoutes(startNodeIndex, finishNodeIndex, outgoingNodes, safeMaximumRouteCount, currentPath, currentPathNodes, report, ref routeLimitExceeded); // 시작부터 종료까지 모든 단순 경로 수집

            if (report.RouteCount == 0) // 발견된 시작부터 종료 경로 없음 확인
            { // 이동 경로 없음 처리
                report.AddIssue(MapPlayableRouteIssueCode.MissingRoute, "시작 노드에서 종료 노드까지 이어지는 이동 경로가 없습니다.", -1, startNodeIndex, finishNodeIndex); // 이동 경로 누락 문제 등록
            } // 이동 경로 없음 처리 종료

            if (routeLimitExceeded) // 경로 탐색 제한 초과 확인
            { // 경로 탐색 제한 초과 처리
                report.AddIssue(MapPlayableRouteIssueCode.RouteLimitExceeded, $"경로 탐색 수가 제한 {safeMaximumRouteCount}개를 초과했습니다.", -1, startNodeIndex, finishNodeIndex); // 경로 제한 문제 등록
            } // 경로 탐색 제한 초과 처리 종료

            ValidateRouteModules(modules, nodes, report); // 각 경로의 모듈 이동 조건 검사

            if (requireBothBranchLanes) // 좌우 분기 경로 필수 여부 확인
            { // 좌우 분기 경로 검사 처리
                ValidateRequiredBranchLanes(nodes, report); // 왼쪽과 오른쪽 경로 존재 검사
            } // 좌우 분기 경로 검사 처리 종료

            report.Complete(); // 경로 검사 완료 상태 적용
            return report; // 완성된 경로 검사 보고서 반환
        } // 플레이 가능 경로 전체 검사 처리 종료

        private static List<int>[] CreateNodeLists(int nodeCount) // 노드별 인접 목록 배열 생성
        { // 인접 목록 배열 생성 처리
            List<int>[] nodeLists = new List<int>[nodeCount]; // 노드 수만큼 빈 배열 생성

            for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++) // 모든 노드 번호 순회
            { // 단일 인접 목록 생성 처리
                nodeLists[nodeIndex] = new List<int>(); // 현재 노드 빈 인접 목록 생성
            } // 단일 인접 목록 생성 처리 종료

            return nodeLists; // 완성된 인접 목록 배열 반환
        } // 인접 목록 배열 생성 처리 종료

        private static void ValidateNodes(IReadOnlyList<MapGenerationGraphNode> nodes, MapPlayableRouteReport report) // 그래프 노드 번호와 중복 검사
        { // 그래프 노드 검사 처리
            HashSet<int> registeredIndices = new HashSet<int>(); // 등록 완료 노드 번호 집합 생성

            for (int listIndex = 0; listIndex < nodes.Count; listIndex++) // 모든 그래프 노드 순회
            { // 단일 그래프 노드 검사 처리
                MapGenerationGraphNode node = nodes[listIndex]; // 현재 그래프 노드 조회

                if (node == null || node.NodeIndex != listIndex || node.NodeIndex < 0 || node.NodeIndex >= nodes.Count || !registeredIndices.Add(node.NodeIndex)) // 노드 누락 또는 순서 또는 번호 오류 또는 중복 확인
                { // 잘못된 노드 처리
                    int invalidIndex = node != null ? node.NodeIndex : listIndex; // 오류 보고용 노드 번호 계산
                    report.AddIssue(MapPlayableRouteIssueCode.InvalidNodeIndex, "그래프 노드 번호가 누락·범위 초과·중복 상태입니다.", -1, invalidIndex, -1); // 잘못된 노드 번호 문제 등록
                } // 잘못된 노드 처리 종료
            } // 단일 그래프 노드 검사 처리 종료
        } // 그래프 노드 검사 처리 종료

        private static void ValidateAndRegisterEdges(IReadOnlyList<MapGenerationGraphEdge> edges, int nodeCount, List<int>[] outgoingNodes, List<int>[] incomingNodes, MapPlayableRouteReport report) // 간선 검사와 인접 목록 구성
        { // 간선 검사와 인접 목록 구성 처리
            HashSet<string> registeredEdges = new HashSet<string>(); // 중복 간선 검사 집합 생성

            for (int edgeIndex = 0; edgeIndex < edges.Count; edgeIndex++) // 모든 그래프 간선 순회
            { // 단일 그래프 간선 검사 처리
                MapGenerationGraphEdge edge = edges[edgeIndex]; // 현재 그래프 간선 조회

                if (edge == null || edge.FromNodeIndex < 0 || edge.FromNodeIndex >= nodeCount || edge.ToNodeIndex < 0 || edge.ToNodeIndex >= nodeCount || edge.FromNodeIndex == edge.ToNodeIndex) // 간선 누락 또는 범위 또는 자기 연결 확인
                { // 잘못된 간선 처리
                    int fromIndex = edge != null ? edge.FromNodeIndex : -1; // 오류 보고용 출발 노드 계산
                    int toIndex = edge != null ? edge.ToNodeIndex : -1; // 오류 보고용 도착 노드 계산
                    report.AddIssue(MapPlayableRouteIssueCode.InvalidEdge, "그래프 간선의 출발·도착 노드가 올바르지 않습니다.", -1, fromIndex, toIndex); // 잘못된 간선 문제 등록
                    continue; // 현재 간선 등록 생략
                } // 잘못된 간선 처리 종료

                string edgeKey = $"{edge.FromNodeIndex}>{edge.ToNodeIndex}"; // 중복 검사용 간선 키 생성

                if (!registeredEdges.Add(edgeKey)) // 동일 방향 간선 중복 확인
                { // 중복 간선 처리
                    report.AddIssue(MapPlayableRouteIssueCode.InvalidEdge, "동일한 출발·도착 노드 간선이 중복되었습니다.", -1, edge.FromNodeIndex, edge.ToNodeIndex); // 중복 간선 문제 등록
                    continue; // 중복 간선 등록 생략
                } // 중복 간선 처리 종료

                outgoingNodes[edge.FromNodeIndex].Add(edge.ToNodeIndex); // 출발 노드의 다음 노드 등록
                incomingNodes[edge.ToNodeIndex].Add(edge.FromNodeIndex); // 도착 노드의 이전 노드 등록
            } // 단일 그래프 간선 검사 처리 종료

            for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++) // 모든 노드의 인접 목록 순회
            { // 인접 노드 순서 정리 처리
                outgoingNodes[nodeIndex].Sort(); // 다음 노드 번호 오름차순 정렬
                incomingNodes[nodeIndex].Sort(); // 이전 노드 번호 오름차순 정렬
            } // 인접 노드 순서 정리 처리 종료
        } // 간선 검사와 인접 목록 구성 처리 종료

        private static List<int> FindFinishCandidates(List<int>[] outgoingNodes) // 출구가 없는 종료 후보 노드 수집
        { // 종료 후보 수집 처리
            List<int> finishCandidates = new List<int>(); // 종료 후보 결과 목록 생성

            for (int nodeIndex = 0; nodeIndex < outgoingNodes.Length; nodeIndex++) // 모든 노드 출구 목록 순회
            { // 단일 종료 후보 검사 처리
                if (outgoingNodes[nodeIndex].Count == 0) // 다음 노드 없는지 확인
                { // 종료 후보 발견 처리
                    finishCandidates.Add(nodeIndex); // 현재 노드를 종료 후보로 추가
                } // 종료 후보 발견 처리 종료
            } // 단일 종료 후보 검사 처리 종료

            return finishCandidates; // 종료 후보 목록 반환
        } // 종료 후보 수집 처리 종료

        private static bool[] CollectReachableNodes(int startNodeIndex, List<int>[] adjacency) // 지정 시작점에서 도달 가능한 노드 계산
        { // 도달 가능한 노드 계산 처리
            bool[] visitedNodes = new bool[adjacency.Length]; // 노드별 방문 여부 배열 생성

            if (startNodeIndex < 0 || startNodeIndex >= adjacency.Length) // 시작 노드 범위 오류 확인
            { // 시작 노드 범위 오류 처리
                return visitedNodes; // 모두 미방문 상태 반환
            } // 시작 노드 범위 오류 처리 종료

            Queue<int> pendingNodes = new Queue<int>(); // 너비 우선 탐색 대기 목록 생성
            pendingNodes.Enqueue(startNodeIndex); // 시작 노드 탐색 대기 등록
            visitedNodes[startNodeIndex] = true; // 시작 노드 방문 표시

            while (pendingNodes.Count > 0) // 탐색 대기 노드 존재 동안 반복
            { // 너비 우선 탐색 처리
                int currentNodeIndex = pendingNodes.Dequeue(); // 현재 탐색 노드 조회

                for (int adjacentIndex = 0; adjacentIndex < adjacency[currentNodeIndex].Count; adjacentIndex++) // 현재 노드의 모든 인접 노드 순회
                { // 단일 인접 노드 방문 처리
                    int nextNodeIndex = adjacency[currentNodeIndex][adjacentIndex]; // 다음 노드 번호 조회

                    if (!visitedNodes[nextNodeIndex]) // 다음 노드 미방문 여부 확인
                    { // 새 노드 방문 처리
                        visitedNodes[nextNodeIndex] = true; // 다음 노드 방문 표시
                        pendingNodes.Enqueue(nextNodeIndex); // 다음 노드 탐색 대기 등록
                    } // 새 노드 방문 처리 종료
                } // 단일 인접 노드 방문 처리 종료
            } // 너비 우선 탐색 처리 종료

            return visitedNodes; // 노드별 도달 가능 여부 반환
        } // 도달 가능한 노드 계산 처리 종료

        private static void ValidateReachability(bool[] reachableFromStart, bool[] canReachFinish, int finishNodeIndex, MapPlayableRouteReport report) // 시작과 종료 기준 도달 가능성 검사
        { // 도달 가능성 검사 처리
            for (int nodeIndex = 0; nodeIndex < reachableFromStart.Length; nodeIndex++) // 모든 노드 번호 순회
            { // 단일 노드 도달 가능성 검사 처리
                if (!reachableFromStart[nodeIndex]) // 시작점 도달 불가 확인
                { // 시작점 도달 불가 처리
                    report.AddIssue(MapPlayableRouteIssueCode.UnreachableNode, "시작 노드에서 도달할 수 없습니다.", -1, nodeIndex, -1); // 도달 불가 노드 문제 등록
                } // 시작점 도달 불가 처리 종료
                else if (nodeIndex != finishNodeIndex && !canReachFinish[nodeIndex]) // 종료점 도달 불가 확인
                { // 막다른 노드 처리
                    report.AddIssue(MapPlayableRouteIssueCode.DeadEnd, "이 노드에서 최종 종료 노드까지 이동할 수 없습니다.", -1, nodeIndex, finishNodeIndex); // 막다른 노드 문제 등록
                } // 막다른 노드 처리 종료
            } // 단일 노드 도달 가능성 검사 처리 종료
        } // 도달 가능성 검사 처리 종료

        private static void CollectRoutes(int currentNodeIndex, int finishNodeIndex, List<int>[] outgoingNodes, int maximumRouteCount, List<int> currentPath, HashSet<int> currentPathNodes, MapPlayableRouteReport report, ref bool routeLimitExceeded) // 깊이 우선 탐색으로 모든 단순 경로 수집
        { // 단순 경로 수집 처리
            if (routeLimitExceeded) // 이미 경로 제한 초과 확인
            { // 경로 제한 초과 처리
                return; // 후속 탐색 중단
            } // 경로 제한 초과 처리 종료

            if (!currentPathNodes.Add(currentNodeIndex)) // 현재 경로 안의 노드 재방문 확인
            { // 순환 경로 처리
                int previousNodeIndex = currentPath.Count > 0 ? currentPath[currentPath.Count - 1] : -1; // 순환 직전 노드 계산
                report.AddIssue(MapPlayableRouteIssueCode.CycleDetected, "시작부터 종료까지 탐색 중 순환 경로를 발견했습니다.", -1, previousNodeIndex, currentNodeIndex); // 순환 경로 문제 등록
                return; // 순환 경로 탐색 중단
            } // 순환 경로 처리 종료

            currentPath.Add(currentNodeIndex); // 현재 노드를 탐색 경로에 추가

            if (currentNodeIndex == finishNodeIndex) // 종료 노드 도달 확인
            { // 완성 경로 처리
                if (report.RouteCount >= maximumRouteCount) // 경로 수 제한 도달 확인
                { // 경로 수 제한 초과 처리
                    routeLimitExceeded = true; // 경로 제한 초과 상태 저장
                } // 경로 수 제한 초과 처리 종료
                else // 경로 수 제한 이내 확인
                { // 완성 경로 등록 처리
                    report.AddRoute(currentPath); // 현재 시작부터 종료 경로 등록
                } // 완성 경로 등록 처리 종료
            } // 완성 경로 처리 종료
            else // 종료 전 노드 확인
            { // 다음 노드 탐색 처리
                for (int nextIndex = 0; nextIndex < outgoingNodes[currentNodeIndex].Count; nextIndex++) // 모든 다음 노드 순회
                { // 단일 다음 노드 탐색 처리
                    CollectRoutes(outgoingNodes[currentNodeIndex][nextIndex], finishNodeIndex, outgoingNodes, maximumRouteCount, currentPath, currentPathNodes, report, ref routeLimitExceeded); // 다음 노드 기준 경로 재귀 탐색
                } // 단일 다음 노드 탐색 처리 종료
            } // 다음 노드 탐색 처리 종료

            currentPath.RemoveAt(currentPath.Count - 1); // 현재 노드를 탐색 경로에서 제거
            currentPathNodes.Remove(currentNodeIndex); // 현재 노드를 순환 검사 집합에서 제거
        } // 단순 경로 수집 처리 종료

        private static void ValidateRouteModules(IReadOnlyList<MapModuleDefinition> modules, IReadOnlyList<MapGenerationGraphNode> nodes, MapPlayableRouteReport report) // 발견된 각 경로의 모듈 이동 조건 검사
        { // 경로 모듈 이동 조건 검사 처리
            for (int routeIndex = 0; routeIndex < report.Routes.Count; routeIndex++) // 모든 발견 경로 순회
            { // 단일 경로 모듈 검사 처리
                MapPlayableRoute route = report.Routes[routeIndex]; // 현재 이동 경로 조회

                for (int pathIndex = 0; pathIndex < route.NodeIndices.Count; pathIndex++) // 현재 경로의 모든 노드 순회
                { // 단일 경로 노드 모듈 검사 처리
                    int nodeIndex = route.NodeIndices[pathIndex]; // 현재 경로 노드 번호 조회

                    if (nodeIndex < 0 || nodeIndex >= modules.Count || nodeIndex >= nodes.Count || modules[nodeIndex] == null) // 모듈 참조 누락 또는 범위 오류 확인
                    { // 경로 모듈 누락 처리
                        report.AddIssue(MapPlayableRouteIssueCode.MissingModule, "경로 노드에 대응하는 생성 모듈이 없습니다.", route.RouteIndex, nodeIndex, -1); // 모듈 누락 문제 등록
                        continue; // 현재 노드 후속 검사 생략
                    } // 경로 모듈 누락 처리 종료

                    MapModuleDefinition module = modules[nodeIndex]; // 현재 경로 모듈 조회

                    if (!module.TryValidate(out string moduleReason)) // 모듈 기본 이동 능력 검사
                    { // 모듈 이동 조건 실패 처리
                        report.AddIssue(MapPlayableRouteIssueCode.InvalidModuleTraversal, $"{module.ModuleId}: {moduleReason}", route.RouteIndex, nodeIndex, -1); // 모듈 이동 조건 문제 등록
                    } // 모듈 이동 조건 실패 처리 종료

                    MapVerticalModuleData verticalData = module.GetComponent<MapVerticalModuleData>(); // 현재 모듈 수직 이동 데이터 조회

                    if (verticalData != null && !verticalData.TryValidate(out string verticalReason)) // 수직 이동 구간 데이터 오류 확인
                    { // 수직 이동 조건 실패 처리
                        report.AddIssue(MapPlayableRouteIssueCode.InvalidVerticalTraversal, $"{module.ModuleId}: {verticalReason}", route.RouteIndex, nodeIndex, -1); // 수직 이동 조건 문제 등록
                    } // 수직 이동 조건 실패 처리 종료
                } // 단일 경로 노드 모듈 검사 처리 종료
            } // 단일 경로 모듈 검사 처리 종료
        } // 경로 모듈 이동 조건 검사 처리 종료

        private static void ValidateRequiredBranchLanes(IReadOnlyList<MapGenerationGraphNode> nodes, MapPlayableRouteReport report) // 왼쪽과 오른쪽 분기 경로 존재 검사
        { // 필수 분기 경로 검사 처리
            bool hasLeftRoute = false; // 왼쪽 분기 경로 존재 여부 초기화
            bool hasRightRoute = false; // 오른쪽 분기 경로 존재 여부 초기화

            for (int routeIndex = 0; routeIndex < report.Routes.Count; routeIndex++) // 모든 발견 경로 순회
            { // 단일 경로 분기 번호 검사 처리
                hasLeftRoute |= report.Routes[routeIndex].ContainsLane(nodes, -1); // 왼쪽 경로 포함 여부 누적
                hasRightRoute |= report.Routes[routeIndex].ContainsLane(nodes, 1); // 오른쪽 경로 포함 여부 누적
            } // 단일 경로 분기 번호 검사 처리 종료

            if (!hasLeftRoute) // 왼쪽 분기 경로 누락 확인
            { // 왼쪽 분기 경로 누락 처리
                report.AddIssue(MapPlayableRouteIssueCode.MissingLeftBranch, "시작부터 종료까지 이어지는 왼쪽 분기 경로가 없습니다.", -1, report.StartNodeIndex, report.FinishNodeIndex); // 왼쪽 분기 누락 문제 등록
            } // 왼쪽 분기 경로 누락 처리 종료

            if (!hasRightRoute) // 오른쪽 분기 경로 누락 확인
            { // 오른쪽 분기 경로 누락 처리
                report.AddIssue(MapPlayableRouteIssueCode.MissingRightBranch, "시작부터 종료까지 이어지는 오른쪽 분기 경로가 없습니다.", -1, report.StartNodeIndex, report.FinishNodeIndex); // 오른쪽 분기 누락 문제 등록
            } // 오른쪽 분기 경로 누락 처리 종료
        } // 필수 분기 경로 검사 처리 종료
    } // 생성 맵 플레이 가능 경로 검사기 묶음 종료
} // 맵 생성 기능 묶음 종료
