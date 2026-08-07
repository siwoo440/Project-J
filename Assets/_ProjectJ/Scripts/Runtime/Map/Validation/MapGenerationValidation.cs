using System; // 문자열 비교 기능 참조
using System.Collections.Generic; // 목록과 집합 기능 참조
using System.Text; // 상세 검사 결과 문자열 기능 참조
using UnityEngine; // Unity 벡터와 영역 기능 참조

namespace ProjectJ.MapGeneration // 맵 생성 기능 네임스페이스 선언
{ // 맵 생성 기능 묶음
    public enum MapGenerationValidationIssueCode // 생성 결과 문제 종류 선언
    { // 생성 결과 문제 종류 묶음
        GenerationNotRun, // 생성 또는 검사 미실행
        GenerationFlowFailed, // 생성 흐름 중단
        EmptyMap, // 생성 모듈 없음
        TargetModuleCountMismatch, // 목표 모듈 수 불일치
        ModuleNodeCountMismatch, // 모듈과 노드 수 불일치
        MissingModule, // 모듈 참조 누락
        InvalidModuleData, // 모듈 이동 규격 오류
        MissingGraphNode, // 그래프 노드 누락
        InvalidGraphNode, // 그래프 노드 데이터 오류
        NodeModuleMismatch, // 노드와 모듈 정보 불일치
        InvalidGraphEdge, // 그래프 간선 데이터 오류
        DisconnectedGraph, // 끊어진 생성 경로
        MissingExitConnection, // 출구 연결 지점 누락
        MissingEntranceConnection, // 입구 연결 지점 누락
        InvalidConnectionRole, // 연결 지점 역할 오류
        ReusedConnection, // 연결 지점 중복 사용
        ConnectionDirectionMismatch, // 연결 방향 불일치
        ConnectionSizeMismatch, // 연결 크기 불일치
        ConnectionPositionMismatch, // 연결 위치 불일치
        TraversalHeightExceeded, // 이동 가능 높이 초과
        ModuleOverlap // 모듈 영역 겹침
    } // 생성 결과 문제 종류 묶음 종료

    [Serializable] // Unity 직렬화 대상 표시
    public sealed class MapGenerationValidationIssue // 생성 결과 단일 문제 선언
    { // 생성 결과 단일 문제 묶음
        [SerializeField] private MapGenerationValidationIssueCode code; // 문제 종류
        [SerializeField] private string message; // 문제 설명
        [SerializeField] private int primaryNodeIndex; // 첫 관련 노드 번호
        [SerializeField] private int secondaryNodeIndex; // 둘째 관련 노드 번호

        public MapGenerationValidationIssueCode Code => code; // 문제 종류 반환
        public string Message => message; // 문제 설명 반환
        public int PrimaryNodeIndex => primaryNodeIndex; // 첫 관련 노드 번호 반환
        public int SecondaryNodeIndex => secondaryNodeIndex; // 둘째 관련 노드 번호 반환

        public MapGenerationValidationIssue(MapGenerationValidationIssueCode newCode, string newMessage, int newPrimaryNodeIndex, int newSecondaryNodeIndex) // 생성 결과 단일 문제 생성
        { // 생성 결과 단일 문제 생성 처리
            code = newCode; // 문제 종류 저장
            message = newMessage; // 문제 설명 저장
            primaryNodeIndex = newPrimaryNodeIndex; // 첫 관련 노드 번호 저장
            secondaryNodeIndex = newSecondaryNodeIndex; // 둘째 관련 노드 번호 저장
        } // 생성 결과 단일 문제 생성 처리 종료
    } // 생성 결과 단일 문제 묶음 종료

    [Serializable] // Unity 직렬화 대상 표시
    public sealed class MapGenerationValidationReport // 생성 결과 종합 검사 보고서 선언
    { // 생성 결과 종합 검사 보고서 묶음
        [SerializeField] private bool isCompleted; // 검사 완료 여부
        [SerializeField] private List<MapGenerationValidationIssue> issues = new List<MapGenerationValidationIssue>(); // 발견 문제 목록

