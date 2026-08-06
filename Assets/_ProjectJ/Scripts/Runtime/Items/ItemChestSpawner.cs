using System.Collections.Generic; // 생성 지점 목록 기능 참조
using ProjectJ.Data; // 아이템 공통 데이터 형식 참조
using ProjectJ.Gameplay; // 경기 종료 상태 기능 참조
using ProjectJ.MapGeneration; // 절차 맵 모듈과 생성기 기능 참조
using UnityEngine; // Unity 오브젝트와 난수 기능 참조

namespace ProjectJ.Items // 프로젝트 아이템 기능 네임스페이스 선언
{ // 프로젝트 아이템 기능 묶음
    [DisallowMultipleComponent] // 상자 생성기 컴포넌트 중복 방지
    public sealed class ItemChestSpawner : MonoBehaviour // 절차 맵 기반 아이템 상자 생성기 선언
    { // 절차 맵 기반 아이템 상자 생성기 묶음
        [Header("데이터 제공자")] // 데이터 제공자 Inspector 구분
        [SerializeField] private ProceduralMapGenerator mapGenerator; // 생성 완료 맵 모듈 제공자 저장
        [SerializeField] private ItemPlacementValidator placementValidator; // 공통 설치 위치 검사기 저장
        [SerializeField] private PrototypeMatchController matchController; // 경기 종료 상태 제공자 저장
        [SerializeField] private ItemDataDefinition[] itemPool; // 상자 지급 가능 아이템 목록 저장

        [Header("생성 규칙")] // 상자 생성 규칙 Inspector 구분
        [SerializeField, Range(0f, 1f)] private float generationProbability = 0.35f; // 대상 모듈별 상자 생성 확률
        [SerializeField, Min(1)] private int maximumChestCount = 4; // 맵 전체 최대 상자 지점 수
        [SerializeField, Min(1)] private int minimumModuleGap = 2; // 상자 사이 최소 모듈 번호 간격
        [SerializeField, Min(1)] private int candidateAttemptsPerModule = 8; // 모듈당 위치 후보 검사 횟수
        [SerializeField, Min(0f)] private float moduleEdgePadding = 0.9f; // 모듈 가장자리 제외 여백
        [SerializeField] private Vector3 chestHalfExtents = new Vector3(0.6f, 0.6f, 0.6f); // 상자 설치 공간 절반 크기

        [Header("재생성 규칙")] // 상자 재생성 규칙 Inspector 구분
        [SerializeField, Min(0f)] private float respawnDelay = 20f; // 획득 후 같은 지점 재생성 대기 시간
        [SerializeField, Min(0)] private int maximumRespawnCountPerPoint = 1; // 지점별 추가 재생성 최대 횟수
        [SerializeField] private bool logGenerationResult = true; // 생성 결과 Console 출력 여부

        private readonly List<ItemChestSpawnPoint> spawnPoints = new List<ItemChestSpawnPoint>(); // 현재 맵 상자 생성 지점 목록
        private Transform generatedChestRoot; // 런타임 상자 지점 보관 루트
        private string appliedGenerationSignature = string.Empty; // 상자 배치를 적용한 맵 생성 서명
        private int appliedFirstModuleInstanceId; // 상자 배치를 적용한 첫 모듈 인스턴스 번호

        public int SpawnPointCount => spawnPoints.Count; // 현재 생성 지점 수 반환
        public string AppliedGenerationSignature => appliedGenerationSignature; // 적용 완료 맵 서명 반환

        private void Awake() // 실행 시작 시 필수 참조 자동 연결
        { // 필수 참조 준비 처리
            ResolveReferences(); // Scene에서 누락된 참조 자동 검색
            EnsureGeneratedChestRoot(); // 생성 상자 전용 루트 보장
        } // 필수 참조 준비 처리 종료

        private void Update() // 맵 생성 완료와 재생성 상태 감시
        { // 맵 생성 상태 감시 처리
            TryRefreshForGeneratedMap(); // 새 맵 생성 서명 확인 후 상자 지점 갱신
        } // 맵 생성 상태 감시 처리 종료

