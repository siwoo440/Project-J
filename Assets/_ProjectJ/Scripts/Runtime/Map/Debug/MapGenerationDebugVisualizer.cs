using System.Collections.Generic; // 문제 노드 집합 기능 참조
using UnityEngine; // Unity 컴포넌트와 기즈모 기능 참조

#if UNITY_EDITOR // Unity Editor 전용 기능 시작
using UnityEditor; // Scene 뷰 문자 표시 기능 참조
#endif // Unity Editor 전용 기능 종료

namespace ProjectJ.MapGeneration // 맵 생성 기능 네임스페이스 선언
{ // 맵 생성 기능 묶음
    [ExecuteAlways] // Edit Mode에서도 기즈모 갱신 허용
    [DisallowMultipleComponent] // 디버그 시각화 컴포넌트 중복 방지
    [RequireComponent(typeof(ProceduralMapGenerator))] // 절차적 맵 생성기 필수 지정
    public sealed class MapGenerationDebugVisualizer : MonoBehaviour // 생성 경로 디버그 시각화 선언
    { // 생성 경로 디버그 시각화 묶음
        [SerializeField] private ProceduralMapGenerator generator; // 시각화할 절차적 맵 생성기
        [SerializeField] private bool drawOnlyWhenSelected = true; // 오브젝트 선택 중에만 표시 여부
        [SerializeField] private bool showNodeLabels = true; // 노드 번호와 모듈 ID 표시 여부
        [SerializeField, Min(0.05f)] private float nodeRadius = 0.3f; // 노드 구체 반지름
        [SerializeField, Min(0f)] private float lineHeightOffset = 0.35f; // 모듈 중심 위 경로선 높이
        [SerializeField] private Color sharedNodeColor = new Color(0.2f, 1f, 0.4f, 1f); // 공통 경로 노드 색상
        [SerializeField] private Color leftNodeColor = new Color(0.2f, 0.65f, 1f, 1f); // 왼쪽 분기 노드 색상
        [SerializeField] private Color rightNodeColor = new Color(1f, 0.35f, 0.85f, 1f); // 오른쪽 분기 노드 색상
        [SerializeField] private Color validEdgeColor = new Color(0.25f, 1f, 1f, 1f); // 정상 경로 간선 색상
        [SerializeField] private Color invalidColor = new Color(1f, 0.15f, 0.1f, 1f); // 실패 노드와 간선 색상
        [SerializeField] private Color finishColor = new Color(1f, 0.85f, 0.15f, 1f); // 종료 노드 색상

        private void Reset() // 컴포넌트 최초 추가 기본 참조 구성
        { // 최초 기본 참조 구성 처리
            generator = GetComponent<ProceduralMapGenerator>(); // 같은 오브젝트의 생성기 자동 연결
        } // 최초 기본 참조 구성 처리 종료

        private void OnValidate() // Inspector 시각화 값 보정
        { // 시각화 값 보정 처리
            generator = generator != null ? generator : GetComponent<ProceduralMapGenerator>(); // 누락된 생성기 참조 자동 연결
            nodeRadius = Mathf.Max(0.05f, nodeRadius); // 노드 반지름 최소값 보장
            lineHeightOffset = Mathf.Max(0f, lineHeightOffset); // 경로선 높이 음수 방지
        } // 시각화 값 보정 처리 종료

        private void OnDrawGizmos() // 선택하지 않은 상태의 경로 기즈모 표시
        { // 일반 경로 기즈모 표시 처리
            if (!drawOnlyWhenSelected) // 항상 표시 설정 확인
            { // 항상 표시 처리
                DrawGeneratedMap(); // 현재 생성 맵 경로 표시
            } // 항상 표시 처리 종료
        } // 일반 경로 기즈모 표시 처리 종료

        private void OnDrawGizmosSelected() // 선택된 상태의 경로 기즈모 표시
        { // 선택 경로 기즈모 표시 처리
            if (drawOnlyWhenSelected) // 선택 중 표시 설정 확인
            { // 선택 중 표시 처리
                DrawGeneratedMap(); // 현재 생성 맵 경로 표시
            } // 선택 중 표시 처리 종료
        } // 선택 경로 기즈모 표시 처리 종료

