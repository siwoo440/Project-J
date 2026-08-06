using System; // 시드 기반 난수 기능 참조
using System.Collections.Generic; // 목록과 집합 기능 참조
using ProjectJ.Data; // 장애물 데이터 정의 참조
using UnityEngine; // Unity 컴포넌트와 Prefab 생성 기능 참조

namespace ProjectJ.MapGeneration // 맵 생성 기능 네임스페이스 선언
{ // 맵 생성 기능 묶음
    [DisallowMultipleComponent] // 분기 장애물 계획기 중복 방지
    [RequireComponent(typeof(ProceduralMapGenerator))] // 절차적 맵 생성기 필수 지정
    public sealed class MapBranchObstaclePlanner : MonoBehaviour // 좌우 분기 장애물 생성과 위험도 계획기 선언
    { // 분기 장애물 계획기 묶음
        [SerializeField] private ProceduralMapGenerator generator; // 연결된 절차적 맵 생성기
        [SerializeField] private bool enablePlanning = true; // 장애물 계획 사용 여부
        [SerializeField] private ObstacleDataDefinition[] obstacleDefinitions; // 생성 후보 장애물 데이터 목록
        [SerializeField, Min(0)] private int safeMinimumRisk = 6; // 안전 경로 최소 위험도
        [SerializeField, Min(0)] private int safeMaximumRisk = 12; // 안전 경로 최대 위험도
        [SerializeField, Min(0)] private int highRiskMinimumRisk = 18; // 고위험 경로 최소 위험도
        [SerializeField, Min(0)] private int highRiskMaximumRisk = 30; // 고위험 경로 최대 위험도
        [SerializeField, Min(1)] private int minimumRiskGap = 8; // 안전과 고위험 경로 최소 차이
        [SerializeField, Min(1)] private int maximumObstaclesPerModule = 2; // 모듈 하나의 최대 장애물 수
        [SerializeField] private MapObstaclePlanReport lastReport = MapObstaclePlanReport.CreateNotRun(); // 최근 장애물 계획 보고서

        public bool EnablePlanning => enablePlanning; // 장애물 계획 사용 여부 반환
        public IReadOnlyList<ObstacleDataDefinition> ObstacleDefinitions => obstacleDefinitions; // 장애물 후보 데이터 반환
        public int SafeMinimumRisk => safeMinimumRisk; // 안전 경로 최소 위험도 반환
        public int SafeMaximumRisk => safeMaximumRisk; // 안전 경로 최대 위험도 반환
        public int HighRiskMinimumRisk => highRiskMinimumRisk; // 고위험 경로 최소 위험도 반환
        public int HighRiskMaximumRisk => highRiskMaximumRisk; // 고위험 경로 최대 위험도 반환
        public int MinimumRiskGap => minimumRiskGap; // 최소 위험도 차이 반환
        public int MaximumObstaclesPerModule => maximumObstaclesPerModule; // 모듈별 최대 장애물 수 반환
        public MapObstaclePlanReport LastReport => lastReport; // 최근 장애물 계획 보고서 반환

        private struct SpawnCandidate // 단일 장애물 배치 후보 선언
        { // 단일 장애물 배치 후보 묶음
            public int NodeIndex; // 후보 그래프 노드 번호
            public int LaneIndex; // 후보 분기 경로 번호
            public MapModuleDefinition Module; // 후보 모듈 인스턴스
            public MapObstacleSpawnPoint SpawnPoint; // 후보 배치 지점
        } // 단일 장애물 배치 후보 묶음 종료

        private void Reset() // 컴포넌트 최초 추가 기본 참조 구성
        { // 최초 기본 참조 구성 처리
            generator = GetComponent<ProceduralMapGenerator>(); // 같은 오브젝트의 생성기 자동 연결
        } // 최초 기본 참조 구성 처리 종료

        private void OnValidate() // Inspector 장애물 계획 설정 보정
        { // 장애물 계획 설정 보정 처리
            generator = generator != null ? generator : GetComponent<ProceduralMapGenerator>(); // 누락된 생성기 참조 자동 연결
            safeMinimumRisk = Mathf.Max(0, safeMinimumRisk); // 안전 경로 최소 위험도 음수 방지
            safeMaximumRisk = Mathf.Max(safeMinimumRisk, safeMaximumRisk); // 안전 경로 최대 위험도 보정
            highRiskMinimumRisk = Mathf.Max(0, highRiskMinimumRisk); // 고위험 경로 최소 위험도 음수 방지
            highRiskMaximumRisk = Mathf.Max(highRiskMinimumRisk, highRiskMaximumRisk); // 고위험 경로 최대 위험도 보정
            minimumRiskGap = Mathf.Max(1, minimumRiskGap); // 최소 위험도 차이 양수 보장
            maximumObstaclesPerModule = Mathf.Max(1, maximumObstaclesPerModule); // 모듈별 최대 장애물 수 양수 보장
        } // 장애물 계획 설정 보정 처리 종료

