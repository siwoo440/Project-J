using System.Collections.Generic; // 해상도 선택 목록 기능 참조
using ProjectJ.Core.Services; // 설정 관리자와 사용자 설정 데이터 참조
using TMPro; // TextMeshPro UI 기능 참조
using UnityEngine; // Unity 화면과 기본 기능 참조
using UnityEngine.UI; // Button과 Slider와 Toggle 기능 참조

namespace ProjectJ.UI // 프로젝트 UI 네임스페이스 선언
{ // 설정 메뉴 UI 기능 정의
    [DisallowMultipleComponent] // 동일 오브젝트의 설정 메뉴 중복 방지
    public sealed class SettingsMenuController : MonoBehaviour // 설정 메뉴 4개 탭과 작업 복사본 관리
    { // 설정 메뉴 런타임 동작 정의
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

        [Header("Sound")] // 사운드 탭 참조 구역
        [SerializeField] private Slider masterVolumeSlider; // 마스터 음량 슬라이더
        [SerializeField] private TMP_Text masterVolumeText; // 마스터 음량 표시 문구
        [SerializeField] private Slider musicVolumeSlider; // BGM 음량 슬라이더
        [SerializeField] private TMP_Text musicVolumeText; // BGM 음량 표시 문구
        [SerializeField] private Slider sfxVolumeSlider; // SFX 음량 슬라이더
        [SerializeField] private TMP_Text sfxVolumeText; // SFX 음량 표시 문구
        [SerializeField] private Toggle muteToggle; // 전체 음소거 토글

        [Header("Controls")] // 조작 탭 참조 구역
        [SerializeField] private TMP_Text controlsInfoText; // 키 재지정 상태 안내 문구

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
        { // 화면 모드 선택값 정의
            FullScreenMode.FullScreenWindow, // 전체 화면 창
            FullScreenMode.Windowed, // 창 모드
            FullScreenMode.ExclusiveFullScreen, // 독점 전체 화면
        }; // 화면 모드 선택값 정의 완료

        private static readonly int[] FrameRates = // 최대 FPS 선택 목록
        { // 최대 FPS 선택값 정의
            -1, 30, 60, 90, 120, 144, 165, 240, 360, // 제한 없음과 일반 모니터 FPS
        }; // 최대 FPS 선택값 정의 완료

        private readonly List<Vector2Int> resolutions = new List<Vector2Int>(); // 현재 장치 해상도 목록
        private ProjectUserSettings workingCopy; // 화면에서 수정할 설정 작업 복사본
        private int resolutionIndex; // 현재 해상도 선택 인덱스
        private int screenModeIndex; // 현재 화면 모드 선택 인덱스
        private int frameRateIndex; // 현재 최대 FPS 선택 인덱스
        private bool ignoreCallbacks; // UI 값 동기화 중 이벤트 무시 여부

        private void Awake() // UI 범위와 이벤트 연결 초기화
        { // 설정 메뉴 초기 상태 준비
            ConfigureSliders(); // 슬라이더 범위 설정
            RegisterListeners(); // 버튼과 값 변경 이벤트 연결
            ShowMainMenu(); // 시작 시 임시 메인 메뉴 표시
        } // UI 범위와 이벤트 연결 초기화 완료

        private void OnDestroy() // UI 이벤트 연결 정리
        { // 중복 이벤트 방지 처리
            UnregisterListeners(); // 버튼과 값 변경 이벤트 해제
        } // UI 이벤트 연결 정리 완료

        public void ConfigureForEditor(GameObject configuredMainMenuRoot, Button configuredOpenButton, GameObject configuredSettingsPanel, TMP_Text configuredStatusText, Button[] configuredTabButtons, GameObject[] configuredTabPanels, TMP_Text configuredResolutionText, Button configuredResolutionPreviousButton, Button configuredResolutionNextButton, TMP_Text configuredScreenModeText, Button configuredScreenModePreviousButton, Button configuredScreenModeNextButton, TMP_Text configuredFrameRateText, Button configuredFrameRatePreviousButton, Button configuredFrameRateNextButton, Toggle configuredVSyncToggle, Slider configuredMasterVolumeSlider, TMP_Text configuredMasterVolumeText, Slider configuredMusicVolumeSlider, TMP_Text configuredMusicVolumeText, Slider configuredSfxVolumeSlider, TMP_Text configuredSfxVolumeText, Toggle configuredMuteToggle, TMP_Text configuredControlsInfoText, Slider configuredMouseSensitivitySlider, TMP_Text configuredMouseSensitivityText, Slider configuredGamepadSensitivitySlider, TMP_Text configuredGamepadSensitivityText, Toggle configuredInvertLookYToggle, Button configuredDefaultsButton, Button configuredCancelButton, Button configuredApplyButton) // Editor 자동 생성 UI 참조 일괄 연결
        { // 자동 생성된 UI 직렬화 참조 저장
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
        } // 자동 생성된 UI 직렬화 참조 저장 완료

