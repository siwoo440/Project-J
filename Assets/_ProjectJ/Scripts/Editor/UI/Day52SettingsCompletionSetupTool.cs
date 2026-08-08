using ProjectJ.Input; // 프로젝트 InputActionAsset 경로 상수 참조
using ProjectJ.UI; // SettingsMenuController 추가 참조 연결 기능 참조
using TMPro; // TextMeshPro UI 생성 기능 참조
using UnityEditor; // Unity Editor 메뉴와 Undo와 AssetDatabase 기능 참조
using UnityEditor.SceneManagement; // Scene 변경 상태 표시 기능 참조
using UnityEngine; // Unity GameObject와 Color 기능 참조
using UnityEngine.InputSystem; // InputActionAsset Editor 로드 형식 참조
using UnityEngine.SceneManagement; // 현재 Scene 정보 참조
using UnityEngine.UI; // Slider와 Button과 ScrollRect 기능 참조

namespace ProjectJ.Editor // 프로젝트 Editor 도구 네임스페이스 선언
{ // 52일차 설정 메뉴 실사용 완성 자동 구성 기능
    public static class Day52SettingsCompletionSetupTool // 51일차 설정 Canvas에 밝기·UI 음량·키 재지정 UI를 추가하는 도구 선언
    { // MainMenu Scene 전용 52일차 설정 UI 업그레이드 기능 구성
        private const string MainMenuScenePath = "Assets/_ProjectJ/Scenes/Game/MainMenu.unity"; // 대상 MainMenu Scene 경로
        private const string CanvasName = "SettingsMenuCanvas"; // 51일차 자동 생성 설정 Canvas 이름
        private const string RegularFontPath = "Assets/_ProjectJ/Fonts/Source/NotoSansKR-Regular SDF.asset"; // 일반 한글 폰트 경로
        private const string BoldFontPath = "Assets/_ProjectJ/Fonts/Source/NotoSansKR-Bold SDF.asset"; // 굵은 한글 폰트 경로
        private static readonly Color RowColor = new Color(0.055f, 0.075f, 0.11f, 0.92f); // 키 재지정 행 배경 색상
        private static readonly Color ButtonColor = new Color(0.16f, 0.24f, 0.34f, 1f); // 일반 버튼 배경 색상
        private static readonly Color AccentColor = new Color(1f, 0.55f, 0.1f, 1f); // Slider Fill과 강조 버튼 색상
        private static TMP_FontAsset regularFont; // 일반 한글 TextMeshPro 폰트 참조
        private static TMP_FontAsset boldFont; // 굵은 한글 TextMeshPro 폰트 참조

