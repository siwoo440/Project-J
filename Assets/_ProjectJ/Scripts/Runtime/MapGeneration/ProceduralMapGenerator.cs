using System; // 시스템 난수 기능 참조
using System.Collections.Generic; // 목록과 사전 기능 참조
using System.Text; // 생성 결과 서명 문자열 기능 참조
using UnityEngine; // Unity 오브젝트 생성 기능 참조

namespace ProjectJ.MapGeneration // 맵 생성 기능 네임스페이스 선언
{ // 맵 생성 기능 묶음
    [DisallowMultipleComponent] // 맵 생성기 컴포넌트 중복 방지
    public sealed class ProceduralMapGenerator : MonoBehaviour // 시드 기반 분기 맵 생성기 선언
    { // 시드 기반 분기 맵 생성기 묶음
        [SerializeField] private MapGenerationSettings settings; // 맵 생성 설정 에셋
        [SerializeField] private Transform generatedRoot; // 생성 모듈 보관 루트
        [SerializeField] private bool generateOnStart = true; // 게임 시작 자동 생성 여부
        [SerializeField] private bool logDetailedResults = true; // 생성 결과 로그 표시 여부

        private readonly List<MapModuleDefinition> generatedModules = new List<MapModuleDefinition>(); // 현재 생성된 모듈 목록
        private readonly List<MapGenerationGraphNode> graphNodes = new List<MapGenerationGraphNode>(); // 현재 생성 경로 노드 목록
        private readonly List<MapGenerationGraphEdge> graphEdges = new List<MapGenerationGraphEdge>(); // 현재 생성 경로 간선 목록
        private readonly Dictionary<MapModuleDefinition, int> graphNodeIndices = new Dictionary<MapModuleDefinition, int>(); // 모듈별 그래프 노드 번호 사전
        private readonly HashSet<MapModuleConnectionPoint> usedConnections = new HashSet<MapModuleConnectionPoint>(); // 사용 완료 연결 지점 집합
        private int effectiveSeed; // 이번 생성에 사용한 시드
        private bool lastGenerationSucceeded; // 최근 생성 성공 여부
        private string generationSignature = string.Empty; // 최근 생성 결과 재현 서명
        private MapGenerationValidationReport lastValidationReport = MapGenerationValidationReport.CreateNotRun(); // 최근 생성 결과 종합 검사 보고서
        private MapPlayableRouteReport lastPlayableRouteReport = MapPlayableRouteReport.CreateNotRun(); // 최근 플레이 가능 경로 검사 보고서
        private MapObstaclePlanReport lastObstaclePlanReport = MapObstaclePlanReport.CreateNotRun(); // 최근 분기 장애물 계획 보고서
        [SerializeField, HideInInspector] private float effectiveTargetHeight; // 이번 수직 생성 목표 높이
        [SerializeField, HideInInspector] private float generatedHeight; // 현재 누적 생성 높이
        [SerializeField, HideInInspector] private int ascendingModuleCount; // 현재 상승 모듈 개수
        [SerializeField, HideInInspector] private int consecutiveFlatModuleCount; // 현재 연속 평지 모듈 개수
        [SerializeField, HideInInspector] private int maximumObservedConsecutiveFlatModules; // 생성 중 최대 연속 평지 모듈 개수
        [SerializeField, HideInInspector] private float leftBranchHeight; // 왼쪽 분기 누적 높이
        [SerializeField, HideInInspector] private float rightBranchHeight; // 오른쪽 분기 누적 높이
        [SerializeField, HideInInspector] private int leftBranchAscendingModuleCount; // 왼쪽 분기 상승 모듈 개수
        [SerializeField, HideInInspector] private int rightBranchAscendingModuleCount; // 오른쪽 분기 상승 모듈 개수
        [SerializeField, HideInInspector] private int branchCombinationRetryCount; // 수직 분기 조합 재시도 횟수
        private float maximumAvailableHeightGain; // 후보 한 개의 최대 상승량
        private float sharedPathHeight; // 분기 전후 공통 경로 누적 높이
        private int sharedAscendingModuleCount; // 분기 전후 공통 경로 상승 모듈 개수
        private int sharedConsecutiveFlatModuleCount; // 공통 경로 연속 평지 모듈 개수
        private int leftBranchConsecutiveFlatModuleCount; // 왼쪽 분기 연속 평지 모듈 개수
        private int rightBranchConsecutiveFlatModuleCount; // 오른쪽 분기 연속 평지 모듈 개수

        public int EffectiveSeed => effectiveSeed; // 실제 사용 시드 반환
        public bool LastGenerationSucceeded => lastGenerationSucceeded; // 최근 생성 성공 여부 반환
        public int GeneratedModuleCount => generatedModules.Count; // 현재 생성 모듈 개수 반환
        public IReadOnlyList<MapModuleDefinition> GeneratedModules => generatedModules; // 현재 생성 모듈 목록 반환
        public IReadOnlyList<MapGenerationGraphNode> GraphNodes => graphNodes; // 현재 그래프 노드 목록 반환
        public IReadOnlyList<MapGenerationGraphEdge> GraphEdges => graphEdges; // 현재 그래프 간선 목록 반환
        public string GenerationSignature => generationSignature; // 최근 생성 결과 서명 반환
        public MapGenerationValidationReport LastValidationReport => lastValidationReport; // 최근 생성 결과 종합 검사 보고서 반환
        public MapPlayableRouteReport LastPlayableRouteReport => lastPlayableRouteReport; // 최근 플레이 가능 경로 검사 보고서 반환
        public MapObstaclePlanReport LastObstaclePlanReport => lastObstaclePlanReport; // 최근 분기 장애물 계획 보고서 반환
        public float EffectiveTargetHeight => effectiveTargetHeight; // 실제 수직 목표 높이 반환
        public float GeneratedHeight => generatedHeight; // 최종 누적 생성 높이 반환
        public int AscendingModuleCount => ascendingModuleCount; // 생성된 상승 모듈 개수 반환
        public int MaximumObservedConsecutiveFlatModules => maximumObservedConsecutiveFlatModules; // 최대 연속 평지 모듈 개수 반환
        public float LeftBranchHeight => leftBranchHeight; // 왼쪽 분기 누적 높이 반환
        public float RightBranchHeight => rightBranchHeight; // 오른쪽 분기 누적 높이 반환
        public int LeftBranchAscendingModuleCount => leftBranchAscendingModuleCount; // 왼쪽 분기 상승 모듈 개수 반환
        public int RightBranchAscendingModuleCount => rightBranchAscendingModuleCount; // 오른쪽 분기 상승 모듈 개수 반환
        public int BranchCombinationRetryCount => branchCombinationRetryCount; // 분기 조합 재시도 횟수 반환

        private struct PlacementOption // 단일 모듈 배치 후보 선언
        { // 단일 모듈 배치 후보 묶음
            public MapModuleDefinition Prefab; // 후보 모듈 Prefab
            public int QuarterTurns; // 후보 직각 회전 횟수
        } // 단일 모듈 배치 후보 묶음

        private struct PlacementResult // 성공한 단일 연결 배치 결과 선언
        { // 성공한 단일 연결 배치 결과 묶음
            public MapModuleDefinition Module; // 생성된 모듈
            public MapModuleConnectionPoint SourceExit; // 이전 모듈 출구
            public MapModuleConnectionPoint Entrance; // 생성 모듈 입구
        } // 성공한 단일 연결 배치 결과 묶음

        private struct VerticalPairOption // 좌우 수직 분기 배치 후보 선언
        { // 좌우 수직 분기 배치 후보 묶음
            public PlacementOption LeftOption; // 왼쪽 배치 후보
            public PlacementOption RightOption; // 오른쪽 배치 후보
        } // 좌우 수직 분기 배치 후보 묶음

        private struct GenerationSnapshot // 분기 조합 재시도용 생성 상태 선언
        { // 생성 상태 묶음
            public int ModuleCount; // 보존할 생성 모듈 개수
            public int NodeCount; // 보존할 그래프 노드 개수
            public int EdgeCount; // 보존할 그래프 간선 개수
            public HashSet<MapModuleConnectionPoint> UsedConnections; // 보존할 사용 연결 지점 집합
            public float SharedPathHeight; // 보존할 공통 경로 높이
            public int SharedAscendingModules; // 보존할 공통 경로 상승 수
            public int SharedConsecutiveFlatModules; // 보존할 공통 경로 연속 평지 수
            public int MaximumConsecutiveFlatModules; // 보존할 최대 연속 평지 수
        } // 생성 상태 묶음

        private void Start() // 게임 시작 처리
        { // 게임 시작 처리 묶음
            if (generateOnStart) // 시작 자동 생성 활성 확인
            { // 시작 자동 생성 처리
                GenerateMap(); // 새 맵 생성 실행
            } // 시작 자동 생성 처리
        } // 게임 시작 처리 묶음

        [ContextMenu("Generate Map")] // Inspector 맵 생성 메뉴 등록
        public void GenerateMap() // 새 시드 맵 생성 실행
        { // 새 시드 맵 생성 처리
            lastGenerationSucceeded = false; // 최근 생성 결과 초기화
            generationSignature = string.Empty; // 최근 생성 서명 초기화
            lastValidationReport = MapGenerationValidationReport.CreateNotRun(); // 최근 종합 검사 결과 초기화
            lastPlayableRouteReport = MapPlayableRouteReport.CreateNotRun(); // 최근 플레이 가능 경로 검사 결과 초기화
            lastObstaclePlanReport = MapObstaclePlanReport.CreateNotRun(); // 최근 분기 장애물 계획 결과 초기화

            if (settings == null) // 생성 설정 누락 확인
            { // 생성 설정 누락 처리
                lastValidationReport = MapGenerationValidationReport.CreateFailure(MapGenerationValidationIssueCode.GenerationFlowFailed, "Map Generation Settings가 연결되지 않았습니다."); // 생성 설정 누락 검사 보고서 저장
                Debug.LogError(lastValidationReport.BuildDetailedMessage(), this); // 생성 설정 누락 오류 출력
                return; // 맵 생성 중단
            } // 생성 설정 누락 처리

            List<MapModuleDefinition> validPrefabs = CollectValidPrefabs(); // 유효한 전체 후보 Prefab 수집
            List<MapModuleDefinition> ordinaryPrefabs = FilterOrdinaryPrefabs(validPrefabs); // 일반 경로 후보 Prefab 수집

            if (ordinaryPrefabs.Count == 0) // 일반 경로 후보 없음 확인
            { // 일반 경로 후보 없음 처리
                lastValidationReport = MapGenerationValidationReport.CreateFailure(MapGenerationValidationIssueCode.GenerationFlowFailed, "생성 가능한 일반 맵 모듈 Prefab이 없습니다."); // 후보 없음 검사 보고서 저장
                Debug.LogError(lastValidationReport.BuildDetailedMessage(), this); // 후보 없음 오류 출력
                return; // 맵 생성 중단
            } // 일반 경로 후보 없음 처리

            EnsureGeneratedRoot(); // 생성 모듈 루트 존재 보장
            ClearGeneratedMap(); // 이전 생성 결과 제거
            effectiveSeed = settings.RandomizeSeed ? Environment.TickCount : settings.Seed; // 이번 생성 시드 결정
            System.Random random = new System.Random(effectiveSeed); // 독립 난수 생성기 준비
            maximumAvailableHeightGain = MapVerticalGenerationRules.GetMaximumHeightGain(ordinaryPrefabs); // 전체 후보의 최대 상승량 계산
            effectiveTargetHeight = settings.UseVerticalGeneration ? CalculateEffectiveTargetHeight(random) : 0f; // 시드 기반 수직 목표 높이 결정
            int verticalRouteSlotCount = settings.UseVerticalGeneration && settings.UseBranchingPath ? MapVerticalBranchGenerationRules.CalculateRouteOrdinarySlotCount(settings.ModuleCount, CalculateBranchPairCount()) : settings.ModuleCount; // 생성 방식별 단일 경로 일반 슬롯 수 계산

            if (settings.UseVerticalGeneration && !MapVerticalGenerationRules.TryValidateConfiguration(verticalRouteSlotCount, effectiveTargetHeight, settings.MinimumAscendingModules, settings.MaximumConsecutiveFlatModules, maximumAvailableHeightGain, out string verticalConfigurationReason)) // 수직 목표 달성 가능성 확인
            { // 수직 목표 달성 불가 처리
                lastValidationReport = MapGenerationValidationReport.CreateFailure(MapGenerationValidationIssueCode.GenerationFlowFailed, $"수직 생성 설정 오류: {verticalConfigurationReason}"); // 수직 설정 오류 보고서 저장
                Debug.LogError(lastValidationReport.BuildDetailedMessage(), this); // 수직 설정 오류 출력
                return; // 맵 생성 중단
            } // 수직 목표 달성 불가 처리 종료

            MapModuleDefinition firstModule = settings.UseVerticalGeneration && settings.UseBranchingPath ? CreateFirstVerticalBranchModule(ordinaryPrefabs, random) : CreateFirstModule(ordinaryPrefabs, random); // 생성 방식에 맞는 첫 모듈 생성

            if (firstModule == null) // 첫 모듈 생성 실패 확인
            { // 첫 모듈 생성 실패 처리
                lastValidationReport = MapGenerationValidationReport.CreateFailure(MapGenerationValidationIssueCode.GenerationFlowFailed, "첫 번째 맵 모듈을 생성하지 못했습니다."); // 첫 모듈 실패 검사 보고서 저장
                Debug.LogError(lastValidationReport.BuildDetailedMessage(), this); // 첫 모듈 생성 오류 출력
                return; // 맵 생성 중단
            } // 첫 모듈 생성 실패 처리

            RegisterFirstModule(firstModule); // 첫 모듈과 그래프 시작 노드 등록
            bool generationFlowSucceeded = settings.UseVerticalGeneration && settings.UseBranchingPath ? GenerateVerticalBranchedPath(firstModule, validPrefabs, ordinaryPrefabs, random) : settings.UseVerticalGeneration ? GenerateLinearPath(firstModule, ordinaryPrefabs, random) : settings.UseBranchingPath ? GenerateBranchedPath(firstModule, validPrefabs, ordinaryPrefabs, random) : GenerateLinearPath(firstModule, ordinaryPrefabs, random); // 수직 분기·수직 선형·기존 분기 설정에 따른 생성 흐름 실행
            lastValidationReport = MapGenerationResultValidator.Validate(generatedModules, graphNodes, graphEdges, settings.ModuleCount, settings.OverlapTolerance, settings.ConnectionSizeTolerance, settings.ConnectionPositionTolerance); // 생성 결과 종합 검사 실행

            if (!generationFlowSucceeded) // 생성 흐름 실패 확인
            { // 생성 흐름 실패 처리
                lastValidationReport.AddIssue(MapGenerationValidationIssueCode.GenerationFlowFailed, "목표 맵 구조를 완성하기 전에 생성 흐름이 중단됐습니다.", -1, -1); // 생성 흐름 실패 문제 등록
            } // 생성 흐름 실패 처리 종료

            ApplyVerticalResultValidation(); // 수직 목표 높이와 상승 모듈 기준 검사
            RunPlayableRouteValidation(); // 시작부터 종료까지 실제 이동 경로 검사
            RunObstaclePlanning(); // 좌우 분기 장애물 생성과 통로 안전성 검사

            lastGenerationSucceeded = lastValidationReport.IsValid && lastPlayableRouteReport.IsValid && lastObstaclePlanReport.IsValid; // 생성 규격과 플레이 경로와 장애물 계획 통합 성공 여부 저장
            generationSignature = BuildGenerationSignature(); // 생성 결과 재현 서명 계산

            if (logDetailedResults) // 상세 로그 표시 활성 확인
            { // 상세 생성 결과 출력 처리
                string resultLabel = lastGenerationSucceeded ? "성공" : "실패"; // 생성 결과 문구 계산
                string verticalLabel = settings.UseVerticalGeneration ? $" | 높이: {generatedHeight:0.00}m/{effectiveTargetHeight:0.00}m | 상승 모듈: {ascendingModuleCount} | 최대 연속 평지: {maximumObservedConsecutiveFlatModules}" : string.Empty; // 수직 생성 결과 문구 계산
                string branchLabel = settings.UseVerticalGeneration && settings.UseBranchingPath ? $" | 분기 높이: L {leftBranchHeight:0.00}m/R {rightBranchHeight:0.00}m | 분기 상승: L {leftBranchAscendingModuleCount}/R {rightBranchAscendingModuleCount} | 재시도: {branchCombinationRetryCount}" : string.Empty; // 수직 분기 결과 문구 계산
                string routeLabel = $" | 플레이 경로: {lastPlayableRouteReport.RouteCount} | 경로 문제: {lastPlayableRouteReport.IssueCount}"; // 플레이 가능 경로 결과 문구 계산
                string obstacleLabel = lastObstaclePlanReport.IsRequired ? $" | 장애물: {lastObstaclePlanReport.PlacementCount} | 위험도: L {lastObstaclePlanReport.LeftBranch.TotalRiskScore}/R {lastObstaclePlanReport.RightBranch.TotalRiskScore} | 장애물 문제: {lastObstaclePlanReport.IssueCount}" : " | 장애물 계획: 미사용"; // 장애물 계획 결과 문구 계산
                Debug.Log($"[ProjectJ][Day38] 맵 생성과 분기 장애물 계획 {resultLabel} | 시드: {effectiveSeed} | 모듈: {generatedModules.Count}/{settings.ModuleCount} | 간선: {graphEdges.Count} | 생성 문제: {lastValidationReport.IssueCount}{routeLabel}{obstacleLabel}{verticalLabel}{branchLabel}", this); // 생성과 플레이 경로와 장애물 계획 요약 로그 출력

                if (!lastValidationReport.IsValid) // 종합 검사 실패 확인
                { // 종합 검사 실패 처리
                    Debug.LogError(lastValidationReport.BuildDetailedMessage(), this); // 모든 생성 문제 상세 로그 출력
                } // 종합 검사 실패 처리 종료

                if (!lastPlayableRouteReport.IsValid) // 플레이 가능 경로 검사 실패 확인
                { // 플레이 가능 경로 검사 실패 처리
                    Debug.LogError(lastPlayableRouteReport.BuildDetailedMessage(), this); // 모든 플레이 가능 경로 문제 상세 로그 출력
                } // 플레이 가능 경로 검사 실패 처리 종료

                if (!lastObstaclePlanReport.IsValid) // 분기 장애물 계획 실패 확인
                { // 분기 장애물 계획 실패 처리
                    Debug.LogError(lastObstaclePlanReport.BuildDetailedMessage(), this); // 모든 장애물 배치와 위험도 문제 상세 로그 출력
                } // 분기 장애물 계획 실패 처리 종료
            } // 상세 생성 결과 출력 처리
        } // 새 시드 맵 생성 처리

