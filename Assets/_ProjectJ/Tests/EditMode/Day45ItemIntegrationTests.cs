using System; // Enum과 문자열 비교 기능 참조
using System.Collections.Generic; // 중복 검사 집합 기능 참조
using System.Linq; // 개수 집계 기능 참조
using NUnit.Framework; // EditMode 단위 테스트 기능 참조
using ProjectJ.Data; // 아이템 데이터 형식 참조
using ProjectJ.Items; // 인벤토리와 가중치 규칙 참조
using UnityEditor; // 프로젝트 아이템 에셋 검색 기능 참조
using UnityEngine; // 테스트 GameObject 기능 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{ // 45일차 아이템 통합 규칙 테스트 정의
    public sealed class Day45ItemIntegrationTests // 28종 데이터와 2슬롯 교체 규칙 검증 테스트 선언
    { // 정적 데이터와 인벤토리 핵심 계약 검증
        private const string ItemDataFolderPath = "Assets/_ProjectJ/Data/Definitions/Item"; // 아이템 데이터 폴더 경로
        private const int ExpectedItemCount = 28; // 현재 구현 완료 아이템 수

        [Test] // Unity Test Runner EditMode 테스트 지정
        public void ItemDataContainsExactlyTwentyEightUniqueEffects() // 28종 데이터와 효과 종류 일대일 매핑 확인
        { // 데이터 누락과 중복 EffectType 방지
            ItemDataDefinition[] items = LoadItems(); // 실제 프로젝트 아이템 데이터 전체 로드
            HashSet<string> ids = new HashSet<string>(items.Select(item => item.DataId), StringComparer.Ordinal); // 고유 DataId 집합 생성
            HashSet<ItemEffectType> effects = new HashSet<ItemEffectType>(items.Select(item => item.EffectType)); // 고유 EffectType 집합 생성

            Assert.AreEqual(ExpectedItemCount, items.Length, "아이템 데이터는 정확히 28개여야 합니다."); // 전체 아이템 수 검증
            Assert.AreEqual(ExpectedItemCount, ids.Count, "DataId가 28개 모두 고유해야 합니다."); // DataId 중복 여부 검증
            Assert.AreEqual(ExpectedItemCount, effects.Count, "ItemEffectType이 28개 모두 고유해야 합니다."); // EffectType 중복 여부 검증
            Assert.AreEqual(ExpectedItemCount, Enum.GetValues(typeof(ItemEffectType)).Length, "Runtime ItemEffectType도 28개여야 합니다."); // Runtime 효과 열거형 수 검증
        } // 28종 일대일 매핑 테스트 완료

        [Test] // Unity Test Runner EditMode 테스트 지정
        public void ItemPrioritiesMatchDay42ToDay44ImplementationCounts() // P0·P1·P2 구현 개수 확인
        { // 42~44일차 구현 범위 회귀 방지
            ItemDataDefinition[] items = LoadItems(); // 실제 프로젝트 아이템 데이터 전체 로드
            int p0Count = items.Count(item => item.ImplementationPriority == ItemImplementationPriority.P0); // P0 아이템 수 계산
            int p1Count = items.Count(item => item.ImplementationPriority == ItemImplementationPriority.P1); // P1 아이템 수 계산
            int p2Count = items.Count(item => item.ImplementationPriority == ItemImplementationPriority.P2); // P2 아이템 수 계산

            Assert.AreEqual(10, p0Count, "P0 아이템은 10개여야 합니다."); // 42일차 구현 개수 검증
            Assert.AreEqual(11, p1Count, "P1 아이템은 11개여야 합니다."); // 43일차 구현 개수 검증
            Assert.AreEqual(7, p2Count, "P2 아이템은 7개여야 합니다."); // 44일차 구현 개수 검증
        } // 구현 우선순위 개수 테스트 완료

        [Test] // Unity Test Runner EditMode 테스트 지정
        public void ItemSpawnWeightsArePositiveAndSelectable() // 모든 등장 가중치와 전체 선택 범위 확인
        { // 잘못된 등장 확률 데이터 방지
            ItemDataDefinition[] items = LoadItems(); // 실제 프로젝트 아이템 데이터 전체 로드
            float totalWeight = ItemSelectionRules.CalculateTotalWeight(items); // 전체 등장 가중치 계산

            Assert.Greater(totalWeight, 0f, "전체 SpawnWeight 합계는 0보다 커야 합니다."); // 전체 가중치 유효성 검증

            for (int index = 0; index < items.Length; index++) // 28종 아이템 전체 순회
            { // 현재 아이템 개별 가중치 확인
                Assert.Greater(items[index].SpawnWeight, 0f, $"{items[index].DataId} SpawnWeight는 0보다 커야 합니다."); // 개별 가중치 양수 검증
            } // 개별 가중치 검사 완료

            Assert.IsNotNull(ItemSelectionRules.SelectByNormalizedValue(items, 0f), "정규화 값 0에서 아이템이 선택되어야 합니다."); // 가중치 구간 시작 선택 검증
            Assert.IsNotNull(ItemSelectionRules.SelectByNormalizedValue(items, 1f), "정규화 값 1에서 마지막 아이템이 선택되어야 합니다."); // 가중치 구간 끝 선택 검증
        } // 등장 가중치 테스트 완료

        [Test] // Unity Test Runner EditMode 테스트 지정
        public void FullInventoryReplacesCurrentlySelectedSlot() // 두 슬롯이 가득 찼을 때 선택 슬롯 교체 확인
        { // 기획서의 상자 획득 교체 규칙 검증
            GameObject playerObject = new GameObject("Day45_InventoryReplace_EditMode"); // 테스트 플레이어 오브젝트 생성
            PlayerItemInventory inventory = playerObject.AddComponent<PlayerItemInventory>(); // 테스트 2슬롯 인벤토리 추가
            ItemDataDefinition[] items = LoadItems(); // 실제 아이템 데이터 전체 로드

            Assert.GreaterOrEqual(items.Length, 3, "교체 테스트에는 최소 3개 아이템 데이터가 필요합니다."); // 테스트 데이터 수 사전 검증
            Assert.IsTrue(inventory.TryAddItem(items[0], out int firstSlot)); // 첫 아이템 빈 슬롯 추가 검증
            Assert.IsTrue(inventory.TryAddItem(items[1], out int secondSlot)); // 둘째 아이템 빈 슬롯 추가 검증
            Assert.AreEqual(0, firstSlot); // 첫 아이템의 첫 슬롯 배치 검증
            Assert.AreEqual(1, secondSlot); // 둘째 아이템의 둘째 슬롯 배치 검증
            Assert.IsTrue(inventory.SelectSlot(1)); // 둘째 슬롯 선택 검증
            Assert.IsTrue(inventory.TryAddOrReplaceSelectedItem(items[2], out int replacedSlot, out ItemDataDefinition replacedItem)); // 가득 찬 상태 새 아이템 지급 검증
            Assert.AreEqual(1, replacedSlot); // 선택된 둘째 슬롯 교체 검증
            Assert.AreSame(items[1], replacedItem); // 교체 전 둘째 아이템 반환 검증
            Assert.AreSame(items[0], inventory.GetItemAt(0)); // 선택하지 않은 첫 슬롯 유지 검증
            Assert.AreSame(items[2], inventory.GetItemAt(1)); // 선택 슬롯 새 아이템 교체 검증
            Assert.AreEqual(1, inventory.GetQuantityAt(1)); // 교체된 아이템 수량 한 개 검증
            UnityEngine.Object.DestroyImmediate(playerObject); // 테스트 플레이어 오브젝트 정리
        } // 가득 찬 인벤토리 교체 테스트 완료

        [Test] // Unity Test Runner EditMode 테스트 지정
        public void ConsumedItemLeavesSlotOnlyWhenQuantityReachesZero() // 소비 시 수량 감소와 빈 슬롯 전환 확인
        { // 아이템 슬롯 소비 회귀 방지
            GameObject playerObject = new GameObject("Day45_InventoryConsume_EditMode"); // 테스트 플레이어 오브젝트 생성
            PlayerItemInventory inventory = playerObject.AddComponent<PlayerItemInventory>(); // 테스트 2슬롯 인벤토리 추가
            ItemDataDefinition stackableItem = FindStackableItem(); // 실제 중첩 가능 아이템 데이터 검색

            Assert.IsNotNull(stackableItem, "MaximumStackCount가 2 이상인 아이템이 하나 이상 필요합니다."); // 중첩 테스트 데이터 존재 검증
            Assert.IsTrue(inventory.TryAddItem(stackableItem, out int slotIndex)); // 중첩 아이템 첫 개 추가 검증
            Assert.IsTrue(inventory.TryAddItem(stackableItem, out int stackedSlotIndex)); // 같은 아이템 둘째 개 중첩 검증
            Assert.AreEqual(slotIndex, stackedSlotIndex); // 두 아이템 같은 슬롯 중첩 검증
            Assert.AreEqual(2, inventory.GetQuantityAt(slotIndex)); // 중첩 뒤 수량 두 개 검증
            Assert.IsTrue(inventory.TryConsumeItemAt(slotIndex, out ItemDataDefinition firstConsumed)); // 첫 소비 성공 검증
            Assert.AreSame(stackableItem, firstConsumed); // 첫 소비 아이템 참조 검증
            Assert.AreEqual(1, inventory.GetQuantityAt(slotIndex)); // 첫 소비 뒤 한 개 유지 검증
            Assert.IsNotNull(inventory.GetItemAt(slotIndex)); // 수량 남은 슬롯 유지 검증
            Assert.IsTrue(inventory.TryConsumeItemAt(slotIndex, out ItemDataDefinition secondConsumed)); // 마지막 소비 성공 검증
            Assert.AreSame(stackableItem, secondConsumed); // 마지막 소비 아이템 참조 검증
            Assert.AreEqual(0, inventory.GetQuantityAt(slotIndex)); // 마지막 소비 뒤 수량 0 검증
            Assert.IsNull(inventory.GetItemAt(slotIndex)); // 마지막 소비 뒤 빈 슬롯 검증
            UnityEngine.Object.DestroyImmediate(playerObject); // 테스트 플레이어 오브젝트 정리
        } // 아이템 소비 테스트 완료

        private static ItemDataDefinition[] LoadItems() // Item 폴더의 실제 아이템 데이터 전체 로드
        { // AssetDatabase 검색 결과를 DataId 순서로 정렬
            string[] guids = AssetDatabase.FindAssets("t:ItemDataDefinition", new[] { ItemDataFolderPath }); // 아이템 데이터 GUID 전체 검색
            List<ItemDataDefinition> items = new List<ItemDataDefinition>(); // 유효 아이템 목록 생성

            for (int index = 0; index < guids.Length; index++) // 검색된 GUID 전체 순회
            { // GUID를 실제 아이템 에셋으로 변환
                string path = AssetDatabase.GUIDToAssetPath(guids[index]); // 현재 GUID의 프로젝트 경로 조회
                ItemDataDefinition item = AssetDatabase.LoadAssetAtPath<ItemDataDefinition>(path); // 현재 아이템 데이터 불러오기

                if (item != null) // 유효한 아이템 데이터 여부 확인
                { // 테스트 대상 목록에 현재 데이터 추가
                    items.Add(item); // 현재 아이템 데이터 추가
                } // 유효 아이템 수집 완료
            } // 아이템 에셋 검색 처리 완료

            return items.OrderBy(item => item.DataId, StringComparer.Ordinal).ToArray(); // DataId 오름차순 배열 반환
        } // 실제 아이템 데이터 로드 완료

        private static ItemDataDefinition FindStackableItem() // 최대 중첩 수가 2 이상인 실제 아이템 검색
        { // 현재 28종 데이터에서 중첩 소비 테스트 대상 선택
            ItemDataDefinition[] items = LoadItems(); // 실제 프로젝트 아이템 데이터 전체 로드
            return items.FirstOrDefault(item => item.MaximumStackCount >= 2); // 첫 중첩 가능 아이템 반환
        } // 중첩 가능 아이템 검색 완료
    } // 45일차 EditMode 통합 테스트 정의
} // 프로젝트 EditMode 테스트 기능 정의
