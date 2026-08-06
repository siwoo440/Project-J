using System.Collections.Generic; // 테스트 Unity 객체 목록 기능 참조
using NUnit.Framework; // EditMode 테스트 기능 참조
using ProjectJ.Data; // 아이템 데이터 형식 참조
using ProjectJ.Items; // 아이템 상자와 인벤토리 기능 참조
using UnityEngine; // Unity 테스트 오브젝트와 물리 기능 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{ // 프로젝트 EditMode 테스트 묶음
    public sealed class ItemChestPickupTests // 아이템 상자 획득 규칙 테스트 선언
    { // 아이템 상자 획득 테스트 묶음
        private readonly List<Object> createdObjects = new List<Object>(); // 테스트 중 생성한 Unity 객체 목록 저장

        [TearDown] // 각 테스트 실행 뒤 정리 메서드 지정
        public void TearDown() // 테스트에서 생성한 Unity 객체 제거
        { // 테스트 객체 정리 처리
            for (int objectIndex = createdObjects.Count - 1; objectIndex >= 0; objectIndex--) // 생성 역순으로 테스트 객체 순회
            { // 현재 테스트 객체 제거 처리
                if (createdObjects[objectIndex] != null) // 현재 테스트 객체 존재 여부 확인
                { // 존재하는 테스트 객체 처리
                    Object.DestroyImmediate(createdObjects[objectIndex]); // 현재 테스트 객체 즉시 제거
                } // 존재하는 테스트 객체 처리 종료
            } // 현재 테스트 객체 제거 처리 종료

            createdObjects.Clear(); // 테스트 객체 목록 초기화
        } // 테스트 객체 정리 처리 종료

        [Test] // Unity Test Runner 테스트 지정
        public void SuccessfulPickupConsumesChestAndFillsFirstSlot() // 획득 성공 시 상자 소비와 첫 슬롯 배치 확인
        { // 상자 획득 성공 테스트 처리
            PlayerItemInventory inventory = CreateInventory(); // 빈 플레이어 인벤토리 생성
            ItemDataDefinition item = CreateItem("ITM-C01", "Chest Item A"); // 상자 지급 아이템 생성
            ItemChestPickup chest = CreateChest(item); // 테스트 아이템 상자 생성

            bool collected = chest.TryCollect(inventory); // 빈 인벤토리로 상자 획득 시도

            Assert.IsTrue(collected); // 상자 획득 성공 확인
            Assert.IsTrue(chest.IsCollected); // 상자 획득 완료 상태 확인
            Assert.AreSame(item, inventory.GetItemAt(0)); // 아이템 0번 슬롯 배치 확인
            Assert.IsFalse(chest.gameObject.activeSelf); // 획득 완료 상자 비활성화 확인
        } // 상자 획득 성공 테스트 처리 종료

        [Test] // Unity Test Runner 테스트 지정
        public void FullInventoryKeepsChestAvailable() // 인벤토리가 가득 차면 상자가 유지되는지 확인
        { // 가득 찬 인벤토리 상자 테스트 처리
            PlayerItemInventory inventory = CreateInventory(); // 빈 플레이어 인벤토리 생성
            inventory.TryAddItem(CreateItem("ITM-C01", "Chest Item A"), out int firstSlotIndex); // 첫 슬롯 채우기
            inventory.TryAddItem(CreateItem("ITM-C02", "Chest Item B"), out int secondSlotIndex); // 두 번째 슬롯 채우기
            ItemChestPickup chest = CreateChest(CreateItem("ITM-C03", "Chest Item C")); // 세 번째 아이템 상자 생성

            bool collected = chest.TryCollect(inventory); // 가득 찬 인벤토리로 상자 획득 시도

            Assert.AreEqual(0, firstSlotIndex); // 첫 슬롯 번호 확인
            Assert.AreEqual(1, secondSlotIndex); // 두 번째 슬롯 번호 확인
            Assert.IsFalse(collected); // 세 번째 상자 획득 실패 확인
            Assert.IsFalse(chest.IsCollected); // 상자 미획득 상태 유지 확인
            Assert.IsTrue(chest.gameObject.activeSelf); // 실패한 상자 활성 상태 유지 확인
            Assert.AreEqual(2, inventory.ItemCount); // 기존 두 아이템 유지 확인
        } // 가득 찬 인벤토리 상자 테스트 처리 종료

        [Test] // Unity Test Runner 테스트 지정
        public void MissingInventoryDoesNotConsumeChest() // 인벤토리 누락 시 상자가 소비되지 않는지 확인
        { // 인벤토리 누락 상자 테스트 처리
            ItemChestPickup chest = CreateChest(CreateItem("ITM-C01", "Chest Item A")); // 테스트 아이템 상자 생성

            bool collected = chest.TryCollect(null); // 빈 인벤토리 참조로 상자 획득 시도

            Assert.IsFalse(collected); // 상자 획득 실패 확인
            Assert.IsFalse(chest.IsCollected); // 상자 미획득 상태 유지 확인
            Assert.IsTrue(chest.gameObject.activeSelf); // 상자 활성 상태 유지 확인
        } // 인벤토리 누락 상자 테스트 처리 종료

        private PlayerItemInventory CreateInventory() // 테스트용 빈 플레이어 인벤토리 생성
        { // 테스트 인벤토리 생성 처리
            GameObject playerObject = new GameObject("TestPlayer"); // 빈 테스트 플레이어 오브젝트 생성
            createdObjects.Add(playerObject); // 정리 대상 플레이어 오브젝트 등록
            return playerObject.AddComponent<PlayerItemInventory>(); // 2슬롯 인벤토리 추가 후 반환
        } // 테스트 인벤토리 생성 처리 종료

        private ItemChestPickup CreateChest(ItemDataDefinition item) // 테스트용 접촉 획득 상자 생성
        { // 테스트 상자 생성 처리
            GameObject chestObject = new GameObject("TestItemChest"); // 빈 테스트 상자 오브젝트 생성
            createdObjects.Add(chestObject); // 정리 대상 상자 오브젝트 등록
            BoxCollider trigger = chestObject.AddComponent<BoxCollider>(); // 테스트 상자 Collider 추가
            trigger.isTrigger = true; // 접촉 전용 Trigger 설정
            ItemChestPickup chest = chestObject.AddComponent<ItemChestPickup>(); // 상자 획득 기능 추가
            chest.ConfigureForEditor(item, trigger, chestObject, true, false); // 테스트 아이템과 Trigger 연결
            return chest; // 구성된 테스트 상자 반환
        } // 테스트 상자 생성 처리 종료

        private ItemDataDefinition CreateItem(string itemId, string displayName) // 테스트용 아이템 데이터 생성
        { // 테스트 아이템 생성 처리
            ItemDataDefinition item = ScriptableObject.CreateInstance<ItemDataDefinition>(); // 빈 테스트 아이템 데이터 생성
            item.SetEditorIdentity(itemId, displayName, new ProjectDataVersion(1, 0, 0)); // 테스트 아이템 식별 정보 설정
            item.ConfigureItemForEditor("상자 획득 테스트", null, Color.cyan, 0.75f); // 테스트 아이템 표시 데이터 설정
            createdObjects.Add(item); // 정리 대상 아이템 데이터 등록
            return item; // 구성된 테스트 아이템 반환
        } // 테스트 아이템 생성 처리 종료
    } // 아이템 상자 획득 테스트 묶음 종료
} // 프로젝트 EditMode 테스트 묶음 종료
