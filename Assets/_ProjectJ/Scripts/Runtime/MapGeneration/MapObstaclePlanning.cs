using System; // 직렬화 기능 참조
using System.Collections.Generic; // 목록과 문자열 정렬 기능 참조
using System.Text; // 보고서와 서명 문자열 기능 참조
using ProjectJ.Data; // 장애물 데이터 정의 참조
using UnityEngine; // Unity 직렬화와 수학 기능 참조

namespace ProjectJ.MapGeneration // 맵 생성 기능 네임스페이스 선언
{ // 맵 생성 기능 묶음
    public enum MapBranchDifficulty // 분기 경로 난이도 종류 선언
    { // 분기 경로 난이도 종류 묶음
        None, // 난이도 미지정
        Safe, // 안전 경로
        HighRisk // 고위험 경로
    } // 분기 경로 난이도 종류 묶음 종료

    public enum MapObstaclePlanIssueCode // 장애물 계획 문제 코드 선언
    { // 장애물 계획 문제 코드 묶음
        MissingPlannerConfiguration, // 장애물 계획 설정 누락
        MissingBranchLane, // 좌우 분기 경로 누락
        MissingSpawnPoint, // 장애물 배치 지점 누락
        InvalidObstacleData, // 장애물 데이터 오류
        UnsafePassageWidth, // 배치 뒤 통로 폭 부족
        DuplicateSpawnPoint, // 같은 지점 중복 사용
        RiskBudgetNotReached, // 분기 최소 위험도 미달
        RiskBudgetExceeded, // 분기 최대 위험도 초과
        RiskGapTooSmall, // 안전과 고위험 경로 차이 부족
        InvalidPlacedObstacle // 생성 장애물 결과 오류
    } // 장애물 계획 문제 코드 묶음 종료

    [Serializable] // Unity 직렬화 대상 표시
    public sealed class MapObstaclePlanIssue // 장애물 계획 단일 문제 선언
    { // 장애물 계획 단일 문제 묶음
        [SerializeField] private MapObstaclePlanIssueCode code; // 문제 종류 코드
        [SerializeField] private string message; // 문제 상세 설명
        [SerializeField] private int nodeIndex; // 관련 그래프 노드 번호
        [SerializeField] private int laneIndex; // 관련 분기 경로 번호

        public MapObstaclePlanIssueCode Code => code; // 문제 종류 코드 반환
        public string Message => message; // 문제 상세 설명 반환
        public int NodeIndex => nodeIndex; // 관련 노드 번호 반환
        public int LaneIndex => laneIndex; // 관련 분기 번호 반환

        public MapObstaclePlanIssue(MapObstaclePlanIssueCode newCode, string newMessage, int newNodeIndex, int newLaneIndex) // 장애물 계획 문제 생성
        { // 장애물 계획 문제 생성 처리
            code = newCode; // 문제 코드 저장
            message = newMessage; // 문제 설명 저장
            nodeIndex = newNodeIndex; // 관련 노드 번호 저장
            laneIndex = newLaneIndex; // 관련 분기 번호 저장
        } // 장애물 계획 문제 생성 처리 종료
    } // 장애물 계획 단일 문제 묶음 종료

    [Serializable] // Unity 직렬화 대상 표시
    public sealed class MapObstaclePlacementRecord // 장애물 계획 단일 배치 기록 선언
    { // 장애물 계획 단일 배치 기록 묶음
        [SerializeField] private int nodeIndex; // 배치 그래프 노드 번호
        [SerializeField] private int laneIndex; // 배치 분기 경로 번호
        [SerializeField] private string moduleId; // 배치 모듈 ID
        [SerializeField] private string pointId; // 사용 배치 지점 ID
        [SerializeField] private string obstacleId; // 사용 장애물 데이터 ID
        [SerializeField] private int riskScore; // 적용 위험도 점수
        [SerializeField] private Vector3 worldPosition; // 장애물 월드 위치