        public bool IsCompleted => isCompleted; // 검사 완료 여부 반환
        public bool IsValid => isCompleted && issues.Count == 0; // 전체 검사 성공 여부 반환
        public int IssueCount => issues.Count; // 발견 문제 개수 반환
        public IReadOnlyList<MapGenerationValidationIssue> Issues => issues; // 발견 문제 목록 반환

        public static MapGenerationValidationReport CreateNotRun() // 미실행 검사 보고서 생성
        { // 미실행 검사 보고서 생성 처리
            return new MapGenerationValidationReport(); // 빈 미완료 보고서 반환
        } // 미실행 검사 보고서 생성 처리 종료

        public static MapGenerationValidationReport CreateFailure(MapGenerationValidationIssueCode code, string message) // 단일 실패 검사 보고서 생성
        { // 단일 실패 검사 보고서 생성 처리
            MapGenerationValidationReport report = new MapGenerationValidationReport(); // 새 검사 보고서 생성
            report.AddIssue(code, message, -1, -1); // 단일 실패 문제 등록
            report.Complete(); // 검사 완료 상태 적용
            return report; // 실패 보고서 반환
        } // 단일 실패 검사 보고서 생성 처리 종료

        public bool Contains(MapGenerationValidationIssueCode code) // 특정 문제 포함 여부 검사
        { // 특정 문제 포함 검사 처리
            for (int issueIndex = 0; issueIndex < issues.Count; issueIndex++) // 모든 발견 문제 순회
            { // 발견 문제 종류 비교 처리
                if (issues[issueIndex].Code == code) // 요청 문제 종류 일치 확인
                { // 요청 문제 종류 일치 처리
                    return true; // 특정 문제 포함 반환
                } // 요청 문제 종류 일치 처리 종료
            } // 발견 문제 종류 비교 처리 종료

            return false; // 특정 문제 없음 반환
        } // 특정 문제 포함 검사 처리 종료

        public string BuildSummary() // 검사 결과 한 줄 요약 생성
        { // 검사 결과 한 줄 요약 생성 처리
            if (!isCompleted) // 검사 미완료 확인
            { // 검사 미완료 처리
                return "검사가 아직 실행되지 않았습니다."; // 검사 미완료 요약 반환
            } // 검사 미완료 처리 종료

            return IsValid ? "모든 생성 결과 검사를 통과했습니다." : $"총 {issues.Count}개의 생성 결과 문제를 발견했습니다."; // 성공 또는 실패 요약 반환
        } // 검사 결과 한 줄 요약 생성 처리 종료

        public string BuildDetailedMessage() // 검사 결과 상세 문자열 생성
        { // 검사 결과 상세 문자열 생성 처리
            StringBuilder builder = new StringBuilder(); // 상세 문자열 생성기 준비
            builder.Append("[ProjectJ][Day33] "); // 공통 로그 머리말 추가
            builder.Append(BuildSummary()); // 검사 요약 추가

            for (int issueIndex = 0; issueIndex < issues.Count; issueIndex++) // 모든 발견 문제 순회
            { // 발견 문제 문자열 추가 처리
                MapGenerationValidationIssue issue = issues[issueIndex]; // 현재 발견 문제 조회
                builder.AppendLine(); // 다음 문제 줄 이동
                builder.Append("- "); // 문제 목록 기호 추가
                builder.Append(issue.Code); // 문제 종류 추가
                builder.Append(" | "); // 문제 구분자 추가
                builder.Append(issue.Message); // 문제 설명 추가

                if (issue.PrimaryNodeIndex >= 0) // 첫 관련 노드 존재 확인
                { // 첫 관련 노드 표시 처리
                    builder.Append(" | 노드 "); // 첫 노드 안내 추가
                    builder.Append(issue.PrimaryNodeIndex); // 첫 노드 번호 추가
                } // 첫 관련 노드 표시 처리 종료

                if (issue.SecondaryNodeIndex >= 0) // 둘째 관련 노드 존재 확인
                { // 둘째 관련 노드 표시 처리
                    builder.Append(" → "); // 노드 방향 기호 추가
                    builder.Append(issue.SecondaryNodeIndex); // 둘째 노드 번호 추가
                } // 둘째 관련 노드 표시 처리 종료
            } // 발견 문제 문자열 추가 처리 종료

            return builder.ToString(); // 완성된 상세 문자열 반환
        } // 검사 결과 상세 문자열 생성 처리 종료

