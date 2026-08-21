using ProjectJ.Items; // 아이템 시스템 사용
using UnityEngine; // 유니티 기능 사용
using UnityEngine.UI; // Canvas UI 사용

namespace ProjectJ.UI // 프로젝트 UI 네임스페이스
{
    [DisallowMultipleComponent] // 중복 View 방지
    public sealed class ItemInventoryCanvasView : MonoBehaviour // 2슬롯 Canvas 인벤토리 표시
    {
        private static readonly Color PanelColor =
            new Color(0.055f, 0.065f, 0.08f, 0.92f); // 전체 Panel 색상

        private static readonly Color NormalSlotColor =
            new Color(0.12f, 0.14f, 0.17f, 0.96f); // 비선택 슬롯 색상

        private static readonly Color SelectedSlotColor =
            new Color(0.42f, 0.32f, 0.11f, 0.98f); // 선택 슬롯 색상

        private static readonly Color EmptyIconColor =
            new Color(0.22f, 0.24f, 0.28f, 1f); // 빈 아이콘 색상

        private static readonly Color EmptyNameColor =
            new Color(0.62f, 0.65f, 0.70f, 1f); // 빈 슬롯 이름 색상

        private const float SlotSize = 170f; // 정사각형 슬롯 크기
        private const float SlotGap = 18f; // 두 슬롯 사이 간격
        private const float IconPadding = 10f; // 슬롯 안쪽 아이콘 여백
        private const float NameHeight = 34f; // 슬롯 위 아이템 이름 높이

        private PlayerItemInventory inventory; // 표시할 Inventory
        private Image[] slotBackgrounds; // 두 슬롯 배경
        private Image[] iconImages; // 두 슬롯 아이콘
        private Text[] itemNameTexts; // 슬롯 위 아이템 이름

        public static ItemInventoryCanvasView Create(Transform parent) // Canvas UI 런타임 생성
        {
            GameObject canvasObject =
                new GameObject("=== Item Inventory Canvas ==="); // Canvas Root 생성

            canvasObject.transform.SetParent(parent, false); // Runtime Root 아래 배치

            Canvas canvas = canvasObject.AddComponent<Canvas>(); // Canvas 추가
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; // 화면 고정 Overlay 사용
            canvas.sortingOrder = 80; // 다른 World UI보다 앞쪽 배치

            CanvasScaler scaler =
                canvasObject.AddComponent<CanvasScaler>(); // 해상도 대응 Scaler 추가

            scaler.uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize; // 기준 해상도 비율 사용

            scaler.referenceResolution =
                new Vector2(1920f, 1080f); // 기준 해상도

            scaler.screenMatchMode =
                CanvasScaler.ScreenMatchMode.MatchWidthOrHeight; // 화면 비율 대응

            scaler.matchWidthOrHeight = 0.5f; // 가로 세로 중간 기준

            ItemInventoryCanvasView view =
                canvasObject.AddComponent<ItemInventoryCanvasView>(); // View 컴포넌트 추가

            view.BuildUi(); // 실제 Panel과 Slot 생성
            return view; // 생성 View 반환
        }

        public void Bind(PlayerItemInventory newInventory) // Inventory 연결
        {
            if (inventory != null) // 기존 Inventory 연결 검사
            {
                inventory.Changed -= Refresh; // 기존 이벤트 해제
            }

            inventory = newInventory; // 새 Inventory 저장

            if (inventory != null) // 새 Inventory 존재 검사
            {
                inventory.Changed += Refresh; // 변경 이벤트 연결
            }

            Refresh(); // 즉시 화면 갱신
        }

        private void OnDestroy() // View 제거 시 이벤트 정리
        {
            if (inventory != null) // Inventory 존재 검사
            {
                inventory.Changed -= Refresh; // 이벤트 해제
            }
        }

        private void BuildUi() // Canvas 인벤토리 구조 생성
        {
            slotBackgrounds = new Image[PlayerItemInventory.SlotCount]; // 슬롯 배경 배열 생성
            iconImages = new Image[PlayerItemInventory.SlotCount]; // 아이콘 배열 생성
            itemNameTexts = new Text[PlayerItemInventory.SlotCount]; // 이름 Text 배열 생성

            Font font =
                Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // Unity 기본 Font 사용

            float panelWidth =
                (SlotSize * PlayerItemInventory.SlotCount) +
                SlotGap +
                36f; // 두 슬롯과 바깥 여백을 포함한 Panel 폭

            float panelHeight =
                NameHeight +
                SlotSize +
                30f; // 이름 영역과 정사각형 슬롯을 포함한 Panel 높이

            GameObject panelObject =
                CreateUiObject("InventoryPanel", transform); // Panel 오브젝트 생성

            RectTransform panelRect =
                panelObject.GetComponent<RectTransform>(); // Panel RectTransform 저장

            panelRect.anchorMin = new Vector2(1f, 0f); // 화면 오른쪽 아래 기준
            panelRect.anchorMax = new Vector2(1f, 0f); // 화면 오른쪽 아래 기준
            panelRect.pivot = new Vector2(1f, 0f); // 우하단 Pivot
            panelRect.anchoredPosition = new Vector2(-28f, 28f); // 화면 가장자리 여백
            panelRect.sizeDelta = new Vector2(panelWidth, panelHeight); // 확대된 인벤토리 크기

            Image panelImage = panelObject.AddComponent<Image>(); // Panel 배경 추가
            panelImage.color = PanelColor; // 배경 색상 적용

            CreateSlot(
                panelObject.transform,
                font,
                0,
                "Q",
                new Vector2(18f, -NameHeight - 18f)
            ); // 첫 번째 Q 슬롯 생성

            CreateSlot(
                panelObject.transform,
                font,
                1,
                "E",
                new Vector2(18f + SlotSize + SlotGap, -NameHeight - 18f)
            ); // 두 번째 E 슬롯 생성

            Refresh(); // 초기 빈 슬롯 상태 표시
        }

