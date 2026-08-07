using System.Collections; // UnityTest 코루틴 반환 형식 참조
using NUnit.Framework; // PlayMode 테스트와 Assertion 기능 참조
using ProjectJ.Data; // 테스트 아이템 데이터 형식 참조
using ProjectJ.Items; // 인벤토리와 상자 획득 기능 참조
using UnityEngine; // 런타임 GameObject와 ScriptableObject 기능 참조
using UnityEngine.TestTools; // UnityTest 기능 참조

namespace ProjectJ.Tests.PlayMode // 프로젝트 PlayMode 테스트 네임스페이스 선언
{ // 45일차 런타임 아이템 흐름 테스트 정의
    public sealed class Day45ItemIntegrationPlayModeTests // 상자 획득·교체·소비 런타임 검증 테스트 선언
    { // 실제 MonoBehaviour 생명주기를 포함한 핵심 아이템 흐름 검증
        [UnityTest] // 여러 프레임을 사용하는 PlayMode 테스트 지정
        public IEnumerator ChestPickupReplacesSelectedSlotWhenInventoryIsFull() // 가득 찬 인벤토리의 상자 획득 교체 확인
        { // 상자에서 인벤토리까지 연결된 런타임 흐름 검증
            GameObject playerObject = new GameObject("Day45_Player_PlayMode"); // 테스트 플레이어 오브젝트 생성
            PlayerItemInventory inventory = playerObject.AddComponent<PlayerItemInventory>(); // 런타임 2슬롯 인벤토리 추가
            ItemDataDefinition firstItem = CreateItem("TEST-001", "첫 아이템", ItemEffectType.SpringShoes); // 첫 슬롯 테스트 아이템 생성
            ItemDataDefinition secondItem = CreateItem("TEST-002", "둘째 아이템", ItemEffectType.JellyShield); // 둘째 슬롯 테스트 아이템 생성
            ItemDataDefinition newItem = CreateItem("TEST-003", "새 아이템", ItemEffectType.BananaCushion); // 교체 획득 테스트 아이템 생성

            Assert.IsTrue(inventory.TryAddItem(firstItem, out int firstSlot)); // 첫 슬롯 아이템 추가 성공 검증
            Assert.IsTrue(inventory.TryAddItem(secondItem, out int secondSlot)); // 둘째 슬롯 아이템 추가 성공 검증
            Assert.AreEqual(0, firstSlot); // 첫 아이템 첫 슬롯 배치 검증
            Assert.AreEqual(1, secondSlot); // 둘째 아이템 둘째 슬롯 배치 검증
            Assert.IsTrue(inventory.SelectSlot(1)); // 교체 대상 둘째 슬롯 선택 검증

            GameObject chestObject = new GameObject("Day45_Chest_PlayMode"); // 테스트 아이템 상자 오브젝트 생성
            BoxCollider chestCollider = chestObject.AddComponent<BoxCollider>(); // 상자 필수 BoxCollider 추가
            ItemChestPickup chest = chestObject.AddComponent<ItemChestPickup>(); // 런타임 상자 지급 컴포넌트 추가
            chest.ConfigureRuntime(newItem, chestCollider, null, false, false); // 테스트용 상자 아이템과 비활성화 규칙 설정

            yield return null; // Awake와 런타임 상태 반영을 위한 한 프레임 대기

            Assert.IsTrue(chest.TryCollect(inventory)); // 상자 아이템 직접 획득 성공 검증
            Assert.IsTrue(chest.IsCollected); // 상자 획득 완료 상태 검증
            Assert.AreSame(firstItem, inventory.GetItemAt(0)); // 선택하지 않은 첫 슬롯 유지 검증
            Assert.AreSame(newItem, inventory.GetItemAt(1)); // 선택한 둘째 슬롯 새 아이템 교체 검증
            Assert.AreEqual(1, inventory.GetQuantityAt(1)); // 교체된 새 아이템 수량 한 개 검증

            Object.Destroy(playerObject); // 테스트 플레이어 오브젝트 정리 예약
            Object.Destroy(chestObject); // 테스트 상자 오브젝트 정리 예약
            Object.Destroy(firstItem); // 첫 테스트 아이템 정리 예약
            Object.Destroy(secondItem); // 둘째 테스트 아이템 정리 예약
            Object.Destroy(newItem); // 새 테스트 아이템 정리 예약
            yield return null; // Destroy 처리 완료를 위한 한 프레임 대기
        } // 가득 찬 상자 획득 교체 테스트 완료

