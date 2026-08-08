using System; // 문자열 비교와 입력 재지정 예외 기능 참조
using System.Collections.Generic; // 해상도 선택 목록 기능 참조
using ProjectJ.Core.Services; // 설정 관리자와 사용자 설정 데이터 참조
using ProjectJ.Diagnostics; // 입력 재지정 오류 로그 기능 참조
using ProjectJ.Input; // 입력 이름과 중복 키 검사 기능 참조
using TMPro; // TextMeshPro UI 기능 참조
using UnityEngine; // Unity 화면과 기본 기능 참조
using UnityEngine.EventSystems; // 키 재지정 중 UI 선택 해제 기능 참조
using UnityEngine.Events; // 런타임 재지정 버튼 이벤트 참조
using UnityEngine.InputSystem; // Input System과 InputActionAsset 기능 참조
using UnityEngine.InputSystem.Controls; // Interactive Rebinding 버튼 입력 형식 참조
using UnityEngine.UI; // Button과 Slider와 Toggle 기능 참조

namespace ProjectJ.UI // 프로젝트 UI 네임스페이스 선언
{ // 설정 메뉴 UI 기능 구성
    [DisallowMultipleComponent] // 동일 오브젝트의 설정 메뉴 중복 방지
    public sealed class SettingsMenuController : MonoBehaviour // 설정 메뉴 4개 탭과 작업 복사본과 키 재지정 관리
    { // 화면·사운드·조작·카메라 설정 통합 기능 구성
        [Header("Root")] // 루트 UI 참조 구역
        [SerializeField] private GameObject mainMenuRoot; // 임시 메인 메뉴 루트
        [SerializeField] private Button openSettingsButton; // 설정 열기 버튼
        [SerializeField] private GameObject settingsPanel; // 설정 화면 루트
        [SerializeField] private TMP_Text statusText; // 하단 상태 안내 문구

        [Header("Tabs")] // 탭 UI 참조 구역
        [SerializeField] private Button[] tabButtons = new Button[4]; // 화면·사운드·조작·카메라 탭 버튼
        [SerializeField] private GameObject[] tabPanels = new GameObject[4]; // 화면·사운드·조작·카메라 탭 내용

        [Header("Screen")] // 화면 탭 참조 구역
        [SerializeField] private TMP_Text resolutionText; // 해상도 표시 문구
        [SerializeField] private Button resolutionPreviousButton; // 이전 해상도 버튼
        [SerializeField] private Button resolutionNextButton; // 다음 해상도 버튼
        [SerializeField] private TMP_Text screenModeText; // 화면 모드 표시 문구
        [SerializeField] private Button screenModePreviousButton; // 이전 화면 모드 버튼
        [SerializeField] private Button screenModeNextButton; // 다음 화면 모드 버튼
        [SerializeField] private TMP_Text frameRateText; // 최대 FPS 표시 문구
        [SerializeField] private Button frameRatePreviousButton; // 이전 최대 FPS 버튼
        [SerializeField] private Button frameRateNextButton; // 다음 최대 FPS 버튼
        [SerializeField] private Toggle vSyncToggle; // VSync 토글
        [SerializeField] private Slider brightnessSlider; // 화면 밝기 슬라이더
        [SerializeField] private TMP_Text brightnessText; // 화면 밝기 표시 문구

        [Header("Sound")] // 사운드 탭 참조 구역
        [SerializeField] private Slider masterVolumeSlider; // 마스터 음량 슬라이더
        [SerializeField] private TMP_Text masterVolumeText; // 마스터 음량 표시 문구
        [SerializeField] private Slider musicVolumeSlider; // BGM 음량 슬라이더
        [SerializeField] private TMP_Text musicVolumeText; // BGM 음량 표시 문구
        [SerializeField] private Slider sfxVolumeSlider; // SFX 음량 슬라이더
        [SerializeField] private TMP_Text sfxVolumeText; // SFX 음량 표시 문구
        [SerializeField] private Slider uiVolumeSlider; // UI 효과음 음량 슬라이더
        [SerializeField] private TMP_Text uiVolumeText; // UI 효과음 음량 표시 문구
        [SerializeField] private Toggle muteToggle; // 전체 음소거 토글

        [Header("Controls")] // 조작 탭 참조 구역
        [SerializeField] private TMP_Text controlsInfoText; // 키 재지정 상태 안내 문구
        [SerializeField] private InputActionAsset inputActions; // 키 재지정 미리보기 원본 InputActionAsset
        [SerializeField] private Button resetBindingsButton; // 전체 키 기본값 미리보기 버튼
        [SerializeField] private Button[] rebindButtons = Array.Empty<Button>(); // 조작별 키 변경 버튼 배열
        [SerializeField] private TMP_Text[] rebindValueTexts = Array.Empty<TMP_Text>(); // 조작별 현재 키 표시 문구 배열

        [Header("Camera")] // 카메라 탭 참조 구역
        [SerializeField] private Slider mouseSensitivitySlider; // 마우스 감도 슬라이더
        [SerializeField] private TMP_Text mouseSensitivityText; // 마우스 감도 표시 문구
        [SerializeField] private Slider gamepadSensitivitySlider; // 게임패드 시점 속도 슬라이더
        [SerializeField] private TMP_Text gamepadSensitivityText; // 게임패드 시점 속도 표시 문구
        [SerializeField] private Toggle invertLookYToggle; // Y축 반전 토글

        [Header("Actions")] // 하단 작업 버튼 참조 구역
        [SerializeField] private Button defaultsButton; // 기본값 미리보기 버튼
        [SerializeField] private Button cancelButton; // 취소 버튼
        [SerializeField] private Button applyButton; // 적용 버튼

        private static readonly FullScreenMode[] ScreenModes = // 화면 모드 선택 목록
        { // 화면 모드 선택값 구성
            FullScreenMode.FullScreenWindow, // 전체 화면 창
            FullScreenMode.Windowed, // 창 모드
            FullScreenMode.ExclusiveFullScreen, // 독점 전체 화면
        }; // 화면 모드 선택값 구성 마무리

        private static readonly int[] FrameRates = // 최대 FPS 선택 목록
        { // 최대 FPS 선택값 구성
            -1, 30, 60, 90, 120, 144, 165, 240, 360, // 제한 없음과 일반 모니터 FPS
        }; // 최대 FPS 선택값 구성 마무리

        private static readonly RebindSpec[] RebindSpecs = // 기본 Keyboard&Mouse 재지정 대상 목록
        { // 조작 탭 표시 순서와 실제 InputAction 대상 구성
            new RebindSpec("앞으로 이동", ProjectInputNames.Gameplay.Move, "up"), // Move 2DVector 위쪽 키
            new RebindSpec("뒤로 이동", ProjectInputNames.Gameplay.Move, "down"), // Move 2DVector 아래쪽 키
            new RebindSpec("왼쪽 이동", ProjectInputNames.Gameplay.Move, "left"), // Move 2DVector 왼쪽 키
            new RebindSpec("오른쪽 이동", ProjectInputNames.Gameplay.Move, "right"), // Move 2DVector 오른쪽 키
            new RebindSpec("점프", ProjectInputNames.Gameplay.Jump, null), // Jump 기본 키
            new RebindSpec("달리기", ProjectInputNames.Gameplay.Sprint, null), // Sprint 기본 키
            new RebindSpec("앉기", ProjectInputNames.Gameplay.Crouch, null), // Crouch 기본 키
            new RebindSpec("밀치기", ProjectInputNames.Gameplay.Push, null), // Push 기본 키
            new RebindSpec("아이템 사용", ProjectInputNames.Gameplay.UseItem, null), // UseItem 기본 키
            new RebindSpec("이전 아이템", ProjectInputNames.Gameplay.SelectPreviousItem, null), // 이전 슬롯 선택 키
            new RebindSpec("다음 아이템", ProjectInputNames.Gameplay.SelectNextItem, null), // 다음 슬롯 선택 키
            new RebindSpec("아이템 보여주기", ProjectInputNames.Gameplay.ShowItem, null), // ShowItem 기본 키
            new RebindSpec("아이템 버리기", ProjectInputNames.Gameplay.DropItem, null), // DropItem 기본 키
            new RebindSpec("상호작용", ProjectInputNames.Gameplay.Interact, null), // Interact 기본 키
            new RebindSpec("순위표", ProjectInputNames.Gameplay.Scoreboard, null), // Scoreboard 기본 키
        }; // 조작 탭 표시 순서와 실제 InputAction 대상 구성 마무리

        private readonly List<Vector2Int> resolutions = new List<Vector2Int>(); // 현재 장치 해상도 목록
        private ProjectUserSettings workingCopy; // 화면에서 수정할 설정 작업 복사본
        private InputActionAsset runtimeRebindActions; // 설정 메뉴 전용 InputActionAsset 복제본
        private InputActionMap runtimeRebindGameplayMap; // 설정 메뉴 전용 Gameplay 액션 맵
        private InputActionRebindingExtensions.RebindingOperation rebindOperation; // 현재 진행 중 Interactive Rebinding 작업
        private UnityAction[] rebindClickActions = Array.Empty<UnityAction>(); // 키 변경 버튼별 이벤트 해제용 델리게이트
        private int resolutionIndex; // 현재 해상도 선택 인덱스
        private int screenModeIndex; // 현재 화면 모드 선택 인덱스
        private int frameRateIndex; // 현재 최대 FPS 선택 인덱스
        private int selectedTabIndex; // 현재 선택된 설정 탭 인덱스
        private int activeRebindSpecIndex = -1; // 현재 키 입력 대기 중인 조작 인덱스
        private string previousOverridePath = string.Empty; // 중복 키 거부 시 복원할 이전 Override 경로
        private bool ignoreCallbacks; // UI 값 동기화 중 이벤트 무시 여부

