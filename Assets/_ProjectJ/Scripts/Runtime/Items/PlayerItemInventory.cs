using System; // 인벤토리 이벤트 기능 참조
using ProjectJ.Data; // 아이템 데이터 형식 참조
using UnityEngine; // Unity 컴포넌트와 직렬화 기능 참조

namespace ProjectJ.Items // 프로젝트 아이템 기능 네임스페이스 선언
{ // 2슬롯 아이템 보관 기능 정의
    [DisallowMultipleComponent] // 플레이어당 인벤토리 한 개 제한
    public sealed class PlayerItemInventory : MonoBehaviour // 플레이어 2슬롯 아이템 인벤토리 선언
    { // 보관·선택·소비·교체 기능 정의
        public const int Capacity = 2; // 인벤토리 고정 슬롯 수

        [SerializeField] private ItemDataDefinition[] slots = new ItemDataDefinition[Capacity]; // 두 아이템 슬롯 저장
        [SerializeField] private int[] quantities = new int[Capacity]; // 슬롯별 현재 수량 저장
        [SerializeField, Range(0, Capacity - 1)] private int selectedSlotIndex; // 현재 선택 슬롯 번호 저장

        public event Action InventoryChanged; // 전체 슬롯 변경 알림
        public event Action<int, ItemDataDefinition> ItemAdded; // 아이템 추가 슬롯 알림
        public event Action<int, ItemDataDefinition, ItemDataDefinition> ItemReplaced; // 선택 슬롯 교체 알림
        public event Action<ItemDataDefinition> ItemRejectedBecauseFull; // 직접 추가 실패 알림
        public event Action<int> SelectedSlotChanged; // 선택 슬롯 변경 알림

        public int SlotCount => Capacity; // 전체 슬롯 수 반환
        public int ItemCount => CountOccupiedSlots(); // 현재 사용 슬롯 수 반환
        public bool IsFull => ItemCount >= Capacity; // 두 슬롯 사용 여부 반환
        public int SelectedSlotIndex => Mathf.Clamp(selectedSlotIndex, 0, Capacity - 1); // 보정된 선택 슬롯 반환
        public ItemDataDefinition SelectedItem => GetItemAt(SelectedSlotIndex); // 현재 선택 아이템 반환

        private void Awake() // 실행 시작 시 슬롯 데이터 보정
        { // 직렬화 데이터 안전성 확보
            EnsureSlotArrays(); // 정확한 두 슬롯 배열 보장
        } // 초기 슬롯 보정 완료

        private void OnValidate() // Inspector 변경 시 슬롯 데이터 보정
        { // 편집 중 배열과 선택값 안전성 확보
            EnsureSlotArrays(); // 정확한 두 슬롯 배열 보장
            selectedSlotIndex = Mathf.Clamp(selectedSlotIndex, 0, Capacity - 1); // 선택 슬롯 안전 범위 보정
        } // Inspector 보정 완료

        public ItemDataDefinition GetItemAt(int slotIndex) // 지정 슬롯 아이템 조회
        { // 범위 확인 뒤 아이템 반환
            EnsureSlotArrays(); // 조회 전 슬롯 배열 보장

            if (!IsValidSlotIndex(slotIndex)) // 슬롯 번호 유효성 확인
            { // 잘못된 슬롯 조회 차단
                return null; // 빈 조회 결과 반환
            } // 잘못된 슬롯 처리 완료

            return slots[slotIndex]; // 지정 슬롯 아이템 반환
        } // 슬롯 아이템 조회 완료

        public int GetQuantityAt(int slotIndex) // 지정 슬롯 현재 수량 조회
        { // 범위와 보유 상태 확인 뒤 수량 반환
            EnsureSlotArrays(); // 조회 전 수량 배열 보장

            if (!IsValidSlotIndex(slotIndex) || slots[slotIndex] == null) // 슬롯 범위와 아이템 존재 여부 확인
            { // 비어 있거나 잘못된 슬롯 처리
                return 0; // 보유 수량 없음 반환
            } // 빈 수량 처리 완료

            return Mathf.Max(1, quantities[slotIndex]); // 최소 한 개로 보정한 수량 반환
        } // 슬롯 수량 조회 완료