        internal void AddIssue(MapGenerationValidationIssueCode code, string message, int primaryNodeIndex, int secondaryNodeIndex) // 검사 문제 내부 등록
        { // 검사 문제 내부 등록 처리
            issues.Add(new MapGenerationValidationIssue(code, message, primaryNodeIndex, secondaryNodeIndex)); // 새 검사 문제 목록 추가
        } // 검사 문제 내부 등록 처리 종료

        internal void Complete() // 검사 완료 상태 내부 적용
        { // 검사 완료 상태 내부 적용 처리
            isCompleted = true; // 검사 완료 상태 저장
        } // 검사 완료 상태 내부 적용 처리 종료
    } // 생성 결과 종합 검사 보고서 묶음 종료

    public static class MapGenerationResultValidator // 생성 결과 종합 검사기 선언
    { // 생성 결과 종합 검사기 묶음
        public static MapGenerationValidationReport Validate(IReadOnlyList<MapModuleDefinition> modules, IReadOnlyList<MapGenerationGraphNode> nodes, IReadOnlyList<MapGenerationGraphEdge> edges, int targetModuleCount, float overlapTolerance, float connectionSizeTolerance, float connectionPositionTolerance) // 생성 결과 전체 검사
        { // 생성 결과 전체 검사 처리
            MapGenerationValidationReport report = new MapGenerationValidationReport(); // 새 검사 보고서 생성

            if (modules == null || nodes == null || edges == null) // 필수 생성 결과 목록 누락 확인
            { // 필수 생성 결과 목록 누락 처리
                report.AddIssue(MapGenerationValidationIssueCode.EmptyMap, "생성 모듈·그래프 노드·그래프 간선 목록이 모두 필요합니다.", -1, -1); // 필수 목록 누락 문제 등록
                report.Complete(); // 검사 완료 상태 적용
                return report; // 목록 누락 보고서 반환
            } // 필수 생성 결과 목록 누락 처리 종료

            if (modules.Count == 0) // 생성 모듈 없음 확인
            { // 생성 모듈 없음 처리
                report.AddIssue(MapGenerationValidationIssueCode.EmptyMap, "생성된 맵 모듈이 없습니다.", -1, -1); // 빈 맵 문제 등록
            } // 생성 모듈 없음 처리 종료

            if (modules.Count != targetModuleCount) // 목표 모듈 수 불일치 확인
            { // 목표 모듈 수 불일치 처리
                report.AddIssue(MapGenerationValidationIssueCode.TargetModuleCountMismatch, $"목표 {targetModuleCount}개와 실제 {modules.Count}개가 다릅니다.", -1, -1); // 목표 수 불일치 문제 등록
            } // 목표 모듈 수 불일치 처리 종료

            if (modules.Count != nodes.Count) // 모듈과 노드 수 불일치 확인
            { // 모듈과 노드 수 불일치 처리
                report.AddIssue(MapGenerationValidationIssueCode.ModuleNodeCountMismatch, $"모듈 {modules.Count}개와 그래프 노드 {nodes.Count}개가 다릅니다.", -1, -1); // 모듈과 노드 수 불일치 문제 등록
            } // 모듈과 노드 수 불일치 처리 종료

            ValidateModulesAndNodes(modules, nodes, connectionPositionTolerance, report); // 모듈과 그래프 노드 대응 검사
            ValidateOverlaps(modules, overlapTolerance, report); // 모든 모듈 영역 겹침 검사
            ValidateGraphReachability(nodes, edges, report); // 그래프 전체 도달 가능성 검사
            ValidateEdges(modules, nodes, edges, connectionSizeTolerance, connectionPositionTolerance, report); // 모든 그래프 간선과 연결 지점 검사
            report.Complete(); // 검사 완료 상태 적용
            return report; // 완성된 검사 보고서 반환
        } // 생성 결과 전체 검사 처리 종료