        private void ConfigureSliders() // 설정 데이터 검증 범위와 Slider 범위 통일
        { // Slider 범위 안전 설정
            SetSliderRange(masterVolumeSlider, 0f, 1f, false); // 마스터 음량 범위 적용
            SetSliderRange(musicVolumeSlider, 0f, 1f, false); // BGM 음량 범위 적용
            SetSliderRange(sfxVolumeSlider, 0f, 1f, false); // SFX 음량 범위 적용
            SetSliderRange(mouseSensitivitySlider, 0.01f, 2f, false); // 마우스 감도 범위 적용
            SetSliderRange(gamepadSensitivitySlider, 30f, 720f, true); // 게임패드 시점 속도 범위 적용
        } // 설정 데이터 검증 범위와 Slider 범위 통일 완료

        private void RegisterListeners() // 설정 UI 이벤트 연결
        { // 버튼과 Slider와 Toggle 동작 연결
            openSettingsButton?.onClick.AddListener(OpenSettings); // 설정 열기 연결
            resolutionPreviousButton?.onClick.AddListener(SelectPreviousResolution); // 이전 해상도 연결
            resolutionNextButton?.onClick.AddListener(SelectNextResolution); // 다음 해상도 연결
            screenModePreviousButton?.onClick.AddListener(SelectPreviousScreenMode); // 이전 화면 모드 연결
            screenModeNextButton?.onClick.AddListener(SelectNextScreenMode); // 다음 화면 모드 연결
            frameRatePreviousButton?.onClick.AddListener(SelectPreviousFrameRate); // 이전 FPS 연결
            frameRateNextButton?.onClick.AddListener(SelectNextFrameRate); // 다음 FPS 연결
            vSyncToggle?.onValueChanged.AddListener(HandleVSyncChanged); // VSync 변경 연결
            masterVolumeSlider?.onValueChanged.AddListener(HandleMasterChanged); // 마스터 음량 변경 연결
            musicVolumeSlider?.onValueChanged.AddListener(HandleMusicChanged); // BGM 음량 변경 연결
            sfxVolumeSlider?.onValueChanged.AddListener(HandleSfxChanged); // SFX 음량 변경 연결
            muteToggle?.onValueChanged.AddListener(HandleMuteChanged); // 음소거 변경 연결
            mouseSensitivitySlider?.onValueChanged.AddListener(HandleMouseSensitivityChanged); // 마우스 감도 변경 연결
            gamepadSensitivitySlider?.onValueChanged.AddListener(HandleGamepadSensitivityChanged); // 게임패드 감도 변경 연결
            invertLookYToggle?.onValueChanged.AddListener(HandleInvertChanged); // Y축 반전 변경 연결
            defaultsButton?.onClick.AddListener(PreviewDefaults); // 기본값 버튼 연결
            cancelButton?.onClick.AddListener(CancelChanges); // 취소 버튼 연결
            applyButton?.onClick.AddListener(ApplyChanges); // 적용 버튼 연결

            if (tabButtons != null) // 탭 버튼 배열 존재 여부 확인
            { // 4개 탭 버튼 이벤트 연결
                if (tabButtons.Length > 0 && tabButtons[0] != null) tabButtons[0].onClick.AddListener(ShowScreenTab); // 화면 탭 연결
                if (tabButtons.Length > 1 && tabButtons[1] != null) tabButtons[1].onClick.AddListener(ShowSoundTab); // 사운드 탭 연결
                if (tabButtons.Length > 2 && tabButtons[2] != null) tabButtons[2].onClick.AddListener(ShowControlsTab); // 조작 탭 연결
                if (tabButtons.Length > 3 && tabButtons[3] != null) tabButtons[3].onClick.AddListener(ShowCameraTab); // 카메라 탭 연결
            } // 4개 탭 버튼 이벤트 연결 완료
        } // 설정 UI 이벤트 연결 완료

