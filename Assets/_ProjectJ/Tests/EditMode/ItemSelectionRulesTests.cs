using NUnit.Framework; // EditMode 단위 테스트 기능 참조
using ProjectJ.Data; // 아이템 데이터와 효과 종류 참조
using ProjectJ.Items; // 아이템 가중치 선택 규칙 참조
using UnityEngine; // ScriptableObject 생성 기능 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{ // 프로젝트 EditMode 테스트 묶음
    public sealed class ItemSelectionRulesTests // 아이템 가중치 선택 규칙 테스트 선언
    { // 아이템 가중치 선택 규칙 테스트 묶음
        [Test] // Unity Test Runner 테스트 지정
        public void SelectByNormalizedValueUsesConfiguredWeightRanges() // 설정된 생성 가중치 구간 선택 확인
        { // 가중치 구간 선택 테스트 처리
            ItemDataDefinition highWeightItem = CreateItem("ITM-T01", "높은 가중치", 9f); // 가중치 9 아이템 생성
            ItemDataDefinition lowWeightItem = CreateItem("ITM-T02", "낮은 가중치", 1f); // 가중치 1 아이템 생성
            ItemDataDefinition[] itemPool = { highWeightItem, lowWeightItem }; // 두 아이템 후보 배열 생성

            Assert.AreSame(highWeightItem, ItemSelectionRules.SelectByNormalizedValue(itemPool, 0.5f)); // 전체 앞 90퍼센트 구간 첫 아이템 확인
            Assert.AreSame(lowWeightItem, ItemSelectionRules.SelectByNormalizedValue(itemPool, 0.95f)); // 전체 마지막 10퍼센트 구간 둘째 아이템 확인
            Object.DestroyImmediate(highWeightItem); // 첫 테스트 아이템 정리
            Object.DestroyImmediate(lowWeightItem); // 둘째 테스트 아이템 정리
        } // 가중치 구간 선택 테스트 처리 종료

        [Test] // Unity Test Runner 테스트 지정
        public void ItemEffectTypeContainsExactlyTwentyEightItems() // 확정 아이템 효과 종류 28개 확인
        { // 아이템 효과 종류 개수 테스트 처리
            int effectTypeCount = System.Enum.GetValues(typeof(ItemEffectType)).Length; // ItemEffectType 전체 항목 수 계산
            Assert.AreEqual(28, effectTypeCount); // 확정된 28종 효과 종류 개수 확인
        } // 아이템 효과 종류 개수 테스트 처리 종료

        private static ItemDataDefinition CreateItem(string id, string displayName, float spawnWeight) // 테스트용 아이템 데이터 생성
        { // 테스트용 아이템 데이터 생성 처리
            ItemDataDefinition itemData = ScriptableObject.CreateInstance<ItemDataDefinition>(); // 메모리 전용 아이템 데이터 생성
            itemData.SetEditorIdentity(id, displayName, new ProjectDataVersion(1, 0, 0)); // 테스트 식별 정보 설정
            itemData.ConfigureItemForEditor(string.Empty, null, Color.white, 0.75f); // 테스트 표시 정보 설정
            itemData.ConfigureUsageForEditor(ItemImplementationPriority.P0, ItemUseType.Instant, ItemEffectType.BalloonTrumpet, spawnWeight, 1, 0f, 0f, 0f, 0f, 0f, 0f, 0f, Vector3.one * 0.5f); // 테스트 생성 가중치 설정
            return itemData; // 구성된 테스트 아이템 반환
        } // 테스트용 아이템 데이터 생성 처리 종료
    } // 아이템 가중치 선택 규칙 테스트 묶음 종료
} // 프로젝트 EditMode 테스트 묶음 종료