        [ContextMenu("Generate Branch Obstacles")] // Inspector 분기 장애물 생성 메뉴 등록
        public void GenerateBranchObstacles() // 현재 생성 맵에 분기 장애물 생성
        { // 분기 장애물 생성 처리
            generator = generator != null ? generator : GetComponent<ProceduralMapGenerator>(); // 현재 생성기 참조 보장
            GeneratePlan(generator); // 현재 생성 결과 기반 장애물 계획 실행

            if (lastReport.IsValid) // 장애물 계획 성공 확인
            { // 장애물 계획 성공 처리
                Debug.Log($"[ProjectJ][Day38] {lastReport.BuildDetailedMessage()}", this); // 장애물 계획 성공 상세 로그 출력
            } // 장애물 계획 성공 처리 종료
            else // 장애물 계획 실패 확인
            { // 장애물 계획 실패 처리
                Debug.LogError(lastReport.BuildDetailedMessage(), this); // 장애물 계획 실패 상세 로그 출력
            } // 장애물 계획 실패 처리 종료
        } // 분기 장애물 생성 처리 종료

        [ContextMenu("Validate Branch Obstacles")] // Inspector 분기 장애물 검사 메뉴 등록
        public void ValidateBranchObstacles() // 현재 생성 장애물 배치 수동 검사
        { // 분기 장애물 수동 검사 처리
            generator = generator != null ? generator : GetComponent<ProceduralMapGenerator>(); // 현재 생성기 참조 보장
            ValidateCurrentPlan(generator); // 현재 장애물 배치 검사 실행

            if (lastReport.IsValid) // 장애물 배치 검사 성공 확인
            { // 장애물 배치 검사 성공 처리
                Debug.Log($"[ProjectJ][Day38] {lastReport.BuildDetailedMessage()}", this); // 장애물 검사 성공 상세 로그 출력
            } // 장애물 배치 검사 성공 처리 종료
            else // 장애물 배치 검사 실패 확인
            { // 장애물 배치 검사 실패 처리
                Debug.LogError(lastReport.BuildDetailedMessage(), this); // 장애물 검사 실패 상세 로그 출력
            } // 장애물 배치 검사 실패 처리 종료
        } // 분기 장애물 수동 검사 처리 종료

        public MapObstaclePlanReport GeneratePlan(ProceduralMapGenerator targetGenerator) // 지정 생성 결과에 장애물 계획과 실제 배치 실행
        { // 장애물 계획과 실제 배치 처리
            ClearPlacedObstacles(targetGenerator); // 기존 생성 장애물과 보고서 제거

            if (!enablePlanning) // 장애물 계획 비활성 확인
            { // 장애물 계획 비활성 처리
                lastReport = MapObstaclePlanReport.CreateNotRequired(); // 계획 불필요 보고서 저장
                return lastReport; // 계획 불필요 보고서 반환
            } // 장애물 계획 비활성 처리 종료

            if (targetGenerator == null) // 생성기 참조 누락 확인
            { // 생성기 참조 누락 처리
                lastReport = MapObstaclePlanReport.CreateRequired(0, 0); // 실패 기록용 필수 보고서 생성
                lastReport.AddIssue(MapObstaclePlanIssueCode.MissingPlannerConfiguration, "ProceduralMapGenerator 참조가 누락됐습니다.", -1, 0); // 생성기 누락 문제 등록
                return lastReport; // 생성기 누락 보고서 반환
            } // 생성기 참조 누락 처리 종료

            int safeLaneIndex = MapObstaclePlacementRules.ResolveSafeLane(targetGenerator.EffectiveSeed); // 고정 시드 기반 안전 경로 번호 결정
            int highRiskLaneIndex = -safeLaneIndex; // 반대편 고위험 경로 번호 결정
            lastReport = MapObstaclePlanReport.CreateRequired(safeLaneIndex, highRiskLaneIndex); // 새 분기 장애물 보고서 생성
            List<ObstacleDataDefinition> validDefinitions = CollectValidDefinitions(lastReport); // 유효한 장애물 후보 수집

            if (validDefinitions.Count == 0) // 유효한 장애물 후보 없음 확인
            { // 유효한 장애물 후보 없음 처리
                lastReport.AddIssue(MapObstaclePlanIssueCode.MissingPlannerConfiguration, "생성 가능한 장애물 데이터가 없습니다.", -1, 0); // 장애물 후보 누락 문제 등록
                return lastReport; // 장애물 후보 누락 보고서 반환
            } // 유효한 장애물 후보 없음 처리 종료

            List<SpawnCandidate> leftCandidates = CollectSpawnCandidates(targetGenerator, -1); // 왼쪽 분기 배치 후보 수집
            List<SpawnCandidate> rightCandidates = CollectSpawnCandidates(targetGenerator, 1); // 오른쪽 분기 배치 후보 수집
            RegisterMissingCandidateIssue(lastReport, leftCandidates, -1); // 왼쪽 분기 지점 누락 검사
            RegisterMissingCandidateIssue(lastReport, rightCandidates, 1); // 오른쪽 분기 지점 누락 검사

            System.Random random = new System.Random(targetGenerator.EffectiveSeed ^ 38038); // 장애물 전용 고정 시드 난수 생성기 준비
            ShuffleCandidates(leftCandidates, random); // 왼쪽 분기 후보 고정 시드 순서 섞기
            ShuffleCandidates(rightCandidates, random); // 오른쪽 분기 후보 고정 시드 순서 섞기
            PlanLane(leftCandidates, validDefinitions, MapObstaclePlacementRules.ResolveDifficulty(-1, safeLaneIndex), random); // 왼쪽 분기 난이도별 장애물 배치
            PlanLane(rightCandidates, validDefinitions, MapObstaclePlacementRules.ResolveDifficulty(1, safeLaneIndex), random); // 오른쪽 분기 난이도별 장애물 배치
            return ValidateCurrentPlan(targetGenerator); // 실제 생성 결과 재수집과 최종 검사 반환
        } // 장애물 계획과 실제 배치 처리 종료