        public static int RebindEntryCount => RebindSpecs.Length; // Editor Setup Tool이 생성할 키 재지정 행 개수 반환

        public static string GetRebindEntryLabel(int index) // Editor Setup Tool용 키 재지정 행 이름 반환
        { // 조작 목록 인덱스 안전 검사와 표시 이름 제공
            if (index < 0 || index >= RebindSpecs.Length) // 조작 목록 범위 초과 여부 확인
            { // 잘못된 Editor Setup Tool 요청 방어
                return string.Empty; // 빈 조작 이름 반환
            } // 잘못된 Editor Setup Tool 요청 방어 마무리

            return RebindSpecs[index].DisplayName; // 지정 조작의 한국어 표시 이름 반환
        } // Editor Setup Tool용 키 재지정 행 이름 반환 마무리

        private void Awake() // UI 범위와 이벤트 연결 초기화
        { // 설정 메뉴 초기 상태 준비
            ConfigureSliders(); // 슬라이더 범위 설정
            RegisterListeners(); // 버튼과 값 변경 이벤트 연결
            ShowMainMenu(); // 시작 시 임시 메인 메뉴 표시
        } // UI 범위와 이벤트 연결 초기화 마무리

        private void OnDestroy() // UI 이벤트와 키 재지정 복제본 정리
        { // Scene 종료 시 Input System 임시 리소스 해제
            UnregisterListeners(); // 버튼과 값 변경 이벤트 해제
            DisposeRebindOperation(); // 진행 중 Interactive Rebinding 작업 폐기
            DestroyRebindPreview(); // 설정 메뉴용 InputActionAsset 복제본 제거
        } // UI 이벤트와 키 재지정 복제본 정리 마무리

        public void ConfigureForEditor(GameObject configuredMainMenuRoot, Button configuredOpenButton, GameObject configuredSettingsPanel, TMP_Text configuredStatusText, Button[] configuredTabButtons, GameObject[] configuredTabPanels, TMP_Text configuredResolutionText, Button configuredResolutionPreviousButton, Button configuredResolutionNextButton, TMP_Text configuredScreenModeText, Button configuredScreenModePreviousButton, Button configuredScreenModeNextButton, TMP_Text configuredFrameRateText, Button configuredFrameRatePreviousButton, Button configuredFrameRateNextButton, Toggle configuredVSyncToggle, Slider configuredMasterVolumeSlider, TMP_Text configuredMasterVolumeText, Slider configuredMusicVolumeSlider, TMP_Text configuredMusicVolumeText, Slider configuredSfxVolumeSlider, TMP_Text configuredSfxVolumeText, Toggle configuredMuteToggle, TMP_Text configuredControlsInfoText, Slider configuredMouseSensitivitySlider, TMP_Text configuredMouseSensitivityText, Slider configuredGamepadSensitivitySlider, TMP_Text configuredGamepadSensitivityText, Toggle configuredInvertLookYToggle, Button configuredDefaultsButton, Button configuredCancelButton, Button configuredApplyButton) // 51일차 Editor 자동 생성 UI 참조 일괄 연결
        { // 기존 Day51 Setup Tool 호환 참조 저장
            mainMenuRoot = configuredMainMenuRoot; // 메인 메뉴 루트 저장
            openSettingsButton = configuredOpenButton; // 설정 열기 버튼 저장
            settingsPanel = configuredSettingsPanel; // 설정 화면 루트 저장
            statusText = configuredStatusText; // 상태 문구 저장
            tabButtons = configuredTabButtons; // 탭 버튼 배열 저장
            tabPanels = configuredTabPanels; // 탭 내용 배열 저장
            resolutionText = configuredResolutionText; // 해상도 문구 저장
            resolutionPreviousButton = configuredResolutionPreviousButton; // 이전 해상도 버튼 저장
            resolutionNextButton = configuredResolutionNextButton; // 다음 해상도 버튼 저장
            screenModeText = configuredScreenModeText; // 화면 모드 문구 저장
            screenModePreviousButton = configuredScreenModePreviousButton; // 이전 화면 모드 버튼 저장
            screenModeNextButton = configuredScreenModeNextButton; // 다음 화면 모드 버튼 저장
            frameRateText = configuredFrameRateText; // FPS 문구 저장
            frameRatePreviousButton = configuredFrameRatePreviousButton; // 이전 FPS 버튼 저장
            frameRateNextButton = configuredFrameRateNextButton; // 다음 FPS 버튼 저장
            vSyncToggle = configuredVSyncToggle; // VSync 토글 저장
            masterVolumeSlider = configuredMasterVolumeSlider; // 마스터 슬라이더 저장
            masterVolumeText = configuredMasterVolumeText; // 마스터 문구 저장
            musicVolumeSlider = configuredMusicVolumeSlider; // BGM 슬라이더 저장
            musicVolumeText = configuredMusicVolumeText; // BGM 문구 저장
            sfxVolumeSlider = configuredSfxVolumeSlider; // SFX 슬라이더 저장
            sfxVolumeText = configuredSfxVolumeText; // SFX 문구 저장
            muteToggle = configuredMuteToggle; // 음소거 토글 저장
            controlsInfoText = configuredControlsInfoText; // 조작 안내 문구 저장
            mouseSensitivitySlider = configuredMouseSensitivitySlider; // 마우스 감도 슬라이더 저장
            mouseSensitivityText = configuredMouseSensitivityText; // 마우스 감도 문구 저장
            gamepadSensitivitySlider = configuredGamepadSensitivitySlider; // 게임패드 감도 슬라이더 저장
            gamepadSensitivityText = configuredGamepadSensitivityText; // 게임패드 감도 문구 저장
            invertLookYToggle = configuredInvertLookYToggle; // Y축 반전 토글 저장
            defaultsButton = configuredDefaultsButton; // 기본값 버튼 저장
            cancelButton = configuredCancelButton; // 취소 버튼 저장
            applyButton = configuredApplyButton; // 적용 버튼 저장
        } // 기존 Day51 Setup Tool 호환 참조 저장 마무리

        public void ConfigureDay52Extras(InputActionAsset configuredInputActions, Slider configuredBrightnessSlider, TMP_Text configuredBrightnessText, Slider configuredUiVolumeSlider, TMP_Text configuredUiVolumeText, Button configuredResetBindingsButton, Button[] configuredRebindButtons, TMP_Text[] configuredRebindValueTexts) // 52일차 추가 설정 UI 참조 연결
        { // 밝기와 UI 음량과 키 재지정 전용 참조 저장
            inputActions = configuredInputActions; // 원본 InputActionAsset 저장
            brightnessSlider = configuredBrightnessSlider; // 밝기 Slider 저장
            brightnessText = configuredBrightnessText; // 밝기 표시 문구 저장
            uiVolumeSlider = configuredUiVolumeSlider; // UI 음량 Slider 저장
            uiVolumeText = configuredUiVolumeText; // UI 음량 표시 문구 저장
            resetBindingsButton = configuredResetBindingsButton; // 키 기본값 버튼 저장
            rebindButtons = configuredRebindButtons ?? Array.Empty<Button>(); // 키 변경 버튼 배열 안전 저장
            rebindValueTexts = configuredRebindValueTexts ?? Array.Empty<TMP_Text>(); // 현재 키 문구 배열 안전 저장
        } // 밝기와 UI 음량과 키 재지정 전용 참조 저장 마무리

        private void ConfigureSliders() // 설정 데이터 검증 범위와 Slider 범위 통일
        { // 화면·사운드·카메라 Slider 유효 범위 적용
            SetSliderRange(brightnessSlider, 0.5f, 1.5f, false); // 밝기 50퍼센트부터 150퍼센트 범위 적용
            SetSliderRange(masterVolumeSlider, 0f, 1f, false); // 마스터 음량 범위 적용
            SetSliderRange(musicVolumeSlider, 0f, 1f, false); // BGM 음량 범위 적용
            SetSliderRange(sfxVolumeSlider, 0f, 1f, false); // SFX 음량 범위 적용
            SetSliderRange(uiVolumeSlider, 0f, 1f, false); // UI 음량 범위 적용
            SetSliderRange(mouseSensitivitySlider, 0.01f, 2f, false); // 마우스 감도 범위 적용
            SetSliderRange(gamepadSensitivitySlider, 30f, 720f, true); // 게임패드 시점 속도 범위 적용
        } // 설정 데이터 검증 범위와 Slider 범위 통일 마무리

