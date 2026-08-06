using System; // 직렬화 기능 참조
using System.Collections.Generic; // 목록과 집합 기능 참조
using UnityEngine; // Unity 벡터 기능 참조

namespace ProjectJ.MapGeneration // 맵 생성 기능 네임스페이스 선언
{ // 맵 생성 기능 묶음
    [Serializable] // Unity 직렬화 대상 표시
    public sealed class MapGenerationGraphNode // 생성 경로 그래프 노드 선언
    { // 생성 경로 그래프 노드 묶음
        [SerializeField] private int nodeIndex; // 노드 순서 번호
        [SerializeField] private string moduleId; // 배치 모듈 ID
        [SerializeField] private int laneIndex; // 중앙 또는 좌우 경로 번호
        [SerializeField] private Vector3 worldPosition; // 모듈 월드 위치

        public int NodeIndex => nodeIndex; // 노드 순서 번호 반환
        public string ModuleId => moduleId; // 모듈 ID 반환
        public int LaneIndex => laneIndex; // 경로 번호 반환
        public Vector3 WorldPosition => worldPosition; // 월드 위치 반환

        public MapGenerationGraphNode(int newNodeIndex, string newModuleId, int newLaneIndex, Vector3 newWorldPosition) // 그래프 노드 데이터 생성
        { // 그래프 노드 생성 처리
            nodeIndex = newNodeIndex; // 노드 순서 저장
            moduleId = newModuleId; // 모듈 ID 저장
            laneIndex = newLaneIndex; // 경로 번호 저장
            worldPosition = newWorldPosition; // 월드 위치 저장
        } // 그래프 노드 생성 처리
    } // 생성 경로 그래프 노드 묶음

    [Serializable] // Unity 직렬화 대상 표시
    public sealed class MapGenerationGraphEdge // 생성 경로 그래프 간선 선언
    { // 생성 경로 그래프 간선 묶음
        [SerializeField] private int fromNodeIndex; // 출발 노드 번호
        [SerializeField] private int toNodeIndex; // 도착 노드 번호
        [SerializeField] private string exitConnectionId; // 사용 출구 ID
        [SerializeField] private string entranceConnectionId; // 사용 입구 ID

        public int FromNodeIndex => fromNodeIndex; // 출발 노드 번호 반환
        public int ToNodeIndex => toNodeIndex; // 도착 노드 번호 반환
        public string ExitConnectionId => exitConnectionId; // 사용 출구 ID 반환
        public string EntranceConnectionId => entranceConnectionId; // 사용 입구 ID 반환

        public MapGenerationGraphEdge(int newFromNodeIndex, int newToNodeIndex, string newExitConnectionId, string newEntranceConnectionId) // 그래프 간선 데이터 생성
        { // 그래프 간선 생성 처리
            fromNodeIndex = newFromNodeIndex; // 출발 노드 번호 저장
            toNodeIndex = newToNodeIndex; // 도착 노드 번호 저장
            exitConnectionId = newExitConnectionId; // 사용 출구 ID 저장
            entranceConnectionId = newEntranceConnectionId; // 사용 입구 ID 저장
        } // 그래프 간선 생성 처리
    } // 생성 경로 그래프 간선 묶음

    public static class MapGenerationGraphRules // 생성 경로 그래프 공통 규칙 선언
    { // 생성 경로 그래프 공통 규칙 묶음
        public static bool AreAllNodesReachable(int nodeCount, IReadOnlyList<MapGenerationGraphEdge> edges, int startNodeIndex) // 시작점 기준 전체 노드 도달 가능 여부 검사
        { // 전체 노드 도달 검사 처리
            if (nodeCount <= 0 || startNodeIndex < 0 || startNodeIndex >= nodeCount || edges == null) // 잘못된 그래프 입력 확인
            { // 잘못된 그래프 입력 처리
                return false; // 전체 도달 실패 반환
            } // 잘못된 그래프 입력 처리

            Queue<int> pendingNodes = new Queue<int>(); // 방문 대기 노드 생성
            HashSet<int> visitedNodes = new HashSet<int>(); // 방문 완료 노드 집합 생성
            pendingNodes.Enqueue(startNodeIndex); // 시작 노드 방문 대기 등록
            visitedNodes.Add(startNodeIndex); // 시작 노드 방문 완료 등록

            while (pendingNodes.Count > 0) // 방문 대기 노드 존재 동안 반복
            { // 그래프 너비 우선 탐색 처리
                int currentNodeIndex = pendingNodes.Dequeue(); // 현재 방문 노드 조회

                for (int edgeIndex = 0; edgeIndex < edges.Count; edgeIndex++) // 모든 간선 순회
                { // 현재 노드 출발 간선 검사
                    MapGenerationGraphEdge edge = edges[edgeIndex]; // 현재 간선 조회

                    if (edge == null || edge.FromNodeIndex != currentNodeIndex) // 현재 노드 출발 간선 여부 확인
                    { // 관련 없는 간선 처리
                        continue; // 현재 간선 제외
                    } // 관련 없는 간선 처리

                    if (edge.ToNodeIndex < 0 || edge.ToNodeIndex >= nodeCount) // 잘못된 도착 노드 확인
                    { // 잘못된 도착 노드 처리
                        return false; // 그래프 검사 실패 반환
                    } // 잘못된 도착 노드 처리

                    if (visitedNodes.Add(edge.ToNodeIndex)) // 새 도착 노드 여부 확인
                    { // 새 도착 노드 처리
                        pendingNodes.Enqueue(edge.ToNodeIndex); // 새 노드 방문 대기 등록
                    } // 새 도착 노드 처리
                } // 현재 노드 출발 간선 검사
            } // 그래프 너비 우선 탐색 처리

            return visitedNodes.Count == nodeCount; // 전체 노드 방문 여부 반환
        } // 전체 노드 도달 검사 처리
    } // 생성 경로 그래프 공통 규칙 묶음
} // 맵 생성 기능 묶음
