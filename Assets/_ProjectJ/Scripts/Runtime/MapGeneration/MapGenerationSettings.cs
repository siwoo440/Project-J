using UnityEngine; // Unity 데이터 에셋 기능 참조

namespace ProjectJ.MapGeneration // 맵 생성 기능 네임스페이스 선언
{ // 맵 생성 기능 묶음
    [CreateAssetMenu(fileName = "MAP-GEN-001_DefaultGenerationSettings", menuName = "Project J/Map Generation/Generation Settings")] // 맵 생성 설정 에셋 메뉴 등록
    public sealed class MapGenerationSettings : ScriptableObject // 맵 생성 공통 설정 선언
    { // 맵 생성 공통 설정 묶음
        private const int MinimumBranchedModuleCount = 5; // 분기 맵 최소 모듈 개수

        [SerializeField] private int seed = 31001; // 고정 생성 시드
        [SerializeField] private bool randomizeSeed; // 실행마다 시드 변경 여부
        [SerializeField, Range(0, 3)] private int startingQuarterTurns; // 첫 모듈 직각 회전 횟수
        [SerializeField, Min(1)] private int moduleCount = 8; // 목표 모듈 개수
        [SerializeField, Min(1)] private int maximumPlacementAttempts = 32; // 배치 단계별 최대 연결 조합 시도 횟수
        [SerializeField, Min(0f)] private float overlapTolerance = 0.05f; // 맞닿은 경계 허용 크기
        [SerializeField, Min(0f)] private float connectionSizeTolerance = 0.05f; // 연결부 크기 허용 오차
        [SerializeField, Min(0f)] private float connectionPositionTolerance = 0.02f; // 합류 연결부 위치 허용 오차
        [SerializeField] private bool useBranchingPath = true; // 분기와 합류 경로 사용 여부
        [SerializeField, Min(1)] private int branchPairCount = 2; // 좌우 병렬 경로 모듈 쌍 개수
        [SerializeField] private bool useVerticalGeneration = true; // 수직 상승형 선형 생성 사용 여부
        [SerializeField, Min(0f)] private float minimumTargetHeight = 8f; // 생성 종료 최소 목표 높이
        [SerializeField, Min(0f)] private float maximumTargetHeight = 16f; // 생성 종료 최대 목표 높이
        [SerializeField, Min(0)] private int minimumAscendingModules = 3; // 최소 상승 모듈 개수
        [SerializeField, Min(0)] private int maximumConsecutiveFlatModules = 2; // 최대 연속 평지 모듈 개수
        [SerializeField] private bool allowDescendingModules; // 하강 모듈 생성 허용 여부
        [SerializeField, Min(0)] private int minimumAscendingModulesPerBranch = 1; // 분기별 최소 상승 모듈 개수
        [SerializeField, Min(0f)] private float branchMergeHeightTolerance = 0.02f; // 좌우 합류 높이 허용 오차
        [SerializeField, Min(1)] private int maximumBranchCombinationRetries = 64; // 수직 분기 조합 최대 재시도 횟수
        [SerializeField] private MapModuleDefinition[] modulePrefabs; // 생성 후보 모듈 Prefab 목록

        public int Seed => seed; // 고정 생성 시드 반환
        public bool RandomizeSeed => randomizeSeed; // 시드 변경 여부 반환
        public int StartingQuarterTurns => startingQuarterTurns; // 첫 모듈 직각 회전 횟수 반환
        public int ModuleCount => moduleCount; // 목표 모듈 개수 반환
        public int MaximumPlacementAttempts => maximumPlacementAttempts; // 최대 연결 조합 시도 횟수 반환
        public float OverlapTolerance => overlapTolerance; // 겹침 허용 크기 반환
        public float ConnectionSizeTolerance => connectionSizeTolerance; // 연결부 크기 허용 오차 반환
        public float ConnectionPositionTolerance => connectionPositionTolerance; // 연결부 위치 허용 오차 반환
        public bool UseBranchingPath => useBranchingPath; // 분기 경로 사용 여부 반환
        public int BranchPairCount => branchPairCount; // 병렬 경로 모듈 쌍 개수 반환
        public bool UseVerticalGeneration => useVerticalGeneration; // 수직 생성 사용 여부 반환
        public float MinimumTargetHeight => minimumTargetHeight; // 최소 목표 높이 반환
        public float MaximumTargetHeight => maximumTargetHeight; // 최대 목표 높이 반환
        public int MinimumAscendingModules => minimumAscendingModules; // 최소 상승 모듈 개수 반환
        public int MaximumConsecutiveFlatModules => maximumConsecutiveFlatModules; // 최대 연속 평지 모듈 개수 반환
        public bool AllowDescendingModules => allowDescendingModules; // 하강 모듈 허용 여부 반환
        public int MinimumAscendingModulesPerBranch => minimumAscendingModulesPerBranch; // 분기별 최소 상승 모듈 개수 반환
        public float BranchMergeHeightTolerance => branchMergeHeightTolerance; // 합류 높이 허용 오차 반환
        public int MaximumBranchCombinationRetries => maximumBranchCombinationRetries; // 분기 조합 최대 재시도 횟수 반환
        public MapModuleDefinition[] ModulePrefabs => modulePrefabs; // 후보 모듈 Prefab 목록 반환