        private static void ValidateModulesAndNodes(IReadOnlyList<MapModuleDefinition> modules, IReadOnlyList<MapGenerationGraphNode> nodes, float positionTolerance, MapGenerationValidationReport report) // 모듈과 노드 대응 검사
        { // 모듈과 노드 대응 검사 처리
            int commonCount = Mathf.Min(modules.Count, nodes.Count); // 함께 검사할 공통 항목 수 계산

            for (int moduleIndex = 0; moduleIndex < modules.Count; moduleIndex++) // 모든 생성 모듈 순회
            { // 생성 모듈 데이터 검사 처리
                MapModuleDefinition module = modules[moduleIndex]; // 현재 생성 모듈 조회

                if (module == null) // 현재 모듈 참조 누락 확인
                { // 현재 모듈 참조 누락 처리
                    report.AddIssue(MapGenerationValidationIssueCode.MissingModule, "생성 모듈 목록에 빈 참조가 있습니다.", moduleIndex, -1); // 모듈 누락 문제 등록
                    continue; // 현재 모듈 후속 검사 생략
                } // 현재 모듈 참조 누락 처리 종료

                if (!module.TryValidate(out string reason)) // 현재 모듈 이동 규격 검사
                { // 현재 모듈 이동 규격 오류 처리
                    report.AddIssue(MapGenerationValidationIssueCode.InvalidModuleData, $"{module.name}: {reason}", moduleIndex, -1); // 모듈 이동 규격 문제 등록
                } // 현재 모듈 이동 규격 오류 처리 종료
            } // 생성 모듈 데이터 검사 처리 종료

            for (int nodeIndex = 0; nodeIndex < nodes.Count; nodeIndex++) // 모든 그래프 노드 순회
            { // 그래프 노드 기본 데이터 검사 처리
                MapGenerationGraphNode node = nodes[nodeIndex]; // 현재 그래프 노드 조회

                if (node == null) // 현재 그래프 노드 누락 확인
                { // 현재 그래프 노드 누락 처리
                    report.AddIssue(MapGenerationValidationIssueCode.MissingGraphNode, "그래프 노드 목록에 빈 항목이 있습니다.", nodeIndex, -1); // 그래프 노드 누락 문제 등록
                    continue; // 현재 그래프 노드 후속 검사 생략
                } // 현재 그래프 노드 누락 처리 종료

                if (node.NodeIndex != nodeIndex) // 저장 노드 번호 불일치 확인
                { // 저장 노드 번호 불일치 처리
                    report.AddIssue(MapGenerationValidationIssueCode.InvalidGraphNode, $"목록 위치 {nodeIndex}와 저장 번호 {node.NodeIndex}가 다릅니다.", nodeIndex, -1); // 노드 번호 문제 등록
                } // 저장 노드 번호 불일치 처리 종료
            } // 그래프 노드 기본 데이터 검사 처리 종료

            for (int commonIndex = 0; commonIndex < commonCount; commonIndex++) // 모듈과 노드 공통 범위 순회
            { // 모듈과 노드 대응 정보 검사 처리
                MapModuleDefinition module = modules[commonIndex]; // 현재 대응 모듈 조회
                MapGenerationGraphNode node = nodes[commonIndex]; // 현재 대응 노드 조회

                if (module == null || node == null) // 대응 항목 누락 확인
                { // 대응 항목 누락 처리
                    continue; // 대응 정보 검사 생략
                } // 대응 항목 누락 처리 종료

                bool moduleIdMatches = string.Equals(module.ModuleId, node.ModuleId, StringComparison.Ordinal); // 모듈 ID 일치 여부 계산
                bool positionMatches = MapGenerationRules.AreConnectionPositionsAligned(module.transform.position, node.WorldPosition, positionTolerance); // 모듈 위치 일치 여부 계산

                if (!moduleIdMatches || !positionMatches) // 노드와 모듈 정보 불일치 확인
                { // 노드와 모듈 정보 불일치 처리
                    report.AddIssue(MapGenerationValidationIssueCode.NodeModuleMismatch, $"모듈 ID 또는 월드 위치가 노드 기록과 다릅니다: {module.name}", commonIndex, -1); // 노드와 모듈 대응 문제 등록
                } // 노드와 모듈 정보 불일치 처리 종료
            } // 모듈과 노드 대응 정보 검사 처리 종료
        } // 모듈과 노드 대응 검사 처리 종료

