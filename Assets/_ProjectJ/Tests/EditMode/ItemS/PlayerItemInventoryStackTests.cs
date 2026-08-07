using NUnit.Framework; // EditMode 단위 테스트 기능 참조
using ProjectJ.Data; // 아이템 데이터 형식 참조
using ProjectJ.Items; // 플레이어 인벤토리 기능 참조
using UnityEngine; // 테스트 GameObject와 ScriptableObject 기능 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{ // 프로젝트 EditMode 테스트 묶음
    public sealed class PlayerItemInventoryStackTests // 풀 공 중첩과 슬롯 선택 테스트 선언
    { // 풀 공 중첩과 슬롯 선택 테스트 묶음
        [Test] // Unity Test Runner 테스트 지정
        public void BallStacksUpToFiveInOneSlot() // 풀 공 한 슬롯 최대 5개 중첩 확인
        { // 풀 공 중첩 테스트 처리
            GameObject playerObject = new GameObject("InventoryStackTestPlayer"); // 테스트 플레이어 오브젝트 생성
            PlayerItemInventory inventory = playerObject.AddComponent<PlayerItemInventory>(); // 테스트 2슬롯 인벤토리 추가
            ItemDataDefinition ballItem = CreateBallItem(); // 최대 5개 풀 공 데이터 생성

            for (int count = 0; count < 5; count++) // 풀 공 다섯 개 추가 반복
            { // 현재 풀 공 추가 처리
                Assert.IsTrue(inventory.TryAddItem(ballItem, out int placedSlotIndex)); // 현재 풀 공 추가 성공 확인
                Assert.AreEqual(0, placedSlotIndex); // 모든 풀 공 첫 슬롯 중첩 확인
            } // 현재 풀 공 추가 처리 종료

            Assert.AreEqual(5, inventory.GetQuantityAt(0)); // 첫 슬롯 최종 수량 다섯 개 확인
            Assert.AreEqual(1, inventory.ItemCount); // 사용 슬롯 한 칸 확인
            Object.DestroyImmediate(ballItem); // 테스트 풀 공 데이터 정리
            Object.DestroyImmediate(playerObject); // 테스트 플레이어 오브젝트 정리
        } // 풀 공 중첩 테스트 처리 종료

        [Test] // Unity Test Runner 테스트 지정
        public void SelectSlotChangesCurrentSelectedItem() // Q와 E 대상 슬롯 선택 상태 변경 확인
        { // 슬롯 선택 상태 테스트 처리
            GameObject playerObject = new GameObject("InventorySelectionTestPlayer"); // 테스트 플레이어 오브젝트 생성
            PlayerItemInventory inventory = playerObject.AddComponent<PlayerItemInventory>(); // 테스트 2슬롯 인벤토리 추가
            Assert.IsTrue(inventory.SelectSlot(1)); // 둘째 슬롯 선택 성공 확인
            Assert.AreEqual(1, inventory.SelectedSlotIndex); // 현재 선택 슬롯 둘째 칸 확인
            Object.DestroyImmediate(playerObject); // 테스트 플레이어 오브젝트 정리
        } // 슬롯 선택 상태 테스트 처리 종료

        private static ItemDataDefinition CreateBallItem() // 테스트용 풀 공 데이터 생성
        { // 테스트용 풀 공 데이터 생성 처리
            ItemDataDefinition itemData = ScriptableObject.CreateInstance<ItemDataDefinition>(); // 메모리 전용 풀 공 데이터 생성
            itemData.SetEditorIdentity("ITM-010", "풀 공", new ProjectDataVersion(1, 1, 0)); // 풀 공 식별 정보 설정
            itemData.ConfigureItemForEditor("한 슬롯 최대 5개", null, Color.green, 0.75f); // 풀 공 표시 정보 설정
            itemData.ConfigureUsageForEditor(ItemImplementationPriority.P0, ItemUseType.Projectile, ItemEffectType.Ball, 10f, 5, 0f, 4f, 0f, 28f, 0.24f, 0f, 16f, Vector3.one * 0.5f); // 풀 공 중첩과 투사체 수치 설정
            return itemData; // 구성된 테스트 풀 공 반환
        } // 테스트용 풀 공 데이터 생성 처리 종료
    } // 풀 공 중첩과 슬롯 선택 테스트 묶음 종료
} // 프로젝트 EditMode 테스트 묶음 종료
