using System; // 설정 변경 이벤트와 문자열 비교 기능 참조
using ProjectJ.Diagnostics; // 프로젝트 공통 로그 기능 참조
using UnityEngine; // Unity 그래픽 기능 참조

namespace ProjectJ.Core.Services // 프로젝트 공통 서비스 네임스페이스 선언
{ // 사용자 설정 저장과 런타임 적용 서비스 구성
    public sealed class SettingsService : GameServiceBase // 사용자 설정 저장과 적용 서비스 선언
    { // 설정 파일 생명주기와 기능별 변경 API 구성
        private const string SettingsFileName = "user-settings.json"; // 사용자 설정 파일 이름
        private SaveService saveService; // 설정 파일 저장 서비스

        public override string ServiceName => "Settings"; // 설정 서비스 이름
        public override int InitializationOrder => 200; // 설정 서비스 초기화 순서
        public ProjectUserSettings Current { get; private set; } // 현재 사용자 설정 데이터
        public float MasterVolume => Current != null ? Current.MasterVolume : 1f; // 기존 코드 호환 마스터 음량
        public SystemLanguage Language { get; private set; } // 현재 시스템 언어
        public event Action<ProjectUserSettings> SettingsChanged; // 설정 변경 알림 이벤트

        protected override void OnInitialize() // 저장 설정 읽기와 초기 적용
        { // 저장 서비스 연결과 현재 설정 준비
            saveService = GameServiceRegistry.Get<SaveService>(); // 초기화된 저장 서비스 조회
            Language = Application.systemLanguage; // 현재 운영체제 언어 저장
            Current = LoadOrCreateSettings(); // 저장 설정 또는 기본 설정 준비
            Current.Validate(); // 설정값 유효 범위 보정
            ApplyLogLevel(); // 최소 로그 등급 우선 적용

            if (Application.isPlaying) // 실제 실행 중 그래픽 적용 여부 확인
            { // 플레이 환경 그래픽과 밝기 적용
                ApplyGraphics(); // 그래픽과 밝기 설정 즉시 적용
            } // 플레이 환경 그래픽과 밝기 적용 마무리

            SaveCurrent(); // 마이그레이션과 보정이 반영된 최신 설정 파일 저장
            ProjectLog.Info(ProjectLogCategory.Core, "사용자 설정을 불러와 적용했습니다.", "SETTINGS_APPLIED"); // 설정 적용 완료 정보 출력
        } // 저장 설정 읽기와 초기 적용 마무리

        public ProjectUserSettings CreateSnapshot() // 현재 설정의 독립 작업 복사본 생성
        { // 설정 UI가 원본을 직접 수정하지 않도록 복사본 제공
            if (Current == null) // 서비스 초기화 전 현재 설정 누락 여부 확인
            { // 초기화 전 안전 기본값 제공
                return ProjectUserSettings.CreateDefault(); // 현재 환경 기반 기본 설정 반환
            } // 초기화 전 안전 기본값 제공 마무리

            return Current.Clone(); // 현재 설정의 독립 복사본 반환
        } // 현재 설정의 독립 작업 복사본 생성 마무리

        public bool ApplySettings(ProjectUserSettings settings) // 작업 복사본을 현재 설정으로 확정하고 저장
        { // 설정 UI 적용 버튼용 통합 진입점
            if (settings == null) // 적용할 설정 객체 누락 여부 확인
            { // 잘못된 설정 적용 요청 방어
                throw new ArgumentNullException(nameof(settings)); // null 설정 적용 예외 발생
            } // 잘못된 설정 적용 요청 방어 마무리

            Current = settings.Clone(); // UI 작업 복사본과 분리된 현재 설정 생성
            return CommitChanges(true, true); // 전체 설정 보정과 적용과 저장 실행
        } // 작업 복사본을 현재 설정으로 확정하고 저장 마무리