        private static void ValidateOverlaps(IReadOnlyList<MapModuleDefinition> modules, float tolerance, MapGenerationValidationReport report) // 모든 모듈 영역 겹침 검사
        { // 모든 모듈 영역 겹침 검사 처리
            for (int firstIndex = 0; firstIndex < modules.Count; firstIndex++) // 첫 비교 모듈 순회
            { // 첫 비교 모듈 처리
                MapModuleDefinition firstModule = modules[firstIndex]; // 첫 비교 모듈 조회

                if (firstModule == null) // 첫 비교 모듈 누락 확인
                { // 첫 비교 모듈 누락 처리
                    continue; // 첫 비교 모듈 검사 생략
                } // 첫 비교 모듈 누락 처리 종료

                for (int secondIndex = firstIndex + 1; secondIndex < modules.Count; secondIndex++) // 둘째 비교 모듈 순회
                { // 모듈 쌍 영역 비교 처리
                    MapModuleDefinition secondModule = modules[secondIndex]; // 둘째 비교 모듈 조회

                    if (secondModule != null && MapGenerationRules.BoundsHaveBlockingOverlap(firstModule.WorldBounds, secondModule.WorldBounds, tolerance)) // 실제 영역 겹침 확인
                    { // 실제 영역 겹침 처리
                        report.AddIssue(MapGenerationValidationIssueCode.ModuleOverlap, $"{firstModule.name}과 {secondModule.name}의 Bounds가 겹칩니다.", firstIndex, secondIndex); // 모듈 영역 겹침 문제 등록
                    } // 실제 영역 겹침 처리 종료
                } // 모듈 쌍 영역 비교 처리 종료
            } // 첫 비교 모듈 처리 종료
        } // 모든 모듈 영역 겹침 검사 처리 종료

        private static void ValidateGraphReachability(IReadOnlyList<MapGenerationGraphNode> nodes, IReadOnlyList<MapGenerationGraphEdge> edges, MapGenerationValidationReport report) // 그래프 전체 도달 가능성 검사
        { // 그래프 전체 도달 가능성 검사 처리
            if (!MapGenerationGraphRules.AreAllNodesReachable(nodes.Count, edges, 0)) // 시작 노드 기준 전체 도달 실패 확인
            { // 시작 노드 기준 전체 도달 실패 처리
                report.AddIssue(MapGenerationValidationIssueCode.DisconnectedGraph, "시작 노드에서 모든 생성 모듈로 이동할 수 없습니다.", 0, -1); // 끊어진 경로 문제 등록
            } // 시작 노드 기준 전체 도달 실패 처리 종료
        } // 그래프 전체 도달 가능성 검사 처리 종료

