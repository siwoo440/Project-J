using System; // 인벤토리 변경 이벤트 기능 참조
using ProjectJ.Data; // 아이템 데이터 형식 참조
using UnityEngine; // Unity 컴포넌트와 직렬화 기능 참조

namespace ProjectJ.Items // 프로젝트 아이템 기능 네임스페이스 선언
{ // 프로젝트 아이템 기능 묶음
    [DisallowMultipleComponent] // 플레이어당 인벤토리 한 개만 허용
    public sealed class PlayerItemInventory : MonoBehaviour // 플레이어 2슬롯 아이템 인벤토리 선언
    { // 플레이어 아이템 인벤토리 묶음
        public const int Capacity = 2; // 인벤토리 고정 슬롯 수 선언

        [SerializeField] private ItemDataDefinition[] slots = new ItemDataDefinition[Capacity]; // 두 아이템 슬롯 저장
        [SerializeField] private int[] quantities = new int[Capacity]; // 두 슬롯별 현재 수량 저장
        [SerializeField, Range(0, Capacity - 1)] private int selectedSlotIndex; // 현재 선택 슬롯 번호 저장

        public event Action InventoryChanged; // 전체 슬롯 변경 알림 이벤트
        public event Action<int, ItemDataDefinition> ItemAdded; // 아이템 추가 슬롯 알림 이벤트
        public event Action<ItemDataDefinition> ItemRejectedBecauseFull; // 인벤토리 가득 참 알림 이벤트
        public event Action<int> SelectedSlotChanged; // 선택 슬롯 변경 알림 이벤트

        public int SlotCount => Capacity; // 전체 슬롯 수 반환
        public int ItemCount => CountOccupiedSlots(); // 현재 사용 중인 슬롯 수 반환
        public bool IsFull => ItemCount >= Capacity; // 두 슬롯 사용 여부 반환
        public int SelectedSlotIndex => Mathf.Clamp(selectedSlotIndex, 0, Capacity - 1); // 안전하게 보정한 선택 슬롯 반환
        public ItemDataDefinition SelectedItem => GetItemAt(SelectedSlotIndex); // 현재 선택 아이템 반환

        private void Awake() // 실행 시작 시 슬롯 배열 크기 보정
        { // 슬롯 배열 초기화 처리
            EnsureSlotArrays(); // 정확한 두 슬롯 배열 보장
        } // 슬롯 배열 초기화 처리 종료

        private void OnValidate() // Inspector 변경 시 슬롯 배열 크기 보정
        { // Inspector 슬롯 배열 검사 처리
            EnsureSlotArrays(); // 정확한 두 슬롯 배열 보장
            selectedSlotIndex = Mathf.Clamp(selectedSlotIndex, 0, Capacity - 1); // 선택 슬롯 안전 범위 보정
        } // Inspector 슬롯 배열 검사 처리 종료

        public ItemDataDefinition GetItemAt(int slotIndex) // 지정 슬롯의 아이템 조회
        { // 지정 슬롯 아이템 조회 처리
            EnsureSlotArrays(); // 조회 전 슬롯 배열 보장

            if (!IsValidSlotIndex(slotIndex)) // 슬롯 범위 유효 여부 확인
            { // 잘못된 슬롯 처리
                return null; // 빈 조회 결과 반환
            } // 잘못된 슬롯 처리 종료

            return slots[slotIndex]; // 지정 슬롯 아이템 반환
        } // 지정 슬롯 아이템 조회 처리 종료

        public int GetQuantityAt(int slotIndex) // 지정 슬롯 현재 수량 조회
        { // 지정 슬롯 수량 조회 처리
            EnsureSlotArrays(); // 조회 전 수량 배열 보장

            if (!IsValidSlotIndex(slotIndex) || slots[slotIndex] == null) // 슬롯 범위와 아이템 존재 여부 확인
            { // 빈 수량 처리
                return 0; // 보유 수량 없음 반환
            } // 빈 수량 처리 종료

            return Mathf.Max(1, quantities[slotIndex]); // 최소 한 개로 보정한 수량 반환
        } // 지정 슬롯 수량 조회 처리 종료

