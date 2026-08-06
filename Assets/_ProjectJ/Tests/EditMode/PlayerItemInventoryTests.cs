using System.Collections.Generic; // 테스트 Unity 객체 목록 기능 참조
using NUnit.Framework; // EditMode 테스트 기능 참조
using ProjectJ.Data; // 아이템 데이터 형식 참조
using ProjectJ.Items; // 플레이어 인벤토리 기능 참조
using UnityEngine; // Unity 테스트 오브젝트 생성 기능 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{ // 프로젝트 EditMode 테스트 묶음
    public sealed class PlayerItemInventoryTests // 2슬롯 인벤토리 규칙 테스트 선언
    { // 2슬롯 인벤토리 테스트 묶음
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
        public void FirstItemUsesSlotZero() // 첫 아이템이 0번 슬롯에 배치되는지 확인
        { // 첫 아이템 배치 테스트 처리
            PlayerItemInventory inventory = CreateInventory(); // 빈 2슬롯 인벤토리 생성
            ItemDataDefinition item = CreateItem("ITM-T01", "Test Item A"); // 첫 테스트 아이템 생성

            bool added = inventory.TryAddItem(item, out int slotIndex); // 첫 아이템 추가 시도

            Assert.IsTrue(added); // 첫 아이템 추가 성공 확인
            Assert.AreEqual(0, slotIndex); // 첫 아이템 0번 슬롯 배치 확인
            Assert.AreSame(item, inventory.GetItemAt(0)); // 0번 슬롯 아이템 참조 확인
        } // 첫 아이템 배치 테스트 처리 종료

        [Test] // Unity Test Runner 테스트 지정
        public void SecondItemUsesSlotOne() // 두 번째 아이템이 1번 슬롯에 배치되는지 확인
        { // 두 번째 아이템 배치 테스트 처리
            PlayerItemInventory inventory = CreateInventory(); // 빈 2슬롯 인벤토리 생성
            ItemDataDefinition firstItem = CreateItem("ITM-T01", "Test Item A"); // 첫 테스트 아이템 생성
            ItemDataDefinition secondItem = CreateItem("ITM-T02", "Test Item B"); // 두 번째 테스트 아이템 생성
            inventory.TryAddItem(firstItem, out int firstSlotIndex); // 첫 슬롯 채우기

            bool added = inventory.TryAddItem(secondItem, out int secondSlotIndex); // 두 번째 아이템 추가 시도

            Assert.AreEqual(0, firstSlotIndex); // 첫 아이템 0번 슬롯 배치 확인
            Assert.IsTrue(added); // 두 번째 아이템 추가 성공 확인
            Assert.AreEqual(1, secondSlotIndex); // 두 번째 아이템 1번 슬롯 배치 확인
            Assert.AreSame(secondItem, inventory.GetItemAt(1)); // 1번 슬롯 아이템 참조 확인
        } // 두 번째 아이템 배치 테스트 처리 종료

        [Test] // Unity Test Runner 테스트 지정
        public void ThirdItemIsRejectedWhenInventoryIsFull() // 세 번째 아이템이 가득 찬 인벤토리에서 거부되는지 확인
        { // 인벤토리 가득 참 테스트 처리
            PlayerItemInventory inventory = CreateInventory(); // 빈 2슬롯 인벤토리 생성
            inventory.TryAddItem(CreateItem("ITM-T01", "Test Item A"), out int firstSlotIndex); // 첫 슬롯 채우기
            inventory.TryAddItem(CreateItem("ITM-T02", "Test Item B"), out int secondSlotIndex); // 두 번째 슬롯 채우기
            ItemDataDefinition thirdItem = CreateItem("ITM-T03", "Test Item C"); // 세 번째 테스트 아이템 생성

            bool added = inventory.TryAddItem(thirdItem, out int thirdSlotIndex); // 세 번째 아이템 추가 시도

            Assert.AreEqual(0, firstSlotIndex); // 첫 슬롯 번호 확인
            Assert.AreEqual(1, secondSlotIndex); // 두 번째 슬롯 번호 확인
            Assert.IsFalse(added); // 세 번째 아이템 추가 실패 확인
            Assert.AreEqual(-1, thirdSlotIndex); // 실패 슬롯 번호 확인
            Assert.IsTrue(inventory.IsFull); // 인벤토리 가득 참 상태 확인
            Assert.AreEqual(2, inventory.ItemCount); // 보유 아이템 두 개 유지 확인
        } // 인벤토리 가득 참 테스트 처리 종료

        [Test] // Unity Test Runner 테스트 지정
        public void RemovedSlotBecomesFirstEmptySlot() // 비운 앞 슬롯이 다시 우선 배치되는지 확인
        { // 빈 슬롯 재사용 테스트 처리
            PlayerItemInventory inventory = CreateInventory(); // 빈 2슬롯 인벤토리 생성
            ItemDataDefinition firstItem = CreateItem("ITM-T01", "Test Item A"); // 첫 테스트 아이템 생성
            ItemDataDefinition secondItem = CreateItem("ITM-T02", "Test Item B"); // 두 번째 테스트 아이템 생성
            ItemDataDefinition replacementItem = CreateItem("ITM-T03", "Test Item C"); // 교체 테스트 아이템 생성
            inventory.TryAddItem(firstItem, out int firstSlotIndex); // 첫 슬롯 채우기
            inventory.TryAddItem(secondItem, out int secondSlotIndex); // 두 번째 슬롯 채우기
            inventory.TryRemoveItemAt(0, out ItemDataDefinition removedItem); // 0번 슬롯 아이템 제거

            bool added = inventory.TryAddItem(replacementItem, out int replacementSlotIndex); // 새 아이템 추가 시도

            Assert.AreEqual(0, firstSlotIndex); // 기존 첫 슬롯 번호 확인
            Assert.AreEqual(1, secondSlotIndex); // 기존 두 번째 슬롯 번호 확인
            Assert.AreSame(firstItem, removedItem); // 제거된 아이템 참조 확인
            Assert.IsTrue(added); // 교체 아이템 추가 성공 확인
            Assert.AreEqual(0, replacementSlotIndex); // 비운 0번 슬롯 우선 재사용 확인
        } // 빈 슬롯 재사용 테스트 처리 종료

        [Test] // Unity Test Runner 테스트 지정
        public void NullItemIsRejected() // 빈 아이템 데이터가 거부되는지 확인
        { // 빈 아이템 데이터 테스트 처리
            PlayerItemInventory inventory = CreateInventory(); // 빈 2슬롯 인벤토리 생성

            bool added = inventory.TryAddItem(null, out int slotIndex); // 빈 아이템 추가 시도

            Assert.IsFalse(added); // 빈 아이템 추가 실패 확인
            Assert.AreEqual(-1, slotIndex); // 실패 슬롯 번호 확인
            Assert.AreEqual(0, inventory.ItemCount); // 인벤토리 비어 있음 확인
        } // 빈 아이템 데이터 테스트 처리 종료

        [Test] // Unity Test Runner 테스트 지정
        public void ItemDataStoresCommonDisplayValues() // 아이템 공통 표시 데이터 저장 여부 확인
        { // 아이템 공통 데이터 테스트 처리
            ItemDataDefinition item = CreateItem("ITM-T01", "Test Item A"); // 공통 데이터가 설정된 테스트 아이템 생성

            Assert.AreEqual("접촉 획득 테스트 아이템", item.Description); // 아이템 설명 저장 확인
            Assert.AreEqual(new Color(0.2f, 0.8f, 1f, 1f), item.PickupColor); // 아이템 대표 색상 저장 확인
            Assert.AreEqual(0.75f, item.PickupVisualScale); // 아이템 표시 크기 저장 확인
        } // 아이템 공통 데이터 테스트 처리 종료

        private PlayerItemInventory CreateInventory() // 테스트용 빈 플레이어 인벤토리 생성
        { // 테스트 인벤토리 생성 처리
            GameObject playerObject = new GameObject("TestPlayer"); // 빈 테스트 플레이어 오브젝트 생성
            createdObjects.Add(playerObject); // 정리 대상 플레이어 오브젝트 등록
            return playerObject.AddComponent<PlayerItemInventory>(); // 2슬롯 인벤토리 추가 후 반환
        } // 테스트 인벤토리 생성 처리 종료

        private ItemDataDefinition CreateItem(string itemId, string displayName) // 공통 데이터가 설정된 테스트 아이템 생성
        { // 테스트 아이템 생성 처리
            ItemDataDefinition item = ScriptableObject.CreateInstance<ItemDataDefinition>(); // 빈 테스트 아이템 데이터 생성
            item.SetEditorIdentity(itemId, displayName, new ProjectDataVersion(1, 0, 0)); // 테스트 아이템 식별 정보 설정
            item.ConfigureItemForEditor("접촉 획득 테스트 아이템", null, new Color(0.2f, 0.8f, 1f, 1f), 0.75f); // 테스트 아이템 표시 데이터 설정
            createdObjects.Add(item); // 정리 대상 아이템 데이터 등록
            return item; // 구성된 테스트 아이템 반환
        } // 테스트 아이템 생성 처리 종료
    } // 2슬롯 인벤토리 테스트 묶음 종료
} // 프로젝트 EditMode 테스트 묶음 종료
