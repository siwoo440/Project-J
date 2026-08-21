using UnityEngine; // 유니티 기능 사용
using UnityEngine.InputSystem; // Input System 사용

namespace ProjectJ.Items // 아이템 시스템 네임스페이스
{
    [DisallowMultipleComponent] // 중복 입력 Controller 방지
    [RequireComponent(typeof(PlayerInput))] // PlayerInput 필수 지정
    [RequireComponent(typeof(PlayerItemInventory))] // Inventory 필수 지정
    [RequireComponent(typeof(PlayerItemUseController))] // 공통 아이템 사용 Controller 필수 지정
    public sealed class PlayerItemInventoryInput : MonoBehaviour // 아이템 슬롯 선택과 사용 입력 처리
    {
        private const string LeftSlotActionName = "ItemSlotLeft"; // Q / D-Pad Left Action
        private const string RightSlotActionName = "ItemSlotRight"; // E / D-Pad Right Action
        private const string UseItemActionName = "UseItem"; // 우클릭 / Gamepad Trigger Action

        private PlayerInput playerInput; // Player Input 참조
        private PlayerItemInventory inventory; // Inventory 참조
        private PlayerItemUseController useController; // 공통 사용 Controller 참조

        private InputAction leftSlotAction; // 왼쪽 슬롯 Action
        private InputAction rightSlotAction; // 오른쪽 슬롯 Action
        private InputAction useItemAction; // 아이템 사용 Action

        private void Awake() // 필수 컴포넌트 준비
        {
            playerInput = GetComponent<PlayerInput>(); // PlayerInput 저장
            inventory = GetComponent<PlayerItemInventory>(); // Inventory 저장
            useController = GetComponent<PlayerItemUseController>(); // Use Controller 저장
        }

        private void OnEnable() // 입력 이벤트 연결
        {
            if (playerInput == null) // PlayerInput 초기화 검사
            {
                playerInput = GetComponent<PlayerInput>(); // 다시 탐색
            }

            if (inventory == null) // Inventory 초기화 검사
            {
                inventory = GetComponent<PlayerItemInventory>(); // 다시 탐색
            }

            if (useController == null) // Use Controller 초기화 검사
            {
                useController = GetComponent<PlayerItemUseController>(); // 다시 탐색
            }

            if (playerInput == null || playerInput.actions == null) // Input Action Asset 검사
            {
                return; // 입력 연결 중단
            }

            leftSlotAction =
                playerInput.actions.FindAction(LeftSlotActionName, false); // 왼쪽 슬롯 Action 탐색

            rightSlotAction =
                playerInput.actions.FindAction(RightSlotActionName, false); // 오른쪽 슬롯 Action 탐색

            useItemAction =
                playerInput.actions.FindAction(UseItemActionName, false); // 아이템 사용 Action 탐색

            if (leftSlotAction != null) // 왼쪽 Action 존재 검사
            {
                leftSlotAction.performed += OnLeftSlotPerformed; // Q 이벤트 연결
            }

            if (rightSlotAction != null) // 오른쪽 Action 존재 검사
            {
                rightSlotAction.performed += OnRightSlotPerformed; // E 이벤트 연결
            }

            if (useItemAction != null) // 사용 Action 존재 검사
            {
                useItemAction.performed += OnUseItemPerformed; // 우클릭 사용 이벤트 연결
            }
        }

        private void OnDisable() // 입력 이벤트 해제
        {
            if (leftSlotAction != null) // 왼쪽 Action 존재 검사
            {
                leftSlotAction.performed -= OnLeftSlotPerformed; // 이벤트 해제
            }

            if (rightSlotAction != null) // 오른쪽 Action 존재 검사
            {
                rightSlotAction.performed -= OnRightSlotPerformed; // 이벤트 해제
            }

            if (useItemAction != null) // 사용 Action 존재 검사
            {
                useItemAction.performed -= OnUseItemPerformed; // 사용 이벤트 해제
            }

            leftSlotAction = null; // Action 참조 초기화
            rightSlotAction = null; // Action 참조 초기화
            useItemAction = null; // Action 참조 초기화
        }

        private void OnLeftSlotPerformed(InputAction.CallbackContext context) // 왼쪽 슬롯 입력
        {
            if (inventory != null) // Inventory 존재 검사
            {
                inventory.SelectSlot(0); // 첫 번째 슬롯 선택
            }
        }

        private void OnRightSlotPerformed(InputAction.CallbackContext context) // 오른쪽 슬롯 입력
        {
            if (inventory != null) // Inventory 존재 검사
            {
                inventory.SelectSlot(1); // 두 번째 슬롯 선택
            }
        }

        private void OnUseItemPerformed(InputAction.CallbackContext context) // 현재 선택 아이템 사용 입력
        {
            if (useController == null) // Use Controller 존재 검사
            {
                return; // 사용 요청 중단
            }

            ItemUseResult result =
                useController.TryUseSelectedItem(); // 공통 사용 파이프라인 실행

            if (
                !result.IsSuccess &&
                result.Status != ItemUseStatus.EmptySlot &&
                result.Status != ItemUseStatus.NoEffectHandler
            ) // 정상적인 미구현 상태를 제외한 실패만 로그 출력
            {
                Debug.Log(
                    $"아이템 사용 실패: {result.Status} / {result.Message}",
                    this
                ); // 개발 중 실패 원인 확인
            }
        }
    }
}