        private void ResolveReferences() // Scene 기반 누락 참조 자동 연결
        { // 누락 참조 자동 연결 처리
            if (mapGenerator == null) // 맵 생성기 참조 누락 여부 확인
            { // 맵 생성기 자동 검색 처리
                mapGenerator = FindFirstObjectByType<ProceduralMapGenerator>(); // 현재 Scene 맵 생성기 저장
            } // 맵 생성기 자동 검색 처리 종료

            if (placementValidator == null) // 공통 위치 검사기 참조 누락 여부 확인
            { // 공통 위치 검사기 자동 검색 처리
                placementValidator = GetComponent<ItemPlacementValidator>(); // 같은 오브젝트 검사기 저장
            } // 공통 위치 검사기 자동 검색 처리 종료

            if (matchController == null) // 경기 관리자 참조 누락 여부 확인
            { // 경기 관리자 자동 검색 처리
                matchController = FindFirstObjectByType<PrototypeMatchController>(); // 현재 Scene 경기 관리자 저장
            } // 경기 관리자 자동 검색 처리 종료
        } // 누락 참조 자동 연결 처리 종료

        private void TryRefreshForGeneratedMap() // 유효한 새 맵 생성 결과에 상자 배치 적용
        { // 새 맵 상자 배치 갱신 처리
            if (mapGenerator == null || placementValidator == null) // 필수 생성기와 검사기 누락 여부 확인
            { // 상자 배치 불가 처리
                return; // 새 맵 상자 배치 생략
            } // 상자 배치 불가 처리 종료

            if (!mapGenerator.LastGenerationSucceeded || mapGenerator.GeneratedModuleCount < 3) // 맵 생성 실패와 모듈 부족 여부 확인
            { // 유효 맵 대기 처리
                return; // 맵 생성 완료 전 상자 배치 생략
            } // 유효 맵 대기 처리 종료

            string currentSignature = mapGenerator.GenerationSignature; // 현재 맵 생성 서명 조회
            MapModuleDefinition firstModule = mapGenerator.GeneratedModules[0]; // 현재 맵 첫 모듈 조회
            int currentFirstModuleInstanceId = firstModule != null ? firstModule.GetInstanceID() : 0; // 동일 시드 재생성 구분용 인스턴스 번호 계산

            if (string.IsNullOrWhiteSpace(currentSignature) || currentSignature == appliedGenerationSignature && currentFirstModuleInstanceId == appliedFirstModuleInstanceId) // 빈 서명과 동일 맵 인스턴스 여부 확인
            { // 상자 배치 갱신 생략 처리
                return; // 동일 맵 중복 배치 방지
            } // 상자 배치 갱신 생략 처리 종료

            RebuildSpawnPoints(currentSignature, currentFirstModuleInstanceId); // 새 맵 기준 상자 생성 지점 전체 재구성
        } // 새 맵 상자 배치 갱신 처리 종료

        private void RebuildSpawnPoints(string currentSignature, int currentFirstModuleInstanceId) // 새 맵 모듈 기준 모든 상자 지점 재구성
        { // 상자 지점 재구성 처리
            ClearSpawnPoints(); // 이전 맵 상자 지점 전체 제거
            EnsureGeneratedChestRoot(); // 새 상자 지점 보관 루트 보장
            System.Random random = new System.Random(mapGenerator.EffectiveSeed ^ 0x41C57); // 맵 시드 기반 상자 전용 결정적 난수 생성
            IReadOnlyList<MapModuleDefinition> modules = mapGenerator.GeneratedModules; // 생성 완료 맵 모듈 목록 조회
            int previousSpawnModuleIndex = -1; // 직전 상자 모듈 번호 초기화

            for (int moduleIndex = 0; moduleIndex < modules.Count; moduleIndex++) // 생성된 모든 모듈 순회
            { // 현재 모듈 상자 생성 처리
                if (spawnPoints.Count >= Mathf.Max(1, maximumChestCount)) // 맵 최대 상자 수 도달 여부 확인
                { // 최대 상자 수 도달 처리
                    break; // 추가 모듈 상자 생성 중단
                } // 최대 상자 수 도달 처리 종료

                if (!ItemChestSpawnRules.IsEligibleModuleIndex(moduleIndex, modules.Count)) // 시작과 종료 제외 규칙 확인
                { // 제외 모듈 처리
                    continue; // 현재 모듈 상자 생성 생략
                } // 제외 모듈 처리 종료

                if (!ItemChestSpawnRules.HasRequiredModuleGap(previousSpawnModuleIndex, moduleIndex, minimumModuleGap)) // 직전 상자와 최소 모듈 간격 확인
                { // 인접 모듈 중복 방지 처리
                    continue; // 현재 모듈 상자 생성 생략
                } // 인접 모듈 중복 방지 처리 종료

                if (!ItemChestSpawnRules.ShouldSpawn((float)random.NextDouble(), generationProbability)) // 모듈별 확률 판정
                { // 확률 미통과 처리
                    continue; // 현재 모듈 상자 생성 생략
                } // 확률 미통과 처리 종료

                MapModuleDefinition module = modules[moduleIndex]; // 현재 생성 모듈 조회

                if (module == null || !TryFindValidPosition(module, random, out ItemPlacementResult placementResult)) // 모듈과 유효 설치 위치 확인
                { // 설치 위치 미확보 처리
                    continue; // 현재 모듈 상자 생성 생략
                } // 설치 위치 미확보 처리 종료

                CreateSpawnPoint(moduleIndex, placementResult.Position, random.Next()); // 유효 위치에 상자 생성 지점 구성
                previousSpawnModuleIndex = moduleIndex; // 직전 상자 모듈 번호 갱신
            } // 현재 모듈 상자 생성 처리 종료

            appliedGenerationSignature = currentSignature; // 새 맵 상자 배치 완료 서명 저장
            appliedFirstModuleInstanceId = currentFirstModuleInstanceId; // 새 맵 첫 모듈 인스턴스 번호 저장
            LogGenerationResult(modules.Count); // 상자 생성 결과 요약 출력
        } // 상자 지점 재구성 처리 종료

