using NUnit.Framework; // NUnit 테스트 사용
using ProjectJ.Items; // 아이템 시스템 사용
using UnityEngine; // GameObject와 ScriptableObject 사용

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스
{
    public sealed class PlayerItemInventoryTests // 2슬롯 인벤토리 테스트
    {
        [Test] // 테스트 등록
        public void TryAdd_FillsEmptySlotsInOrder() // 빈 슬롯 우선 저장 테스트
        {
            GameObject player = new GameObject("Player"); // 테스트 Player 생성
            PlayerItemInventory inventory =
                player.AddComponent<PlayerItemInventory>(); // Inventory 추가

            ItemDefinition first = CreateItem("first"); // 첫 아이템 생성
            ItemDefinition second = CreateItem("second"); // 두 번째 아이템 생성

            try
            {
                inventory.TryAdd(first, out int firstSlot); // 첫 아이템 저장
                inventory.TryAdd(second, out int secondSlot); // 두 번째 아이템 저장

                Assert.AreEqual(0, firstSlot); // 첫 슬롯 저장 확인
                Assert.AreEqual(1, secondSlot); // 두 번째 슬롯 저장 확인
                Assert.AreSame(first, inventory.GetItem(0)); // 첫 데이터 확인
                Assert.AreSame(second, inventory.GetItem(1)); // 두 번째 데이터 확인
            }
            finally
            {
                Object.DestroyImmediate(first); // 첫 아이템 제거
                Object.DestroyImmediate(second); // 두 번째 아이템 제거
                Object.DestroyImmediate(player); // Player 제거
            }
        }

        [Test] // 테스트 등록
        public void TryAdd_ReplacesSelectedSlotWhenFull() // 가득 찬 경우 선택 슬롯 교체 테스트
        {
            GameObject player = new GameObject("Player"); // 테스트 Player 생성
            PlayerItemInventory inventory =
                player.AddComponent<PlayerItemInventory>(); // Inventory 추가

            ItemDefinition first = CreateItem("first"); // 첫 아이템 생성
            ItemDefinition second = CreateItem("second"); // 두 번째 아이템 생성
            ItemDefinition replacement = CreateItem("replacement"); // 교체 아이템 생성

            try
            {
                inventory.TryAdd(first, out _); // 첫 슬롯 저장
                inventory.TryAdd(second, out _); // 두 번째 슬롯 저장
                inventory.SelectSlot(1); // E 슬롯 선택
                inventory.TryAdd(replacement, out int replacedSlot); // 가득 찬 상태에서 새 아이템 저장

                Assert.AreEqual(1, replacedSlot); // 선택 슬롯 교체 확인
                Assert.AreSame(first, inventory.GetItem(0)); // Q 슬롯 유지 확인
                Assert.AreSame(replacement, inventory.GetItem(1)); // E 슬롯 교체 확인
            }
            finally
            {
                Object.DestroyImmediate(first); // 첫 아이템 제거
                Object.DestroyImmediate(second); // 두 번째 아이템 제거
                Object.DestroyImmediate(replacement); // 교체 아이템 제거
                Object.DestroyImmediate(player); // Player 제거
            }
        }

        [Test] // 테스트 등록
        public void SelectSlot_ChangesSelectedSlot() // 슬롯 선택 테스트
        {
            GameObject player = new GameObject("Player"); // 테스트 Player 생성
            PlayerItemInventory inventory =
                player.AddComponent<PlayerItemInventory>(); // Inventory 추가

            try
            {
                bool result = inventory.SelectSlot(1); // E 슬롯 선택

                Assert.IsTrue(result); // 선택 성공 확인
                Assert.AreEqual(1, inventory.SelectedSlotIndex); // 선택 Index 확인
            }
            finally
            {
                Object.DestroyImmediate(player); // Player 제거
            }
        }

        private static ItemDefinition CreateItem(string id) // 테스트 아이템 생성
        {
            ItemDefinition definition =
                ScriptableObject.CreateInstance<ItemDefinition>(); // 데이터 생성

            definition.Configure(
                id,
                id,
                ItemCategory.Utility,
                ItemUseMode.Instant,
                ItemTargetType.Self,
                0f,
                0f,
                false
            ); // 기본 데이터 설정

            return definition; // 생성 데이터 반환
        }
    }
}