        [ContextMenu("Validate Generated Map")] // Inspector 생성 결과 검사 메뉴 등록
        public void ValidateGeneratedMap() // 현재 생성 결과 종합 검사 실행
        { // 현재 생성 결과 종합 검사 처리
            if (settings == null) // 생성 설정 누락 확인
            { // 생성 설정 누락 처리
                lastValidationReport = MapGenerationValidationReport.CreateFailure(MapGenerationValidationIssueCode.GenerationFlowFailed, "Map Generation Settings가 연결되지 않았습니다."); // 생성 설정 누락 검사 보고서 저장
                Debug.LogError(lastValidationReport.BuildDetailedMessage(), this); // 생성 설정 누락 오류 출력
                return; // 생성 결과 검사 중단
            } // 생성 설정 누락 처리 종료

            RecalculateVerticalProgress(); // 현재 모듈 목록에서 수직 진행 수치 다시 계산
            lastValidationReport = MapGenerationResultValidator.Validate(generatedModules, graphNodes, graphEdges, settings.ModuleCount, settings.OverlapTolerance, settings.ConnectionSizeTolerance, settings.ConnectionPositionTolerance); // 현재 생성 결과 종합 검사 실행
            ApplyVerticalResultValidation(); // 수직 생성 결과 기준 추가 검사
            RunPlayableRouteValidation(); // 현재 생성 결과의 실제 이동 경로 검사
            RunObstacleValidation(); // 현재 분기 장애물 배치와 위험도 재검사
            lastGenerationSucceeded = lastValidationReport.IsValid && lastPlayableRouteReport.IsValid && lastObstaclePlanReport.IsValid; // 생성 규격과 플레이 경로와 장애물 계획 통합 성공 상태 갱신

            if (lastGenerationSucceeded) // 생성 결과와 플레이 경로 검사 성공 확인
            { // 생성 결과 검사 성공 처리
                Debug.Log($"[ProjectJ][Day38] {lastValidationReport.BuildSummary()} {lastPlayableRouteReport.BuildSummary()} {lastObstaclePlanReport.BuildSummary()}", this); // 통합 검사 성공 로그 출력
            } // 생성 결과 검사 성공 처리 종료
            else // 생성 결과 검사 실패 확인
            { // 생성 결과 검사 실패 처리
                if (!lastValidationReport.IsValid) // 생성 규격 검사 실패 확인
                { // 생성 규격 검사 실패 처리
                    Debug.LogError(lastValidationReport.BuildDetailedMessage(), this); // 생성 규격 실패 상세 로그 출력
                } // 생성 규격 검사 실패 처리 종료

                if (!lastPlayableRouteReport.IsValid) // 플레이 가능 경로 검사 실패 확인
                { // 플레이 가능 경로 검사 실패 처리
                    Debug.LogError(lastPlayableRouteReport.BuildDetailedMessage(), this); // 경로 검사 실패 상세 로그 출력
                } // 플레이 가능 경로 검사 실패 처리 종료

                if (!lastObstaclePlanReport.IsValid) // 분기 장애물 계획 검사 실패 확인
                { // 분기 장애물 계획 검사 실패 처리
                    Debug.LogError(lastObstaclePlanReport.BuildDetailedMessage(), this); // 장애물 계획 실패 상세 로그 출력
                } // 분기 장애물 계획 검사 실패 처리 종료
            } // 생성 결과 검사 실패 처리 종료
        } // 현재 생성 결과 종합 검사 처리 종료

        [ContextMenu("Validate Playable Routes")] // Inspector 플레이 가능 경로 검사 메뉴 등록
        public void ValidatePlayableRoutes() // 현재 생성 결과의 시작부터 종료 경로 검사
        { // 플레이 가능 경로 수동 검사 처리
            RunPlayableRouteValidation(); // 현재 생성 그래프 경로 검사 실행
            lastGenerationSucceeded = lastValidationReport.IsValid && lastPlayableRouteReport.IsValid && lastObstaclePlanReport.IsValid; // 생성 규격과 경로와 장애물 계획 통합 상태 갱신

            if (lastPlayableRouteReport.IsValid) // 플레이 가능 경로 검사 성공 확인
            { // 플레이 가능 경로 검사 성공 처리
                Debug.Log($"[ProjectJ][Day37] {lastPlayableRouteReport.BuildDetailedMessage()}", this); // 발견된 모든 정상 경로 출력
            } // 플레이 가능 경로 검사 성공 처리 종료
            else // 플레이 가능 경로 검사 실패 확인
            { // 플레이 가능 경로 검사 실패 처리
                Debug.LogError(lastPlayableRouteReport.BuildDetailedMessage(), this); // 경로별 실패 원인 출력
            } // 플레이 가능 경로 검사 실패 처리 종료
        } // 플레이 가능 경로 수동 검사 처리 종료

        [ContextMenu("Regenerate Branch Obstacles")] // Inspector 분기 장애물 재생성 메뉴 등록
        public void RegenerateBranchObstacles() // 현재 생성 맵의 좌우 분기 장애물 다시 생성
        { // 분기 장애물 재생성 처리
            RunObstaclePlanning(); // 현재 생성 결과 기반 장애물 계획 다시 실행
            lastGenerationSucceeded = lastValidationReport.IsValid && lastPlayableRouteReport.IsValid && lastObstaclePlanReport.IsValid; // 모든 검사 통합 성공 상태 갱신
            generationSignature = BuildGenerationSignature(); // 장애물 계획을 포함한 생성 서명 갱신

            if (lastObstaclePlanReport.IsValid) // 장애물 계획 성공 확인
            { // 장애물 계획 성공 처리
                Debug.Log($"[ProjectJ][Day38] {lastObstaclePlanReport.BuildDetailedMessage()}", this); // 장애물 계획 성공 상세 로그 출력
            } // 장애물 계획 성공 처리 종료
            else // 장애물 계획 실패 확인
            { // 장애물 계획 실패 처리
                Debug.LogError(lastObstaclePlanReport.BuildDetailedMessage(), this); // 장애물 계획 실패 상세 로그 출력
            } // 장애물 계획 실패 처리 종료
        } // 분기 장애물 재생성 처리 종료

        [ContextMenu("Validate Branch Obstacles")] // Inspector 분기 장애물 검사 메뉴 등록
        public void ValidateBranchObstacles() // 현재 생성 장애물의 통로 폭과 위험도 검사
        { // 분기 장애물 수동 검사 처리
            RunObstacleValidation(); // 현재 장애물 배치 검사 실행
            lastGenerationSucceeded = lastValidationReport.IsValid && lastPlayableRouteReport.IsValid && lastObstaclePlanReport.IsValid; // 모든 검사 통합 성공 상태 갱신

            if (lastObstaclePlanReport.IsValid) // 장애물 배치 검사 성공 확인
            { // 장애물 배치 검사 성공 처리
                Debug.Log($"[ProjectJ][Day38] {lastObstaclePlanReport.BuildDetailedMessage()}", this); // 장애물 배치 성공 상세 로그 출력
            } // 장애물 배치 검사 성공 처리 종료
            else // 장애물 배치 검사 실패 확인
            { // 장애물 배치 검사 실패 처리
                Debug.LogError(lastObstaclePlanReport.BuildDetailedMessage(), this); // 장애물 배치 실패 상세 로그 출력
            } // 장애물 배치 검사 실패 처리 종료
        } // 분기 장애물 수동 검사 처리 종료

        [ContextMenu("Clear Generated Map")] // Inspector 생성 맵 제거 메뉴 등록
        public void ClearGeneratedMap() // 현재 생성된 맵 전체 제거
        { // 생성 맵 제거 처리
            MapBranchObstaclePlanner obstaclePlanner = GetComponent<MapBranchObstaclePlanner>(); // 현재 분기 장애물 계획기 조회

            if (obstaclePlanner != null) // 분기 장애물 계획기 존재 확인
            { // 분기 장애물 계획기 처리
                obstaclePlanner.ClearPlacedObstacles(this); // 생성된 장애물과 계획 보고서 먼저 제거
            } // 분기 장애물 계획기 처리 종료

            generatedModules.Clear(); // 생성 모듈 목록 초기화
            graphNodes.Clear(); // 그래프 노드 목록 초기화
            graphEdges.Clear(); // 그래프 간선 목록 초기화
            graphNodeIndices.Clear(); // 그래프 노드 번호 사전 초기화
            usedConnections.Clear(); // 사용 연결 지점 집합 초기화
            generationSignature = string.Empty; // 생성 결과 서명 초기화
            lastValidationReport = MapGenerationValidationReport.CreateNotRun(); // 생성 결과 검사 상태 초기화
            lastPlayableRouteReport = MapPlayableRouteReport.CreateNotRun(); // 플레이 가능 경로 검사 상태 초기화
            lastObstaclePlanReport = MapObstaclePlanReport.CreateNotRun(); // 분기 장애물 계획 상태 초기화
            lastGenerationSucceeded = false; // 최근 생성 성공 상태 초기화
            effectiveTargetHeight = 0f; // 수직 목표 높이 초기화
            generatedHeight = 0f; // 누적 생성 높이 초기화
            ascendingModuleCount = 0; // 상승 모듈 개수 초기화
            consecutiveFlatModuleCount = 0; // 연속 평지 모듈 개수 초기화
            maximumObservedConsecutiveFlatModules = 0; // 최대 연속 평지 모듈 개수 초기화
            leftBranchHeight = 0f; // 왼쪽 분기 높이 초기화
            rightBranchHeight = 0f; // 오른쪽 분기 높이 초기화
            leftBranchAscendingModuleCount = 0; // 왼쪽 분기 상승 수 초기화
            rightBranchAscendingModuleCount = 0; // 오른쪽 분기 상승 수 초기화
            branchCombinationRetryCount = 0; // 분기 조합 재시도 수 초기화
            sharedPathHeight = 0f; // 공통 경로 높이 초기화
            sharedAscendingModuleCount = 0; // 공통 경로 상승 수 초기화
            sharedConsecutiveFlatModuleCount = 0; // 공통 경로 연속 평지 수 초기화
            leftBranchConsecutiveFlatModuleCount = 0; // 왼쪽 분기 연속 평지 수 초기화
            rightBranchConsecutiveFlatModuleCount = 0; // 오른쪽 분기 연속 평지 수 초기화
            maximumAvailableHeightGain = 0f; // 후보 최대 상승량 초기화

            if (generatedRoot == null) // 생성 루트 누락 확인
            { // 생성 루트 누락 처리
                return; // 자식 제거 생략
            } // 생성 루트 누락 처리

            for (int childIndex = generatedRoot.childCount - 1; childIndex >= 0; childIndex--) // 모든 생성 자식 역순 순회
            { // 생성 자식 제거 처리
                GameObject childObject = generatedRoot.GetChild(childIndex).gameObject; // 현재 생성 자식 조회

                if (Application.isPlaying) // Play Mode 여부 확인
                { // Play Mode 제거 처리
                    childObject.transform.SetParent(null, true); // 삭제 대기 모듈을 현재 생성 루트에서 즉시 분리
                    childObject.SetActive(false); // 제거 대기 오브젝트 비활성화
                    Destroy(childObject); // 프레임 종료 시 오브젝트 제거
                } // Play Mode 제거 처리
                else // Edit Mode 여부 확인
                { // Edit Mode 제거 처리
                    DestroyImmediate(childObject); // 생성 오브젝트 즉시 제거
                } // EditMode 제거 처리
            } // 생성 자식 제거 처리
        } // 생성 맵 제거 처리

        private bool GenerateLinearPath(MapModuleDefinition firstModule, List<MapModuleDefinition> ordinaryPrefabs, System.Random random) // 기존 선형 생성 방식 실행
        { // 기존 선형 생성 처리
            MapModuleDefinition previousModule = firstModule; // 현재 경로 마지막 모듈 저장

            while (generatedModules.Count < settings.ModuleCount) // 목표 모듈 개수까지 반복
            { // 선형 후속 모듈 생성 처리
                if (!TryCreateNextModule(previousModule, ordinaryPrefabs, random, out PlacementResult placement)) // 다음 모듈 배치 시도
                { // 다음 모듈 배치 실패 처리
                    return false; // 선형 생성 실패 반환
                } // 다음 모듈 배치 실패 처리

                RegisterPlacement(previousModule, placement, 0); // 중앙 경로 모듈과 간선 등록
                previousModule = placement.Module; // 현재 경로 마지막 모듈 갱신
            } // 선형 후속 모듈 생성 처리

            return true; // 선형 생성 성공 반환
        } // 기존 선형 생성 처리