        public int NodeIndex => nodeIndex; // 배치 노드 번호 반환
        public int LaneIndex => laneIndex; // 배치 분기 번호 반환
        public string ModuleId => moduleId; // 배치 모듈 ID 반환
        public string PointId => pointId; // 배치 지점 ID 반환
        public string ObstacleId => obstacleId; // 장애물 데이터 ID 반환
        public int RiskScore => riskScore; // 적용 위험도 반환
        public Vector3 WorldPosition => worldPosition; // 장애물 월드 위치 반환

        public MapObstaclePlacementRecord(int newNodeIndex, int newLaneIndex, string newModuleId, string newPointId, string newObstacleId, int newRiskScore, Vector3 newWorldPosition) // 장애물 배치 기록 생성
        { // 장애물 배치 기록 생성 처리
            nodeIndex = newNodeIndex; // 배치 노드 번호 저장
            laneIndex = newLaneIndex; // 배치 분기 번호 저장
            moduleId = newModuleId; // 배치 모듈 ID 저장
            pointId = newPointId; // 배치 지점 ID 저장
            obstacleId = newObstacleId; // 장애물 데이터 ID 저장
            riskScore = newRiskScore; // 적용 위험도 저장
            worldPosition = newWorldPosition; // 장애물 월드 위치 저장
        } // 장애물 배치 기록 생성 처리 종료

        public string BuildSignature() // 단일 장애물 배치 재현 서명 생성
        { // 단일 배치 서명 생성 처리
            return $"{nodeIndex}:{laneIndex}:{moduleId}:{pointId}:{obstacleId}:{riskScore}:{worldPosition.x:0.000},{worldPosition.y:0.000},{worldPosition.z:0.000}"; // 단일 배치 서명 반환
        } // 단일 배치 서명 생성 처리 종료
    } // 장애물 계획 단일 배치 기록 묶음 종료

    [Serializable] // Unity 직렬화 대상 표시
    public sealed class MapObstacleBranchSummary // 분기별 장애물 위험도 요약 선언
    { // 분기별 장애물 위험도 요약 묶음
        [SerializeField] private int laneIndex; // 분기 경로 번호
        [SerializeField] private MapBranchDifficulty difficulty; // 분기 난이도 종류
        [SerializeField] private int totalRiskScore; // 분기 총 위험도
        [SerializeField] private int obstacleCount; // 분기 장애물 개수

        public int LaneIndex => laneIndex; // 분기 경로 번호 반환
        public MapBranchDifficulty Difficulty => difficulty; // 분기 난이도 반환
        public int TotalRiskScore => totalRiskScore; // 분기 총 위험도 반환
        public int ObstacleCount => obstacleCount; // 분기 장애물 개수 반환

        public MapObstacleBranchSummary(int newLaneIndex, MapBranchDifficulty newDifficulty, int newTotalRiskScore, int newObstacleCount) // 분기 장애물 요약 생성
        { // 분기 장애물 요약 생성 처리
            laneIndex = newLaneIndex; // 분기 경로 번호 저장
            difficulty = newDifficulty; // 분기 난이도 저장
            totalRiskScore = Mathf.Max(0, newTotalRiskScore); // 음수가 아닌 총 위험도 저장
            obstacleCount = Mathf.Max(0, newObstacleCount); // 음수가 아닌 장애물 수 저장
        } // 분기 장애물 요약 생성 처리 종료
    } // 분기별 장애물 위험도 요약 묶음 종료

    [Serializable] // Unity 직렬화 대상 표시
    public sealed class MapObstaclePlanReport // 장애물 배치와 위험도 검사 보고서 선언
    { // 장애물 배치와 위험도 검사 보고서 묶음
        [SerializeField] private bool isCompleted; // 검사 완료 여부
        [SerializeField] private bool isRequired; // 현재 맵의 장애물 계획 필수 여부
        [SerializeField] private int safeLaneIndex; // 안전 경로 분기 번호
        [SerializeField] private int highRiskLaneIndex; // 고위험 경로 분기 번호
        [SerializeField] private MapObstacleBranchSummary leftBranch; // 왼쪽 분기 위험도 요약
        [SerializeField] private MapObstacleBranchSummary rightBranch; // 오른쪽 분기 위험도 요약
        [SerializeField] private List<MapObstaclePlacementRecord> placements = new List<MapObstaclePlacementRecord>(); // 장애물 배치 기록 목록
        [SerializeField] private List<MapObstaclePlanIssue> issues = new List<MapObstaclePlanIssue>(); // 장애물 계획 문제 목록

