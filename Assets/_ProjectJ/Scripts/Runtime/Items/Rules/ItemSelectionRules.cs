using ProjectJ.Data; // 아이템 데이터 형식 참조
using UnityEngine; // Unity 수학 기능 참조

namespace ProjectJ.Items // 프로젝트 아이템 기능 네임스페이스 선언
{ // 프로젝트 아이템 기능 묶음
    public static class ItemSelectionRules // 상자 아이템 가중치 선택 규칙 선언
    { // 상자 아이템 가중치 선택 규칙 묶음
        public static float CalculateTotalWeight(ItemDataDefinition[] itemPool) // 유효 아이템 전체 생성 가중치 계산
        { // 전체 생성 가중치 계산 처리
            if (itemPool == null) // 아이템 후보 목록 누락 여부 확인
            { // 누락 후보 처리
                return 0f; // 전체 가중치 없음 반환
            } // 누락 후보 처리 종료

            float totalWeight = 0f; // 전체 생성 가중치 초기화

            for (int itemIndex = 0; itemIndex < itemPool.Length; itemIndex++) // 모든 아이템 후보 순회
            { // 현재 아이템 가중치 처리
                if (itemPool[itemIndex] != null) // 유효 아이템 데이터 여부 확인
                { // 유효 아이템 가중치 합산 처리
                    totalWeight += itemPool[itemIndex].SpawnWeight; // 현재 아이템 가중치 추가
                } // 유효 아이템 가중치 합산 처리 종료
            } // 현재 아이템 가중치 처리 종료

            return Mathf.Max(0f, totalWeight); // 음수가 없는 전체 가중치 반환
        } // 전체 생성 가중치 계산 처리 종료

        public static ItemDataDefinition SelectByNormalizedValue(ItemDataDefinition[] itemPool, float normalizedValue) // 0부터 1 사이 값으로 가중치 아이템 선택
        { // 가중치 아이템 선택 처리
            float totalWeight = CalculateTotalWeight(itemPool); // 유효 아이템 전체 가중치 계산

            if (totalWeight <= 0f || itemPool == null) // 선택 가능한 가중치 여부 확인
            { // 선택 불가 처리
                return null; // 선택 아이템 없음 반환
            } // 선택 불가 처리 종료

            float targetWeight = Mathf.Clamp01(normalizedValue) * totalWeight; // 전체 가중치 안의 목표 지점 계산
            float accumulatedWeight = 0f; // 누적 가중치 초기화
            ItemDataDefinition lastValidItem = null; // 경계값 보정용 마지막 아이템 저장

            for (int itemIndex = 0; itemIndex < itemPool.Length; itemIndex++) // 모든 아이템 후보 순회
            { // 현재 아이템 선택 구간 처리
                ItemDataDefinition itemData = itemPool[itemIndex]; // 현재 아이템 데이터 조회

                if (itemData == null) // 누락 아이템 여부 확인
                { // 누락 아이템 처리
                    continue; // 현재 후보 생략
                } // 누락 아이템 처리 종료

                lastValidItem = itemData; // 마지막 유효 아이템 갱신
                accumulatedWeight += itemData.SpawnWeight; // 현재 아이템 구간 끝 누적

                if (targetWeight < accumulatedWeight) // 목표 지점이 현재 아이템 구간 내부인지 확인
                { // 현재 아이템 선택 처리
                    return itemData; // 가중치 기반 선택 아이템 반환
                } // 현재 아이템 선택 처리 종료
            } // 현재 아이템 선택 구간 처리 종료

            return lastValidItem; // 정확한 1 경계값에서 마지막 아이템 반환
        } // 가중치 아이템 선택 처리 종료
    } // 상자 아이템 가중치 선택 규칙 묶음 종료
} // 프로젝트 아이템 기능 묶음 종료
