using ProjectJ.UI; // 설정 메뉴 런타임 컨트롤러 참조
using TMPro; // TextMeshPro UI 생성 기능 참조
using UnityEditor; // Unity Editor 메뉴와 Undo 기능 참조
using UnityEditor.SceneManagement; // Scene 변경 상태 표시 기능 참조
using UnityEngine; // Unity 오브젝트와 색상 기능 참조
using UnityEngine.EventSystems; // EventSystem 기능 참조
using UnityEngine.InputSystem.UI; // Input System UI 입력 모듈 참조
using UnityEngine.SceneManagement; // 현재 Scene 정보 참조
using UnityEngine.UI; // Canvas UI 컴포넌트 기능 참조

namespace ProjectJ.Editor // 프로젝트 Editor 도구 네임스페이스 선언
{ // 51일차 설정 UI 자동 구성 기능 정의
    public static class Day51SettingsMenuSetupTool // MainMenu 설정 화면 자동 생성 도구 선언
    { // MainMenu Scene 전용 설정 UI 생성 기능 정의
        private const string MainMenuScenePath = "Assets/_ProjectJ/Scenes/Game/MainMenu.unity"; // 대상 MainMenu Scene 경로
        private const string CanvasName = "SettingsMenuCanvas"; // 자동 생성 Canvas 이름
        private const string RegularFontPath = "Assets/_ProjectJ/Fonts/Source/NotoSansKR-Regular SDF.asset"; // 일반 한글 폰트 경로
        private const string BoldFontPath = "Assets/_ProjectJ/Fonts/Source/NotoSansKR-Bold SDF.asset"; // 굵은 한글 폰트 경로
        private static readonly Color BackgroundColor = new Color(0.025f, 0.035f, 0.055f, 1f); // 전체 배경 색상
        private static readonly Color PanelColor = new Color(0.055f, 0.075f, 0.11f, 0.98f); // 카드 배경 색상
        private static readonly Color SubPanelColor = new Color(0.035f, 0.05f, 0.075f, 0.98f); // 탭 배경 색상
        private static readonly Color AccentColor = new Color(1f, 0.55f, 0.1f, 1f); // 적용과 설정 버튼 강조 색상
        private static readonly Color ButtonColor = new Color(0.16f, 0.24f, 0.34f, 1f); // 일반 버튼 색상
        private static TMP_FontAsset regularFont; // 일반 한글 폰트 참조
        private static TMP_FontAsset boldFont; // 굵은 한글 폰트 참조