        private void RegisterListeners() // 설정 UI 이벤트 연결
        { // 버튼과 Slider와 Toggle과 키 재지정 동작 연결
            openSettingsButton?.onClick.AddListener(OpenSettings); // 설정 열기 연결
            resolutionPreviousButton?.onClick.AddListener(SelectPreviousResolution); // 이전 해상도 연결
            resolutionNextButton?.onClick.AddListener(SelectNextResolution); // 다음 해상도 연결
            screenModePreviousButton?.onClick.AddListener(SelectPreviousScreenMode); // 이전 화면 모드 연결
            screenModeNextButton?.onClick.AddListener(SelectNextScreenMode); // 다음 화면 모드 연결
            frameRatePreviousButton?.onClick.AddListener(SelectPreviousFrameRate); // 이전 FPS 연결
            frameRateNextButton?.onClick.AddListener(SelectNextFrameRate); // 다음 FPS 연결
            vSyncToggle?.onValueChanged.AddListener(HandleVSyncChanged); // VSync 변경 연결
            brightnessSlider?.onValueChanged.AddListener(HandleBrightnessChanged); // 밝기 변경 연결
            masterVolumeSlider?.onValueChanged.AddListener(HandleMasterChanged); // 마스터 음량 변경 연결
            musicVolumeSlider?.onValueChanged.AddListener(HandleMusicChanged); // BGM 음량 변경 연결
            sfxVolumeSlider?.onValueChanged.AddListener(HandleSfxChanged); // SFX 음량 변경 연결
            uiVolumeSlider?.onValueChanged.AddListener(HandleUiVolumeChanged); // UI 음량 변경 연결
            muteToggle?.onValueChanged.AddListener(HandleMuteChanged); // 음소거 변경 연결
            mouseSensitivitySlider?.onValueChanged.AddListener(HandleMouseSensitivityChanged); // 마우스 감도 변경 연결
            gamepadSensitivitySlider?.onValueChanged.AddListener(HandleGamepadSensitivityChanged); // 게임패드 시점 속도 변경 연결
            invertLookYToggle?.onValueChanged.AddListener(HandleInvertChanged); // Y축 반전 변경 연결
            resetBindingsButton?.onClick.AddListener(ResetBindingOverridesPreview); // 키 기본값 미리보기 연결
            defaultsButton?.onClick.AddListener(PreviewDefaults); // 전체 기본값 버튼 연결
            cancelButton?.onClick.AddListener(CancelChanges); // 취소 버튼 연결
            applyButton?.onClick.AddListener(ApplyChanges); // 적용 버튼 연결

            if (tabButtons != null) // 탭 버튼 배열 존재 여부 확인
            { // 4개 탭 버튼 이벤트 연결
                if (tabButtons.Length > 0 && tabButtons[0] != null) tabButtons[0].onClick.AddListener(ShowScreenTab); // 화면 탭 연결
                if (tabButtons.Length > 1 && tabButtons[1] != null) tabButtons[1].onClick.AddListener(ShowSoundTab); // 사운드 탭 연결
                if (tabButtons.Length > 2 && tabButtons[2] != null) tabButtons[2].onClick.AddListener(ShowControlsTab); // 조작 탭 연결
                if (tabButtons.Length > 3 && tabButtons[3] != null) tabButtons[3].onClick.AddListener(ShowCameraTab); // 카메라 탭 연결
            } // 4개 탭 버튼 이벤트 연결 마무리

            RegisterRebindButtonListeners(); // 동적 키 재지정 버튼별 이벤트 연결
        } // 설정 UI 이벤트 연결 마무리

        private void RegisterRebindButtonListeners() // 키 재지정 버튼 배열 이벤트 연결
        { // 각 버튼이 자신의 조작 인덱스를 전달하도록 델리게이트 저장
            if (rebindButtons == null || rebindButtons.Length == 0) // 키 재지정 버튼 없음 여부 확인
            { // 51일차 Scene 호환 처리
                rebindClickActions = Array.Empty<UnityAction>(); // 빈 이벤트 배열 저장
                return; // 키 재지정 이벤트 연결 생략
            } // 51일차 Scene 호환 처리 마무리

            rebindClickActions = new UnityAction[rebindButtons.Length]; // 버튼 개수와 같은 이벤트 배열 생성

            for (int index = 0; index < rebindButtons.Length; index++) // 모든 키 재지정 버튼 순회
            { // 현재 버튼의 고정 조작 인덱스 이벤트 생성
                int capturedIndex = index; // Lambda가 사용할 현재 조작 인덱스 복사
                UnityAction action = () => BeginRebind(capturedIndex); // 현재 조작 재지정 시작 델리게이트 생성
                rebindClickActions[index] = action; // 나중에 해제할 델리게이트 저장
                rebindButtons[index]?.onClick.AddListener(action); // 현재 키 변경 버튼 이벤트 연결
            } // 현재 버튼의 고정 조작 인덱스 이벤트 생성 마무리
        } // 키 재지정 버튼 배열 이벤트 연결 마무리

        private void UnregisterListeners() // 설정 UI 이벤트 해제
        { // 버튼과 Slider와 Toggle과 키 재지정 동작 연결 제거
            openSettingsButton?.onClick.RemoveListener(OpenSettings); // 설정 열기 해제
            resolutionPreviousButton?.onClick.RemoveListener(SelectPreviousResolution); // 이전 해상도 해제
            resolutionNextButton?.onClick.RemoveListener(SelectNextResolution); // 다음 해상도 해제
            screenModePreviousButton?.onClick.RemoveListener(SelectPreviousScreenMode); // 이전 화면 모드 해제
            screenModeNextButton?.onClick.RemoveListener(SelectNextScreenMode); // 다음 화면 모드 해제
            frameRatePreviousButton?.onClick.RemoveListener(SelectPreviousFrameRate); // 이전 FPS 해제
            frameRateNextButton?.onClick.RemoveListener(SelectNextFrameRate); // 다음 FPS 해제
            vSyncToggle?.onValueChanged.RemoveListener(HandleVSyncChanged); // VSync 변경 해제
            brightnessSlider?.onValueChanged.RemoveListener(HandleBrightnessChanged); // 밝기 변경 해제
            masterVolumeSlider?.onValueChanged.RemoveListener(HandleMasterChanged); // 마스터 음량 변경 해제
            musicVolumeSlider?.onValueChanged.RemoveListener(HandleMusicChanged); // BGM 음량 변경 해제
            sfxVolumeSlider?.onValueChanged.RemoveListener(HandleSfxChanged); // SFX 음량 변경 해제
            uiVolumeSlider?.onValueChanged.RemoveListener(HandleUiVolumeChanged); // UI 음량 변경 해제
            muteToggle?.onValueChanged.RemoveListener(HandleMuteChanged); // 음소거 변경 해제
            mouseSensitivitySlider?.onValueChanged.RemoveListener(HandleMouseSensitivityChanged); // 마우스 감도 변경 해제
            gamepadSensitivitySlider?.onValueChanged.RemoveListener(HandleGamepadSensitivityChanged); // 게임패드 시점 속도 변경 해제
            invertLookYToggle?.onValueChanged.RemoveListener(HandleInvertChanged); // Y축 반전 변경 해제
            resetBindingsButton?.onClick.RemoveListener(ResetBindingOverridesPreview); // 키 기본값 버튼 해제
            defaultsButton?.onClick.RemoveListener(PreviewDefaults); // 기본값 버튼 해제
            cancelButton?.onClick.RemoveListener(CancelChanges); // 취소 버튼 해제
            applyButton?.onClick.RemoveListener(ApplyChanges); // 적용 버튼 해제

            if (tabButtons != null) // 탭 버튼 배열 존재 여부 확인
            { // 4개 탭 버튼 이벤트 해제
                if (tabButtons.Length > 0 && tabButtons[0] != null) tabButtons[0].onClick.RemoveListener(ShowScreenTab); // 화면 탭 해제
                if (tabButtons.Length > 1 && tabButtons[1] != null) tabButtons[1].onClick.RemoveListener(ShowSoundTab); // 사운드 탭 해제
                if (tabButtons.Length > 2 && tabButtons[2] != null) tabButtons[2].onClick.RemoveListener(ShowControlsTab); // 조작 탭 해제
                if (tabButtons.Length > 3 && tabButtons[3] != null) tabButtons[3].onClick.RemoveListener(ShowCameraTab); // 카메라 탭 해제
            } // 4개 탭 버튼 이벤트 해제 마무리

            UnregisterRebindButtonListeners(); // 키 변경 버튼별 동적 이벤트 해제
        } // 설정 UI 이벤트 해제 마무리

        private void UnregisterRebindButtonListeners() // 키 재지정 버튼 배열 이벤트 해제
        { // 저장된 UnityAction으로 정확한 Listener 제거
            int count = Mathf.Min(rebindButtons != null ? rebindButtons.Length : 0, rebindClickActions != null ? rebindClickActions.Length : 0); // 안전한 이벤트 해제 개수 계산

            for (int index = 0; index < count; index++) // 연결된 키 재지정 이벤트 순회
            { // 현재 버튼 Listener 해제
                if (rebindButtons[index] != null && rebindClickActions[index] != null) // 버튼과 저장 델리게이트 존재 여부 확인
                { // 정확한 동적 Listener 제거
                    rebindButtons[index].onClick.RemoveListener(rebindClickActions[index]); // 현재 키 변경 버튼 이벤트 해제
                } // 정확한 동적 Listener 제거 마무리
            } // 현재 버튼 Listener 해제 마무리

            rebindClickActions = Array.Empty<UnityAction>(); // 이벤트 배열 초기화
        } // 키 재지정 버튼 배열 이벤트 해제 마무리

        private void OpenSettings() // 현재 설정 작업 복사본으로 설정 화면 열기
        { // SettingsManager 기반 UI 편집 시작
            mainMenuRoot?.SetActive(false); // 임시 메인 메뉴 숨김
            settingsPanel?.SetActive(true); // 설정 화면 표시
            ShowTab(0); // 화면 탭 기본 선택

            if (!SettingsManager.TryCreateWorkingCopy(out workingCopy)) // 설정 서비스 준비 여부 확인
            { // Bootstrap을 거치지 않은 MainMenu 직접 실행 처리
                SetEditingEnabled(false); // 설정 편집 비활성화
                SetStatus("SettingsManager가 준비되지 않았습니다. Bootstrap Scene에서 실행합니다."); // 실행 경로 안내
                return; // 설정 데이터 표시 중단
            } // Bootstrap을 거치지 않은 MainMenu 직접 실행 처리 마무리

            SetEditingEnabled(true); // 설정 편집 활성화
            BuildResolutionList(); // 현재 장치 해상도 목록 생성
            SyncIndexes(); // 작업 설정과 순환 선택 인덱스 동기화
            PrepareRebindPreview(); // 저장된 Binding Override 기반 키 재지정 미리보기 생성
            RefreshUi(); // 전체 작업 설정 UI 표시
            SetStatus("값을 변경해도 적용 전에는 저장되지 않습니다."); // 작업 복사본 안내
        } // 현재 설정 작업 복사본으로 설정 화면 열기 마무리