        private static void ValidateEdges(IReadOnlyList<MapModuleDefinition> modules, IReadOnlyList<MapGenerationGraphNode> nodes, IReadOnlyList<MapGenerationGraphEdge> edges, float sizeTolerance, float positionTolerance, MapGenerationValidationReport report) // 모든 간선과 연결 지점 검사
        { // 모든 간선과 연결 지점 검사 처리
            HashSet<string> usedExits = new HashSet<string>(StringComparer.Ordinal); // 사용 출구 키 집합 생성
            HashSet<string> usedEntrances = new HashSet<string>(StringComparer.Ordinal); // 사용 입구 키 집합 생성

            for (int edgeIndex = 0; edgeIndex < edges.Count; edgeIndex++) // 모든 그래프 간선 순회
            { // 현재 그래프 간선 검사 처리
                MapGenerationGraphEdge edge = edges[edgeIndex]; // 현재 그래프 간선 조회

                if (edge == null) // 현재 간선 누락 확인
                { // 현재 간선 누락 처리
                    report.AddIssue(MapGenerationValidationIssueCode.InvalidGraphEdge, $"{edgeIndex}번째 그래프 간선이 비어 있습니다.", -1, -1); // 빈 간선 문제 등록
                    continue; // 현재 간선 후속 검사 생략
                } // 현재 간선 누락 처리 종료

                if (!AreNodeIndicesValid(edge, nodes.Count, modules.Count)) // 간선 노드 번호 범위 검사
                { // 간선 노드 번호 범위 오류 처리
                    report.AddIssue(MapGenerationValidationIssueCode.InvalidGraphEdge, $"{edgeIndex}번째 간선의 출발 또는 도착 노드 번호가 범위를 벗어났습니다.", edge.FromNodeIndex, edge.ToNodeIndex); // 간선 범위 문제 등록
                    continue; // 현재 간선 후속 검사 생략
                } // 간선 노드 번호 범위 오류 처리 종료

                MapModuleDefinition sourceModule = modules[edge.FromNodeIndex]; // 출발 모듈 조회
                MapModuleDefinition targetModule = modules[edge.ToNodeIndex]; // 도착 모듈 조회

                if (sourceModule == null || targetModule == null) // 간선 연결 모듈 누락 확인
                { // 간선 연결 모듈 누락 처리
                    continue; // 현재 간선 연결 검사 생략
                } // 간선 연결 모듈 누락 처리 종료

                MapModuleConnectionPoint sourceExit = FindConnection(sourceModule, edge.ExitConnectionId); // 출발 출구 연결 지점 조회
                MapModuleConnectionPoint targetEntrance = FindConnection(targetModule, edge.EntranceConnectionId); // 도착 입구 연결 지점 조회

                if (sourceExit == null) // 출발 출구 누락 확인
                { // 출발 출구 누락 처리
                    report.AddIssue(MapGenerationValidationIssueCode.MissingExitConnection, $"{sourceModule.name}에서 출구 ID {edge.ExitConnectionId}을 찾을 수 없습니다.", edge.FromNodeIndex, edge.ToNodeIndex); // 출구 누락 문제 등록
                } // 출발 출구 누락 처리 종료

                if (targetEntrance == null) // 도착 입구 누락 확인
                { // 도착 입구 누락 처리
                    report.AddIssue(MapGenerationValidationIssueCode.MissingEntranceConnection, $"{targetModule.name}에서 입구 ID {edge.EntranceConnectionId}을 찾을 수 없습니다.", edge.FromNodeIndex, edge.ToNodeIndex); // 입구 누락 문제 등록
                } // 도착 입구 누락 처리 종료

                if (sourceExit == null || targetEntrance == null) // 필수 연결 지점 누락 확인
                { // 필수 연결 지점 누락 처리
                    continue; // 현재 간선 후속 검사 생략
                } // 필수 연결 지점 누락 처리 종료

                ValidateConnectionRoles(sourceExit, targetEntrance, edge, report); // 연결 지점 역할 검사
                ValidateConnectionReuse(usedExits, usedEntrances, edge, report); // 연결 지점 중복 사용 검사
                ValidateConnectionGeometry(sourceExit, targetEntrance, edge, sizeTolerance, positionTolerance, report); // 연결 지점 방향과 크기와 위치 검사
                ValidateTraversalHeight(sourceModule, targetModule, sourceExit, targetEntrance, edge, report); // 연결 높이 이동 가능성 검사
            } // 현재 그래프 간선 검사 처리 종료
        } // 모든 간선과 연결 지점 검사 처리 종료

        private static bool AreNodeIndicesValid(MapGenerationGraphEdge edge, int nodeCount, int moduleCount) // 간선 노드 번호 범위 검사
        { // 간선 노드 번호 범위 검사 처리
            bool fromIsValid = edge.FromNodeIndex >= 0 && edge.FromNodeIndex < nodeCount && edge.FromNodeIndex < moduleCount; // 출발 노드 범위 유효성 계산
            bool toIsValid = edge.ToNodeIndex >= 0 && edge.ToNodeIndex < nodeCount && edge.ToNodeIndex < moduleCount; // 도착 노드 범위 유효성 계산
            return fromIsValid && toIsValid; // 출발과 도착 범위 통합 결과 반환
        } // 간선 노드 번호 범위 검사 처리 종료