        public bool SelectSlot(int slotIndex) // Q와 E 입력 기반 슬롯 선택 시도
        { // 선택 가능 범위 확인 뒤 현재 슬롯 변경
            if (!IsValidSlotIndex(slotIndex)) // 슬롯 번호 범위 확인
            { // 잘못된 슬롯 선택 차단
                return false; // 슬롯 선택 실패 반환
            } // 잘못된 슬롯 처리 완료

            if (selectedSlotIndex == slotIndex) // 이미 선택된 슬롯 여부 확인
            { // 중복 선택 변경 생략
                return true; // 기존 선택 유지 성공 반환
            } // 중복 선택 처리 완료

            selectedSlotIndex = slotIndex; // 새 선택 슬롯 저장
            SelectedSlotChanged?.Invoke(selectedSlotIndex); // 선택 슬롯 변경 정보 전달
            InventoryChanged?.Invoke(); // HUD 전체 슬롯 갱신 요청
            return true; // 슬롯 선택 성공 반환
        } // 슬롯 선택 처리 완료

        public bool TryAddItem(ItemDataDefinition itemData, out int placedSlotIndex) // 중첩 슬롯과 빈 슬롯만 사용하는 기존 추가 시도
        { // 교체 없이 기존 추가 규칙 유지
            placedSlotIndex = -1; // 실패 기본 슬롯 번호 설정
            EnsureSlotArrays(); // 추가 전 슬롯 배열 보장

            if (itemData == null) // 아이템 데이터 누락 여부 확인
            { // 잘못된 추가 요청 차단
                return false; // 아이템 추가 실패 반환
            } // 누락 아이템 처리 완료

            if (TryStackItem(itemData, out placedSlotIndex)) // 같은 아이템 중첩 가능 여부 확인
            { // 기존 슬롯 중첩 성공 처리
                return true; // 아이템 중첩 성공 반환
            } // 중첩 추가 처리 완료

            placedSlotIndex = FindFirstEmptySlot(); // 앞에서부터 첫 빈 슬롯 검색

            if (placedSlotIndex < 0) // 빈 슬롯 없음 여부 확인
            { // 직접 추가 API의 기존 가득 참 규칙 유지
                ItemRejectedBecauseFull?.Invoke(itemData); // 가득 참 알림 전달
                return false; // 아이템 추가 실패 반환
            } // 가득 찬 인벤토리 처리 완료

            PlaceNewItem(placedSlotIndex, itemData); // 빈 슬롯에 새 아이템 한 개 저장
            return true; // 빈 슬롯 추가 성공 반환
        } // 기존 추가 API 처리 완료

        public bool TryAddOrReplaceSelectedItem(ItemDataDefinition itemData, out int placedSlotIndex, out ItemDataDefinition replacedItem) // 상자 획득용 추가 또는 선택 슬롯 교체 시도
        { // 기획서의 가득 찬 인벤토리 교체 규칙 적용
            placedSlotIndex = -1; // 실패 기본 슬롯 번호 설정
            replacedItem = null; // 교체 없음 기본 결과 설정
            EnsureSlotArrays(); // 추가 전 슬롯 배열 보장

            if (itemData == null) // 아이템 데이터 누락 여부 확인
            { // 잘못된 상자 지급 요청 차단
                return false; // 아이템 지급 실패 반환
            } // 누락 아이템 처리 완료

            if (TryStackItem(itemData, out placedSlotIndex)) // 먼저 기존 중첩 가능 슬롯 검색
            { // 중첩 가능 아이템 우선 처리
                return true; // 아이템 중첩 성공 반환
            } // 상자 중첩 처리 완료

            placedSlotIndex = FindFirstEmptySlot(); // 다음으로 첫 빈 슬롯 검색

            if (placedSlotIndex >= 0) // 빈 슬롯 존재 여부 확인
            { // 빈 슬롯에 정상 추가 처리
                PlaceNewItem(placedSlotIndex, itemData); // 새 아이템 한 개 저장
                return true; // 빈 슬롯 추가 성공 반환
            } // 빈 슬롯 지급 처리 완료

            placedSlotIndex = SelectedSlotIndex; // 가득 찬 경우 현재 선택 슬롯 지정
            replacedItem = slots[placedSlotIndex]; // 교체 전 아이템 참조 저장
            slots[placedSlotIndex] = itemData; // 선택 슬롯을 새 아이템으로 교체
            quantities[placedSlotIndex] = 1; // 새 아이템 수량 한 개 적용
            ItemReplaced?.Invoke(placedSlotIndex, replacedItem, itemData); // 교체 전후 정보 전달
            ItemAdded?.Invoke(placedSlotIndex, itemData); // 기존 추가 알림과 호환 유지
            InventoryChanged?.Invoke(); // HUD 전체 슬롯 갱신 요청
            return true; // 선택 슬롯 교체 성공 반환
        } // 상자 추가 또는 교체 처리 완료

