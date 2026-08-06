using System.Collections.Generic; // 목록 기능 참조
using NUnit.Framework; // NUnit 자동 테스트 기능 참조
using ProjectJ.MapGeneration; // 맵 생성 그래프 규칙 참조

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스 선언
{ // EditMode 테스트 묶음
    public sealed class MapGenerationGraphRulesTests // 생성 그래프 연결성 자동 테스트 선언
    { // 생성 그래프 연결성 자동 테스트 묶음
        [Test] // 자동 테스트 항목 표시
        public void BranchedAndMergedGraphIsFullyReachable() // 분기와 합류 그래프 전체 도달 확인
        { // 분기와 합류 그래프 테스트 처리
            List<MapGenerationGraphEdge> edges = new List<MapGenerationGraphEdge>(); // 테스트 간선 목록 생성
            edges.Add(new MapGenerationGraphEdge(0, 1, "Exit", "Entrance")); // 시작에서 분기 간선 추가
            edges.Add(new MapGenerationGraphEdge(1, 2, "ExitLeft", "Entrance")); // 분기에서 왼쪽 경로 간선 추가
            edges.Add(new MapGenerationGraphEdge(1, 3, "ExitRight", "Entrance")); // 분기에서 오른쪽 경로 간선 추가
            edges.Add(new MapGenerationGraphEdge(2, 4, "Exit", "EntranceLeft")); // 왼쪽 경로에서 합류 간선 추가
            edges.Add(new MapGenerationGraphEdge(3, 4, "Exit", "EntranceRight")); // 오른쪽 경로에서 합류 간선 추가
            bool result = MapGenerationGraphRules.AreAllNodesReachable(5, edges, 0); // 시작 노드 기준 전체 도달 검사
            Assert.IsTrue(result); // 전체 노드 도달 가능 확인
        } // 분기와 합류 그래프 테스트 처리

        [Test] // 자동 테스트 항목 표시
        public void DisconnectedGraphIsRejected() // 끊어진 그래프 차단 확인
        { // 끊어진 그래프 테스트 처리
            List<MapGenerationGraphEdge> edges = new List<MapGenerationGraphEdge>(); // 테스트 간선 목록 생성
            edges.Add(new MapGenerationGraphEdge(0, 1, "Exit", "Entrance")); // 시작에서 둘째 노드 간선 추가
            bool result = MapGenerationGraphRules.AreAllNodesReachable(3, edges, 0); // 연결되지 않은 셋째 노드를 포함한 검사
            Assert.IsFalse(result); // 끊어진 그래프 차단 확인
        } // 끊어진 그래프 테스트 처리

        [Test] // 자동 테스트 항목 표시
        public void EdgeWithInvalidTargetIsRejected() // 잘못된 도착 노드 차단 확인
        { // 잘못된 도착 노드 테스트 처리
            List<MapGenerationGraphEdge> edges = new List<MapGenerationGraphEdge>(); // 테스트 간선 목록 생성
            edges.Add(new MapGenerationGraphEdge(0, 3, "Exit", "Entrance")); // 범위를 벗어난 도착 간선 추가
            bool result = MapGenerationGraphRules.AreAllNodesReachable(2, edges, 0); // 잘못된 그래프 검사
            Assert.IsFalse(result); // 잘못된 도착 노드 차단 확인
        } // 잘못된 도착 노드 테스트 처리
    } // 생성 그래프 연결성 자동 테스트 묶음
} // EditMode 테스트 묶음