        [MenuItem(ProjectJEditorMenuPaths.UI + "/설정 화면/MainMenu 설정 UI 구성 (Day 51일차)")] // Project J 상단 메뉴 등록
        public static void ConfigureSettingsMenu() // MainMenu에 설정 메뉴 자동 구성
        { // 51일차 Scene 자동 구성 처리
            Scene scene = SceneManager.GetActiveScene(); // 현재 열린 Scene 조회

            if (scene.path != MainMenuScenePath) // 대상 MainMenu Scene 여부 확인
            { // 잘못된 Scene 처리
                EditorUtility.DisplayDialog("Project J Day 51", "MainMenu Scene을 연 뒤 다시 실행합니다.\n\n" + MainMenuScenePath, "확인"); // 대상 Scene 안내
                return; // 자동 구성 중단
            } // 잘못된 Scene 처리 완료

            LoadFonts(); // 한글 폰트 로드
            RemoveExistingCanvas(); // 기존 Day51 Canvas 제거
            Canvas canvas = CreateCanvas(); // 설정 Canvas 생성
            GameObject mainMenuRoot = CreateMainMenuRoot(canvas.transform); // 임시 MainMenu 화면 생성
            Button openButton = CreateButton("OpenSettingsButton", mainMenuRoot.transform, "설정", AccentColor, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -20f), new Vector2(280f, 72f)); // 설정 진입 버튼 생성
            GameObject settingsPanel = CreateImage("SettingsPanel", canvas.transform, BackgroundColor, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, true).gameObject; // 전체 설정 화면 생성
            Image card = CreateImage("SettingsCard", settingsPanel.transform, PanelColor, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1320f, 820f), false); // 중앙 설정 카드 생성
            CreateText("Title", card.transform, "설정", 40f, true, TextAlignmentOptions.Left, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -28f), new Vector2(300f, 60f)); // 설정 제목 생성
            TMP_Text statusText = CreateText("StatusText", card.transform, "설정 데이터를 준비하는 중입니다.", 15f, false, TextAlignmentOptions.Left, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 78f), new Vector2(-80f, 30f)); // 하단 상태 안내 생성

            RectTransform tabRoot = CreateRect("TabRoot", card.transform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -110f), new Vector2(220f, -220f)); // 왼쪽 탭 영역 생성
            Button[] tabButtons = new Button[4]; // 4개 탭 버튼 배열 생성
            tabButtons[0] = CreateButton("ScreenTabButton", tabRoot, "화면", ButtonColor, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(220f, 64f)); // 화면 탭 버튼 생성
            tabButtons[1] = CreateButton("SoundTabButton", tabRoot, "사운드", ButtonColor, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -78f), new Vector2(220f, 64f)); // 사운드 탭 버튼 생성
            tabButtons[2] = CreateButton("ControlsTabButton", tabRoot, "조작", ButtonColor, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -156f), new Vector2(220f, 64f)); // 조작 탭 버튼 생성
            tabButtons[3] = CreateButton("CameraTabButton", tabRoot, "카메라", ButtonColor, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -234f), new Vector2(220f, 64f)); // 카메라 탭 버튼 생성

            RectTransform contentRoot = CreateRect("ContentRoot", card.transform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(150f, 10f), new Vector2(-380f, -220f)); // 오른쪽 탭 내용 영역 생성
            GameObject[] tabPanels = new GameObject[4]; // 4개 탭 내용 배열 생성
            tabPanels[0] = CreateTabPanel("ScreenTabPanel", contentRoot); // 화면 탭 패널 생성
            tabPanels[1] = CreateTabPanel("SoundTabPanel", contentRoot); // 사운드 탭 패널 생성
            tabPanels[2] = CreateTabPanel("ControlsTabPanel", contentRoot); // 조작 탭 패널 생성
            tabPanels[3] = CreateTabPanel("CameraTabPanel", contentRoot); // 카메라 탭 패널 생성

            CycleRow resolutionRow = CreateCycleRow(tabPanels[0].transform, "해상도", "1920 × 1080", -120f); // 해상도 행 생성
            CycleRow modeRow = CreateCycleRow(tabPanels[0].transform, "화면 모드", "전체 화면 창", -220f); // 화면 모드 행 생성
            CycleRow fpsRow = CreateCycleRow(tabPanels[0].transform, "최대 FPS", "제한 없음", -320f); // 최대 FPS 행 생성
            Toggle vSyncToggle = CreateToggle(tabPanels[0].transform, "VSync 사용", new Vector2(34f, -430f)); // VSync 토글 생성
            CreateTabTitle(tabPanels[0].transform, "화면"); // 화면 탭 제목 생성

            SliderRow masterRow = CreateSliderRow(tabPanels[1].transform, "마스터", 0f, 1f, -140f); // 마스터 음량 행 생성
            SliderRow musicRow = CreateSliderRow(tabPanels[1].transform, "BGM", 0f, 1f, -250f); // BGM 음량 행 생성
            SliderRow sfxRow = CreateSliderRow(tabPanels[1].transform, "SFX", 0f, 1f, -360f); // SFX 음량 행 생성
            Toggle muteToggle = CreateToggle(tabPanels[1].transform, "전체 음소거", new Vector2(34f, -480f)); // 음소거 토글 생성
            CreateTabTitle(tabPanels[1].transform, "사운드"); // 사운드 탭 제목 생성

            CreateTabTitle(tabPanels[2].transform, "조작"); // 조작 탭 제목 생성
            TMP_Text controlsInfo = CreateText("ControlsInfoText", tabPanels[2].transform, "현재 상태 : 기본 키 사용 중\n\n기본 키 재지정 UI는 53일차에서 연결합니다.", 21f, false, TextAlignmentOptions.TopLeft, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(34f, -130f), new Vector2(-68f, 220f)); // 조작 상태 안내 생성
            controlsInfo.textWrappingMode = TextWrappingModes.Normal; // 조작 안내 줄바꿈 허용
            controlsInfo.overflowMode = TextOverflowModes.Overflow; // 조작 안내 전체 표시

            SliderRow mouseRow = CreateSliderRow(tabPanels[3].transform, "마우스 감도", 0.01f, 2f, -150f); // 마우스 감도 행 생성
            SliderRow gamepadRow = CreateSliderRow(tabPanels[3].transform, "게임패드 시점 속도", 30f, 720f, -280f); // 게임패드 감도 행 생성
            gamepadRow.Slider.wholeNumbers = true; // 게임패드 감도 정수 단위 적용
            Toggle invertToggle = CreateToggle(tabPanels[3].transform, "Y축 반전", new Vector2(34f, -420f)); // Y축 반전 토글 생성
            CreateTabTitle(tabPanels[3].transform, "카메라"); // 카메라 탭 제목 생성

            Button defaultsButton = CreateButton("DefaultsButton", card.transform, "기본값", ButtonColor, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-460f, 24f), new Vector2(180f, 56f)); // 기본값 버튼 생성
            Button cancelButton = CreateButton("CancelButton", card.transform, "취소", ButtonColor, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-260f, 24f), new Vector2(180f, 56f)); // 취소 버튼 생성
            Button applyButton = CreateButton("ApplyButton", card.transform, "적용", AccentColor, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-60f, 24f), new Vector2(180f, 56f)); // 적용 버튼 생성

            SettingsMenuController controller = Undo.AddComponent<SettingsMenuController>(canvas.gameObject); // 설정 메뉴 컨트롤러 추가
            controller.ConfigureForEditor(mainMenuRoot, openButton, settingsPanel, statusText, tabButtons, tabPanels, resolutionRow.ValueText, resolutionRow.PreviousButton, resolutionRow.NextButton, modeRow.ValueText, modeRow.PreviousButton, modeRow.NextButton, fpsRow.ValueText, fpsRow.PreviousButton, fpsRow.NextButton, vSyncToggle, masterRow.Slider, masterRow.ValueText, musicRow.Slider, musicRow.ValueText, sfxRow.Slider, sfxRow.ValueText, muteToggle, controlsInfo, mouseRow.Slider, mouseRow.ValueText, gamepadRow.Slider, gamepadRow.ValueText, invertToggle, defaultsButton, cancelButton, applyButton); // 생성 UI 전체 참조 연결
            EditorUtility.SetDirty(controller); // 컨트롤러 저장 대상으로 표시
            settingsPanel.SetActive(false); // 설정 화면 기본 숨김
            EnsureEventSystem(); // Input System EventSystem 보장
            EditorSceneManager.MarkSceneDirty(scene); // MainMenu Scene 변경 표시
            Selection.activeGameObject = canvas.gameObject; // 생성 Canvas 선택
            Debug.Log("[ProjectJ][Day51] MainMenu 설정 UI와 화면·사운드·조작·카메라 4개 탭 구성을 완료했습니다. Ctrl + S로 MainMenu Scene을 저장합니다."); // 자동 구성 완료 로그
        } // 51일차 Scene 자동 구성 처리 완료

        private static void LoadFonts() // 한글 TextMeshPro 폰트 준비
        { // 프로젝트 폰트 우선 로드
            regularFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(RegularFontPath); // 일반 한글 폰트 로드
            boldFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BoldFontPath); // 굵은 한글 폰트 로드
            if (regularFont == null) regularFont = TMP_Settings.defaultFontAsset; // 일반 폰트 누락 시 기본 폰트 사용
            if (boldFont == null) boldFont = regularFont; // 굵은 폰트 누락 시 일반 폰트 사용
        } // 한글 TextMeshPro 폰트 준비 완료

        private static void RemoveExistingCanvas() // 기존 자동 생성 Canvas 제거
        { // 재실행 중복 방지
            GameObject existing = GameObject.Find(CanvasName); // 기존 Canvas 검색
            if (existing != null) Undo.DestroyObjectImmediate(existing); // 기존 Canvas Undo 제거
        } // 기존 자동 생성 Canvas 제거 완료

        private static Canvas CreateCanvas() // 설정 Overlay Canvas 생성
        { // Full HD 기준 Canvas 구성
            GameObject root = new GameObject(CanvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)); // Canvas 오브젝트 생성
            Undo.RegisterCreatedObjectUndo(root, "Create Day51 Settings Canvas"); // Canvas 생성 Undo 등록
            Canvas canvas = root.GetComponent<Canvas>(); // Canvas 컴포넌트 조회
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; // Overlay 방식 적용
            canvas.sortingOrder = 10; // 기본 UI 위 표시 순서 적용
            CanvasScaler scaler = root.GetComponent<CanvasScaler>(); // CanvasScaler 조회
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; // 화면 크기 기반 배율 적용
            scaler.referenceResolution = new Vector2(1920f, 1080f); // Full HD 기준 적용
            scaler.matchWidthOrHeight = 0.5f; // 가로와 세로 동일 비중 적용
            ApplyUiLayer(root); // Canvas UI Layer 적용
            return canvas; // 구성 Canvas 반환
        } // 설정 Overlay Canvas 생성 완료

        private static GameObject CreateMainMenuRoot(Transform parent) // 설정 진입용 임시 MainMenu 생성
        { // 83일차 정식 MainMenu 이전 임시 화면
            Image background = CreateImage("MainMenuRoot", parent, BackgroundColor, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, false); // 전체 배경 생성
            CreateText("GameTitle", background.transform, "PROJECT J", 72f, true, TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -180f), new Vector2(900f, 100f)); // 게임 제목 생성
            CreateText("DevelopmentLabel", background.transform, "51일차 설정 UI 개발 화면", 20f, false, TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -285f), new Vector2(700f, 40f)); // 개발 화면 안내 생성
            return background.gameObject; // 임시 MainMenu 반환
        } // 설정 진입용 임시 MainMenu 생성 완료

        private static GameObject CreateTabPanel(string name, Transform parent) // 공통 탭 패널 생성
        { // ContentRoot 전체 채움 패널
            return CreateImage(name, parent, SubPanelColor, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, false).gameObject; // 탭 패널 반환
        } // 공통 탭 패널 생성 완료

        private static void CreateTabTitle(Transform parent, string title) // 탭 제목 생성
        { // 탭 왼쪽 상단 제목 배치
            CreateText("TabTitle", parent, title, 30f, true, TextAlignmentOptions.Left, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(34f, -28f), new Vector2(400f, 44f)); // 탭 제목 TextMeshPro 생성
        } // 탭 제목 생성 완료

        private static CycleRow CreateCycleRow(Transform parent, string label, string initialValue, float y) // 앞뒤 버튼 선택 행 생성
        { // 해상도와 화면 모드와 FPS 공통 행
            RectTransform root = CreateRect(label + "Row", parent, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(-68f, 70f)); // 선택 행 부모 생성
            CreateText("Label", root, label, 20f, true, TextAlignmentOptions.Left, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(34f, 0f), new Vector2(210f, 40f)); // 설정 이름 생성
            Button previous = CreateButton("PreviousButton", root, "<", ButtonColor, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-420f, 0f), new Vector2(54f, 48f)); // 이전 버튼 생성
            TMP_Text valueText = CreateText("ValueText", root, initialValue, 19f, false, TextAlignmentOptions.Center, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-225f, 0f), new Vector2(320f, 48f)); // 현재 값 생성
            Button next = CreateButton("NextButton", root, ">", ButtonColor, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-30f, 0f), new Vector2(54f, 48f)); // 다음 버튼 생성
            return new CycleRow(valueText, previous, next); // 선택 행 참조 반환
        } // 앞뒤 버튼 선택 행 생성 완료

        private static SliderRow CreateSliderRow(Transform parent, string label, float minimum, float maximum, float y) // Slider 설정 행 생성
        { // 음량과 감도 공통 행
            RectTransform root = CreateRect(label + "Row", parent, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(-68f, 80f)); // Slider 행 부모 생성
            CreateText("Label", root, label, 20f, true, TextAlignmentOptions.Left, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(34f, 0f), new Vector2(260f, 40f)); // 설정 이름 생성
            Slider slider = CreateSlider(root, minimum, maximum); // Slider 생성
            TMP_Text valueText = CreateText("ValueText", root, "-", 18f, false, TextAlignmentOptions.Right, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-24f, 0f), new Vector2(150f, 40f)); // 현재 값 생성
            return new SliderRow(slider, valueText); // Slider 행 참조 반환
        } // Slider 설정 행 생성 완료

        private static Slider CreateSlider(Transform parent, float minimum, float maximum) // 기본 Slider 생성
        { // 배경과 Fill과 Handle 구성
            RectTransform root = CreateRect("Slider", parent, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(300f, 0f), new Vector2(420f, 34f)); // Slider 루트 생성
            Slider slider = Undo.AddComponent<Slider>(root.gameObject); // Slider 컴포넌트 추가
            slider.minValue = minimum; // Slider 최소값 적용
            slider.maxValue = maximum; // Slider 최대값 적용
            Image background = CreateImage("Background", root, new Color(0.015f, 0.025f, 0.04f, 1f), new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(0f, 12f), false); // Slider 배경 생성
            RectTransform fillArea = CreateRect("FillArea", root, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-18f, 12f)); // Fill 영역 생성
            Image fill = CreateImage("Fill", fillArea, AccentColor, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, false); // Fill Image 생성
            RectTransform handleArea = CreateRect("HandleArea", root, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-20f, 0f)); // Handle 영역 생성
            Image handle = CreateImage("Handle", handleArea, Color.white, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(24f, 24f), true); // Handle Image 생성
            slider.fillRect = fill.rectTransform; // Slider Fill 연결
            slider.handleRect = handle.rectTransform; // Slider Handle 연결
            slider.targetGraphic = handle; // Slider 선택 그래픽 연결
            background.raycastTarget = false; // 배경 Raycast 비활성화
            return slider; // 구성 Slider 반환
        } // 기본 Slider 생성 완료

        private static Toggle CreateToggle(Transform parent, string label, Vector2 position) // 체크박스 Toggle 생성
        { // 체크 표시와 라벨 구성
            RectTransform root = CreateRect(label + "Toggle", parent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), position, new Vector2(420f, 50f)); // Toggle 루트 생성
            Toggle toggle = Undo.AddComponent<Toggle>(root.gameObject); // Toggle 컴포넌트 추가
            Image background = CreateImage("Background", root, ButtonColor, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(34f, 34f), true); // 체크박스 배경 생성
            Image checkmark = CreateImage("Checkmark", background.transform, AccentColor, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-10f, -10f), false); // 체크 표시 생성
            CreateText("Label", root, label, 20f, false, TextAlignmentOptions.Left, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f), new Vector2(50f, 0f), new Vector2(-50f, 0f)); // Toggle 라벨 생성
            toggle.targetGraphic = background; // Toggle 배경 연결
            toggle.graphic = checkmark; // Toggle 체크 표시 연결
            return toggle; // 구성 Toggle 반환
        } // 체크박스 Toggle 생성 완료

        private static Button CreateButton(string name, Transform parent, string label, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size) // 공통 Button 생성
        { // 배경 Image와 TextMeshPro 라벨 구성
            Image image = CreateImage(name, parent, color, anchorMin, anchorMax, pivot, position, size, true); // Button 배경 생성
            Button button = Undo.AddComponent<Button>(image.gameObject); // Button 컴포넌트 추가
            button.targetGraphic = image; // Button 대상 그래픽 연결
            CreateText("Label", image.transform, label, 19f, true, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-12f, -8f)); // Button 라벨 생성
            return button; // 구성 Button 반환
        } // 공통 Button 생성 완료

        private static Image CreateImage(string name, Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size, bool raycast) // 공통 Image 생성
        { // RectTransform과 Image 구성
            RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, pivot, position, size); // Image RectTransform 생성
            Image image = Undo.AddComponent<Image>(rect.gameObject); // Image 컴포넌트 추가
            image.color = color; // Image 색상 적용
            image.raycastTarget = raycast; // Raycast 사용 여부 적용
            return image; // 구성 Image 반환
        } // 공통 Image 생성 완료

        private static TMP_Text CreateText(string name, Transform parent, string value, float size, bool isBold, TextAlignmentOptions alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 rectSize) // 공통 TextMeshPro 생성
        { // 폰트와 RectTransform 설정
            RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, pivot, position, rectSize); // Text RectTransform 생성
            TextMeshProUGUI text = Undo.AddComponent<TextMeshProUGUI>(rect.gameObject); // TextMeshProUGUI 추가
            text.text = value; // 표시 문구 적용
            text.fontSize = size; // 글자 크기 적용
            text.fontStyle = isBold ? FontStyles.Bold : FontStyles.Normal; // 글자 스타일 적용
            text.alignment = alignment; // 글자 정렬 적용
            text.color = Color.white; // 기본 흰색 글자 적용
            text.font = isBold ? boldFont : regularFont; // 한글 폰트 적용
            text.raycastTarget = false; // Text Raycast 비활성화
            text.textWrappingMode = TextWrappingModes.NoWrap; // 기본 자동 줄바꿈 비활성화
            text.overflowMode = TextOverflowModes.Ellipsis; // 기본 말줄임 적용
            return text; // 구성 TextMeshPro 반환
        } // 공통 TextMeshPro 생성 완료

        private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size) // 공통 UI RectTransform 생성
        { // UI Layer와 부모 연결 처리
            GameObject target = new GameObject(name, typeof(RectTransform)); // UI GameObject 생성
            Undo.RegisterCreatedObjectUndo(target, "Create Day51 UI"); // 생성 Undo 등록
            ApplyUiLayer(target); // UI Layer 적용
            RectTransform rect = target.GetComponent<RectTransform>(); // RectTransform 조회
            rect.SetParent(parent, false); // 전달 부모 연결
            rect.anchorMin = anchorMin; // 최소 앵커 적용
            rect.anchorMax = anchorMax; // 최대 앵커 적용
            rect.pivot = pivot; // 피벗 적용
            rect.anchoredPosition = position; // 위치 적용
            rect.sizeDelta = size; // 크기 적용
            return rect; // 구성 RectTransform 반환
        } // 공통 UI RectTransform 생성 완료

        private static void ApplyUiLayer(GameObject target) // UI Layer 안전 적용
        { // UI Layer 존재 여부 확인
            int layer = LayerMask.NameToLayer("UI"); // UI Layer 번호 조회
            if (layer >= 0) target.layer = layer; // UI Layer 존재 시 적용
        } // UI Layer 안전 적용 완료

        private static void EnsureEventSystem() // Input System EventSystem 보장
        { // 기존 EventSystem 재사용 또는 생성
            EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include); // 현재 Scene EventSystem 검색

            if (eventSystem == null) // EventSystem 누락 여부 확인
            { // 새 EventSystem 생성
                GameObject root = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule)); // EventSystem과 InputSystemUIInputModule 생성
                Undo.RegisterCreatedObjectUndo(root, "Create Day51 EventSystem"); // EventSystem Undo 등록
                return; // 생성 후 처리 종료
            } // 새 EventSystem 생성 완료

            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null) Undo.AddComponent<InputSystemUIInputModule>(eventSystem.gameObject); // 기존 EventSystem에 Input System UI 모듈 보강
        } // Input System EventSystem 보장 완료

        private readonly struct CycleRow // 순환 선택 행 참조 묶음
        { // 값 문구와 앞뒤 버튼 정의
            public CycleRow(TMP_Text valueText, Button previousButton, Button nextButton) // 참조 묶음 생성
            { // 참조 저장 처리
                ValueText = valueText; // 현재 값 문구 저장
                PreviousButton = previousButton; // 이전 버튼 저장
                NextButton = nextButton; // 다음 버튼 저장
            } // 참조 저장 처리 완료
            public TMP_Text ValueText { get; } // 현재 값 문구 반환
            public Button PreviousButton { get; } // 이전 버튼 반환
            public Button NextButton { get; } // 다음 버튼 반환
        } // 값 문구와 앞뒤 버튼 정의 완료

        private readonly struct SliderRow // Slider 행 참조 묶음
        { // Slider와 값 문구 정의
            public SliderRow(Slider slider, TMP_Text valueText) // 참조 묶음 생성
            { // 참조 저장 처리
                Slider = slider; // Slider 저장
                ValueText = valueText; // 값 문구 저장
            } // 참조 저장 처리 완료
            public Slider Slider { get; } // Slider 반환
            public TMP_Text ValueText { get; } // 값 문구 반환
        } // Slider와 값 문구 정의 완료
    } // MainMenu Scene 전용 설정 UI 생성 기능 정의 완료
} // 51일차 설정 UI 자동 구성 기능 정의 완료