        public bool SelectSlot(int slotIndex) // Q와 E 입력 기반 슬롯 선택 시도
        { // 슬롯 선택 처리
            if (!IsValidSlotIndex(slotIndex)) // 슬롯 번호 범위 확인
            { // 잘못된 슬롯 선택 처리
                return false; // 슬롯 선택 실패 반환
            } // 잘못된 슬롯 선택 처리 종료

            if (selectedSlotIndex == slotIndex) // 이미 선택된 슬롯 여부 확인
            { // 같은 슬롯 선택 처리
                return true; // 기존 선택 유지 성공 반환
            } // 같은 슬롯 선택 처리 종료

            selectedSlotIndex = slotIndex; // 새 선택 슬롯 저장
            SelectedSlotChanged?.Invoke(selectedSlotIndex); // 선택 슬롯 변경 정보 전달
            InventoryChanged?.Invoke(); // HUD 전체 슬롯 갱신 요청
            return true; // 슬롯 선택 성공 반환
        } // 슬롯 선택 처리 종료

        public bool TryAddItem(ItemDataDefinition itemData, out int placedSlotIndex) // 중첩 슬롯과 빈 슬롯 순서로 아이템 추가 시도
        { // 아이템 추가 처리
            placedSlotIndex = -1; // 실패 기본 슬롯 번호 설정
            EnsureSlotArrays(); // 추가 전 슬롯 배열 보장

            if (itemData == null) // 아이템 데이터 누락 여부 확인
            { // 누락 아이템 처리
                return false; // 아이템 추가 실패 반환
            } // 누락 아이템 처리 종료

            placedSlotIndex = FindStackableSlot(itemData); // 같은 중첩 아이템 슬롯 검색

            if (placedSlotIndex >= 0) // 중첩 가능한 슬롯 존재 여부 확인
            { // 기존 슬롯 수량 증가 처리
                quantities[placedSlotIndex]++; // 현재 아이템 수량 한 개 증가
                ItemAdded?.Invoke(placedSlotIndex, itemData); // 아이템 추가 정보 전달
                InventoryChanged?.Invoke(); // 전체 슬롯 변경 알림 전달
                return true; // 아이템 중첩 성공 반환
            } // 기존 슬롯 수량 증가 처리 종료

            placedSlotIndex = FindFirstEmptySlot(); // 앞에서부터 첫 빈 슬롯 검색

            if (placedSlotIndex < 0) // 빈 슬롯 없음 여부 확인
            { // 가득 찬 인벤토리 처리
                ItemRejectedBecauseFull?.Invoke(itemData); // 가득 참 알림 전달
                return false; // 아이템 추가 실패 반환
            } // 가득 찬 인벤토리 처리 종료

            slots[placedSlotIndex] = itemData; // 찾은 빈 슬롯에 아이템 저장
            quantities[placedSlotIndex] = 1; // 새 슬롯 수량 한 개 적용
            ItemAdded?.Invoke(placedSlotIndex, itemData); // 아이템 추가 정보 전달
            InventoryChanged?.Invoke(); // 전체 슬롯 변경 알림 전달
            return true; // 아이템 추가 성공 반환
        } // 아이템 추가 처리 종료

        public bool TryConsumeSelectedItem(out ItemDataDefinition consumedItem) // 현재 선택 아이템 한 개 소비 시도
        { // 선택 아이템 소비 처리
            return TryConsumeItemAt(SelectedSlotIndex, out consumedItem); // 선택 슬롯 소비 결과 반환
        } // 선택 아이템 소비 처리 종료

        public bool TryConsumeItemAt(int slotIndex, out ItemDataDefinition consumedItem) // 지정 슬롯 아이템 한 개 소비 시도
        { // 지정 슬롯 아이템 소비 처리
            consumedItem = null; // 소비 실패 기본 결과 설정
            EnsureSlotArrays(); // 소비 전 슬롯 배열 보장

            if (!IsValidSlotIndex(slotIndex) || slots[slotIndex] == null) // 슬롯 범위와 아이템 존재 여부 확인
            { // 소비 불가 슬롯 처리
                return false; // 아이템 소비 실패 반환
            } // 소비 불가 슬롯 처리 종료

            consumedItem = slots[slotIndex]; // 소비할 아이템 결과 저장
            quantities[slotIndex] = Mathf.Max(0, quantities[slotIndex] - 1); // 현재 수량 한 개 감소

            if (quantities[slotIndex] <= 0) // 마지막 수량 소비 여부 확인
            { // 빈 슬롯 전환 처리
                slots[slotIndex] = null; // 지정 슬롯 아이템 제거
                quantities[slotIndex] = 0; // 빈 슬롯 수량 초기화
            } // 빈 슬롯 전환 처리 종료

            InventoryChanged?.Invoke(); // 전체 슬롯 변경 알림 전달
            return true; // 아이템 소비 성공 반환
        } // 지정 슬롯 아이템 소비 처리 종료