        private static MapModuleConnectionPoint FindConnection(MapModuleDefinition module, string connectionId) // 모듈 내부 연결 지점 ID 검색
        { // 모듈 내부 연결 지점 ID 검색 처리
            MapModuleConnectionPoint[] points = module.ConnectionPoints; // 모듈 연결 지점 목록 조회

            if (points == null) // 연결 지점 목록 누락 확인
            { // 연결 지점 목록 누락 처리
                return null; // 연결 지점 검색 실패 반환
            } // 연결 지점 목록 누락 처리 종료

            for (int pointIndex = 0; pointIndex < points.Length; pointIndex++) // 모든 연결 지점 순회
            { // 연결 지점 ID 비교 처리
                MapModuleConnectionPoint point = points[pointIndex]; // 현재 연결 지점 조회

                if (point != null && string.Equals(point.ConnectionId, connectionId, StringComparison.Ordinal)) // 요청 연결 ID 일치 확인
                { // 요청 연결 ID 일치 처리
                    return point; // 일치 연결 지점 반환
                } // 요청 연결 ID 일치 처리 종료
            } // 연결 지점 ID 비교 처리 종료

            return null; // 일치 연결 지점 없음 반환
        } // 모듈 내부 연결 지점 ID 검색 처리 종료

        private static void ValidateConnectionRoles(MapModuleConnectionPoint sourceExit, MapModuleConnectionPoint targetEntrance, MapGenerationGraphEdge edge, MapGenerationValidationReport report) // 연결 지점 역할 검사
        { // 연결 지점 역할 검사 처리
            if (sourceExit.Role != MapConnectionRole.Exit || targetEntrance.Role != MapConnectionRole.Entrance) // 출구와 입구 역할 불일치 확인
            { // 출구와 입구 역할 불일치 처리
                report.AddIssue(MapGenerationValidationIssueCode.InvalidConnectionRole, "간선은 출구에서 입구 방향으로 연결되어야 합니다.", edge.FromNodeIndex, edge.ToNodeIndex); // 연결 역할 문제 등록
            } // 출구와 입구 역할 불일치 처리 종료
        } // 연결 지점 역할 검사 처리 종료

        private static void ValidateConnectionReuse(HashSet<string> usedExits, HashSet<string> usedEntrances, MapGenerationGraphEdge edge, MapGenerationValidationReport report) // 연결 지점 중복 사용 검사
        { // 연결 지점 중복 사용 검사 처리
            string exitKey = $"{edge.FromNodeIndex}:{edge.ExitConnectionId}"; // 출발 출구 고유 키 계산
            string entranceKey = $"{edge.ToNodeIndex}:{edge.EntranceConnectionId}"; // 도착 입구 고유 키 계산

            if (!usedExits.Add(exitKey)) // 출구 중복 사용 확인
            { // 출구 중복 사용 처리
                report.AddIssue(MapGenerationValidationIssueCode.ReusedConnection, $"출구 {edge.ExitConnectionId}이 여러 간선에서 사용됐습니다.", edge.FromNodeIndex, edge.ToNodeIndex); // 출구 중복 문제 등록
            } // 출구 중복 사용 처리 종료

            if (!usedEntrances.Add(entranceKey)) // 입구 중복 사용 확인
            { // 입구 중복 사용 처리
                report.AddIssue(MapGenerationValidationIssueCode.ReusedConnection, $"입구 {edge.EntranceConnectionId}이 여러 간선에서 사용됐습니다.", edge.FromNodeIndex, edge.ToNodeIndex); // 입구 중복 문제 등록
            } // 입구 중복 사용 처리 종료
        } // 연결 지점 중복 사용 검사 처리 종료

