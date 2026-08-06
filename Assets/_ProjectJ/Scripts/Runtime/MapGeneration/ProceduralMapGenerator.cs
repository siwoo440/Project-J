using System; // 시스템 난수 기능 참조
using System.Collections.Generic; // 목록 기능 참조
using UnityEngine; // Unity 오브젝트 생성 기능 참조

namespace ProjectJ.MapGeneration // 맵 생성 기능 네임스페이스 선언
{ // 맵 생성 기능 묶음
    [DisallowMultipleComponent] // 맵 생성기 컴포넌트 중복 방지
    public sealed class ProceduralMapGenerator : MonoBehaviour // 시드 기반 선형 맵 생성기 선언
    { // 시드 기반 선형 맵 생성기 묶음
        [SerializeField] private MapGenerationSettings settings; // 맵 생성 설정 에셋
        [SerializeField] private Transform generatedRoot; // 생성 모듈 보관 루트
        [SerializeField] private bool generateOnStart = true; // 게임 시작 자동 생성 여부
        [SerializeField] private bool logDetailedResults = true; // 생성 결과 로그 표시 여부

        private readonly List<MapModuleDefinition> generatedModules = new List<MapModuleDefinition>(); // 현재 생성된 모듈 목록
        private int effectiveSeed; // 이번 생성에 사용한 시드
        private bool lastGenerationSucceeded; // 최근 생성 성공 여부

        public int EffectiveSeed => effectiveSeed; // 실제 사용 시드 반환
        public bool LastGenerationSucceeded => lastGenerationSucceeded; // 최근 생성 성공 여부 반환
        public int GeneratedModuleCount => generatedModules.Count; // 현재 생성 모듈 개수 반환
        public IReadOnlyList<MapModuleDefinition> GeneratedModules => generatedModules; // 현재 생성 모듈 목록 반환

        private struct PlacementOption // 단일 모듈 배치 후보 선언
        { // 단일 모듈 배치 후보 묶음
            public MapModuleDefinition Prefab; // 후보 모듈 Prefab
            public int QuarterTurns; // 후보 직각 회전 횟수
        } // 단일 모듈 배치 후보 묶음

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

            if (settings == null) // 생성 설정 누락 확인
            { // 생성 설정 누락 처리
                Debug.LogError("[ProjectJ][Day31] Map Generation Settings가 연결되지 않았습니다.", this); // 생성 설정 누락 오류 출력
                return; // 맵 생성 중단
            } // 생성 설정 누락 처리

            List<MapModuleDefinition> validPrefabs = CollectValidPrefabs(); // 유효한 후보 Prefab 수집

            if (validPrefabs.Count == 0) // 유효한 후보 없음 확인
            { // 유효한 후보 없음 처리
                Debug.LogError("[ProjectJ][Day31] 생성 가능한 맵 모듈 Prefab이 없습니다.", this); // 후보 없음 오류 출력
                return; // 맵 생성 중단
            } // 유효한 후보 없음 처리

            EnsureGeneratedRoot(); // 생성 모듈 루트 존재 보장
            ClearGeneratedMap(); // 이전 생성 결과 제거
            effectiveSeed = settings.RandomizeSeed ? Environment.TickCount : settings.Seed; // 이번 생성 시드 결정
            System.Random random = new System.Random(effectiveSeed); // 독립 난수 생성기 준비
            MapModuleDefinition firstModule = CreateFirstModule(validPrefabs, random); // 첫 모듈 생성

            if (firstModule == null) // 첫 모듈 생성 실패 확인
            { // 첫 모듈 생성 실패 처리
                Debug.LogError("[ProjectJ][Day31] 첫 번째 맵 모듈을 생성하지 못했습니다.", this); // 첫 모듈 생성 오류 출력
                return; // 맵 생성 중단
            } // 첫 모듈 생성 실패 처리

            generatedModules.Add(firstModule); // 첫 모듈 목록 등록

