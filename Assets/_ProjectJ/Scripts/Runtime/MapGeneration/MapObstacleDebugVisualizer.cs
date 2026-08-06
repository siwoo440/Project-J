using System.Collections.Generic; // 노드와 배치 지점 조회 기능 참조
using UnityEngine; // Unity 컴포넌트와 기즈모 기능 참조

#if UNITY_EDITOR // Unity Editor 전용 기능 시작
using UnityEditor; // Scene 뷰 문자 표시 기능 참조
#endif // Unity Editor 전용 기능 종료

namespace ProjectJ.MapGeneration // 맵 생성 기능 네임스페이스 선언
{ // 맵 생성 기능 묶음
    [ExecuteAlways] // Edit Mode에서도 기즈모 갱신 허용
    [DisallowMultipleComponent] // 장애물 디버그 시각화 중복 방지
    [RequireComponent(typeof(ProceduralMapGenerator))] // 절차적 맵 생성기 필수 지정
    [RequireComponent(typeof(MapBranchObstaclePlanner))] // 분기 장애물 계획기 필수 지정
    public sealed class MapObstacleDebugVisualizer : MonoBehaviour // 장애물 배치 지점과 위험도 시각화 선언
    { // 장애물 디버그 시각화 묶음
        [SerializeField] private ProceduralMapGenerator generator; // 시각화할 절차적 맵 생성기
        [SerializeField] private MapBranchObstaclePlanner planner; // 시각화할 장애물 계획기
        [SerializeField] private bool drawOnlyWhenSelected = true; // 오브젝트 선택 중에만 표시 여부
        [SerializeField] private bool showLabels = true; // 배치 지점과 위험도 문자 표시 여부
        [SerializeField, Min(0.02f)] private float pointRadius = 0.18f; // 빈 배치 지점 표시 반지름
        [SerializeField] private Color availablePointColor = new Color(0.75f, 0.75f, 0.75f, 0.8f); // 사용 가능한 빈 지점 색상
        [SerializeField] private Color safeObstacleColor = new Color(0.2f, 1f, 0.35f, 0.9f); // 안전 경로 장애물 색상
        [SerializeField] private Color highRiskObstacleColor = new Color(1f, 0.35f, 0.1f, 0.9f); // 고위험 경로 장애물 색상
        [SerializeField] private Color invalidObstacleColor = new Color(1f, 0.05f, 0.05f, 1f); // 잘못된 장애물 배치 색상

        private void Reset() // 컴포넌트 최초 추가 기본 참조 구성
        { // 최초 기본 참조 구성 처리
            generator = GetComponent<ProceduralMapGenerator>(); // 같은 오브젝트의 생성기 자동 연결
            planner = GetComponent<MapBranchObstaclePlanner>(); // 같은 오브젝트의 계획기 자동 연결
        } // 최초 기본 참조 구성 처리 종료

        private void OnValidate() // Inspector 시각화 설정 보정
        { // 시각화 설정 보정 처리
            generator = generator != null ? generator : GetComponent<ProceduralMapGenerator>(); // 누락된 생성기 참조 자동 연결
            planner = planner != null ? planner : GetComponent<MapBranchObstaclePlanner>(); // 누락된 계획기 참조 자동 연결
            pointRadius = Mathf.Max(0.02f, pointRadius); // 지점 표시 반지름 양수 보장
        } // 시각화 설정 보정 처리 종료

        private void OnDrawGizmos() // 선택하지 않은 상태의 장애물 기즈모 표시
        { // 일반 장애물 기즈모 표시 처리
            if (!drawOnlyWhenSelected) // 항상 표시 설정 확인
            { // 항상 표시 처리
                DrawObstaclePlan(); // 현재 장애물 계획 표시
            } // 항상 표시 처리 종료
        } // 일반 장애물 기즈모 표시 처리 종료

        private void OnDrawGizmosSelected() // 선택된 상태의 장애물 기즈모 표시
        { // 선택 장애물 기즈모 표시 처리
            if (drawOnlyWhenSelected) // 선택 중 표시 설정 확인
            { // 선택 중 표시 처리
                DrawObstaclePlan(); // 현재 장애물 계획 표시
            } // 선택 중 표시 처리 종료
        } // 선택 장애물 기즈모 표시 처리 종료

        private void DrawObstaclePlan() // 배치 지점과 생성 장애물 위험도 표시
        { // 장애물 계획 시각화 처리
            if (generator == null || planner == null) // 필수 참조 누락 확인
            { // 필수 참조 누락 처리
                return; // 장애물 계획 표시 생략
            } // 필수 참조 누락 처리 종료

            MapObstacleSpawnPoint[] spawnPoints = generator.GetComponentsInChildren<MapObstacleSpawnPoint>(true); // 생성 모듈의 모든 배치 지점 수집
            MapPlacedObstacle[] placedObstacles = generator.GetComponentsInChildren<MapPlacedObstacle>(true); // 생성된 모든 장애물 표식 수집
            Dictionary<MapObstacleSpawnPoint, MapPlacedObstacle> obstaclesByPoint = BuildObstacleLookup(placedObstacles); // 배치 지점별 생성 장애물 사전 생성

            for (int pointIndex = 0; pointIndex < spawnPoints.Length; pointIndex++) // 모든 장애물 배치 지점 순회
            { // 단일 장애물 배치 지점 표시 처리
                MapObstacleSpawnPoint spawnPoint = spawnPoints[pointIndex]; // 현재 장애물 배치 지점 조회

                if (spawnPoint == null || !spawnPoint.EnabledForGeneration) // 표시 가능한 배치 지점 여부 확인
                { // 표시 불가 배치 지점 처리
                    continue; // 현재 지점 표시 생략
                } // 표시 불가 배치 지점 처리 종료

                if (obstaclesByPoint.TryGetValue(spawnPoint, out MapPlacedObstacle placedObstacle)) // 현재 지점 생성 장애물 존재 확인
                { // 생성 장애물 존재 처리
                    DrawPlacedObstacle(placedObstacle); // 생성 장애물 위험도 표시
                } // 생성 장애물 존재 처리 종료
                else // 현재 지점 생성 장애물 없음 확인
                { // 빈 배치 지점 처리
                    Gizmos.color = availablePointColor; // 빈 지점 색상 적용
                    Gizmos.DrawWireSphere(spawnPoint.transform.position, pointRadius); // 빈 배치 지점 구체 표시

#if UNITY_EDITOR // Unity Editor 전용 문자 표시 시작
                    if (showLabels) // 빈 지점 문자 표시 활성 확인
                    { // 빈 지점 문자 표시 처리
                        Handles.Label(spawnPoint.transform.position + Vector3.up * 0.25f, $"빈 지점 | {spawnPoint.PointId}"); // 배치 지점 ID 문자 표시
                    } // 빈 지점 문자 표시 처리 종료
#endif // Unity Editor 전용 문자 표시 종료
                } // 빈 배치 지점 처리 종료
            } // 단일 장애물 배치 지점 표시 처리 종료
        } // 장애물 계획 시각화 처리 종료