        public bool TryRemoveItemAt(int slotIndex, out ItemDataDefinition removedItem) // 지정 슬롯 아이템 전체 제거 시도
        { // 아이템 전체 제거 처리
            removedItem = null; // 제거 실패 기본 결과 설정
            EnsureSlotArrays(); // 제거 전 슬롯 배열 보장

            if (!IsValidSlotIndex(slotIndex) || slots[slotIndex] == null) // 슬롯 범위와 아이템 존재 여부 확인
            { // 제거 불가 슬롯 처리
                return false; // 아이템 제거 실패 반환
            } // 제거 불가 슬롯 처리 종료

            removedItem = slots[slotIndex]; // 제거할 아이템 결과 저장
            slots[slotIndex] = null; // 지정 슬롯 비우기
            quantities[slotIndex] = 0; // 지정 슬롯 수량 비우기
            InventoryChanged?.Invoke(); // 전체 슬롯 변경 알림 전달
            return true; // 아이템 제거 성공 반환
        } // 아이템 전체 제거 처리 종료

        [ContextMenu("Clear Item Inventory")] // Inspector 인벤토리 초기화 메뉴 등록
        public void ClearInventory() // 두 아이템 슬롯 전체 비우기
        { // 인벤토리 초기화 처리
            EnsureSlotArrays(); // 초기화 전 슬롯 배열 보장

            for (int slotIndex = 0; slotIndex < Capacity; slotIndex++) // 두 슬롯 전체 순회
            { // 현재 슬롯 초기화 처리
                slots[slotIndex] = null; // 현재 슬롯 아이템 제거
                quantities[slotIndex] = 0; // 현재 슬롯 수량 제거
            } // 현재 슬롯 초기화 처리 종료

            selectedSlotIndex = 0; // 첫 슬롯 선택 상태 복원
            SelectedSlotChanged?.Invoke(selectedSlotIndex); // 선택 슬롯 초기화 알림 전달
            InventoryChanged?.Invoke(); // 전체 슬롯 변경 알림 전달
        } // 인벤토리 초기화 처리 종료

        private int FindStackableSlot(ItemDataDefinition itemData) // 같은 아이템 중첩 가능 슬롯 검색
        { // 중첩 가능 슬롯 검색 처리
            for (int slotIndex = 0; slotIndex < Capacity; slotIndex++) // 두 슬롯 전체 순회
            { // 현재 슬롯 중첩 여부 검사
                if (slots[slotIndex] == itemData && quantities[slotIndex] < itemData.MaximumStackCount) // 같은 데이터와 최대 수량 미만 여부 확인
                { // 중첩 가능 슬롯 발견 처리
                    return slotIndex; // 중첩 가능 슬롯 번호 반환
                } // 중첩 가능 슬롯 발견 처리 종료
            } // 현재 슬롯 중첩 여부 검사 종료

            return -1; // 중첩 가능 슬롯 없음 반환
        } // 중첩 가능 슬롯 검색 처리 종료

        private int FindFirstEmptySlot() // 앞에서부터 첫 빈 슬롯 번호 검색
        { // 빈 슬롯 검색 처리
            for (int slotIndex = 0; slotIndex < Capacity; slotIndex++) // 0번부터 마지막 슬롯까지 순회
            { // 현재 슬롯 검사 처리
                if (slots[slotIndex] == null) // 현재 슬롯 비어 있음 여부 확인
                { // 빈 슬롯 발견 처리
                    return slotIndex; // 첫 빈 슬롯 번호 반환
                } // 빈 슬롯 발견 처리 종료
            } // 현재 슬롯 검사 처리 종료

            return -1; // 빈 슬롯 없음 반환
        } // 빈 슬롯 검색 처리 종료