        private bool TryFindValidPosition(MapModuleDefinition module, System.Random random, out ItemPlacementResult result) // 모듈 영역 내부 유효한 상자 위치 검색
        { // 모듈 상자 위치 검색 처리
            Bounds moduleBounds = module.WorldBounds; // 현재 모듈 월드 영역 조회
            int attemptCount = Mathf.Max(1, candidateAttemptsPerModule); // 최소 한 번 후보 검사 보장

            for (int attemptIndex = 0; attemptIndex < attemptCount; attemptIndex++) // 모듈 위치 후보 횟수만큼 반복
            { // 현재 위치 후보 검사 처리
                float xPosition = Mathf.Lerp(moduleBounds.min.x, moduleBounds.max.x, (float)random.NextDouble()); // 모듈 X 범위 무작위 위치 계산
                float zPosition = Mathf.Lerp(moduleBounds.min.z, moduleBounds.max.z, (float)random.NextDouble()); // 모듈 Z 범위 무작위 위치 계산
                Vector3 requestedPosition = new Vector3(xPosition, moduleBounds.max.y, zPosition); // 모듈 상단 기준 지면 검사 후보 생성

                if (placementValidator.TryValidateInsideBounds(requestedPosition, chestHalfExtents, Quaternion.identity, null, moduleBounds, moduleEdgePadding, out result)) // 공통 지면과 경사와 장애물 검사 실행
                { // 유효 위치 발견 처리
                    return true; // 현재 설치 결과 반환
                } // 유효 위치 발견 처리 종료
            } // 현재 위치 후보 검사 처리 종료

            result = ItemPlacementResult.CreateFailure(ItemPlacementFailureReason.NoGround, moduleBounds.center); // 모든 후보 실패 기본 결과 생성
            return false; // 모듈 내 유효 위치 없음 반환
        } // 모듈 상자 위치 검색 처리 종료

        private void CreateSpawnPoint(int moduleIndex, Vector3 worldPosition, int pointSeed) // 유효 위치에 단일 상자 생성 지점 구성
        { // 단일 상자 생성 지점 구성 처리
            GameObject pointObject = new GameObject($"ItemChestSpawnPoint_Module_{moduleIndex:00}"); // 모듈 번호 기반 생성 지점 오브젝트 생성
            pointObject.transform.SetParent(generatedChestRoot, true); // 런타임 상자 루트 아래 지점 배치
            pointObject.transform.position = worldPosition; // 공통 검사 완료 지면 위치 적용
            pointObject.transform.rotation = Quaternion.identity; // 상자 기본 회전 적용
            ItemChestSpawnPoint spawnPoint = pointObject.AddComponent<ItemChestSpawnPoint>(); // 단일 지점 생성과 재생성 기능 추가
            spawnPoint.ConfigureRuntime(itemPool, matchController, respawnDelay, maximumRespawnCountPerPoint, pointSeed); // 아이템 후보와 재생성 규칙 연결
            spawnPoints.Add(spawnPoint); // 현재 맵 생성 지점 목록 등록
        } // 단일 상자 생성 지점 구성 처리 종료

        private void EnsureGeneratedChestRoot() // 런타임 상자 생성 지점 보관 루트 보장
        { // 생성 지점 루트 준비 처리
            if (generatedChestRoot != null) // 기존 루트 참조 존재 여부 확인
            { // 기존 루트 유지 처리
                return; // 새 루트 생성 생략
            } // 기존 루트 유지 처리 종료

            Transform existingRoot = transform.Find("GeneratedItemChests"); // 같은 관리자 아래 기존 루트 검색

            if (existingRoot != null) // 기존 이름 루트 존재 여부 확인
            { // 기존 루트 재사용 처리
                generatedChestRoot = existingRoot; // 기존 Transform 저장
                return; // 새 루트 생성 생략
            } // 기존 루트 재사용 처리 종료

            GameObject rootObject = new GameObject("GeneratedItemChests"); // 새 런타임 상자 루트 생성
            rootObject.transform.SetParent(transform, false); // 상자 생성기 아래 기본 위치 배치
            generatedChestRoot = rootObject.transform; // 새 루트 Transform 저장
        } // 생성 지점 루트 준비 처리 종료

