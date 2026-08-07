using TMPro; // TextMeshPro 텍스트 기능 참조
using UnityEngine; // Unity 오브젝트와 화면 기능 참조
using UnityEngine.EventSystems; // Canvas 버튼 이벤트 시스템 기능 참조
using UnityEngine.InputSystem.UI; // Input System Canvas 입력 모듈 기능 참조
using UnityEngine.UI; // Canvas와 Image와 Button 기능 참조

namespace ProjectJ.UI // 프로젝트 Canvas UI 네임스페이스 선언
{ // 프로젝트 Canvas UI 기능 묶음
    [DisallowMultipleComponent] // 치명 오류 화면 컴포넌트 중복 방지
    public sealed class FatalErrorScreen : MonoBehaviour // 기존 이름을 유지한 Canvas 치명 오류 화면 선언
    { // Canvas 치명 오류 화면 기능 묶음
        [SerializeField] private GameObject rootPanel; // 치명 오류 전체 화면 패널 참조
        [SerializeField] private TMP_Text titleText; // 치명 오류 제목 텍스트 참조
        [SerializeField] private TMP_Text messageText; // 치명 오류 내용 텍스트 참조
        [SerializeField] private Button quitButton; // 게임 종료 버튼 참조

        private bool isVisible; // 오류 안내 화면 표시 여부
        private bool buttonBound; // 게임 종료 버튼 이벤트 연결 여부
        private string title = "게임을 시작할 수 없습니다"; // 치명 오류 기본 제목
        private string message = "필수 데이터 초기화에 실패했습니다."; // 치명 오류 기본 내용

        public bool IsVisible => isVisible; // 오류 안내 화면 표시 여부 반환

        private void Awake() // Canvas 치명 오류 화면 참조 준비
        { // Canvas 치명 오류 화면 준비 처리
            EnsureRuntimeCanvasView(); // Scene 구성 누락 시 Canvas 오류 화면 안전 생성
            BindQuitButton(); // 게임 종료 버튼 이벤트 연결
            Hide(); // 최초 오류 화면 숨김
        } // Canvas 치명 오류 화면 준비 처리 종료

        private void OnDestroy() // Canvas 치명 오류 화면 제거 시 이벤트 정리
        { // Canvas 치명 오류 화면 제거 처리
            UnbindQuitButton(); // 게임 종료 버튼 이벤트 해제
        } // Canvas 치명 오류 화면 제거 처리 종료

        public void Show(string newTitle, string newMessage) // 치명 오류 제목과 내용을 Canvas에 표시
        { // 치명 오류 Canvas 표시 처리
            EnsureRuntimeCanvasView(); // 호출 시점 Canvas 참조 안전 보장
            title = string.IsNullOrWhiteSpace(newTitle) ? title : newTitle.Trim(); // 비어 있지 않은 오류 제목 저장
            message = string.IsNullOrWhiteSpace(newMessage) ? message : newMessage.Trim(); // 비어 있지 않은 오류 내용 저장

            if (titleText != null) // 오류 제목 텍스트 존재 여부 확인
            { // 오류 제목 적용 처리
                titleText.text = title; // 저장된 치명 오류 제목 표시
            } // 오류 제목 적용 처리 종료

            if (messageText != null) // 오류 내용 텍스트 존재 여부 확인
            { // 오류 내용 적용 처리
                messageText.text = message; // 저장된 치명 오류 내용 표시
            } // 오류 내용 적용 처리 종료

            if (rootPanel != null) // 오류 전체 화면 패널 존재 여부 확인
            { // 오류 전체 화면 표시 처리
                rootPanel.SetActive(true); // 치명 오류 전체 화면 표시
            } // 오류 전체 화면 표시 처리 종료

            isVisible = true; // 오류 안내 화면 표시 상태 저장
            Cursor.lockState = CursorLockMode.None; // 오류 화면 조작용 커서 잠금 해제
            Cursor.visible = true; // 오류 화면 조작용 커서 표시
        } // 치명 오류 Canvas 표시 처리 종료

