using System; // Action 이벤트 사용
using UnityEngine; // 유니티 기능 사용

namespace ProjectJ.Items // 아이템 시스템 네임스페이스
{
    [DisallowMultipleComponent] // 중복 사용 Controller 방지
    [RequireComponent(typeof(PlayerItemInventory))] // Inventory 필수 지정
    public sealed class PlayerItemUseController : MonoBehaviour // 선택 아이템 공통 사용 처리
    {
        private PlayerItemInventory inventory; // 플레이어 Inventory

        public event Action<ItemUseResult> UseCompleted; // 사용 시도 결과 이벤트

        private void Awake() // 필수 컴포넌트 준비
        {
            inventory = GetComponent<PlayerItemInventory>(); // Inventory 저장
        }

        public ItemUseResult TryUseSelectedItem() // 현재 선택 슬롯 아이템 사용 시도
        {
            if (inventory == null) // Inventory 초기화 검사
            {
                inventory = GetComponent<PlayerItemInventory>(); // 다시 탐색
            }

            if (inventory == null) // Inventory 누락 검사
            {
                return Complete(
                    ItemUseResult.Fail(
                        ItemUseStatus.EffectFailed,
                        "PlayerItemInventory를 찾을 수 없습니다."
                    )
                ); // 구조 오류 반환
            }

            int slotIndex = inventory.SelectedSlotIndex; // 현재 선택 슬롯 저장
            ItemDefinition definition = inventory.GetItem(slotIndex); // 현재 아이템 조회

            if (definition == null) // 빈 슬롯 검사
            {
                return Complete(
                    ItemUseResult.Fail(
                        ItemUseStatus.EmptySlot,
                        "선택 슬롯이 비어 있습니다."
                    )
                ); // 빈 슬롯 실패 반환
            }

            if (!definition.IsDefinitionValid(out string errorMessage)) // ItemDefinition 유효성 검사
            {
                return Complete(
                    ItemUseResult.Fail(
                        ItemUseStatus.InvalidItem,
                        errorMessage
                    )
                ); // 잘못된 아이템 데이터 반환
            }

            if (
                !ItemUseEffectRegistry.TryResolve(
                    definition,
                    out IItemUseEffect effect
                )
            ) // 현재 아이템 Effect 등록 여부 검사
            {
                return Complete(
                    ItemUseResult.Fail(
                        ItemUseStatus.NoEffectHandler,
                        $"{definition.DisplayName} Effect가 아직 등록되지 않았습니다."
                    )
                ); // 미구현 Effect는 소비하지 않음
            }

            ItemUseContext context = new ItemUseContext(
                gameObject,
                inventory,
                definition,
                slotIndex
            ); // Effect에 전달할 사용 정보 생성

            ItemUseResult effectResult; // Effect 실행 결과

            try
            {
                effectResult = effect.TryUse(context); // 실제 아이템 효과 실행
            }
            catch (Exception exception) // Effect 예외 방어
            {
                Debug.LogException(exception, this); // 상세 예외 출력

                return Complete(
                    ItemUseResult.Fail(
                        ItemUseStatus.EffectFailed,
                        exception.Message
                    )
                ); // 예외 발생 시 아이템 유지
            }

            if (!effectResult.IsSuccess) // Effect 사용 실패 검사
            {
                return Complete(effectResult); // 실패 결과 그대로 반환하고 아이템 유지
            }

            if (inventory.GetItem(slotIndex) != definition) // Effect 실행 중 슬롯 내용 변경 검사
            {
                return Complete(
                    ItemUseResult.Fail(
                        ItemUseStatus.InventoryChanged,
                        "Effect 실행 중 선택 슬롯 내용이 변경되어 자동 소비를 중단했습니다."
                    )
                ); // 다른 아이템을 실수로 제거하지 않음
            }

            if (!inventory.RemoveItem(slotIndex)) // 성공 후 정확히 한 번 소비
            {
                return Complete(
                    ItemUseResult.Fail(
                        ItemUseStatus.InventoryChanged,
                        "사용 성공 후 선택 슬롯 소비에 실패했습니다."
                    )
                ); // 소비 실패를 명확하게 반환
            }

            return Complete(ItemUseResult.Success()); // 효과와 소비 모두 성공
        }

        private ItemUseResult Complete(ItemUseResult result) // 결과 이벤트와 반환을 한곳에서 처리
        {
            UseCompleted?.Invoke(result); // UI 피드백 확장용 이벤트 발생
            return result; // 호출자에게 결과 반환
        }
    }
}