        private void DrawGeneratedMap() // 생성 노드와 간선과 실패 위치 표시
        { // 생성 맵 시각화 처리
            if (generator == null) // 생성기 참조 누락 확인
            { // 생성기 참조 누락 처리
                return; // 경로 표시 생략
            } // 생성기 참조 누락 처리 종료

            IReadOnlyList<MapGenerationGraphNode> nodes = generator.GraphNodes; // 생성 그래프 노드 목록 조회
            IReadOnlyList<MapGenerationGraphEdge> edges = generator.GraphEdges; // 생성 그래프 간선 목록 조회

            if (nodes == null || edges == null || nodes.Count == 0) // 표시할 생성 결과 없음 확인
            { // 표시할 생성 결과 없음 처리
                return; // 경로 표시 생략
            } // 표시할 생성 결과 없음 처리 종료

            HashSet<int> invalidNodes = CollectInvalidNodeIndices(generator.LastPlayableRouteReport); // 경로 검사 실패 노드 번호 수집
            DrawEdges(nodes, edges, invalidNodes); // 생성 그래프 간선 표시
            DrawNodes(nodes, invalidNodes, generator.LastPlayableRouteReport); // 생성 그래프 노드와 문자 표시
        } // 생성 맵 시각화 처리 종료

        private HashSet<int> CollectInvalidNodeIndices(MapPlayableRouteReport report) // 경로 보고서에서 실패 노드 번호 수집
        { // 실패 노드 번호 수집 처리
            HashSet<int> invalidNodes = new HashSet<int>(); // 실패 노드 번호 결과 집합 생성

            if (report == null || !report.IsCompleted) // 경로 보고서 누락 또는 미완료 확인
            { // 경로 보고서 미사용 처리
                return invalidNodes; // 빈 실패 노드 집합 반환
            } // 경로 보고서 미사용 처리 종료

            for (int issueIndex = 0; issueIndex < report.Issues.Count; issueIndex++) // 모든 경로 문제 순회
            { // 단일 문제 관련 노드 수집 처리
                MapPlayableRouteIssue issue = report.Issues[issueIndex]; // 현재 경로 문제 조회

                if (issue.PrimaryNodeIndex >= 0) // 첫 관련 노드 존재 확인
                { // 첫 관련 노드 등록 처리
                    invalidNodes.Add(issue.PrimaryNodeIndex); // 첫 관련 노드 실패 집합 추가
                } // 첫 관련 노드 등록 처리 종료

                if (issue.SecondaryNodeIndex >= 0) // 둘째 관련 노드 존재 확인
                { // 둘째 관련 노드 등록 처리
                    invalidNodes.Add(issue.SecondaryNodeIndex); // 둘째 관련 노드 실패 집합 추가
                } // 둘째 관련 노드 등록 처리 종료
            } // 단일 문제 관련 노드 수집 처리 종료

            return invalidNodes; // 완성된 실패 노드 집합 반환
        } // 실패 노드 번호 수집 처리 종료

        private void DrawEdges(IReadOnlyList<MapGenerationGraphNode> nodes, IReadOnlyList<MapGenerationGraphEdge> edges, HashSet<int> invalidNodes) // 생성 그래프 간선 표시
        { // 생성 그래프 간선 표시 처리
            for (int edgeIndex = 0; edgeIndex < edges.Count; edgeIndex++) // 모든 생성 간선 순회
            { // 단일 생성 간선 표시 처리
                MapGenerationGraphEdge edge = edges[edgeIndex]; // 현재 생성 간선 조회

                if (edge == null || edge.FromNodeIndex < 0 || edge.FromNodeIndex >= nodes.Count || edge.ToNodeIndex < 0 || edge.ToNodeIndex >= nodes.Count || nodes[edge.FromNodeIndex] == null || nodes[edge.ToNodeIndex] == null) // 간선 또는 관련 노드 오류 확인
                { // 잘못된 간선 표시 생략 처리
                    continue; // 현재 간선 표시 생략
                } // 잘못된 간선 표시 생략 처리 종료

                Vector3 startPosition = nodes[edge.FromNodeIndex].WorldPosition + Vector3.up * lineHeightOffset; // 출발 노드 경로선 위치 계산
                Vector3 endPosition = nodes[edge.ToNodeIndex].WorldPosition + Vector3.up * lineHeightOffset; // 도착 노드 경로선 위치 계산
                bool isInvalid = invalidNodes.Contains(edge.FromNodeIndex) || invalidNodes.Contains(edge.ToNodeIndex); // 간선 관련 실패 노드 여부 계산
                Gizmos.color = isInvalid ? invalidColor : validEdgeColor; // 간선 검사 상태별 색상 적용
                Gizmos.DrawLine(startPosition, endPosition); // 출발부터 도착까지 경로선 표시
                Vector3 direction = (endPosition - startPosition).normalized; // 경로선 진행 방향 계산
                Vector3 right = Vector3.Cross(Vector3.up, direction).normalized; // 화살표 좌우 방향 계산
                Vector3 arrowCenter = Vector3.Lerp(startPosition, endPosition, 0.7f); // 화살표 중심 위치 계산
                Gizmos.DrawLine(arrowCenter, arrowCenter - direction * 0.45f + right * 0.25f); // 경로 화살표 오른쪽 날개 표시
                Gizmos.DrawLine(arrowCenter, arrowCenter - direction * 0.45f - right * 0.25f); // 경로 화살표 왼쪽 날개 표시
            } // 단일 생성 간선 표시 처리 종료
        } // 생성 그래프 간선 표시 처리 종료