        public MapObstaclePlanReport ValidateCurrentPlan(ProceduralMapGenerator targetGenerator) // 현재 생성 장애물 배치와 위험도 검사
        { // 현재 장애물 계획 검사 처리
            if (!enablePlanning) // 장애물 계획 비활성 확인
            { // 장애물 계획 비활성 처리
                lastReport = MapObstaclePlanReport.CreateNotRequired(); // 계획 불필요 보고서 저장
                return lastReport; // 계획 불필요 보고서 반환
            } // 장애물 계획 비활성 처리 종료

            if (targetGenerator == null) // 생성기 참조 누락 확인
            { // 생성기 참조 누락 처리
                lastReport = MapObstaclePlanReport.CreateRequired(0, 0); // 실패 기록용 필수 보고서 생성
                lastReport.AddIssue(MapObstaclePlanIssueCode.MissingPlannerConfiguration, "ProceduralMapGenerator 참조가 누락됐습니다.", -1, 0); // 생성기 누락 문제 등록
                return lastReport; // 생성기 누락 보고서 반환
            } // 생성기 참조 누락 처리 종료

            int safeLaneIndex = MapObstaclePlacementRules.ResolveSafeLane(targetGenerator.EffectiveSeed); // 고정 시드 기반 안전 경로 번호 결정
            int highRiskLaneIndex = -safeLaneIndex; // 반대편 고위험 경로 번호 결정
            MapObstaclePlanReport validationReport = MapObstaclePlanReport.CreateRequired(safeLaneIndex, highRiskLaneIndex); // 새 검사 보고서 생성
            CollectValidDefinitions(validationReport); // 현재 장애물 후보 설정 유효성 재검사
            RegisterMissingCandidateIssue(validationReport, CollectSpawnCandidates(targetGenerator, -1), -1); // 왼쪽 분기 배치 지점 존재 재검사
            RegisterMissingCandidateIssue(validationReport, CollectSpawnCandidates(targetGenerator, 1), 1); // 오른쪽 분기 배치 지점 존재 재검사
            MapPlacedObstacle[] placedObstacles = targetGenerator.GetComponentsInChildren<MapPlacedObstacle>(true); // 현재 생성 맵의 모든 장애물 표식 수집
            HashSet<string> usedPointKeys = new HashSet<string>(StringComparer.Ordinal); // 사용 배치 지점 중복 검사 집합 생성
            int leftRisk = 0; // 왼쪽 분기 총 위험도 초기화
            int rightRisk = 0; // 오른쪽 분기 총 위험도 초기화
            int leftCount = 0; // 왼쪽 분기 장애물 수 초기화
            int rightCount = 0; // 오른쪽 분기 장애물 수 초기화

            for (int obstacleIndex = 0; obstacleIndex < placedObstacles.Length; obstacleIndex++) // 모든 생성 장애물 순회
            { // 단일 생성 장애물 검사 처리
                MapPlacedObstacle placedObstacle = placedObstacles[obstacleIndex]; // 현재 생성 장애물 조회

                if (placedObstacle == null) // 생성 장애물 참조 누락 확인
                { // 생성 장애물 참조 누락 처리
                    continue; // 현재 장애물 검사 생략
                } // 생성 장애물 참조 누락 처리 종료

                if (!placedObstacle.TryValidate(out string placementReason)) // 장애물 배치 자체 검사
                { // 장애물 배치 자체 실패 처리
                    validationReport.AddIssue(MapObstaclePlanIssueCode.InvalidPlacedObstacle, placementReason, placedObstacle.NodeIndex, placedObstacle.LaneIndex); // 잘못된 배치 문제 등록
                } // 장애물 배치 자체 실패 처리 종료

                string pointId = placedObstacle.SpawnPoint != null ? placedObstacle.SpawnPoint.PointId : "MissingPoint"; // 안전한 배치 지점 ID 계산
                string pointKey = $"{placedObstacle.NodeIndex}:{pointId}"; // 노드와 지점 고유 키 생성

                if (!MapObstaclePlacementRules.IsUniquePoint(usedPointKeys, placedObstacle.NodeIndex, pointId)) // 같은 지점 중복 사용 확인
                { // 같은 지점 중복 사용 처리
                    validationReport.AddIssue(MapObstaclePlanIssueCode.DuplicateSpawnPoint, $"{pointKey} 지점에 장애물이 중복 배치됐습니다.", placedObstacle.NodeIndex, placedObstacle.LaneIndex); // 중복 지점 문제 등록
                } // 같은 지점 중복 사용 처리 종료
                else // 새 배치 지점 확인
                { // 새 배치 지점 처리
                    usedPointKeys.Add(pointKey); // 사용 배치 지점 집합 추가
                } // 새 배치 지점 처리 종료

                string moduleId = ResolveModuleId(targetGenerator, placedObstacle.NodeIndex); // 배치 노드의 모듈 ID 계산
                string obstacleId = placedObstacle.ObstacleData != null ? placedObstacle.ObstacleData.DataId : "MissingObstacle"; // 안전한 장애물 ID 계산
                validationReport.AddPlacement(new MapObstaclePlacementRecord(placedObstacle.NodeIndex, placedObstacle.LaneIndex, moduleId, pointId, obstacleId, placedObstacle.AppliedRiskScore, placedObstacle.transform.position)); // 현재 장애물 배치 기록 추가

                if (placedObstacle.LaneIndex < 0) // 왼쪽 분기 장애물 확인
                { // 왼쪽 분기 장애물 처리
                    leftRisk += placedObstacle.AppliedRiskScore; // 왼쪽 총 위험도 누적
                    leftCount++; // 왼쪽 장애물 수 증가
                } // 왼쪽 분기 장애물 처리 종료
                else if (placedObstacle.LaneIndex > 0) // 오른쪽 분기 장애물 확인
                { // 오른쪽 분기 장애물 처리
                    rightRisk += placedObstacle.AppliedRiskScore; // 오른쪽 총 위험도 누적
                    rightCount++; // 오른쪽 장애물 수 증가
                } // 오른쪽 분기 장애물 처리 종료
            } // 단일 생성 장애물 검사 처리 종료

            MapBranchDifficulty leftDifficulty = MapObstaclePlacementRules.ResolveDifficulty(-1, safeLaneIndex); // 왼쪽 분기 난이도 결정
            MapBranchDifficulty rightDifficulty = MapObstaclePlacementRules.ResolveDifficulty(1, safeLaneIndex); // 오른쪽 분기 난이도 결정
            validationReport.SetBranchSummaries(new MapObstacleBranchSummary(-1, leftDifficulty, leftRisk, leftCount), new MapObstacleBranchSummary(1, rightDifficulty, rightRisk, rightCount)); // 좌우 분기 위험도 요약 저장
            ValidateBranchBudget(validationReport, -1, leftDifficulty, leftRisk); // 왼쪽 분기 위험도 예산 검사
            ValidateBranchBudget(validationReport, 1, rightDifficulty, rightRisk); // 오른쪽 분기 위험도 예산 검사
            int safeRisk = safeLaneIndex < 0 ? leftRisk : rightRisk; // 안전 경로 총 위험도 계산
            int highRisk = highRiskLaneIndex < 0 ? leftRisk : rightRisk; // 고위험 경로 총 위험도 계산

            if (!MapObstaclePlacementRules.HasRequiredRiskGap(safeRisk, highRisk, minimumRiskGap)) // 안전과 고위험 경로 차이 부족 확인
            { // 위험도 차이 부족 처리
                validationReport.AddIssue(MapObstaclePlanIssueCode.RiskGapTooSmall, $"안전 {safeRisk}와 고위험 {highRisk}의 차이가 최소 {minimumRiskGap}보다 작습니다.", -1, 0); // 위험도 차이 문제 등록
            } // 위험도 차이 부족 처리 종료

            lastReport = validationReport; // 완성된 검사 보고서 저장
            return lastReport; // 완성된 검사 보고서 반환
        } // 현재 장애물 계획 검사 처리 종료