        private void CreateSlot( // 한 개 정사각형 슬롯 UI 생성
            Transform parent,
            Font font,
            int slotIndex,
            string keyLabel,
            Vector2 slotPosition
        )
        {
            Text nameText = CreateText(
                "ItemName_" + (slotIndex + 1),
                parent,
                font,
                "빈 슬롯",
                20,
                TextAnchor.MiddleCenter
            ); // 슬롯 위 아이템 이름 생성

            RectTransform nameRect =
                nameText.GetComponent<RectTransform>(); // 이름 RectTransform 저장

            SetRect(
                nameRect,
                new Vector2(slotPosition.x, -8f),
                new Vector2(SlotSize, NameHeight),
                new Vector2(0f, 1f)
            ); // 슬롯 바로 위에 이름 배치

            nameText.resizeTextForBestFit = true; // 긴 아이템 이름 자동 축소
            nameText.resizeTextMinSize = 12; // 최소 글자 크기
            nameText.resizeTextMaxSize = 20; // 최대 글자 크기
            nameText.fontStyle = FontStyle.Bold; // 아이템 이름 강조
            itemNameTexts[slotIndex] = nameText; // 배열 저장

            GameObject slotObject =
                CreateUiObject("Slot_" + (slotIndex + 1), parent); // 슬롯 Root 생성

            RectTransform slotRect =
                slotObject.GetComponent<RectTransform>(); // 슬롯 RectTransform 저장

            SetRect(
                slotRect,
                slotPosition,
                new Vector2(SlotSize, SlotSize),
                new Vector2(0f, 1f)
            ); // 슬롯을 정사각형으로 설정

            Image background = slotObject.AddComponent<Image>(); // 슬롯 배경 추가
            background.color = NormalSlotColor; // 기본 색상 적용
            slotBackgrounds[slotIndex] = background; // 배열에 저장

            GameObject iconObject =
                CreateUiObject("Icon", slotObject.transform); // 아이콘 오브젝트 생성

            RectTransform iconRect =
                iconObject.GetComponent<RectTransform>(); // 아이콘 RectTransform 저장

            iconRect.anchorMin = Vector2.zero; // 슬롯 전체 기준
            iconRect.anchorMax = Vector2.one; // 슬롯 전체 기준
            iconRect.pivot = new Vector2(0.5f, 0.5f); // 중앙 Pivot
            iconRect.offsetMin = new Vector2(IconPadding, IconPadding); // 좌하단 여백
            iconRect.offsetMax = new Vector2(-IconPadding, -IconPadding); // 우상단 여백

            Image iconImage = iconObject.AddComponent<Image>(); // 아이콘 Image 추가
            iconImage.color = EmptyIconColor; // 빈 슬롯 기본 색상
            iconImage.preserveAspect = true; // 원본 이미지 비율 유지
            iconImage.raycastTarget = false; // UI 클릭 판정 제외
            iconImages[slotIndex] = iconImage; // 배열에 저장

            Text keyText = CreateText(
                "Key",
                slotObject.transform,
                font,
                keyLabel,
                24,
                TextAnchor.MiddleCenter
            ); // Q 또는 E 표시

            RectTransform keyRect =
                keyText.GetComponent<RectTransform>(); // Key RectTransform 저장

            SetRect(
                keyRect,
                new Vector2(8f, -8f),
                new Vector2(34f, 34f),
                new Vector2(0f, 1f)
            ); // 슬롯 왼쪽 위에 키 표시

            Image keyBackground =
                CreateKeyBackground(slotObject.transform, keyRect); // 키 가독성용 배경 생성

            keyBackground.transform.SetSiblingIndex(
                keyText.transform.GetSiblingIndex()
            ); // 키 Text 바로 뒤에 배경 배치

            keyText.transform.SetAsLastSibling(); // 키 Text를 아이콘 위에 표시
        }

