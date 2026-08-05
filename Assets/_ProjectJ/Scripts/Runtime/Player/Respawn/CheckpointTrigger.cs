using UnityEngine; // Unity 충돌 기능 참조

namespace ProjectJ.Player // 플레이어 기능 네임스페이스
{
    [DisallowMultipleComponent] // 체크포인트 컴포넌트 중복 방지
    [RequireComponent(typeof(BoxCollider))] // 트리거 충돌체 보장
    public sealed class CheckpointTrigger : MonoBehaviour // 체크포인트 감지 컴포넌트
    {
        [SerializeField] private string checkpointId = "CP-001"; // 체크포인트 식별자
        [SerializeField] private Transform respawnPoint; // 실제 부활 위치
        [SerializeField] private bool activateOnce = true; // 최초 한 번만 활성화 여부

        private bool hasActivated; // 활성화 완료 상태

        private void Reset() // 컴포넌트 추가 시 기본값 준비
        {
            BoxCollider triggerCollider = GetComponent<BoxCollider>(); // BoxCollider 조회
            triggerCollider.isTrigger = true; // 트리거 모드 활성화
            respawnPoint = transform; // 현재 Transform을 부활 지점으로 연결
        }

        private void OnValidate() // Inspector 변경값 보정
        {
            BoxCollider triggerCollider = GetComponent<BoxCollider>(); // BoxCollider 조회

            if (triggerCollider != null) // 충돌체 존재 확인
            {
                triggerCollider.isTrigger = true; // 트리거 모드 유지
            }
        }

        private void OnTriggerEnter(Collider other) // 플레이어 진입 감지
        {
            if (activateOnce && hasActivated) // 재활성화 차단 확인
            {
                return; // 중복 활성화 생략
            }

            PlayerRespawnController respawnController = other.GetComponentInParent<PlayerRespawnController>(); // 진입 대상 부활 컴포넌트 조회

            if (respawnController == null) // 플레이어 부활 컴포넌트 누락 확인
            {
                return; // 플레이어 외 대상 처리 생략
            }

            Transform targetRespawnPoint = respawnPoint != null ? respawnPoint : transform; // 사용할 부활 지점 선택
            respawnController.ActivateCheckpoint(checkpointId, targetRespawnPoint); // 플레이어 체크포인트 갱신
            hasActivated = true; // 활성화 완료 저장
        }
    }
}