        private bool GenerateVerticalBranchedPath(MapModuleDefinition firstModule, List<MapModuleDefinition> validPrefabs, List<MapModuleDefinition> ordinaryPrefabs, System.Random random) // 높이가 일치하는 수직 분기와 합류 경로 생성
        { // 수직 분기와 합류 경로 생성 처리
            List<MapModuleDefinition> branchPrefabs = FilterPrefabsByKind(validPrefabs, MapModuleKind.Branch); // 분기 모듈 후보 수집
            List<MapModuleDefinition> mergePrefabs = FilterPrefabsByKind(validPrefabs, MapModuleKind.Merge); // 합류 모듈 후보 수집

            if (branchPrefabs.Count == 0 || mergePrefabs.Count == 0) // 분기 또는 합류 후보 누락 확인
            { // 수직 분기 후보 누락 처리
                Debug.LogError("[ProjectJ][Day36] Branch와 Merge 모듈 Prefab이 각각 하나 이상 필요합니다.", this); // 특수 모듈 누락 오류 출력
                return false; // 수직 분기 생성 실패 반환
            } // 수직 분기 후보 누락 처리 종료

            if (!TryCreateNextModule(firstModule, branchPrefabs, random, out PlacementResult branchPlacement)) // 시작 모듈 뒤 분기 모듈 배치 시도
            { // 분기 모듈 배치 실패 처리
                Debug.LogWarning("[ProjectJ][Day36] 수직 분기 모듈을 배치하지 못했습니다.", this); // 분기 배치 실패 경고 출력
                return false; // 수직 분기 생성 실패 반환
            } // 분기 모듈 배치 실패 처리 종료

            RegisterPlacement(firstModule, branchPlacement, 0); // 분기 모듈과 시작 간선 등록
            MapModuleDefinition branchModule = branchPlacement.Module; // 생성된 분기 모듈 저장
            List<MapModuleConnectionPoint> branchExits = GetAvailableConnections(branchModule, MapConnectionRole.Exit); // 분기 모듈 사용 가능 출구 수집
            SortConnectionsByWorldX(branchExits); // 좌우 순서로 분기 출구 정렬

            if (branchExits.Count < 2) // 분기 출구 수량 확인
            { // 분기 출구 부족 처리
                Debug.LogWarning("[ProjectJ][Day36] 수직 분기 모듈에 사용 가능한 출구가 두 개 미만입니다.", branchModule); // 분기 출구 부족 경고 출력
                return false; // 수직 분기 생성 실패 반환
            } // 분기 출구 부족 처리 종료

            int pairCount = CalculateBranchPairCount(); // 실제 병렬 경로 단계 수 계산
            int fixedStructureCount = 3 + pairCount * 2; // 시작·분기·병렬 쌍·합류 구조 수 계산
            int sharedTailCount = Mathf.Max(0, settings.ModuleCount - fixedStructureCount); // 합류 뒤 공통 모듈 수 계산
            GenerationSnapshot branchSnapshot = CaptureGenerationSnapshot(); // 분기 직후 되돌리기 상태 저장
            bool branchSucceeded = false; // 수직 분기 완성 여부 초기화
            MapModuleDefinition mergeModule = null; // 성공한 합류 모듈 초기화

            for (int retryIndex = 0; retryIndex < settings.MaximumBranchCombinationRetries; retryIndex++) // 허용된 분기 조합 재시도 순회
            { // 단일 수직 분기 조합 시도 처리
                if (retryIndex > 0) // 둘째 이후 조합 확인
                { // 이전 분기 조합 되돌리기 처리
                    RollbackToSnapshot(branchSnapshot); // 분기 직후 상태로 복원
                } // 이전 분기 조합 되돌리기 처리 종료

                branchCombinationRetryCount = retryIndex; // 현재 분기 조합 재시도 수 저장
                InitializeBranchProgress(); // 좌우 분기 진행 상태 초기화
                MapModuleDefinition leftPrevious = branchModule; // 왼쪽 경로 이전 모듈 초기화
                MapModuleDefinition rightPrevious = branchModule; // 오른쪽 경로 이전 모듈 초기화
                MapModuleConnectionPoint forcedLeftExit = branchExits[0]; // 왼쪽 고정 분기 출구 저장
                MapModuleConnectionPoint forcedRightExit = branchExits[branchExits.Count - 1]; // 오른쪽 고정 분기 출구 저장
                bool pairSequenceSucceeded = true; // 병렬 단계 완성 여부 초기화

                for (int pairIndex = 0; pairIndex < pairCount; pairIndex++) // 모든 수직 병렬 단계 순회
                { // 단일 수직 병렬 단계 처리
                    int remainingBranchSlots = pairCount - pairIndex - 1; // 현재 후보 뒤 남는 분기 단계 수 계산

                    if (!TryCreateVerticalParallelPair(leftPrevious, rightPrevious, forcedLeftExit, forcedRightExit, ordinaryPrefabs, remainingBranchSlots, sharedTailCount, random, out PlacementResult leftPlacement, out PlacementResult rightPlacement)) // 높이가 맞는 좌우 후보 쌍 배치 시도
                    { // 수직 병렬 단계 배치 실패 처리
                        pairSequenceSucceeded = false; // 병렬 단계 실패 상태 저장
                        break; // 현재 분기 조합 중단
                    } // 수직 병렬 단계 배치 실패 처리 종료

                    RegisterPlacement(leftPrevious, leftPlacement, -1); // 왼쪽 분기 모듈과 간선 등록
                    RegisterPlacement(rightPrevious, rightPlacement, 1); // 오른쪽 분기 모듈과 간선 등록
                    leftPrevious = leftPlacement.Module; // 왼쪽 경로 마지막 모듈 갱신
                    rightPrevious = rightPlacement.Module; // 오른쪽 경로 마지막 모듈 갱신
                    forcedLeftExit = null; // 다음 왼쪽 출구 자동 선택 전환
                    forcedRightExit = null; // 다음 오른쪽 출구 자동 선택 전환
                } // 단일 수직 병렬 단계 처리 종료

                if (!pairSequenceSucceeded) // 병렬 단계 실패 확인
                { // 병렬 단계 실패 처리
                    continue; // 다음 분기 조합 시도
                } // 병렬 단계 실패 처리 종료

                if (!MapVerticalBranchGenerationRules.TryValidateMerge(leftBranchHeight, rightBranchHeight, leftBranchAscendingModuleCount, rightBranchAscendingModuleCount, settings.MinimumAscendingModulesPerBranch, settings.BranchMergeHeightTolerance, out string mergeReason)) // 좌우 누적 높이와 상승 수 검사
                { // 수직 합류 기준 미달 처리
                    Debug.LogWarning($"[ProjectJ][Day36] 수직 분기 조합 재시도: {mergeReason}", this); // 합류 기준 미달 원인 출력
                    continue; // 다음 분기 조합 시도
                } // 수직 합류 기준 미달 처리 종료

                int mergeConsecutiveFlatModules = Mathf.Max(leftBranchConsecutiveFlatModuleCount, rightBranchConsecutiveFlatModuleCount) + 1; // 평면 합류 모듈까지 이어지는 연속 평지 수 계산

                if (mergeConsecutiveFlatModules > settings.MaximumConsecutiveFlatModules) // 합류 시 연속 평지 제한 초과 확인
                { // 합류 연속 평지 제한 초과 처리
                    continue; // 다음 분기 조합 시도
                } // 합류 연속 평지 제한 초과 처리 종료

                if (!TryCreateMergeModule(leftPrevious, rightPrevious, mergePrefabs, random, out mergeModule, out PlacementResult leftMergePlacement, out PlacementResult rightMergePlacement)) // 실제 XYZ 합류 모듈 배치 시도
                { // 실제 합류 배치 실패 처리
                    continue; // 다음 분기 조합 시도
                } // 실제 합류 배치 실패 처리 종료

                PrepareSharedProgressForMerge(); // 좌우 경로의 연속 평지 상태를 공통 경로로 연결
                RegisterMergePlacement(leftPrevious, rightPrevious, mergeModule, leftMergePlacement, rightMergePlacement); // 합류 모듈과 두 간선 등록
                branchSucceeded = true; // 수직 분기 완성 상태 저장
                break; // 분기 조합 재시도 종료
            } // 단일 수직 분기 조합 시도 처리 종료

            if (!branchSucceeded || mergeModule == null) // 모든 수직 분기 조합 실패 확인
            { // 모든 수직 분기 조합 실패 처리
                RollbackToSnapshot(branchSnapshot); // 분기 직후 상태로 복원
                Debug.LogWarning("[ProjectJ][Day36] 높이와 XYZ 위치가 모두 맞는 수직 분기·합류 조합을 찾지 못했습니다.", this); // 수직 분기 조합 실패 경고 출력
                return false; // 수직 분기 생성 실패 반환
            } // 모든 수직 분기 조합 실패 처리 종료

            MapModuleDefinition previousModule = mergeModule; // 합류 뒤 공통 경로 마지막 모듈 저장

            while (generatedModules.Count < settings.ModuleCount) // 남은 목표 개수까지 반복
            { // 합류 뒤 공통 경로 생성 처리
                if (!TryCreateNextModule(previousModule, ordinaryPrefabs, random, out PlacementResult placement)) // 다음 공통 모듈 배치 시도
                { // 다음 공통 모듈 배치 실패 처리
                    return false; // 수직 분기 생성 실패 반환
                } // 다음 공통 모듈 배치 실패 처리 종료

                RegisterPlacement(previousModule, placement, 0); // 공통 후속 모듈과 간선 등록
                previousModule = placement.Module; // 공통 경로 마지막 모듈 갱신
            } // 합류 뒤 공통 경로 생성 처리 종료

            return true; // 수직 분기 생성 성공 반환
        } // 수직 분기와 합류 경로 생성 처리 종료

        private bool GenerateBranchedPath(MapModuleDefinition firstModule, List<MapModuleDefinition> validPrefabs, List<MapModuleDefinition> ordinaryPrefabs, System.Random random) // 분기와 합류 경로 생성
        { // 분기와 합류 경로 생성 처리
            List<MapModuleDefinition> branchPrefabs = FilterPrefabsByKind(validPrefabs, MapModuleKind.Branch); // 분기 모듈 후보 수집
            List<MapModuleDefinition> mergePrefabs = FilterPrefabsByKind(validPrefabs, MapModuleKind.Merge); // 합류 모듈 후보 수집

            if (branchPrefabs.Count == 0 || mergePrefabs.Count == 0) // 분기 또는 합류 모듈 누락 확인
            { // 특수 모듈 누락 처리
                Debug.LogError("[ProjectJ][Day32] Branch와 Merge 모듈 Prefab이 각각 하나 이상 필요합니다.", this); // 특수 모듈 누락 오류 출력
                return false; // 분기 생성 실패 반환
            } // 특수 모듈 누락 처리

            if (!TryCreateNextModule(firstModule, branchPrefabs, random, out PlacementResult branchPlacement)) // 시작 모듈 뒤 분기 모듈 배치 시도
            { // 분기 모듈 배치 실패 처리
                Debug.LogWarning("[ProjectJ][Day32] 분기 모듈을 배치하지 못했습니다.", this); // 분기 배치 실패 경고 출력
                return false; // 분기 생성 실패 반환
            } // 분기 모듈 배치 실패 처리

            RegisterPlacement(firstModule, branchPlacement, 0); // 분기 모듈과 시작 간선 등록
            MapModuleDefinition branchModule = branchPlacement.Module; // 생성된 분기 모듈 저장
            List<MapModuleConnectionPoint> branchExits = GetAvailableConnections(branchModule, MapConnectionRole.Exit); // 분기 모듈 사용 가능 출구 수집
            SortConnectionsByWorldX(branchExits); // 좌우 순서로 분기 출구 정렬

            if (branchExits.Count < 2) // 분기 출구 수량 확인
            { // 분기 출구 부족 처리
                Debug.LogWarning("[ProjectJ][Day32] 분기 모듈에 사용 가능한 출구가 두 개 미만입니다.", branchModule); // 분기 출구 부족 경고 출력
                return false; // 분기 생성 실패 반환
            } // 분기 출구 부족 처리

            MapModuleDefinition leftPrevious = branchModule; // 왼쪽 경로 이전 모듈 초기화
            MapModuleDefinition rightPrevious = branchModule; // 오른쪽 경로 이전 모듈 초기화
            MapModuleConnectionPoint forcedLeftExit = branchExits[0]; // 왼쪽 분기 출구 저장
            MapModuleConnectionPoint forcedRightExit = branchExits[branchExits.Count - 1]; // 오른쪽 분기 출구 저장
            int maximumPairsForTarget = Mathf.Max(1, (settings.ModuleCount - 3) / 2); // 목표 개수 안의 최대 병렬 쌍 계산
            int pairCount = Mathf.Min(settings.BranchPairCount, maximumPairsForTarget); // 실제 병렬 경로 쌍 개수 계산

            for (int pairIndex = 0; pairIndex < pairCount; pairIndex++) // 모든 병렬 경로 단계 순회
            { // 병렬 경로 모듈 쌍 생성 처리
                if (!TryCreateParallelPair(leftPrevious, rightPrevious, forcedLeftExit, forcedRightExit, ordinaryPrefabs, random, out PlacementResult leftPlacement, out PlacementResult rightPlacement)) // 좌우 모듈 쌍 배치 시도
                { // 병렬 경로 배치 실패 처리
                    Debug.LogWarning($"[ProjectJ][Day32] {pairIndex + 1}번째 병렬 경로 쌍을 배치하지 못했습니다.", this); // 병렬 경로 배치 실패 경고 출력
                    return false; // 분기 생성 실패 반환
                } // 병렬 경로 배치 실패 처리

                RegisterPlacement(leftPrevious, leftPlacement, -1); // 왼쪽 경로 모듈과 간선 등록
                RegisterPlacement(rightPrevious, rightPlacement, 1); // 오른쪽 경로 모듈과 간선 등록
                leftPrevious = leftPlacement.Module; // 왼쪽 경로 마지막 모듈 갱신
                rightPrevious = rightPlacement.Module; // 오른쪽 경로 마지막 모듈 갱신
                forcedLeftExit = null; // 다음 단계 왼쪽 출구 자동 선택 전환
                forcedRightExit = null; // 다음 단계 오른쪽 출구 자동 선택 전환
            } // 병렬 경로 모듈 쌍 생성 처리

            if (!TryCreateMergeModule(leftPrevious, rightPrevious, mergePrefabs, random, out MapModuleDefinition mergeModule, out PlacementResult leftMergePlacement, out PlacementResult rightMergePlacement)) // 두 경로 합류 모듈 배치 시도
            { // 합류 모듈 배치 실패 처리
                Debug.LogWarning("[ProjectJ][Day32] 두 병렬 경로를 합류하지 못했습니다.", this); // 합류 실패 경고 출력
                return false; // 분기 생성 실패 반환
            } // 합류 모듈 배치 실패 처리

            RegisterMergePlacement(leftPrevious, rightPrevious, mergeModule, leftMergePlacement, rightMergePlacement); // 합류 모듈과 두 간선 등록
            MapModuleDefinition previousModule = mergeModule; // 합류 뒤 중앙 경로 마지막 모듈 저장

            while (generatedModules.Count < settings.ModuleCount) // 남은 목표 개수까지 반복
            { // 합류 뒤 후속 경로 생성 처리
                if (!TryCreateNextModule(previousModule, ordinaryPrefabs, random, out PlacementResult placement)) // 후속 중앙 모듈 배치 시도
                { // 후속 중앙 모듈 배치 실패 처리
                    return false; // 분기 생성 실패 반환
                } // 후속 중앙 모듈 배치 실패 처리

                RegisterPlacement(previousModule, placement, 0); // 중앙 후속 모듈과 간선 등록
                previousModule = placement.Module; // 중앙 경로 마지막 모듈 갱신
            } // 합류 뒤 후속 경로 생성 처리

            return true; // 분기 생성 성공 반환
        } // 분기와 합류 경로 생성 처리