        private static void ValidateConnectionGeometry(MapModuleConnectionPoint sourceExit, MapModuleConnectionPoint targetEntrance, MapGenerationGraphEdge edge, float sizeTolerance, float positionTolerance, MapGenerationValidationReport report) // 연결 지점 기하 정보 검사
        { // 연결 지점 기하 정보 검사 처리
            if (!MapGenerationRules.AreWorldDirectionsOpposite(sourceExit.WorldDirection, targetEntrance.WorldDirection)) // 연결 지점 방향 불일치 확인
            { // 연결 지점 방향 불일치 처리
                report.AddIssue(MapGenerationValidationIssueCode.ConnectionDirectionMismatch, "출구와 입구가 서로 마주 보지 않습니다.", edge.FromNodeIndex, edge.ToNodeIndex); // 연결 방향 문제 등록
            } // 연결 지점 방향 불일치 처리 종료

            if (!MapGenerationRules.AreConnectionSizesCompatible(sourceExit.ConnectionWidth, sourceExit.ConnectionHeight, targetEntrance.ConnectionWidth, targetEntrance.ConnectionHeight, sizeTolerance)) // 연결 지점 크기 불일치 확인
            { // 연결 지점 크기 불일치 처리
                report.AddIssue(MapGenerationValidationIssueCode.ConnectionSizeMismatch, "출구와 입구의 너비 또는 높이가 다릅니다.", edge.FromNodeIndex, edge.ToNodeIndex); // 연결 크기 문제 등록
            } // 연결 지점 크기 불일치 처리 종료

            if (!MapGenerationRules.AreConnectionPositionsAligned(sourceExit.transform.position, targetEntrance.transform.position, positionTolerance)) // 연결 지점 위치 불일치 확인
            { // 연결 지점 위치 불일치 처리
                float distance = Vector3.Distance(sourceExit.transform.position, targetEntrance.transform.position); // 두 연결 지점 거리 계산
                report.AddIssue(MapGenerationValidationIssueCode.ConnectionPositionMismatch, $"출구와 입구가 {distance:0.000}m 떨어져 있습니다.", edge.FromNodeIndex, edge.ToNodeIndex); // 연결 위치 문제 등록
            } // 연결 지점 위치 불일치 처리 종료
        } // 연결 지점 기하 정보 검사 처리 종료

        private static void ValidateTraversalHeight(MapModuleDefinition sourceModule, MapModuleDefinition targetModule, MapModuleConnectionPoint sourceExit, MapModuleConnectionPoint targetEntrance, MapGenerationGraphEdge edge, MapGenerationValidationReport report) // 연결 높이 이동 가능성 검사
        { // 연결 높이 이동 가능성 검사 처리
            MapTraversalProfile sourceProfile = sourceModule.TraversalProfile; // 출발 모듈 이동 능력 기준 조회
            MapTraversalProfile targetProfile = targetModule.TraversalProfile; // 도착 모듈 이동 능력 기준 조회

            if (sourceProfile == null || targetProfile == null) // 이동 능력 기준 누락 확인
            { // 이동 능력 기준 누락 처리
                return; // 높이 이동 검사 생략
            } // 이동 능력 기준 누락 처리 종료

            float heightDifference = targetEntrance.transform.position.y - sourceExit.transform.position.y; // 출구 기준 입구 높이 차이 계산
            float maximumRise = Mathf.Min(sourceProfile.MaximumSafeJumpRise, targetProfile.MaximumSafeJumpRise); // 두 모듈 공통 안전 상승 높이 계산
            float maximumDrop = Mathf.Min(sourceProfile.MaximumSafeDropHeight, targetProfile.MaximumSafeDropHeight); // 두 모듈 공통 안전 낙하 높이 계산

            if (heightDifference > maximumRise || heightDifference < -maximumDrop) // 안전 이동 높이 범위 초과 확인
            { // 안전 이동 높이 범위 초과 처리
                report.AddIssue(MapGenerationValidationIssueCode.TraversalHeightExceeded, $"연결 높이 차이 {heightDifference:0.00}m가 상승 {maximumRise:0.00}m 또는 낙하 {maximumDrop:0.00}m 범위를 넘었습니다.", edge.FromNodeIndex, edge.ToNodeIndex); // 이동 높이 문제 등록
            } // 안전 이동 높이 범위 초과 처리 종료
        } // 연결 높이 이동 가능성 검사 처리 종료
    } // 생성 결과 종합 검사기 묶음 종료
} // 맵 생성 기능 묶음 종료
