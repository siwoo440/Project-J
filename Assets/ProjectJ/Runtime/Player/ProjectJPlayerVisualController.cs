using UnityEngine; // Unity Visual 생성 사용

namespace ProjectJ.Player // Player Visual 네임스페이스
{ // 네임스페이스 시작
    [DisallowMultipleComponent] // Visual Controller 중복 방지
    public sealed class ProjectJPlayerVisualController : MonoBehaviour // Player 외형 교체 제어
    { // 클래스 시작
        public const string ChefVisualName = "Character_4c4e64"; // 검은색 요리사 기본 이름

        [SerializeField] // Visual Root 직렬화
        private Transform visualRoot; // 런타임 모델 부모

        [SerializeField] // 외형 Prefab 목록 직렬화
        private GameObject[] visualPrefabs = new GameObject[0]; // 꾸미기 후보 목록

        [SerializeField] // 기본 외형 이름 직렬화
        private string defaultVisualName = ChefVisualName; // 최초 검은색 요리사 선택

        [SerializeField] // 모델 위치 직렬화
        private Vector3 modelLocalPosition = // 모델 위치 보정값
            new Vector3(0f, -1f, 0f); // 현재 발 위치 기본값

        [SerializeField] // 모델 회전 직렬화
        private Vector3 modelLocalEulerAngles = // 모델 방향 보정값
            Vector3.zero; // 현재 정면 기본값

        [SerializeField] // 모델 크기 직렬화
        private Vector3 modelLocalScale = // 모델 크기 보정값
            Vector3.one; // 원본 크기 기본값

        [SerializeField] // Visual Collider 차단 여부 직렬화
        private bool disableVisualColliders = true; // Gameplay 물리 분리 기본값

        private GameObject activeVisual; // 현재 생성된 외형

        public int CurrentVisualIndex { get; private set; } = -1; // 현재 외형 인덱스

        public string CurrentVisualName { get; private set; } = string.Empty; // 현재 외형 이름

        public string DefaultVisualName => defaultVisualName; // 기본 외형 이름 조회

        public void Configure( // Editor 자동 설정
            Transform root, // Visual Root
            GameObject[] candidates, // 외형 후보 목록
            string defaultName // 기본 외형 이름
        ) // 매개변수 종료
        { // 메서드 시작
            visualRoot = root; // Visual Root 연결
            visualPrefabs = candidates ?? new GameObject[0]; // 후보 목록 연결
            defaultVisualName = string.IsNullOrEmpty(defaultName) // 기본 이름 유효성 검사
                ? ChefVisualName // 누락 시 검은색 요리사 사용
                : defaultName; // 지정된 기본 이름 사용
        } // 메서드 종료

        private void Start() // 런타임 최초 외형 적용
        { // 메서드 시작
            ApplyVisualByName(defaultVisualName); // 검은색 요리사 기본 적용
        } // 메서드 종료

        public bool ApplyVisualByName( // 꾸미기 이름 기반 외형 교체
            string requestedVisualName // 요청 외형 이름
        ) // 매개변수 종료
        { // 메서드 시작
            if (visualRoot == null || // Visual Root 누락 조건
                visualPrefabs == null || // 후보 배열 누락 조건
                visualPrefabs.Length == 0) // 빈 후보 조건
            { // 설정 누락 분기 시작
                CurrentVisualIndex = -1; // 선택 상태 초기화
                CurrentVisualName = string.Empty; // 외형 이름 초기화
                return false; // 외형 적용 실패 반환
            } // 설정 누락 분기 종료

            string[] candidateNames = BuildCandidateNames(); // 등록 외형 이름 목록 생성
            int resolvedIndex = ProjectJPlayerVisualIndex.ResolveByName( // 이름 기반 외형 선택
                requestedVisualName, // 꾸미기 요청 이름
                defaultVisualName, // 검은색 요리사 기본 이름
                candidateNames // 등록 외형 이름 목록
            ); // 외형 선택 종료

            if (resolvedIndex < 0 || // 잘못된 인덱스 조건
                resolvedIndex >= visualPrefabs.Length || // 배열 범위 초과 조건
                visualPrefabs[resolvedIndex] == null) // Prefab 누락 조건
            { // 유효 외형 없음 분기 시작
                CurrentVisualIndex = -1; // 선택 상태 초기화
                CurrentVisualName = string.Empty; // 외형 이름 초기화
                return false; // 외형 적용 실패 반환
            } // 유효 외형 없음 분기 종료

            ClearActiveVisual(); // 기존 외형 제거

            GameObject prefab = visualPrefabs[resolvedIndex]; // 선택 Prefab 조회
            activeVisual = Instantiate( // 새 외형 생성
                prefab, // 선택 Prefab
                visualRoot // Visual Root 부모
            ); // 외형 생성 종료
            activeVisual.name = $"Visual_{prefab.name}"; // 런타임 이름 정리

            Transform modelTransform = activeVisual.transform; // 모델 Transform 조회
            modelTransform.localPosition = modelLocalPosition; // 발 위치 보정
            modelTransform.localRotation = Quaternion.Euler( // 모델 회전 적용
                modelLocalEulerAngles // Inspector 회전값
            ); // 모델 회전 적용 종료
            modelTransform.localScale = modelLocalScale; // 모델 크기 적용

            PrepareVisualHierarchy(activeVisual); // Gameplay 물리 분리

            CurrentVisualIndex = resolvedIndex; // 현재 외형 인덱스 저장
            CurrentVisualName = prefab.name; // 현재 외형 이름 저장
            return true; // 외형 적용 성공 반환
        } // 메서드 종료

