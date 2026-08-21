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

        private PlayerItemInventory inventory; // 표시할 Inventory
        private Image[] slotBackgrounds; // 두 슬롯 배경
        private Image[] iconImages; // 두 슬롯 아이콘
        private Text[] itemNameTexts; // 두 슬롯 이름
        private Text[] modeTexts; // 두 슬롯 사용 방식

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
            modeTexts = new Text[PlayerItemInventory.SlotCount]; // 방식 Text 배열 생성

            Font font =
                Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // Unity 기본 Font 사용

            GameObject panelObject =
                CreateUiObject("InventoryPanel", transform); // Panel 오브젝트 생성

            RectTransform panelRect =
                panelObject.GetComponent<RectTransform>(); // Panel RectTransform 저장

            panelRect.anchorMin = new Vector2(1f, 0f); // 화면 오른쪽 아래 기준
            panelRect.anchorMax = new Vector2(1f, 0f); // 화면 오른쪽 아래 기준
            panelRect.pivot = new Vector2(1f, 0f); // 우하단 Pivot
            panelRect.anchoredPosition = new Vector2(-28f, 28f); // 화면 가장자리 여백
            panelRect.sizeDelta = new Vector2(390f, 142f); // 전체 인벤토리 크기

            Image panelImage = panelObject.AddComponent<Image>(); // Panel 배경 추가
            panelImage.color = PanelColor; // 배경 색상 적용

            Text title = CreateText( // 제목 Text 생성
                "Title",
                panelObject.transform,
                font,
                "ITEM INVENTORY",
                18,
                TextAnchor.MiddleLeft
            );

            RectTransform titleRect =
                title.GetComponent<RectTransform>(); // 제목 RectTransform 저장

            SetRect(
                titleRect,
                new Vector2(18f, -9f),
                new Vector2(354f, 28f),
                new Vector2(0f, 1f)
            ); // Panel 상단 제목 배치

            CreateSlot( // 첫 번째 Q 슬롯 생성
                panelObject.transform,
                font,
                0,
                "Q",
                new Vector2(18f, -43f)
            );

            CreateSlot( // 두 번째 E 슬롯 생성
                panelObject.transform,
                font,
                1,
                "E",
                new Vector2(201f, -43f)
            );

            Refresh(); // 초기 EMPTY 상태 표시
        }

        private void CreateSlot( // 한 개 슬롯 UI 생성
            Transform parent,
            Font font,
            int slotIndex,
            string keyLabel,
            Vector2 anchoredPosition
        )
        {
            GameObject slotObject =
                CreateUiObject("Slot_" + (slotIndex + 1), parent); // 슬롯 Root 생성

            RectTransform slotRect =
                slotObject.GetComponent<RectTransform>(); // 슬롯 RectTransform 저장

            SetRect(
                slotRect,
                anchoredPosition,
                new Vector2(171f, 82f),
                new Vector2(0f, 1f)
            ); // 슬롯 위치와 크기 설정

            Image background = slotObject.AddComponent<Image>(); // 슬롯 배경 추가
            background.color = NormalSlotColor; // 기본 색상 적용
            slotBackgrounds[slotIndex] = background; // 배열에 저장

            Text keyText = CreateText( // Q 또는 E 표시
                "Key",
                slotObject.transform,
                font,
                "[" + keyLabel + "]",
                19,
                TextAnchor.MiddleCenter
            );

            RectTransform keyRect =
                keyText.GetComponent<RectTransform>(); // Key RectTransform

            SetRect(
                keyRect,
                new Vector2(8f, -8f),
                new Vector2(38f, 28f),
                new Vector2(0f, 1f)
            ); // 슬롯 왼쪽 위 배치

            GameObject iconObject =
                CreateUiObject("Icon", slotObject.transform); // 아이콘 오브젝트 생성

            RectTransform iconRect =
                iconObject.GetComponent<RectTransform>(); // 아이콘 RectTransform

            SetRect(
                iconRect,
                new Vector2(8f, -38f),
                new Vector2(36f, 36f),
                new Vector2(0f, 1f)
            ); // 슬롯 왼쪽 아래 배치

            Image iconImage = iconObject.AddComponent<Image>(); // 아이콘 Image 추가
            iconImage.color = EmptyIconColor; // 빈 아이콘 기본 색상
            iconImage.preserveAspect = true; // Sprite 비율 유지
            iconImages[slotIndex] = iconImage; // 배열에 저장

            Text nameText = CreateText( // 아이템 이름 Text 생성
                "ItemName",
                slotObject.transform,
                font,
                "EMPTY",
                16,
                TextAnchor.MiddleLeft
            );

            RectTransform nameRect =
                nameText.GetComponent<RectTransform>(); // 이름 RectTransform

            SetRect(
                nameRect,
                new Vector2(51f, -12f),
                new Vector2(111f, 30f),
                new Vector2(0f, 1f)
            ); // 이름 위치 설정

            nameText.resizeTextForBestFit = true; // 긴 이름 자동 축소
            nameText.resizeTextMinSize = 10; // 최소 글자 크기
            nameText.resizeTextMaxSize = 16; // 최대 글자 크기
            itemNameTexts[slotIndex] = nameText; // 배열에 저장

            Text modeText = CreateText( // 사용 방식 Text 생성
                "Mode",
                slotObject.transform,
                font,
                "-",
                12,
                TextAnchor.MiddleLeft
            );

            RectTransform modeRect =
                modeText.GetComponent<RectTransform>(); // 방식 RectTransform

            SetRect(
                modeRect,
                new Vector2(51f, -48f),
                new Vector2(111f, 24f),
                new Vector2(0f, 1f)
            ); // 방식 위치 설정

            modeText.color = new Color(0.78f, 0.80f, 0.84f, 1f); // 보조 정보 색상
            modeTexts[slotIndex] = modeText; // 배열에 저장
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
                    itemNameTexts[i].text = "EMPTY"; // 빈 슬롯 이름 표시
                    modeTexts[i].text = "-"; // 사용 방식 숨김
                    iconImages[i].sprite = null; // Sprite 제거
                    iconImages[i].color = EmptyIconColor; // 빈 아이콘 색상 적용
                    continue; // 다음 슬롯 처리
                }

                itemNameTexts[i].text =
                    string.IsNullOrWhiteSpace(definition.DisplayName)
                        ? definition.ItemId
                        : definition.DisplayName; // 표시 이름 적용

                modeTexts[i].text =
                    definition.Category + " / " + definition.UseMode; // 아이템 역할과 사용 방식 표시

                iconImages[i].sprite = definition.Icon; // 아이콘 Sprite 적용
                iconImages[i].color =
                    definition.Icon != null
                        ? Color.white
                        : GetCategoryColor(definition.Category); // 아이콘이 없으면 카테고리 색상 표시
            }
        }

        private static Color GetCategoryColor(ItemCategory category) // 카테고리별 임시 아이콘 색상
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