        public bool IsCompleted => isCompleted; // 검사 완료 여부 반환
        public bool IsRequired => isRequired; // 장애물 계획 필수 여부 반환
        public bool IsValid => isCompleted && issues.Count == 0; // 장애물 계획 성공 여부 반환
        public int SafeLaneIndex => safeLaneIndex; // 안전 경로 번호 반환
        public int HighRiskLaneIndex => highRiskLaneIndex; // 고위험 경로 번호 반환
        public MapObstacleBranchSummary LeftBranch => leftBranch; // 왼쪽 분기 요약 반환
        public MapObstacleBranchSummary RightBranch => rightBranch; // 오른쪽 분기 요약 반환
        public IReadOnlyList<MapObstaclePlacementRecord> Placements => placements; // 장애물 배치 기록 반환
        public IReadOnlyList<MapObstaclePlanIssue> Issues => issues; // 장애물 계획 문제 반환
        public int PlacementCount => placements.Count; // 장애물 배치 개수 반환
        public int IssueCount => issues.Count; // 장애물 문제 개수 반환

        private MapObstaclePlanReport(bool newIsCompleted, bool newIsRequired) // 장애물 계획 보고서 내부 생성
        { // 장애물 계획 보고서 생성 처리
            isCompleted = newIsCompleted; // 검사 완료 여부 저장
            isRequired = newIsRequired; // 장애물 계획 필수 여부 저장
            safeLaneIndex = 0; // 안전 경로 번호 초기화
            highRiskLaneIndex = 0; // 고위험 경로 번호 초기화
            leftBranch = new MapObstacleBranchSummary(-1, MapBranchDifficulty.None, 0, 0); // 왼쪽 분기 기본 요약 생성
            rightBranch = new MapObstacleBranchSummary(1, MapBranchDifficulty.None, 0, 0); // 오른쪽 분기 기본 요약 생성
        } // 장애물 계획 보고서 생성 처리 종료

        public static MapObstaclePlanReport CreateNotRun() // 아직 실행하지 않은 보고서 생성
        { // 미실행 보고서 생성 처리
            return new MapObstaclePlanReport(false, false); // 미실행 보고서 반환
        } // 미실행 보고서 생성 처리 종료

        public static MapObstaclePlanReport CreateNotRequired() // 장애물 계획이 필요 없는 보고서 생성
        { // 계획 불필요 보고서 생성 처리
            return new MapObstaclePlanReport(true, false); // 성공 상태의 불필요 보고서 반환
        } // 계획 불필요 보고서 생성 처리 종료

        public static MapObstaclePlanReport CreateRequired(int newSafeLaneIndex, int newHighRiskLaneIndex) // 분기 장애물 계획 보고서 생성
        { // 분기 장애물 계획 보고서 생성 처리
            MapObstaclePlanReport report = new MapObstaclePlanReport(true, true); // 완료 상태의 필수 보고서 생성
            report.safeLaneIndex = newSafeLaneIndex; // 안전 경로 번호 저장
            report.highRiskLaneIndex = newHighRiskLaneIndex; // 고위험 경로 번호 저장
            return report; // 구성된 필수 보고서 반환
        } // 분기 장애물 계획 보고서 생성 처리 종료