            while (generatedModules.Count < settings.ModuleCount) // 목표 모듈 개수까지 반복
            { // 후속 모듈 생성 처리
                MapModuleDefinition previousModule = generatedModules[generatedModules.Count - 1]; // 직전 모듈 조회

                if (!TryCreateNextModule(previousModule, validPrefabs, random, out MapModuleDefinition nextModule)) // 다음 모듈 배치 시도
                { // 다음 모듈 배치 실패 처리
                    Debug.LogWarning($"[ProjectJ][Day31] {generatedModules.Count}번째 연결 뒤에 모듈을 배치하지 못했습니다. 시드: {effectiveSeed}", this); // 배치 실패 경고 출력
                    break; // 후속 생성 중단
                } // 다음 모듈 배치 실패 처리

                generatedModules.Add(nextModule); // 배치된 모듈 목록 등록
            } // 후속 모듈 생성 처리

            lastGenerationSucceeded = generatedModules.Count == settings.ModuleCount; // 목표 개수 달성 여부 저장

            if (logDetailedResults) // 상세 로그 표시 활성 확인
            { // 상세 생성 결과 출력 처리
                string resultLabel = lastGenerationSucceeded ? "성공" : "부분 성공"; // 생성 결과 문구 계산
                Debug.Log($"[ProjectJ][Day31] 맵 생성 {resultLabel} | 시드: {effectiveSeed} | 모듈: {generatedModules.Count}/{settings.ModuleCount}", this); // 생성 결과 로그 출력
            } // 상세 생성 결과 출력 처리
        } // 새 시드 맵 생성 처리