        private void ShowMainMenu() // 설정 화면 닫기와 작업 복사본 폐기
        { // 미적용 설정과 임시 InputActionAsset 제거
            DisposeRebindOperation(); // 진행 중 키 입력 대기 작업 폐기
            DestroyRebindPreview(); // 키 재지정 미리보기 복제본 제거
            workingCopy = null; // 작업 복사본 폐기
            settingsPanel?.SetActive(false); // 설정 화면 숨김
            mainMenuRoot?.SetActive(true); // 임시 메인 메뉴 표시
        } // 설정 화면 닫기와 작업 복사본 폐기 마무리

        private void PreviewDefaults() // 기본값을 작업 화면에만 불러오기
        { // 실제 저장 없는 전체 설정 기본값 미리보기
            if (!SettingsManager.IsReady) // SettingsService 준비 여부 확인
            { // 기본값 작업 복사본 생성 불가 상태 처리
                SetStatus("SettingsManager가 준비되지 않았습니다."); // 서비스 준비 오류 안내
                return; // 기본값 미리보기 중단
            } // 기본값 작업 복사본 생성 불가 상태 처리 마무리

            workingCopy = SettingsManager.CreateDefaultWorkingCopy(); // 기본 작업 설정 생성
            BuildResolutionList(); // 기본 해상도를 포함한 목록 재생성
            SyncIndexes(); // 기본 설정 선택 인덱스 계산
            PrepareRebindPreview(); // 기본 키 상태 InputActionAsset 복제본 재생성
            RefreshUi(); // 기본값 UI 표시
            SetStatus("기본값 미리보기입니다. 적용 전에는 저장되지 않습니다."); // 기본값 미리보기 안내
        } // 기본값을 작업 화면에만 불러오기 마무리

        private void CancelChanges() // 미적용 변경 취소
        { // 작업 복사본과 키 재지정 미리보기 폐기
            ShowMainMenu(); // 임시 메인 메뉴 복귀
        } // 미적용 변경 취소 마무리

        private void ApplyChanges() // 작업 설정 실제 적용과 저장
        { // SettingsManager.Apply와 Binding Override 통합 저장
            if (rebindOperation != null) // 현재 키 입력 대기 중 여부 확인
            { // 진행 중 재지정 완료 또는 취소 요구
                SetStatus("키 재지정 입력을 완료하거나 Esc로 취소한 뒤 적용합니다."); // 진행 중 키 재지정 안내
                return; // 설정 적용 중단
            } // 진행 중 재지정 완료 또는 취소 요구 마무리

            if (workingCopy == null || !SettingsManager.IsReady) // 적용 가능한 상태 확인
            { // 설정 서비스 미준비 처리
                SetStatus("적용할 설정 데이터가 준비되지 않았습니다."); // 적용 실패 안내
                return; // 적용 중단
            } // 설정 서비스 미준비 처리 마무리

            bool saved = SettingsManager.Apply(workingCopy); // 작업 설정 전체 적용과 JSON 저장 실행
            SetStatus(saved ? "설정을 적용하고 저장했습니다." : "설정 저장에 실패했습니다. Console을 확인합니다."); // 적용 결과 안내

            if (saved) // 저장 성공 여부 확인
            { // 적용 결과 기준 UI 재동기화
                workingCopy = SettingsManager.CreateWorkingCopy(); // 실제 설정 새 작업 복사본 생성
                BuildResolutionList(); // 적용 후 해상도 목록 갱신
                SyncIndexes(); // 적용 후 인덱스 동기화
                PrepareRebindPreview(); // 저장 완료 Binding Override 복제본 재생성
                RefreshUi(); // 실제 적용값 UI 표시
            } // 적용 결과 기준 UI 재동기화 마무리
        } // 작업 설정 실제 적용과 저장 마무리

        private void BuildResolutionList() // 장치 지원 해상도에서 중복 크기 제거
        { // 화면 크기 기준 선택 목록 생성
            resolutions.Clear(); // 이전 해상도 목록 제거
            Resolution[] available = Screen.resolutions; // 현재 장치 지원 해상도 조회

            for (int index = 0; index < available.Length; index++) // 전체 장치 해상도 순회
            { // 현재 해상도 크기 처리
                Vector2Int size = new Vector2Int(available[index].width, available[index].height); // 주사율을 제거한 화면 크기 생성

                if (size.x < 640 || size.y < 360 || resolutions.Contains(size)) // 최소 해상도 미만 또는 중복 여부 확인
                { // 선택 목록 제외 처리
                    continue; // 현재 항목 건너뛰기
                } // 선택 목록 제외 처리 마무리

                resolutions.Add(size); // 새 화면 크기 선택 목록 추가
            } // 현재 해상도 크기 처리 마무리

            Vector2Int current = new Vector2Int(Mathf.Max(640, workingCopy.ResolutionWidth), Mathf.Max(360, workingCopy.ResolutionHeight)); // 현재 작업 해상도 생성

            if (!resolutions.Contains(current)) // 현재 작업 해상도 목록 포함 여부 확인
            { // 저장 해상도 보강 처리
                resolutions.Add(current); // 현재 작업 해상도 목록 추가
            } // 저장 해상도 보강 처리 마무리

            resolutions.Sort(CompareResolution); // 작은 해상도부터 정렬
        } // 장치 지원 해상도에서 중복 크기 제거 마무리

        private void SyncIndexes() // 작업 설정의 현재 선택 인덱스 계산
        { // 해상도·화면 모드·FPS 인덱스 동기화
            resolutionIndex = Mathf.Max(0, resolutions.IndexOf(new Vector2Int(workingCopy.ResolutionWidth, workingCopy.ResolutionHeight))); // 저장 해상도 인덱스 계산
            screenModeIndex = FindScreenModeIndex(workingCopy.FullScreenModeValue); // 저장 화면 모드 인덱스 계산
            frameRateIndex = FindFrameRateIndex(workingCopy.TargetFrameRate); // 저장 FPS 인덱스 계산
        } // 작업 설정의 현재 선택 인덱스 계산 마무리

        private void RefreshUi() // 전체 작업 설정값 UI 표시
        { // UI 이벤트 없이 현재 작업값 동기화
            if (workingCopy == null) // 작업 설정 존재 여부 확인
            { // 작업 설정 없음 처리
                return; // UI 갱신 중단
            } // 작업 설정 없음 처리 마무리

            ignoreCallbacks = true; // Slider와 Toggle 콜백 일시 중지
            SetText(resolutionText, resolutions.Count > 0 ? FormatResolution(resolutions[resolutionIndex]) : "-"); // 해상도 문구 적용
            SetText(screenModeText, FormatScreenMode(ScreenModes[screenModeIndex])); // 화면 모드 문구 적용
            SetText(frameRateText, FormatFrameRate(FrameRates[frameRateIndex])); // 최대 FPS 문구 적용
            SetToggleValue(vSyncToggle, workingCopy.VSyncCount > 0); // VSync 상태 적용
            SetSliderValue(brightnessSlider, workingCopy.Brightness); // 화면 밝기 적용
            SetText(brightnessText, FormatBrightness(workingCopy.Brightness)); // 화면 밝기 문구 적용
            SetSliderValue(masterVolumeSlider, workingCopy.MasterVolume); // 마스터 음량 적용
            SetText(masterVolumeText, FormatPercent(workingCopy.MasterVolume)); // 마스터 음량 문구 적용
            SetSliderValue(musicVolumeSlider, workingCopy.MusicVolume); // BGM 음량 적용
            SetText(musicVolumeText, FormatPercent(workingCopy.MusicVolume)); // BGM 음량 문구 적용
            SetSliderValue(sfxVolumeSlider, workingCopy.SfxVolume); // SFX 음량 적용
            SetText(sfxVolumeText, FormatPercent(workingCopy.SfxVolume)); // SFX 음량 문구 적용
            SetSliderValue(uiVolumeSlider, workingCopy.UiVolume); // UI 음량 적용
            SetText(uiVolumeText, FormatPercent(workingCopy.UiVolume)); // UI 음량 문구 적용
            SetToggleValue(muteToggle, workingCopy.IsMuted); // 음소거 상태 적용
            SetSliderValue(mouseSensitivitySlider, workingCopy.MouseSensitivity); // 마우스 감도 적용
            SetText(mouseSensitivityText, workingCopy.MouseSensitivity.ToString("0.00")); // 마우스 감도 문구 적용
            SetSliderValue(gamepadSensitivitySlider, workingCopy.GamepadLookDegreesPerSecond); // 게임패드 시점 속도 적용
            SetText(gamepadSensitivityText, $"{Mathf.RoundToInt(workingCopy.GamepadLookDegreesPerSecond)}°/s"); // 게임패드 시점 속도 문구 적용
            SetToggleValue(invertLookYToggle, workingCopy.InvertLookY); // Y축 반전 상태 적용
            ignoreCallbacks = false; // Slider와 Toggle 콜백 재개
            RefreshRebindUi(); // 조작 탭 현재 키 문구와 버튼 상태 갱신
        } // 전체 작업 설정값 UI 표시 마무리