        public void ClearPlacedObstacles(ProceduralMapGenerator targetGenerator) // 현재 생성된 장애물 인스턴스와 보고서 제거
        { // 생성 장애물 제거 처리
            if (targetGenerator != null) // 제거 대상 생성기 존재 확인
            { // 제거 대상 생성기 처리
                MapPlacedObstacle[] placedObstacles = targetGenerator.GetComponentsInChildren<MapPlacedObstacle>(true); // 생성기 아래 모든 장애물 표식 수집

                for (int obstacleIndex = placedObstacles.Length - 1; obstacleIndex >= 0; obstacleIndex--) // 모든 생성 장애물 역순 순회
                { // 단일 생성 장애물 제거 처리
                    MapPlacedObstacle placedObstacle = placedObstacles[obstacleIndex]; // 현재 제거할 장애물 조회

                    if (placedObstacle == null) // 제거 대상 누락 확인
                    { // 제거 대상 누락 처리
                        continue; // 현재 제거 생략
                    } // 제거 대상 누락 처리 종료

                    DestroyPlacedObstacle(placedObstacle.gameObject); // 현재 생성 장애물 오브젝트 제거
                } // 단일 생성 장애물 제거 처리 종료
            } // 제거 대상 생성기 처리 종료

            lastReport = MapObstaclePlanReport.CreateNotRun(); // 최근 장애물 보고서 초기화
        } // 생성 장애물 제거 처리 종료