        [MenuItem(ProjectJEditorMenuPaths.UI + "/설정 화면/MainMenu 설정 실사용 완성 (Day 52일차)")] // Project J 상단 52일차 설정 메뉴 등록
        public static void ConfigureDay52SettingsMenu() // MainMenu 설정 Canvas를 52일차 완성 구조로 재구성
        { // 51일차 기본 UI 재생성과 52일차 추가 UI 연결
            Scene scene = SceneManager.GetActiveScene(); // 현재 열린 Scene 조회

            if (scene.path != MainMenuScenePath) // 대상 MainMenu Scene 여부 확인
            { // 잘못된 Scene 실행 방어
                EditorUtility.DisplayDialog("Project J Day 52", "MainMenu Scene을 연 뒤 다시 실행합니다.\n\n" + MainMenuScenePath, "확인"); // 대상 Scene 안내
                return; // 52일차 자동 구성 중단
            } // 잘못된 Scene 실행 방어 마무리

            Day51SettingsMenuSetupTool.ConfigureSettingsMenu(); // 기존 51일차 설정 화면을 깨끗한 기본 상태로 다시 생성
            LoadFonts(); // 52일차 추가 UI용 한글 폰트 준비
            GameObject canvasObject = GameObject.Find(CanvasName); // 다시 생성된 설정 Canvas 조회

            if (canvasObject == null) // 51일차 Canvas 생성 실패 여부 확인
            { // 기반 설정 UI 누락 처리
                Debug.LogError("[ProjectJ][Day52] SettingsMenuCanvas를 찾지 못했습니다."); // Canvas 누락 오류 로그
                return; // 52일차 추가 구성 중단
            } // 기반 설정 UI 누락 처리 마무리

            SettingsMenuController controller = canvasObject.GetComponent<SettingsMenuController>(); // 설정 Canvas의 런타임 컨트롤러 조회
            Transform screenPanel = FindChildRecursive(canvasObject.transform, "ScreenTabPanel"); // 화면 설정 탭 조회
            Transform soundPanel = FindChildRecursive(canvasObject.transform, "SoundTabPanel"); // 사운드 설정 탭 조회
            Transform controlsPanel = FindChildRecursive(canvasObject.transform, "ControlsTabPanel"); // 조작 설정 탭 조회
            Transform muteToggleTransform = FindChildRecursive(canvasObject.transform, "MuteToggle"); // 기존 음소거 Toggle 조회
            TMP_Text controlsInfoText = FindChildRecursive(canvasObject.transform, "ControlsInfoText")?.GetComponent<TMP_Text>(); // 기존 조작 안내 Text 조회

            if (controller == null || screenPanel == null || soundPanel == null || controlsPanel == null) // 필수 51일차 UI 참조 확인
            { // 기반 UI 일부 누락 처리
                Debug.LogError("[ProjectJ][Day52] 51일차 설정 UI의 필수 오브젝트를 찾지 못했습니다."); // 필수 기반 UI 누락 오류 로그
                return; // 52일차 추가 구성 중단
            } // 기반 UI 일부 누락 처리 마무리

            SliderRow brightnessRow = CreateSliderRow("Day52BrightnessRow", screenPanel, "밝기", 0.5f, 1.5f, -520f); // 화면 탭 밝기 Slider 행 생성
            SliderRow uiVolumeRow = CreateSliderRow("Day52UiVolumeRow", soundPanel, "UI", 0f, 1f, -455f); // 사운드 탭 UI 음량 Slider 행 생성

            if (muteToggleTransform is RectTransform muteRect) // 기존 음소거 Toggle RectTransform 여부 확인
            { // UI 음량 행과 겹치지 않도록 음소거 Toggle 이동
                muteRect.anchoredPosition = new Vector2(34f, -545f); // 음소거 Toggle 하단 위치 적용
            } // UI 음량 행과 겹치지 않도록 음소거 Toggle 이동 마무리

            ConfigureControlsInfo(controlsInfoText); // 조작 탭 상단 안내 문구를 52일차 내용으로 변경
            Button resetBindingsButton = CreateButton("ResetBindingsButton", controlsPanel, "기본 키", ButtonColor, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-34f, -78f), new Vector2(140f, 44f)); // 키 기본값 미리보기 버튼 생성
            RebindUiViews rebindUi = CreateRebindScrollView(controlsPanel); // 15개 Keyboard&Mouse 재지정 행 Scroll View 생성
            InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ProjectInputNames.AssetPath); // 프로젝트 InputSystem_Actions 에셋 로드

            if (inputActions == null) // 입력 액션 에셋 로드 실패 여부 확인
            { // 키 재지정 기능 필수 에셋 누락 경고
                Debug.LogError($"[ProjectJ][Day52] InputActionAsset을 찾지 못했습니다. Path={ProjectInputNames.AssetPath}"); // 입력 에셋 누락 오류 로그
            } // 키 재지정 기능 필수 에셋 누락 경고 마무리