        private void DrawNodes(IReadOnlyList<MapGenerationGraphNode> nodes, HashSet<int> invalidNodes, MapPlayableRouteReport report) // 생성 그래프 노드와 문자 표시
        { // 생성 그래프 노드 표시 처리
            int finishNodeIndex = report != null && report.IsCompleted ? report.FinishNodeIndex : -1; // 경로 보고서 종료 노드 번호 계산

            for (int nodeIndex = 0; nodeIndex < nodes.Count; nodeIndex++) // 모든 생성 노드 순회
            { // 단일 생성 노드 표시 처리
                MapGenerationGraphNode node = nodes[nodeIndex]; // 현재 생성 노드 조회

                if (node == null) // 현재 생성 노드 누락 확인
                { // 현재 생성 노드 누락 처리
                    continue; // 현재 노드 표시 생략
                } // 현재 생성 노드 누락 처리 종료

                Vector3 nodePosition = node.WorldPosition + Vector3.up * lineHeightOffset; // 노드 구체 표시 위치 계산
                Gizmos.color = ResolveNodeColor(node, invalidNodes.Contains(node.NodeIndex), node.NodeIndex == finishNodeIndex); // 노드 상태별 색상 적용
                Gizmos.DrawSphere(nodePosition, nodeRadius); // 현재 노드 구체 표시

#if UNITY_EDITOR // Unity Editor 전용 문자 표시 시작
                if (showNodeLabels) // 노드 문자 표시 활성 확인
                { // 노드 문자 표시 처리
                    string laneLabel = node.LaneIndex < 0 ? "L" : node.LaneIndex > 0 ? "R" : "C"; // 경로 번호 문자 계산
                    Handles.Label(nodePosition + Vector3.up * (nodeRadius + 0.15f), $"{node.NodeIndex} | {laneLabel} | {node.ModuleId}"); // 노드 번호와 경로와 모듈 ID 표시
                } // 노드 문자 표시 처리 종료
#endif // Unity Editor 전용 문자 표시 종료
            } // 단일 생성 노드 표시 처리 종료
        } // 생성 그래프 노드 표시 처리 종료

        private Color ResolveNodeColor(MapGenerationGraphNode node, bool isInvalid, bool isFinish) // 노드 상태별 표시 색상 결정
        { // 노드 표시 색상 결정 처리
            if (isInvalid) // 실패 노드 여부 확인
            { // 실패 노드 처리
                return invalidColor; // 실패 색상 반환
            } // 실패 노드 처리 종료

            if (isFinish) // 종료 노드 여부 확인
            { // 종료 노드 처리
                return finishColor; // 종료 색상 반환
            } // 종료 노드 처리 종료

            if (node.LaneIndex < 0) // 왼쪽 분기 노드 여부 확인
            { // 왼쪽 분기 노드 처리
                return leftNodeColor; // 왼쪽 분기 색상 반환
            } // 왼쪽 분기 노드 처리 종료

            if (node.LaneIndex > 0) // 오른쪽 분기 노드 여부 확인
            { // 오른쪽 분기 노드 처리
                return rightNodeColor; // 오른쪽 분기 색상 반환
            } // 오른쪽 분기 노드 처리 종료

            return sharedNodeColor; // 공통 경로 색상 반환
        } // 노드 표시 색상 결정 처리 종료

#if UNITY_EDITOR // Unity Editor 전용 설정 시작
        public void ConfigureForEditor(ProceduralMapGenerator newGenerator, bool newDrawOnlyWhenSelected, bool newShowNodeLabels, float newNodeRadius, float newLineHeightOffset) // Editor 도구용 디버그 시각화 설정 적용
        { // Editor 디버그 시각화 설정 처리
            generator = newGenerator; // 새 절차적 맵 생성기 참조 저장
            drawOnlyWhenSelected = newDrawOnlyWhenSelected; // 새 선택 중 표시 여부 저장
            showNodeLabels = newShowNodeLabels; // 새 노드 문자 표시 여부 저장
            nodeRadius = newNodeRadius; // 새 노드 반지름 저장
            lineHeightOffset = newLineHeightOffset; // 새 경로선 높이 저장
            OnValidate(); // 시각화 설정값 즉시 보정
        } // Editor 디버그 시각화 설정 처리 종료
#endif // Unity Editor 전용 설정 종료
    } // 생성 경로 디버그 시각화 묶음 종료
} // 맵 생성 기능 묶음 종료