        private List<MapModuleDefinition> CollectValidPrefabs() // 설정에서 유효한 후보 Prefab 수집
        { // 유효 후보 Prefab 수집 처리
            List<MapModuleDefinition> validPrefabs = new List<MapModuleDefinition>(); // 유효 후보 목록 생성
            MapModuleDefinition[] configuredPrefabs = settings.ModulePrefabs; // 설정된 후보 목록 조회

            if (configuredPrefabs == null) // 후보 목록 누락 확인
            { // 후보 목록 누락 처리
                return validPrefabs; // 빈 후보 목록 반환
            } // 후보 목록 누락 처리

            for (int prefabIndex = 0; prefabIndex < configuredPrefabs.Length; prefabIndex++) // 모든 후보 Prefab 순회
            { // 후보 Prefab 검사 처리
                MapModuleDefinition prefab = configuredPrefabs[prefabIndex]; // 현재 후보 Prefab 조회

                if (prefab == null) // 빈 후보 항목 확인
                { // 빈 후보 항목 처리
                    continue; // 현재 후보 제외
                } // 빈 후보 항목 처리

                if (!prefab.TryValidate(out string reason)) // 모듈 데이터 유효성 검사
                { // 잘못된 모듈 처리
                    Debug.LogWarning($"[ProjectJ][Day32] {prefab.name} 제외: {reason}", prefab); // 후보 제외 사유 출력
                    continue; // 현재 후보 제외
                } // 잘못된 모듈 처리

                MapVerticalModuleData verticalData = prefab.GetComponent<MapVerticalModuleData>(); // 후보 수직 데이터 조회

                if (verticalData != null && !verticalData.TryValidate(out string verticalReason)) // 수직 데이터 유효성 확인
                { // 잘못된 수직 모듈 처리
                    Debug.LogWarning($"[ProjectJ][Day35] {prefab.name} 제외: {verticalReason}", prefab); // 수직 후보 제외 사유 출력
                    continue; // 현재 후보 제외
                } // 잘못된 수직 모듈 처리 종료

                if (MapGenerationRules.GetAllowedQuarterTurns(prefab.AllowedRotations).Length == 0) // 허용 회전 누락 확인
                { // 허용 회전 누락 처리
                    continue; // 현재 후보 제외
                } // 허용 회전 누락 처리

                validPrefabs.Add(prefab); // 유효 후보 목록 등록
            } // 후보 Prefab 검사 처리

            return validPrefabs; // 유효 후보 목록 반환
        } // 유효 후보 Prefab 수집 처리

        private List<MapModuleDefinition> FilterOrdinaryPrefabs(List<MapModuleDefinition> prefabs) // 분기와 합류를 제외한 일반 후보 수집
        { // 일반 후보 수집 처리
            List<MapModuleDefinition> results = new List<MapModuleDefinition>(); // 일반 후보 결과 목록 생성

            for (int prefabIndex = 0; prefabIndex < prefabs.Count; prefabIndex++) // 모든 유효 후보 순회
            { // 일반 후보 종류 검사
                MapModuleDefinition prefab = prefabs[prefabIndex]; // 현재 후보 조회

                if (prefab.ModuleKind != MapModuleKind.Branch && prefab.ModuleKind != MapModuleKind.Merge) // 일반 경로 종류 확인
                { // 일반 경로 종류 처리
                    results.Add(prefab); // 일반 후보 목록 등록
                } // 일반 경로 종류 처리
            } // 일반 후보 종류 검사

            return results; // 일반 후보 결과 반환
        } // 일반 후보 수집 처리

        private List<MapModuleDefinition> FilterVerticalFeasiblePrefabs(List<MapModuleDefinition> prefabs) // 남은 슬롯으로 수직 목표를 달성할 후보 수집
        { // 수직 목표 달성 가능 후보 수집 처리
            List<MapModuleDefinition> results = new List<MapModuleDefinition>(); // 수직 목표 가능 후보 목록 생성
            int remainingSlotsAfterCandidate = Mathf.Max(0, settings.ModuleCount - generatedModules.Count - 1); // 현재 후보 배치 뒤 남는 모듈 슬롯 계산

            for (int prefabIndex = 0; prefabIndex < prefabs.Count; prefabIndex++) // 모든 일반 후보 순회
            { // 수직 목표 가능성 검사 처리
                MapModuleDefinition prefab = prefabs[prefabIndex]; // 현재 후보 Prefab 조회
                float candidateHeightGain = MapVerticalGenerationRules.GetHeightGain(prefab); // 현재 후보 상승량 조회
                bool isFeasible = MapVerticalGenerationRules.IsCandidateFeasible(generatedHeight, ascendingModuleCount, consecutiveFlatModuleCount, candidateHeightGain, remainingSlotsAfterCandidate, effectiveTargetHeight, settings.MinimumAscendingModules, settings.MaximumConsecutiveFlatModules, maximumAvailableHeightGain, settings.AllowDescendingModules); // 후보 선택 후 최종 목표 달성 가능성 계산

                if (isFeasible) // 수직 목표 달성 가능 후보 확인
                { // 수직 목표 달성 가능 후보 처리
                    results.Add(prefab); // 선택 가능 후보 목록 등록
                } // 수직 목표 달성 가능 후보 처리 종료
            } // 수직 목표 가능성 검사 처리 종료

            return results; // 수직 목표 가능 후보 목록 반환
        } // 수직 목표 달성 가능 후보 수집 처리 종료

        private List<MapModuleDefinition> FilterPrefabsByKind(List<MapModuleDefinition> prefabs, MapModuleKind kind) // 지정 종류 후보 수집
        { // 지정 종류 후보 수집 처리
            List<MapModuleDefinition> results = new List<MapModuleDefinition>(); // 지정 종류 결과 목록 생성

            for (int prefabIndex = 0; prefabIndex < prefabs.Count; prefabIndex++) // 모든 유효 후보 순회
            { // 지정 종류 검사 처리
                if (prefabs[prefabIndex].ModuleKind == kind) // 지정 종류 일치 확인
                { // 지정 종류 일치 처리
                    results.Add(prefabs[prefabIndex]); // 지정 종류 후보 등록
                } // 지정 종류 일치 처리
            } // 지정 종류 검사 처리

            return results; // 지정 종류 결과 반환
        } // 지정 종류 후보 수집 처리

        private MapModuleDefinition CreateFirstModule(List<MapModuleDefinition> validPrefabs, System.Random random) // 첫 번째 모듈 생성
        { // 첫 번째 모듈 생성 처리
            List<MapModuleDefinition> selectablePrefabs = settings.UseVerticalGeneration ? FilterVerticalFeasiblePrefabs(validPrefabs) : validPrefabs; // 현재 목표를 만족할 첫 후보 목록 계산

            if (selectablePrefabs.Count == 0) // 첫 배치 가능 후보 없음 확인
            { // 첫 배치 가능 후보 없음 처리
                return null; // 첫 모듈 생성 실패 반환
            } // 첫 배치 가능 후보 없음 처리 종료

            MapModuleDefinition selectedPrefab = selectablePrefabs[random.Next(selectablePrefabs.Count)]; // 첫 후보 Prefab 무작위 선택
            int[] allowedQuarterTurns = MapGenerationRules.GetAllowedQuarterTurns(selectedPrefab.AllowedRotations); // 허용 회전 목록 조회
            int selectedQuarterTurns = MapGenerationRules.IsRotationAllowed(selectedPrefab.AllowedRotations, settings.StartingQuarterTurns) ? settings.StartingQuarterTurns : allowedQuarterTurns[0]; // 설정 회전 또는 첫 허용 회전 선택
            MapModuleDefinition instance = Instantiate(selectedPrefab, generatedRoot); // 첫 모듈 인스턴스 생성
            instance.name = $"{selectedPrefab.ModuleId}_00"; // 첫 모듈 이름 적용
            instance.transform.localPosition = Vector3.zero; // 생성 루트 원점 배치
            instance.transform.localRotation = MapGenerationRules.QuarterTurnRotation(selectedQuarterTurns); // 허용 직각 회전 적용
            return instance; // 생성된 첫 모듈 반환
        } // 첫 번째 모듈 생성 처리

        private MapModuleDefinition CreateFirstVerticalBranchModule(List<MapModuleDefinition> validPrefabs, System.Random random) // 수직 분기 경로의 첫 모듈 생성
        { // 수직 분기 첫 모듈 생성 처리
            List<MapModuleDefinition> selectablePrefabs = new List<MapModuleDefinition>(); // 수직 분기 첫 후보 목록 생성
            int routeSlotCount = MapVerticalBranchGenerationRules.CalculateRouteOrdinarySlotCount(settings.ModuleCount, CalculateBranchPairCount()); // 단일 경로 일반 슬롯 수 계산
            int remainingRouteSlots = Mathf.Max(0, routeSlotCount - 1); // 첫 후보 뒤 남는 단일 경로 슬롯 수 계산

            for (int prefabIndex = 0; prefabIndex < validPrefabs.Count; prefabIndex++) // 모든 일반 후보 순회
            { // 첫 후보 목표 가능성 검사 처리
                MapModuleDefinition prefab = validPrefabs[prefabIndex]; // 현재 첫 후보 조회
                float heightGain = MapVerticalGenerationRules.GetHeightGain(prefab); // 현재 첫 후보 상승량 조회
                bool isFeasible = MapVerticalGenerationRules.IsCandidateFeasible(0f, 0, 0, heightGain, remainingRouteSlots, effectiveTargetHeight, settings.MinimumAscendingModules, settings.MaximumConsecutiveFlatModules, maximumAvailableHeightGain, settings.AllowDescendingModules); // 첫 후보 뒤 최종 경로 목표 달성 가능성 계산

                if (isFeasible) // 첫 후보 선택 가능 확인
                { // 첫 후보 선택 가능 처리
                    selectablePrefabs.Add(prefab); // 수직 분기 첫 후보 목록 등록
                } // 첫 후보 선택 가능 처리 종료
            } // 첫 후보 목표 가능성 검사 처리 종료

            if (selectablePrefabs.Count == 0) // 수직 분기 첫 후보 없음 확인
            { // 수직 분기 첫 후보 없음 처리
                return null; // 첫 모듈 생성 실패 반환
            } // 수직 분기 첫 후보 없음 처리 종료

            MapModuleDefinition selectedPrefab = selectablePrefabs[random.Next(selectablePrefabs.Count)]; // 첫 후보 Prefab 시드 기반 선택
            int[] allowedQuarterTurns = MapGenerationRules.GetAllowedQuarterTurns(selectedPrefab.AllowedRotations); // 허용 회전 목록 조회
            int selectedQuarterTurns = MapGenerationRules.IsRotationAllowed(selectedPrefab.AllowedRotations, settings.StartingQuarterTurns) ? settings.StartingQuarterTurns : allowedQuarterTurns[0]; // 설정 회전 또는 첫 허용 회전 선택
            MapModuleDefinition instance = Instantiate(selectedPrefab, generatedRoot); // 첫 모듈 인스턴스 생성
            instance.name = $"{selectedPrefab.ModuleId}_00"; // 첫 모듈 이름 적용
            instance.transform.localPosition = Vector3.zero; // 생성 루트 원점 배치
            instance.transform.localRotation = MapGenerationRules.QuarterTurnRotation(selectedQuarterTurns); // 허용 직각 회전 적용
            return instance; // 생성된 첫 수직 분기 모듈 반환
        } // 수직 분기 첫 모듈 생성 처리 종료

        private int CalculateBranchPairCount() // 목표 모듈 수 안의 실제 병렬 단계 수 계산
        { // 실제 병렬 단계 수 계산 처리
            int maximumPairsForTarget = Mathf.Max(1, (settings.ModuleCount - 3) / 2); // 목표 개수 안의 최대 병렬 단계 수 계산
            return Mathf.Min(settings.BranchPairCount, maximumPairsForTarget); // 설정과 목표를 반영한 병렬 단계 수 반환
        } // 실제 병렬 단계 수 계산 처리 종료

        private bool TryCreateNextModule(MapModuleDefinition previousModule, List<MapModuleDefinition> candidatePrefabs, System.Random random, out PlacementResult result) // 이전 모듈의 모든 출구 뒤에 다음 모듈 생성
        { // 다음 모듈 생성 처리
            result = new PlacementResult(); // 생성 결과 초기화
            List<MapModuleDefinition> selectablePrefabs = settings.UseVerticalGeneration ? FilterVerticalFeasiblePrefabs(candidatePrefabs) : candidatePrefabs; // 현재 수직 목표를 만족할 후보 목록 계산

            if (selectablePrefabs.Count == 0) // 배치 가능 후보 없음 확인
            { // 배치 가능 후보 없음 처리
                return false; // 다음 모듈 생성 실패 반환
            } // 배치 가능 후보 없음 처리 종료

            List<MapModuleConnectionPoint> exits = GetAvailableConnections(previousModule, MapConnectionRole.Exit); // 이전 모듈 사용 가능 출구 수집
            ShuffleList(exits, random); // 시드 기반 출구 순서 섞기
            List<PlacementOption> placementOptions = BuildPlacementOptions(selectablePrefabs); // 목표 달성 가능 Prefab과 회전 조합 생성
            ShuffleList(placementOptions, random); // 시드 기반 후보 순서 섞기
            int attemptCount = 0; // 실제 연결 조합 시도 횟수 초기화

            for (int exitIndex = 0; exitIndex < exits.Count; exitIndex++) // 모든 이전 출구 순회
            { // 이전 출구 조합 처리
                MapModuleConnectionPoint sourceExit = exits[exitIndex]; // 현재 이전 출구 조회

                for (int optionIndex = 0; optionIndex < placementOptions.Count; optionIndex++) // 모든 Prefab 회전 후보 순회
                { // Prefab 회전 후보 처리
                    PlacementOption option = placementOptions[optionIndex]; // 현재 배치 후보 조회
                    MapModuleDefinition candidate = CreateCandidate(option, generatedModules.Count); // 임시 후보 모듈 생성
                    List<MapModuleConnectionPoint> entrances = GetAvailableConnections(candidate, MapConnectionRole.Entrance); // 후보 사용 가능 입구 수집
                    ShuffleList(entrances, random); // 시드 기반 입구 순서 섞기

                    for (int entranceIndex = 0; entranceIndex < entrances.Count; entranceIndex++) // 모든 후보 입구 순회
                    { // 출구와 입구 조합 처리
                        attemptCount++; // 실제 연결 조합 시도 횟수 증가

                        if (attemptCount > settings.MaximumPlacementAttempts) // 최대 시도 횟수 초과 확인
                        { // 최대 시도 횟수 초과 처리
                            DestroyCandidate(candidate); // 임시 후보 제거
                            return false; // 다음 모듈 생성 실패 반환
                        } // 최대 시도 횟수 초과 처리

                        MapModuleConnectionPoint entrance = entrances[entranceIndex]; // 현재 후보 입구 조회
                        ResetCandidateTransform(candidate, option.QuarterTurns); // 후보 위치와 회전 초기화

                        if (!AreConnectionsCompatible(sourceExit, entrance)) // 방향과 크기 호환성 확인
                        { // 호환되지 않는 연결 처리
                            continue; // 다음 입구 조합 시도
                        } // 호환되지 않는 연결 처리

                        AlignCandidate(sourceExit, entrance, candidate); // 후보 입구를 이전 출구에 정렬

                        if (OverlapsPlacedModule(candidate)) // 기존 모듈 실제 겹침 확인
                        { // 기존 모듈 겹침 처리
                            continue; // 다음 입구 조합 시도
                        } // 기존 모듈 겹침 처리

                        usedConnections.Add(sourceExit); // 이전 출구 사용 완료 등록
                        usedConnections.Add(entrance); // 후보 입구 사용 완료 등록
                        result.Module = candidate; // 생성 성공 모듈 저장
                        result.SourceExit = sourceExit; // 사용 이전 출구 저장
                        result.Entrance = entrance; // 사용 후보 입구 저장
                        return true; // 다음 모듈 생성 성공 반환
                    } // 출구와 입구 조합 처리

                    DestroyCandidate(candidate); // 실패한 현재 후보 제거
                } // Prefab 회전 후보 처리
            } // 이전 출구 조합 처리

            return false; // 모든 연결 조합 배치 실패 반환
        } // 다음 모듈 생성 처리