        public bool TryConsumeSelectedItem(out ItemDataDefinition consumedItem) // 현재 선택 아이템 한 개 소비 시도
        { // 선택 슬롯 소비 API 연결
            return TryConsumeItemAt(SelectedSlotIndex, out consumedItem); // 선택 슬롯 소비 결과 반환
        } // 선택 아이템 소비 처리 완료

        public bool TryConsumeItemAt(int slotIndex, out ItemDataDefinition consumedItem) // 지정 슬롯 아이템 한 개 소비 시도
        { // 수량 감소와 마지막 아이템 제거 처리
            consumedItem = null; // 소비 실패 기본 결과 설정
            EnsureSlotArrays(); // 소비 전 슬롯 배열 보장

            if (!IsValidSlotIndex(slotIndex) || slots[slotIndex] == null) // 슬롯 범위와 아이템 존재 여부 확인
            { // 소비 불가 슬롯 차단
                return false; // 아이템 소비 실패 반환
            } // 소비 불가 슬롯 처리 완료

            consumedItem = slots[slotIndex]; // 소비할 아이템 결과 저장
            quantities[slotIndex] = Mathf.Max(0, quantities[slotIndex] - 1); // 현재 수량 한 개 감소

            if (quantities[slotIndex] <= 0) // 마지막 수량 소비 여부 확인
            { // 빈 슬롯 전환 처리
                slots[slotIndex] = null; // 지정 슬롯 아이템 제거
                quantities[slotIndex] = 0; // 빈 슬롯 수량 초기화
            } // 마지막 수량 처리 완료

            InventoryChanged?.Invoke(); // HUD 전체 슬롯 갱신 요청
            return true; // 아이템 소비 성공 반환
        } // 지정 슬롯 소비 처리 완료

        public bool TryRemoveItemAt(int slotIndex, out ItemDataDefinition removedItem) // 지정 슬롯 아이템 전체 제거 시도
        { // 아이템과 수량을 함께 제거
            removedItem = null; // 제거 실패 기본 결과 설정
            EnsureSlotArrays(); // 제거 전 슬롯 배열 보장

            if (!IsValidSlotIndex(slotIndex) || slots[slotIndex] == null) // 슬롯 범위와 아이템 존재 여부 확인
            { // 제거 불가 슬롯 차단
                return false; // 아이템 제거 실패 반환
            } // 제거 불가 슬롯 처리 완료

            removedItem = slots[slotIndex]; // 제거할 아이템 결과 저장
            slots[slotIndex] = null; // 지정 슬롯 비우기
            quantities[slotIndex] = 0; // 지정 슬롯 수량 비우기
            InventoryChanged?.Invoke(); // HUD 전체 슬롯 갱신 요청
            return true; // 아이템 제거 성공 반환
        } // 전체 제거 처리 완료

        [ContextMenu("Clear Item Inventory")] // Inspector 인벤토리 초기화 메뉴 등록
        public void ClearInventory() // 두 아이템 슬롯 전체 비우기
        { // 슬롯·수량·선택 상태 초기화
            EnsureSlotArrays(); // 초기화 전 슬롯 배열 보장

            for (int slotIndex = 0; slotIndex < Capacity; slotIndex++) // 두 슬롯 전체 순회
            { // 현재 슬롯 데이터 제거
                slots[slotIndex] = null; // 현재 슬롯 아이템 제거
                quantities[slotIndex] = 0; // 현재 슬롯 수량 제거
            } // 전체 슬롯 제거 완료

            selectedSlotIndex = 0; // 첫 슬롯 선택 상태 복원
            SelectedSlotChanged?.Invoke(selectedSlotIndex); // 선택 슬롯 초기화 알림 전달
            InventoryChanged?.Invoke(); // HUD 전체 슬롯 갱신 요청
        } // 인벤토리 초기화 완료