        public bool ReloadFromDisk() // 설정 파일을 다시 읽어 현재 설정으로 복원
        { // 저장 파일 기준 전체 설정 재적용
            Current = LoadOrCreateSettings(); // 저장 설정 또는 안전 기본값 다시 준비
            Current.Validate(); // 다시 읽은 설정값 유효 범위 보정
            ApplyLogLevel(); // 저장된 최소 로그 등급 적용

            if (Application.isPlaying) // 실제 실행 중 그래픽 재적용 여부 확인
            { // 플레이 환경 그래픽과 밝기 재적용
                ApplyGraphics(); // 저장된 그래픽과 밝기 설정 적용
            } // 플레이 환경 그래픽과 밝기 재적용 마무리

            bool saveSucceeded = SaveCurrent(); // 마이그레이션과 보정이 반영된 현재 설정 저장
            SettingsChanged?.Invoke(Current); // 오디오와 카메라와 입력 연결 시스템에 재적용 알림
            ProjectLog.Info(ProjectLogCategory.Core, "사용자 설정을 저장 파일 기준으로 다시 불러왔습니다.", "SETTINGS_RELOADED"); // 설정 다시 불러오기 정보 출력
            return saveSucceeded; // 최종 설정 저장 성공 여부 반환
        } // 설정 파일을 다시 읽어 현재 설정으로 복원 마무리

        public void SetGraphics(string qualityName, int width, int height, FullScreenMode fullScreenMode, int vSyncCount, int targetFrameRate) // 기존 그래픽 설정 변경과 저장
        { // 기존 호출부 호환용 밝기 유지 처리
            float currentBrightness = Current != null ? Current.Brightness : 1f; // 현재 밝기 값 조회
            SetGraphics(qualityName, width, height, fullScreenMode, vSyncCount, targetFrameRate, currentBrightness); // 밝기를 유지한 새 그래픽 API 호출
        } // 기존 그래픽 설정 변경과 저장 마무리

        public void SetGraphics(string qualityName, int width, int height, FullScreenMode fullScreenMode, int vSyncCount, int targetFrameRate, float brightness) // 화면과 밝기 설정 변경과 저장
        { // 그래픽 설정 전체 값 갱신
            Current.GraphicsQualityName = qualityName; // 그래픽 품질 이름 변경
            Current.ResolutionWidth = width; // 가로 해상도 변경
            Current.ResolutionHeight = height; // 세로 해상도 변경
            Current.FullScreenModeValue = (int)fullScreenMode; // 전체 화면 모드 변경
            Current.VSyncCount = vSyncCount; // 수직 동기화 값 변경
            Current.TargetFrameRate = targetFrameRate; // 목표 프레임 변경
            Current.Brightness = brightness; // 화면 밝기 변경
            CommitChanges(true, false); // 그래픽과 밝기 적용과 설정 저장
        } // 화면과 밝기 설정 변경과 저장 마무리

        public void SetAudio(float masterVolume, float musicVolume, float sfxVolume, bool isMuted) // 기존 오디오 설정 변경과 저장
        { // 기존 호출부 호환용 UI 음량 유지 처리
            float currentUiVolume = Current != null ? Current.UiVolume : 1f; // 현재 UI 음량 값 조회
            SetAudio(masterVolume, musicVolume, sfxVolume, currentUiVolume, isMuted); // UI 음량을 유지한 새 오디오 API 호출
        } // 기존 오디오 설정 변경과 저장 마무리

        public void SetAudio(float masterVolume, float musicVolume, float sfxVolume, float uiVolume, bool isMuted) // 전체 오디오 설정 변경과 저장
        { // Master와 BGM과 SFX와 UI 음량 전체 값 갱신
            Current.MasterVolume = masterVolume; // 마스터 음량 변경
            Current.MusicVolume = musicVolume; // 배경 음악 음량 변경
            Current.SfxVolume = sfxVolume; // 효과음 음량 변경
            Current.UiVolume = uiVolume; // UI 효과음 음량 변경
            Current.IsMuted = isMuted; // 전체 음소거 상태 변경
            CommitChanges(false, false); // 설정 저장과 오디오 변경 알림
        } // 전체 오디오 설정 변경과 저장 마무리

        public void SetControls(float mouseSensitivity, float gamepadLookDegreesPerSecond, bool invertLookY) // 카메라 조작 설정 변경과 저장
        { // 기존 카메라 조작 설정 변경 API 유지
            Current.MouseSensitivity = mouseSensitivity; // 마우스 감도 변경
            Current.GamepadLookDegreesPerSecond = gamepadLookDegreesPerSecond; // 게임패드 감도 변경
            Current.InvertLookY = invertLookY; // 수직 시점 반전 변경
            CommitChanges(false, false); // 설정 저장과 조작 변경 알림
        } // 카메라 조작 설정 변경과 저장 마무리