        private void PrepareRebindPreview() // 작업 복사본 Binding Override를 적용한 임시 InputActionAsset 생성
        { // 실제 PlayerInputReader를 건드리지 않는 키 재지정 미리보기 준비
            DisposeRebindOperation(); // 이전 재지정 작업 폐기
            DestroyRebindPreview(); // 이전 InputActionAsset 복제본 제거

            if (inputActions == null || workingCopy == null) // 원본 InputActionAsset 또는 작업 설정 누락 여부 확인
            { // 키 재지정 UI 사용 불가 처리
                return; // 미리보기 생성 생략
            } // 키 재지정 UI 사용 불가 처리 마무리

            runtimeRebindActions = Instantiate(inputActions); // 원본 InputActionAsset 런타임 복제
            runtimeRebindGameplayMap = runtimeRebindActions.FindActionMap(ProjectInputNames.Gameplay.Map, false); // 복제본 Gameplay 맵 조회

            if (runtimeRebindGameplayMap == null) // Gameplay 맵 누락 여부 확인
            { // 입력 에셋 구성 오류 처리
                ProjectLog.Warning(ProjectLogCategory.Input, "설정 메뉴에서 Gameplay 액션 맵을 찾지 못했습니다.", "SETTINGS_REBIND_MAP_MISSING", this); // 키 재지정 맵 누락 경고
                return; // Binding Override 적용 생략
            } // 입력 에셋 구성 오류 처리 마무리

            if (string.IsNullOrWhiteSpace(workingCopy.InputBindingOverridesJson)) // 작업 복사본에 저장된 Override 없음 여부 확인
            { // 기본 키 상태 유지
                return; // 추가 JSON 적용 없이 기본 바인딩 사용
            } // 기본 키 상태 유지 마무리

            try // 저장된 Binding Override JSON 적용 예외 감시
            { // 설정 메뉴용 InputActionAsset에 기존 사용자 키 적용
                runtimeRebindActions.LoadBindingOverridesFromJson(workingCopy.InputBindingOverridesJson, true); // 작업 설정의 입력 재지정 JSON 적용
            } // 설정 메뉴용 InputActionAsset에 기존 사용자 키 적용 마무리
            catch (Exception exception) // 손상된 Binding Override JSON 처리
            { // 설정 메뉴에서 안전하게 기본 키로 복구
                runtimeRebindActions.RemoveAllBindingOverrides(); // 잘못된 재지정 제거
                workingCopy.InputBindingOverridesJson = string.Empty; // 작업 복사본 재지정 JSON 기본값 복구
                ProjectLog.Warning(ProjectLogCategory.Input, $"설정 메뉴 입력 재지정 JSON을 읽지 못해 기본 키를 표시합니다. {exception.Message}", "SETTINGS_REBIND_JSON_INVALID", this); // 손상 재지정 경고
            } // 설정 메뉴에서 안전하게 기본 키로 복구 마무리
        } // 작업 복사본 Binding Override를 적용한 임시 InputActionAsset 생성 마무리

        private void DestroyRebindPreview() // 설정 메뉴 전용 InputActionAsset 복제본 제거
        { // Scene 전환과 설정 닫기 때 임시 입력 리소스 정리
            runtimeRebindGameplayMap = null; // 임시 Gameplay 맵 참조 초기화

            if (runtimeRebindActions == null) // 제거할 InputActionAsset 복제본 없음 여부 확인
            { // 임시 입력 리소스 없음 처리
                return; // Destroy 생략
            } // 임시 입력 리소스 없음 처리 마무리

            Destroy(runtimeRebindActions); // 런타임 InputActionAsset 복제본 제거
            runtimeRebindActions = null; // 임시 InputActionAsset 참조 초기화
        } // 설정 메뉴 전용 InputActionAsset 복제본 제거 마무리

        private void RefreshRebindUi() // 조작 탭 현재 키 문구와 버튼 상태 갱신
        { // 작업용 Gameplay 맵의 effectivePath 기반 표시
            bool canRebind = workingCopy != null && runtimeRebindGameplayMap != null && inputActions != null; // 키 재지정 기능 사용 가능 여부 계산
            string overrideState = workingCopy != null && !string.IsNullOrWhiteSpace(workingCopy.InputBindingOverridesJson) ? "사용자 키 재지정 있음" : "기본 키 사용 중"; // 현재 작업 복사본 재지정 상태 문구 계산
            SetText(controlsInfoText, canRebind ? $"{overrideState} · 변경 버튼을 누른 뒤 새 키를 입력합니다. Esc는 취소입니다." : "InputSystem_Actions 참조가 없어 키 재지정을 사용할 수 없습니다."); // 조작 탭 안내 문구 적용
            SetButtonInteractable(resetBindingsButton, canRebind && rebindOperation == null); // 키 기본값 버튼 상태 적용

            int count = Mathf.Min(RebindSpecs.Length, Mathf.Min(rebindButtons != null ? rebindButtons.Length : 0, rebindValueTexts != null ? rebindValueTexts.Length : 0)); // 안전한 재지정 UI 행 개수 계산

            for (int index = 0; index < count; index++) // 키 재지정 UI 행 전체 순회
            { // 현재 조작의 바인딩과 표시 문구 갱신
                RebindSpec spec = RebindSpecs[index]; // 현재 조작 재지정 사양 조회
                InputAction action = runtimeRebindGameplayMap != null ? runtimeRebindGameplayMap.FindAction(spec.ActionName, false) : null; // 현재 조작 InputAction 조회
                int bindingIndex = InputBindingConflictRules.FindKeyboardMouseBindingIndex(action, spec.CompositePartName); // Keyboard&Mouse 대상 바인딩 인덱스 검색
                bool rowAvailable = canRebind && action != null && bindingIndex >= 0; // 현재 조작 재지정 가능 여부 계산
                string display = rowAvailable ? action.GetBindingDisplayString(bindingIndex) : "-"; // 현재 유효 키 사용자 표시 문구 생성
                SetText(rebindValueTexts[index], display); // 현재 키 문구 적용
                SetButtonInteractable(rebindButtons[index], rowAvailable && rebindOperation == null); // 현재 키 변경 버튼 상태 적용
            } // 현재 조작의 바인딩과 표시 문구 갱신 마무리
        } // 조작 탭 현재 키 문구와 버튼 상태 갱신 마무리

        private void BeginRebind(int specIndex) // 지정 조작의 Interactive Rebinding 시작
        { // 작업 InputActionAsset에만 새 Keyboard 또는 Mouse Button 키 입력 대기
            if (workingCopy == null || runtimeRebindGameplayMap == null || specIndex < 0 || specIndex >= RebindSpecs.Length) // 재지정 시작 조건 확인
            { // 잘못된 재지정 시작 요청 방어
                SetStatus("키 재지정 데이터를 준비하지 못했습니다."); // 재지정 준비 실패 안내
                return; // 키 재지정 시작 중단
            } // 잘못된 재지정 시작 요청 방어 마무리

            DisposeRebindOperation(); // 이전 재지정 작업 안전 폐기
            RebindSpec spec = RebindSpecs[specIndex]; // 현재 조작 재지정 사양 조회
            InputAction action = runtimeRebindGameplayMap.FindAction(spec.ActionName, false); // 대상 InputAction 조회
            int bindingIndex = InputBindingConflictRules.FindKeyboardMouseBindingIndex(action, spec.CompositePartName); // 대상 Keyboard&Mouse 바인딩 인덱스 검색

            if (action == null || bindingIndex < 0) // 실제 바인딩 대상 누락 여부 확인
            { // InputAction 구성 오류 처리
                SetStatus($"{spec.DisplayName}의 Keyboard&Mouse 바인딩을 찾지 못했습니다."); // 대상 바인딩 누락 안내
                return; // Interactive Rebinding 시작 중단
            } // InputAction 구성 오류 처리 마무리

            activeRebindSpecIndex = specIndex; // 현재 입력 대기 중인 조작 인덱스 저장
            previousOverridePath = action.bindings[bindingIndex].overridePath ?? string.Empty; // 중복 시 복원할 이전 Override 경로 저장
            EventSystem.current?.SetSelectedGameObject(null); // 입력 대기 중 UI 키보드 포커스 해제
            SetRebindCaptureState(true); // 키 입력 대기 중 다른 설정 조작 잠금
            SetTextAt(rebindValueTexts, specIndex, "입력 대기..."); // 현재 키 문구를 입력 대기 상태로 변경
            SetStatus($"{spec.DisplayName}에 사용할 새 키 또는 마우스 버튼을 누릅니다. Esc는 취소입니다."); // 새 키 입력 안내
            rebindOperation = action.PerformInteractiveRebinding(bindingIndex) // 지정 Keyboard&Mouse 바인딩 Interactive Rebinding 생성
                .WithExpectedControlType<ButtonControl>() // 키와 마우스 버튼 계열 입력만 허용
                .WithControlsExcluding("<Gamepad>") // 게임패드 재지정 제외
                .WithControlsExcluding("<Joystick>") // 일반 조이스틱 재지정 제외
                .WithControlsExcluding("<Touchscreen>") // 터치 입력 재지정 제외
                .WithControlsExcluding("<XRController>") // XR 컨트롤러 재지정 제외
                .WithCancelingThrough("<Keyboard>/escape") // Escape 키를 재지정 취소 입력으로 예약
                .OnMatchWaitForAnother(0.05f) // 짧은 복합 입력 노이즈 대기
                .OnCancel(HandleRebindCancelled) // Escape 취소 콜백 연결
                .OnComplete(HandleRebindCompleted); // 새 키 입력 완료 콜백 연결
            rebindOperation.Start(); // Interactive Rebinding 입력 대기 시작
        } // 지정 조작의 Interactive Rebinding 시작 마무리