        private int CountOccupiedSlots() // 현재 사용 중인 슬롯 수 계산
        { // 사용 슬롯 수 계산 처리
            EnsureSlotArrays(); // 계산 전 슬롯 배열 보장
            int itemCount = 0; // 사용 슬롯 수 초기화

            for (int slotIndex = 0; slotIndex < Capacity; slotIndex++) // 두 슬롯 전체 순회
            { // 현재 슬롯 보유 여부 검사 처리
                if (slots[slotIndex] != null) // 현재 슬롯 아이템 존재 여부 확인
                { // 사용 슬롯 집계 처리
                    itemCount++; // 사용 슬롯 수 증가
                } // 사용 슬롯 집계 처리 종료
            } // 현재 슬롯 보유 여부 검사 처리 종료

            return itemCount; // 계산된 사용 슬롯 수 반환
        } // 사용 슬롯 수 계산 처리 종료

        private static bool IsValidSlotIndex(int slotIndex) // 슬롯 번호 범위 유효 여부 검사
        { // 슬롯 번호 범위 검사 처리
            return slotIndex >= 0 && slotIndex < Capacity; // 두 슬롯 범위 포함 여부 반환
        } // 슬롯 번호 범위 검사 처리 종료

        private void EnsureSlotArrays() // 직렬화 배열을 정확한 두 슬롯으로 보정
        { // 슬롯 배열 보정 처리
            if (slots != null && slots.Length == Capacity && quantities != null && quantities.Length == Capacity) // 올바른 두 배열 여부 확인
            { // 올바른 두 배열 처리
                for (int slotIndex = 0; slotIndex < Capacity; slotIndex++) // 기존 두 슬롯 일관성 확인
                { // 현재 기존 슬롯 일관성 처리
                    quantities[slotIndex] = slots[slotIndex] == null ? 0 : Mathf.Max(1, quantities[slotIndex]); // 아이템 존재 여부 기반 수량 보정
                } // 현재 기존 슬롯 일관성 처리 종료

                return; // 새 배열 생성 생략
            } // 올바른 두 배열 처리 종료

            ItemDataDefinition[] resizedSlots = new ItemDataDefinition[Capacity]; // 새 두 슬롯 배열 생성
            int[] resizedQuantities = new int[Capacity]; // 새 두 수량 배열 생성

            if (slots != null) // 기존 슬롯 배열 존재 여부 확인
            { // 기존 슬롯 복사 처리
                int copyCount = Mathf.Min(slots.Length, Capacity); // 복사 가능한 슬롯 수 계산

                for (int slotIndex = 0; slotIndex < copyCount; slotIndex++) // 보존 가능한 기존 슬롯 순회
                { // 기존 슬롯 복사 처리
                    resizedSlots[slotIndex] = slots[slotIndex]; // 기존 아이템 참조 보존
                } // 기존 슬롯 복사 처리 종료
            } // 기존 슬롯 복사 처리 종료

            if (quantities != null) // 기존 수량 배열 존재 여부 확인
            { // 기존 수량 복사 처리
                int copyCount = Mathf.Min(quantities.Length, Capacity); // 복사 가능한 수량 수 계산

                for (int slotIndex = 0; slotIndex < copyCount; slotIndex++) // 보존 가능한 기존 수량 순회
                { // 기존 수량 복사 처리
                    resizedQuantities[slotIndex] = Mathf.Max(0, quantities[slotIndex]); // 음수가 없는 기존 수량 보존
                } // 기존 수량 복사 처리 종료
            } // 기존 수량 복사 처리 종료

            for (int slotIndex = 0; slotIndex < Capacity; slotIndex++) // 두 슬롯 데이터 일관성 보정
            { // 현재 슬롯 일관성 보정 처리
                if (resizedSlots[slotIndex] == null) // 빈 슬롯 여부 확인
                { // 빈 슬롯 수량 처리
                    resizedQuantities[slotIndex] = 0; // 빈 슬롯 수량 제거
                } // 빈 슬롯 수량 처리 종료
                else if (resizedQuantities[slotIndex] <= 0) // 기존 에셋의 수량 미설정 여부 확인
                { // 기존 슬롯 마이그레이션 처리
                    resizedQuantities[slotIndex] = 1; // 기존 보유 아이템 한 개로 복구
                } // 기존 슬롯 마이그레이션 처리 종료
            } // 현재 슬롯 일관성 보정 처리 종료

            slots = resizedSlots; // 보정된 두 슬롯 배열 저장
            quantities = resizedQuantities; // 보정된 두 수량 배열 저장
        } // 슬롯 배열 보정 처리 종료
    } // 플레이어 아이템 인벤토리 묶음 종료
} // 프로젝트 아이템 기능 묶음 종료
