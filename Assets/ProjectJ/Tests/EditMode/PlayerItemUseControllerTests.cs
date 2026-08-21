using NUnit.Framework; // NUnit 테스트 사용
using ProjectJ.Items; // 아이템 시스템 사용
using UnityEngine; // GameObject와 ScriptableObject 사용

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스
{
    public sealed class PlayerItemUseControllerTests // 공통 아이템 사용 파이프라인 테스트
    {
        [SetUp] // 각 테스트 시작 전 실행
        public void SetUp()
        {
            ItemUseEffectRegistry.Clear(); // 이전 테스트 Effect 제거
        }

        [TearDown] // 각 테스트 종료 후 실행
        public void TearDown()
        {
            ItemUseEffectRegistry.Clear(); // Static Registry 정리
        }

        [Test] // 테스트 등록
        public void TryUseSelectedItem_ReturnsEmptySlot_WhenSelectedSlotIsEmpty() // 빈 슬롯 실패 테스트
        {
            GameObject player = CreatePlayer(
                out PlayerItemInventory inventory,
                out PlayerItemUseController controller
            ); // 테스트 Player 생성

            try
            {
                ItemUseResult result =
                    controller.TryUseSelectedItem(); // 빈 슬롯 사용 시도

                Assert.AreEqual(ItemUseStatus.EmptySlot, result.Status); // 빈 슬롯 결과 확인
                Assert.IsFalse(result.IsSuccess); // 실패 상태 확인
                Assert.IsNull(inventory.SelectedItem); // Inventory 유지 확인
            }
            finally
            {
                Object.DestroyImmediate(player); // Player 제거
            }
        }

        [Test] // 테스트 등록
        public void TryUseSelectedItem_DoesNotConsume_WhenEffectIsMissing() // Effect 미구현 미소비 테스트
        {
            GameObject player = CreatePlayer(
                out PlayerItemInventory inventory,
                out PlayerItemUseController controller
            ); // 테스트 Player 생성

            ItemDefinition definition =
                CreateItem("spring_shoes", "Spring Shoes"); // 테스트 아이템 생성

            try
            {
                inventory.TryAdd(definition, out _); // Q 슬롯 저장

                ItemUseResult result =
                    controller.TryUseSelectedItem(); // Effect 없는 상태에서 사용 시도

                Assert.AreEqual(
                    ItemUseStatus.NoEffectHandler,
                    result.Status
                ); // 미구현 Effect 결과 확인

                Assert.AreSame(
                    definition,
                    inventory.GetItem(0)
                ); // 실패 시 아이템 유지 확인
            }
            finally
            {
                Object.DestroyImmediate(definition); // 데이터 제거
                Object.DestroyImmediate(player); // Player 제거
            }
        }

        [Test] // 테스트 등록
        public void TryUseSelectedItem_ConsumesItem_WhenEffectSucceeds() // 성공 시 소비 테스트
        {
            GameObject player = CreatePlayer(
                out PlayerItemInventory inventory,
                out PlayerItemUseController controller
            ); // 테스트 Player 생성

            ItemDefinition definition =
                CreateItem("test_success", "Success Item"); // 성공용 아이템 생성

            FakeEffect effect =
                new FakeEffect(ItemUseResult.Success()); // 성공 Effect 생성

            try
            {
                inventory.TryAdd(definition, out _); // Q 슬롯 저장
                ItemUseEffectRegistry.Register(definition.ItemId, effect); // 성공 Effect 등록

                ItemUseResult result =
                    controller.TryUseSelectedItem(); // 아이템 사용

                Assert.IsTrue(result.IsSuccess); // 성공 결과 확인
                Assert.IsNull(inventory.GetItem(0)); // 성공 후 슬롯 소비 확인
                Assert.AreEqual(1, effect.CallCount); // Effect 정확히 한 번 실행 확인
            }
            finally
            {
                Object.DestroyImmediate(definition); // 데이터 제거
                Object.DestroyImmediate(player); // Player 제거
            }
        }

        [Test] // 테스트 등록
        public void TryUseSelectedItem_KeepsItem_WhenEffectFails() // Effect 실패 시 아이템 유지 테스트
        {
            GameObject player = CreatePlayer(
                out PlayerItemInventory inventory,
                out PlayerItemUseController controller
            ); // 테스트 Player 생성

            ItemDefinition definition =
                CreateItem("test_fail", "Fail Item"); // 실패용 아이템 생성

            FakeEffect effect =
                new FakeEffect(
                    ItemUseResult.Fail(
                        ItemUseStatus.InvalidTarget,
                        "Target 없음"
                    )
                ); // 실패 Effect 생성

            try
            {
                inventory.TryAdd(definition, out _); // Q 슬롯 저장
                ItemUseEffectRegistry.Register(definition.ItemId, effect); // 실패 Effect 등록

                ItemUseResult result =
                    controller.TryUseSelectedItem(); // 사용 시도

                Assert.AreEqual(
                    ItemUseStatus.InvalidTarget,
                    result.Status
                ); // Effect 실패 이유 유지 확인

                Assert.AreSame(
                    definition,
                    inventory.GetItem(0)
                ); // 실패 시 아이템 미소비 확인
            }
            finally
            {
                Object.DestroyImmediate(definition); // 데이터 제거
                Object.DestroyImmediate(player); // Player 제거
            }
        }

        [Test] // 테스트 등록
        public void TryUseSelectedItem_OnlyConsumesSelectedSlot() // 선택 슬롯만 소비 테스트
        {
            GameObject player = CreatePlayer(
                out PlayerItemInventory inventory,
                out PlayerItemUseController controller
            ); // 테스트 Player 생성

            ItemDefinition first =
                CreateItem("first", "First"); // Q 아이템 생성

            ItemDefinition second =
                CreateItem("second", "Second"); // E 아이템 생성

            FakeEffect effect =
                new FakeEffect(ItemUseResult.Success()); // 성공 Effect 생성

            try
            {
                inventory.TryAdd(first, out _); // Q 슬롯 저장
                inventory.TryAdd(second, out _); // E 슬롯 저장
                inventory.SelectSlot(1); // E 슬롯 선택
                ItemUseEffectRegistry.Register(second.ItemId, effect); // E Effect 등록

                ItemUseResult result =
                    controller.TryUseSelectedItem(); // 선택 아이템 사용

                Assert.IsTrue(result.IsSuccess); // 성공 확인
                Assert.AreSame(first, inventory.GetItem(0)); // Q 슬롯 유지
                Assert.IsNull(inventory.GetItem(1)); // E 슬롯만 소비
            }
            finally
            {
                Object.DestroyImmediate(first); // Q 데이터 제거
                Object.DestroyImmediate(second); // E 데이터 제거
                Object.DestroyImmediate(player); // Player 제거
            }
        }

        private static GameObject CreatePlayer( // 테스트 Player 공통 생성
            out PlayerItemInventory inventory,
            out PlayerItemUseController controller
        )
        {
            GameObject player = new GameObject("Player"); // Player 생성
            inventory = player.AddComponent<PlayerItemInventory>(); // Inventory 추가
            controller = player.AddComponent<PlayerItemUseController>(); // Use Controller 추가
            return player; // 생성 Player 반환
        }

        private static ItemDefinition CreateItem( // 테스트 ItemDefinition 생성
            string itemId,
            string displayName
        )
        {
            ItemDefinition definition =
                ScriptableObject.CreateInstance<ItemDefinition>(); // ScriptableObject 생성

            definition.Configure(
                itemId,
                displayName,
                ItemCategory.Utility,
                ItemUseMode.Instant,
                ItemTargetType.Self,
                0f,
                0f,
                false
            ); // 정상 기본 데이터 설정

            return definition; // 생성 데이터 반환
        }

        private sealed class FakeEffect : IItemUseEffect // 테스트용 가짜 Effect
        {
            private readonly ItemUseResult result; // 미리 정한 실행 결과

            public int CallCount { get; private set; } // 호출 횟수 확인

            public FakeEffect(ItemUseResult result) // 가짜 Effect 생성
            {
                this.result = result; // 반환 결과 저장
            }

            public ItemUseResult TryUse(ItemUseContext context) // 가짜 Effect 실행
            {
                CallCount++; // 호출 횟수 증가
                return result; // 미리 정한 결과 반환
            }
        }
    }
}
