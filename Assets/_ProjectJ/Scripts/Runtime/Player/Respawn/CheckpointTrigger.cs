using UnityEngine; // Unity 충돌과 시각 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스 선언
{ // 체크포인트 트리거 범위
    [DisallowMultipleComponent] // 체크포인트 컴포넌트 중복 방지
    [RequireComponent(typeof(BoxCollider))] // 트리거 충돌체 보장
    public sealed class CheckpointTrigger : MonoBehaviour // 순서가 있는 체크포인트 감지 컴포넌트 선언
    { // 체크포인트 감지 기능 범위
        private static readonly int BaseColorPropertyId = Shader.PropertyToID("_BaseColor"); // URP 기본 색상 속성 식별자
        private static readonly int ColorPropertyId = Shader.PropertyToID("_Color"); // 기본 셰이더 색상 속성 식별자

        [Header("Checkpoint")] // 체크포인트 설정 구역 제목
        [SerializeField, Min(1)] private int checkpointIndex = 1; // 전체 코스 기준 체크포인트 번호
        [SerializeField] private string checkpointId = "CP-01"; // 체크포인트 식별자
        [SerializeField] private Transform respawnPoint; // 실제 부활 위치와 회전
        [Header("Visual Feedback")] // 체크포인트 시각 피드백 설정 구역 제목
        [SerializeField] private Renderer[] activationRenderers; // 활성화 색상을 적용할 렌더러 목록
        [SerializeField] private Color inactiveColor = new Color(0.25f, 0.35f, 0.45f, 1f); // 활성화 전 색상
        [SerializeField] private Color activeColor = new Color(0.1f, 1f, 0.45f, 1f); // 활성화 후 색상

        private MaterialPropertyBlock propertyBlock; // 재질 복제 없는 색상 변경 데이터
        private bool hasAnyPlayerActivated; // 현재 실행에서 한 명 이상 활성화한 상태

        public int CheckpointIndex => checkpointIndex; // 체크포인트 번호 반환
        public string CheckpointId => checkpointId; // 체크포인트 식별자 반환
        public bool HasAnyPlayerActivated => hasAnyPlayerActivated; // 한 명 이상 활성화 여부 반환

        private void Awake() // 체크포인트 시각 상태 준비
        { // 체크포인트 준비 범위
            propertyBlock = new MaterialPropertyBlock(); // 재사용할 재질 속성 블록 생성
            ApplyVisualState(false); // 활성화 전 색상 적용
        } // 체크포인트 준비 범위 종료

        private void Reset() // 컴포넌트 추가 시 기본값 준비
        { // 체크포인트 기본값 준비 범위
            BoxCollider triggerCollider = GetComponent<BoxCollider>(); // BoxCollider 조회
            triggerCollider.isTrigger = true; // 트리거 모드 활성화
            respawnPoint = transform; // 현재 Transform을 부활 지점으로 연결
            activationRenderers = GetComponentsInChildren<Renderer>(true); // 자식 렌더러 자동 연결
        } // 체크포인트 기본값 준비 범위 종료

        private void OnValidate() // Inspector 변경값 보정
        { // Inspector 설정 보정 범위
            checkpointIndex = Mathf.Max(1, checkpointIndex); // 최소 첫 번째 체크포인트 번호 보장
            BoxCollider triggerCollider = GetComponent<BoxCollider>(); // BoxCollider 조회

            if (triggerCollider != null) // 충돌체 존재 확인
            { // 충돌체 존재 범위
                triggerCollider.isTrigger = true; // 트리거 모드 유지
            } // 충돌체 존재 범위 종료
        } // Inspector 설정 보정 범위 종료

        private void OnTriggerEnter(Collider other) // 플레이어 진입과 즉시 활성화 감지
        { // 플레이어 진입 감지 범위
            PlayerRespawnController respawnController = other.GetComponentInParent<PlayerRespawnController>(); // 진입 대상 부활 컴포넌트 조회

            if (respawnController == null) // 플레이어 부활 컴포넌트 누락 확인
            { // 플레이어 외 대상 범위
                return; // 플레이어 외 대상 처리 생략
            } // 플레이어 외 대상 범위 종료

            Transform targetRespawnPoint = respawnPoint != null ? respawnPoint : transform; // 사용할 부활 지점 선택
            bool activated = respawnController.TryActivateCheckpoint(checkpointIndex, checkpointId, targetRespawnPoint); // 해당 플레이어의 더 높은 체크포인트 활성화 시도

            if (!activated) // 새 체크포인트 활성화 실패 확인
            { // 체크포인트 활성화 실패 범위
                return; // 기존 또는 낮은 체크포인트 시각 처리 생략
            } // 체크포인트 활성화 실패 범위 종료

            hasAnyPlayerActivated = true; // 한 명 이상 체크포인트 활성화 상태 저장
            ApplyVisualState(true); // 활성화 완료 색상 적용
        } // 플레이어 진입 감지 범위 종료

        private void ApplyVisualState(bool isActive) // 체크포인트 렌더러 색상 갱신
        { // 체크포인트 시각 상태 적용 범위
            if (activationRenderers == null || activationRenderers.Length == 0) // 시각 렌더러 누락 확인
            { // 시각 렌더러 누락 범위
                return; // 색상 처리 생략
            } // 시각 렌더러 누락 범위 종료

            if (propertyBlock == null) // 재질 속성 블록 생성 여부 확인
            { // 재질 속성 블록 누락 범위
                propertyBlock = new MaterialPropertyBlock(); // 재질 속성 블록 지연 생성
            } // 재질 속성 블록 누락 범위 종료

            Color targetColor = isActive ? activeColor : inactiveColor; // 활성화 상태별 목표 색상 선택

            foreach (Renderer targetRenderer in activationRenderers) // 연결된 렌더러 순회
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
        } // 체크포인트 시각 상태 적용 범위 종료
    } // 체크포인트 감지 기능 범위 종료
} // 체크포인트 트리거 범위 종료
