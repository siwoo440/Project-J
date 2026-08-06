using ProjectJ.Gameplay; // 경기 관리자 기능 참조
using ProjectJ.Items; // 플레이어 아이템 인벤토리 기능 참조
using ProjectJ.Player; // 플레이어 진행과 입력 기능 참조
using ProjectJ.UI; // Canvas HUD와 경기 메뉴 기능 참조
using TMPro; // TextMeshPro UI 생성 기능 참조
using UnityEditor; // Unity Editor 메뉴와 Undo 기능 참조
using UnityEditor.SceneManagement; // Scene 변경 상태 표시 기능 참조
using UnityEngine; // Unity 오브젝트와 색상 기능 참조
using UnityEngine.EventSystems; // Canvas EventSystem 기능 참조
using UnityEngine.InputSystem.UI; // Input System Canvas 입력 모듈 기능 참조
using UnityEngine.SceneManagement; // 현재 Scene 정보 기능 참조
using UnityEngine.UI; // Canvas와 Image와 Layout 기능 참조

namespace ProjectJ.Editor // 프로젝트 Editor 도구 네임스페이스 선언
{ // 프로젝트 Editor 도구 묶음
    public static class Day40CanvasUISetupTool // 40일차 Canvas UI 자동 설정 도구 선언
    { // 40일차 Canvas UI 자동 설정 도구 묶음
        private const string GameScenePath = "Assets/_ProjectJ/Scenes/Game/Game.unity"; // 설정 대상 Game Scene 경로
        private static readonly Color PanelColor = new Color(0.035f, 0.055f, 0.085f, 0.94f); // 공통 HUD 패널 색상
        private static readonly Color PopupPanelColor = new Color(0.055f, 0.075f, 0.11f, 1f); // 공통 팝업 패널 색상
        private static readonly Color OrangeColor = new Color(1f, 0.55f, 0.1f, 1f); // 진행과 주요 버튼 강조 색상
        private static readonly Color GreenColor = new Color(0.2f, 0.8f, 0.35f, 1f); // 스태미나 강조 색상
        private static readonly Color CyanColor = new Color(0.2f, 1f, 0.9f, 1f); // 로컬 플레이어 강조 색상

        [MenuItem("Project J/Day 40/Configure Canvas UI In Game Scene")] // Unity 상단 자동 설정 메뉴 등록
        public static void ConfigureCanvasUiInGameScene() // Game Scene Canvas HUD와 팝업 전체 자동 구성
        { // Game Scene Canvas UI 자동 구성 처리
            Scene activeScene = SceneManager.GetActiveScene(); // 현재 열린 Scene 조회

            if (activeScene.path != GameScenePath) // Game Scene 경로 일치 여부 확인
            { // 잘못된 Scene 처리
                EditorUtility.DisplayDialog("Project J Day 40", "Game Scene을 연 뒤 다시 실행합니다.\n\n대상 경로: " + GameScenePath, "확인"); // 정확한 대상 Scene 안내
                return; // 잘못된 Scene 구성 중단
            } // 잘못된 Scene 처리 종료

            PlayerRespawnController respawnController = Object.FindFirstObjectByType<PlayerRespawnController>(FindObjectsInactive.Include); // 플레이어 부활 관리자 검색
            PlayerHeightProgressController heightProgressController = Object.FindFirstObjectByType<PlayerHeightProgressController>(FindObjectsInactive.Include); // 플레이어 높이 진행 관리자 검색
            PlayerMovementController movementController = Object.FindFirstObjectByType<PlayerMovementController>(FindObjectsInactive.Include); // 플레이어 이동 관리자 검색
            PlayerInputReader inputReader = Object.FindFirstObjectByType<PlayerInputReader>(FindObjectsInactive.Include); // 플레이어 입력 제공자 검색
            PrototypeMatchController matchController = Object.FindFirstObjectByType<PrototypeMatchController>(FindObjectsInactive.Include); // 경기 관리자 검색
            PlayerItemInventory itemInventory = Object.FindFirstObjectByType<PlayerItemInventory>(FindObjectsInactive.Include); // 39일차 두 슬롯 인벤토리 검색

            if (respawnController == null || heightProgressController == null || movementController == null || matchController == null || itemInventory == null) // 필수 게임 데이터 제공자 누락 확인
            { // 필수 게임 데이터 제공자 누락 처리
                EditorUtility.DisplayDialog("Project J Day 40", "PlayerRespawnController, PlayerHeightProgressController, PlayerMovementController, PrototypeMatchController, PlayerItemInventory 연결을 확인합니다.", "확인"); // 누락된 필수 구성 안내
                return; // Canvas UI 자동 구성 중단
            } // 필수 게임 데이터 제공자 누락 처리 종료

            RemoveExistingCanvasRoot(); // 이전 자동 생성 Canvas 안전 제거
            Canvas canvas = CreateGameCanvas(); // 기준 설정이 적용된 GameCanvas 생성
            RectTransform safeArea = CreateUiObject("SafeArea", canvas.transform); // 전체 HUD SafeArea 부모 생성
            SetRect(safeArea, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero); // SafeArea 전체 화면 Stretch 적용
            RectTransform popupRoot = CreateUiObject("PopupRoot", canvas.transform); // 전체 화면 팝업 부모 생성
            SetRect(popupRoot, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero); // 팝업 부모 전체 화면 Stretch 적용

            TopCenterViews topCenterViews = CreateTopCenterViews(safeArea); // 상단 중앙 시간과 순위 UI 생성
            ProgressViews progressViews = CreateProgressViews(safeArea); // 왼쪽 상단 진행 정보 UI 생성
            RankingViews rankingViews = CreateRankingViews(safeArea); // 오른쪽 상단 실시간 순위 UI 생성
            StaminaViews staminaViews = CreateStaminaViews(safeArea); // 왼쪽 하단 스태미나 UI 생성
            CanvasItemSlotView[] itemSlotViews = CreateItemSlotViews(safeArea); // 오른쪽 하단 두 아이템 슬롯 UI 생성
            GameObject respawnNoticePanel = CreateRespawnNotice(safeArea); // 화면 중앙 부활 안내 UI 생성
            ResultViews resultViews = CreateResultViews(popupRoot); // 전체 화면 경기 결과 UI 생성
            RespawnMenuViews respawnMenuViews = CreateRespawnMenuViews(popupRoot); // ESC 경기 메뉴와 부활 암전 UI 생성

            MinimalPlayerHud hud = FindOrCreateHud(); // 기존 HUD 컴포넌트 조회 또는 생성
            hud.ConfigureForEditor(respawnController, heightProgressController, movementController, matchController, itemInventory, topCenterViews.TimerText, topCenterViews.RankText, progressViews.HeightText, progressViews.SectionText, progressViews.CourseFill, progressViews.CheckpointText, progressViews.CourseTopText, staminaViews.StaminaText, staminaViews.StaminaFill, rankingViews.EntryRoot, rankingViews.EntryTemplate, itemSlotViews, respawnNoticePanel, resultViews.RootPanel, resultViews.TitleText, resultViews.ReasonText, resultViews.FinalRankText, resultViews.EntryRoot, resultViews.EntryTemplate); // 새 Canvas HUD와 모든 데이터 제공자 연결
            EditorUtility.SetDirty(hud); // HUD 참조 변경 저장 대상으로 표시

            PrototypeRespawnMenu respawnMenu = respawnController.GetComponent<PrototypeRespawnMenu>(); // 플레이어의 기존 경기 메뉴 컴포넌트 조회

            if (respawnMenu == null) // 기존 경기 메뉴 컴포넌트 누락 여부 확인
            { // 경기 메뉴 컴포넌트 추가 처리
                respawnMenu = Undo.AddComponent<PrototypeRespawnMenu>(respawnController.gameObject); // Player에 Canvas 경기 메뉴 컴포넌트 추가
            } // 경기 메뉴 컴포넌트 추가 처리 종료

            respawnMenu.ConfigureForEditor(respawnController, inputReader, respawnMenuViews.FadePanel, respawnMenuViews.MenuPanel, respawnMenuViews.CheckpointText, respawnMenuViews.RespawnButton, respawnMenuViews.CloseButton); // 새 Canvas 경기 메뉴와 플레이어 데이터 연결
            EditorUtility.SetDirty(respawnMenu); // 경기 메뉴 참조 변경 저장 대상으로 표시
            EnsureInputSystemEventSystem(); // Canvas Button용 Input System EventSystem 보장
            EditorSceneManager.MarkSceneDirty(activeScene); // Game Scene 변경 상태 표시
            Selection.activeGameObject = canvas.gameObject; // 생성된 GameCanvas Hierarchy 선택
            Debug.Log("[ProjectJ][Day40] OnGUI 교체용 Canvas HUD, 2슬롯 인벤토리, ESC 메뉴, 결과 화면 구성을 완료했습니다. Ctrl + S로 Game Scene을 저장합니다."); // 자동 설정 완료 로그 출력
        } // Game Scene Canvas UI 자동 구성 처리 종료