        [ContextMenu("Clear Generated Map")] // Inspector 생성 맵 제거 메뉴 등록
        public void ClearGeneratedMap() // 현재 생성된 맵 전체 제거
        { // 생성 맵 제거 처리
            generatedModules.Clear(); // 생성 모듈 목록 초기화

            if (generatedRoot == null) // 생성 루트 누락 확인
            { // 생성 루트 누락 처리
                return; // 자식 제거 생략
            } // 생성 루트 누락 처리

            for (int childIndex = generatedRoot.childCount - 1; childIndex >= 0; childIndex--) // 모든 생성 자식 역순 순회
            { // 생성 자식 제거 처리
                GameObject childObject = generatedRoot.GetChild(childIndex).gameObject; // 현재 생성 자식 조회

                if (Application.isPlaying) // Play Mode 여부 확인
                { // Play Mode 제거 처리
                    childObject.SetActive(false); // 제거 대기 오브젝트 비활성화
                    Destroy(childObject); // 프레임 종료 시 오브젝트 제거
                } // Play Mode 제거 처리
                else // Edit Mode 여부 확인
                { // Edit Mode 제거 처리
                    DestroyImmediate(childObject); // 생성 오브젝트 즉시 제거
                } // Edit Mode 제거 처리
            } // 생성 자식 제거 처리
        } // 생성 맵 제거 처리

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
                    Debug.LogWarning($"[ProjectJ][Day31] {prefab.name} 제외: {reason}", prefab); // 후보 제외 사유 출력
                    continue; // 현재 후보 제외
                } // 잘못된 모듈 처리

                if (MapGenerationRules.GetAllowedQuarterTurns(prefab.AllowedRotations).Length == 0) // 허용 회전 누락 확인
                { // 허용 회전 누락 처리
                    continue; // 현재 후보 제외
                } // 허용 회전 누락 처리

                validPrefabs.Add(prefab); // 유효 후보 목록 등록
            } // 후보 Prefab 검사 처리

            return validPrefabs; // 유효 후보 목록 반환
        } // 유효 후보 Prefab 수집 처리

        private MapModuleDefinition CreateFirstModule(List<MapModuleDefinition> validPrefabs, System.Random random) // 첫 번째 모듈 생성
        { // 첫 번째 모듈 생성 처리
            MapModuleDefinition selectedPrefab = validPrefabs[random.Next(validPrefabs.Count)]; // 첫 후보 Prefab 무작위 선택
            int[] allowedQuarterTurns = MapGenerationRules.GetAllowedQuarterTurns(selectedPrefab.AllowedRotations); // 허용 회전 목록 조회
            int selectedQuarterTurns = MapGenerationRules.IsRotationAllowed(selectedPrefab.AllowedRotations, settings.StartingQuarterTurns) ? settings.StartingQuarterTurns : allowedQuarterTurns[0]; // 설정 회전 또는 첫 허용 회전 선택
            MapModuleDefinition instance = Instantiate(selectedPrefab, generatedRoot); // 첫 모듈 인스턴스 생성
            instance.name = $"{selectedPrefab.ModuleId}_00"; // 첫 모듈 이름 적용
            instance.transform.localPosition = Vector3.zero; // 생성 루트 원점 배치
            instance.transform.localRotation = MapGenerationRules.QuarterTurnRotation(selectedQuarterTurns); // 허용 직각 회전 적용
            return instance; // 생성된 첫 모듈 반환
        } // 첫 번째 모듈 생성 처리

        private bool TryCreateNextModule(MapModuleDefinition previousModule, List<MapModuleDefinition> validPrefabs, System.Random random, out MapModuleDefinition createdModule) // 직전 모듈 뒤에 다음 모듈 생성
        { // 다음 모듈 생성 처리
            createdModule = null; // 생성 결과 초기화
            MapModuleConnectionPoint previousExit = ChooseRandomConnection(previousModule, MapConnectionRole.Exit, random); // 직전 모듈 출구 선택

            if (previousExit == null) // 직전 출구 누락 확인
            { // 직전 출구 누락 처리
                return false; // 다음 모듈 생성 실패 반환
            } // 직전 출구 누락 처리

            List<PlacementOption> placementOptions = BuildPlacementOptions(validPrefabs); // 전체 Prefab과 회전 조합 생성
            ShufflePlacementOptions(placementOptions, random); // 시드 기반 후보 순서 섞기
            int attemptCount = Mathf.Min(settings.MaximumPlacementAttempts, placementOptions.Count); // 실제 배치 시도 횟수 계산

            for (int attemptIndex = 0; attemptIndex < attemptCount; attemptIndex++) // 배치 후보 순서대로 시도
            { // 단일 후보 배치 처리
                PlacementOption option = placementOptions[attemptIndex]; // 현재 배치 후보 조회
                MapModuleDefinition candidate = Instantiate(option.Prefab, generatedRoot); // 후보 모듈 인스턴스 생성
                candidate.name = $"{option.Prefab.ModuleId}_{generatedModules.Count:00}"; // 후보 모듈 순서 이름 적용
                candidate.transform.localPosition = Vector3.zero; // 후보 모듈 임시 원점 배치
                candidate.transform.localRotation = MapGenerationRules.QuarterTurnRotation(option.QuarterTurns); // 후보 직각 회전 적용
                MapModuleConnectionPoint entrance = ChooseCompatibleEntrance(candidate, previousExit.WorldDirection, random); // 직전 출구 호환 입구 선택

                if (entrance == null) // 호환 입구 누락 확인
                { // 호환 입구 누락 처리
                    DestroyCandidate(candidate); // 현재 후보 제거
                    continue; // 다음 후보 시도
                } // 호환 입구 누락 처리

                candidate.transform.position = MapGenerationRules.CalculateAlignedRootPosition(candidate.transform.position, previousExit.transform.position, entrance.transform.position); // 두 연결 지점 위치 일치

                if (OverlapsPlacedModule(candidate)) // 기존 모듈 실제 겹침 확인
                { // 기존 모듈 겹침 처리
                    DestroyCandidate(candidate); // 겹친 후보 제거
                    continue; // 다음 후보 시도
                } // 기존 모듈 겹침 처리

                createdModule = candidate; // 생성 성공 모듈 저장
                return true; // 다음 모듈 생성 성공 반환
            } // 단일 후보 배치 처리

            return false; // 모든 후보 배치 실패 반환
        } // 다음 모듈 생성 처리

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

        private void ShufflePlacementOptions(List<PlacementOption> options, System.Random random) // 시드 기반 배치 후보 순서 섞기
        { // 배치 후보 순서 섞기 처리
            for (int currentIndex = options.Count - 1; currentIndex > 0; currentIndex--) // 뒤에서 두 번째 위치까지 역순 순회
            { // Fisher-Yates 순서 섞기 처리
                int randomIndex = random.Next(currentIndex + 1); // 교환 대상 무작위 위치 계산
                PlacementOption temporaryOption = options[currentIndex]; // 현재 후보 임시 저장
                options[currentIndex] = options[randomIndex]; // 무작위 후보를 현재 위치로 이동
                options[randomIndex] = temporaryOption; // 기존 현재 후보를 무작위 위치로 이동
            } // Fisher-Yates 순서 섞기 처리
        } // 배치 후보 순서 섞기 처리

        private MapModuleConnectionPoint ChooseRandomConnection(MapModuleDefinition module, MapConnectionRole role, System.Random random) // 지정 역할 연결 지점 무작위 선택
        { // 연결 지점 무작위 선택 처리
            List<MapModuleConnectionPoint> matches = new List<MapModuleConnectionPoint>(); // 역할 일치 연결 목록 생성
            MapModuleConnectionPoint[] points = module.ConnectionPoints; // 모듈 연결 지점 목록 조회

            for (int pointIndex = 0; pointIndex < points.Length; pointIndex++) // 모든 연결 지점 순회
            { // 연결 역할 검사 처리
                MapModuleConnectionPoint point = points[pointIndex]; // 현재 연결 지점 조회

                if (point != null && point.Role == role) // 연결 지점과 역할 일치 확인
                { // 역할 일치 연결 처리
                    matches.Add(point); // 일치 연결 목록 등록
                } // 역할 일치 연결 처리
            } // 연결 역할 검사 처리

            return matches.Count > 0 ? matches[random.Next(matches.Count)] : null; // 무작위 일치 연결 또는 빈 결과 반환
        } // 연결 지점 무작위 선택 처리

        private MapModuleConnectionPoint ChooseCompatibleEntrance(MapModuleDefinition module, Vector3 previousExitDirection, System.Random random) // 직전 출구와 호환되는 입구 선택
        { // 호환 입구 선택 처리
            List<MapModuleConnectionPoint> matches = new List<MapModuleConnectionPoint>(); // 호환 입구 목록 생성
            MapModuleConnectionPoint[] points = module.ConnectionPoints; // 후보 모듈 연결 지점 목록 조회

            for (int pointIndex = 0; pointIndex < points.Length; pointIndex++) // 모든 후보 연결 지점 순회
            { // 후보 연결 지점 검사 처리
                MapModuleConnectionPoint point = points[pointIndex]; // 현재 후보 연결 지점 조회

                if (point == null || point.Role != MapConnectionRole.Entrance) // 입구가 아닌 지점 확인
                { // 입구 외 지점 처리
                    continue; // 현재 지점 제외
                } // 입구 외 지점 처리

                if (MapGenerationRules.AreWorldDirectionsOpposite(previousExitDirection, point.WorldDirection)) // 직전 출구 반대 방향 확인
                { // 호환 입구 처리
                    matches.Add(point); // 호환 입구 목록 등록
                } // 호환 입구 처리
            } // 후보 연결 지점 검사 처리

            return matches.Count > 0 ? matches[random.Next(matches.Count)] : null; // 무작위 호환 입구 또는 빈 결과 반환
        } // 호환 입구 선택 처리

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
    } // 시드 기반 선형 맵 생성기 묶음
} // 맵 생성 기능 묶음