        private bool TryCreateVerticalParallelPair(MapModuleDefinition leftPrevious, MapModuleDefinition rightPrevious, MapModuleConnectionPoint forcedLeftExit, MapModuleConnectionPoint forcedRightExit, List<MapModuleDefinition> candidatePrefabs, int remainingBranchSlots, int remainingSharedSlots, System.Random random, out PlacementResult leftResult, out PlacementResult rightResult) // 누적 높이가 맞는 좌우 수직 모듈 쌍 배치
        { // 수직 병렬 모듈 쌍 배치 처리
            leftResult = new PlacementResult(); // 왼쪽 배치 결과 초기화
            rightResult = new PlacementResult(); // 오른쪽 배치 결과 초기화
            List<MapModuleConnectionPoint> leftExits = forcedLeftExit != null ? new List<MapModuleConnectionPoint> { forcedLeftExit } : GetAvailableConnections(leftPrevious, MapConnectionRole.Exit); // 왼쪽 사용 가능 출구 준비
            List<MapModuleConnectionPoint> rightExits = forcedRightExit != null ? new List<MapModuleConnectionPoint> { forcedRightExit } : GetAvailableConnections(rightPrevious, MapConnectionRole.Exit); // 오른쪽 사용 가능 출구 준비
            List<VerticalPairOption> pairOptions = BuildVerticalPairOptions(candidatePrefabs, remainingBranchSlots, remainingSharedSlots); // 목표와 합류가 가능한 좌우 후보 조합 생성
            ShuffleList(leftExits, random); // 왼쪽 출구 순서 섞기
            ShuffleList(rightExits, random); // 오른쪽 출구 순서 섞기
            ShuffleList(pairOptions, random); // 수직 후보 조합 순서 섞기
            int attemptCount = 0; // 실제 수직 병렬 조합 시도 횟수 초기화

            for (int leftExitIndex = 0; leftExitIndex < leftExits.Count; leftExitIndex++) // 모든 왼쪽 출구 순회
            { // 왼쪽 수직 출구 조합 처리
                for (int rightExitIndex = 0; rightExitIndex < rightExits.Count; rightExitIndex++) // 모든 오른쪽 출구 순회
                { // 오른쪽 수직 출구 조합 처리
                    for (int pairOptionIndex = 0; pairOptionIndex < pairOptions.Count; pairOptionIndex++) // 모든 수직 후보 쌍 순회
                    { // 수직 후보 쌍 처리
                        VerticalPairOption pairOption = pairOptions[pairOptionIndex]; // 현재 좌우 수직 후보 조회
                        MapModuleDefinition leftCandidate = CreateCandidate(pairOption.LeftOption, generatedModules.Count); // 왼쪽 임시 후보 생성
                        MapModuleDefinition rightCandidate = CreateCandidate(pairOption.RightOption, generatedModules.Count + 1); // 오른쪽 임시 후보 생성
                        List<MapModuleConnectionPoint> leftEntrances = GetAvailableConnections(leftCandidate, MapConnectionRole.Entrance); // 왼쪽 후보 입구 수집
                        List<MapModuleConnectionPoint> rightEntrances = GetAvailableConnections(rightCandidate, MapConnectionRole.Entrance); // 오른쪽 후보 입구 수집
                        ShuffleList(leftEntrances, random); // 왼쪽 입구 순서 섞기
                        ShuffleList(rightEntrances, random); // 오른쪽 입구 순서 섞기

                        for (int leftEntranceIndex = 0; leftEntranceIndex < leftEntrances.Count; leftEntranceIndex++) // 모든 왼쪽 입구 순회
                        { // 왼쪽 수직 입구 조합 처리
                            for (int rightEntranceIndex = 0; rightEntranceIndex < rightEntrances.Count; rightEntranceIndex++) // 모든 오른쪽 입구 순회
                            { // 오른쪽 수직 입구 조합 처리
                                attemptCount++; // 실제 수직 병렬 조합 시도 횟수 증가

                                if (attemptCount > settings.MaximumPlacementAttempts) // 최대 시도 횟수 초과 확인
                                { // 수직 병렬 최대 시도 초과 처리
                                    DestroyCandidate(leftCandidate); // 왼쪽 임시 후보 제거
                                    DestroyCandidate(rightCandidate); // 오른쪽 임시 후보 제거
                                    return false; // 수직 병렬 배치 실패 반환
                                } // 수직 병렬 최대 시도 초과 처리 종료

                                MapModuleConnectionPoint leftExit = leftExits[leftExitIndex]; // 현재 왼쪽 출구 조회
                                MapModuleConnectionPoint rightExit = rightExits[rightExitIndex]; // 현재 오른쪽 출구 조회
                                MapModuleConnectionPoint leftEntrance = leftEntrances[leftEntranceIndex]; // 현재 왼쪽 입구 조회
                                MapModuleConnectionPoint rightEntrance = rightEntrances[rightEntranceIndex]; // 현재 오른쪽 입구 조회
                                ResetCandidateTransform(leftCandidate, pairOption.LeftOption.QuarterTurns); // 왼쪽 후보 위치와 회전 초기화
                                ResetCandidateTransform(rightCandidate, pairOption.RightOption.QuarterTurns); // 오른쪽 후보 위치와 회전 초기화

                                if (!AreConnectionsCompatible(leftExit, leftEntrance) || !AreConnectionsCompatible(rightExit, rightEntrance)) // 좌우 연결 호환성 확인
                                { // 좌우 수직 연결 비호환 처리
                                    continue; // 다음 입구 조합 시도
                                } // 좌우 수직 연결 비호환 처리 종료

                                AlignCandidate(leftExit, leftEntrance, leftCandidate); // 왼쪽 후보 XYZ 정렬
                                AlignCandidate(rightExit, rightEntrance, rightCandidate); // 오른쪽 후보 XYZ 정렬
                                bool pairOverlaps = MapGenerationRules.BoundsHaveBlockingOverlap(leftCandidate.WorldBounds, rightCandidate.WorldBounds, settings.OverlapTolerance); // 좌우 후보 상호 겹침 검사

                                if (pairOverlaps || OverlapsPlacedModule(leftCandidate) || OverlapsPlacedModule(rightCandidate)) // 전체 수직 후보 영역 겹침 확인
                                { // 수직 병렬 후보 겹침 처리
                                    continue; // 다음 입구 조합 시도
                                } // 수직 병렬 후보 겹침 처리 종료

                                usedConnections.Add(leftExit); // 왼쪽 이전 출구 사용 완료 등록
                                usedConnections.Add(rightExit); // 오른쪽 이전 출구 사용 완료 등록
                                usedConnections.Add(leftEntrance); // 왼쪽 후보 입구 사용 완료 등록
                                usedConnections.Add(rightEntrance); // 오른쪽 후보 입구 사용 완료 등록
                                leftResult.Module = leftCandidate; // 왼쪽 생성 모듈 저장
                                leftResult.SourceExit = leftExit; // 왼쪽 사용 출구 저장
                                leftResult.Entrance = leftEntrance; // 왼쪽 사용 입구 저장
                                rightResult.Module = rightCandidate; // 오른쪽 생성 모듈 저장
                                rightResult.SourceExit = rightExit; // 오른쪽 사용 출구 저장
                                rightResult.Entrance = rightEntrance; // 오른쪽 사용 입구 저장
                                return true; // 수직 병렬 배치 성공 반환
                            } // 오른쪽 수직 입구 조합 처리 종료
                        } // 왼쪽 수직 입구 조합 처리 종료

                        DestroyCandidate(leftCandidate); // 실패한 왼쪽 수직 후보 제거
                        DestroyCandidate(rightCandidate); // 실패한 오른쪽 수직 후보 제거
                    } // 수직 후보 쌍 처리 종료
                } // 오른쪽 수직 출구 조합 처리 종료
            } // 왼쪽 수직 출구 조합 처리 종료

            return false; // 모든 수직 병렬 조합 실패 반환
        } // 수직 병렬 모듈 쌍 배치 처리 종료

        private List<VerticalPairOption> BuildVerticalPairOptions(List<MapModuleDefinition> candidatePrefabs, int remainingBranchSlots, int remainingSharedSlots) // 목표 달성 가능한 좌우 수직 후보 조합 생성
        { // 좌우 수직 후보 조합 생성 처리
            List<VerticalPairOption> results = new List<VerticalPairOption>(); // 좌우 수직 후보 결과 목록 생성
            List<PlacementOption> placementOptions = BuildPlacementOptions(candidatePrefabs); // 일반 Prefab과 회전 조합 생성

            for (int leftOptionIndex = 0; leftOptionIndex < placementOptions.Count; leftOptionIndex++) // 모든 왼쪽 후보 순회
            { // 왼쪽 수직 후보 처리
                PlacementOption leftOption = placementOptions[leftOptionIndex]; // 현재 왼쪽 후보 조회
                float leftHeightGain = MapVerticalGenerationRules.GetHeightGain(leftOption.Prefab); // 왼쪽 후보 상승량 조회

                for (int rightOptionIndex = 0; rightOptionIndex < placementOptions.Count; rightOptionIndex++) // 모든 오른쪽 후보 순회
                { // 오른쪽 수직 후보 처리
                    PlacementOption rightOption = placementOptions[rightOptionIndex]; // 현재 오른쪽 후보 조회
                    float rightHeightGain = MapVerticalGenerationRules.GetHeightGain(rightOption.Prefab); // 오른쪽 후보 상승량 조회
                    bool leftFlatLimitExceeded = Mathf.Abs(leftHeightGain) <= MapVerticalGenerationRules.HeightEpsilon && leftBranchConsecutiveFlatModuleCount + 1 > settings.MaximumConsecutiveFlatModules; // 왼쪽 후보의 연속 평지 제한 초과 계산
                    bool rightFlatLimitExceeded = Mathf.Abs(rightHeightGain) <= MapVerticalGenerationRules.HeightEpsilon && rightBranchConsecutiveFlatModuleCount + 1 > settings.MaximumConsecutiveFlatModules; // 오른쪽 후보의 연속 평지 제한 초과 계산

                    if (leftFlatLimitExceeded || rightFlatLimitExceeded) // 좌우 연속 평지 제한 초과 확인
                    { // 좌우 연속 평지 제한 초과 처리
                        continue; // 현재 좌우 조합 제외
                    } // 좌우 연속 평지 제한 초과 처리 종료

                    bool isFeasible = MapVerticalBranchGenerationRules.IsBranchPairFeasible(sharedPathHeight, sharedAscendingModuleCount, leftBranchHeight, leftBranchAscendingModuleCount, rightBranchHeight, rightBranchAscendingModuleCount, leftHeightGain, rightHeightGain, remainingBranchSlots, remainingSharedSlots, effectiveTargetHeight, settings.MinimumAscendingModules, settings.MinimumAscendingModulesPerBranch, maximumAvailableHeightGain, settings.BranchMergeHeightTolerance, settings.AllowDescendingModules); // 좌우 후보 선택 뒤 합류와 최종 목표 달성 가능성 계산

                    if (!isFeasible) // 목표 달성 불가 후보 확인
                    { // 목표 달성 불가 후보 처리
                        continue; // 현재 좌우 조합 제외
                    } // 목표 달성 불가 후보 처리 종료

                    VerticalPairOption pairOption = new VerticalPairOption(); // 새 좌우 수직 후보 조합 생성
                    pairOption.LeftOption = leftOption; // 왼쪽 후보 저장
                    pairOption.RightOption = rightOption; // 오른쪽 후보 저장
                    results.Add(pairOption); // 수직 후보 조합 목록 등록
                } // 오른쪽 수직 후보 처리 종료
            } // 왼쪽 수직 후보 처리 종료

            return results; // 목표 달성 가능한 좌우 후보 조합 반환
        } // 좌우 수직 후보 조합 생성 처리 종료

        private bool TryCreateParallelPair(MapModuleDefinition leftPrevious, MapModuleDefinition rightPrevious, MapModuleConnectionPoint forcedLeftExit, MapModuleConnectionPoint forcedRightExit, List<MapModuleDefinition> candidatePrefabs, System.Random random, out PlacementResult leftResult, out PlacementResult rightResult) // 동일 모듈을 좌우 경로에 나란히 배치
        { // 병렬 모듈 쌍 배치 처리
            leftResult = new PlacementResult(); // 왼쪽 배치 결과 초기화
            rightResult = new PlacementResult(); // 오른쪽 배치 결과 초기화
            List<MapModuleConnectionPoint> leftExits = forcedLeftExit != null ? new List<MapModuleConnectionPoint> { forcedLeftExit } : GetAvailableConnections(leftPrevious, MapConnectionRole.Exit); // 왼쪽 사용 가능 출구 준비
            List<MapModuleConnectionPoint> rightExits = forcedRightExit != null ? new List<MapModuleConnectionPoint> { forcedRightExit } : GetAvailableConnections(rightPrevious, MapConnectionRole.Exit); // 오른쪽 사용 가능 출구 준비
            List<PlacementOption> placementOptions = BuildPlacementOptions(candidatePrefabs); // 동일 Prefab 회전 후보 생성
            ShuffleList(leftExits, random); // 왼쪽 출구 순서 섞기
            ShuffleList(rightExits, random); // 오른쪽 출구 순서 섞기
            ShuffleList(placementOptions, random); // 후보 순서 섞기
            int attemptCount = 0; // 실제 병렬 조합 시도 횟수 초기화

            for (int leftExitIndex = 0; leftExitIndex < leftExits.Count; leftExitIndex++) // 모든 왼쪽 출구 순회
            { // 왼쪽 출구 조합 처리
                for (int rightExitIndex = 0; rightExitIndex < rightExits.Count; rightExitIndex++) // 모든 오른쪽 출구 순회
                { // 오른쪽 출구 조합 처리
                    for (int optionIndex = 0; optionIndex < placementOptions.Count; optionIndex++) // 모든 동일 후보 순회
                    { // 동일 후보 조합 처리
                        PlacementOption option = placementOptions[optionIndex]; // 현재 동일 후보 조회
                        MapModuleDefinition leftCandidate = CreateCandidate(option, generatedModules.Count); // 왼쪽 임시 후보 생성
                        MapModuleDefinition rightCandidate = CreateCandidate(option, generatedModules.Count + 1); // 오른쪽 임시 후보 생성
                        List<MapModuleConnectionPoint> leftEntrances = GetAvailableConnections(leftCandidate, MapConnectionRole.Entrance); // 왼쪽 후보 입구 수집
                        List<MapModuleConnectionPoint> rightEntrances = GetAvailableConnections(rightCandidate, MapConnectionRole.Entrance); // 오른쪽 후보 입구 수집
                        ShuffleList(leftEntrances, random); // 왼쪽 입구 순서 섞기
                        ShuffleList(rightEntrances, random); // 오른쪽 입구 순서 섞기

                        for (int leftEntranceIndex = 0; leftEntranceIndex < leftEntrances.Count; leftEntranceIndex++) // 모든 왼쪽 입구 순회
                        { // 왼쪽 입구 조합 처리
                            for (int rightEntranceIndex = 0; rightEntranceIndex < rightEntrances.Count; rightEntranceIndex++) // 모든 오른쪽 입구 순회
                            { // 오른쪽 입구 조합 처리
                                attemptCount++; // 실제 병렬 조합 시도 횟수 증가

                                if (attemptCount > settings.MaximumPlacementAttempts) // 최대 시도 횟수 초과 확인
                                { // 최대 시도 횟수 초과 처리
                                    DestroyCandidate(leftCandidate); // 왼쪽 임시 후보 제거
                                    DestroyCandidate(rightCandidate); // 오른쪽 임시 후보 제거
                                    return false; // 병렬 배치 실패 반환
                                } // 최대 시도 횟수 초과 처리

                                MapModuleConnectionPoint leftExit = leftExits[leftExitIndex]; // 현재 왼쪽 출구 조회
                                MapModuleConnectionPoint rightExit = rightExits[rightExitIndex]; // 현재 오른쪽 출구 조회
                                MapModuleConnectionPoint leftEntrance = leftEntrances[leftEntranceIndex]; // 현재 왼쪽 입구 조회
                                MapModuleConnectionPoint rightEntrance = rightEntrances[rightEntranceIndex]; // 현재 오른쪽 입구 조회
                                ResetCandidateTransform(leftCandidate, option.QuarterTurns); // 왼쪽 후보 위치와 회전 초기화
                                ResetCandidateTransform(rightCandidate, option.QuarterTurns); // 오른쪽 후보 위치와 회전 초기화

                                if (!AreConnectionsCompatible(leftExit, leftEntrance) || !AreConnectionsCompatible(rightExit, rightEntrance)) // 좌우 연결 호환성 확인
                                { // 좌우 연결 비호환 처리
                                    continue; // 다음 입구 조합 시도
                                } // 좌우 연결 비호환 처리

                                AlignCandidate(leftExit, leftEntrance, leftCandidate); // 왼쪽 후보 정렬
                                AlignCandidate(rightExit, rightEntrance, rightCandidate); // 오른쪽 후보 정렬
                                bool pairOverlaps = MapGenerationRules.BoundsHaveBlockingOverlap(leftCandidate.WorldBounds, rightCandidate.WorldBounds, settings.OverlapTolerance); // 좌우 후보 상호 겹침 검사

                                if (pairOverlaps || OverlapsPlacedModule(leftCandidate) || OverlapsPlacedModule(rightCandidate)) // 전체 배치 영역 겹침 확인
                                { // 병렬 후보 겹침 처리
                                    continue; // 다음 입구 조합 시도
                                } // 병렬 후보 겹침 처리

                                usedConnections.Add(leftExit); // 왼쪽 이전 출구 사용 완료 등록
                                usedConnections.Add(rightExit); // 오른쪽 이전 출구 사용 완료 등록
                                usedConnections.Add(leftEntrance); // 왼쪽 후보 입구 사용 완료 등록
                                usedConnections.Add(rightEntrance); // 오른쪽 후보 입구 사용 완료 등록
                                leftResult.Module = leftCandidate; // 왼쪽 생성 모듈 저장
                                leftResult.SourceExit = leftExit; // 왼쪽 사용 출구 저장
                                leftResult.Entrance = leftEntrance; // 왼쪽 사용 입구 저장
                                rightResult.Module = rightCandidate; // 오른쪽 생성 모듈 저장
                                rightResult.SourceExit = rightExit; // 오른쪽 사용 출구 저장
                                rightResult.Entrance = rightEntrance; // 오른쪽 사용 입구 저장
                                return true; // 병렬 배치 성공 반환
                            } // 오른쪽 입구 조합 처리
                        } // 왼쪽 입구 조합 처리

                        DestroyCandidate(leftCandidate); // 실패한 왼쪽 후보 제거
                        DestroyCandidate(rightCandidate); // 실패한 오른쪽 후보 제거
                    } // 동일 후보 조합 처리
                } // 오른쪽 출구 조합 처리
            } // 왼쪽 출구 조합 처리

            return false; // 모든 병렬 조합 실패 반환
        } // 병렬 모듈 쌍 배치 처리