        private static void RemoveExistingCanvasRoot() // 기존 Day40 GameCanvas 안전 제거
        { // 기존 GameCanvas 제거 처리
            GameObject existingCanvas = GameObject.Find("GameCanvas"); // 정확한 이름의 기존 GameCanvas 검색

            if (existingCanvas != null) // 기존 GameCanvas 존재 여부 확인
            { // 기존 GameCanvas 제거 처리
                Undo.DestroyObjectImmediate(existingCanvas); // Undo 가능한 기존 GameCanvas 제거
            } // 기존 GameCanvas 제거 처리 종료
        } // 기존 GameCanvas 제거 처리 종료

        private static Canvas CreateGameCanvas() // 기준 설정이 적용된 Overlay Canvas 생성
        { // GameCanvas 생성 처리
            GameObject canvasObject = new GameObject("GameCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)); // Canvas 필수 컴포넌트 오브젝트 생성
            Undo.RegisterCreatedObjectUndo(canvasObject, "Create Day40 GameCanvas"); // Canvas 생성 Undo 등록
            ApplyUiLayer(canvasObject); // Canvas 오브젝트 UI Layer 적용
            Canvas canvas = canvasObject.GetComponent<Canvas>(); // 생성된 Canvas 컴포넌트 조회
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; // 카메라 없는 화면 Overlay 방식 적용
            canvas.pixelPerfect = false; // 다양한 해상도 대응용 Pixel Perfect 비활성화
            canvas.sortingOrder = 0; // 기본 게임 HUD 표시 순서 적용
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>(); // 생성된 Canvas Scaler 조회
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; // 기준 해상도 기반 UI 크기 조절 적용
            scaler.referenceResolution = new Vector2(1920f, 1080f); // Full HD 기준 해상도 적용
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight; // 가로와 세로 혼합 대응 방식 적용
            scaler.matchWidthOrHeight = 0.5f; // 가로와 세로 동일 비중 적용
            scaler.referencePixelsPerUnit = 100f; // 스프라이트 100픽셀 기준 단위 적용
            GraphicRaycaster raycaster = canvasObject.GetComponent<GraphicRaycaster>(); // 생성된 Graphic Raycaster 조회
            raycaster.ignoreReversedGraphics = true; // 뒤집힌 그래픽 입력 무시 적용
            raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None; // 2D와 3D Collider 기반 UI 차단 비활성화
            return canvas; // 구성된 GameCanvas 반환
        } // GameCanvas 생성 처리 종료

        private static TopCenterViews CreateTopCenterViews(RectTransform parent) // 상단 중앙 시간과 순위 UI 생성
        { // 상단 중앙 UI 생성 처리
            Image panel = CreateImage("TopCenterPanel", parent, PanelColor, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(300f, 104f), false); // 상단 중앙 패널 생성
            TMP_Text timerText = CreateText("MatchTimerText", panel.transform, "10:00", 38f, FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(260f, 46f), Color.white); // 남은 시간 TextMeshPro 생성
            TMP_Text rankText = CreateText("CurrentRankText", panel.transform, "1 / 1", 22f, FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -62f), new Vector2(260f, 28f), CyanColor); // 현재 순위 TextMeshPro 생성
            return new TopCenterViews(timerText, rankText); // 상단 중앙 UI 참조 묶음 반환
        } // 상단 중앙 UI 생성 처리 종료

