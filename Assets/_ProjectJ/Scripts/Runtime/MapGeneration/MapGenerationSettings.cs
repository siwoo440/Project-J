using UnityEngine; // Unity 데이터 에셋 기능 참조

namespace ProjectJ.MapGeneration // 맵 생성 기능 네임스페이스 선언
{ // 맵 생성 기능 묶음
    [CreateAssetMenu(fileName = "MAP-GEN-001_DefaultGenerationSettings", menuName = "Project J/Map Generation/Generation Settings")] // 맵 생성 설정 에셋 메뉴 등록
    public sealed class MapGenerationSettings : ScriptableObject // 맵 생성 공통 설정 선언
    { // 맵 생성 공통 설정 묶음
        [SerializeField] private int seed = 31001; // 고정 생성 시드
        [SerializeField] private bool randomizeSeed; // 실행마다 시드 변경 여부
        [SerializeField, Range(0, 3)] private int startingQuarterTurns; // 첫 모듈 직각 회전 횟수
        [SerializeField, Min(1)] private int moduleCount = 8; // 목표 모듈 개수
        [SerializeField, Min(1)] private int maximumPlacementAttempts = 32; // 배치 위치별 최대 시도 횟수
        [SerializeField, Min(0f)] private float overlapTolerance = 0.05f; // 맞닿은 경계 허용 크기
        [SerializeField] private MapModuleDefinition[] modulePrefabs; // 생성 후보 모듈 Prefab 목록

        public int Seed => seed; // 고정 생성 시드 반환
        public bool RandomizeSeed => randomizeSeed; // 시드 변경 여부 반환
        public int StartingQuarterTurns => startingQuarterTurns; // 첫 모듈 직각 회전 횟수 반환
        public int ModuleCount => moduleCount; // 목표 모듈 개수 반환
        public int MaximumPlacementAttempts => maximumPlacementAttempts; // 최대 배치 시도 횟수 반환
        public float OverlapTolerance => overlapTolerance; // 겹침 허용 크기 반환
        public MapModuleDefinition[] ModulePrefabs => modulePrefabs; // 후보 모듈 Prefab 목록 반환

        private void OnValidate() // Inspector 설정값 보정
        { // 설정값 보정 처리
            startingQuarterTurns = ((startingQuarterTurns % 4) + 4) % 4; // 첫 회전 횟수 0부터 3까지 보정
            moduleCount = Mathf.Max(1, moduleCount); // 모듈 개수 최소값 보장
            maximumPlacementAttempts = Mathf.Max(1, maximumPlacementAttempts); // 배치 시도 횟수 최소값 보장
            overlapTolerance = Mathf.Max(0f, overlapTolerance); // 겹침 허용 크기 음수 방지
        } // 설정값 보정 처리

#if UNITY_EDITOR // Unity Editor 전용 설정
        public void ConfigureForEditor(int newSeed, bool newRandomizeSeed, int newStartingQuarterTurns, int newModuleCount, int newMaximumPlacementAttempts, float newOverlapTolerance, MapModuleDefinition[] newModulePrefabs) // Editor 도구용 생성 설정 적용
        { // Editor 설정 적용 처리
            seed = newSeed; // 새 고정 시드 저장
            randomizeSeed = newRandomizeSeed; // 새 시드 변경 여부 저장
            startingQuarterTurns = newStartingQuarterTurns; // 새 첫 모듈 회전 횟수 저장
            moduleCount = newModuleCount; // 새 목표 모듈 개수 저장
            maximumPlacementAttempts = newMaximumPlacementAttempts; // 새 최대 배치 시도 횟수 저장
            overlapTolerance = newOverlapTolerance; // 새 겹침 허용 크기 저장
            modulePrefabs = newModulePrefabs; // 새 후보 모듈 목록 저장
            OnValidate(); // 설정값 즉시 보정
        } // Editor 설정 적용 처리
#endif // Unity Editor 전용 설정
    } // 맵 생성 공통 설정 묶음
} // 맵 생성 기능 묶음