        public void ResetReport() // 장애물 계획 보고서만 초기화
        { // 장애물 계획 보고서 초기화 처리
            lastReport = MapObstaclePlanReport.CreateNotRun(); // 최근 장애물 보고서 미실행 상태 저장
        } // 장애물 계획 보고서 초기화 처리 종료

        private List<ObstacleDataDefinition> CollectValidDefinitions(MapObstaclePlanReport report) // 유효한 장애물 후보 데이터 수집
        { // 유효한 장애물 후보 수집 처리
            List<ObstacleDataDefinition> validDefinitions = new List<ObstacleDataDefinition>(); // 유효 장애물 결과 목록 생성

            if (obstacleDefinitions == null) // 장애물 후보 배열 누락 확인
            { // 장애물 후보 배열 누락 처리
                return validDefinitions; // 빈 장애물 결과 반환
            } // 장애물 후보 배열 누락 처리 종료

            for (int definitionIndex = 0; definitionIndex < obstacleDefinitions.Length; definitionIndex++) // 모든 장애물 후보 순회
            { // 단일 장애물 후보 검사 처리
                ObstacleDataDefinition definition = obstacleDefinitions[definitionIndex]; // 현재 장애물 후보 조회

                if (definition == null) // 장애물 후보 참조 누락 확인
                { // 장애물 후보 참조 누락 처리
                    report.AddIssue(MapObstaclePlanIssueCode.InvalidObstacleData, $"장애물 후보 {definitionIndex}번 참조가 비어 있습니다.", -1, 0); // 빈 후보 문제 등록
                    continue; // 현재 후보 제외
                } // 장애물 후보 참조 누락 처리 종료

                if (!definition.TryValidateObstacle(out string reason)) // 장애물 후보 데이터 검사
                { // 장애물 후보 데이터 실패 처리
                    report.AddIssue(MapObstaclePlanIssueCode.InvalidObstacleData, reason, -1, 0); // 장애물 데이터 문제 등록
                    continue; // 현재 후보 제외
                } // 장애물 후보 데이터 실패 처리 종료

                validDefinitions.Add(definition); // 유효 장애물 후보 등록
            } // 단일 장애물 후보 검사 처리 종료

            validDefinitions.Sort((left, right) => string.CompareOrdinal(left.DataId, right.DataId)); // 장애물 ID 기준 고정 정렬
            return validDefinitions; // 유효 장애물 결과 반환
        } // 유효한 장애물 후보 수집 처리 종료

        private List<SpawnCandidate> CollectSpawnCandidates(ProceduralMapGenerator targetGenerator, int laneIndex) // 지정 분기 장애물 배치 후보 수집
        { // 지정 분기 배치 후보 수집 처리
            List<SpawnCandidate> candidates = new List<SpawnCandidate>(); // 분기 배치 후보 결과 목록 생성
            IReadOnlyList<MapGenerationGraphNode> nodes = targetGenerator.GraphNodes; // 생성 그래프 노드 목록 조회
            IReadOnlyList<MapModuleDefinition> modules = targetGenerator.GeneratedModules; // 생성 모듈 인스턴스 목록 조회
            int sharedCount = Mathf.Min(nodes.Count, modules.Count); // 안전하게 함께 순회할 개수 계산

            for (int nodeIndex = 0; nodeIndex < sharedCount; nodeIndex++) // 모든 생성 노드와 모듈 순회
            { // 단일 생성 노드 배치 지점 수집 처리
                MapGenerationGraphNode node = nodes[nodeIndex]; // 현재 생성 그래프 노드 조회
                MapModuleDefinition module = modules[nodeIndex]; // 현재 생성 모듈 조회

                if (node == null || module == null || node.LaneIndex != laneIndex) // 대상 분기 모듈 여부 확인
                { // 대상 외 모듈 처리
                    continue; // 현재 모듈 제외
                } // 대상 외 모듈 처리 종료

                MapObstacleSpawnPoint[] spawnPoints = module.GetComponentsInChildren<MapObstacleSpawnPoint>(true); // 현재 모듈의 모든 배치 지점 수집

                for (int pointIndex = 0; pointIndex < spawnPoints.Length; pointIndex++) // 모든 모듈 배치 지점 순회
                { // 단일 배치 지점 후보 등록 처리
                    MapObstacleSpawnPoint spawnPoint = spawnPoints[pointIndex]; // 현재 배치 지점 조회

                    if (spawnPoint == null || !spawnPoint.EnabledForGeneration) // 배치 지점 사용 가능 여부 확인
                    { // 사용 불가 배치 지점 처리
                        continue; // 현재 배치 지점 제외
                    } // 사용 불가 배치 지점 처리 종료

                    SpawnCandidate candidate = new SpawnCandidate(); // 새 배치 후보 생성
                    candidate.NodeIndex = node.NodeIndex; // 후보 노드 번호 저장
                    candidate.LaneIndex = laneIndex; // 후보 분기 번호 저장
                    candidate.Module = module; // 후보 모듈 저장
                    candidate.SpawnPoint = spawnPoint; // 후보 배치 지점 저장
                    candidates.Add(candidate); // 완성된 배치 후보 등록
                } // 단일 배치 지점 후보 등록 처리 종료
            } // 단일 생성 노드 배치 지점 수집 처리 종료

            candidates.Sort(CompareCandidates); // 노드와 지점 ID 기준 고정 정렬
            return candidates; // 분기 배치 후보 결과 반환
        } // 지정 분기 배치 후보 수집 처리 종료