        public void AddPlacement(MapObstaclePlacementRecord placement) // 장애물 배치 기록 추가
        { // 장애물 배치 기록 추가 처리
            if (placement != null) // 유효한 배치 기록 확인
            { // 유효한 배치 기록 처리
                placements.Add(placement); // 배치 기록 목록에 추가
            } // 유효한 배치 기록 처리 종료
        } // 장애물 배치 기록 추가 처리 종료

        public void AddIssue(MapObstaclePlanIssueCode code, string message, int nodeIndex, int laneIndex) // 장애물 계획 문제 추가
        { // 장애물 계획 문제 추가 처리
            issues.Add(new MapObstaclePlanIssue(code, message, nodeIndex, laneIndex)); // 새 문제 기록 추가
        } // 장애물 계획 문제 추가 처리 종료

        public void SetBranchSummaries(MapObstacleBranchSummary newLeftBranch, MapObstacleBranchSummary newRightBranch) // 좌우 분기 위험도 요약 저장
        { // 좌우 분기 위험도 요약 저장 처리
            leftBranch = newLeftBranch ?? new MapObstacleBranchSummary(-1, MapBranchDifficulty.None, 0, 0); // 왼쪽 분기 요약 저장
            rightBranch = newRightBranch ?? new MapObstacleBranchSummary(1, MapBranchDifficulty.None, 0, 0); // 오른쪽 분기 요약 저장
        } // 좌우 분기 위험도 요약 저장 처리 종료

        public string BuildSignature() // 장애물 계획 재현 서명 생성
        { // 장애물 계획 서명 생성 처리
            List<string> placementSignatures = new List<string>(); // 정렬할 단일 배치 서명 목록 생성

            for (int placementIndex = 0; placementIndex < placements.Count; placementIndex++) // 모든 장애물 배치 순회
            { // 단일 장애물 배치 서명 수집 처리
                placementSignatures.Add(placements[placementIndex].BuildSignature()); // 현재 배치 서명 목록 추가
            } // 단일 장애물 배치 서명 수집 처리 종료

            placementSignatures.Sort(StringComparer.Ordinal); // 실행 순서와 무관한 고정 정렬 적용
            StringBuilder builder = new StringBuilder(); // 장애물 계획 서명 문자열 생성기 준비
            builder.Append($"S{safeLaneIndex}:H{highRiskLaneIndex}:L{leftBranch.TotalRiskScore}:R{rightBranch.TotalRiskScore}|"); // 분기 역할과 위험도 추가

            for (int signatureIndex = 0; signatureIndex < placementSignatures.Count; signatureIndex++) // 정렬된 모든 배치 서명 순회
            { // 단일 배치 서명 연결 처리
                builder.Append(placementSignatures[signatureIndex]); // 현재 배치 서명 추가
                builder.Append('|'); // 배치 구분자 추가
            } // 단일 배치 서명 연결 처리 종료

            return builder.ToString(); // 완성된 장애물 계획 서명 반환
        } // 장애물 계획 서명 생성 처리 종료

        public string BuildSummary() // 장애물 계획 한 줄 요약 생성
        { // 장애물 계획 요약 생성 처리
            return $"장애물 계획 {(IsValid ? "성공" : "실패")} | 배치: {PlacementCount} | 위험도: L {leftBranch.TotalRiskScore}/R {rightBranch.TotalRiskScore} | 문제: {IssueCount}"; // 장애물 계획 요약 반환
        } // 장애물 계획 요약 생성 처리 종료

        public string BuildDetailedMessage() // 장애물 계획 상세 결과 생성
        { // 장애물 계획 상세 결과 생성 처리
            StringBuilder builder = new StringBuilder(); // 상세 결과 문자열 생성기 준비
            builder.AppendLine(BuildSummary()); // 첫 줄에 요약 추가
            builder.AppendLine($"안전 경로: {safeLaneIndex} | 고위험 경로: {highRiskLaneIndex}"); // 분기 역할 정보 추가

            for (int issueIndex = 0; issueIndex < issues.Count; issueIndex++) // 모든 장애물 계획 문제 순회
            { // 단일 장애물 계획 문제 출력 처리
                MapObstaclePlanIssue issue = issues[issueIndex]; // 현재 장애물 계획 문제 조회
                builder.AppendLine($"- {issue.Code} | Lane {issue.LaneIndex} | Node {issue.NodeIndex} | {issue.Message}"); // 현재 문제 상세 내용 추가
            } // 단일 장애물 계획 문제 출력 처리 종료

            return builder.ToString().TrimEnd(); // 완성된 상세 결과 반환
        } // 장애물 계획 상세 결과 생성 처리 종료
    } // 장애물 배치와 위험도 검사 보고서 묶음 종료

