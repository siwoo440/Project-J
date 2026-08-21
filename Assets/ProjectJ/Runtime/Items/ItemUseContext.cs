using UnityEngine; // GameObject 사용

namespace ProjectJ.Items // 아이템 시스템 네임스페이스
{
    public readonly struct ItemUseContext // Effect 실행에 전달할 공통 사용 정보
    {
        public GameObject User { get; } // 아이템을 사용하는 플레이어

        public PlayerItemInventory Inventory { get; } // 사용자의 인벤토리

        public ItemDefinition Definition { get; } // 현재 사용 아이템 데이터

        public int SlotIndex { get; } // 사용 요청 슬롯

        public ItemUseContext( // 사용 Context 생성
            GameObject user,
            PlayerItemInventory inventory,
            ItemDefinition definition,
            int slotIndex
        )
        {
            User = user; // 플레이어 저장
            Inventory = inventory; // Inventory 저장
            Definition = definition; // 아이템 데이터 저장
            SlotIndex = slotIndex; // 슬롯 Index 저장
        }
    }
}