        private void UnregisterListeners() // 설정 UI 이벤트 해제
        { // 버튼과 Slider와 Toggle 동작 연결 제거
            openSettingsButton?.onClick.RemoveListener(OpenSettings); // 설정 열기 해제
            resolutionPreviousButton?.onClick.RemoveListener(SelectPreviousResolution); // 이전 해상도 해제
            resolutionNextButton?.onClick.RemoveListener(SelectNextResolution); // 다음 해상도 해제
            screenModePreviousButton?.onClick.RemoveListener(SelectPreviousScreenMode); // 이전 화면 모드 해제
            screenModeNextButton?.onClick.RemoveListener(SelectNextScreenMode); // 다음 화면 모드 해제
            frameRatePreviousButton?.onClick.RemoveListener(SelectPreviousFrameRate); // 이전 FPS 해제
            frameRateNextButton?.onClick.RemoveListener(SelectNextFrameRate); // 다음 FPS 해제
            vSyncToggle?.onValueChanged.RemoveListener(HandleVSyncChanged); // VSync 변경 해제
            masterVolumeSlider?.onValueChanged.RemoveListener(HandleMasterChanged); // 마스터 음량 변경 해제
            musicVolumeSlider?.onValueChanged.RemoveListener(HandleMusicChanged); // BGM 음량 변경 해제
            sfxVolumeSlider?.onValueChanged.RemoveListener(HandleSfxChanged); // SFX 음량 변경 해제
            muteToggle?.onValueChanged.RemoveListener(HandleMuteChanged); // 음소거 변경 해제
            mouseSensitivitySlider?.onValueChanged.RemoveListener(HandleMouseSensitivityChanged); // 마우스 감도 변경 해제
            gamepadSensitivitySlider?.onValueChanged.RemoveListener(HandleGamepadSensitivityChanged); // 게임패드 감도 변경 해제
            invertLookYToggle?.onValueChanged.RemoveListener(HandleInvertChanged); // Y축 반전 변경 해제
            defaultsButton?.onClick.RemoveListener(PreviewDefaults); // 기본값 버튼 해제
            cancelButton?.onClick.RemoveListener(CancelChanges); // 취소 버튼 해제
            applyButton?.onClick.RemoveListener(ApplyChanges); // 적용 버튼 해제

            if (tabButtons != null) // 탭 버튼 배열 존재 여부 확인
            { // 4개 탭 버튼 이벤트 해제
                if (tabButtons.Length > 0 && tabButtons[0] != null) tabButtons[0].onClick.RemoveListener(ShowScreenTab); // 화면 탭 해제
                if (tabButtons.Length > 1 && tabButtons[1] != null) tabButtons[1].onClick.RemoveListener(ShowSoundTab); // 사운드 탭 해제
                if (tabButtons.Length > 2 && tabButtons[2] != null) tabButtons[2].onClick.RemoveListener(ShowControlsTab); // 조작 탭 해제
                if (tabButtons.Length > 3 && tabButtons[3] != null) tabButtons[3].onClick.RemoveListener(ShowCameraTab); // 카메라 탭 해제
            } // 4개 탭 버튼 이벤트 해제 완료
        } // 설정 UI 이벤트 해제 완료

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
            } // Bootstrap을 거치지 않은 MainMenu 직접 실행 처리 완료