        private int CompareCandidates(SpawnCandidate left, SpawnCandidate right) // 배치 후보 고정 정렬 비교
        { // 배치 후보 비교 처리
            int nodeComparison = left.NodeIndex.CompareTo(right.NodeIndex); // 노드 번호 우선 비교
            return nodeComparison != 0 ? nodeComparison : string.CompareOrdinal(left.SpawnPoint.PointId, right.SpawnPoint.PointId); // 노드가 같으면 지점 ID 비교 반환
        } // 배치 후보 비교 처리 종료

        private void RegisterMissingCandidateIssue(MapObstaclePlanReport report, List<SpawnCandidate> candidates, int laneIndex) // 분기 배치 지점 누락 문제 등록
        { // 분기 배치 지점 누락 검사 처리
            if (candidates.Count == 0) // 분기 배치 지점 없음 확인
            { // 분기 배치 지점 없음 처리
                report.AddIssue(MapObstaclePlanIssueCode.MissingSpawnPoint, $"Lane {laneIndex} 분기에 활성 장애물 배치 지점이 없습니다.", -1, laneIndex); // 배치 지점 누락 문제 등록
            } // 분기 배치 지점 없음 처리 종료
        } // 분기 배치 지점 누락 검사 처리 종료

        private void PlanLane(List<SpawnCandidate> candidates, List<ObstacleDataDefinition> definitions, MapBranchDifficulty difficulty, System.Random random) // 단일 분기 목표 위험도까지 장애물 배치
        { // 단일 분기 장애물 계획 처리
            int minimumRisk = difficulty == MapBranchDifficulty.Safe ? safeMinimumRisk : highRiskMinimumRisk; // 난이도별 최소 위험도 선택
            int maximumRisk = difficulty == MapBranchDifficulty.Safe ? safeMaximumRisk : highRiskMaximumRisk; // 난이도별 최대 위험도 선택
            int targetRisk = MapObstaclePlacementRules.CalculateTargetRisk(difficulty, safeMinimumRisk, safeMaximumRisk, highRiskMinimumRisk, highRiskMaximumRisk); // 난이도별 목표 위험도 계산
            Dictionary<int, int> obstacleCountsByNode = new Dictionary<int, int>(); // 노드별 배치 장애물 수 사전 생성
            int currentRisk = 0; // 현재 분기 누적 위험도 초기화

            for (int candidateIndex = 0; candidateIndex < candidates.Count; candidateIndex++) // 모든 배치 후보 순회
            { // 단일 배치 후보 계획 처리
                SpawnCandidate candidate = candidates[candidateIndex]; // 현재 배치 후보 조회
                int currentNodeCount = obstacleCountsByNode.TryGetValue(candidate.NodeIndex, out int storedCount) ? storedCount : 0; // 현재 노드 배치 수 조회

                if (currentNodeCount >= maximumObstaclesPerModule) // 모듈별 장애물 제한 도달 확인
                { // 모듈별 장애물 제한 도달 처리
                    continue; // 현재 배치 후보 제외
                } // 모듈별 장애물 제한 도달 처리 종료

                ObstacleDataDefinition selectedDefinition = SelectDefinition(candidate.SpawnPoint, definitions, targetRisk - currentRisk, maximumRisk - currentRisk, random); // 남은 위험도에 맞는 장애물 선택

                if (selectedDefinition == null) // 배치 가능한 장애물 없음 확인
                { // 배치 가능한 장애물 없음 처리
                    continue; // 현재 배치 후보 제외
                } // 배치 가능한 장애물 없음 처리 종료

                int appliedRisk = candidate.SpawnPoint.CalculateRiskScore(selectedDefinition); // 지점 배율 적용 위험도 계산
                CreatePlacedObstacle(candidate, selectedDefinition, appliedRisk); // 실제 장애물 Prefab 생성과 표식 초기화
                currentRisk += appliedRisk; // 분기 누적 위험도 증가
                obstacleCountsByNode[candidate.NodeIndex] = currentNodeCount + 1; // 현재 노드 장애물 수 갱신

                if (currentRisk >= targetRisk && currentRisk >= minimumRisk) // 목표와 최소 위험도 달성 확인
                { // 목표 위험도 달성 처리
                    break; // 현재 분기 추가 배치 종료
                } // 목표 위험도 달성 처리 종료
            } // 단일 배치 후보 계획 처리 종료
        } // 단일 분기 장애물 계획 처리 종료

