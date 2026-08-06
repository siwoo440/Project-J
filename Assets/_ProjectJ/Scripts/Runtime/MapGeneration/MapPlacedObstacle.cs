using ProjectJ.Data; // 장애물 데이터 정의 참조
using UnityEngine; // Unity 컴포넌트 기능 참조

namespace ProjectJ.MapGeneration // 맵 생성 기능 네임스페이스 선언
{ // 맵 생성 기능 묶음
    [DisallowMultipleComponent] // 배치 결과 표식 중복 방지
    public sealed class MapPlacedObstacle : MonoBehaviour // 생성된 장애물 배치 결과 선언
    { // 생성 장애물 배치 결과 묶음
        [SerializeField] private ObstacleDataDefinition obstacleData; // 생성에 사용한 장애물 데이터
        [SerializeField] private MapObstacleSpawnPoint spawnPoint; // 사용한 배치 지점
        [SerializeField] private int nodeIndex = -1; // 배치된 그래프 노드 번호
        [SerializeField] private int laneIndex; // 배치된 분기 경로 번호
        [SerializeField] private int appliedRiskScore; // 지점 배율 적용 위험도

        public ObstacleDataDefinition ObstacleData => obstacleData; // 장애물 데이터 반환
        public MapObstacleSpawnPoint SpawnPoint => spawnPoint; // 배치 지점 반환
        public int NodeIndex => nodeIndex; // 그래프 노드 번호 반환
        public int LaneIndex => laneIndex; // 분기 경로 번호 반환
        public int AppliedRiskScore => appliedRiskScore; // 적용 위험도 반환

        public void Initialize(ObstacleDataDefinition newObstacleData, MapObstacleSpawnPoint newSpawnPoint, int newNodeIndex, int newLaneIndex, int newAppliedRiskScore) // 생성 직후 배치 결과 초기화
        { // 배치 결과 초기화 처리
            obstacleData = newObstacleData; // 사용 장애물 데이터 저장
            spawnPoint = newSpawnPoint; // 사용 배치 지점 저장
            nodeIndex = newNodeIndex; // 그래프 노드 번호 저장
            laneIndex = newLaneIndex; // 분기 경로 번호 저장
            appliedRiskScore = Mathf.Max(0, newAppliedRiskScore); // 음수가 아닌 적용 위험도 저장
        } // 배치 결과 초기화 처리 종료

        public bool TryValidate(out string reason) // 현재 생성 장애물 배치 결과 검사
        { // 생성 장애물 배치 결과 검사 처리
            if (obstacleData == null) // 장애물 데이터 누락 확인
            { // 장애물 데이터 누락 처리
                reason = "생성 장애물의 ObstacleData가 누락됐습니다."; // 장애물 데이터 누락 이유 저장
                return false; // 배치 결과 실패 반환
            } // 장애물 데이터 누락 처리 종료

            if (spawnPoint == null) // 배치 지점 누락 확인
            { // 배치 지점 누락 처리
                reason = $"{obstacleData.DataId} 장애물의 배치 지점이 누락됐습니다."; // 배치 지점 누락 이유 저장
                return false; // 배치 결과 실패 반환
            } // 배치 지점 누락 처리 종료

            if (nodeIndex < 0 || laneIndex == 0) // 분기 노드 정보 오류 확인
            { // 분기 노드 정보 오류 처리
                reason = $"{obstacleData.DataId} 장애물의 노드 또는 분기 번호가 잘못됐습니다."; // 노드 정보 오류 이유 저장
                return false; // 배치 결과 실패 반환
            } // 분기 노드 정보 오류 처리 종료

            if (!spawnPoint.CanPlace(obstacleData, out reason)) // 배치 뒤 통로 안전성 검사
            { // 통로 안전성 실패 처리
                return false; // 배치 결과 실패 반환
            } // 통로 안전성 실패 처리 종료

            if (appliedRiskScore != spawnPoint.CalculateRiskScore(obstacleData)) // 적용 위험도 불일치 확인
            { // 적용 위험도 불일치 처리
                reason = $"{obstacleData.DataId} 장애물의 저장 위험도와 계산 위험도가 다릅니다."; // 위험도 불일치 이유 저장
                return false; // 배치 결과 실패 반환
            } // 적용 위험도 불일치 처리 종료

            reason = string.Empty; // 성공 이유 문자열 초기화
            return true; // 배치 결과 성공 반환
        } // 생성 장애물 배치 결과 검사 처리 종료
    } // 생성 장애물 배치 결과 묶음 종료
} // 맵 생성 기능 묶음 종료