        private string[] BuildCandidateNames() // 등록 외형 이름 목록 생성
        { // 메서드 시작
            string[] candidateNames = new string[visualPrefabs.Length]; // 이름 배열 생성

            for (int index = 0; index < visualPrefabs.Length; index++) // 모든 후보 순회
            { // 반복 시작
                GameObject prefab = visualPrefabs[index]; // 현재 Prefab 조회
                candidateNames[index] = prefab != null // Prefab 존재 검사
                    ? prefab.name // 실제 외형 이름 저장
                    : string.Empty; // 누락 이름 저장
            } // 반복 종료

            return candidateNames; // 외형 이름 배열 반환
        } // 메서드 종료

        private void ClearActiveVisual() // 기존 외형 제거
        { // 메서드 시작
            if (activeVisual == null) // 기존 외형 누락 검사
            { // 누락 분기 시작
                return; // 제거 생략
            } // 누락 분기 종료

            Destroy(activeVisual); // 이전 외형 제거 예약
            activeVisual = null; // 기존 참조 초기화
        } // 메서드 종료

        private void PrepareVisualHierarchy( // Visual 물리 분리
            GameObject visual // 생성된 외형
        ) // 매개변수 종료
        { // 메서드 시작
            SetLayerRecursively( // Player Layer 통일
                visual.transform, // 외형 Root Transform
                gameObject.layer // Player Layer 번호
            ); // Layer 적용 종료

            Animator[] animators = visual.GetComponentsInChildren<Animator>( // Animator 수집
                true // 비활성 자식 포함
            ); // Animator 수집 종료

            foreach (Animator animator in animators) // 모든 Animator 순회
            { // 반복 시작
                animator.applyRootMotion = false; // Root Motion 차단
            } // 반복 종료

            if (!disableVisualColliders) // Collider 유지 설정 검사
            { // 유지 분기 시작
                return; // 물리 차단 생략
            } // 유지 분기 종료

            Collider[] colliders = visual.GetComponentsInChildren<Collider>( // Visual Collider 수집
                true // 비활성 자식 포함
            ); // Collider 수집 종료

            foreach (Collider collider in colliders) // 모든 Collider 순회
            { // 반복 시작
                collider.enabled = false; // Visual 충돌 차단
            } // 반복 종료

            Rigidbody[] rigidbodies = visual.GetComponentsInChildren<Rigidbody>( // Visual Rigidbody 수집
                true // 비활성 자식 포함
            ); // Rigidbody 수집 종료

            foreach (Rigidbody rigidbody in rigidbodies) // 모든 Rigidbody 순회
            { // 반복 시작
                rigidbody.useGravity = false; // Visual 중력 차단
                rigidbody.isKinematic = true; // Visual 물리 이동 차단
                rigidbody.detectCollisions = false; // Visual 충돌 차단
            } // 반복 종료
        } // 메서드 종료

        private static void SetLayerRecursively( // Visual Layer 재귀 적용
            Transform current, // 현재 Transform
            int layer // 적용할 Layer
        ) // 매개변수 종료
        { // 메서드 시작
            current.gameObject.layer = layer; // 현재 GameObject Layer 적용

            for (int index = 0; index < current.childCount; index++) // 모든 자식 순회
            { // 반복 시작
                SetLayerRecursively( // 자식 Layer 재귀 적용
                    current.GetChild(index), // 현재 자식 Transform
                    layer // 동일 Player Layer
                ); // 자식 Layer 적용 종료
            } // 반복 종료
        } // 메서드 종료
    } // 클래스 종료
} // 네임스페이스 종료