            controller.ConfigureDay52Extras(inputActions, brightnessRow.Slider, brightnessRow.ValueText, uiVolumeRow.Slider, uiVolumeRow.ValueText, resetBindingsButton, rebindUi.Buttons, rebindUi.ValueTexts); // 52일차 추가 UI와 InputActionAsset 참조 연결
            EditorUtility.SetDirty(controller); // 새 직렬화 참조를 Scene 저장 대상으로 표시
            EditorSceneManager.MarkSceneDirty(scene); // MainMenu Scene 변경 상태 표시
            Selection.activeGameObject = canvasObject; // 설정 Canvas Hierarchy 선택
            Debug.Log("[ProjectJ][Day52] 밝기·UI 음량·Keyboard&Mouse 키 재지정 UI 구성을 완료했습니다. Ctrl + S로 MainMenu Scene을 저장합니다."); // 52일차 UI 구성 완료 로그
        } // MainMenu 설정 Canvas를 52일차 완성 구조로 재구성 마무리

        private static void LoadFonts() // 52일차 추가 UI용 한글 TextMeshPro 폰트 준비
        { // 프로젝트 Font Asset 우선 로드
            regularFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(RegularFontPath); // 일반 NotoSansKR Font Asset 로드
            boldFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BoldFontPath); // 굵은 NotoSansKR Font Asset 로드
            if (regularFont == null) regularFont = TMP_Settings.defaultFontAsset; // 일반 폰트 누락 시 TMP 기본 폰트 대체
            if (boldFont == null) boldFont = regularFont; // 굵은 폰트 누락 시 일반 폰트 대체
        } // 52일차 추가 UI용 한글 TextMeshPro 폰트 준비 마무리

        private static void ConfigureControlsInfo(TMP_Text infoText) // 조작 탭 안내 문구와 RectTransform 정리
        { // 51일차 예정 문구를 실제 키 재지정 사용법으로 교체
            if (infoText == null) // 기존 ControlsInfoText 누락 여부 확인
            { // 안내 문구 없음 처리
                return; // 안내 문구 수정 생략
            } // 안내 문구 없음 처리 마무리

            infoText.text = "변경 버튼을 누른 뒤 새 키 또는 마우스 버튼을 입력합니다. Esc는 취소입니다."; // 52일차 키 재지정 사용법 적용
            infoText.fontSize = 16f; // Scroll View 공간 확보용 안내 글자 크기 적용
            infoText.textWrappingMode = TextWrappingModes.Normal; // 긴 안내 문구 줄바꿈 허용

            if (infoText.rectTransform != null) // 안내 Text RectTransform 존재 여부 확인
            { // 조작 탭 상단 한 줄 안내 영역 배치
                infoText.rectTransform.anchorMin = new Vector2(0f, 1f); // 왼쪽 상단 기준 최소 앵커 적용
                infoText.rectTransform.anchorMax = new Vector2(1f, 1f); // 오른쪽 상단 기준 최대 앵커 적용
                infoText.rectTransform.pivot = new Vector2(0f, 1f); // 왼쪽 상단 피벗 적용
                infoText.rectTransform.anchoredPosition = new Vector2(34f, -82f); // 조작 탭 안내 위치 적용
                infoText.rectTransform.sizeDelta = new Vector2(-230f, 52f); // 기본 키 버튼 공간을 제외한 안내 크기 적용
            } // 조작 탭 상단 한 줄 안내 영역 배치 마무리
        } // 조작 탭 안내 문구와 RectTransform 정리 마무리

        private static SliderRow CreateSliderRow(string objectName, Transform parent, string label, float minimum, float maximum, float anchoredY) // 설정 이름과 Slider와 값 문구 한 행 생성
        { // 51일차 UI와 같은 어두운 스타일의 추가 설정 행 구성
            RectTransform root = CreateRect(objectName, parent); // Slider 행 루트 RectTransform 생성
            root.anchorMin = new Vector2(0f, 1f); // 가로 Stretch 최소 앵커 적용
            root.anchorMax = new Vector2(1f, 1f); // 가로 Stretch 최대 앵커 적용
            root.pivot = new Vector2(0.5f, 1f); // 상단 중심 피벗 적용
            root.anchoredPosition = new Vector2(0f, anchoredY); // 탭 상단 기준 행 위치 적용
            root.sizeDelta = new Vector2(-68f, 76f); // 좌우 여백을 제외한 행 크기 적용
            CreateText("Label", root, label, 20f, true, TextAlignmentOptions.Left, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(34f, 0f), new Vector2(230f, 40f)); // 설정 항목 이름 생성
            Slider slider = CreateSlider("Slider", root, minimum, maximum, new Vector2(300f, 0f), new Vector2(420f, 34f)); // 사용자 값 Slider 생성
            TMP_Text valueText = CreateText("ValueText", root, "-", 18f, false, TextAlignmentOptions.Right, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-24f, 0f), new Vector2(150f, 40f)); // Slider 현재 값 문구 생성
            return new SliderRow(slider, valueText); // 생성된 Slider와 문구 참조 반환
        } // 설정 이름과 Slider와 값 문구 한 행 생성 마무리

        private static Slider CreateSlider(string objectName, Transform parent, float minimum, float maximum, Vector2 anchoredPosition, Vector2 sizeDelta) // 기본 Unity Slider 시각 요소 생성
        { // 배경과 Fill과 Handle이 연결된 Slider 구성
            RectTransform root = CreateRect(objectName, parent); // Slider 루트 RectTransform 생성
            root.anchorMin = new Vector2(0f, 0.5f); // Slider 왼쪽 중앙 최소 앵커 적용
            root.anchorMax = new Vector2(0f, 0.5f); // Slider 왼쪽 중앙 최대 앵커 적용
            root.pivot = new Vector2(0f, 0.5f); // Slider 왼쪽 중앙 피벗 적용
            root.anchoredPosition = anchoredPosition; // Slider 행 내부 위치 적용
            root.sizeDelta = sizeDelta; // Slider 크기 적용
            Slider slider = Undo.AddComponent<Slider>(root.gameObject); // Slider 컴포넌트 추가
            slider.minValue = minimum; // Slider 최소값 적용
            slider.maxValue = maximum; // Slider 최대값 적용
            slider.direction = Slider.Direction.LeftToRight; // 왼쪽에서 오른쪽 증가 방향 적용
            Image background = CreateImage("Background", root, new Color(0.015f, 0.025f, 0.04f, 1f), new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0f, 12f), false); // Slider 어두운 배경 생성
            RectTransform fillArea = CreateRect("FillArea", root); // Slider Fill 이동 영역 생성
            fillArea.anchorMin = new Vector2(0f, 0.5f); // Fill 영역 왼쪽 중앙 최소 앵커 적용
            fillArea.anchorMax = new Vector2(1f, 0.5f); // Fill 영역 오른쪽 중앙 최대 앵커 적용
            fillArea.pivot = new Vector2(0.5f, 0.5f); // Fill 영역 중심 피벗 적용
            fillArea.sizeDelta = new Vector2(-18f, 12f); // Handle 여유를 제외한 Fill 크기 적용
            Image fill = CreateImage("Fill", fillArea, AccentColor, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, false); // Slider Accent Fill 생성
            RectTransform handleArea = CreateRect("HandleSlideArea", root); // Slider Handle 이동 영역 생성
            handleArea.anchorMin = new Vector2(0f, 0.5f); // Handle 영역 왼쪽 중앙 최소 앵커 적용
            handleArea.anchorMax = new Vector2(1f, 0.5f); // Handle 영역 오른쪽 중앙 최대 앵커 적용
            handleArea.pivot = new Vector2(0.5f, 0.5f); // Handle 영역 중심 피벗 적용
            handleArea.sizeDelta = new Vector2(-20f, 0f); // Handle 이동 여유 적용
            Image handle = CreateImage("Handle", handleArea, Color.white, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(24f, 24f), true); // Slider 흰색 Handle 생성
            slider.fillRect = fill.rectTransform; // Slider Fill RectTransform 연결
            slider.handleRect = handle.rectTransform; // Slider Handle RectTransform 연결
            slider.targetGraphic = handle; // Slider 선택 상태 대상 연결
            background.raycastTarget = false; // Slider 배경 Raycast 비활성화
            fill.raycastTarget = false; // Slider Fill Raycast 비활성화
            return slider; // 완성된 Slider 반환
        } // 기본 Unity Slider 시각 요소 생성 마무리

        private static RebindUiViews CreateRebindScrollView(Transform parent) // 조작 탭 15개 키 재지정 행 Scroll View 생성
        { // 좁은 설정 카드 안에서 모든 기본 키를 확인 가능한 세로 Scroll 구성
            RectTransform scrollRoot = CreateRect("Day52RebindScrollView", parent); // Scroll View 루트 생성
            scrollRoot.anchorMin = new Vector2(0f, 0f); // 조작 탭 왼쪽 하단 최소 앵커 적용
            scrollRoot.anchorMax = new Vector2(1f, 1f); // 조작 탭 오른쪽 상단 최대 앵커 적용
            scrollRoot.offsetMin = new Vector2(34f, 28f); // Scroll View 왼쪽과 아래 여백 적용
            scrollRoot.offsetMax = new Vector2(-34f, -150f); // Scroll View 오른쪽과 위 여백 적용
            Image scrollBackground = Undo.AddComponent<Image>(scrollRoot.gameObject); // Scroll View 배경 Image 추가
            scrollBackground.color = new Color(0.02f, 0.03f, 0.05f, 0.75f); // Scroll View 어두운 배경색 적용
            ScrollRect scrollRect = Undo.AddComponent<ScrollRect>(scrollRoot.gameObject); // 세로 ScrollRect 컴포넌트 추가
            scrollRect.horizontal = false; // 가로 스크롤 비활성화
            scrollRect.vertical = true; // 세로 스크롤 활성화
            scrollRect.movementType = ScrollRect.MovementType.Clamped; // Content 경계 밖 이동 제한
            scrollRect.scrollSensitivity = 24f; // 마우스 휠 스크롤 감도 적용

            RectTransform viewport = CreateRect("Viewport", scrollRoot); // ScrollRect Viewport 생성
            viewport.anchorMin = Vector2.zero; // Viewport 전체 Stretch 최소 앵커 적용
            viewport.anchorMax = Vector2.one; // Viewport 전체 Stretch 최대 앵커 적용
            viewport.offsetMin = new Vector2(6f, 6f); // Viewport 왼쪽과 아래 안쪽 여백 적용
            viewport.offsetMax = new Vector2(-6f, -6f); // Viewport 오른쪽과 위 안쪽 여백 적용
            Image viewportImage = Undo.AddComponent<Image>(viewport.gameObject); // Mask용 Viewport Image 추가
            viewportImage.color = new Color(0f, 0f, 0f, 0.01f); // 거의 투명한 Mask Graphic 적용
            Mask viewportMask = Undo.AddComponent<Mask>(viewport.gameObject); // Scroll View 영역 Mask 추가
            viewportMask.showMaskGraphic = false; // Mask용 Image 자체 표시 숨김
            scrollRect.viewport = viewport; // ScrollRect Viewport 참조 연결

            RectTransform content = CreateRect("Content", viewport); // 재지정 행 Content 생성
            content.anchorMin = new Vector2(0f, 1f); // Content 상단 Stretch 최소 앵커 적용
            content.anchorMax = new Vector2(1f, 1f); // Content 상단 Stretch 최대 앵커 적용
            content.pivot = new Vector2(0.5f, 1f); // Content 상단 중심 피벗 적용
            content.anchoredPosition = Vector2.zero; // Content 시작 위치 초기화
            content.sizeDelta = Vector2.zero; // LayoutGroup 기반 Content 크기 초기화
            VerticalLayoutGroup layout = Undo.AddComponent<VerticalLayoutGroup>(content.gameObject); // 재지정 행 세로 자동 배치 추가
            layout.padding = new RectOffset(6, 6, 6, 6); // Content 안쪽 여백 적용
            layout.spacing = 6f; // 재지정 행 사이 간격 적용
            layout.childAlignment = TextAnchor.UpperCenter; // 재지정 행 위쪽 정렬 적용
            layout.childControlWidth = true; // 재지정 행 가로 크기 Layout 관리
            layout.childControlHeight = true; // 재지정 행 세로 크기 Layout 관리
            layout.childForceExpandWidth = true; // 재지정 행 가로 폭 전체 사용
            layout.childForceExpandHeight = false; // 재지정 행 세로 강제 확장 비활성화
            ContentSizeFitter fitter = Undo.AddComponent<ContentSizeFitter>(content.gameObject); // Content 세로 크기 자동 계산 추가
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize; // 모든 행의 Preferred Height 합산 적용
            scrollRect.content = content; // ScrollRect Content 참조 연결

            int entryCount = SettingsMenuController.RebindEntryCount; // SettingsMenuController 실제 재지정 대상 개수 조회
            Button[] buttons = new Button[entryCount]; // 키 변경 버튼 배열 생성
            TMP_Text[] valueTexts = new TMP_Text[entryCount]; // 현재 키 문구 배열 생성

            for (int index = 0; index < entryCount; index++) // 모든 기본 Keyboard&Mouse 조작 순회
            { // 현재 조작 한 행 생성
                string label = SettingsMenuController.GetRebindEntryLabel(index); // 현재 조작 한국어 표시 이름 조회
                CreateRebindRow(content, index, label, out Button button, out TMP_Text valueText); // 현재 조작 재지정 행 생성
                buttons[index] = button; // 현재 키 변경 버튼 배열 저장
                valueTexts[index] = valueText; // 현재 키 문구 배열 저장
            } // 현재 조작 한 행 생성 마무리

            return new RebindUiViews(buttons, valueTexts); // Scroll View 재지정 UI 참조 묶음 반환
        } // 조작 탭 15개 키 재지정 행 Scroll View 생성 마무리

        private static void CreateRebindRow(Transform parent, int index, string label, out Button button, out TMP_Text valueText) // 키 재지정 한 행 생성
        { // 조작 이름과 현재 키와 변경 버튼 가로 배치
            RectTransform row = CreateRect($"RebindRow_{index:00}", parent); // 재지정 행 RectTransform 생성
            Image rowImage = Undo.AddComponent<Image>(row.gameObject); // 재지정 행 배경 Image 추가
            rowImage.color = RowColor; // 재지정 행 배경 색상 적용
            LayoutElement rowLayout = Undo.AddComponent<LayoutElement>(row.gameObject); // 재지정 행 Layout 크기 요소 추가
            rowLayout.preferredHeight = 52f; // 재지정 행 고정 높이 적용
            HorizontalLayoutGroup horizontal = Undo.AddComponent<HorizontalLayoutGroup>(row.gameObject); // 조작 이름과 키와 버튼 가로 자동 배치 추가
            horizontal.padding = new RectOffset(12, 12, 6, 6); // 재지정 행 안쪽 여백 적용
            horizontal.spacing = 10f; // 가로 요소 사이 간격 적용
            horizontal.childAlignment = TextAnchor.MiddleLeft; // 가로 요소 중앙 왼쪽 정렬
            horizontal.childControlWidth = true; // 자식 가로 LayoutElement 크기 사용
            horizontal.childControlHeight = true; // 자식 세로 LayoutElement 크기 사용
            horizontal.childForceExpandWidth = false; // Preferred Width 유지
            horizontal.childForceExpandHeight = true; // 행 높이 안에서 자식 세로 확장
            TMP_Text labelText = CreateLayoutText("ActionLabel", row, label, 17f, true, TextAlignmentOptions.Left, 260f); // 조작 한국어 이름 생성
            valueText = CreateLayoutText("BindingValue", row, "-", 17f, false, TextAlignmentOptions.Center, 260f); // 현재 Keyboard&Mouse 키 문구 생성
            button = CreateLayoutButton("RebindButton", row, "변경", 110f); // 현재 조작 키 변경 버튼 생성
            labelText.textWrappingMode = TextWrappingModes.NoWrap; // 조작 이름 한 줄 표시 적용
            valueText.textWrappingMode = TextWrappingModes.NoWrap; // 현재 키 한 줄 표시 적용
        } // 키 재지정 한 행 생성 마무리

        private static TMP_Text CreateLayoutText(string objectName, Transform parent, string textValue, float fontSize, bool useBold, TextAlignmentOptions alignment, float preferredWidth) // LayoutGroup 내부 TextMeshPro 생성
        { // 키 재지정 행용 고정 폭 Text 구성
            RectTransform rect = CreateRect(objectName, parent); // Layout Text RectTransform 생성
            TextMeshProUGUI text = Undo.AddComponent<TextMeshProUGUI>(rect.gameObject); // TextMeshProUGUI 컴포넌트 추가
            text.text = textValue; // 초기 표시 문구 적용
            text.font = useBold ? boldFont : regularFont; // 굵기별 한글 Font Asset 적용
            text.fontSize = fontSize; // 글자 크기 적용
            text.fontStyle = useBold ? FontStyles.Bold : FontStyles.Normal; // 글자 굵기 스타일 적용
            text.alignment = alignment; // 글자 정렬 적용
            text.color = Color.white; // 흰색 글자 적용
            text.raycastTarget = false; // Text Raycast 비활성화
            text.overflowMode = TextOverflowModes.Ellipsis; // 긴 키 이름 말줄임 처리
            LayoutElement layout = Undo.AddComponent<LayoutElement>(rect.gameObject); // Text 고정 폭 LayoutElement 추가
            layout.preferredWidth = preferredWidth; // Text Preferred Width 적용
            layout.flexibleWidth = 0f; // 남은 공간에 Text 폭 확장 방지
            return text; // 완성된 Layout Text 반환
        } // LayoutGroup 내부 TextMeshPro 생성 마무리

        private static Button CreateLayoutButton(string objectName, Transform parent, string label, float preferredWidth) // LayoutGroup 내부 Button 생성
        { // 키 재지정 행용 고정 폭 Button 구성
            RectTransform rect = CreateRect(objectName, parent); // Layout Button RectTransform 생성
            Image image = Undo.AddComponent<Image>(rect.gameObject); // Button 배경 Image 추가
            image.color = ButtonColor; // 일반 버튼 배경 색상 적용
            Button button = Undo.AddComponent<Button>(rect.gameObject); // Button 컴포넌트 추가
            button.targetGraphic = image; // Button 상태 표시 대상 Image 연결
            LayoutElement layout = Undo.AddComponent<LayoutElement>(rect.gameObject); // Button 고정 폭 LayoutElement 추가
            layout.preferredWidth = preferredWidth; // Button Preferred Width 적용
            layout.flexibleWidth = 0f; // 남은 공간에 Button 폭 확장 방지
            CreateText("Label", rect, label, 16f, true, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-8f, -4f)); // Button 가운데 한국어 라벨 생성
            return button; // 완성된 Layout Button 반환
        } // LayoutGroup 내부 Button 생성 마무리

        private static Button CreateButton(string objectName, Transform parent, string label, Color backgroundColor, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta) // 일반 고정 배치 Button 생성
        { // 기본 키 버튼 같은 탭 상단 버튼 구성
            Image image = CreateImage(objectName, parent, backgroundColor, anchorMin, anchorMax, pivot, anchoredPosition, sizeDelta, true); // Button 배경 Image 생성
            Button button = Undo.AddComponent<Button>(image.gameObject); // Button 컴포넌트 추가
            button.targetGraphic = image; // Button 상태 대상 Image 연결
            CreateText("Label", image.transform, label, 16f, true, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-8f, -4f)); // Button 가운데 라벨 생성
            return button; // 완성된 Button 반환
        } // 일반 고정 배치 Button 생성 마무리

        private static TMP_Text CreateText(string objectName, Transform parent, string textValue, float fontSize, bool useBold, TextAlignmentOptions alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta) // 일반 TextMeshPro UI 생성
        { // 한글 Font Asset과 RectTransform 공통 적용
            RectTransform rect = CreateRect(objectName, parent); // TextMeshPro RectTransform 생성
            rect.anchorMin = anchorMin; // 최소 앵커 적용
            rect.anchorMax = anchorMax; // 최대 앵커 적용
            rect.pivot = pivot; // 피벗 적용
            rect.anchoredPosition = anchoredPosition; // 앵커 기준 위치 적용
            rect.sizeDelta = sizeDelta; // 앵커 기준 크기 적용
            TextMeshProUGUI text = Undo.AddComponent<TextMeshProUGUI>(rect.gameObject); // TextMeshProUGUI 컴포넌트 추가
            text.text = textValue; // 초기 표시 문구 적용
            text.font = useBold ? boldFont : regularFont; // 굵기별 한글 Font Asset 적용
            text.fontSize = fontSize; // 글자 크기 적용
            text.fontStyle = useBold ? FontStyles.Bold : FontStyles.Normal; // 글자 굵기 스타일 적용
            text.alignment = alignment; // 글자 정렬 적용
            text.color = Color.white; // 흰색 글자 적용
            text.raycastTarget = false; // 불필요한 Text Raycast 비활성화
            text.overflowMode = TextOverflowModes.Ellipsis; // 영역 초과 문구 말줄임 처리
            return text; // 완성된 TextMeshPro 반환
        } // 일반 TextMeshPro UI 생성 마무리

        private static Image CreateImage(string objectName, Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta, bool raycastTarget) // 일반 Canvas Image 생성
        { // RectTransform과 Image 공통 구성
            RectTransform rect = CreateRect(objectName, parent); // Image RectTransform 생성
            rect.anchorMin = anchorMin; // 최소 앵커 적용
            rect.anchorMax = anchorMax; // 최대 앵커 적용
            rect.pivot = pivot; // 피벗 적용
            rect.anchoredPosition = anchoredPosition; // 앵커 기준 위치 적용
            rect.sizeDelta = sizeDelta; // 앵커 기준 크기 적용
            Image image = Undo.AddComponent<Image>(rect.gameObject); // Image 컴포넌트 추가
            image.color = color; // Image 색상 적용
            image.raycastTarget = raycastTarget; // Raycast 사용 여부 적용
            return image; // 완성된 Image 반환
        } // 일반 Canvas Image 생성 마무리

        private static RectTransform CreateRect(string objectName, Transform parent) // 기본 UI RectTransform GameObject 생성
        { // Undo와 UI Layer와 부모 연결 공통 처리
            GameObject gameObject = new GameObject(objectName, typeof(RectTransform)); // RectTransform 기반 UI GameObject 생성
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Day52 Settings UI"); // 52일차 UI 생성 Undo 등록
            int uiLayer = LayerMask.NameToLayer("UI"); // 프로젝트 UI Layer 번호 조회
            if (uiLayer >= 0) gameObject.layer = uiLayer; // UI Layer가 있으면 생성 오브젝트에 적용
            RectTransform rect = gameObject.GetComponent<RectTransform>(); // 생성 RectTransform 조회
            rect.SetParent(parent, false); // 지정 UI 부모에 로컬 값 유지 연결
            rect.localScale = Vector3.one; // UI 로컬 크기 1배 적용
            rect.localRotation = Quaternion.identity; // UI 로컬 회전 초기화
            return rect; // 생성된 RectTransform 반환
        } // 기본 UI RectTransform GameObject 생성 마무리

        private static Transform FindChildRecursive(Transform root, string objectName) // 비활성 오브젝트를 포함한 이름 기반 자식 검색
        { // Day51 Setup Tool이 숨긴 SettingsPanel 내부 오브젝트 조회
            if (root == null) // 검색 루트 누락 여부 확인
            { // 잘못된 검색 요청 방어
                return null; // 검색 실패 반환
            } // 잘못된 검색 요청 방어 마무리

            Transform[] allTransforms = root.GetComponentsInChildren<Transform>(true); // 비활성 자식을 포함한 전체 Transform 조회

            for (int index = 0; index < allTransforms.Length; index++) // 모든 자식 Transform 순회
            { // 현재 오브젝트 이름 비교
                if (allTransforms[index].name == objectName) // 찾는 오브젝트 이름과 일치 여부 확인
                { // 대상 UI 오브젝트 발견
                    return allTransforms[index]; // 일치 Transform 반환
                } // 대상 UI 오브젝트 발견 마무리
            } // 현재 오브젝트 이름 비교 마무리

            return null; // 일치하는 자식 없음 반환
        } // 비활성 오브젝트를 포함한 이름 기반 자식 검색 마무리

        private readonly struct SliderRow // 추가 Slider 행 참조 묶음 선언
        { // Slider와 값 Text 참조 저장
            public SliderRow(Slider slider, TMP_Text valueText) // Slider 행 참조 묶음 생성
            { // 추가 UI 참조 저장
                Slider = slider; // Slider 참조 저장
                ValueText = valueText; // 값 Text 참조 저장
            } // Slider 행 참조 묶음 생성 마무리

            public Slider Slider { get; } // 생성 Slider 반환
            public TMP_Text ValueText { get; } // 생성 값 Text 반환
        } // 추가 Slider 행 참조 묶음 마무리

        private readonly struct RebindUiViews // 키 재지정 Scroll View 참조 묶음 선언
        { // 키 변경 Button 배열과 현재 키 Text 배열 저장
            public RebindUiViews(Button[] buttons, TMP_Text[] valueTexts) // 재지정 UI 참조 묶음 생성
            { // 생성된 재지정 UI 배열 저장
                Buttons = buttons; // 키 변경 Button 배열 저장
                ValueTexts = valueTexts; // 현재 키 Text 배열 저장
            } // 재지정 UI 참조 묶음 생성 마무리

            public Button[] Buttons { get; } // 키 변경 Button 배열 반환
            public TMP_Text[] ValueTexts { get; } // 현재 키 Text 배열 반환
        } // 키 재지정 Scroll View 참조 묶음 마무리
    } // 51일차 설정 Canvas에 밝기·UI 음량·키 재지정 UI를 추가하는 도구 마무리
} // 프로젝트 Editor 도구 네임스페이스 마무리
