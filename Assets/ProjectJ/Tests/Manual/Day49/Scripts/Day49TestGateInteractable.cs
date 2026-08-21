using ProjectJ.Interaction; // 공통 상호작용 시스템 사용
using UnityEngine; // 유니티 기능 사용

namespace ProjectJ.Tests.Manual // 수동 테스트 네임스페이스
{
    [DisallowMultipleComponent] // 중복 컴포넌트 방지
    public sealed class Day49TestGateInteractable : InteractableBehaviour // 테스트용 개폐 게이트
    {
        [SerializeField] // 인스펙터 직렬화
        private Transform gate; // 움직일 게이트 Transform

        [SerializeField] // 인스펙터 직렬화
        private Vector3 openOffset = new Vector3(0f, 4f, 0f); // 개방 이동량

        [SerializeField] // 인스펙터 직렬화
        private bool isOpen; // 현재 게이트 상태

        private Vector3 closedLocalPosition; // 닫힌 위치 저장

        public bool IsOpen // 현재 개방 상태 조회
        {
            get
            {
                return isOpen; // 상태 반환
            }
        }

        public void Configure(Transform targetGate, Vector3 offset) // Editor 테스트맵 초기 설정
        {
            gate = targetGate; // 게이트 연결
            openOffset = offset; // 개방 이동량 저장

            if (gate != null) // 게이트 존재 검사
            {
                closedLocalPosition = gate.localPosition; // 닫힌 위치 저장
            }

            ApplyState(); // 현재 상태 적용
        }

        public override bool CanInteract(GameObject interactor) // 상호작용 가능 여부
        {
            return isActiveAndEnabled && interactor != null && gate != null; // 기본 유효성 반환
        }

        public override void Interact(GameObject interactor) // 게이트 상호작용
        {
            if (!CanInteract(interactor)) // 실행 가능 상태 검사
            {
                return; // 잘못된 실행 차단
            }

            isOpen = !isOpen; // 게이트 상태 반전
            ApplyState(); // 변경 상태 적용
        }

        private void Awake() // 초기 위치 저장
        {
            if (gate != null) // 게이트 존재 검사
            {
                closedLocalPosition = gate.localPosition; // 현재 위치를 닫힌 위치로 저장
            }

            ApplyState(); // 초기 상태 적용
        }

        private void ApplyState() // 게이트 위치 적용
        {
            if (gate == null) // 게이트 누락 검사
            {
                return; // 처리 중단
            }

            gate.localPosition = isOpen // 상태에 따른 위치 선택
                ? closedLocalPosition + openOffset
                : closedLocalPosition;
        }
    }
}