        [UnityTest] // 여러 프레임을 사용하는 PlayMode 테스트 지정
        public IEnumerator InventoryConsumeAndClearFlowLeavesNoStaleSlotState() // 소비와 전체 초기화 뒤 잔여 슬롯 상태 확인
        { // HUD가 참조하는 인벤토리 상태의 런타임 일관성 검증
            GameObject playerObject = new GameObject("Day45_ConsumeClear_PlayMode"); // 테스트 플레이어 오브젝트 생성
            PlayerItemInventory inventory = playerObject.AddComponent<PlayerItemInventory>(); // 런타임 2슬롯 인벤토리 추가
            ItemDataDefinition firstItem = CreateItem("TEST-011", "소비 아이템", ItemEffectType.WaterGun); // 소비 테스트 아이템 생성
            ItemDataDefinition secondItem = CreateItem("TEST-012", "유지 아이템", ItemEffectType.Firework); // 초기화 테스트 아이템 생성

            Assert.IsTrue(inventory.TryAddItem(firstItem, out int firstSlot)); // 첫 아이템 추가 성공 검증
            Assert.IsTrue(inventory.TryAddItem(secondItem, out int secondSlot)); // 둘째 아이템 추가 성공 검증
            Assert.AreEqual(0, firstSlot); // 첫 슬롯 배치 검증
            Assert.AreEqual(1, secondSlot); // 둘째 슬롯 배치 검증
            Assert.IsTrue(inventory.SelectSlot(0)); // 소비 대상 첫 슬롯 선택 검증
            Assert.IsTrue(inventory.TryConsumeSelectedItem(out ItemDataDefinition consumedItem)); // 선택 아이템 소비 성공 검증
            Assert.AreSame(firstItem, consumedItem); // 소비된 아이템 참조 검증
            Assert.IsNull(inventory.GetItemAt(0)); // 마지막 수량 소비 뒤 첫 슬롯 비움 검증
            Assert.AreSame(secondItem, inventory.GetItemAt(1)); // 소비하지 않은 둘째 슬롯 유지 검증

            inventory.ClearInventory(); // 전체 인벤토리 초기화 실행
            yield return null; // 런타임 이벤트 처리 확인을 위한 한 프레임 대기

            Assert.AreEqual(0, inventory.ItemCount); // 초기화 뒤 사용 슬롯 수 0 검증
            Assert.AreEqual(0, inventory.SelectedSlotIndex); // 초기화 뒤 첫 슬롯 선택 검증
            Assert.IsNull(inventory.GetItemAt(0)); // 초기화 뒤 첫 슬롯 비움 검증
            Assert.IsNull(inventory.GetItemAt(1)); // 초기화 뒤 둘째 슬롯 비움 검증

            Object.Destroy(playerObject); // 테스트 플레이어 오브젝트 정리 예약
            Object.Destroy(firstItem); // 첫 테스트 아이템 정리 예약
            Object.Destroy(secondItem); // 둘째 테스트 아이템 정리 예약
            yield return null; // Destroy 처리 완료를 위한 한 프레임 대기
        } // 소비와 초기화 런타임 테스트 완료

        private static ItemDataDefinition CreateItem(string dataId, string displayName, ItemEffectType effectType) // 메모리 전용 테스트 아이템 생성
        { // 런타임 인벤토리 흐름에 필요한 최소 데이터 구성
            ItemDataDefinition item = ScriptableObject.CreateInstance<ItemDataDefinition>(); // 메모리 전용 ItemDataDefinition 생성
#if UNITY_EDITOR // Editor에서 실행되는 PlayMode 테스트 설정 API 포함 조건
            item.SetEditorIdentity(dataId, displayName, new ProjectDataVersion(1, 0, 0)); // 테스트 데이터 ID와 표시 이름 설정
            item.ConfigureItemForEditor("45일차 PlayMode 통합 테스트", null, Color.white, 0.75f); // 테스트 표시 데이터 설정
            item.ConfigureUsageForEditor(ItemImplementationPriority.P0, ItemUseType.Instant, effectType, 1f, 1, 0f, 0f, 0f, 0f, 0f, 0f, 0f, Vector3.one * 0.5f); // 테스트 사용 규칙과 효과 종류 설정
#endif // Editor 전용 테스트 데이터 설정 제외 경계
            return item; // 구성된 테스트 아이템 반환
        } // 테스트 아이템 생성 완료
    } // 45일차 PlayMode 통합 테스트 정의
} // 프로젝트 PlayMode 테스트 기능 정의