        private bool TryCreateMergeModule(MapModuleDefinition leftPrevious, MapModuleDefinition rightPrevious, List<MapModuleDefinition> mergePrefabs, System.Random random, out MapModuleDefinition mergeModule, out PlacementResult leftResult, out PlacementResult rightResult) // 두 경로 끝에 합류 모듈 배치
        { // 합류 모듈 배치 처리
            mergeModule = null; // 합류 모듈 결과 초기화
            leftResult = new PlacementResult(); // 왼쪽 합류 결과 초기화
            rightResult = new PlacementResult(); // 오른쪽 합류 결과 초기화
            List<MapModuleConnectionPoint> leftExits = GetAvailableConnections(leftPrevious, MapConnectionRole.Exit); // 왼쪽 경로 출구 수집
            List<MapModuleConnectionPoint> rightExits = GetAvailableConnections(rightPrevious, MapConnectionRole.Exit); // 오른쪽 경로 출구 수집
            List<PlacementOption> placementOptions = BuildPlacementOptions(mergePrefabs); // 합류 Prefab 회전 후보 생성
            ShuffleList(leftExits, random); // 왼쪽 출구 순서 섞기
            ShuffleList(rightExits, random); // 오른쪽 출구 순서 섞기
            ShuffleList(placementOptions, random); // 합류 후보 순서 섞기
            int attemptCount = 0; // 실제 합류 조합 시도 횟수 초기화

            for (int leftExitIndex = 0; leftExitIndex < leftExits.Count; leftExitIndex++) // 모든 왼쪽 출구 순회
            { // 왼쪽 합류 출구 처리
                for (int rightExitIndex = 0; rightExitIndex < rightExits.Count; rightExitIndex++) // 모든 오른쪽 출구 순회
                { // 오른쪽 합류 출구 처리
                    for (int optionIndex = 0; optionIndex < placementOptions.Count; optionIndex++) // 모든 합류 후보 순회
                    { // 합류 후보 처리
                        PlacementOption option = placementOptions[optionIndex]; // 현재 합류 후보 조회
                        MapModuleDefinition candidate = CreateCandidate(option, generatedModules.Count); // 임시 합류 후보 생성
                        List<MapModuleConnectionPoint> entrances = GetAvailableConnections(candidate, MapConnectionRole.Entrance); // 합류 후보 입구 수집
                        ShuffleList(entrances, random); // 합류 입구 순서 섞기

                        for (int firstEntranceIndex = 0; firstEntranceIndex < entrances.Count; firstEntranceIndex++) // 첫 합류 입구 순회
                        { // 첫 합류 입구 처리
                            for (int secondEntranceIndex = 0; secondEntranceIndex < entrances.Count; secondEntranceIndex++) // 둘째 합류 입구 순회
                            { // 둘째 합류 입구 처리
                                if (firstEntranceIndex == secondEntranceIndex) // 같은 입구 중복 사용 확인
                                { // 같은 입구 중복 처리
                                    continue; // 현재 조합 제외
                                } // 같은 입구 중복 처리

                                attemptCount++; // 실제 합류 조합 시도 횟수 증가

                                if (attemptCount > settings.MaximumPlacementAttempts) // 최대 시도 횟수 초과 확인
                                { // 최대 시도 횟수 초과 처리
                                    DestroyCandidate(candidate); // 임시 합류 후보 제거
                                    return false; // 합류 배치 실패 반환
                                } // 최대 시도 횟수 초과 처리

                                MapModuleConnectionPoint leftExit = leftExits[leftExitIndex]; // 현재 왼쪽 출구 조회
                                MapModuleConnectionPoint rightExit = rightExits[rightExitIndex]; // 현재 오른쪽 출구 조회
                                MapModuleConnectionPoint firstEntrance = entrances[firstEntranceIndex]; // 현재 첫 합류 입구 조회
                                MapModuleConnectionPoint secondEntrance = entrances[secondEntranceIndex]; // 현재 둘째 합류 입구 조회
                                ResetCandidateTransform(candidate, option.QuarterTurns); // 합류 후보 위치와 회전 초기화

                                if (!AreConnectionsCompatible(leftExit, firstEntrance) || !AreConnectionsCompatible(rightExit, secondEntrance)) // 두 합류 연결 호환성 확인
                                { // 합류 연결 비호환 처리
                                    continue; // 다음 입구 조합 시도
                                } // 합류 연결 비호환 처리

                                AlignCandidate(leftExit, firstEntrance, candidate); // 첫 입구 기준 합류 후보 정렬
                                bool secondPositionMatches = MapGenerationRules.AreConnectionPositionsAligned(rightExit.transform.position, secondEntrance.transform.position, settings.ConnectionPositionTolerance); // 둘째 입구와 오른쪽 출구 위치 일치 검사

                                if (!secondPositionMatches || OverlapsPlacedModule(candidate)) // 둘째 위치 또는 영역 겹침 확인
                                { // 합류 배치 불가 처리
                                    continue; // 다음 입구 조합 시도
                                } // 합류 배치 불가 처리

                                usedConnections.Add(leftExit); // 왼쪽 이전 출구 사용 완료 등록
                                usedConnections.Add(rightExit); // 오른쪽 이전 출구 사용 완료 등록
                                usedConnections.Add(firstEntrance); // 첫 합류 입구 사용 완료 등록
                                usedConnections.Add(secondEntrance); // 둘째 합류 입구 사용 완료 등록
                                mergeModule = candidate; // 합류 모듈 결과 저장
                                leftResult.Module = candidate; // 왼쪽 합류 대상 저장
                                leftResult.SourceExit = leftExit; // 왼쪽 합류 출구 저장
                                leftResult.Entrance = firstEntrance; // 첫 합류 입구 저장
                                rightResult.Module = candidate; // 오른쪽 합류 대상 저장
                                rightResult.SourceExit = rightExit; // 오른쪽 합류 출구 저장
                                rightResult.Entrance = secondEntrance; // 둘째 합류 입구 저장
                                return true; // 합류 배치 성공 반환
                            } // 둘째 합류 입구 처리
                        } // 첫 합류 입구 처리

                        DestroyCandidate(candidate); // 실패한 합류 후보 제거
                    } // 합류 후보 처리
                } // 오른쪽 합류 출구 처리
            } // 왼쪽 합류 출구 처리

            return false; // 모든 합류 조합 실패 반환
        } // 합류 모듈 배치 처리

        private List<PlacementOption> BuildPlacementOptions(List<MapModuleDefinition> validPrefabs) // Prefab과 허용 회전 조합 생성
        { // 배치 후보 조합 생성 처리
            List<PlacementOption> options = new List<PlacementOption>(); // 배치 후보 목록 생성

            for (int prefabIndex = 0; prefabIndex < validPrefabs.Count; prefabIndex++) // 모든 유효 Prefab 순회
            { // 유효 Prefab 조합 처리
                MapModuleDefinition prefab = validPrefabs[prefabIndex]; // 현재 유효 Prefab 조회
                int[] allowedQuarterTurns = MapGenerationRules.GetAllowedQuarterTurns(prefab.AllowedRotations); // 현재 Prefab 허용 회전 조회

                for (int rotationIndex = 0; rotationIndex < allowedQuarterTurns.Length; rotationIndex++) // 모든 허용 회전 순회
                { // 허용 회전 조합 처리
                    PlacementOption option = new PlacementOption(); // 새 배치 후보 생성
                    option.Prefab = prefab; // 후보 Prefab 저장
                    option.QuarterTurns = allowedQuarterTurns[rotationIndex]; // 후보 회전 저장
                    options.Add(option); // 배치 후보 목록 등록
                } // 허용 회전 조합 처리
            } // 유효 Prefab 조합 처리

            return options; // 전체 배치 후보 반환
        } // 배치 후보 조합 생성 처리

        private MapModuleDefinition CreateCandidate(PlacementOption option, int moduleIndex) // 배치 검사용 후보 인스턴스 생성
        { // 배치 검사용 후보 생성 처리
            MapModuleDefinition candidate = Instantiate(option.Prefab, generatedRoot); // 후보 Prefab 인스턴스 생성
            candidate.name = $"{option.Prefab.ModuleId}_{moduleIndex:00}"; // 후보 순서 이름 적용
            ResetCandidateTransform(candidate, option.QuarterTurns); // 후보 위치와 회전 초기화
            return candidate; // 생성된 후보 반환
        } // 배치 검사용 후보 생성 처리

        private void ResetCandidateTransform(MapModuleDefinition candidate, int quarterTurns) // 후보 Transform 초기 상태 적용
        { // 후보 Transform 초기화 처리
            candidate.transform.localPosition = Vector3.zero; // 후보 로컬 위치 초기화
            candidate.transform.localRotation = MapGenerationRules.QuarterTurnRotation(quarterTurns); // 후보 허용 직각 회전 적용
        } // 후보 Transform 초기화 처리

        private void AlignCandidate(MapModuleConnectionPoint sourceExit, MapModuleConnectionPoint entrance, MapModuleDefinition candidate) // 후보 입구를 이전 출구 위치에 정렬
        { // 후보 정렬 처리
            candidate.transform.position = MapGenerationRules.CalculateAlignedRootPosition(candidate.transform.position, sourceExit.transform.position, entrance.transform.position); // 두 연결 지점 위치 일치
        } // 후보 정렬 처리

        private bool AreConnectionsCompatible(MapModuleConnectionPoint sourceExit, MapModuleConnectionPoint entrance) // 출구와 입구 방향과 크기 호환성 검사
        { // 연결 지점 통합 호환성 검사 처리
            bool directionsMatch = MapGenerationRules.AreWorldDirectionsOpposite(sourceExit.WorldDirection, entrance.WorldDirection); // 월드 방향 마주 보기 검사
            bool sizesMatch = MapGenerationRules.AreConnectionSizesCompatible(sourceExit.ConnectionWidth, sourceExit.ConnectionHeight, entrance.ConnectionWidth, entrance.ConnectionHeight, settings.ConnectionSizeTolerance); // 연결부 너비와 높이 검사
            return directionsMatch && sizesMatch; // 방향과 크기 통합 결과 반환
        } // 연결 지점 통합 호환성 검사 처리

        private List<MapModuleConnectionPoint> GetAvailableConnections(MapModuleDefinition module, MapConnectionRole role) // 지정 역할의 미사용 연결 지점 수집
        { // 미사용 연결 지점 수집 처리
            List<MapModuleConnectionPoint> matches = new List<MapModuleConnectionPoint>(); // 역할 일치 결과 목록 생성
            MapModuleConnectionPoint[] points = module.ConnectionPoints; // 모듈 연결 지점 목록 조회

            for (int pointIndex = 0; pointIndex < points.Length; pointIndex++) // 모든 연결 지점 순회
            { // 연결 지점 역할과 사용 상태 검사
                MapModuleConnectionPoint point = points[pointIndex]; // 현재 연결 지점 조회

                if (point != null && point.Role == role && !usedConnections.Contains(point)) // 역할 일치와 미사용 상태 확인
                { // 사용 가능한 연결 지점 처리
                    matches.Add(point); // 사용 가능 목록 등록
                } // 사용 가능한 연결 지점 처리
            } // 연결 지점 역할과 사용 상태 검사

            return matches; // 사용 가능 연결 지점 목록 반환
        } // 미사용 연결 지점 수집 처리

        private void SortConnectionsByWorldX(List<MapModuleConnectionPoint> connections) // 연결 지점을 월드 X축 오름차순 정렬
        { // 연결 지점 좌우 정렬 처리
            connections.Sort((first, second) => first.transform.position.x.CompareTo(second.transform.position.x)); // 월드 X축 비교 정렬
        } // 연결 지점 좌우 정렬 처리

        private void ShuffleList<T>(List<T> items, System.Random random) // 시드 기반 목록 순서 섞기
        { // Fisher-Yates 순서 섞기 처리
            for (int currentIndex = items.Count - 1; currentIndex > 0; currentIndex--) // 뒤에서 두 번째 위치까지 역순 순회
            { // 항목 교환 처리
                int randomIndex = random.Next(currentIndex + 1); // 교환 대상 무작위 위치 계산
                T temporaryItem = items[currentIndex]; // 현재 항목 임시 저장
                items[currentIndex] = items[randomIndex]; // 무작위 항목을 현재 위치로 이동
                items[randomIndex] = temporaryItem; // 기존 현재 항목을 무작위 위치로 이동
            } // 항목 교환 처리
        } // Fisher-Yates 순서 섞기 처리

        private bool OverlapsPlacedModule(MapModuleDefinition candidate) // 후보와 기존 모듈 겹침 검사
        { // 기존 모듈 겹침 검사 처리
            Bounds candidateBounds = candidate.WorldBounds; // 후보 모듈 월드 영역 조회

            for (int moduleIndex = 0; moduleIndex < generatedModules.Count; moduleIndex++) // 모든 기존 모듈 순회
            { // 기존 모듈 영역 비교 처리
                MapModuleDefinition placedModule = generatedModules[moduleIndex]; // 현재 기존 모듈 조회

                if (placedModule != null && MapGenerationRules.BoundsHaveBlockingOverlap(candidateBounds, placedModule.WorldBounds, settings.OverlapTolerance)) // 허용값을 넘는 실제 겹침 확인
                { // 실제 겹침 처리
                    return true; // 겹침 있음 반환
                } // 실제 겹침 처리
            } // 기존 모듈 영역 비교 처리

            return false; // 겹침 없음 반환
        } // 기존 모듈 겹침 검사 처리