        private bool TryStackItem(ItemDataDefinition itemData, out int stackedSlotIndex) // 기존 아이템 중첩 가능 슬롯 증가 시도
        { // 중첩 가능 수량 확인과 이벤트 전달
            stackedSlotIndex = FindStackableSlot(itemData); // 같은 중첩 아이템 슬롯 검색

            if (stackedSlotIndex < 0) // 중첩 가능 슬롯 없음 여부 확인
            { // 새 슬롯 또는 교체 단계로 진행
                return false; // 중첩 실패 반환
            } // 중첩 불가 처리 완료

            quantities[stackedSlotIndex]++; // 현재 아이템 수량 한 개 증가
            ItemAdded?.Invoke(stackedSlotIndex, itemData); // 아이템 추가 정보 전달
            InventoryChanged?.Invoke(); // HUD 전체 슬롯 갱신 요청
            return true; // 아이템 중첩 성공 반환
        } // 중첩 추가 처리 완료

        private void PlaceNewItem(int slotIndex, ItemDataDefinition itemData) // 빈 슬롯에 새 아이템 한 개 저장
        { // 새 슬롯 데이터와 알림 일괄 처리
            slots[slotIndex] = itemData; // 지정 슬롯에 아이템 저장
            quantities[slotIndex] = 1; // 새 슬롯 수량 한 개 적용
            ItemAdded?.Invoke(slotIndex, itemData); // 아이템 추가 정보 전달
            InventoryChanged?.Invoke(); // HUD 전체 슬롯 갱신 요청
        } // 새 슬롯 저장 완료

        private int FindStackableSlot(ItemDataDefinition itemData) // 같은 아이템 중첩 가능 슬롯 검색
        { // 최대 중첩 수량 미만 슬롯 탐색
            for (int slotIndex = 0; slotIndex < Capacity; slotIndex++) // 두 슬롯 전체 순회
            { // 현재 슬롯 중첩 가능성 검사
                if (slots[slotIndex] == itemData && quantities[slotIndex] < itemData.MaximumStackCount) // 같은 데이터와 최대 수량 미만 여부 확인
                { // 중첩 가능 슬롯 발견
                    return slotIndex; // 중첩 가능 슬롯 번호 반환
                } // 중첩 후보 확정 완료
            } // 두 슬롯 중첩 검사 완료

            return -1; // 중첩 가능 슬롯 없음 반환
        } // 중첩 가능 슬롯 검색 완료

        private int FindFirstEmptySlot() // 앞에서부터 첫 빈 슬롯 번호 검색
        { // 빈 슬롯 순차 탐색
            for (int slotIndex = 0; slotIndex < Capacity; slotIndex++) // 첫 슬롯부터 마지막 슬롯까지 순회
            { // 현재 슬롯 비어 있음 검사
                if (slots[slotIndex] == null) // 현재 슬롯 비어 있음 여부 확인
                { // 첫 빈 슬롯 발견
                    return slotIndex; // 빈 슬롯 번호 반환
                } // 빈 슬롯 확정 완료
            } // 전체 슬롯 빈 공간 검사 완료

            return -1; // 빈 슬롯 없음 반환
        } // 빈 슬롯 검색 완료

        private int CountOccupiedSlots() // 현재 사용 중인 슬롯 수 계산
        { // 두 슬롯의 아이템 존재 여부 집계
            EnsureSlotArrays(); // 계산 전 슬롯 배열 보장
            int itemCount = 0; // 사용 슬롯 수 초기화

            for (int slotIndex = 0; slotIndex < Capacity; slotIndex++) // 두 슬롯 전체 순회
            { // 현재 슬롯 보유 상태 검사
                if (slots[slotIndex] != null) // 현재 슬롯 아이템 존재 여부 확인
                { // 사용 슬롯 집계
                    itemCount++; // 사용 슬롯 수 증가
                } // 현재 슬롯 집계 완료
            } // 두 슬롯 집계 완료

            return itemCount; // 계산된 사용 슬롯 수 반환
        } // 사용 슬롯 수 계산 완료