        private void OnValidate() // Inspector 설정값 보정
        { // 설정값 보정 처리
            startingQuarterTurns = ((startingQuarterTurns % 4) + 4) % 4; // 첫 회전 횟수 0부터 3까지 보정
            moduleCount = Mathf.Max(useBranchingPath ? MinimumBranchedModuleCount : 1, moduleCount); // 생성 방식별 최소 모듈 개수 보장
            maximumPlacementAttempts = Mathf.Max(1, maximumPlacementAttempts); // 배치 시도 횟수 최소값 보장
            overlapTolerance = Mathf.Max(0f, overlapTolerance); // 겹침 허용 크기 음수 방지
            connectionSizeTolerance = Mathf.Max(0f, connectionSizeTolerance); // 연결부 크기 허용 오차 음수 방지
            connectionPositionTolerance = Mathf.Max(0f, connectionPositionTolerance); // 연결부 위치 허용 오차 음수 방지
            branchPairCount = Mathf.Max(1, branchPairCount); // 병렬 경로 쌍 최소값 보장
            minimumTargetHeight = Mathf.Max(0f, minimumTargetHeight); // 최소 목표 높이 음수 방지
            maximumTargetHeight = Mathf.Max(minimumTargetHeight, maximumTargetHeight); // 최대 목표 높이를 최소 목표 이상으로 보정
            minimumAscendingModules = Mathf.Clamp(minimumAscendingModules, 0, moduleCount); // 최소 상승 모듈 수를 전체 모듈 수 안으로 보정
            maximumConsecutiveFlatModules = Mathf.Max(0, maximumConsecutiveFlatModules); // 연속 평지 제한 음수 방지
            minimumAscendingModulesPerBranch = Mathf.Clamp(minimumAscendingModulesPerBranch, 0, branchPairCount); // 분기별 상승 수를 병렬 단계 수 안으로 보정
            branchMergeHeightTolerance = Mathf.Max(0f, branchMergeHeightTolerance); // 합류 높이 허용 오차 음수 방지
            maximumBranchCombinationRetries = Mathf.Max(1, maximumBranchCombinationRetries); // 분기 조합 재시도 최소값 보장
        } // 설정값 보정 처리

#if UNITY_EDITOR // Unity Editor 전용 설정
        public void ConfigureForEditor(int newSeed, bool newRandomizeSeed, int newStartingQuarterTurns, int newModuleCount, int newMaximumPlacementAttempts, float newOverlapTolerance, MapModuleDefinition[] newModulePrefabs) // 기존 Editor 도구용 생성 설정 적용
        { // 기존 Editor 설정 적용 처리
            ConfigureForEditor(newSeed, newRandomizeSeed, newStartingQuarterTurns, newModuleCount, newMaximumPlacementAttempts, newOverlapTolerance, 0.05f, 0.02f, false, 2, newModulePrefabs); // 31일차 선형 생성 호환 설정으로 전달
        } // 기존 Editor 설정 적용 처리

        public void ConfigureForEditor(int newSeed, bool newRandomizeSeed, int newStartingQuarterTurns, int newModuleCount, int newMaximumPlacementAttempts, float newOverlapTolerance, float newConnectionSizeTolerance, float newConnectionPositionTolerance, bool newUseBranchingPath, int newBranchPairCount, MapModuleDefinition[] newModulePrefabs) // Editor 도구용 전체 생성 설정 적용
        { // Editor 전체 설정 적용 처리
            seed = newSeed; // 새 고정 시드 저장
            randomizeSeed = newRandomizeSeed; // 새 시드 변경 여부 저장
            startingQuarterTurns = newStartingQuarterTurns; // 새 첫 모듈 회전 횟수 저장
            moduleCount = newModuleCount; // 새 목표 모듈 개수 저장
            maximumPlacementAttempts = newMaximumPlacementAttempts; // 새 최대 배치 시도 횟수 저장
            overlapTolerance = newOverlapTolerance; // 새 겹침 허용 크기 저장
            connectionSizeTolerance = newConnectionSizeTolerance; // 새 연결부 크기 허용 오차 저장
            connectionPositionTolerance = newConnectionPositionTolerance; // 새 연결부 위치 허용 오차 저장
            useBranchingPath = newUseBranchingPath; // 새 분기 경로 사용 여부 저장
            branchPairCount = newBranchPairCount; // 새 병렬 경로 쌍 개수 저장
            useVerticalGeneration = false; // 기존 Editor 도구의 수평 생성 방식 유지
            modulePrefabs = newModulePrefabs; // 새 후보 모듈 목록 저장
            OnValidate(); // 설정값 즉시 보정
        } // Editor 전체 설정 적용 처리

