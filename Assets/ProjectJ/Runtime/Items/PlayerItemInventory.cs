using System; // Action 이벤트 사용
using UnityEngine; // 유니티 기능 사용

namespace ProjectJ.Items // 아이템 시스템 네임스페이스
{
    [DisallowMultipleComponent] // 중복 인벤토리 방지
    public sealed class PlayerItemInventory : MonoBehaviour // 플레이어 2슬롯 아이템 보관함
    {
        public const int SlotCount = 2; // Project J 아이템 슬롯 수

        [SerializeField] // 인스펙터 직렬화
        private ItemDefinition[] slots = new ItemDefinition[SlotCount]; // 두 개의 아이템 슬롯

        [SerializeField] // 인스펙터 직렬화
        [Range(0, SlotCount - 1)] // 선택 범위 제한
        private int selectedSlotIndex; // 현재 선택 슬롯

        public event Action Changed; // 인벤토리 또는 선택 슬롯 변경 이벤트

        public int SelectedSlotIndex // 현재 선택 슬롯 조회
        {
            get
            {
                return selectedSlotIndex; // 선택 슬롯 반환
            }
        }

        public ItemDefinition SelectedItem // 현재 선택 아이템 조회
        {
            get
            {
                return GetItem(selectedSlotIndex); // 선택 슬롯 아이템 반환
            }
        }

        public ItemDefinition GetItem(int slotIndex) // 지정 슬롯 아이템 조회
        {
            EnsureSlotArray(); // 배열 구조 확인

            if (!IsValidSlot(slotIndex)) // 슬롯 번호 검사
            {
                return null; // 잘못된 슬롯은 없음 반환
            }

            return slots[slotIndex]; // 아이템 반환
        }

        public bool TryAdd(ItemDefinition definition, out int storedSlotIndex) // 빈 슬롯 우선 아이템 저장
        {
            EnsureSlotArray(); // 배열 구조 확인
            storedSlotIndex = -1; // 저장 실패 기본값

            if (definition == null) // 아이템 누락 검사
            {
                return false; // 저장 실패 반환
            }

            for (int i = 0; i < SlotCount; i++) // 빈 슬롯 검색
            {
                if (slots[i] != null) // 사용 중 슬롯 검사
                {
                    continue; // 다음 슬롯 확인
                }

                slots[i] = definition; // 빈 슬롯에 저장
                storedSlotIndex = i; // 저장 위치 기록
                NotifyChanged(); // UI 갱신 이벤트 발생
                return true; // 저장 성공 반환
            }

            slots[selectedSlotIndex] = definition; // 모두 가득 찬 경우 현재 선택 슬롯 교체
            storedSlotIndex = selectedSlotIndex; // 교체 위치 기록
            NotifyChanged(); // UI 갱신 이벤트 발생
            return true; // 저장 성공 반환
        }

        public bool RemoveItem(int slotIndex) // 지정 슬롯 비우기
        {
            EnsureSlotArray(); // 배열 구조 확인

            if (!IsValidSlot(slotIndex) || slots[slotIndex] == null) // 제거 가능 여부 검사
            {
                return false; // 제거 실패 반환
            }

            slots[slotIndex] = null; // 슬롯 비우기
            NotifyChanged(); // UI 갱신 이벤트 발생
            return true; // 제거 성공 반환
        }

        public bool SelectSlot(int slotIndex) // 슬롯 선택
        {
            if (!IsValidSlot(slotIndex)) // 슬롯 번호 검사
            {
                return false; // 선택 실패 반환
            }

            if (selectedSlotIndex == slotIndex) // 이미 선택된 슬롯 검사
            {
                return true; // 상태 변경 없이 성공 반환
            }

            selectedSlotIndex = slotIndex; // 새 슬롯 선택
            NotifyChanged(); // UI 갱신 이벤트 발생
            return true; // 선택 성공 반환
        }

        public void Clear() // 모든 슬롯 초기화
        {
            EnsureSlotArray(); // 배열 구조 확인

            for (int i = 0; i < SlotCount; i++) // 모든 슬롯 반복
            {
                slots[i] = null; // 슬롯 비우기
            }

            selectedSlotIndex = 0; // 첫 슬롯을 기본 선택
            NotifyChanged(); // UI 갱신 이벤트 발생
        }

        private void Awake() // 런타임 구조 준비
        {
            EnsureSlotArray(); // 슬롯 배열 크기 보장
            selectedSlotIndex =
                Mathf.Clamp(selectedSlotIndex, 0, SlotCount - 1); // 선택 슬롯 보정
        }

        private void OnValidate() // 인스펙터 변경 보정
        {
            EnsureSlotArray(); // 슬롯 배열 크기 보장
            selectedSlotIndex =
                Mathf.Clamp(selectedSlotIndex, 0, SlotCount - 1); // 선택 슬롯 보정
        }

        private void EnsureSlotArray() // 항상 정확히 두 슬롯 유지
        {
            if (slots != null && slots.Length == SlotCount) // 정상 배열 검사
            {
                return; // 수정 불필요
            }

            ItemDefinition[] newSlots =
                new ItemDefinition[SlotCount]; // 새 두 슬롯 배열 생성

            if (slots != null) // 기존 배열 존재 검사
            {
                int copyCount =
                    Mathf.Min(slots.Length, SlotCount); // 복사 가능한 슬롯 수 계산

                for (int i = 0; i < copyCount; i++) // 기존 데이터 복사
                {
                    newSlots[i] = slots[i]; // 슬롯 데이터 유지
                }
            }

            slots = newSlots; // 새 배열 적용
        }

        private static bool IsValidSlot(int slotIndex) // 슬롯 번호 유효성 검사
        {
            return slotIndex >= 0 && slotIndex < SlotCount; // 두 슬롯 범위 반환
        }

        private void NotifyChanged() // 변경 이벤트 호출
        {
            Changed?.Invoke(); // 구독 중인 UI 등에 알림
        }
    }
}