        public void Hide() // 치명 오류 Canvas 숨김
        { // 치명 오류 Canvas 숨김 처리
            isVisible = false; // 오류 안내 화면 숨김 상태 저장

            if (rootPanel != null) // 오류 전체 화면 패널 존재 여부 확인
            { // 오류 전체 화면 숨김 처리
                rootPanel.SetActive(false); // 치명 오류 전체 화면 숨김
            } // 오류 전체 화면 숨김 처리 종료
        } // 치명 오류 Canvas 숨김 처리 종료

        private void OnQuitButtonClicked() // 게임 종료 버튼 입력 처리
        { // 게임 종료 버튼 입력 처리
            Application.Quit(); // 실행 중인 게임 종료 요청
        } // 게임 종료 버튼 입력 처리 종료

        private void BindQuitButton() // 게임 종료 버튼 이벤트 안전 연결
        { // 게임 종료 버튼 이벤트 연결 처리
            if (buttonBound || quitButton == null) // 중복 연결과 버튼 누락 확인
            { // 게임 종료 버튼 이벤트 연결 생략 처리
                return; // 잘못된 버튼 이벤트 연결 방지
            } // 게임 종료 버튼 이벤트 연결 생략 처리 종료

            quitButton.onClick.AddListener(OnQuitButtonClicked); // 게임 종료 버튼에 종료 처리 연결
            buttonBound = true; // 게임 종료 버튼 이벤트 연결 상태 저장
        } // 게임 종료 버튼 이벤트 연결 처리 종료

        private void UnbindQuitButton() // 게임 종료 버튼 이벤트 안전 해제
        { // 게임 종료 버튼 이벤트 해제 처리
            if (!buttonBound || quitButton == null) // 연결 상태와 버튼 존재 여부 확인
            { // 게임 종료 버튼 이벤트 해제 생략 처리
                return; // 연결되지 않은 버튼 이벤트 해제 방지
            } // 게임 종료 버튼 이벤트 해제 생략 처리 종료

            quitButton.onClick.RemoveListener(OnQuitButtonClicked); // 게임 종료 버튼 종료 처리 해제
            buttonBound = false; // 게임 종료 버튼 이벤트 연결 상태 초기화
        } // 게임 종료 버튼 이벤트 해제 처리 종료