        public void ConfigureVerticalForEditor(int newSeed, bool newRandomizeSeed, int newStartingQuarterTurns, int newModuleCount, int newMaximumPlacementAttempts, float newOverlapTolerance, float newConnectionSizeTolerance, float newConnectionPositionTolerance, float newMinimumTargetHeight, float newMaximumTargetHeight, int newMinimumAscendingModules, int newMaximumConsecutiveFlatModules, bool newAllowDescendingModules, MapModuleDefinition[] newModulePrefabs) // Editor 도구용 수직 생성 설정 적용
        { // Editor 수직 생성 설정 적용 처리
            seed = newSeed; // 새 고정 시드 저장
            randomizeSeed = newRandomizeSeed; // 새 시드 변경 여부 저장
            startingQuarterTurns = newStartingQuarterTurns; // 새 첫 모듈 회전 횟수 저장
            moduleCount = newModuleCount; // 새 목표 모듈 개수 저장
            maximumPlacementAttempts = newMaximumPlacementAttempts; // 새 최대 배치 시도 횟수 저장
            overlapTolerance = newOverlapTolerance; // 새 겹침 허용 크기 저장
            connectionSizeTolerance = newConnectionSizeTolerance; // 새 연결부 크기 허용 오차 저장
            connectionPositionTolerance = newConnectionPositionTolerance; // 새 연결부 위치 허용 오차 저장
            useBranchingPath = false; // 수직 생성의 분기 경로 비활성화
            branchPairCount = 2; // 기존 분기 설정 안전 기본값 저장
            useVerticalGeneration = true; // 수직 생성 활성화
            minimumTargetHeight = newMinimumTargetHeight; // 새 최소 목표 높이 저장
            maximumTargetHeight = newMaximumTargetHeight; // 새 최대 목표 높이 저장
            minimumAscendingModules = newMinimumAscendingModules; // 새 최소 상승 모듈 수 저장
            maximumConsecutiveFlatModules = newMaximumConsecutiveFlatModules; // 새 연속 평지 제한 저장
            allowDescendingModules = newAllowDescendingModules; // 새 하강 모듈 허용 여부 저장
            modulePrefabs = newModulePrefabs; // 새 후보 모듈 목록 저장
            OnValidate(); // 수직 설정값 즉시 보정
        } // Editor 수직 생성 설정 적용 처리 종료

        public void ConfigureVerticalBranchingForEditor(int newSeed, bool newRandomizeSeed, int newStartingQuarterTurns, int newModuleCount, int newMaximumPlacementAttempts, float newOverlapTolerance, float newConnectionSizeTolerance, float newConnectionPositionTolerance, int newBranchPairCount, float newMinimumTargetHeight, float newMaximumTargetHeight, int newMinimumAscendingModules, int newMaximumConsecutiveFlatModules, bool newAllowDescendingModules, int newMinimumAscendingModulesPerBranch, float newBranchMergeHeightTolerance, int newMaximumBranchCombinationRetries, MapModuleDefinition[] newModulePrefabs) // Editor 도구용 수직 분기 생성 설정 적용
        { // Editor 수직 분기 생성 설정 적용 처리
            seed = newSeed; // 새 고정 시드 저장
            randomizeSeed = newRandomizeSeed; // 새 시드 변경 여부 저장
            startingQuarterTurns = newStartingQuarterTurns; // 새 첫 모듈 회전 횟수 저장
            moduleCount = newModuleCount; // 새 목표 모듈 개수 저장
            maximumPlacementAttempts = newMaximumPlacementAttempts; // 새 최대 배치 시도 횟수 저장
            overlapTolerance = newOverlapTolerance; // 새 겹침 허용 크기 저장
            connectionSizeTolerance = newConnectionSizeTolerance; // 새 연결부 크기 허용 오차 저장
            connectionPositionTolerance = newConnectionPositionTolerance; // 새 연결부 위치 허용 오차 저장
            useBranchingPath = true; // 수직 분기 경로 활성화
            branchPairCount = newBranchPairCount; // 새 병렬 경로 쌍 개수 저장
            useVerticalGeneration = true; // 수직 생성 활성화
            minimumTargetHeight = newMinimumTargetHeight; // 새 최소 목표 높이 저장
            maximumTargetHeight = newMaximumTargetHeight; // 새 최대 목표 높이 저장
            minimumAscendingModules = newMinimumAscendingModules; // 새 경로 최소 상승 모듈 수 저장
            maximumConsecutiveFlatModules = newMaximumConsecutiveFlatModules; // 새 연속 평지 제한 저장
            allowDescendingModules = newAllowDescendingModules; // 새 하강 모듈 허용 여부 저장
            minimumAscendingModulesPerBranch = newMinimumAscendingModulesPerBranch; // 새 분기별 최소 상승 모듈 수 저장
            branchMergeHeightTolerance = newBranchMergeHeightTolerance; // 새 합류 높이 허용 오차 저장
            maximumBranchCombinationRetries = newMaximumBranchCombinationRetries; // 새 분기 조합 재시도 횟수 저장
            modulePrefabs = newModulePrefabs; // 새 후보 모듈 목록 저장
            OnValidate(); // 수직 분기 설정값 즉시 보정
        } // Editor 수직 분기 생성 설정 적용 처리 종료
#endif // Unity Editor 전용 설정
    } // 맵 생성 공통 설정 묶음
} // 맵 생성 기능 묶음