        private void HandleRebindCompleted(InputActionRebindingExtensions.RebindingOperation operation) // 새 키 입력 완료와 중복 검사 처리
        { // 중복 키면 이전 바인딩 복원 후 작업 JSON 미변경
            int completedSpecIndex = activeRebindSpecIndex; // 완료된 조작 인덱스 임시 저장
            RebindSpec spec = completedSpecIndex >= 0 && completedSpecIndex < RebindSpecs.Length ? RebindSpecs[completedSpecIndex] : default; // 완료된 조작 사양 안전 조회
            InputAction action = runtimeRebindGameplayMap != null ? runtimeRebindGameplayMap.FindAction(spec.ActionName, false) : null; // 완료 대상 InputAction 다시 조회
            int bindingIndex = InputBindingConflictRules.FindKeyboardMouseBindingIndex(action, spec.CompositePartName); // 완료 대상 바인딩 인덱스 다시 검색
            operation.Dispose(); // 완료된 RebindingOperation 리소스 해제
            rebindOperation = null; // 진행 중 재지정 작업 참조 초기화

            if (action == null || bindingIndex < 0) // 완료 대상 InputAction 유효성 확인
            { // 완료 직후 입력 에셋 상태 이상 처리
                ResetActiveRebindState(); // 현재 재지정 상태 초기화
                SetRebindCaptureState(false); // 설정 UI 조작 잠금 해제
                RefreshRebindUi(); // 현재 키 UI 원상 복구
                SetStatus("키 재지정 완료 대상을 다시 찾지 못했습니다."); // 대상 누락 안내
                return; // 완료 처리 중단
            } // 완료 직후 입력 에셋 상태 이상 처리 마무리

            if (InputBindingConflictRules.HasDuplicateEffectivePath(runtimeRebindGameplayMap, action, bindingIndex)) // 다른 Gameplay 조작과 같은 키 중복 여부 확인
            { // 중복 키 재지정 거부
                RestorePreviousBindingOverride(action, bindingIndex); // 새 중복 키를 이전 상태로 복원
                ResetActiveRebindState(); // 현재 재지정 상태 초기화
                SetRebindCaptureState(false); // 설정 UI 조작 잠금 해제
                RefreshRebindUi(); // 복원된 기존 키 문구 표시
                SetStatus("이미 다른 조작에서 사용하는 키입니다. 다른 키를 지정합니다."); // 중복 키 거부 안내
                return; // 중복 키 저장 방지
            } // 중복 키 재지정 거부 마무리

            workingCopy.InputBindingOverridesJson = runtimeRebindActions.SaveBindingOverridesAsJson(); // 현재 작업용 InputActionAsset의 전체 Override JSON 저장
            ResetActiveRebindState(); // 현재 재지정 상태 초기화
            SetRebindCaptureState(false); // 설정 UI 조작 잠금 해제
            RefreshRebindUi(); // 새 키 표시 문구 갱신
            SetStatus($"{spec.DisplayName} 키를 작업 복사본에서 변경했습니다. 적용 전에는 저장되지 않습니다."); // 재지정 미저장 상태 안내
        } // 새 키 입력 완료와 중복 검사 처리 마무리

        private void HandleRebindCancelled(InputActionRebindingExtensions.RebindingOperation operation) // Escape 기반 키 재지정 취소 처리
        { // 입력 대기 종료와 기존 키 유지
            operation.Dispose(); // 취소된 RebindingOperation 리소스 해제
            rebindOperation = null; // 진행 중 재지정 작업 참조 초기화
            ResetActiveRebindState(); // 현재 재지정 상태 초기화
            SetRebindCaptureState(false); // 설정 UI 조작 잠금 해제
            RefreshRebindUi(); // 기존 키 문구 복원
            SetStatus("키 재지정을 취소했습니다."); // 재지정 취소 안내
        } // Escape 기반 키 재지정 취소 처리 마무리

        private void RestorePreviousBindingOverride(InputAction action, int bindingIndex) // 중복 키 거부 시 기존 Override 상태 복원
        { // 기본 바인딩과 기존 사용자 Override 구분 복원
            action.RemoveBindingOverride(bindingIndex); // 새로 적용된 중복 Override 제거

            if (!string.IsNullOrWhiteSpace(previousOverridePath)) // 이전 사용자 Override 존재 여부 확인
            { // 기존 사용자 재지정 경로 복구
                action.ApplyBindingOverride(bindingIndex, previousOverridePath); // 이전 Override 경로 다시 적용
            } // 기존 사용자 재지정 경로 복구 마무리
        } // 중복 키 거부 시 기존 Override 상태 복원 마무리

        private void ResetBindingOverridesPreview() // 조작 탭의 키 재지정을 기본값으로 미리보기
        { // 실제 저장 파일과 PlayerInputReader를 건드리지 않는 작업 복사본 초기화
            if (workingCopy == null || runtimeRebindActions == null) // 키 기본값 미리보기 준비 여부 확인
            { // InputActionAsset 미준비 처리
                SetStatus("키 재지정 데이터를 준비하지 못했습니다."); // 키 초기화 실패 안내
                return; // 기본 키 미리보기 중단
            } // InputActionAsset 미준비 처리 마무리

            DisposeRebindOperation(); // 진행 중 키 재지정 작업 폐기
            runtimeRebindActions.RemoveAllBindingOverrides(); // 작업 InputActionAsset의 모든 사용자 Override 제거
            workingCopy.InputBindingOverridesJson = string.Empty; // 작업 복사본의 입력 재지정 JSON 기본값 적용
            ResetActiveRebindState(); // 현재 재지정 상태 초기화
            SetRebindCaptureState(false); // 설정 UI 조작 잠금 해제
            RefreshRebindUi(); // 기본 키 문구 갱신
            SetStatus("기본 키를 작업 화면에 불러왔습니다. 적용 전에는 저장되지 않습니다."); // 기본 키 미리보기 안내
        } // 조작 탭의 키 재지정을 기본값으로 미리보기 마무리

        private void DisposeRebindOperation() // 진행 중 Interactive Rebinding 리소스 강제 정리
        { // 설정 닫기와 Scene 종료 때 Callback 없이 작업 폐기
            if (rebindOperation == null) // 진행 중 RebindingOperation 없음 여부 확인
            { // 정리할 입력 대기 작업 없음 처리
                ResetActiveRebindState(); // 재지정 상태 안전 초기화
                return; // Dispose 생략
            } // 정리할 입력 대기 작업 없음 처리 마무리

            rebindOperation.Dispose(); // 진행 중 Interactive Rebinding 리소스 해제
            rebindOperation = null; // RebindingOperation 참조 초기화
            ResetActiveRebindState(); // 현재 재지정 상태 초기화
        } // 진행 중 Interactive Rebinding 리소스 강제 정리 마무리

        private void ResetActiveRebindState() // 현재 키 재지정 임시 상태 초기화
        { // 중복 복원용 정보와 활성 인덱스 제거
            activeRebindSpecIndex = -1; // 활성 재지정 조작 인덱스 초기화
            previousOverridePath = string.Empty; // 이전 Override 경로 임시값 초기화
        } // 현재 키 재지정 임시 상태 초기화 마무리

        private void SetRebindCaptureState(bool isCapturing) // 키 입력 대기 중 다른 UI 조작 잠금 또는 복원
        { // 마우스 클릭과 키보드 UI 이동이 새 키로 오인되는 상황 방지
            if (isCapturing) // Interactive Rebinding 입력 대기 시작 여부 확인
            { // 설정 메뉴 대부분의 조작 일시 잠금
                SetEditingEnabled(false); // 모든 설정 편집 컨트롤 비활성화
                SetButtonInteractable(cancelButton, false); // 마우스 취소 버튼도 입력 캡처 중 비활성화

                if (tabButtons != null) // 탭 버튼 배열 존재 여부 확인
                { // 키 입력 대기 중 탭 이동 잠금
                    for (int index = 0; index < tabButtons.Length; index++) // 전체 탭 버튼 순회
                    { // 현재 탭 버튼 비활성화
                        SetButtonInteractable(tabButtons[index], false); // 입력 대기 중 탭 버튼 잠금
                    } // 현재 탭 버튼 비활성화 마무리
                } // 키 입력 대기 중 탭 이동 잠금 마무리

                return; // 입력 대기 잠금 처리 완료
            } // 설정 메뉴 대부분의 조작 일시 잠금 마무리

            bool canEdit = workingCopy != null && SettingsManager.IsReady; // 설정 메뉴 편집 가능 상태 다시 계산
            SetEditingEnabled(canEdit); // 설정 편집 컨트롤 상태 복원
            SetButtonInteractable(cancelButton, true); // 취소 버튼 복원
            ShowTab(selectedTabIndex); // 선택 중이던 탭 버튼 상태 복원
        } // 키 입력 대기 중 다른 UI 조작 잠금 또는 복원 마무리

        private void SelectPreviousResolution() // 이전 해상도 선택
        { // 해상도 인덱스 감소 처리
            ChangeResolution(-1); // 이전 해상도 적용
        } // 이전 해상도 선택 마무리

        private void SelectNextResolution() // 다음 해상도 선택
        { // 해상도 인덱스 증가 처리
            ChangeResolution(1); // 다음 해상도 적용
        } // 다음 해상도 선택 마무리

        private void ChangeResolution(int offset) // 작업 해상도 순환 변경
        { // 작업 복사본 화면 크기 갱신
            if (workingCopy == null || resolutions.Count == 0) return; // 설정 데이터 없음 시 변경 중단
            resolutionIndex = WrapIndex(resolutionIndex + offset, resolutions.Count); // 해상도 순환 인덱스 계산
            workingCopy.ResolutionWidth = resolutions[resolutionIndex].x; // 작업 가로 해상도 변경
            workingCopy.ResolutionHeight = resolutions[resolutionIndex].y; // 작업 세로 해상도 변경
            SetText(resolutionText, FormatResolution(resolutions[resolutionIndex])); // 해상도 문구 갱신
            SetStatus("화면 설정이 변경되었습니다. 적용 전에는 저장되지 않습니다."); // 미적용 안내
        } // 작업 해상도 순환 변경 마무리

        private void SelectPreviousScreenMode() // 이전 화면 모드 선택
        { // 화면 모드 인덱스 감소 처리
            ChangeScreenMode(-1); // 이전 화면 모드 적용
        } // 이전 화면 모드 선택 마무리