        public void SetInputBindingOverrides(string bindingOverridesJson) // 입력 재지정 JSON 변경과 저장
        { // Input System 재지정 저장 API 유지
            Current.InputBindingOverridesJson = bindingOverridesJson ?? string.Empty; // 입력 재지정 JSON 변경
            CommitChanges(false, false); // 설정 저장과 입력 변경 알림
        } // 입력 재지정 JSON 변경과 저장 마무리

        public void SetMinimumLogLevel(ProjectLogLevel minimumLevel) // 최소 로그 등급 변경과 저장
        { // 기존 로그 설정 변경 API 유지
            Current.MinimumLogLevelValue = (int)minimumLevel; // 최소 로그 등급 정수값 변경
            CommitChanges(false, true); // 로그 등급 적용과 설정 저장
        } // 최소 로그 등급 변경과 저장 마무리

        public bool ResetToDefaults() // 전체 사용자 설정 기본값 복원
        { // 현재 실행 환경 기준 기본 설정 재생성
            Current = ProjectUserSettings.CreateDefault(); // 현재 환경 기반 기본 설정 생성
            bool saveSucceeded = CommitChanges(true, true); // 전체 설정 적용과 저장
            ProjectLog.Info(ProjectLogCategory.Core, "사용자 설정을 기본값으로 복원했습니다.", "SETTINGS_RESET"); // 기본값 복원 정보 출력
            return saveSucceeded; // 기본 설정 저장 성공 여부 반환
        } // 전체 사용자 설정 기본값 복원 마무리

        public bool SaveCurrent() // 현재 설정 JSON 파일 저장
        { // 설정 데이터 검증과 JSON 직렬화 처리
            if (Current == null) // 현재 설정 누락 여부 확인
            { // 초기화 전 저장 요청 차단
                ProjectLog.Warning(ProjectLogCategory.Core, "현재 사용자 설정이 없어 저장을 생략합니다.", "SETTINGS_SAVE_SKIPPED"); // 설정 누락 저장 경고 출력
                return false; // 설정 저장 실패 반환
            } // 초기화 전 저장 요청 차단 마무리

            Current.Validate(); // 저장 전 설정값 범위 보정
            string json = SettingsJsonSerializer.Serialize(Current, true); // 읽기 쉬운 안전 JSON 문자열 생성
            return saveService.SaveSettingsText(SettingsFileName, json); // 설정 파일 저장 결과 반환
        } // 현재 설정 JSON 파일 저장 마무리

        private ProjectUserSettings LoadOrCreateSettings() // 저장 설정 읽기 또는 기본 설정 생성
        { // 설정 파일 존재와 JSON 유효성 기준 복구 처리
            if (!saveService.TryLoadSettingsText(SettingsFileName, out string json)) // 저장 설정 읽기 실패 또는 최초 실행 확인
            { // 저장 설정이 없는 상태 처리
                ProjectLog.Info(ProjectLogCategory.Core, "저장된 사용자 설정이 없어 기본값을 사용합니다.", "SETTINGS_DEFAULT_CREATED"); // 최초 설정 생성 정보 출력
                return ProjectUserSettings.CreateDefault(); // 기본 설정 반환
            } // 저장 설정이 없는 상태 처리 마무리

            if (!SettingsJsonSerializer.TryDeserialize(json, out ProjectUserSettings loadedSettings, out string failureReason)) // 저장 JSON 변환 실패 여부 확인
            { // 손상 또는 지원하지 않는 설정 복구
                ProjectLog.Warning(ProjectLogCategory.Core, $"설정 파일을 사용할 수 없어 기본값으로 복원합니다. {failureReason}", "SETTINGS_JSON_INVALID"); // 설정 복구 경고 출력
                return ProjectUserSettings.CreateDefault(); // 안전한 기본 설정 반환
            } // 손상 또는 지원하지 않는 설정 복구 마무리

            return loadedSettings; // 검증과 마이그레이션 완료 설정 반환
        } // 저장 설정 읽기 또는 기본 설정 생성 마무리

