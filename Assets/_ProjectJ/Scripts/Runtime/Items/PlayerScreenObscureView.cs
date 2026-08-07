using TMPro; // 비눗방울 탈출 안내 텍스트 기능 참조
using UnityEngine; // Unity 오브젝트와 시간 기능 참조
using UnityEngine.UI; // 화면 가림 Canvas 기능 참조

namespace ProjectJ.Items // 프로젝트 아이템 기능 네임스페이스 선언
{ // 프로젝트 아이템 기능 묶음
    [DisallowMultipleComponent] // 플레이어당 화면 방해 표시 한 개만 허용
    public sealed class PlayerScreenObscureView : MonoBehaviour // 먹물과 연막과 비눗방울 화면 표시 선언
    { // 플레이어 화면 방해 표시 묶음
        private Canvas effectCanvas; // 화면 방해 전용 Canvas 저장
        private Image inkImage; // 화면 중앙 먹물 이미지 저장
        private Image smokeImage; // 전체 화면 연막 이미지 저장
        private Image bubbleImage; // 전체 화면 비눗방울 이미지 저장
        private TMP_Text bubbleGuideText; // 비눗방울 탈출 안내 저장
        private float inkRemaining; // 먹물 표시 남은 시간 저장
        private float smokeRemaining; // 연막 표시 남은 시간 저장
        private bool bubbleActive; // 비눗방울 표시 활성 여부 저장

        private void Update() // 시간 기반 화면 방해 표시 갱신
        { // 화면 방해 표시 프레임 처리
            float deltaTime = Mathf.Max(0f, Time.deltaTime); // 음수가 없는 프레임 시간 계산
            inkRemaining = Mathf.Max(0f, inkRemaining - deltaTime); // 먹물 남은 시간 감소
            smokeRemaining = Mathf.Max(0f, smokeRemaining - deltaTime); // 연막 남은 시간 감소

            if (inkImage != null) // 먹물 이미지 존재 여부 확인
            { // 먹물 표시 상태 적용
                inkImage.enabled = inkRemaining > 0f; // 남은 시간 기반 먹물 표시 전환
            } // 먹물 표시 상태 적용 종료

            if (smokeImage != null) // 연막 이미지 존재 여부 확인
            { // 연막 표시 상태 적용
                smokeImage.enabled = smokeRemaining > 0f; // 남은 시간 기반 연막 표시 전환
            } // 연막 표시 상태 적용 종료
        } // 화면 방해 표시 프레임 처리 종료

        public void ShowInk(float duration, float centerCoverage) // 화면 중앙 먹물 가림 표시
        { // 먹물 표시 처리
            EnsureCanvas(); // 화면 방해 Canvas 준비
            float coverage = Mathf.Clamp(centerCoverage, 0.1f, 1f); // 화면 중앙 가림 비율 보정
            float margin = (1f - coverage) * 0.5f; // 양쪽 빈 여백 비율 계산
            RectTransform inkRect = inkImage.rectTransform; // 먹물 이미지 RectTransform 조회
            inkRect.anchorMin = new Vector2(margin, margin); // 화면 중앙 가림 최소 앵커 적용
            inkRect.anchorMax = new Vector2(1f - margin, 1f - margin); // 화면 중앙 가림 최대 앵커 적용
            inkRect.offsetMin = Vector2.zero; // 가림 최소 여백 초기화
            inkRect.offsetMax = Vector2.zero; // 가림 최대 여백 초기화
            inkRemaining = Mathf.Max(inkRemaining, duration); // 더 긴 먹물 표시 시간 유지
            inkImage.enabled = inkRemaining > 0f; // 먹물 이미지 즉시 표시
        } // 먹물 표시 처리 종료

        public void RefreshSmoke(float refreshDuration) // 연막 안에 있는 동안 전체 화면 방해 갱신
        { // 연막 표시 갱신 처리
            EnsureCanvas(); // 화면 방해 Canvas 준비
            smokeRemaining = Mathf.Max(smokeRemaining, refreshDuration); // Trigger 갱신 사이 연막 표시 유지
            smokeImage.enabled = smokeRemaining > 0f; // 연막 이미지 즉시 표시
        } // 연막 표시 갱신 처리 종료