        private ObstacleDataDefinition SelectDefinition(MapObstacleSpawnPoint spawnPoint, List<ObstacleDataDefinition> definitions, int remainingTargetRisk, int remainingMaximumRisk, System.Random random) // 배치 지점과 남은 위험도에 맞는 장애물 선택
        { // 장애물 후보 선택 처리
            List<ObstacleDataDefinition> fittingDefinitions = new List<ObstacleDataDefinition>(); // 배치 가능한 장애물 결과 목록 생성

            for (int definitionIndex = 0; definitionIndex < definitions.Count; definitionIndex++) // 모든 유효 장애물 후보 순회
            { // 단일 장애물 배치 가능성 검사 처리
                ObstacleDataDefinition definition = definitions[definitionIndex]; // 현재 장애물 후보 조회
                int appliedRisk = spawnPoint.CalculateRiskScore(definition); // 현재 후보 적용 위험도 계산

                if (appliedRisk > remainingMaximumRisk) // 남은 최대 위험도 초과 확인
                { // 남은 최대 위험도 초과 처리
                    continue; // 현재 장애물 후보 제외
                } // 남은 최대 위험도 초과 처리 종료

                if (spawnPoint.CanPlace(definition, out string unusedReason)) // 배치 지점 통로 안전성 확인
                { // 배치 가능한 장애물 처리
                    fittingDefinitions.Add(definition); // 배치 가능 목록 추가
                } // 배치 가능한 장애물 처리 종료
            } // 단일 장애물 배치 가능성 검사 처리 종료

            if (fittingDefinitions.Count == 0) // 배치 가능한 장애물 없음 확인
            { // 배치 가능한 장애물 없음 처리
                return null; // 장애물 선택 실패 반환
            } // 배치 가능한 장애물 없음 처리 종료

            int randomizedOffset = random.Next(fittingDefinitions.Count); // 동점 후보 순서용 고정 시드 오프셋 계산
            ObstacleDataDefinition bestDefinition = null; // 최적 장애물 결과 초기화
            int bestDistance = int.MaxValue; // 목표 위험도와 최적 차이 초기화

            for (int offsetIndex = 0; offsetIndex < fittingDefinitions.Count; offsetIndex++) // 모든 배치 가능 장애물 순회
            { // 단일 배치 가능 장애물 평가 처리
                int definitionIndex = (randomizedOffset + offsetIndex) % fittingDefinitions.Count; // 오프셋이 적용된 후보 번호 계산
                ObstacleDataDefinition definition = fittingDefinitions[definitionIndex]; // 현재 평가 장애물 조회
                int appliedRisk = spawnPoint.CalculateRiskScore(definition); // 현재 후보 적용 위험도 계산
                int distance = Mathf.Abs(remainingTargetRisk - appliedRisk); // 남은 목표와 위험도 차이 계산

                if (distance < bestDistance) // 기존 최적보다 가까운 후보 확인
                { // 새 최적 후보 처리
                    bestDefinition = definition; // 새 최적 장애물 저장
                    bestDistance = distance; // 새 최적 차이 저장
                } // 새 최적 후보 처리 종료
            } // 단일 배치 가능 장애물 평가 처리 종료

            return bestDefinition; // 최종 선택 장애물 반환
        } // 장애물 후보 선택 처리 종료

        private void CreatePlacedObstacle(SpawnCandidate candidate, ObstacleDataDefinition definition, int appliedRisk) // 선택 장애물 Prefab 실제 생성
        { // 선택 장애물 생성 처리
            GameObject obstacleObject = Instantiate(definition.ObstaclePrefab, candidate.SpawnPoint.transform); // 배치 지점 자식으로 장애물 Prefab 생성
            obstacleObject.name = $"GeneratedObstacle_{candidate.NodeIndex}_{candidate.SpawnPoint.PointId}_{definition.DataId}"; // 생성 장애물 식별 이름 적용
            obstacleObject.transform.localPosition = Vector3.zero; // 배치 지점 중심 위치 적용
            obstacleObject.transform.localRotation = Quaternion.identity; // 배치 지점 회전 적용
            MapPlacedObstacle placedObstacle = obstacleObject.GetComponent<MapPlacedObstacle>(); // 기존 장애물 배치 표식 조회

            if (placedObstacle == null) // 장애물 배치 표식 누락 확인
            { // 장애물 배치 표식 추가 처리
                placedObstacle = obstacleObject.AddComponent<MapPlacedObstacle>(); // 새 장애물 배치 표식 추가
            } // 장애물 배치 표식 추가 처리 종료

            placedObstacle.Initialize(definition, candidate.SpawnPoint, candidate.NodeIndex, candidate.LaneIndex, appliedRisk); // 생성 장애물 배치 정보 초기화
        } // 선택 장애물 생성 처리 종료

        private void ValidateBranchBudget(MapObstaclePlanReport report, int laneIndex, MapBranchDifficulty difficulty, int totalRisk) // 단일 분기 위험도 예산 검사
        { // 단일 분기 위험도 예산 검사 처리
            int minimumRisk = difficulty == MapBranchDifficulty.Safe ? safeMinimumRisk : highRiskMinimumRisk; // 난이도별 최소 위험도 선택
            int maximumRisk = difficulty == MapBranchDifficulty.Safe ? safeMaximumRisk : highRiskMaximumRisk; // 난이도별 최대 위험도 선택

            if (totalRisk < minimumRisk) // 분기 최소 위험도 미달 확인
            { // 분기 최소 위험도 미달 처리
                report.AddIssue(MapObstaclePlanIssueCode.RiskBudgetNotReached, $"Lane {laneIndex} 위험도 {totalRisk}가 최소 {minimumRisk}보다 낮습니다.", -1, laneIndex); // 최소 위험도 미달 문제 등록
            } // 분기 최소 위험도 미달 처리 종료

            if (totalRisk > maximumRisk) // 분기 최대 위험도 초과 확인
            { // 분기 최대 위험도 초과 처리
                report.AddIssue(MapObstaclePlanIssueCode.RiskBudgetExceeded, $"Lane {laneIndex} 위험도 {totalRisk}가 최대 {maximumRisk}를 초과합니다.", -1, laneIndex); // 최대 위험도 초과 문제 등록
            } // 분기 최대 위험도 초과 처리 종료
        } // 단일 분기 위험도 예산 검사 처리 종료

