using UnityEngine; // Unity 충돌과 시각 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스 선언
{ // 정상 지점 트리거 범위
    [DisallowMultipleComponent] // 정상 지점 컴포넌트 중복 방지
    [RequireComponent(typeof(BoxCollider))] // 정상 지점 트리거 충돌체 보장
    public sealed class CourseTopTrigger : MonoBehaviour // 정상 지점 도달 감지 컴포넌트 선언
    { // 정상 지점 감지 기능 범위
        private static readonly int BaseColorPropertyId = Shader.PropertyToID("_BaseColor"); // URP 기본 색상 속성 식별자
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color"); // 기본 셰이더 색상 속성 식별자

        [SerializeField] private Renderer[] arrivalRenderers; // 정상 도달 색상을 적용할 렌더러 목록
        [SerializeField] private Color waitingColor = new Color(1f, 0.6f, 0.1f, 1f); // 정상 도달 전 색상
        [SerializeField] private Color reachedColor = new Color(0.2f, 1f, 0.9f, 1f); // 정상 도달 후 색상

        private MaterialPropertyBlock propertyBlock; // 재질 복제 없는 색상 변경 데이터
        private bool hasAnyPlayerReached; // 현재 실행에서 한 명 이상 정상 도달 상태

        public bool HasAnyPlayerReached => hasAnyPlayerReached; // 한 명 이상 정상 도달 여부 반환

        private void Awake() // 정상 지점 시각 상태 준비
        { // 정상 지점 준비 범위
            propertyBlock = new MaterialPropertyBlock(); // 재사용할 재질 속성 블록 생성
            ApplyVisualState(false); // 정상 도달 전 색상 적용
        } // 정상 지점 준비 범위 종료

        private void Reset() // 컴포넌트 추가 시 기본값 준비
        { // 정상 지점 기본값 준비 범위
            BoxCollider triggerCollider = GetComponent<BoxCollider>(); // BoxCollider 조회
            triggerCollider.isTrigger = true; // 트리거 모드 활성화
            arrivalRenderers = GetComponentsInChildren<Renderer>(true); // 자식 렌더러 자동 연결
        } // 정상 지점 기본값 준비 범위 종료

        private void OnValidate() // Inspector 충돌체 설정 보정
        { // Inspector 설정 보정 범위
            BoxCollider triggerCollider = GetComponent<BoxCollider>(); // BoxCollider 조회

            if (triggerCollider != null) // 충돌체 존재 확인
            { // 충돌체 존재 범위
                triggerCollider.isTrigger = true; // 트리거 모드 유지
            } // 충돌체 존재 범위 종료
        } // Inspector 설정 보정 범위 종료

        private void OnTriggerEnter(Collider other) // 플레이어 정상 진입 감지
        { // 정상 진입 감지 범위
            PlayerRespawnController respawnController = other.GetComponentInParent<PlayerRespawnController>(); // 진입 대상 부활 컴포넌트 조회

            if (respawnController == null) // 플레이어 부활 컴포넌트 누락 확인
            { // 플레이어 외 대상 범위
                return; // 플레이어 외 대상 처리 생략
            } // 플레이어 외 대상 범위 종료

            bool reachedForFirstTime = respawnController.MarkCourseTopReached(); // 해당 플레이어의 정상 최초 도달 기록

            if (!reachedForFirstTime) // 정상 중복 도달 여부 확인
            { // 정상 중복 도달 범위
                return; // 중복 도달 시각 처리 생략
            } // 정상 중복 도달 범위 종료

            hasAnyPlayerReached = true; // 한 명 이상 정상 도달 상태 저장
            ApplyVisualState(true); // 정상 도달 완료 색상 적용
        } // 정상 진입 감지 범위 종료

        private void ApplyVisualState(bool isReached) // 정상 지점 렌더러 색상 갱신
        { // 정상 지점 시각 상태 적용 범위
            if (arrivalRenderers == null || arrivalRenderers.Length == 0) // 시각 렌더러 누락 확인
            { // 시각 렌더러 누락 범위
                return; // 색상 처리 생략
            } // 시각 렌더러 누락 범위 종료

            if (propertyBlock == null) // 재질 속성 블록 생성 여부 확인
            { // 재질 속성 블록 누락 범위
                propertyBlock = new MaterialPropertyBlock(); // 재질 속성 블록 지연 생성
            } // 재질 속성 블록 누락 범위 종료

            Color targetColor = isReached ? reachedColor : waitingColor; // 도달 상태별 목표 색상 선택

            foreach (Renderer targetRenderer in arrivalRenderers) // 연결된 렌더러 순회
            { // 렌더러 색상 적용 반복 범위
                if (targetRenderer == null) // 비어 있는 렌더러 참조 확인
                { // 비어 있는 렌더러 범위
                    continue; // 현재 렌더러 처리 생략
                } // 비어 있는 렌더러 범위 종료

                targetRenderer.GetPropertyBlock(propertyBlock); // 기존 렌더러 속성 블록 읽기
                propertyBlock.SetColor(BaseColorPropertyId, targetColor); // URP 기본 색상 적용
                propertyBlock.SetColor(ColorPropertyId, targetColor); // 기본 셰이더 색상 적용
                targetRenderer.SetPropertyBlock(propertyBlock); // 변경된 색상 속성 적용
                propertyBlock.Clear(); // 다음 렌더러용 속성 블록 초기화
            } // 렌더러 색상 적용 반복 범위 종료
        } // 정상 지점 시각 상태 적용 범위 종료
    } // 정상 지점 감지 기능 범위 종료
} // 정상 지점 트리거 범위 종료
