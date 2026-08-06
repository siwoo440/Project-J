using ProjectJ.Data; // 아이템 공통 데이터 형식 참조
using TMPro; // TextMeshPro 텍스트 기능 참조
using UnityEngine; // Unity 컴포넌트와 색상 기능 참조
using UnityEngine.UI; // Canvas Image 기능 참조

namespace ProjectJ.UI // 프로젝트 Canvas UI 네임스페이스 선언
{ // 프로젝트 Canvas UI 기능 묶음
    [DisallowMultipleComponent] // 슬롯 표시 컴포넌트 중복 방지
    public sealed class CanvasItemSlotView : MonoBehaviour // Canvas 아이템 슬롯 한 칸 표시 선언
    { // Canvas 아이템 슬롯 표시 묶음
        [SerializeField] private Image slotBackground; // 슬롯 배경 이미지 참조
        [SerializeField] private Image itemIcon; // 아이템 아이콘 이미지 참조
        [SerializeField] private TMP_Text slotNumberText; // 슬롯 번호 텍스트 참조
        [SerializeField] private TMP_Text itemNameText; // 아이템 이름 텍스트 참조
        [SerializeField] private Color emptyBackgroundColor = new Color(0.08f, 0.11f, 0.16f, 0.94f); // 빈 슬롯 배경 색상
        [SerializeField] private Color occupiedBackgroundColor = new Color(0.12f, 0.18f, 0.26f, 0.96f); // 아이콘 보유 슬롯 배경 색상

        public void Refresh(int slotIndex, ItemDataDefinition itemData) // 전달된 슬롯 번호와 아이템으로 표시 갱신
        { // 아이템 슬롯 표시 갱신 처리
            if (slotNumberText != null) // 슬롯 번호 텍스트 존재 여부 확인
            { // 슬롯 번호 텍스트 갱신 처리
                slotNumberText.text = $"SLOT {slotIndex + 1}"; // 사용자용 1부터 시작하는 슬롯 번호 표시
            } // 슬롯 번호 텍스트 갱신 처리 종료

            if (itemData == null) // 빈 슬롯 여부 확인
            { // 빈 슬롯 표시 처리
                ApplyEmptyState(); // 빈 슬롯 시각 상태 적용
                return; // 보유 아이템 표시 생략
            } // 빈 슬롯 표시 처리 종료

            ApplyOccupiedState(itemData); // 보유 아이템 시각 상태 적용
        } // 아이템 슬롯 표시 갱신 처리 종료

        private void ApplyEmptyState() // 빈 슬롯 시각 상태 적용
        { // 빈 슬롯 시각 상태 처리
            if (slotBackground != null) // 슬롯 배경 이미지 존재 여부 확인
            { // 빈 슬롯 배경 적용 처리
                slotBackground.color = emptyBackgroundColor; // 빈 슬롯 배경 색상 적용
            } // 빈 슬롯 배경 적용 처리 종료

            if (itemIcon != null) // 아이템 아이콘 이미지 존재 여부 확인
            { // 빈 슬롯 아이콘 처리
                itemIcon.sprite = null; // 기존 아이템 아이콘 제거
                itemIcon.enabled = false; // 빈 아이콘 이미지 숨김
            } // 빈 슬롯 아이콘 처리 종료

            if (itemNameText != null) // 아이템 이름 텍스트 존재 여부 확인
            { // 빈 슬롯 이름 처리
                itemNameText.text = "비어 있음"; // 빈 슬롯 안내 문구 표시
                itemNameText.color = new Color(0.68f, 0.72f, 0.78f, 1f); // 빈 슬롯 안내 색상 적용
            } // 빈 슬롯 이름 처리 종료
        } // 빈 슬롯 시각 상태 처리 종료

        private void ApplyOccupiedState(ItemDataDefinition itemData) // 보유 아이템 시각 상태 적용
        { // 보유 아이템 시각 상태 처리
            bool hasIcon = itemData.InventoryIcon != null; // 등록된 아이템 아이콘 존재 여부 확인

            if (slotBackground != null) // 슬롯 배경 이미지 존재 여부 확인
            { // 보유 슬롯 배경 적용 처리
                slotBackground.color = hasIcon ? occupiedBackgroundColor : itemData.PickupColor; // 아이콘 유무 기반 배경 색상 적용
            } // 보유 슬롯 배경 적용 처리 종료

            if (itemIcon != null) // 아이템 아이콘 이미지 존재 여부 확인
            { // 보유 아이템 아이콘 처리
                itemIcon.sprite = itemData.InventoryIcon; // 등록된 아이템 아이콘 연결
                itemIcon.color = Color.white; // 아이콘 원본 색상 유지
                itemIcon.enabled = hasIcon; // 아이콘 등록 시에만 표시
                itemIcon.preserveAspect = true; // 아이콘 원본 비율 유지
            } // 보유 아이템 아이콘 처리 종료

            if (itemNameText != null) // 아이템 이름 텍스트 존재 여부 확인
            { // 보유 아이템 이름 처리
                itemNameText.text = string.IsNullOrWhiteSpace(itemData.DisplayName) ? itemData.DataId : itemData.DisplayName; // 표시 이름 또는 데이터 ID 표시
                itemNameText.color = Color.white; // 보유 아이템 이름 흰색 적용
            } // 보유 아이템 이름 처리 종료
        } // 보유 아이템 시각 상태 처리 종료

#if UNITY_EDITOR // Editor 전용 설정 시작
        public void ConfigureForEditor(Image newSlotBackground, Image newItemIcon, TMP_Text newSlotNumberText, TMP_Text newItemNameText) // 자동 설정 도구용 슬롯 참조 연결
        { // 자동 설정 도구용 슬롯 참조 연결 처리
            slotBackground = newSlotBackground; // 슬롯 배경 이미지 참조 저장
            itemIcon = newItemIcon; // 아이템 아이콘 이미지 참조 저장
            slotNumberText = newSlotNumberText; // 슬롯 번호 텍스트 참조 저장
            itemNameText = newItemNameText; // 아이템 이름 텍스트 참조 저장
        } // 자동 설정 도구용 슬롯 참조 연결 처리 종료
#endif // Editor 전용 설정 종료
    } // Canvas 아이템 슬롯 표시 묶음 종료
} // 프로젝트 Canvas UI 기능 묶음 종료