        private static bool IsValidSlotIndex(int slotIndex) // 슬롯 번호 범위 유효 여부 검사
        { // 0과 1만 허용
            return slotIndex >= 0 && slotIndex < Capacity; // 두 슬롯 범위 포함 여부 반환
        } // 슬롯 번호 검사 완료

        private void EnsureSlotArrays() // 직렬화 배열을 정확한 두 슬롯으로 보정
        { // 기존 Scene 데이터 보존과 배열 크기 복구
            if (slots != null && slots.Length == Capacity && quantities != null && quantities.Length == Capacity) // 올바른 두 배열 여부 확인
            { // 현재 배열을 그대로 유지 가능한 경우 처리
                for (int slotIndex = 0; slotIndex < Capacity; slotIndex++) // 기존 두 슬롯 일관성 확인
                { // 아이템 존재 여부에 맞춘 수량 보정
                    quantities[slotIndex] = slots[slotIndex] == null ? 0 : Mathf.Max(1, quantities[slotIndex]); // 빈 슬롯 0 또는 보유 슬롯 최소 1 적용
                } // 기존 수량 일관성 보정 완료

                return; // 새 배열 생성 생략
            } // 정상 배열 빠른 처리 완료

            ItemDataDefinition[] resizedSlots = new ItemDataDefinition[Capacity]; // 새 두 슬롯 배열 생성
            int[] resizedQuantities = new int[Capacity]; // 새 두 수량 배열 생성

            if (slots != null) // 기존 슬롯 배열 존재 여부 확인
            { // 보존 가능한 기존 아이템 복사
                int copyCount = Mathf.Min(slots.Length, Capacity); // 복사 가능한 슬롯 수 계산

                for (int slotIndex = 0; slotIndex < copyCount; slotIndex++) // 보존 가능한 기존 슬롯 순회
                { // 기존 아이템 참조 보존
                    resizedSlots[slotIndex] = slots[slotIndex]; // 기존 아이템 참조 복사
                } // 기존 슬롯 복사 완료
            } // 기존 슬롯 보존 완료

            if (quantities != null) // 기존 수량 배열 존재 여부 확인
            { // 보존 가능한 기존 수량 복사
                int copyCount = Mathf.Min(quantities.Length, Capacity); // 복사 가능한 수량 수 계산

                for (int slotIndex = 0; slotIndex < copyCount; slotIndex++) // 보존 가능한 기존 수량 순회
                { // 음수가 없는 값으로 기존 수량 보존
                    resizedQuantities[slotIndex] = Mathf.Max(0, quantities[slotIndex]); // 기존 수량 복사
                } // 기존 수량 복사 완료
            } // 기존 수량 보존 완료

            for (int slotIndex = 0; slotIndex < Capacity; slotIndex++) // 두 슬롯 데이터 일관성 보정
            { // 아이템과 수량 조합 검사
                if (resizedSlots[slotIndex] == null) // 빈 슬롯 여부 확인
                { // 빈 슬롯 수량 정리
                    resizedQuantities[slotIndex] = 0; // 빈 슬롯 수량 제거
                } // 빈 슬롯 정리 완료
                else if (resizedQuantities[slotIndex] <= 0) // 기존 에셋의 수량 미설정 여부 확인
                { // 기존 보유 아이템 마이그레이션
                    resizedQuantities[slotIndex] = 1; // 기존 보유 아이템 한 개로 복구
                } // 보유 수량 마이그레이션 완료
            } // 배열 데이터 일관성 보정 완료

            slots = resizedSlots; // 보정된 두 슬롯 배열 저장
            quantities = resizedQuantities; // 보정된 두 수량 배열 저장
        } // 직렬화 슬롯 데이터 보정 완료
    } // 플레이어 인벤토리 기능 정의
} // 프로젝트 아이템 기능 정의