        private static ProgressViews CreateProgressViews(RectTransform parent) // 왼쪽 상단 진행 정보 UI 생성
        { // 왼쪽 상단 진행 정보 UI 생성 처리
            Image panel = CreateImage("ProgressPanel", parent, PanelColor, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(400f, 292f), false); // 왼쪽 상단 진행 패널 생성
            CreateText("TitleText", panel.transform, "PROJECT J", 24f, FontStyles.Bold, TextAlignmentOptions.Left, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -16f), new Vector2(364f, 34f), Color.white); // 진행 패널 제목 생성
            TMP_Text heightText = CreateText("HeightText", panel.transform, "현재 0.0 m  |  최고 0.0 m", 18f, FontStyles.Normal, TextAlignmentOptions.Left, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -58f), new Vector2(364f, 28f), Color.white); // 높이 정보 TextMeshPro 생성
            TMP_Text sectionText = CreateText("SectionText", panel.transform, "구간 1/5  |  전체 0%", 18f, FontStyles.Normal, TextAlignmentOptions.Left, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -92f), new Vector2(364f, 28f), Color.white); // 구간 정보 TextMeshPro 생성
            Image courseFill = CreateFilledBar("CourseProgressBar", panel.transform, OrangeColor, new Vector2(18f, -128f), new Vector2(364f, 18f), new Vector2(0f, 1f)); // 전체 코스 진행 막대 생성
            TMP_Text checkpointText = CreateText("CheckpointText", panel.transform, "체크포인트 0/0  |  START", 17f, FontStyles.Normal, TextAlignmentOptions.Left, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -160f), new Vector2(364f, 28f), Color.white); // 체크포인트 TextMeshPro 생성
            TMP_Text courseTopText = CreateText("CourseTopText", panel.transform, "정상 지점 : 미도달", 17f, FontStyles.Normal, TextAlignmentOptions.Left, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -194f), new Vector2(364f, 28f), Color.white); // 정상 도달 상태 TextMeshPro 생성
            CreateText("GuideText", panel.transform, "ESC  경기 메뉴", 15f, FontStyles.Normal, TextAlignmentOptions.Left, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -244f), new Vector2(364f, 24f), new Color(0.68f, 0.72f, 0.78f, 1f)); // 조작 안내 TextMeshPro 생성
            return new ProgressViews(heightText, sectionText, courseFill, checkpointText, courseTopText); // 진행 정보 UI 참조 묶음 반환
        } // 왼쪽 상단 진행 정보 UI 생성 처리 종료

        private static RankingViews CreateRankingViews(RectTransform parent) // 오른쪽 상단 실시간 순위 UI 생성
        { // 오른쪽 상단 실시간 순위 UI 생성 처리
            Image panel = CreateImage("RankingPanel", parent, PanelColor, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -24f), new Vector2(360f, 310f), false); // 오른쪽 상단 순위 패널 생성
            CreateText("TitleText", panel.transform, "실시간 순위", 24f, FontStyles.Bold, TextAlignmentOptions.Left, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -16f), new Vector2(324f, 34f), Color.white); // 실시간 순위 제목 생성
            RectTransform entryRoot = CreateUiObject("RankingEntryContainer", panel.transform); // 실시간 순위 항목 부모 생성
            SetRect(entryRoot, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -22f), new Vector2(-36f, -82f)); // 순위 항목 부모 Stretch와 내부 여백 적용
            VerticalLayoutGroup layout = Undo.AddComponent<VerticalLayoutGroup>(entryRoot.gameObject); // 실시간 순위 수직 자동 배치 추가
            layout.spacing = 4f; // 순위 항목 사이 4픽셀 간격 적용
            layout.childAlignment = TextAnchor.UpperLeft; // 순위 항목 왼쪽 위 정렬 적용
            layout.childControlWidth = true; // 부모 너비 기반 항목 너비 제어
            layout.childControlHeight = true; // Layout Element 기반 항목 높이 제어
            layout.childForceExpandWidth = true; // 순위 항목 가로 영역 채움
            layout.childForceExpandHeight = false; // 순위 항목 불필요한 세로 확장 방지
            TMP_Text entryTemplate = CreateText("RankingEntryTemplate", entryRoot, "1위  PLAYER  |  0.0 m", 17f, FontStyles.Normal, TextAlignmentOptions.Left, Vector2.zero, Vector2.one, new Vector2(0f, 1f), Vector2.zero, new Vector2(0f, 28f), Color.white); // 실시간 순위 복제 원본 생성
            LayoutElement entryLayout = Undo.AddComponent<LayoutElement>(entryTemplate.gameObject); // 순위 복제 원본 높이 정보 추가
            entryLayout.preferredHeight = 28f; // 순위 항목 권장 높이 28픽셀 적용
            entryTemplate.gameObject.SetActive(false); // 순위 복제 원본 숨김
            return new RankingViews(entryRoot, entryTemplate); // 실시간 순위 UI 참조 묶음 반환
        } // 오른쪽 상단 실시간 순위 UI 생성 처리 종료

        private static StaminaViews CreateStaminaViews(RectTransform parent) // 왼쪽 하단 스태미나 UI 생성
        { // 왼쪽 하단 스태미나 UI 생성 처리
            Image panel = CreateImage("StaminaPanel", parent, PanelColor, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(24f, 24f), new Vector2(400f, 100f), false); // 왼쪽 하단 스태미나 패널 생성
            TMP_Text staminaText = CreateText("StaminaText", panel.transform, "스태미나  100%", 20f, FontStyles.Bold, TextAlignmentOptions.Left, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -14f), new Vector2(364f, 30f), Color.white); // 스태미나 백분율 TextMeshPro 생성
            Image staminaFill = CreateFilledBar("StaminaBar", panel.transform, GreenColor, new Vector2(18f, 18f), new Vector2(364f, 22f), new Vector2(0f, 0f)); // 스태미나 진행 막대 생성
            return new StaminaViews(staminaText, staminaFill); // 스태미나 UI 참조 묶음 반환
        } // 왼쪽 하단 스태미나 UI 생성 처리 종료

        private static CanvasItemSlotView[] CreateItemSlotViews(RectTransform parent) // 오른쪽 하단 두 아이템 슬롯 UI 생성
        { // 오른쪽 하단 두 아이템 슬롯 UI 생성 처리
            Image panel = CreateImage("ItemSlotPanel", parent, PanelColor, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-24f, 24f), new Vector2(320f, 166f), false); // 오른쪽 하단 아이템 슬롯 패널 생성
            CreateText("TitleText", panel.transform, "보유 아이템", 20f, FontStyles.Bold, TextAlignmentOptions.Left, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(16f, -12f), new Vector2(288f, 28f), Color.white); // 아이템 슬롯 패널 제목 생성
            CanvasItemSlotView[] slotViews = new CanvasItemSlotView[PlayerItemInventory.Capacity]; // 정확한 두 슬롯 표시 배열 생성

            for (int slotIndex = 0; slotIndex < PlayerItemInventory.Capacity; slotIndex++) // 두 아이템 슬롯 순회
            { // 현재 아이템 슬롯 생성 처리
                float slotX = 16f + slotIndex * 148f; // 슬롯 번호 기반 가로 위치 계산
                Image slotBackground = CreateImage($"ItemSlot{slotIndex + 1:00}", panel.transform, new Color(0.08f, 0.11f, 0.16f, 0.94f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(slotX, 16f), new Vector2(132f, 112f), false); // 현재 아이템 슬롯 배경 생성
                CanvasItemSlotView slotView = Undo.AddComponent<CanvasItemSlotView>(slotBackground.gameObject); // 현재 슬롯 표시 기능 추가
                TMP_Text slotNumberText = CreateText("SlotNumberText", slotBackground.transform, $"SLOT {slotIndex + 1}", 14f, FontStyles.Bold, TextAlignmentOptions.Left, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f, -8f), new Vector2(112f, 20f), new Color(0.72f, 0.76f, 0.82f, 1f)); // 슬롯 번호 TextMeshPro 생성
                Image itemIcon = CreateImage("ItemIcon", slotBackground.transform, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 4f), new Vector2(52f, 52f), false); // 아이템 아이콘 Image 생성
                itemIcon.enabled = false; // 최초 빈 슬롯 아이콘 숨김
                TMP_Text itemNameText = CreateText("ItemNameText", slotBackground.transform, "비어 있음", 14f, FontStyles.Normal, TextAlignmentOptions.Center, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 8f), new Vector2(112f, 22f), new Color(0.68f, 0.72f, 0.78f, 1f)); // 아이템 이름 TextMeshPro 생성
                slotView.ConfigureForEditor(slotBackground, itemIcon, slotNumberText, itemNameText); // 현재 슬롯 표시 참조 연결
                EditorUtility.SetDirty(slotView); // 현재 슬롯 참조 변경 저장 대상으로 표시
                slotViews[slotIndex] = slotView; // 현재 슬롯 표시 배열 등록
            } // 현재 아이템 슬롯 생성 처리 종료

            return slotViews; // 구성된 두 아이템 슬롯 표시 반환
        } // 오른쪽 하단 두 아이템 슬롯 UI 생성 처리 종료

        private static GameObject CreateRespawnNotice(RectTransform parent) // 화면 중앙 부활 안내 UI 생성
        { // 화면 중앙 부활 안내 UI 생성 처리
            Image panel = CreateImage("RespawnNoticePanel", parent, new Color(0.02f, 0.035f, 0.06f, 0.96f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(460f, 120f), false); // 화면 중앙 부활 안내 패널 생성
            CreateText("TitleText", panel.transform, "추락", 30f, FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -16f), new Vector2(420f, 42f), Color.white); // 부활 안내 제목 생성
            CreateText("MessageText", panel.transform, "체크포인트로 복귀 중", 19f, FontStyles.Normal, TextAlignmentOptions.Center, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 16f), new Vector2(420f, 30f), new Color(0.78f, 0.84f, 0.92f, 1f)); // 부활 안내 내용 생성
            panel.gameObject.SetActive(false); // 최초 부활 안내 패널 숨김
            return panel.gameObject; // 부활 안내 패널 반환
        } // 화면 중앙 부활 안내 UI 생성 처리 종료

        private static ResultViews CreateResultViews(RectTransform parent) // 경기 결과 전체 화면 UI 생성
        { // 경기 결과 전체 화면 UI 생성 처리
            Image backdrop = CreateImage("MatchResultPanel", parent, new Color(0f, 0f, 0f, 0.78f), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, true); // 경기 결과 전체 화면 암전과 입력 차단 생성
            Image panel = CreateImage("ResultContentPanel", backdrop.transform, PopupPanelColor, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(640f, 520f), true); // 경기 결과 중앙 내용 패널 생성
            TMP_Text titleText = CreateText("ResultTitleText", panel.transform, "경기 종료", 38f, FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(584f, 52f), Color.white); // 경기 결과 제목 TextMeshPro 생성
            TMP_Text reasonText = CreateText("ResultReasonText", panel.transform, "종료 원인 : 미정", 19f, FontStyles.Normal, TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -88f), new Vector2(584f, 30f), Color.white); // 경기 종료 원인 TextMeshPro 생성
            TMP_Text finalRankText = CreateText("FinalRankText", panel.transform, "최종 순위 : 1 / 1", 24f, FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -130f), new Vector2(584f, 34f), CyanColor); // 플레이어 최종 순위 TextMeshPro 생성
            RectTransform entryRoot = CreateUiObject("ResultEntryContainer", panel.transform); // 최종 순위 항목 부모 생성
            SetRect(entryRoot, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, -48f), new Vector2(-56f, -224f)); // 최종 순위 항목 부모 Stretch와 내부 여백 적용
            VerticalLayoutGroup layout = Undo.AddComponent<VerticalLayoutGroup>(entryRoot.gameObject); // 최종 순위 수직 자동 배치 추가
            layout.spacing = 6f; // 최종 순위 항목 사이 6픽셀 간격 적용
            layout.childAlignment = TextAnchor.UpperLeft; // 최종 순위 항목 왼쪽 위 정렬 적용
            layout.childControlWidth = true; // 부모 너비 기반 항목 너비 제어
            layout.childControlHeight = true; // Layout Element 기반 항목 높이 제어
            layout.childForceExpandWidth = true; // 최종 순위 항목 가로 영역 채움
            layout.childForceExpandHeight = false; // 최종 순위 항목 불필요한 세로 확장 방지
            TMP_Text entryTemplate = CreateText("ResultEntryTemplate", entryRoot, "1위  PLAYER  |  0.0 m", 18f, FontStyles.Normal, TextAlignmentOptions.Left, Vector2.zero, Vector2.one, new Vector2(0f, 1f), Vector2.zero, new Vector2(0f, 30f), Color.white); // 최종 순위 복제 원본 생성
            LayoutElement entryLayout = Undo.AddComponent<LayoutElement>(entryTemplate.gameObject); // 최종 순위 복제 원본 높이 정보 추가
            entryLayout.preferredHeight = 30f; // 최종 순위 항목 권장 높이 30픽셀 적용
            entryTemplate.gameObject.SetActive(false); // 최종 순위 복제 원본 숨김
            backdrop.gameObject.SetActive(false); // 최초 경기 결과 전체 화면 숨김
            return new ResultViews(backdrop.gameObject, titleText, reasonText, finalRankText, entryRoot, entryTemplate); // 경기 결과 UI 참조 묶음 반환
        } // 경기 결과 전체 화면 UI 생성 처리 종료

        private static RespawnMenuViews CreateRespawnMenuViews(RectTransform parent) // ESC 경기 메뉴와 부활 암전 UI 생성
        { // ESC 경기 메뉴와 부활 암전 UI 생성 처리
            Image fade = CreateImage("FadePanel", parent, new Color(0f, 0f, 0f, 0.68f), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, true); // 전체 화면 반투명 검은색 암전 생성
            Image menuPanel = CreateImage("RespawnMenuPanel", parent, PopupPanelColor, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(540f, 340f), true); // 화면 중앙 ESC 경기 메뉴 패널 생성
            CreateText("TitleText", menuPanel.transform, "경기 메뉴", 32f, FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -24f), new Vector2(492f, 44f), Color.white); // 경기 메뉴 제목 TextMeshPro 생성
            TMP_Text checkpointText = CreateText("CheckpointText", menuPanel.transform, "현재 체크포인트 : START", 19f, FontStyles.Normal, TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -82f), new Vector2(492f, 30f), Color.white); // 현재 체크포인트 TextMeshPro 생성
            CreateText("GuideText", menuPanel.transform, "직접 부활은 확인과 재사용 대기시간 없이 즉시 실행됩니다.", 16f, FontStyles.Normal, TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -122f), new Vector2(492f, 44f), new Color(0.78f, 0.84f, 0.92f, 1f)); // 직접 부활 규칙 안내 TextMeshPro 생성
            Button respawnButton = CreateButton("RespawnButton", menuPanel.transform, "마지막 체크포인트에서 부활", OrangeColor, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 92f), new Vector2(440f, 54f)); // 마지막 체크포인트 부활 버튼 생성
            Button closeButton = CreateButton("CloseButton", menuPanel.transform, "경기로 돌아가기", new Color(0.18f, 0.25f, 0.34f, 1f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 26f), new Vector2(440f, 48f)); // 경기 복귀 버튼 생성
            fade.gameObject.SetActive(false); // 최초 전체 화면 암전 숨김
            menuPanel.gameObject.SetActive(false); // 최초 ESC 경기 메뉴 숨김
            return new RespawnMenuViews(fade.gameObject, menuPanel.gameObject, checkpointText, respawnButton, closeButton); // 경기 메뉴 UI 참조 묶음 반환
        } // ESC 경기 메뉴와 부활 암전 UI 생성 처리 종료

        private static MinimalPlayerHud FindOrCreateHud() // 기존 HUD 조회 또는 Canvas HUD 컴포넌트 생성
        { // HUD 조회 또는 생성 처리
            MinimalPlayerHud hud = Object.FindFirstObjectByType<MinimalPlayerHud>(FindObjectsInactive.Include); // 기존 HUD 컴포넌트 검색

            if (hud != null) // 기존 HUD 컴포넌트 존재 여부 확인
            { // 기존 HUD 사용 처리
                return hud; // 기존 HUD 컴포넌트 반환
            } // 기존 HUD 사용 처리 종료

            GameObject hudObject = new GameObject("GameplayHUD"); // Canvas HUD 데이터 연결용 오브젝트 생성
            Undo.RegisterCreatedObjectUndo(hudObject, "Create Day40 GameplayHUD"); // HUD 오브젝트 생성 Undo 등록
            return Undo.AddComponent<MinimalPlayerHud>(hudObject); // Canvas HUD 컴포넌트 추가 후 반환
        } // HUD 조회 또는 생성 처리 종료

        private static void EnsureInputSystemEventSystem() // Input System 기반 EventSystem 보장
        { // Input System EventSystem 보장 처리
            EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include); // 현재 Scene EventSystem 검색

            if (eventSystem == null) // 기존 EventSystem 누락 여부 확인
            { // 새 EventSystem 생성 처리
                GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule)); // Input System EventSystem 오브젝트 생성
                Undo.RegisterCreatedObjectUndo(eventSystemObject, "Create Day40 EventSystem"); // EventSystem 생성 Undo 등록
                return; // 기존 EventSystem 정리 생략
            } // 새 EventSystem 생성 처리 종료

            StandaloneInputModule standaloneModule = eventSystem.GetComponent<StandaloneInputModule>(); // 구 Input Manager UI 모듈 조회

            if (standaloneModule != null) // 구 UI 입력 모듈 존재 여부 확인
            { // 구 UI 입력 모듈 제거 처리
                Undo.DestroyObjectImmediate(standaloneModule); // Input System 중복 입력 방지를 위한 구 모듈 제거
            } // 구 UI 입력 모듈 제거 처리 종료

            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null) // Input System UI 모듈 누락 여부 확인
            { // Input System UI 모듈 추가 처리
                Undo.AddComponent<InputSystemUIInputModule>(eventSystem.gameObject); // 기존 EventSystem에 Input System UI 모듈 추가
            } // Input System UI 모듈 추가 처리 종료
        } // Input System EventSystem 보장 처리 종료

        private static RectTransform CreateUiObject(string objectName, Transform parent) // 기본 RectTransform UI 오브젝트 생성
        { // 기본 UI 오브젝트 생성 처리
            GameObject uiObject = new GameObject(objectName, typeof(RectTransform)); // RectTransform 기반 UI 오브젝트 생성
            Undo.RegisterCreatedObjectUndo(uiObject, "Create " + objectName); // UI 오브젝트 생성 Undo 등록
            uiObject.transform.SetParent(parent, false); // 전달된 Canvas 부모 아래 배치
            ApplyUiLayer(uiObject); // UI 오브젝트 UI Layer 적용
            return uiObject.GetComponent<RectTransform>(); // 생성된 RectTransform 반환
        } // 기본 UI 오브젝트 생성 처리 종료

        private static Image CreateImage(string objectName, Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta, bool raycastTarget) // 설정값이 적용된 Canvas Image 생성
        { // Canvas Image 생성 처리
            RectTransform rectTransform = CreateUiObject(objectName, parent); // 기본 RectTransform UI 오브젝트 생성
            SetRect(rectTransform, anchorMin, anchorMax, pivot, anchoredPosition, sizeDelta); // 전달된 RectTransform 설정 적용
            CanvasRenderer canvasRenderer = Undo.AddComponent<CanvasRenderer>(rectTransform.gameObject); // Image 렌더링용 Canvas Renderer 추가
            canvasRenderer.cullTransparentMesh = true; // 완전히 투명한 메시 렌더링 생략
            Image image = Undo.AddComponent<Image>(rectTransform.gameObject); // Canvas Image 컴포넌트 추가
            image.color = color; // 전달된 Image 색상 적용
            image.raycastTarget = raycastTarget; // 전달된 입력 차단 여부 적용
            return image; // 구성된 Canvas Image 반환
        } // Canvas Image 생성 처리 종료

        private static TMP_Text CreateText(string objectName, Transform parent, string content, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta, Color color) // 설정값이 적용된 TextMeshPro 생성
        { // TextMeshPro 생성 처리
            RectTransform rectTransform = CreateUiObject(objectName, parent); // 기본 RectTransform UI 오브젝트 생성
            SetRect(rectTransform, anchorMin, anchorMax, pivot, anchoredPosition, sizeDelta); // 전달된 RectTransform 설정 적용
            CanvasRenderer canvasRenderer = Undo.AddComponent<CanvasRenderer>(rectTransform.gameObject); // TextMeshPro 렌더링용 Canvas Renderer 추가
            canvasRenderer.cullTransparentMesh = true; // 완전히 투명한 메시 렌더링 생략
            TextMeshProUGUI text = Undo.AddComponent<TextMeshProUGUI>(rectTransform.gameObject); // TextMeshProUGUI 컴포넌트 추가
            text.text = content; // 전달된 기본 문구 적용
            text.fontSize = fontSize; // 전달된 글자 크기 적용
            text.fontStyle = fontStyle; // 전달된 글자 스타일 적용
            text.alignment = alignment; // 전달된 글자 정렬 적용
            text.color = color; // 전달된 글자 색상 적용
            text.enableWordWrapping = false; // HUD 한 줄 문구 자동 줄바꿈 비활성화
            text.overflowMode = TextOverflowModes.Ellipsis; // 영역 초과 문구 말줄임 처리
            text.raycastTarget = false; // Text의 불필요한 Raycast 비활성화

            if (TMP_Settings.defaultFontAsset != null) // TextMeshPro 기본 폰트 에셋 존재 여부 확인
            { // TextMeshPro 기본 폰트 적용 처리
                text.font = TMP_Settings.defaultFontAsset; // 프로젝트 기본 TextMeshPro 폰트 적용
            } // TextMeshPro 기본 폰트 적용 처리 종료

            return text; // 구성된 TextMeshPro 반환
        } // TextMeshPro 생성 처리 종료

        private static Image CreateFilledBar(string objectName, Transform parent, Color fillColor, Vector2 anchoredPosition, Vector2 sizeDelta, Vector2 anchor) // 배경과 채움으로 구성된 진행 막대 생성
        { // 진행 막대 생성 처리
            Image background = CreateImage(objectName, parent, new Color(0.015f, 0.02f, 0.03f, 0.96f), anchor, anchor, anchor, anchoredPosition, sizeDelta, false); // 어두운 진행 막대 배경 생성
            Image fill = CreateImage("Fill", background.transform, fillColor, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, false); // 진행 막대 전체 Stretch 채움 생성
            fill.type = Image.Type.Filled; // Image 채움 방식 적용
            fill.fillMethod = Image.FillMethod.Horizontal; // 가로 방향 채움 방식 적용
            fill.fillOrigin = (int)Image.OriginHorizontal.Left; // 왼쪽에서 오른쪽 채움 방향 적용
            fill.fillAmount = 1f; // 최초 100퍼센트 채움 적용
            return fill; // 진행 막대 채움 Image 반환
        } // 진행 막대 생성 처리 종료

        private static Button CreateButton(string objectName, Transform parent, string label, Color backgroundColor, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta) // TextMeshPro 라벨이 포함된 Canvas Button 생성
        { // Canvas Button 생성 처리
            Image buttonImage = CreateImage(objectName, parent, backgroundColor, anchorMin, anchorMax, pivot, anchoredPosition, sizeDelta, true); // Button 배경 Image 생성
            Button button = Undo.AddComponent<Button>(buttonImage.gameObject); // Canvas Button 컴포넌트 추가
            button.targetGraphic = buttonImage; // Button 상태 색상 대상 Image 연결
            button.transition = Selectable.Transition.ColorTint; // Button 상태 전환 Color Tint 적용
            ColorBlock colors = button.colors; // Button 상태별 색상 묶음 조회
            colors.normalColor = Color.white; // 기본 상태 원본 배경색 유지
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.92f); // 마우스 강조 상태 색상 적용
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f); // 누른 상태 색상 적용
            colors.selectedColor = colors.highlightedColor; // 선택 상태 강조 색상 적용
            colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.75f); // 비활성 상태 색상 적용
            colors.colorMultiplier = 1f; // 상태 색상 배율 기본값 적용
            colors.fadeDuration = 0.1f; // 상태 색상 전환 시간 0.1초 적용
            button.colors = colors; // 구성된 Button 상태 색상 저장
            CreateText("Label", buttonImage.transform, label, 19f, FontStyles.Bold, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(-20f, -10f), Color.white); // Button 전체 영역 TextMeshPro 라벨 생성
            return button; // 구성된 Canvas Button 반환
        } // Canvas Button 생성 처리 종료

        private static void SetRect(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta) // RectTransform 앵커와 위치와 크기 공통 적용
        { // RectTransform 공통 설정 처리
            rectTransform.anchorMin = anchorMin; // 최소 앵커 적용
            rectTransform.anchorMax = anchorMax; // 최대 앵커 적용
            rectTransform.pivot = pivot; // 피벗 적용
            rectTransform.anchoredPosition = anchoredPosition; // 앵커 기준 위치 적용
            rectTransform.sizeDelta = sizeDelta; // 앵커 기준 크기 또는 여백 적용
            rectTransform.localScale = Vector3.one; // UI 로컬 크기 1배 적용
            rectTransform.localRotation = Quaternion.identity; // UI 로컬 회전 초기화
        } // RectTransform 공통 설정 처리 종료

        private static void ApplyUiLayer(GameObject targetObject) // 생성된 UI 오브젝트에 UI Layer 적용
        { // UI Layer 적용 처리
            int uiLayer = LayerMask.NameToLayer("UI"); // 프로젝트 UI Layer 번호 조회

            if (uiLayer >= 0) // UI Layer 존재 여부 확인
            { // UI Layer 저장 처리
                targetObject.layer = uiLayer; // 생성된 UI 오브젝트 UI Layer 적용
            } // UI Layer 저장 처리 종료
        } // UI Layer 적용 처리 종료

        private readonly struct TopCenterViews // 상단 중앙 UI 참조 묶음 선언
        { // 상단 중앙 UI 참조 묶음
            public TopCenterViews(TMP_Text timerText, TMP_Text rankText) // 상단 중앙 UI 참조 묶음 생성
            { // 상단 중앙 UI 참조 저장 처리
                TimerText = timerText; // 남은 시간 텍스트 저장
                RankText = rankText; // 현재 순위 텍스트 저장
            } // 상단 중앙 UI 참조 저장 처리 종료

            public TMP_Text TimerText { get; } // 남은 시간 텍스트 반환
            public TMP_Text RankText { get; } // 현재 순위 텍스트 반환
        } // 상단 중앙 UI 참조 묶음 종료

        private readonly struct ProgressViews // 진행 정보 UI 참조 묶음 선언
        { // 진행 정보 UI 참조 묶음
            public ProgressViews(TMP_Text heightText, TMP_Text sectionText, Image courseFill, TMP_Text checkpointText, TMP_Text courseTopText) // 진행 정보 UI 참조 묶음 생성
            { // 진행 정보 UI 참조 저장 처리
                HeightText = heightText; // 높이 텍스트 저장
                SectionText = sectionText; // 구간 텍스트 저장
                CourseFill = courseFill; // 코스 진행 막대 저장
                CheckpointText = checkpointText; // 체크포인트 텍스트 저장
                CourseTopText = courseTopText; // 정상 도달 텍스트 저장
            } // 진행 정보 UI 참조 저장 처리 종료

            public TMP_Text HeightText { get; } // 높이 텍스트 반환
            public TMP_Text SectionText { get; } // 구간 텍스트 반환
            public Image CourseFill { get; } // 코스 진행 막대 반환
            public TMP_Text CheckpointText { get; } // 체크포인트 텍스트 반환
            public TMP_Text CourseTopText { get; } // 정상 도달 텍스트 반환
        } // 진행 정보 UI 참조 묶음 종료

        private readonly struct RankingViews // 실시간 순위 UI 참조 묶음 선언
        { // 실시간 순위 UI 참조 묶음
            public RankingViews(RectTransform entryRoot, TMP_Text entryTemplate) // 실시간 순위 UI 참조 묶음 생성
            { // 실시간 순위 UI 참조 저장 처리
                EntryRoot = entryRoot; // 실시간 순위 항목 부모 저장
                EntryTemplate = entryTemplate; // 실시간 순위 복제 원본 저장
            } // 실시간 순위 UI 참조 저장 처리 종료

            public RectTransform EntryRoot { get; } // 실시간 순위 항목 부모 반환
            public TMP_Text EntryTemplate { get; } // 실시간 순위 복제 원본 반환
        } // 실시간 순위 UI 참조 묶음 종료

        private readonly struct StaminaViews // 스태미나 UI 참조 묶음 선언
        { // 스태미나 UI 참조 묶음
            public StaminaViews(TMP_Text staminaText, Image staminaFill) // 스태미나 UI 참조 묶음 생성
            { // 스태미나 UI 참조 저장 처리
                StaminaText = staminaText; // 스태미나 텍스트 저장
                StaminaFill = staminaFill; // 스태미나 막대 저장
            } // 스태미나 UI 참조 저장 처리 종료

            public TMP_Text StaminaText { get; } // 스태미나 텍스트 반환
            public Image StaminaFill { get; } // 스태미나 막대 반환
        } // 스태미나 UI 참조 묶음 종료

        private readonly struct ResultViews // 경기 결과 UI 참조 묶음 선언
        { // 경기 결과 UI 참조 묶음
            public ResultViews(GameObject rootPanel, TMP_Text titleText, TMP_Text reasonText, TMP_Text finalRankText, RectTransform entryRoot, TMP_Text entryTemplate) // 경기 결과 UI 참조 묶음 생성
            { // 경기 결과 UI 참조 저장 처리
                RootPanel = rootPanel; // 경기 결과 전체 화면 저장
                TitleText = titleText; // 경기 결과 제목 저장
                ReasonText = reasonText; // 경기 종료 원인 저장
                FinalRankText = finalRankText; // 최종 순위 텍스트 저장
                EntryRoot = entryRoot; // 최종 순위 항목 부모 저장
                EntryTemplate = entryTemplate; // 최종 순위 복제 원본 저장
            } // 경기 결과 UI 참조 저장 처리 종료

            public GameObject RootPanel { get; } // 경기 결과 전체 화면 반환
            public TMP_Text TitleText { get; } // 경기 결과 제목 반환
            public TMP_Text ReasonText { get; } // 경기 종료 원인 반환
            public TMP_Text FinalRankText { get; } // 최종 순위 텍스트 반환
            public RectTransform EntryRoot { get; } // 최종 순위 항목 부모 반환
            public TMP_Text EntryTemplate { get; } // 최종 순위 복제 원본 반환
        } // 경기 결과 UI 참조 묶음 종료

        private readonly struct RespawnMenuViews // ESC 경기 메뉴 UI 참조 묶음 선언
        { // ESC 경기 메뉴 UI 참조 묶음
            public RespawnMenuViews(GameObject fadePanel, GameObject menuPanel, TMP_Text checkpointText, Button respawnButton, Button closeButton) // ESC 경기 메뉴 UI 참조 묶음 생성
            { // ESC 경기 메뉴 UI 참조 저장 처리
                FadePanel = fadePanel; // 전체 화면 암전 저장
                MenuPanel = menuPanel; // ESC 경기 메뉴 패널 저장
                CheckpointText = checkpointText; // 체크포인트 텍스트 저장
                RespawnButton = respawnButton; // 직접 부활 버튼 저장
                CloseButton = closeButton; // 경기 복귀 버튼 저장
            } // ESC 경기 메뉴 UI 참조 저장 처리 종료

            public GameObject FadePanel { get; } // 전체 화면 암전 반환
            public GameObject MenuPanel { get; } // ESC 경기 메뉴 패널 반환
            public TMP_Text CheckpointText { get; } // 체크포인트 텍스트 반환
            public Button RespawnButton { get; } // 직접 부활 버튼 반환
            public Button CloseButton { get; } // 경기 복귀 버튼 반환
        } // ESC 경기 메뉴 UI 참조 묶음 종료
    } // 40일차 Canvas UI 자동 설정 도구 묶음 종료
} // 프로젝트 Editor 도구 묶음 종료