        private void SelectNextScreenMode() // 다음 화면 모드 선택
        { // 화면 모드 인덱스 증가 처리
            ChangeScreenMode(1); // 다음 화면 모드 적용
        } // 다음 화면 모드 선택 마무리

        private void ChangeScreenMode(int offset) // 작업 화면 모드 순환 변경
        { // 작업 복사본 화면 모드 갱신
            if (workingCopy == null) return; // 설정 데이터 없음 시 변경 중단
            screenModeIndex = WrapIndex(screenModeIndex + offset, ScreenModes.Length); // 화면 모드 순환 인덱스 계산
            workingCopy.FullScreenModeValue = (int)ScreenModes[screenModeIndex]; // 작업 화면 모드 변경
            SetText(screenModeText, FormatScreenMode(ScreenModes[screenModeIndex])); // 화면 모드 문구 갱신
            SetStatus("화면 설정이 변경되었습니다. 적용 전에는 저장되지 않습니다."); // 미적용 안내
        } // 작업 화면 모드 순환 변경 마무리

        private void SelectPreviousFrameRate() // 이전 최대 FPS 선택
        { // FPS 인덱스 감소 처리
            ChangeFrameRate(-1); // 이전 FPS 적용
        } // 이전 최대 FPS 선택 마무리

        private void SelectNextFrameRate() // 다음 최대 FPS 선택
        { // FPS 인덱스 증가 처리
            ChangeFrameRate(1); // 다음 FPS 적용
        } // 다음 최대 FPS 선택 마무리

        private void ChangeFrameRate(int offset) // 작업 최대 FPS 순환 변경
        { // 작업 복사본 목표 프레임 갱신
            if (workingCopy == null) return; // 설정 데이터 없음 시 변경 중단
            frameRateIndex = WrapIndex(frameRateIndex + offset, FrameRates.Length); // FPS 순환 인덱스 계산
            workingCopy.TargetFrameRate = FrameRates[frameRateIndex]; // 작업 목표 FPS 변경
            SetText(frameRateText, FormatFrameRate(FrameRates[frameRateIndex])); // FPS 문구 갱신
            SetStatus("화면 설정이 변경되었습니다. 적용 전에는 저장되지 않습니다."); // 미적용 안내
        } // 작업 최대 FPS 순환 변경 마무리

        private void HandleVSyncChanged(bool value) // VSync 변경 처리
        { // 작업 복사본 VSync 갱신
            if (ignoreCallbacks || workingCopy == null) return; // UI 동기화 중 변경 무시
            workingCopy.VSyncCount = value ? 1 : 0; // VSync 작업값 변경
            SetStatus("화면 설정이 변경되었습니다. 적용 전에는 저장되지 않습니다."); // 미적용 안내
        } // VSync 변경 처리 마무리

        private void HandleBrightnessChanged(float value) // 화면 밝기 변경 처리
        { // 작업 복사본 밝기와 표시 문구 갱신
            if (ignoreCallbacks || workingCopy == null) return; // UI 동기화 중 변경 무시
            workingCopy.Brightness = Mathf.Clamp(value, 0.5f, 1.5f); // 화면 밝기 작업값 변경
            SetText(brightnessText, FormatBrightness(workingCopy.Brightness)); // 화면 밝기 문구 갱신
            SetStatus("밝기를 변경했습니다. 적용 전에는 실제 화면이 바뀌지 않습니다."); // 작업 복사본 원칙 안내
        } // 화면 밝기 변경 처리 마무리

        private void HandleMasterChanged(float value) // 마스터 음량 변경 처리
        { // 작업 복사본 마스터 음량 갱신
            if (ignoreCallbacks || workingCopy == null) return; // UI 동기화 중 변경 무시
            workingCopy.MasterVolume = Mathf.Clamp01(value); // 마스터 음량 작업값 변경
            SetText(masterVolumeText, FormatPercent(workingCopy.MasterVolume)); // 마스터 음량 문구 갱신
        } // 마스터 음량 변경 처리 마무리

        private void HandleMusicChanged(float value) // BGM 음량 변경 처리
        { // 작업 복사본 BGM 음량 갱신
            if (ignoreCallbacks || workingCopy == null) return; // UI 동기화 중 변경 무시
            workingCopy.MusicVolume = Mathf.Clamp01(value); // BGM 음량 작업값 변경
            SetText(musicVolumeText, FormatPercent(workingCopy.MusicVolume)); // BGM 음량 문구 갱신
        } // BGM 음량 변경 처리 마무리

        private void HandleSfxChanged(float value) // SFX 음량 변경 처리
        { // 작업 복사본 SFX 음량 갱신
            if (ignoreCallbacks || workingCopy == null) return; // UI 동기화 중 변경 무시
            workingCopy.SfxVolume = Mathf.Clamp01(value); // SFX 음량 작업값 변경
            SetText(sfxVolumeText, FormatPercent(workingCopy.SfxVolume)); // SFX 음량 문구 갱신
        } // SFX 음량 변경 처리 마무리

        private void HandleUiVolumeChanged(float value) // UI 효과음 음량 변경 처리
        { // 작업 복사본 UI 음량 갱신
            if (ignoreCallbacks || workingCopy == null) return; // UI 동기화 중 변경 무시
            workingCopy.UiVolume = Mathf.Clamp01(value); // UI 음량 작업값 변경
            SetText(uiVolumeText, FormatPercent(workingCopy.UiVolume)); // UI 음량 문구 갱신
        } // UI 효과음 음량 변경 처리 마무리

        private void HandleMuteChanged(bool value) // 전체 음소거 변경 처리
        { // 작업 복사본 음소거 갱신
            if (ignoreCallbacks || workingCopy == null) return; // UI 동기화 중 변경 무시
            workingCopy.IsMuted = value; // 전체 음소거 작업값 변경
        } // 전체 음소거 변경 처리 마무리

        private void HandleMouseSensitivityChanged(float value) // 마우스 감도 변경 처리
        { // 작업 복사본 마우스 감도 갱신
            if (ignoreCallbacks || workingCopy == null) return; // UI 동기화 중 변경 무시
            workingCopy.MouseSensitivity = Mathf.Clamp(value, 0.01f, 2f); // 마우스 감도 작업값 변경
            SetText(mouseSensitivityText, workingCopy.MouseSensitivity.ToString("0.00")); // 마우스 감도 문구 갱신
        } // 마우스 감도 변경 처리 마무리

        private void HandleGamepadSensitivityChanged(float value) // 게임패드 시점 속도 변경 처리
        { // 작업 복사본 게임패드 감도 갱신
            if (ignoreCallbacks || workingCopy == null) return; // UI 동기화 중 변경 무시
            workingCopy.GamepadLookDegreesPerSecond = Mathf.Clamp(value, 30f, 720f); // 게임패드 감도 작업값 변경
            SetText(gamepadSensitivityText, $"{Mathf.RoundToInt(workingCopy.GamepadLookDegreesPerSecond)}°/s"); // 게임패드 감도 문구 갱신
        } // 게임패드 시점 속도 변경 처리 마무리

        private void HandleInvertChanged(bool value) // Y축 반전 변경 처리
        { // 작업 복사본 Y축 반전 갱신
            if (ignoreCallbacks || workingCopy == null) return; // UI 동기화 중 변경 무시
            workingCopy.InvertLookY = value; // Y축 반전 작업값 변경
        } // Y축 반전 변경 처리 마무리

        private void ShowScreenTab() // 화면 탭 선택
        { // 화면 탭 표시 처리
            ShowTab(0); // 첫 번째 탭 활성화
        } // 화면 탭 선택 마무리

        private void ShowSoundTab() // 사운드 탭 선택
        { // 사운드 탭 표시 처리
            ShowTab(1); // 두 번째 탭 활성화
        } // 사운드 탭 선택 마무리

        private void ShowControlsTab() // 조작 탭 선택
        { // 조작 탭 표시 처리
            ShowTab(2); // 세 번째 탭 활성화
        } // 조작 탭 선택 마무리

        private void ShowCameraTab() // 카메라 탭 선택
        { // 카메라 탭 표시 처리
            ShowTab(3); // 네 번째 탭 활성화
        } // 카메라 탭 선택 마무리

        private void ShowTab(int selectedIndex) // 지정한 한 개 탭만 활성화
        { // 탭 내용과 버튼 상호작용 상태 동기화
            selectedTabIndex = Mathf.Clamp(selectedIndex, 0, 3); // 현재 선택 탭 인덱스 저장

            for (int index = 0; index < 4; index++) // 4개 탭 순회
            { // 현재 탭 활성 상태 처리
                if (tabPanels != null && index < tabPanels.Length && tabPanels[index] != null) tabPanels[index].SetActive(index == selectedTabIndex); // 선택 탭 내용만 표시
                if (tabButtons != null && index < tabButtons.Length && tabButtons[index] != null) tabButtons[index].interactable = index != selectedTabIndex; // 선택 탭 버튼 비활성화
            } // 현재 탭 활성 상태 처리 마무리
        } // 지정한 한 개 탭만 활성화 마무리