        private Dictionary<MapObstacleSpawnPoint, MapPlacedObstacle> BuildObstacleLookup(MapPlacedObstacle[] placedObstacles) // 배치 지점별 생성 장애물 사전 생성
        { // 생성 장애물 사전 생성 처리
            Dictionary<MapObstacleSpawnPoint, MapPlacedObstacle> result = new Dictionary<MapObstacleSpawnPoint, MapPlacedObstacle>(); // 빈 생성 장애물 사전 생성

            for (int obstacleIndex = 0; obstacleIndex < placedObstacles.Length; obstacleIndex++) // 모든 생성 장애물 순회
            { // 단일 생성 장애물 사전 등록 처리
                MapPlacedObstacle placedObstacle = placedObstacles[obstacleIndex]; // 현재 생성 장애물 조회

                if (placedObstacle != null && placedObstacle.SpawnPoint != null && !result.ContainsKey(placedObstacle.SpawnPoint)) // 유효하고 새 배치 지점 확인
                { // 새 배치 지점 처리
                    result.Add(placedObstacle.SpawnPoint, placedObstacle); // 배치 지점과 장애물 등록
                } // 새 배치 지점 처리 종료
            } // 단일 생성 장애물 사전 등록 처리 종료

            return result; // 완성된 생성 장애물 사전 반환
        } // 생성 장애물 사전 생성 처리 종료

        private void DrawPlacedObstacle(MapPlacedObstacle placedObstacle) // 생성 장애물 난이도와 검사 상태 표시
        { // 생성 장애물 표시 처리
            bool isValid = placedObstacle.TryValidate(out string reason); // 현재 장애물 배치 유효성 검사
            MapObstaclePlanReport report = planner.LastReport; // 최근 장애물 계획 보고서 조회
            bool isSafeLane = report != null && placedObstacle.LaneIndex == report.SafeLaneIndex; // 안전 경로 장애물 여부 계산
            Gizmos.color = !isValid ? invalidObstacleColor : isSafeLane ? safeObstacleColor : highRiskObstacleColor; // 검사와 난이도별 색상 적용
            Vector3 footprintSize = placedObstacle.ObstacleData != null ? placedObstacle.ObstacleData.FootprintSize : Vector3.one; // 안전한 장애물 점유 크기 계산
            Gizmos.DrawWireCube(placedObstacle.transform.position, footprintSize); // 장애물 점유 영역 표시

#if UNITY_EDITOR // Unity Editor 전용 문자 표시 시작
            if (showLabels) // 장애물 문자 표시 활성 확인
            { // 장애물 문자 표시 처리
                string difficultyLabel = isSafeLane ? "안전" : "고위험"; // 분기 난이도 문자 계산
                string statusLabel = isValid ? $"위험도 {placedObstacle.AppliedRiskScore}" : reason; // 검사 상태 문자 계산
                Handles.Label(placedObstacle.transform.position + Vector3.up * (footprintSize.y * 0.5f + 0.2f), $"{difficultyLabel} | {statusLabel}"); // 장애물 난이도와 검사 상태 표시
            } // 장애물 문자 표시 처리 종료
#endif // Unity Editor 전용 문자 표시 종료
        } // 생성 장애물 표시 처리 종료

#if UNITY_EDITOR // Unity Editor 전용 설정 시작
        public void ConfigureForEditor(ProceduralMapGenerator newGenerator, MapBranchObstaclePlanner newPlanner, bool newDrawOnlyWhenSelected, bool newShowLabels, float newPointRadius) // Editor 도구용 장애물 시각화 설정
        { // Editor 장애물 시각화 설정 처리
            generator = newGenerator; // 새 절차적 맵 생성기 참조 저장
            planner = newPlanner; // 새 장애물 계획기 참조 저장
            drawOnlyWhenSelected = newDrawOnlyWhenSelected; // 새 선택 중 표시 여부 저장
            showLabels = newShowLabels; // 새 문자 표시 여부 저장
            pointRadius = newPointRadius; // 새 지점 표시 반지름 저장
            OnValidate(); // 시각화 설정값 즉시 보정
        } // Editor 장애물 시각화 설정 처리 종료
#endif // Unity Editor 전용 설정 종료
    } // 장애물 디버그 시각화 묶음 종료
} // 맵 생성 기능 묶음 종료