        private void RegisterFirstModule(MapModuleDefinition module) // 첫 모듈과 그래프 시작 노드 등록
        { // 첫 모듈 등록 처리
            generatedModules.Add(module); // 생성 모듈 목록 등록
            UpdateVerticalProgress(module, 0); // 첫 모듈 공통 경로 상승량 누적
            int nodeIndex = AddGraphNode(module, 0); // 중앙 경로 그래프 노드 등록
            graphNodeIndices.Add(module, nodeIndex); // 모듈별 노드 번호 등록
        } // 첫 모듈 등록 처리

        private void RegisterPlacement(MapModuleDefinition previousModule, PlacementResult placement, int laneIndex) // 단일 연결 배치 결과 등록
        { // 단일 연결 배치 등록 처리
            generatedModules.Add(placement.Module); // 생성 모듈 목록 등록
            UpdateVerticalProgress(placement.Module, laneIndex); // 새 모듈 경로별 상승량 누적
            int nodeIndex = AddGraphNode(placement.Module, laneIndex); // 새 그래프 노드 등록
            graphNodeIndices.Add(placement.Module, nodeIndex); // 모듈별 노드 번호 등록
            int previousNodeIndex = graphNodeIndices[previousModule]; // 이전 그래프 노드 번호 조회
            graphEdges.Add(new MapGenerationGraphEdge(previousNodeIndex, nodeIndex, placement.SourceExit.ConnectionId, placement.Entrance.ConnectionId)); // 이전 모듈에서 새 모듈 간선 등록
        } // 단일 연결 배치 등록 처리

        private void RegisterMergePlacement(MapModuleDefinition leftPrevious, MapModuleDefinition rightPrevious, MapModuleDefinition mergeModule, PlacementResult leftResult, PlacementResult rightResult) // 두 경로 합류 배치 결과 등록
        { // 합류 배치 등록 처리
            generatedModules.Add(mergeModule); // 합류 모듈 목록 등록
            UpdateVerticalProgress(mergeModule, 0); // 합류 모듈 공통 경로 상승량 누적
            int mergeNodeIndex = AddGraphNode(mergeModule, 0); // 중앙 합류 그래프 노드 등록
            graphNodeIndices.Add(mergeModule, mergeNodeIndex); // 합류 모듈 노드 번호 등록
            int leftNodeIndex = graphNodeIndices[leftPrevious]; // 왼쪽 이전 노드 번호 조회
            int rightNodeIndex = graphNodeIndices[rightPrevious]; // 오른쪽 이전 노드 번호 조회
            graphEdges.Add(new MapGenerationGraphEdge(leftNodeIndex, mergeNodeIndex, leftResult.SourceExit.ConnectionId, leftResult.Entrance.ConnectionId)); // 왼쪽 경로 합류 간선 등록
            graphEdges.Add(new MapGenerationGraphEdge(rightNodeIndex, mergeNodeIndex, rightResult.SourceExit.ConnectionId, rightResult.Entrance.ConnectionId)); // 오른쪽 경로 합류 간선 등록
        } // 합류 배치 등록 처리

        private int AddGraphNode(MapModuleDefinition module, int laneIndex) // 모듈 기반 그래프 노드 추가
        { // 그래프 노드 추가 처리
            int nodeIndex = graphNodes.Count; // 새 노드 순서 번호 계산
            graphNodes.Add(new MapGenerationGraphNode(nodeIndex, module.ModuleId, laneIndex, module.transform.position)); // 새 그래프 노드 등록
            return nodeIndex; // 새 노드 번호 반환
        } // 그래프 노드 추가 처리

        private float CalculateEffectiveTargetHeight(System.Random random) // 시드 기반 실제 목표 높이 계산
        { // 실제 목표 높이 계산 처리
            float minimumHeight = settings.MinimumTargetHeight; // 최소 목표 높이 조회
            float maximumHeight = settings.MaximumTargetHeight; // 최대 목표 높이 조회

            if (Mathf.Approximately(minimumHeight, maximumHeight)) // 단일 목표 높이 확인
            { // 단일 목표 높이 처리
                return minimumHeight; // 고정 목표 높이 반환
            } // 단일 목표 높이 처리 종료

            float interpolation = (float)random.NextDouble(); // 시드 기반 0부터 1 사이 비율 계산
            float rawTargetHeight = Mathf.Lerp(minimumHeight, maximumHeight, interpolation); // 최소와 최대 사이 목표 높이 계산
            return Mathf.Round(rawTargetHeight * 2f) * 0.5f; // 0.5미터 단위 목표 높이 반환
        } // 실제 목표 높이 계산 처리 종료

        private void UpdateVerticalProgress(MapModuleDefinition module, int laneIndex) // 생성 방식과 경로에 맞는 수직 진행 수치 누적
        { // 경로별 수직 진행 누적 처리
            if (settings != null && settings.UseVerticalGeneration && settings.UseBranchingPath) // 수직 분기 생성 활성 확인
            { // 수직 분기 진행 누적 처리
                UpdateVerticalBranchProgress(module, laneIndex); // 좌우 또는 공통 경로 수치 갱신
                return; // 선형 진행 누적 생략
            } // 수직 분기 진행 누적 처리 종료

            UpdateVerticalProgress(module); // 기존 수직 선형 진행 수치 누적
        } // 경로별 수직 진행 누적 처리 종료

        private void UpdateVerticalBranchProgress(MapModuleDefinition module, int laneIndex) // 수직 분기 경로별 진행 수치 누적
        { // 수직 분기 진행 누적 처리
            float heightGain = MapVerticalGenerationRules.GetHeightGain(module); // 현재 모듈 상승량 조회
            bool isAscending = heightGain > MapVerticalGenerationRules.HeightEpsilon; // 현재 모듈 상승 여부 계산
            bool isFlat = Mathf.Abs(heightGain) <= MapVerticalGenerationRules.HeightEpsilon; // 현재 모듈 평지 여부 계산

            if (laneIndex < 0) // 왼쪽 분기 경로 확인
            { // 왼쪽 분기 진행 처리
                leftBranchHeight += heightGain; // 왼쪽 분기 누적 높이 갱신
                leftBranchAscendingModuleCount += isAscending ? 1 : 0; // 왼쪽 분기 상승 수 갱신
                leftBranchConsecutiveFlatModuleCount = isFlat ? leftBranchConsecutiveFlatModuleCount + 1 : 0; // 왼쪽 연속 평지 수 갱신
                maximumObservedConsecutiveFlatModules = Mathf.Max(maximumObservedConsecutiveFlatModules, leftBranchConsecutiveFlatModuleCount); // 전체 최대 연속 평지 수 갱신
            } // 왼쪽 분기 진행 처리 종료
            else if (laneIndex > 0) // 오른쪽 분기 경로 확인
            { // 오른쪽 분기 진행 처리
                rightBranchHeight += heightGain; // 오른쪽 분기 누적 높이 갱신
                rightBranchAscendingModuleCount += isAscending ? 1 : 0; // 오른쪽 분기 상승 수 갱신
                rightBranchConsecutiveFlatModuleCount = isFlat ? rightBranchConsecutiveFlatModuleCount + 1 : 0; // 오른쪽 연속 평지 수 갱신
                maximumObservedConsecutiveFlatModules = Mathf.Max(maximumObservedConsecutiveFlatModules, rightBranchConsecutiveFlatModuleCount); // 전체 최대 연속 평지 수 갱신
            } // 오른쪽 분기 진행 처리 종료
            else // 분기 전후 공통 경로 확인
            { // 공통 경로 진행 처리
                sharedPathHeight += heightGain; // 공통 경로 누적 높이 갱신
                sharedAscendingModuleCount += isAscending ? 1 : 0; // 공통 경로 상승 수 갱신
                sharedConsecutiveFlatModuleCount = isFlat ? sharedConsecutiveFlatModuleCount + 1 : 0; // 공통 경로 연속 평지 수 갱신
                maximumObservedConsecutiveFlatModules = Mathf.Max(maximumObservedConsecutiveFlatModules, sharedConsecutiveFlatModuleCount); // 전체 최대 연속 평지 수 갱신
            } // 공통 경로 진행 처리 종료

            RefreshVerticalBranchSummary(); // 좌우 경로 공통 요약 갱신
        } // 수직 분기 진행 누적 처리 종료

        private void InitializeBranchProgress() // 분기 시작 시 좌우 진행 수치 초기화
        { // 좌우 분기 진행 초기화 처리
            leftBranchHeight = 0f; // 왼쪽 분기 높이 초기화
            rightBranchHeight = 0f; // 오른쪽 분기 높이 초기화
            leftBranchAscendingModuleCount = 0; // 왼쪽 분기 상승 수 초기화
            rightBranchAscendingModuleCount = 0; // 오른쪽 분기 상승 수 초기화
            leftBranchConsecutiveFlatModuleCount = sharedConsecutiveFlatModuleCount; // 왼쪽 연속 평지 수를 공통 경로에서 이어받기
            rightBranchConsecutiveFlatModuleCount = sharedConsecutiveFlatModuleCount; // 오른쪽 연속 평지 수를 공통 경로에서 이어받기
            RefreshVerticalBranchSummary(); // 초기 분기 요약 갱신
        } // 좌우 분기 진행 초기화 처리 종료

        private void PrepareSharedProgressForMerge() // 합류 뒤 공통 경로의 연속 평지 상태 준비
        { // 합류 공통 진행 준비 처리
            sharedConsecutiveFlatModuleCount = Mathf.Max(leftBranchConsecutiveFlatModuleCount, rightBranchConsecutiveFlatModuleCount); // 더 긴 좌우 연속 평지 수를 공통 경로로 연결
        } // 합류 공통 진행 준비 처리 종료

        private void RefreshVerticalBranchSummary() // 좌우 분기 기준 최종 높이와 상승 수 요약 갱신
        { // 수직 분기 요약 갱신 처리
            float reachableBranchHeight = Mathf.Min(leftBranchHeight, rightBranchHeight); // 양쪽 모두 도달 가능한 분기 높이 계산
            int reachableBranchAscendingModules = Mathf.Min(leftBranchAscendingModuleCount, rightBranchAscendingModuleCount); // 양쪽 모두 만족하는 분기 상승 수 계산
            generatedHeight = sharedPathHeight + reachableBranchHeight; // 공통 경로와 양쪽 분기 높이 합산
            ascendingModuleCount = sharedAscendingModuleCount + reachableBranchAscendingModules; // 공통 경로와 양쪽 최소 상승 수 합산
            consecutiveFlatModuleCount = sharedConsecutiveFlatModuleCount; // 기존 후보 필터용 공통 연속 평지 수 동기화
        } // 수직 분기 요약 갱신 처리 종료

        private void UpdateVerticalProgress(MapModuleDefinition module) // 단일 모듈의 수직 진행 수치 누적
        { // 단일 모듈 수직 진행 누적 처리
            if (settings == null || !settings.UseVerticalGeneration) // 수직 생성 비활성 확인
            { // 수직 생성 비활성 처리
                return; // 수직 진행 누적 생략
            } // 수직 생성 비활성 처리 종료

            float heightGain = MapVerticalGenerationRules.GetHeightGain(module); // 현재 모듈 상승량 조회
            generatedHeight += heightGain; // 누적 생성 높이에 현재 상승량 추가

            if (heightGain > MapVerticalGenerationRules.HeightEpsilon) // 상승 모듈 확인
            { // 상승 모듈 처리
                ascendingModuleCount++; // 상승 모듈 개수 증가
                consecutiveFlatModuleCount = 0; // 연속 평지 개수 초기화
            } // 상승 모듈 처리 종료
            else if (Mathf.Abs(heightGain) <= MapVerticalGenerationRules.HeightEpsilon) // 평지 모듈 확인
            { // 평지 모듈 처리
                consecutiveFlatModuleCount++; // 연속 평지 개수 증가
                maximumObservedConsecutiveFlatModules = Mathf.Max(maximumObservedConsecutiveFlatModules, consecutiveFlatModuleCount); // 최대 연속 평지 개수 갱신
            } // 평지 모듈 처리 종료
            else // 하강 모듈 확인
            { // 하강 모듈 처리
                consecutiveFlatModuleCount = 0; // 연속 평지 개수 초기화
            } // 하강 모듈 처리 종료
        } // 단일 모듈 수직 진행 누적 처리 종료

        private void RecalculateVerticalProgress() // 생성 모듈 목록 기반 수직 진행 수치 재계산
        { // 수직 진행 수치 재계산 처리
            generatedHeight = 0f; // 누적 생성 높이 초기화
            ascendingModuleCount = 0; // 상승 모듈 개수 초기화
            consecutiveFlatModuleCount = 0; // 연속 평지 모듈 개수 초기화
            maximumObservedConsecutiveFlatModules = 0; // 최대 연속 평지 모듈 개수 초기화
            leftBranchHeight = 0f; // 왼쪽 분기 높이 초기화
            rightBranchHeight = 0f; // 오른쪽 분기 높이 초기화
            leftBranchAscendingModuleCount = 0; // 왼쪽 분기 상승 수 초기화
            rightBranchAscendingModuleCount = 0; // 오른쪽 분기 상승 수 초기화
            sharedPathHeight = 0f; // 공통 경로 높이 초기화
            sharedAscendingModuleCount = 0; // 공통 경로 상승 수 초기화
            sharedConsecutiveFlatModuleCount = 0; // 공통 경로 연속 평지 수 초기화
            leftBranchConsecutiveFlatModuleCount = 0; // 왼쪽 분기 연속 평지 수 초기화
            rightBranchConsecutiveFlatModuleCount = 0; // 오른쪽 분기 연속 평지 수 초기화
            bool branchStarted = false; // 수직 분기 시작 여부 초기화
            bool branchMerged = false; // 수직 분기 합류 여부 초기화

            for (int moduleIndex = 0; moduleIndex < generatedModules.Count; moduleIndex++) // 모든 생성 모듈 순회
            { // 생성 모듈 수직 진행 반영 처리
                int laneIndex = moduleIndex < graphNodes.Count && graphNodes[moduleIndex] != null ? graphNodes[moduleIndex].LaneIndex : 0; // 현재 모듈 그래프 경로 번호 조회

                if (settings.UseVerticalGeneration && settings.UseBranchingPath && laneIndex != 0 && !branchStarted) // 첫 분기 경로 진입 확인
                { // 첫 분기 경로 진입 처리
                    InitializeBranchProgress(); // 공통 경로에서 좌우 진행 상태 분리
                    branchStarted = true; // 분기 시작 상태 저장
                } // 첫 분기 경로 진입 처리 종료

                if (settings.UseVerticalGeneration && settings.UseBranchingPath && laneIndex == 0 && branchStarted && !branchMerged) // 분기 뒤 첫 공통 모듈 확인
                { // 분기 합류 상태 연결 처리
                    PrepareSharedProgressForMerge(); // 좌우 연속 평지 상태를 공통 경로로 연결
                    branchMerged = true; // 분기 합류 상태 저장
                } // 분기 합류 상태 연결 처리 종료

                UpdateVerticalProgress(generatedModules[moduleIndex], laneIndex); // 현재 모듈의 경로별 상승량과 연속 평지 수 반영
            } // 생성 모듈 수직 진행 반영 처리 종료
        } // 수직 진행 수치 재계산 처리 종료