        public void ShowBubbleProgress(int currentCount, int requiredCount) // 비눗방울 탈출 진행 표시
        { // 비눗방울 표시 처리
            EnsureCanvas(); // 화면 방해 Canvas 준비
            bubbleActive = true; // 비눗방울 표시 활성화
            bubbleImage.enabled = true; // 비눗방울 화면 테두리 표시
            bubbleGuideText.enabled = true; // 비눗방울 탈출 안내 표시
            bubbleGuideText.text = $"A / D 교대 입력 {currentCount} / {Mathf.Max(1, requiredCount)}"; // 현재 탈출 입력 진행도 표시
        } // 비눗방울 표시 처리 종료

        public void HideBubble() // 비눗방울 탈출 또는 상태 종료 표시 정리
        { // 비눗방울 표시 정리 처리
            bubbleActive = false; // 비눗방울 표시 상태 해제

            if (bubbleImage != null) // 비눗방울 이미지 존재 여부 확인
            { // 비눗방울 이미지 숨김 처리
                bubbleImage.enabled = false; // 비눗방울 화면 테두리 숨김
            } // 비눗방울 이미지 숨김 처리 종료

            if (bubbleGuideText != null) // 탈출 안내 텍스트 존재 여부 확인
            { // 탈출 안내 숨김 처리
                bubbleGuideText.enabled = false; // 비눗방울 안내 숨김
            } // 탈출 안내 숨김 처리 종료
        } // 비눗방울 표시 정리 처리 종료

        public void ClearAll() // 부활과 경기 종료 시 화면 방해 전체 제거
        { // 화면 방해 전체 제거 처리
            inkRemaining = 0f; // 먹물 남은 시간 제거
            smokeRemaining = 0f; // 연막 남은 시간 제거
            HideBubble(); // 비눗방울 표시 제거

            if (inkImage != null) // 먹물 이미지 존재 여부 확인
            { // 먹물 이미지 숨김 처리
                inkImage.enabled = false; // 먹물 이미지 숨김
            } // 먹물 이미지 숨김 처리 종료

            if (smokeImage != null) // 연막 이미지 존재 여부 확인
            { // 연막 이미지 숨김 처리
                smokeImage.enabled = false; // 연막 이미지 숨김
            } // 연막 이미지 숨김 처리 종료
        } // 화면 방해 전체 제거 처리 종료

