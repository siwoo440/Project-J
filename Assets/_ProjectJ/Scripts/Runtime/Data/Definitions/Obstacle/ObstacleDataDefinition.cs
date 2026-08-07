using UnityEngine; // Unity ScriptableObject와 Prefab 데이터 기능 참조

namespace ProjectJ.Data // 프로젝트 데이터 네임스페이스 선언
{ // 프로젝트 데이터 묶음
    public enum ObstacleTraversalEffect // 장애물 통과 영향 종류 선언
    { // 장애물 통과 영향 종류 묶음
        None, // 추가 이동 영향 없음
        Slow, // 이동 속도 감소
        Push, // 플레이어 밀어내기
        Knockback, // 강한 넉백 발생
        FallRisk // 낙하 위험 증가
    } // 장애물 통과 영향 종류 묶음 종료

    [CreateAssetMenu(fileName = "ObstacleData", menuName = "Project J/Data/Obstacle")] // Project 창 장애물 데이터 생성 메뉴 등록
    public sealed class ObstacleDataDefinition : ProjectDataAsset // 장애물 공통 데이터 에셋 선언
    { // 장애물 공통 데이터 묶음
        [SerializeField] private GameObject obstaclePrefab; // 실제 생성할 장애물 Prefab
        [SerializeField, Range(1, 100)] private int riskScore = 5; // 장애물 기본 위험도 점수
        [SerializeField] private Vector3 footprintSize = Vector3.one; // 장애물 통로 점유 크기
        [SerializeField] private bool blocksPassage = true; // 통로 폭을 차감하는 장애물 여부
        [SerializeField] private ObstacleTraversalEffect traversalEffect = ObstacleTraversalEffect.None; // 플레이어 이동에 주는 영향

        public override ProjectDataCategory Category => ProjectDataCategory.Obstacle; // 장애물 데이터 분류 반환
        public GameObject ObstaclePrefab => obstaclePrefab; // 장애물 Prefab 반환
        public int RiskScore => riskScore; // 장애물 기본 위험도 반환
        public Vector3 FootprintSize => footprintSize; // 장애물 점유 크기 반환
        public bool BlocksPassage => blocksPassage; // 통로 차단 여부 반환
        public ObstacleTraversalEffect TraversalEffect => traversalEffect; // 이동 영향 종류 반환

        private void OnValidate() // Inspector 장애물 데이터 보정
        { // 장애물 데이터 보정 처리
            riskScore = Mathf.Clamp(riskScore, 1, 100); // 위험도 점수 범위 보장
            footprintSize.x = Mathf.Max(0.01f, footprintSize.x); // 점유 폭 양수 보장
            footprintSize.y = Mathf.Max(0.01f, footprintSize.y); // 점유 높이 양수 보장
            footprintSize.z = Mathf.Max(0.01f, footprintSize.z); // 점유 깊이 양수 보장
        } // 장애물 데이터 보정 처리 종료

        public bool TryValidateObstacle(out string reason) // 장애물 생성 데이터 유효성 검사
        { // 장애물 생성 데이터 검사 처리
            if (obstaclePrefab == null) // 장애물 Prefab 누락 확인
            { // 장애물 Prefab 누락 처리
                reason = $"{DataId} 장애물 Prefab이 연결되지 않았습니다."; // Prefab 누락 이유 저장
                return false; // 장애물 데이터 실패 반환
            } // 장애물 Prefab 누락 처리 종료

            if (riskScore <= 0) // 위험도 점수 오류 확인
            { // 위험도 점수 오류 처리
                reason = $"{DataId} 위험도는 1 이상이어야 합니다."; // 위험도 오류 이유 저장
                return false; // 장애물 데이터 실패 반환
            } // 위험도 점수 오류 처리 종료

            if (footprintSize.x <= 0f || footprintSize.y <= 0f || footprintSize.z <= 0f) // 점유 크기 오류 확인
            { // 점유 크기 오류 처리
                reason = $"{DataId} 점유 크기의 모든 축은 0보다 커야 합니다."; // 점유 크기 오류 이유 저장
                return false; // 장애물 데이터 실패 반환
            } // 점유 크기 오류 처리 종료

            reason = string.Empty; // 성공 이유 문자열 초기화
            return true; // 장애물 데이터 성공 반환
        } // 장애물 생성 데이터 검사 처리 종료

#if UNITY_EDITOR // Unity Editor 전용 설정 시작
        public void ConfigureObstacleForEditor(GameObject newObstaclePrefab, int newRiskScore, Vector3 newFootprintSize, bool newBlocksPassage, ObstacleTraversalEffect newTraversalEffect) // Editor 도구용 장애물 데이터 설정
        { // Editor 장애물 데이터 설정 처리
            obstaclePrefab = newObstaclePrefab; // 새 장애물 Prefab 저장
            riskScore = newRiskScore; // 새 위험도 점수 저장
            footprintSize = newFootprintSize; // 새 점유 크기 저장
            blocksPassage = newBlocksPassage; // 새 통로 차단 여부 저장
            traversalEffect = newTraversalEffect; // 새 이동 영향 종류 저장
            OnValidate(); // 장애물 설정값 즉시 보정
        } // Editor 장애물 데이터 설정 처리 종료
#endif // Unity Editor 전용 설정 종료
    } // 장애물 공통 데이터 묶음 종료
} // 프로젝트 데이터 묶음 종료