        private void Refresh() // Inventory 상태를 Canvas에 반영
        {
            if (
                slotBackgrounds == null ||
                slotBackgrounds.Length != PlayerItemInventory.SlotCount
            ) // UI 생성 전 검사
            {
                return; // 갱신 중단
            }

            for (int i = 0; i < PlayerItemInventory.SlotCount; i++) // 두 슬롯 반복
            {
                bool isSelected =
                    inventory != null &&
                    inventory.SelectedSlotIndex == i; // 선택 슬롯 여부 계산

                slotBackgrounds[i].color =
                    isSelected ? SelectedSlotColor : NormalSlotColor; // 선택 강조 적용

                ItemDefinition definition =
                    inventory != null ? inventory.GetItem(i) : null; // 현재 슬롯 데이터 조회

                if (definition == null) // 빈 슬롯 검사
                {
                    itemNameTexts[i].text = "빈 슬롯"; // 슬롯 위 빈 상태 표시
                    itemNameTexts[i].color = EmptyNameColor; // 빈 슬롯 이름 색상
                    iconImages[i].sprite = null; // Sprite 제거
                    iconImages[i].color = EmptyIconColor; // 빈 슬롯 색상 적용
                    continue; // 다음 슬롯 처리
                }

                itemNameTexts[i].text =
                    string.IsNullOrWhiteSpace(definition.DisplayName)
                        ? definition.ItemId
                        : definition.DisplayName; // 슬롯 위에 현재 아이템 이름 표시

                itemNameTexts[i].color = Color.white; // 보유 아이템 이름 흰색 표시
                iconImages[i].sprite = definition.Icon; // 실제 아이템 Sprite 적용
                iconImages[i].color =
                    definition.Icon != null
                        ? Color.white
                        : GetCategoryColor(definition.Category); // 아이콘 누락 시 카테고리 색상 표시
            }
        }

        private static Image CreateKeyBackground( // Q/E 표시 뒤 작은 배경 생성
            Transform parent,
            RectTransform keyRect
        )
        {
            GameObject backgroundObject =
                CreateUiObject("KeyBackground", parent); // 키 배경 생성

            RectTransform backgroundRect =
                backgroundObject.GetComponent<RectTransform>(); // 배경 RectTransform 저장

            backgroundRect.anchorMin = keyRect.anchorMin; // Key와 동일 Anchor 사용
            backgroundRect.anchorMax = keyRect.anchorMax; // Key와 동일 Anchor 사용
            backgroundRect.pivot = keyRect.pivot; // Key와 동일 Pivot 사용
            backgroundRect.anchoredPosition = keyRect.anchoredPosition; // Key와 같은 위치
            backgroundRect.sizeDelta = keyRect.sizeDelta; // Key와 같은 크기

            Image image = backgroundObject.AddComponent<Image>(); // 배경 Image 추가
            image.color = new Color(0f, 0f, 0f, 0.62f); // 반투명 검정 배경
            image.raycastTarget = false; // 클릭 판정 제외
            return image; // 생성된 배경 반환
        }

        private static Color GetCategoryColor(ItemCategory category) // 아이콘 누락 시 카테고리별 임시 색상
        {
            switch (category)
            {
                case ItemCategory.Mobility:
                    return new Color(0.28f, 0.65f, 0.95f, 1f);

                case ItemCategory.Defense:
                    return new Color(0.35f, 0.82f, 0.58f, 1f);

                case ItemCategory.Offensive:
                    return new Color(0.90f, 0.37f, 0.31f, 1f);

                case ItemCategory.Trap:
                    return new Color(0.76f, 0.55f, 0.24f, 1f);

                default:
                    return new Color(0.64f, 0.48f, 0.88f, 1f);
            }
        }

        private static GameObject CreateUiObject(string objectName, Transform parent) // RectTransform 기반 UI 오브젝트 생성
        {
            GameObject uiObject =
                new GameObject(objectName, typeof(RectTransform)); // UI 오브젝트 생성

            uiObject.transform.SetParent(parent, false); // Canvas 계층에 연결
            return uiObject; // 생성 오브젝트 반환
        }

        private static Text CreateText( // 공통 Text 생성
            string objectName,
            Transform parent,
            Font font,
            string value,
            int fontSize,
            TextAnchor alignment
        )
        {
            GameObject textObject =
                CreateUiObject(objectName, parent); // Text 오브젝트 생성

            Text text = textObject.AddComponent<Text>(); // Text 컴포넌트 추가
            text.font = font; // 기본 Font 적용
            text.text = value; // 초기 문자열 적용
            text.fontSize = fontSize; // 글자 크기 적용
            text.alignment = alignment; // 정렬 적용
            text.color = Color.white; // 기본 흰색 사용
            text.raycastTarget = false; // 클릭 판정 제외
            return text; // Text 반환
        }

        private static void SetRect( // Top-Left 기준 UI Rect 설정
            RectTransform rect,
            Vector2 anchoredPosition,
            Vector2 size,
            Vector2 anchor
        )
        {
            rect.anchorMin = anchor; // 최소 Anchor 설정
            rect.anchorMax = anchor; // 최대 Anchor 설정
            rect.pivot = anchor; // Pivot을 Anchor와 일치
            rect.anchoredPosition = anchoredPosition; // 위치 적용
            rect.sizeDelta = size; // 크기 적용
        }
    }
}