    public static class MapObstaclePlacementRules // 장애물 배치와 위험도 공통 규칙 선언
    { // 장애물 배치와 위험도 공통 규칙 묶음
        public static int ResolveSafeLane(int seed) // 고정 시드 기반 안전 경로 번호 결정
        { // 안전 경로 번호 결정 처리
            return (seed & 1) == 0 ? -1 : 1; // 시드 짝수는 왼쪽 홀수는 오른쪽 반환
        } // 안전 경로 번호 결정 처리 종료

        public static MapBranchDifficulty ResolveDifficulty(int laneIndex, int safeLaneIndex) // 분기 번호별 난이도 결정
        { // 분기 난이도 결정 처리
            if (laneIndex == 0) // 공통 경로 확인
            { // 공통 경로 처리
                return MapBranchDifficulty.None; // 난이도 없음 반환
            } // 공통 경로 처리 종료

            return laneIndex == safeLaneIndex ? MapBranchDifficulty.Safe : MapBranchDifficulty.HighRisk; // 안전 경로 일치 여부별 난이도 반환
        } // 분기 난이도 결정 처리 종료

        public static bool IsRiskWithinBudget(int totalRisk, int minimumRisk, int maximumRisk) // 분기 위험도 예산 충족 여부 검사
        { // 분기 위험도 예산 검사 처리
            return totalRisk >= minimumRisk && totalRisk <= maximumRisk; // 최소와 최대 범위 포함 여부 반환
        } // 분기 위험도 예산 검사 처리 종료

        public static bool HasRequiredRiskGap(int safeRisk, int highRisk, int minimumGap) // 안전과 고위험 경로 차이 검사
        { // 위험도 차이 검사 처리
            return highRisk - safeRisk >= minimumGap; // 최소 위험도 차이 충족 여부 반환
        } // 위험도 차이 검사 처리 종료

        public static int CalculateTargetRisk(MapBranchDifficulty difficulty, int safeMinimum, int safeMaximum, int highMinimum, int highMaximum) // 난이도별 목표 위험도 계산
        { // 목표 위험도 계산 처리
            int minimumRisk = difficulty == MapBranchDifficulty.Safe ? safeMinimum : highMinimum; // 난이도별 최소 위험도 선택
            int maximumRisk = difficulty == MapBranchDifficulty.Safe ? safeMaximum : highMaximum; // 난이도별 최대 위험도 선택
            return minimumRisk + Mathf.Max(0, maximumRisk - minimumRisk) / 2; // 예산 중앙값 목표 반환
        } // 목표 위험도 계산 처리 종료

        public static bool IsUniquePoint(ISet<string> usedPointKeys, int nodeIndex, string pointId) // 노드와 지점 조합 중복 여부 검사
        { // 배치 지점 중복 검사 처리
            if (usedPointKeys == null || string.IsNullOrWhiteSpace(pointId)) // 잘못된 중복 검사 입력 확인
            { // 잘못된 중복 검사 입력 처리
                return false; // 고유 지점 실패 반환
            } // 잘못된 중복 검사 입력 처리 종료

            return !usedPointKeys.Contains($"{nodeIndex}:{pointId}"); // 기존 사용 목록 미포함 여부 반환
        } // 배치 지점 중복 검사 처리 종료
    } // 장애물 배치와 위험도 공통 규칙 묶음 종료
} // 맵 생성 기능 묶음 종료