        private void ClearSpawnPoints() // 이전 맵 상자 생성 지점 전체 제거
        { // 이전 상자 지점 제거 처리
            spawnPoints.Clear(); // 생성 지점 목록 초기화

            if (generatedChestRoot != null) // 생성 루트 존재 여부 확인
            { // 미등록 자식 정리 처리
                for (int childIndex = generatedChestRoot.childCount - 1; childIndex >= 0; childIndex--) // 생성 루트 모든 자식 역순 순회
                { // 현재 미등록 자식 제거 처리
                    Destroy(generatedChestRoot.GetChild(childIndex).gameObject); // 이전 상자 지점 자식 제거
                } // 현재 미등록 자식 제거 처리 종료
            } // 미등록 자식 정리 처리 종료
        } // 이전 상자 지점 제거 처리 종료

        private void LogGenerationResult(int moduleCount) // 현재 맵 상자 생성 결과 요약 출력
        { // 상자 생성 결과 로그 처리
            if (!logGenerationResult) // 생성 결과 로그 비활성화 여부 확인
            { // 생성 결과 로그 생략 처리
                return; // 로그 처리 중단
            } // 생성 결과 로그 생략 처리 종료

            Debug.Log($"[ProjectJ][Day41] 상자 생성 완료 | 시드: {mapGenerator.EffectiveSeed} | 모듈: {moduleCount} | 확률: {generationProbability:P0} | 생성 지점: {spawnPoints.Count}/{maximumChestCount} | 지점별 재생성: {maximumRespawnCountPerPoint}회", this); // 상자 생성 규칙과 결과 출력
        } // 상자 생성 결과 로그 처리 종료

#if UNITY_EDITOR // Editor 전용 설정 시작
        public void ConfigureForEditor(ProceduralMapGenerator newMapGenerator, ItemPlacementValidator newPlacementValidator, PrototypeMatchController newMatchController, ItemDataDefinition[] newItemPool, float newGenerationProbability, int newMaximumChestCount, int newMinimumModuleGap, int newCandidateAttemptsPerModule, float newModuleEdgePadding, Vector3 newChestHalfExtents, float newRespawnDelay, int newMaximumRespawnCountPerPoint) // 자동 설정 도구용 상자 생성 규칙 연결
        { // 자동 설정 도구용 상자 생성기 설정 처리
            mapGenerator = newMapGenerator; // 맵 생성기 참조 저장
            placementValidator = newPlacementValidator; // 공통 위치 검사기 참조 저장
            matchController = newMatchController; // 경기 관리자 참조 저장
            itemPool = newItemPool; // 지급 아이템 후보 목록 저장
            generationProbability = ItemChestSpawnRules.ClampProbability(newGenerationProbability); // 생성 확률 보정 후 저장
            maximumChestCount = Mathf.Max(1, newMaximumChestCount); // 최대 상자 수 보정 후 저장
            minimumModuleGap = Mathf.Max(1, newMinimumModuleGap); // 최소 모듈 간격 보정 후 저장
            candidateAttemptsPerModule = Mathf.Max(1, newCandidateAttemptsPerModule); // 위치 후보 검사 횟수 보정 후 저장
            moduleEdgePadding = Mathf.Max(0f, newModuleEdgePadding); // 모듈 가장자리 여백 보정 후 저장
            chestHalfExtents = new Vector3(Mathf.Max(0.01f, newChestHalfExtents.x), Mathf.Max(0.01f, newChestHalfExtents.y), Mathf.Max(0.01f, newChestHalfExtents.z)); // 상자 검사 절반 크기 보정 후 저장
            respawnDelay = Mathf.Max(0f, newRespawnDelay); // 재생성 시간 보정 후 저장
            maximumRespawnCountPerPoint = Mathf.Max(0, newMaximumRespawnCountPerPoint); // 최대 재생성 횟수 보정 후 저장
        } // 자동 설정 도구용 상자 생성기 설정 처리 종료
#endif // Editor 전용 설정 종료
    } // 절차 맵 기반 아이템 상자 생성기 묶음 종료
} // 프로젝트 아이템 기능 묶음 종료