        private void ApplyVerticalResultValidation() // 현재 수직 생성 결과를 종합 검사에 반영
        { // 수직 생성 결과 반영 처리
            if (settings == null || !settings.UseVerticalGeneration) // 수직 생성 검사 제외 확인
            { // 수직 생성 검사 제외 처리
                return; // 수직 결과 검사 생략
            } // 수직 생성 검사 제외 처리 종료

            bool isValid = MapVerticalGenerationRules.TryValidateResult(generatedHeight, effectiveTargetHeight, ascendingModuleCount, settings.MinimumAscendingModules, maximumObservedConsecutiveFlatModules, settings.MaximumConsecutiveFlatModules, out string reason); // 수직 목표 높이와 구성 기준 검사

            if (!isValid) // 수직 생성 결과 실패 확인
            { // 수직 생성 결과 실패 처리
                lastValidationReport.AddIssue(MapGenerationValidationIssueCode.GenerationFlowFailed, $"수직 생성 기준 미달: {reason}", -1, -1); // 수직 기준 미달 문제 등록
            } // 수직 생성 결과 실패 처리 종료

            if (settings.UseBranchingPath && !MapVerticalBranchGenerationRules.TryValidateMerge(leftBranchHeight, rightBranchHeight, leftBranchAscendingModuleCount, rightBranchAscendingModuleCount, settings.MinimumAscendingModulesPerBranch, settings.BranchMergeHeightTolerance, out string branchReason)) // 수직 분기 합류 결과 검사
            { // 수직 분기 합류 결과 실패 처리
                lastValidationReport.AddIssue(MapGenerationValidationIssueCode.GenerationFlowFailed, $"수직 분기 합류 기준 미달: {branchReason}", -1, -1); // 수직 분기 기준 미달 문제 등록
            } // 수직 분기 합류 결과 실패 처리 종료
        } // 수직 생성 결과 반영 처리 종료

        private void RunPlayableRouteValidation() // 현재 그래프의 시작부터 종료까지 이동 가능 경로 검사
        { // 플레이 가능 경로 검사 처리
            bool requireBothBranchLanes = settings != null && settings.UseBranchingPath; // 현재 설정의 좌우 분기 경로 필수 여부 계산
            lastPlayableRouteReport = MapPlayableRouteValidator.Validate(generatedModules, graphNodes, graphEdges, 0, 16, requireBothBranchLanes); // 최대 16개 경로와 좌우 분기 이동 가능성 검사
        } // 플레이 가능 경로 검사 처리 종료

        private void RunObstaclePlanning() // 현재 생성된 좌우 분기에 장애물 생성과 위험도 계획 실행
        { // 분기 장애물 계획 처리
            MapBranchObstaclePlanner obstaclePlanner = GetComponent<MapBranchObstaclePlanner>(); // 같은 오브젝트의 분기 장애물 계획기 조회

            if (settings == null || !settings.UseBranchingPath || obstaclePlanner == null) // 장애물 계획 미사용 조건 확인
            { // 장애물 계획 미사용 처리
                lastObstaclePlanReport = MapObstaclePlanReport.CreateNotRequired(); // 계획 불필요 성공 보고서 저장
                return; // 장애물 계획 실행 생략
            } // 장애물 계획 미사용 처리 종료

            if (!lastValidationReport.IsValid || !lastPlayableRouteReport.IsValid) // 선행 생성과 경로 검사 실패 확인
            { // 선행 검사 실패 처리
                obstaclePlanner.ClearPlacedObstacles(this); // 잘못된 맵의 기존 장애물 제거
                lastObstaclePlanReport = MapObstaclePlanReport.CreateNotRequired(); // 선행 실패로 계획 생략 보고서 저장
                return; // 장애물 계획 실행 중단
            } // 선행 검사 실패 처리 종료

            lastObstaclePlanReport = obstaclePlanner.GeneratePlan(this); // 고정 시드 장애물 생성과 통로 폭과 위험도 검사 실행
        } // 분기 장애물 계획 처리 종료

        private void RunObstacleValidation() // 현재 생성 장애물의 통로 폭과 분기 위험도 재검사
        { // 분기 장애물 검사 처리
            MapBranchObstaclePlanner obstaclePlanner = GetComponent<MapBranchObstaclePlanner>(); // 같은 오브젝트의 분기 장애물 계획기 조회

            if (settings == null || !settings.UseBranchingPath || obstaclePlanner == null) // 장애물 검사 미사용 조건 확인
            { // 장애물 검사 미사용 처리
                lastObstaclePlanReport = MapObstaclePlanReport.CreateNotRequired(); // 검사 불필요 성공 보고서 저장
                return; // 장애물 검사 실행 생략
            } // 장애물 검사 미사용 처리 종료

            lastObstaclePlanReport = obstaclePlanner.ValidateCurrentPlan(this); // 현재 장애물 배치와 위험도 검사 실행
        } // 분기 장애물 검사 처리 종료

        private string BuildGenerationSignature() // 동일 시드 재현 확인용 생성 결과 서명 계산
        { // 생성 결과 서명 계산 처리
            StringBuilder builder = new StringBuilder(); // 결과 서명 문자열 생성기 준비

            for (int nodeIndex = 0; nodeIndex < graphNodes.Count; nodeIndex++) // 모든 그래프 노드 순회
            { // 그래프 노드 서명 추가 처리
                MapGenerationGraphNode node = graphNodes[nodeIndex]; // 현재 그래프 노드 조회
                builder.Append(node.NodeIndex); // 노드 번호 추가
                builder.Append(':'); // 노드 항목 구분자 추가
                builder.Append(node.ModuleId); // 모듈 ID 추가
                builder.Append(':'); // 노드 항목 구분자 추가
                builder.Append(node.LaneIndex); // 경로 번호 추가
                builder.Append(':'); // 노드 항목 구분자 추가
                builder.Append(node.WorldPosition.x.ToString("0.000")); // X 위치 추가
                builder.Append(','); // 위치 축 구분자 추가
                builder.Append(node.WorldPosition.y.ToString("0.000")); // Y 위치 추가
                builder.Append(','); // 위치 축 구분자 추가
                builder.Append(node.WorldPosition.z.ToString("0.000")); // Z 위치 추가
                builder.Append('|'); // 노드 구분자 추가
            } // 그래프 노드 서명 추가 처리

            for (int edgeIndex = 0; edgeIndex < graphEdges.Count; edgeIndex++) // 모든 그래프 간선 순회
            { // 그래프 간선 서명 추가 처리
                MapGenerationGraphEdge edge = graphEdges[edgeIndex]; // 현재 그래프 간선 조회
                builder.Append(edge.FromNodeIndex); // 출발 노드 번호 추가
                builder.Append('>'); // 간선 방향 구분자 추가
                builder.Append(edge.ToNodeIndex); // 도착 노드 번호 추가
                builder.Append(':'); // 간선 항목 구분자 추가
                builder.Append(edge.ExitConnectionId); // 출구 ID 추가
                builder.Append('>'); // 연결 방향 구분자 추가
                builder.Append(edge.EntranceConnectionId); // 입구 ID 추가
                builder.Append('|'); // 간선 구분자 추가
            } // 그래프 간선 서명 추가 처리

            if (settings != null && settings.UseVerticalGeneration) // 수직 생성 서명 추가 여부 확인
            { // 수직 생성 서명 추가 처리
                builder.Append("V:"); // 수직 정보 시작 표시 추가
                builder.Append(effectiveTargetHeight.ToString("0.000")); // 목표 높이 추가
                builder.Append(':'); // 수직 정보 구분자 추가
                builder.Append(generatedHeight.ToString("0.000")); // 생성 높이 추가
                builder.Append(':'); // 수직 정보 구분자 추가
                builder.Append(ascendingModuleCount); // 상승 모듈 수 추가
                builder.Append(':'); // 수직 정보 구분자 추가
                builder.Append(maximumObservedConsecutiveFlatModules); // 최대 연속 평지 수 추가

                if (settings.UseBranchingPath) // 수직 분기 서명 추가 여부 확인
                { // 수직 분기 서명 추가 처리
                    builder.Append(":B:"); // 수직 분기 정보 시작 표시 추가
                    builder.Append(leftBranchHeight.ToString("0.000")); // 왼쪽 분기 높이 추가
                    builder.Append(':'); // 수직 분기 정보 구분자 추가
                    builder.Append(rightBranchHeight.ToString("0.000")); // 오른쪽 분기 높이 추가
                    builder.Append(':'); // 수직 분기 정보 구분자 추가
                    builder.Append(leftBranchAscendingModuleCount); // 왼쪽 분기 상승 수 추가
                    builder.Append(':'); // 수직 분기 정보 구분자 추가
                    builder.Append(rightBranchAscendingModuleCount); // 오른쪽 분기 상승 수 추가
                    builder.Append(':'); // 수직 분기 정보 구분자 추가
                    builder.Append(branchCombinationRetryCount); // 분기 조합 재시도 수 추가
                } // 수직 분기 서명 추가 처리 종료
            } // 수직 생성 서명 추가 처리 종료

            if (lastObstaclePlanReport != null && lastObstaclePlanReport.IsCompleted && lastObstaclePlanReport.IsRequired) // 장애물 계획 서명 추가 여부 확인
            { // 장애물 계획 서명 추가 처리
                builder.Append("|O:"); // 장애물 계획 정보 시작 표시 추가
                builder.Append(lastObstaclePlanReport.BuildSignature()); // 장애물 배치와 위험도 재현 서명 추가
            } // 장애물 계획 서명 추가 처리 종료

            return builder.ToString(); // 완성된 생성 결과 서명 반환
        } // 생성 결과 서명 계산 처리

        private GenerationSnapshot CaptureGenerationSnapshot() // 현재 생성 상태의 안전한 되돌리기 지점 생성
        { // 생성 상태 저장 처리
            GenerationSnapshot snapshot = new GenerationSnapshot(); // 새 생성 상태 스냅샷 생성
            snapshot.ModuleCount = generatedModules.Count; // 현재 모듈 개수 저장
            snapshot.NodeCount = graphNodes.Count; // 현재 그래프 노드 개수 저장
            snapshot.EdgeCount = graphEdges.Count; // 현재 그래프 간선 개수 저장
            snapshot.UsedConnections = new HashSet<MapModuleConnectionPoint>(usedConnections); // 현재 사용 연결 지점 복사
            snapshot.SharedPathHeight = sharedPathHeight; // 현재 공통 경로 높이 저장
            snapshot.SharedAscendingModules = sharedAscendingModuleCount; // 현재 공통 경로 상승 수 저장
            snapshot.SharedConsecutiveFlatModules = sharedConsecutiveFlatModuleCount; // 현재 공통 경로 연속 평지 수 저장
            snapshot.MaximumConsecutiveFlatModules = maximumObservedConsecutiveFlatModules; // 현재 최대 연속 평지 수 저장
            return snapshot; // 완성된 생성 상태 스냅샷 반환
        } // 생성 상태 저장 처리 종료

        private void RollbackToSnapshot(GenerationSnapshot snapshot) // 실패한 분기 조합을 저장된 생성 상태로 복원
        { // 생성 상태 복원 처리
            for (int moduleIndex = generatedModules.Count - 1; moduleIndex >= snapshot.ModuleCount; moduleIndex--) // 스냅샷 뒤 생성 모듈 역순 순회
            { // 실패 모듈 제거 처리
                MapModuleDefinition module = generatedModules[moduleIndex]; // 현재 제거 대상 모듈 조회
                graphNodeIndices.Remove(module); // 제거 모듈 그래프 번호 삭제
                generatedModules.RemoveAt(moduleIndex); // 생성 모듈 목록에서 제거
                DestroyCandidate(module); // 실패 모듈 오브젝트 제거
            } // 실패 모듈 제거 처리 종료

            if (graphNodes.Count > snapshot.NodeCount) // 추가 그래프 노드 존재 확인
            { // 추가 그래프 노드 제거 처리
                graphNodes.RemoveRange(snapshot.NodeCount, graphNodes.Count - snapshot.NodeCount); // 스냅샷 뒤 그래프 노드 제거
            } // 추가 그래프 노드 제거 처리 종료

            if (graphEdges.Count > snapshot.EdgeCount) // 추가 그래프 간선 존재 확인
            { // 추가 그래프 간선 제거 처리
                graphEdges.RemoveRange(snapshot.EdgeCount, graphEdges.Count - snapshot.EdgeCount); // 스냅샷 뒤 그래프 간선 제거
            } // 추가 그래프 간선 제거 처리 종료

            usedConnections.Clear(); // 현재 사용 연결 지점 초기화

            foreach (MapModuleConnectionPoint connection in snapshot.UsedConnections) // 저장된 사용 연결 지점 순회
            { // 사용 연결 지점 복원 처리
                usedConnections.Add(connection); // 저장된 연결 지점 다시 등록
            } // 사용 연결 지점 복원 처리 종료

            sharedPathHeight = snapshot.SharedPathHeight; // 공통 경로 높이 복원
            sharedAscendingModuleCount = snapshot.SharedAscendingModules; // 공통 경로 상승 수 복원
            sharedConsecutiveFlatModuleCount = snapshot.SharedConsecutiveFlatModules; // 공통 경로 연속 평지 수 복원
            maximumObservedConsecutiveFlatModules = snapshot.MaximumConsecutiveFlatModules; // 최대 연속 평지 수 복원
            leftBranchHeight = 0f; // 왼쪽 분기 높이 초기화
            rightBranchHeight = 0f; // 오른쪽 분기 높이 초기화
            leftBranchAscendingModuleCount = 0; // 왼쪽 분기 상승 수 초기화
            rightBranchAscendingModuleCount = 0; // 오른쪽 분기 상승 수 초기화
            leftBranchConsecutiveFlatModuleCount = sharedConsecutiveFlatModuleCount; // 왼쪽 연속 평지 수 복원
            rightBranchConsecutiveFlatModuleCount = sharedConsecutiveFlatModuleCount; // 오른쪽 연속 평지 수 복원
            RefreshVerticalBranchSummary(); // 복원된 수직 분기 요약 갱신
        } // 생성 상태 복원 처리 종료

        private void EnsureGeneratedRoot() // 생성 모듈 보관 루트 존재 보장
        { // 생성 루트 존재 보장 처리
            if (generatedRoot != null) // 기존 생성 루트 확인
            { // 기존 생성 루트 처리
                return; // 새 루트 생성 생략
            } // 기존 생성 루트 처리

            GameObject rootObject = new GameObject("GeneratedMap"); // 새 생성 루트 오브젝트 생성
            rootObject.transform.SetParent(transform, false); // 생성기를 부모로 설정
            generatedRoot = rootObject.transform; // 생성 루트 참조 저장
        } // 생성 루트 존재 보장 처리

        private void DestroyCandidate(MapModuleDefinition candidate) // 실패한 후보 모듈 제거
        { // 후보 모듈 제거 처리
            if (candidate == null) // 빈 후보 확인
            { // 빈 후보 처리
                return; // 후보 제거 생략
            } // 빈 후보 처리

            if (Application.isPlaying) // Play Mode 여부 확인
            { // Play Mode 후보 제거 처리
                candidate.transform.SetParent(null, true); // 삭제 대기 후보를 현재 생성 루트에서 즉시 분리
                candidate.gameObject.SetActive(false); // 제거 대기 후보 비활성화
                Destroy(candidate.gameObject); // 프레임 종료 시 후보 제거
            } // Play Mode 후보 제거 처리
            else // Edit Mode 여부 확인
            { // Edit Mode 후보 제거 처리
                DestroyImmediate(candidate.gameObject); // 후보 즉시 제거
            } // Edit Mode 후보 제거 처리
        } // 후보 모듈 제거 처리

#if UNITY_EDITOR // Unity Editor 전용 설정
        public void ConfigureForEditor(MapGenerationSettings newSettings, Transform newGeneratedRoot, bool newGenerateOnStart, bool newLogDetailedResults) // Editor 도구용 생성기 설정 적용
        { // Editor 생성기 설정 적용 처리
            settings = newSettings; // 새 생성 설정 연결
            generatedRoot = newGeneratedRoot; // 새 생성 루트 연결
            generateOnStart = newGenerateOnStart; // 새 자동 생성 여부 저장
            logDetailedResults = newLogDetailedResults; // 새 상세 로그 여부 저장
        } // Editor 생성기 설정 적용 처리
#endif // Unity Editor 전용 설정
    } // 시드 기반 분기 맵 생성기 묶음
} // 맵 생성 기능 묶음