        private bool CommitChanges(bool applyGraphics, bool applyLogLevel) // 설정 보정과 적용과 저장 통합
        { // 기능별 변경 API의 공통 마무리 처리
            Current.Validate(); // 변경된 설정값 범위 보정

            if (applyLogLevel) // 로그 등급 즉시 적용 여부 확인
            { // 로그 출력 기준 갱신
                ApplyLogLevel(); // 최소 로그 등급 적용
            } // 로그 출력 기준 갱신 마무리

            if (applyGraphics && Application.isPlaying) // 실제 실행 중 그래픽 즉시 적용 여부 확인
            { // 플레이 환경 그래픽과 밝기 갱신
                ApplyGraphics(); // 그래픽과 밝기 설정 적용
            } // 플레이 환경 그래픽과 밝기 갱신 마무리

            bool saveSucceeded = SaveCurrent(); // 변경된 전체 설정 저장
            SettingsChanged?.Invoke(Current); // 오디오와 카메라와 입력 기능에 변경 알림
            return saveSucceeded; // 설정 저장 성공 여부 반환
        } // 설정 보정과 적용과 저장 통합 마무리

        private void ApplyGraphics() // 현재 그래픽과 밝기 설정을 Unity에 적용
        { // 품질과 화면과 URP 밝기 설정 런타임 적용
            int qualityIndex = FindQualityIndex(Current.GraphicsQualityName); // 저장된 품질 이름의 현재 인덱스 검색

            if (qualityIndex >= 0) // 품질 단계 검색 성공 확인
            { // 유효한 품질 단계 적용
                QualitySettings.SetQualityLevel(qualityIndex, true); // 그래픽 품질 단계와 고비용 변경 적용
            } // 유효한 품질 단계 적용 마무리
            else // 저장된 품질 단계 누락 처리
            { // 품질 단계 누락 시 현재 단계 유지
                ProjectLog.Warning(ProjectLogCategory.Core, $"그래픽 품질 단계 '{Current.GraphicsQualityName}'를 찾지 못해 현재 단계를 유지합니다.", "GRAPHICS_QUALITY_NOT_FOUND"); // 품질 단계 누락 경고 출력
            } // 품질 단계 누락 시 현재 단계 유지 마무리

            QualitySettings.vSyncCount = Current.VSyncCount; // 수직 동기화 값 적용
            Application.targetFrameRate = Current.TargetFrameRate; // 목표 프레임 값 적용
            FullScreenMode fullScreenMode = (FullScreenMode)Current.FullScreenModeValue; // 저장된 전체 화면 모드 변환
            Screen.SetResolution(Current.ResolutionWidth, Current.ResolutionHeight, fullScreenMode); // 해상도와 전체 화면 모드 적용
            DisplayBrightnessApplier.Apply(Current.Brightness); // URP Post Exposure 기반 화면 밝기 적용
        } // 현재 그래픽과 밝기 설정을 Unity에 적용 마무리

        private void ApplyLogLevel() // 현재 최소 로그 등급 적용
        { // 공통 로그 출력 기준 갱신
            ProjectLogLevel minimumLevel = (ProjectLogLevel)Current.MinimumLogLevelValue; // 저장된 최소 로그 등급 변환
            ProjectLog.ConfigureMinimumLevel(minimumLevel); // 공통 로그 출력 기준 적용
        } // 현재 최소 로그 등급 적용 마무리

        private static int FindQualityIndex(string qualityName) // 품질 이름에 해당하는 현재 인덱스 검색
        { // 현재 플랫폼 품질 이름 배열 순회
            string[] qualityNames = QualitySettings.names; // 현재 플랫폼 품질 이름 배열 조회

            for (int index = 0; index < qualityNames.Length; index++) // 전체 품질 이름 순회
            { // 현재 품질 이름과 저장 이름 비교
                if (string.Equals(qualityNames[index], qualityName, StringComparison.Ordinal)) // 저장 이름과 현재 이름 일치 확인
                { // 일치 품질 단계 처리
                    return index; // 일치하는 품질 인덱스 반환
                } // 일치 품질 단계 처리 마무리
            } // 전체 품질 이름 순회 마무리

            return -1; // 품질 이름 검색 실패 반환
        } // 품질 이름에 해당하는 현재 인덱스 검색 마무리
    } // 사용자 설정 저장과 적용 서비스 마무리
} // 프로젝트 공통 서비스 네임스페이스 마무리
