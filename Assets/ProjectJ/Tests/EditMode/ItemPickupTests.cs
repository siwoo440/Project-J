using NUnit.Framework; // NUnit 테스트 사용
using ProjectJ.Items; // 아이템 시스템 사용
using UnityEngine; // Unity 오브젝트 사용

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스
{
    public sealed class ItemPickupTests // ItemPickup 핵심 규칙 테스트
    {
        [Test] // 테스트 등록
        public void TryCollect_AddsItemToInventoryOnce() // 한 Pickup은 한 번만 지급 테스트
        {
            GameObject player = new GameObject("Player"); // 테스트 Player 생성
            PlayerItemInventory inventory =
                player.AddComponent<PlayerItemInventory>(); // Inventory 추가

            ItemDefinition definition = CreateDefinition(
                "spring_shoes",
                "Spring Shoes"
            ); // 정상 아이템 데이터 생성

            GameObject pickupObject = new GameObject("Pickup"); // Pickup Root 생성
            BoxCollider trigger = pickupObject.AddComponent<BoxCollider>(); // Trigger Collider 추가
            trigger.isTrigger = true; // Trigger 설정
            ItemPickup pickup = pickupObject.AddComponent<ItemPickup>(); // Pickup 기능 추가
            pickup.Configure(definition, trigger, null, false); // 테스트 데이터 연결

            try
            {
                bool firstResult = pickup.TryCollect(player); // 첫 획득 시도
                bool secondResult = pickup.TryCollect(player); // 두 번째 획득 시도

                Assert.IsTrue(firstResult); // 첫 획득 성공 확인
                Assert.IsFalse(secondResult); // 중복 획득 차단 확인
                Assert.AreSame(definition, inventory.GetItem(0)); // Q 슬롯 저장 확인
                Assert.IsTrue(pickup.IsCollected); // Pickup 소비 상태 확인
            }
            finally
            {
                Object.DestroyImmediate(pickupObject); // Pickup 제거
                Object.DestroyImmediate(definition); // 데이터 제거
                Object.DestroyImmediate(player); // Player 제거
            }
        }

        [Test] // 테스트 등록
        public void TryCollect_RejectsMissingDefinition() // ItemDefinition 없는 Pickup 거부 테스트
        {
            GameObject player = new GameObject("Player"); // 테스트 Player 생성
            player.AddComponent<PlayerItemInventory>(); // Inventory 추가

            GameObject pickupObject = new GameObject("Pickup"); // Pickup Root 생성
            BoxCollider trigger = pickupObject.AddComponent<BoxCollider>(); // Trigger 추가
            trigger.isTrigger = true; // Trigger 설정
            ItemPickup pickup = pickupObject.AddComponent<ItemPickup>(); // Pickup 기능 추가
            pickup.Configure(null, trigger, null, false); // Definition 없이 구성

            try
            {
                bool result = pickup.TryCollect(player); // 획득 시도

                Assert.IsFalse(result); // 획득 실패 확인
                Assert.IsFalse(pickup.IsCollected); // 소비되지 않음 확인
            }
            finally
            {
                Object.DestroyImmediate(pickupObject); // Pickup 제거
                Object.DestroyImmediate(player); // Player 제거
            }
        }

        [Test] // 테스트 등록
        public void TryCollect_UsesSelectedSlotReplacementRuleWhenFull() // 가득 찬 Inventory 교체 규칙 연결 테스트
        {
            GameObject player = new GameObject("Player"); // 테스트 Player 생성
            PlayerItemInventory inventory =
                player.AddComponent<PlayerItemInventory>(); // Inventory 추가

            ItemDefinition first = CreateDefinition("first", "First"); // 첫 아이템 생성
            ItemDefinition second = CreateDefinition("second", "Second"); // 두 번째 아이템 생성
            ItemDefinition replacement = CreateDefinition(
                "replacement",
                "Replacement"
            ); // 교체 아이템 생성

            inventory.TryAdd(first, out _); // Q 슬롯 채우기
            inventory.TryAdd(second, out _); // E 슬롯 채우기
            inventory.SelectSlot(1); // E 슬롯 선택

            GameObject pickupObject = new GameObject("Pickup"); // Pickup Root 생성
            BoxCollider trigger = pickupObject.AddComponent<BoxCollider>(); // Trigger 추가
            trigger.isTrigger = true; // Trigger 설정
            ItemPickup pickup = pickupObject.AddComponent<ItemPickup>(); // Pickup 추가
            pickup.Configure(replacement, trigger, null, false); // 교체 아이템 연결

            try
            {
                bool result = pickup.TryCollect(player); // 세 번째 아이템 획득

                Assert.IsTrue(result); // 획득 성공 확인
                Assert.AreSame(first, inventory.GetItem(0)); // Q 슬롯 유지
                Assert.AreSame(replacement, inventory.GetItem(1)); // 선택 E 슬롯 교체
            }
            finally
            {
                Object.DestroyImmediate(pickupObject); // Pickup 제거
                Object.DestroyImmediate(first); // 첫 데이터 제거
                Object.DestroyImmediate(second); // 두 번째 데이터 제거
                Object.DestroyImmediate(replacement); // 교체 데이터 제거
                Object.DestroyImmediate(player); // Player 제거
            }
        }

        private static ItemDefinition CreateDefinition( // 정상 테스트 아이템 생성
            string itemId,
            string displayName
        )
        {
            ItemDefinition definition =
                ScriptableObject.CreateInstance<ItemDefinition>(); // 데이터 생성

            definition.Configure(
                itemId,
                displayName,
                ItemCategory.Utility,
                ItemUseMode.Instant,
                ItemTargetType.Self,
                0f,
                0f,
                false
            ); // 정상 기본 데이터 적용

            return definition; // 생성 데이터 반환
        }
    }
}