        private void EnsureCanvas() // 런타임 화면 방해 Canvas와 자식 표시 준비
        { // 화면 방해 Canvas 준비 처리
            if (effectCanvas != null) // 이미 생성된 Canvas 여부 확인
            { // 중복 생성 방지 처리
                return; // 기존 Canvas 재사용
            } // 중복 생성 방지 처리 종료

            GameObject canvasObject = new GameObject("P1ScreenObscureCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)); // 화면 방해 Canvas 오브젝트 생성
            canvasObject.transform.SetParent(transform, false); // 대상 플레이어 아래 Canvas 배치
            effectCanvas = canvasObject.GetComponent<Canvas>(); // 생성된 Canvas 조회
            effectCanvas.renderMode = RenderMode.ScreenSpaceOverlay; // 화면 전체 Overlay 방식 적용
            effectCanvas.sortingOrder = 500; // 기존 HUD보다 위쪽 표시 순서 적용
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>(); // Canvas 크기 보정 기능 조회
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; // 해상도 기반 크기 보정 적용
            scaler.referenceResolution = new Vector2(1920f, 1080f); // 기준 해상도 적용
            GraphicRaycaster raycaster = canvasObject.GetComponent<GraphicRaycaster>(); // 입력 차단 가능 컴포넌트 조회
            raycaster.enabled = false; // 게임 입력을 막지 않도록 UI Raycast 비활성화
            inkImage = CreateImage(canvasObject.transform, "InkObscure", new Color(0.04f, 0.01f, 0.06f, 0.94f)); // 먹물 가림 이미지 생성
            smokeImage = CreateImage(canvasObject.transform, "SmokeObscure", new Color(0.18f, 0.18f, 0.2f, 0.72f)); // 연막 가림 이미지 생성
            bubbleImage = CreateImage(canvasObject.transform, "BubbleObscure", new Color(0.55f, 0.9f, 1f, 0.28f)); // 비눗방울 가림 이미지 생성
            SetFullScreen(smokeImage.rectTransform); // 연막 이미지 전체 화면 배치
            SetFullScreen(bubbleImage.rectTransform); // 비눗방울 이미지 전체 화면 배치
            bubbleGuideText = CreateGuideText(canvasObject.transform); // 비눗방울 탈출 안내 텍스트 생성
            inkImage.enabled = false; // 최초 먹물 이미지 숨김
            smokeImage.enabled = false; // 최초 연막 이미지 숨김
            bubbleImage.enabled = bubbleActive; // 현재 비눗방울 상태 기반 표시
            bubbleGuideText.enabled = bubbleActive; // 현재 비눗방울 상태 기반 안내 표시
        } // 화면 방해 Canvas 준비 처리 종료

        private static Image CreateImage(Transform parent, string objectName, Color color) // 단색 화면 방해 이미지 생성
        { // 화면 방해 이미지 생성 처리
            GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); // UI Image 오브젝트 생성
            imageObject.transform.SetParent(parent, false); // 지정 Canvas 아래 이미지 배치
            Image image = imageObject.GetComponent<Image>(); // 생성된 Image 조회
            image.color = color; // 화면 방해 종류별 색상 적용
            image.raycastTarget = false; // 마우스 입력 통과 설정
            return image; // 구성된 화면 방해 이미지 반환
        } // 화면 방해 이미지 생성 처리 종료

        private static TMP_Text CreateGuideText(Transform parent) // 비눗방울 탈출 안내 텍스트 생성
        { // 탈출 안내 텍스트 생성 처리
            GameObject textObject = new GameObject("BubbleEscapeGuide", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI)); // TextMeshPro 안내 오브젝트 생성
            textObject.transform.SetParent(parent, false); // 화면 방해 Canvas 아래 안내 배치
            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>(); // 생성된 TextMeshPro 조회
            RectTransform rectTransform = text.rectTransform; // 안내 RectTransform 조회
            rectTransform.anchorMin = new Vector2(0.25f, 0.42f); // 화면 중앙 안내 최소 앵커 적용
            rectTransform.anchorMax = new Vector2(0.75f, 0.58f); // 화면 중앙 안내 최대 앵커 적용
            rectTransform.offsetMin = Vector2.zero; // 안내 최소 여백 초기화
            rectTransform.offsetMax = Vector2.zero; // 안내 최대 여백 초기화
            text.alignment = TextAlignmentOptions.Center; // 안내 문구 중앙 정렬
            text.fontSize = 38f; // 안내 문구 크기 적용
            text.color = Color.white; // 안내 문구 흰색 적용
            text.raycastTarget = false; // 안내 문구 마우스 입력 통과 설정
            return text; // 구성된 탈출 안내 반환
        } // 탈출 안내 텍스트 생성 처리 종료

        private static void SetFullScreen(RectTransform rectTransform) // UI 표시 전체 화면 배치
        { // 전체 화면 배치 처리
            rectTransform.anchorMin = Vector2.zero; // 화면 왼쪽 아래 앵커 적용
            rectTransform.anchorMax = Vector2.one; // 화면 오른쪽 위 앵커 적용
            rectTransform.offsetMin = Vector2.zero; // 왼쪽 아래 여백 초기화
            rectTransform.offsetMax = Vector2.zero; // 오른쪽 위 여백 초기화
        } // 전체 화면 배치 처리 종료
    } // 플레이어 화면 방해 표시 묶음 종료
} // 프로젝트 아이템 기능 묶음 종료
