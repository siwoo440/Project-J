using UnityEngine; // 유니티 기능 사용

namespace ProjectJ.Interaction // 상호작용 시스템 네임스페이스
{
    [DisallowMultipleComponent] // 중복 컴포넌트 방지
    public sealed class TestInteractableButton : InteractableBehaviour // 공통 상호작용 테스트 버튼
    {
        [SerializeField] // 인스펙터 직렬화
        private bool interactionEnabled = true; // 버튼 사용 가능 여부

        [SerializeField] // 인스펙터 직렬화
        private GameObject activeIndicator; // 작동 상태 표시 오브젝트

        [SerializeField] // 인스펙터 직렬화
        private bool activated; // 현재 작동 상태

        [SerializeField] // 인스펙터 직렬화
        private int interactionCount; // 상호작용 실행 횟수

        public bool IsActivated // 작동 상태 조회
        {
            get
            {
                return activated; // 현재 상태 반환
            }
        }

        public int InteractionCount // 실행 횟수 조회
        {
            get
            {
                return interactionCount; // 실행 횟수 반환
            }
        }

        public void Configure(bool canInteract, GameObject indicator) // 테스트 버튼 초기 설정
        {
            interactionEnabled = canInteract; // 사용 가능 상태 저장
            activeIndicator = indicator; // 상태 표시 오브젝트 저장
            ApplyIndicator(); // 현재 상태 표시 적용
        }

        public void SetInteractionEnabled(bool value) // 사용 가능 상태 변경
        {
            interactionEnabled = value; // 사용 가능 상태 저장
        }

        public override bool CanInteract(GameObject interactor) // 현재 상호작용 가능 판정
        {
            return isActiveAndEnabled && interactionEnabled && interactor != null; // 최종 사용 가능 여부 반환
        }

        public override void Interact(GameObject interactor) // 버튼 상호작용 실행
        {
            if (!CanInteract(interactor)) // 실행 가능 상태 검사
            {
                return; // 잘못된 실행 차단
            }

            activated = !activated; // 버튼 상태 반전
            interactionCount++; // 실행 횟수 증가
            ApplyIndicator(); // 상태 표시 갱신
            Debug.Log($"{name} 상호작용 실행 : {interactionCount}회", this); // 테스트 실행 로그 출력
        }

        private void Awake() // 초기 표시 상태 적용
        {
            ApplyIndicator(); // 상태 표시 갱신
        }

        private void ApplyIndicator() // 상태 표시 오브젝트 갱신
        {
            if (activeIndicator == null) // 표시 오브젝트 존재 검사
            {
                return; // 표시 갱신 중단
            }

            activeIndicator.SetActive(activated); // 작동 상태 표시 적용
        }
    }
}