        private void SetEditingEnabled(bool enabledValue) // 설정 편집 가능 상태 변경
        { // Bootstrap 직접 실행과 키 입력 대기 상태에 따른 안전 잠금
            SetButtonInteractable(resolutionPreviousButton, enabledValue); // 이전 해상도 버튼 상태 변경
            SetButtonInteractable(resolutionNextButton, enabledValue); // 다음 해상도 버튼 상태 변경
            SetButtonInteractable(screenModePreviousButton, enabledValue); // 이전 화면 모드 버튼 상태 변경
            SetButtonInteractable(screenModeNextButton, enabledValue); // 다음 화면 모드 버튼 상태 변경
            SetButtonInteractable(frameRatePreviousButton, enabledValue); // 이전 FPS 버튼 상태 변경
            SetButtonInteractable(frameRateNextButton, enabledValue); // 다음 FPS 버튼 상태 변경
            SetSelectableInteractable(vSyncToggle, enabledValue); // VSync 토글 상태 변경
            SetSelectableInteractable(brightnessSlider, enabledValue); // 밝기 Slider 상태 변경
            SetSelectableInteractable(masterVolumeSlider, enabledValue); // 마스터 Slider 상태 변경
            SetSelectableInteractable(musicVolumeSlider, enabledValue); // BGM Slider 상태 변경
            SetSelectableInteractable(sfxVolumeSlider, enabledValue); // SFX Slider 상태 변경
            SetSelectableInteractable(uiVolumeSlider, enabledValue); // UI Slider 상태 변경
            SetSelectableInteractable(muteToggle, enabledValue); // 음소거 Toggle 상태 변경
            SetSelectableInteractable(mouseSensitivitySlider, enabledValue); // 마우스 감도 Slider 상태 변경
            SetSelectableInteractable(gamepadSensitivitySlider, enabledValue); // 게임패드 감도 Slider 상태 변경
            SetSelectableInteractable(invertLookYToggle, enabledValue); // Y축 반전 Toggle 상태 변경
            SetButtonInteractable(applyButton, enabledValue); // 적용 버튼 상태 변경
            SetButtonInteractable(defaultsButton, enabledValue); // 전체 기본값 버튼 상태 변경
            SetButtonInteractable(resetBindingsButton, enabledValue && runtimeRebindGameplayMap != null); // 키 기본값 버튼 상태 변경

            if (rebindButtons != null) // 키 재지정 버튼 배열 존재 여부 확인
            { // 모든 키 재지정 버튼 공통 상태 적용
                for (int index = 0; index < rebindButtons.Length; index++) // 키 변경 버튼 전체 순회
                { // 현재 키 변경 버튼 상태 적용
                    SetButtonInteractable(rebindButtons[index], enabledValue && runtimeRebindGameplayMap != null); // 키 변경 버튼 활성 상태 변경
                } // 현재 키 변경 버튼 상태 적용 마무리
            } // 모든 키 재지정 버튼 공통 상태 적용 마무리
        } // 설정 편집 가능 상태 변경 마무리

        private void SetStatus(string value) // 하단 상태 문구 변경
        { // 상태 TextMeshPro 안전 갱신
            SetText(statusText, value); // 상태 안내 문구 적용
        } // 하단 상태 문구 변경 마무리

        private static int CompareResolution(Vector2Int left, Vector2Int right) // 해상도 정렬 비교
        { // 세로 크기 우선 정렬
            int heightCompare = left.y.CompareTo(right.y); // 세로 해상도 비교
            return heightCompare != 0 ? heightCompare : left.x.CompareTo(right.x); // 세로 후 가로 비교 결과 반환
        } // 해상도 정렬 비교 마무리

        private static int FindScreenModeIndex(int storedValue) // 저장 화면 모드 인덱스 검색
        { // 화면 모드 배열 순회
            for (int index = 0; index < ScreenModes.Length; index++) if ((int)ScreenModes[index] == storedValue) return index; // 일치 화면 모드 인덱스 반환
            return 0; // 기본 전체 화면 창 인덱스 반환
        } // 저장 화면 모드 인덱스 검색 마무리

        private static int FindFrameRateIndex(int storedValue) // 저장 FPS 인덱스 검색
        { // FPS 배열 순회
            for (int index = 0; index < FrameRates.Length; index++) if (FrameRates[index] == storedValue) return index; // 일치 FPS 인덱스 반환
            return storedValue < 0 ? 0 : 2; // 제한 없음 또는 60 FPS 기본 인덱스 반환
        } // 저장 FPS 인덱스 검색 마무리

        private static int WrapIndex(int value, int count) // 선택 목록 순환 인덱스 계산
        { // 음수와 범위 초과 보정
            if (count <= 0) return 0; // 빈 목록 기본 인덱스 반환
            int result = value % count; // 나머지 인덱스 계산
            return result < 0 ? result + count : result; // 음수 보정 인덱스 반환
        } // 선택 목록 순환 인덱스 계산 마무리

        private static string FormatResolution(Vector2Int value) // 해상도 표시 문구 생성
        { // 가로와 세로 픽셀 문구 구성
            return $"{value.x} × {value.y}"; // 해상도 문구 반환
        } // 해상도 표시 문구 생성 마무리

        private static string FormatScreenMode(FullScreenMode value) // 화면 모드 한국어 문구 생성
        { // Unity 화면 모드별 사용자 문구 선택
            if (value == FullScreenMode.Windowed) return "창 모드"; // 창 모드 문구 반환
            if (value == FullScreenMode.ExclusiveFullScreen) return "독점 전체 화면"; // 독점 전체 화면 문구 반환
            return "전체 화면 창"; // 기본 전체 화면 창 문구 반환
        } // 화면 모드 한국어 문구 생성 마무리

        private static string FormatFrameRate(int value) // 최대 FPS 표시 문구 생성
        { // 제한 없음과 숫자 FPS 구분
            return value < 0 ? "제한 없음" : $"{value} FPS"; // 최대 FPS 문구 반환
        } // 최대 FPS 표시 문구 생성 마무리

        private static string FormatPercent(float value) // 0에서 1 값을 백분율 문구로 변환
        { // 오디오 표시값 계산
            return $"{Mathf.RoundToInt(Mathf.Clamp01(value) * 100f)}%"; // 안전한 백분율 문구 반환
        } // 0에서 1 값을 백분율 문구로 변환 마무리

        private static string FormatBrightness(float value) // 화면 밝기 배율을 백분율 문구로 변환
        { // 0.5에서 1.5 범위 밝기 사용자 표시
            return $"{Mathf.RoundToInt(Mathf.Clamp(value, 0.5f, 1.5f) * 100f)}%"; // 밝기 백분율 문구 반환
        } // 화면 밝기 배율을 백분율 문구로 변환 마무리

        private static void SetSliderRange(Slider slider, float minimum, float maximum, bool wholeNumbers) // Slider 범위 안전 설정
        { // Slider 참조 존재 시 범위 적용
            if (slider == null) return; // Slider 없음 시 처리 종료
            slider.minValue = minimum; // Slider 최소값 적용
            slider.maxValue = maximum; // Slider 최대값 적용
            slider.wholeNumbers = wholeNumbers; // 정수 단위 사용 여부 적용
        } // Slider 범위 안전 설정 마무리

        private static void SetSliderValue(Slider slider, float value) // Slider 값 이벤트 없이 안전 적용
        { // Slider 참조 존재 시 현재 작업값 표시
            if (slider != null) slider.SetValueWithoutNotify(value); // 값 변경 Callback 없이 Slider 값 적용
        } // Slider 값 이벤트 없이 안전 적용 마무리

        private static void SetToggleValue(Toggle toggle, bool value) // Toggle 값 이벤트 없이 안전 적용
        { // Toggle 참조 존재 시 현재 작업값 표시
            if (toggle != null) toggle.SetIsOnWithoutNotify(value); // 값 변경 Callback 없이 Toggle 값 적용
        } // Toggle 값 이벤트 없이 안전 적용 마무리

        private static void SetText(TMP_Text text, string value) // TextMeshPro 문구 안전 적용
        { // TextMeshPro 참조 누락을 허용하는 공통 표시 함수
            if (text != null) text.text = value; // 전달 문구 적용
        } // TextMeshPro 문구 안전 적용 마무리

        private static void SetTextAt(TMP_Text[] texts, int index, string value) // TextMeshPro 배열의 특정 문구 안전 적용
        { // 키 재지정 행 배열 인덱스 방어
            if (texts == null || index < 0 || index >= texts.Length) return; // 배열 범위 오류 시 처리 종료
            SetText(texts[index], value); // 지정 인덱스 문구 적용
        } // TextMeshPro 배열의 특정 문구 안전 적용 마무리

        private static void SetButtonInteractable(Button button, bool value) // Button 상호작용 상태 안전 적용
        { // Button 참조 누락을 허용하는 공통 상태 함수
            if (button != null) button.interactable = value; // 전달 상호작용 상태 적용
        } // Button 상호작용 상태 안전 적용 마무리

        private static void SetSelectableInteractable(Selectable selectable, bool value) // Slider와 Toggle 상호작용 상태 안전 적용
        { // Selectable 참조 누락을 허용하는 공통 상태 함수
            if (selectable != null) selectable.interactable = value; // 전달 상호작용 상태 적용
        } // Slider와 Toggle 상호작용 상태 안전 적용 마무리

        private readonly struct RebindSpec // 조작 탭 한 행의 InputAction 대상 사양 선언
        { // 표시 이름과 액션 이름과 복합 파트 이름 저장
            public RebindSpec(string displayName, string actionName, string compositePartName) // 재지정 대상 한 행 생성
            { // 재지정 대상 식별값 저장
                DisplayName = displayName; // 한국어 조작 이름 저장
                ActionName = actionName; // InputAction 이름 저장
                CompositePartName = compositePartName; // Move 복합 바인딩 파트 이름 저장
            } // 재지정 대상 한 행 생성 마무리

            public string DisplayName { get; } // 한국어 조작 이름 반환
            public string ActionName { get; } // InputAction 이름 반환
            public string CompositePartName { get; } // 복합 바인딩 파트 이름 반환
        } // 조작 탭 한 행의 InputAction 대상 사양 마무리
    } // 설정 메뉴 4개 탭과 작업 복사본과 키 재지정 관리 마무리
} // 프로젝트 UI 네임스페이스 마무리