        private void EnsureRuntimeCanvasView() // Scene 설정 누락 시 최소 Canvas 오류 화면 생성
        { // 최소 Canvas 오류 화면 생성 처리
            if (rootPanel != null && titleText != null && messageText != null && quitButton != null) // 모든 Canvas 오류 화면 참조 연결 여부 확인
            { // 기존 Canvas 오류 화면 사용 처리
                BindQuitButton(); // 기존 게임 종료 버튼 이벤트 연결 보장
                return; // 런타임 Canvas 중복 생성 생략
            } // 기존 Canvas 오류 화면 사용 처리 종료

            GameObject canvasObject = new GameObject("FatalErrorCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)); // 치명 오류 전용 Canvas 오브젝트 생성
            canvasObject.transform.SetParent(transform, false); // Bootstrap 오브젝트 아래 Canvas 배치
            Canvas canvas = canvasObject.GetComponent<Canvas>(); // 생성된 Canvas 컴포넌트 조회
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; // 카메라 없는 전체 화면 Overlay 방식 적용
            canvas.sortingOrder = 1000; // 다른 모든 UI보다 높은 표시 순서 적용
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>(); // 생성된 Canvas Scaler 조회
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; // 해상도 기반 UI 크기 조절 방식 적용
            scaler.referenceResolution = new Vector2(1920f, 1080f); // Full HD 기준 해상도 적용
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight; // 가로와 세로 혼합 대응 방식 적용
            scaler.matchWidthOrHeight = 0.5f; // 가로와 세로 동일 비중 적용
            scaler.referencePixelsPerUnit = 100f; // 스프라이트 100픽셀 기준 단위 적용
            rootPanel = CreateRuntimeImage("FatalErrorRoot", canvasObject.transform, new Color(0.01f, 0.02f, 0.04f, 0.96f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero).gameObject; // 전체 화면 입력 차단 배경 생성
            Image panelImage = CreateRuntimeImage("FatalErrorPanel", rootPanel.transform, new Color(0.055f, 0.075f, 0.11f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(680f, 360f)); // 중앙 오류 패널 생성
            titleText = CreateRuntimeText("TitleText", panelImage.transform, title, 32f, FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0f, -46f), new Vector2(620f, 54f)); // 오류 제목 TextMeshPro 생성
            messageText = CreateRuntimeText("MessageText", panelImage.transform, message, 18f, FontStyles.Normal, TextAlignmentOptions.TopLeft, new Vector2(0f, -132f), new Vector2(620f, 100f)); // 오류 내용 TextMeshPro 생성
            quitButton = CreateRuntimeButton("QuitButton", panelImage.transform, "게임 종료", new Vector2(0f, -128f), new Vector2(260f, 52f)); // 게임 종료 Canvas 버튼 생성
            EnsureEventSystem(); // Canvas 버튼용 Input System EventSystem 보장
            BindQuitButton(); // 새 게임 종료 버튼 이벤트 연결
        } // 최소 Canvas 오류 화면 생성 처리 종료

        private static Image CreateRuntimeImage(string objectName, Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta) // 런타임 Canvas Image 생성
        { // 런타임 Canvas Image 생성 처리
            GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); // RectTransform과 Image 오브젝트 생성
            imageObject.transform.SetParent(parent, false); // 전달된 Canvas 부모 아래 배치
            RectTransform rectTransform = imageObject.GetComponent<RectTransform>(); // 생성된 RectTransform 조회
            rectTransform.anchorMin = anchorMin; // 최소 앵커 적용
            rectTransform.anchorMax = anchorMax; // 최대 앵커 적용
            rectTransform.pivot = new Vector2(0.5f, 0.5f); // 중앙 피벗 적용
            rectTransform.anchoredPosition = anchoredPosition; // 앵커 기준 위치 적용
            rectTransform.sizeDelta = sizeDelta; // 앵커 기준 크기 적용
            Image image = imageObject.GetComponent<Image>(); // 생성된 Image 컴포넌트 조회
            image.color = color; // 전달된 Image 색상 적용
            image.raycastTarget = true; // 오류 화면 입력 차단 활성화
            return image; // 구성된 Canvas Image 반환
        } // 런타임 Canvas Image 생성 처리 종료

        private static TMP_Text CreateRuntimeText(string objectName, Transform parent, string content, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment, Vector2 anchoredPosition, Vector2 sizeDelta) // 런타임 TextMeshPro 생성
        { // 런타임 TextMeshPro 생성 처리
            GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI)); // RectTransform과 TextMeshPro 오브젝트 생성
            textObject.transform.SetParent(parent, false); // 전달된 Canvas 부모 아래 배치
            RectTransform rectTransform = textObject.GetComponent<RectTransform>(); // 생성된 RectTransform 조회
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f); // 중앙 최소 앵커 적용
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f); // 중앙 최대 앵커 적용
            rectTransform.pivot = new Vector2(0.5f, 0.5f); // 중앙 피벗 적용
            rectTransform.anchoredPosition = anchoredPosition; // 앵커 기준 위치 적용
            rectTransform.sizeDelta = sizeDelta; // TextMeshPro 표시 크기 적용
            TMP_Text text = textObject.GetComponent<TMP_Text>(); // 생성된 TextMeshPro 컴포넌트 조회
            text.text = content; // 전달된 표시 문구 적용
            text.fontSize = fontSize; // 전달된 글자 크기 적용
            text.fontStyle = fontStyle; // 전달된 글자 스타일 적용
            text.alignment = alignment; // 전달된 글자 정렬 적용
            text.color = Color.white; // 흰색 글자 적용
            text.textWrappingMode = TextWrappingModes.Normal; // 긴 오류 문구 자동 줄바꿈 활성화
            text.raycastTarget = false; // 텍스트의 불필요한 입력 차단 해제
            return text; // 구성된 TextMeshPro 반환
        } // 런타임 TextMeshPro 생성 처리 종료

        private static Button CreateRuntimeButton(string objectName, Transform parent, string label, Vector2 anchoredPosition, Vector2 sizeDelta) // 런타임 Canvas Button 생성
        { // 런타임 Canvas Button 생성 처리
            Image buttonImage = CreateRuntimeImage(objectName, parent, new Color(0.95f, 0.48f, 0.12f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, sizeDelta); // 주황색 버튼 배경 생성
            Button button = buttonImage.gameObject.AddComponent<Button>(); // 생성된 Image에 Button 기능 추가
            ColorBlock colors = button.colors; // Button 기본 상태 색상 묶음 조회
            colors.normalColor = Color.white; // 기본 상태 원본 Image 색상 유지
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.92f); // 마우스 강조 상태 색상 적용
            colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f); // 누른 상태 색상 적용
            colors.selectedColor = colors.highlightedColor; // 선택 상태 강조 색상 적용
            colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.75f); // 비활성 상태 색상 적용
            button.colors = colors; // 구성된 Button 상태 색상 저장
            CreateRuntimeText("Label", buttonImage.transform, label, 20f, FontStyles.Bold, TextAlignmentOptions.Center, Vector2.zero, sizeDelta - new Vector2(20f, 10f)); // 버튼 중앙 TextMeshPro 라벨 생성
            return button; // 구성된 Canvas Button 반환
        } // 런타임 Canvas Button 생성 처리 종료

        private static void EnsureEventSystem() // Input System 기반 Canvas EventSystem 보장
        { // Canvas EventSystem 보장 처리
            EventSystem eventSystem = FindFirstObjectByType<EventSystem>(); // 현재 Scene의 EventSystem 검색

            if (eventSystem != null) // 기존 EventSystem 존재 여부 확인
            { // 기존 EventSystem 사용 처리
                if (eventSystem.GetComponent<InputSystemUIInputModule>() == null) // Input System UI 모듈 누락 여부 확인
                { // Input System UI 모듈 추가 처리
                    eventSystem.gameObject.AddComponent<InputSystemUIInputModule>(); // 기존 EventSystem에 Input System UI 모듈 추가
                } // Input System UI 모듈 추가 처리 종료

                return; // 새 EventSystem 생성 생략
            } // 기존 EventSystem 사용 처리 종료

            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule)); // Input System 기반 EventSystem 생성
            DontDestroyOnLoad(eventSystemObject); // Bootstrap 이후 Scene에서도 EventSystem 유지
        } // Canvas EventSystem 보장 처리 종료

#if UNITY_EDITOR // Editor 전용 설정 시작
        public void ConfigureForEditor(GameObject newRootPanel, TMP_Text newTitleText, TMP_Text newMessageText, Button newQuitButton) // 자동 설정 도구용 치명 오류 화면 참조 연결
        { // 자동 설정 도구용 치명 오류 화면 참조 연결 처리
            rootPanel = newRootPanel; // 치명 오류 전체 화면 패널 참조 저장
            titleText = newTitleText; // 치명 오류 제목 참조 저장
            messageText = newMessageText; // 치명 오류 내용 참조 저장
            quitButton = newQuitButton; // 게임 종료 버튼 참조 저장
        } // 자동 설정 도구용 치명 오류 화면 참조 연결 처리 종료
#endif // Editor 전용 설정 종료
    } // Canvas 치명 오류 화면 기능 묶음 종료
} // 프로젝트 Canvas UI 기능 묶음 종료