        private string ResolveModuleId(ProceduralMapGenerator targetGenerator, int nodeIndex) // 그래프 노드 번호의 모듈 ID 계산
        { // 모듈 ID 계산 처리
            IReadOnlyList<MapGenerationGraphNode> nodes = targetGenerator.GraphNodes; // 생성 그래프 노드 목록 조회

            if (nodeIndex < 0 || nodeIndex >= nodes.Count || nodes[nodeIndex] == null) // 노드 번호 범위와 참조 확인
            { // 잘못된 노드 번호 처리
                return "MissingModule"; // 누락 모듈 ID 반환
            } // 잘못된 노드 번호 처리 종료

            return nodes[nodeIndex].ModuleId; // 현재 노드 모듈 ID 반환
        } // 모듈 ID 계산 처리 종료

        private void ShuffleCandidates(List<SpawnCandidate> candidates, System.Random random) // 배치 후보 목록 고정 시드 섞기
        { // 배치 후보 목록 섞기 처리
            for (int index = candidates.Count - 1; index > 0; index--) // 뒤에서 두 번째 원소까지 역순 순회
            { // 단일 후보 교환 처리
                int swapIndex = random.Next(index + 1); // 현재 범위의 교환 대상 번호 선택
                SpawnCandidate temporary = candidates[index]; // 현재 후보 임시 저장
                candidates[index] = candidates[swapIndex]; // 교환 대상 후보를 현재 위치에 저장
                candidates[swapIndex] = temporary; // 임시 후보를 교환 대상 위치에 저장
            } // 단일 후보 교환 처리 종료
        } // 배치 후보 목록 섞기 처리 종료

        private void DestroyPlacedObstacle(GameObject obstacleObject) // 생성 장애물 오브젝트 안전 제거
        { // 생성 장애물 제거 처리
            if (obstacleObject == null) // 제거 대상 누락 확인
            { // 제거 대상 누락 처리
                return; // 제거 작업 생략
            } // 제거 대상 누락 처리 종료

            if (Application.isPlaying) // Play Mode 여부 확인
            { // Play Mode 제거 처리
                obstacleObject.transform.SetParent(null, true); // 삭제 대기 장애물을 현재 생성 맵 계층에서 즉시 분리
                obstacleObject.SetActive(false); // 제거 대기 장애물 비활성화
                Destroy(obstacleObject); // 프레임 종료 시 장애물 제거
            } // Play Mode 제거 처리 종료
            else // Edit Mode 여부 확인
            { // Edit Mode 제거 처리
                DestroyImmediate(obstacleObject); // 생성 장애물 즉시 제거
            } // Edit Mode 제거 처리 종료
        } // 생성 장애물 제거 처리 종료

#if UNITY_EDITOR // Unity Editor 전용 설정 시작
        public void ConfigureForEditor(ProceduralMapGenerator newGenerator, bool newEnablePlanning, ObstacleDataDefinition[] newObstacleDefinitions, int newSafeMinimumRisk, int newSafeMaximumRisk, int newHighRiskMinimumRisk, int newHighRiskMaximumRisk, int newMinimumRiskGap, int newMaximumObstaclesPerModule) // Editor 도구용 장애물 계획 설정
        { // Editor 장애물 계획 설정 처리
            generator = newGenerator; // 새 절차적 맵 생성기 참조 저장
            enablePlanning = newEnablePlanning; // 새 장애물 계획 사용 여부 저장
            obstacleDefinitions = newObstacleDefinitions; // 새 장애물 후보 목록 저장
            safeMinimumRisk = newSafeMinimumRisk; // 새 안전 경로 최소 위험도 저장
            safeMaximumRisk = newSafeMaximumRisk; // 새 안전 경로 최대 위험도 저장
            highRiskMinimumRisk = newHighRiskMinimumRisk; // 새 고위험 경로 최소 위험도 저장
            highRiskMaximumRisk = newHighRiskMaximumRisk; // 새 고위험 경로 최대 위험도 저장
            minimumRiskGap = newMinimumRiskGap; // 새 최소 위험도 차이 저장
            maximumObstaclesPerModule = newMaximumObstaclesPerModule; // 새 모듈별 최대 장애물 수 저장
            OnValidate(); // 장애물 계획 설정값 즉시 보정
        } // Editor 장애물 계획 설정 처리 종료
#endif // Unity Editor 전용 설정 종료
    } // 분기 장애물 계획기 묶음 종료
} // 맵 생성 기능 묶음 종료