            SetEditingEnabled(true); // 설정 편집 활성화
            BuildResolutionList(); // 현재 장치 해상도 목록 생성
            SyncIndexes(); // 작업 설정과 순환 선택 인덱스 동기화
            RefreshUi(); // 전체 작업 설정 UI 표시
            SetStatus("값을 변경해도 적용 전에는 저장되지 않습니다."); // 작업 복사본 안내
        } // 현재 설정 작업 복사본으로 설정 화면 열기 완료

        private void ShowMainMenu() // 설정 화면 닫기와 작업 복사본 폐기
        { // MainMenu 복귀 처리
            workingCopy = null; // 작업 복사본 폐기
            settingsPanel?.SetActive(false); // 설정 화면 숨김
            mainMenuRoot?.SetActive(true); // 임시 메인 메뉴 표시
        } // 설정 화면 닫기와 작업 복사본 폐기 완료

        private void PreviewDefaults() // 기본값을 작업 화면에만 불러오기
        { // 실제 저장 없는 기본값 미리보기
            workingCopy = SettingsManager.CreateDefaultWorkingCopy(); // 기본 작업 설정 생성
            BuildResolutionList(); // 기본 해상도를 포함한 목록 재생성
            SyncIndexes(); // 기본 설정 선택 인덱스 계산
            RefreshUi(); // 기본값 UI 표시
            SetStatus("기본값 미리보기입니다. 적용 전에는 저장되지 않습니다."); // 기본값 미리보기 안내
        } // 기본값을 작업 화면에만 불러오기 완료

        private void CancelChanges() // 미적용 변경 취소
        { // 작업 복사본 폐기 처리
            ShowMainMenu(); // 임시 메인 메뉴 복귀
        } // 미적용 변경 취소 완료

        private void ApplyChanges() // 작업 설정 실제 적용과 저장
        { // SettingsManager.Apply 연결
            if (workingCopy == null || !SettingsManager.IsReady) // 적용 가능한 상태 확인
            { // 설정 서비스 미준비 처리
                SetStatus("적용할 설정 데이터가 준비되지 않았습니다."); // 적용 실패 안내
                return; // 적용 중단
            } // 설정 서비스 미준비 처리 완료

            bool saved = SettingsManager.Apply(workingCopy); // 작업 설정 전체 적용과 JSON 저장 실행
            SetStatus(saved ? "설정을 적용하고 저장했습니다." : "설정 저장에 실패했습니다. Console을 확인합니다."); // 적용 결과 안내

            if (saved) // 저장 성공 여부 확인
            { // 적용 결과 기준 UI 재동기화
                workingCopy = SettingsManager.CreateWorkingCopy(); // 실제 설정 새 작업 복사본 생성
                BuildResolutionList(); // 적용 후 해상도 목록 갱신
                SyncIndexes(); // 적용 후 인덱스 동기화
                RefreshUi(); // 실제 적용값 UI 표시
            } // 적용 결과 기준 UI 재동기화 완료
        } // 작업 설정 실제 적용과 저장 완료

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
                } // 선택 목록 제외 처리 완료

                resolutions.Add(size); // 새 화면 크기 선택 목록 추가
            } // 현재 해상도 크기 처리 완료

            Vector2Int current = new Vector2Int(Mathf.Max(640, workingCopy.ResolutionWidth), Mathf.Max(360, workingCopy.ResolutionHeight)); // 현재 작업 해상도 생성

            if (!resolutions.Contains(current)) // 현재 작업 해상도 목록 포함 여부 확인
            { // 저장 해상도 보강 처리
                resolutions.Add(current); // 현재 작업 해상도 목록 추가
            } // 저장 해상도 보강 처리 완료

            resolutions.Sort(CompareResolution); // 작은 해상도부터 정렬
        } // 장치 지원 해상도에서 중복 크기 제거 완료

        private void SyncIndexes() // 작업 설정의 현재 선택 인덱스 계산
        { // 해상도·화면 모드·FPS 인덱스 동기화
            resolutionIndex = Mathf.Max(0, resolutions.IndexOf(new Vector2Int(workingCopy.ResolutionWidth, workingCopy.ResolutionHeight))); // 저장 해상도 인덱스 계산
            screenModeIndex = FindScreenModeIndex(workingCopy.FullScreenModeValue); // 저장 화면 모드 인덱스 계산
            frameRateIndex = FindFrameRateIndex(workingCopy.TargetFrameRate); // 저장 FPS 인덱스 계산
        } // 작업 설정의 현재 선택 인덱스 계산 완료

        private void RefreshUi() // 전체 작업 설정값 UI 표시
        { // UI 이벤트 없이 현재 작업값 동기화
            if (workingCopy == null) // 작업 설정 존재 여부 확인
            { // 작업 설정 없음 처리
                return; // UI 갱신 중단
            } // 작업 설정 없음 처리 완료

            ignoreCallbacks = true; // Slider와 Toggle 콜백 일시 중지
            resolutionText.text = resolutions.Count > 0 ? FormatResolution(resolutions[resolutionIndex]) : "-"; // 해상도 문구 적용
            screenModeText.text = FormatScreenMode(ScreenModes[screenModeIndex]); // 화면 모드 문구 적용
            frameRateText.text = FormatFrameRate(FrameRates[frameRateIndex]); // 최대 FPS 문구 적용
            vSyncToggle.SetIsOnWithoutNotify(workingCopy.VSyncCount > 0); // VSync 상태 적용
            masterVolumeSlider.SetValueWithoutNotify(workingCopy.MasterVolume); // 마스터 음량 적용
            masterVolumeText.text = FormatPercent(workingCopy.MasterVolume); // 마스터 음량 문구 적용
            musicVolumeSlider.SetValueWithoutNotify(workingCopy.MusicVolume); // BGM 음량 적용
            musicVolumeText.text = FormatPercent(workingCopy.MusicVolume); // BGM 음량 문구 적용
            sfxVolumeSlider.SetValueWithoutNotify(workingCopy.SfxVolume); // SFX 음량 적용
            sfxVolumeText.text = FormatPercent(workingCopy.SfxVolume); // SFX 음량 문구 적용
            muteToggle.SetIsOnWithoutNotify(workingCopy.IsMuted); // 음소거 상태 적용
            mouseSensitivitySlider.SetValueWithoutNotify(workingCopy.MouseSensitivity); // 마우스 감도 적용
            mouseSensitivityText.text = workingCopy.MouseSensitivity.ToString("0.00"); // 마우스 감도 문구 적용
            gamepadSensitivitySlider.SetValueWithoutNotify(workingCopy.GamepadLookDegreesPerSecond); // 게임패드 시점 속도 적용
            gamepadSensitivityText.text = $"{Mathf.RoundToInt(workingCopy.GamepadLookDegreesPerSecond)}°/s"; // 게임패드 시점 속도 문구 적용
            invertLookYToggle.SetIsOnWithoutNotify(workingCopy.InvertLookY); // Y축 반전 상태 적용
            controlsInfoText.text = string.IsNullOrWhiteSpace(workingCopy.InputBindingOverridesJson) ? "현재 상태 : 기본 키 사용 중\n\n기본 키 재지정 UI는 53일차에서 연결합니다." : "현재 상태 : 저장된 키 재지정 있음\n\n기본 키 재지정 UI는 53일차에서 연결합니다."; // 조작 탭 현재 상태 표시
            ignoreCallbacks = false; // Slider와 Toggle 콜백 재개
        } // 전체 작업 설정값 UI 표시 완료

        private void SelectPreviousResolution() // 이전 해상도 선택
        { // 해상도 인덱스 감소 처리
            ChangeResolution(-1); // 이전 해상도 적용
        } // 이전 해상도 선택 완료

        private void SelectNextResolution() // 다음 해상도 선택
        { // 해상도 인덱스 증가 처리
            ChangeResolution(1); // 다음 해상도 적용
        } // 다음 해상도 선택 완료

        private void ChangeResolution(int offset) // 작업 해상도 순환 변경
        { // 작업 복사본 화면 크기 갱신
            if (workingCopy == null || resolutions.Count == 0) return; // 설정 데이터 없음 시 변경 중단
            resolutionIndex = WrapIndex(resolutionIndex + offset, resolutions.Count); // 해상도 순환 인덱스 계산
            workingCopy.ResolutionWidth = resolutions[resolutionIndex].x; // 작업 가로 해상도 변경
            workingCopy.ResolutionHeight = resolutions[resolutionIndex].y; // 작업 세로 해상도 변경
            resolutionText.text = FormatResolution(resolutions[resolutionIndex]); // 해상도 문구 갱신
            SetStatus("화면 설정이 변경되었습니다. 적용 전에는 저장되지 않습니다."); // 미적용 안내
        } // 작업 해상도 순환 변경 완료

        private void SelectPreviousScreenMode() // 이전 화면 모드 선택
        { // 화면 모드 인덱스 감소 처리
            ChangeScreenMode(-1); // 이전 화면 모드 적용
        } // 이전 화면 모드 선택 완료

        private void SelectNextScreenMode() // 다음 화면 모드 선택
        { // 화면 모드 인덱스 증가 처리
            ChangeScreenMode(1); // 다음 화면 모드 적용
        } // 다음 화면 모드 선택 완료

        private void ChangeScreenMode(int offset) // 작업 화면 모드 순환 변경
        { // 작업 복사본 화면 모드 갱신
            if (workingCopy == null) return; // 설정 데이터 없음 시 변경 중단
            screenModeIndex = WrapIndex(screenModeIndex + offset, ScreenModes.Length); // 화면 모드 순환 인덱스 계산
            workingCopy.FullScreenModeValue = (int)ScreenModes[screenModeIndex]; // 작업 화면 모드 변경
            screenModeText.text = FormatScreenMode(ScreenModes[screenModeIndex]); // 화면 모드 문구 갱신
            SetStatus("화면 설정이 변경되었습니다. 적용 전에는 저장되지 않습니다."); // 미적용 안내
        } // 작업 화면 모드 순환 변경 완료

        private void SelectPreviousFrameRate() // 이전 최대 FPS 선택
        { // FPS 인덱스 감소 처리
            ChangeFrameRate(-1); // 이전 FPS 적용
        } // 이전 최대 FPS 선택 완료

        private void SelectNextFrameRate() // 다음 최대 FPS 선택
        { // FPS 인덱스 증가 처리
            ChangeFrameRate(1); // 다음 FPS 적용
        } // 다음 최대 FPS 선택 완료

        private void ChangeFrameRate(int offset) // 작업 최대 FPS 순환 변경
        { // 작업 복사본 목표 프레임 갱신
            if (workingCopy == null) return; // 설정 데이터 없음 시 변경 중단
            frameRateIndex = WrapIndex(frameRateIndex + offset, FrameRates.Length); // FPS 순환 인덱스 계산
            workingCopy.TargetFrameRate = FrameRates[frameRateIndex]; // 작업 목표 FPS 변경
            frameRateText.text = FormatFrameRate(FrameRates[frameRateIndex]); // FPS 문구 갱신
            SetStatus("화면 설정이 변경되었습니다. 적용 전에는 저장되지 않습니다."); // 미적용 안내
        } // 작업 최대 FPS 순환 변경 완료

        private void HandleVSyncChanged(bool value) // VSync 변경 처리
        { // 작업 복사본 VSync 갱신
            if (ignoreCallbacks || workingCopy == null) return; // UI 동기화 중 변경 무시
            workingCopy.VSyncCount = value ? 1 : 0; // VSync 작업값 변경
            SetStatus("화면 설정이 변경되었습니다. 적용 전에는 저장되지 않습니다."); // 미적용 안내
        } // VSync 변경 처리 완료

        private void HandleMasterChanged(float value) // 마스터 음량 변경 처리
        { // 작업 복사본 마스터 음량 갱신
            if (ignoreCallbacks || workingCopy == null) return; // UI 동기화 중 변경 무시
            workingCopy.MasterVolume = Mathf.Clamp01(value); // 마스터 음량 작업값 변경
            masterVolumeText.text = FormatPercent(workingCopy.MasterVolume); // 마스터 음량 문구 갱신
        } // 마스터 음량 변경 처리 완료

        private void HandleMusicChanged(float value) // BGM 음량 변경 처리
        { // 작업 복사본 BGM 음량 갱신
            if (ignoreCallbacks || workingCopy == null) return; // UI 동기화 중 변경 무시
            workingCopy.MusicVolume = Mathf.Clamp01(value); // BGM 음량 작업값 변경
            musicVolumeText.text = FormatPercent(workingCopy.MusicVolume); // BGM 음량 문구 갱신
        } // BGM 음량 변경 처리 완료

        private void HandleSfxChanged(float value) // SFX 음량 변경 처리
        { // 작업 복사본 SFX 음량 갱신
            if (ignoreCallbacks || workingCopy == null) return; // UI 동기화 중 변경 무시
            workingCopy.SfxVolume = Mathf.Clamp01(value); // SFX 음량 작업값 변경
            sfxVolumeText.text = FormatPercent(workingCopy.SfxVolume); // SFX 음량 문구 갱신
        } // SFX 음량 변경 처리 완료

        private void HandleMuteChanged(bool value) // 전체 음소거 변경 처리
        { // 작업 복사본 음소거 갱신
            if (ignoreCallbacks || workingCopy == null) return; // UI 동기화 중 변경 무시
            workingCopy.IsMuted = value; // 전체 음소거 작업값 변경
        } // 전체 음소거 변경 처리 완료

        private void HandleMouseSensitivityChanged(float value) // 마우스 감도 변경 처리
        { // 작업 복사본 마우스 감도 갱신
            if (ignoreCallbacks || workingCopy == null) return; // UI 동기화 중 변경 무시
            workingCopy.MouseSensitivity = Mathf.Clamp(value, 0.01f, 2f); // 마우스 감도 작업값 변경
            mouseSensitivityText.text = workingCopy.MouseSensitivity.ToString("0.00"); // 마우스 감도 문구 갱신
        } // 마우스 감도 변경 처리 완료

        private void HandleGamepadSensitivityChanged(float value) // 게임패드 시점 속도 변경 처리
        { // 작업 복사본 게임패드 감도 갱신
            if (ignoreCallbacks || workingCopy == null) return; // UI 동기화 중 변경 무시
            workingCopy.GamepadLookDegreesPerSecond = Mathf.Clamp(value, 30f, 720f); // 게임패드 감도 작업값 변경
            gamepadSensitivityText.text = $"{Mathf.RoundToInt(workingCopy.GamepadLookDegreesPerSecond)}°/s"; // 게임패드 감도 문구 갱신
        } // 게임패드 시점 속도 변경 처리 완료

        private void HandleInvertChanged(bool value) // Y축 반전 변경 처리
        { // 작업 복사본 Y축 반전 갱신
            if (ignoreCallbacks || workingCopy == null) return; // UI 동기화 중 변경 무시
            workingCopy.InvertLookY = value; // Y축 반전 작업값 변경
        } // Y축 반전 변경 처리 완료

        private void ShowScreenTab() // 화면 탭 선택
        { // 화면 탭 표시 처리
            ShowTab(0); // 첫 번째 탭 활성화
        } // 화면 탭 선택 완료

        private void ShowSoundTab() // 사운드 탭 선택
        { // 사운드 탭 표시 처리
            ShowTab(1); // 두 번째 탭 활성화
        } // 사운드 탭 선택 완료

        private void ShowControlsTab() // 조작 탭 선택
        { // 조작 탭 표시 처리
            ShowTab(2); // 세 번째 탭 활성화
        } // 조작 탭 선택 완료

        private void ShowCameraTab() // 카메라 탭 선택
        { // 카메라 탭 표시 처리
            ShowTab(3); // 네 번째 탭 활성화
        } // 카메라 탭 선택 완료

        private void ShowTab(int selectedIndex) // 지정한 한 개 탭만 활성화
        { // 탭 내용과 버튼 상호작용 상태 동기화
            for (int index = 0; index < 4; index++) // 4개 탭 순회
            { // 현재 탭 활성 상태 처리
                if (tabPanels != null && index < tabPanels.Length && tabPanels[index] != null) tabPanels[index].SetActive(index == selectedIndex); // 선택 탭 내용만 표시
                if (tabButtons != null && index < tabButtons.Length && tabButtons[index] != null) tabButtons[index].interactable = index != selectedIndex; // 선택 탭 버튼 비활성화
            } // 현재 탭 활성 상태 처리 완료
        } // 지정한 한 개 탭만 활성화 완료

        private void SetEditingEnabled(bool enabledValue) // 설정 편집 가능 상태 변경
        { // Bootstrap 직접 실행 여부에 따른 안전 잠금
            if (applyButton != null) applyButton.interactable = enabledValue; // 적용 버튼 상태 변경
            if (defaultsButton != null) defaultsButton.interactable = enabledValue; // 기본값 버튼 상태 변경
            if (vSyncToggle != null) vSyncToggle.interactable = enabledValue; // VSync 토글 상태 변경
            if (masterVolumeSlider != null) masterVolumeSlider.interactable = enabledValue; // 마스터 Slider 상태 변경
            if (musicVolumeSlider != null) musicVolumeSlider.interactable = enabledValue; // BGM Slider 상태 변경
            if (sfxVolumeSlider != null) sfxVolumeSlider.interactable = enabledValue; // SFX Slider 상태 변경
            if (muteToggle != null) muteToggle.interactable = enabledValue; // 음소거 Toggle 상태 변경
            if (mouseSensitivitySlider != null) mouseSensitivitySlider.interactable = enabledValue; // 마우스 감도 Slider 상태 변경
            if (gamepadSensitivitySlider != null) gamepadSensitivitySlider.interactable = enabledValue; // 게임패드 감도 Slider 상태 변경
            if (invertLookYToggle != null) invertLookYToggle.interactable = enabledValue; // Y축 반전 Toggle 상태 변경
        } // 설정 편집 가능 상태 변경 완료

        private void SetStatus(string value) // 하단 상태 문구 변경
        { // 상태 TextMeshPro 안전 갱신
            if (statusText != null) statusText.text = value; // 상태 안내 문구 적용
        } // 하단 상태 문구 변경 완료

        private static int CompareResolution(Vector2Int left, Vector2Int right) // 해상도 정렬 비교
        { // 세로 크기 우선 정렬
            int heightCompare = left.y.CompareTo(right.y); // 세로 해상도 비교
            return heightCompare != 0 ? heightCompare : left.x.CompareTo(right.x); // 세로 후 가로 비교 결과 반환
        } // 해상도 정렬 비교 완료

        private static int FindScreenModeIndex(int storedValue) // 저장 화면 모드 인덱스 검색
        { // 화면 모드 배열 순회
            for (int index = 0; index < ScreenModes.Length; index++) if ((int)ScreenModes[index] == storedValue) return index; // 일치 화면 모드 인덱스 반환
            return 0; // 기본 전체 화면 창 인덱스 반환
        } // 저장 화면 모드 인덱스 검색 완료

        private static int FindFrameRateIndex(int storedValue) // 저장 FPS 인덱스 검색
        { // FPS 배열 순회
            for (int index = 0; index < FrameRates.Length; index++) if (FrameRates[index] == storedValue) return index; // 일치 FPS 인덱스 반환
            return storedValue < 0 ? 0 : 2; // 제한 없음 또는 60 FPS 기본 인덱스 반환
        } // 저장 FPS 인덱스 검색 완료

        private static int WrapIndex(int value, int count) // 선택 목록 순환 인덱스 계산
        { // 음수와 범위 초과 보정
            if (count <= 0) return 0; // 빈 목록 기본 인덱스 반환
            int result = value % count; // 나머지 인덱스 계산
            return result < 0 ? result + count : result; // 음수 보정 인덱스 반환
        } // 선택 목록 순환 인덱스 계산 완료

        private static string FormatResolution(Vector2Int value) // 해상도 표시 문구 생성
        { // 가로와 세로 픽셀 문구 구성
            return $"{value.x} × {value.y}"; // 해상도 문구 반환
        } // 해상도 표시 문구 생성 완료

        private static string FormatScreenMode(FullScreenMode value) // 화면 모드 한국어 문구 생성
        { // Unity 화면 모드별 사용자 문구 선택
            if (value == FullScreenMode.Windowed) return "창 모드"; // 창 모드 문구 반환
            if (value == FullScreenMode.ExclusiveFullScreen) return "독점 전체 화면"; // 독점 전체 화면 문구 반환
            return "전체 화면 창"; // 기본 전체 화면 창 문구 반환
        } // 화면 모드 한국어 문구 생성 완료

        private static string FormatFrameRate(int value) // 최대 FPS 표시 문구 생성
        { // 제한 없음과 숫자 FPS 구분
            return value < 0 ? "제한 없음" : $"{value} FPS"; // 최대 FPS 문구 반환
        } // 최대 FPS 표시 문구 생성 완료

        private static string FormatPercent(float value) // 0에서 1 값을 백분율 문구로 변환
        { // 오디오 표시값 계산
            return $"{Mathf.RoundToInt(Mathf.Clamp01(value) * 100f)}%"; // 안전한 백분율 문구 반환
        } // 0에서 1 값을 백분율 문구로 변환 완료

        private static void SetSliderRange(Slider slider, float minimum, float maximum, bool wholeNumbers) // Slider 범위 안전 설정
        { // Slider 참조 존재 시 범위 적용
            if (slider == null) return; // Slider 없음 시 처리 종료
            slider.minValue = minimum; // Slider 최소값 적용
            slider.maxValue = maximum; // Slider 최대값 적용
            slider.wholeNumbers = wholeNumbers; // 정수 단위 사용 여부 적용
        } // Slider 범위 안전 설정 완료
    } // 설정 메뉴 런타임 동작 정의 완료
} // 설정 메뉴 UI 기능 정의 완료
