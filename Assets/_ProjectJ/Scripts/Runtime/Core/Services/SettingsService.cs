using System; // 설정 변경 이벤트와 문자열 비교 기능 참조
using ProjectJ.Diagnostics; // 프로젝트 공통 로그 기능 참조
using UnityEngine; // Unity 그래픽과 JSON 기능 참조

namespace ProjectJ.Core.Services // 프로젝트 공통 서비스 네임스페이스
{ // 네임스페이스 범위
    public sealed class SettingsService : GameServiceBase // 사용자 설정 저장과 적용 서비스
    { // 클래스 범위
        private const string SettingsFileName = "user-settings.json"; // 사용자 설정 파일 이름
        private SaveService saveService; // 설정 파일 저장 서비스

        public override string ServiceName => "Settings"; // 설정 서비스 이름
        public override int InitializationOrder => 200; // 설정 서비스 초기화 순서
        public ProjectUserSettings Current { get; private set; } // 현재 사용자 설정 데이터
        public float MasterVolume => Current != null ? Current.MasterVolume : 1f; // 기존 코드 호환 마스터 음량
        public SystemLanguage Language { get; private set; } // 현재 시스템 언어
        public event Action<ProjectUserSettings> SettingsChanged; // 설정 변경 알림 이벤트

        protected override void OnInitialize() // 저장 설정 읽기와 초기 적용
        { // 메서드 범위
            saveService = GameServiceRegistry.Get<SaveService>(); // 초기화된 저장 서비스 조회
            Language = Application.systemLanguage; // 현재 운영체제 언어 저장
            Current = LoadOrCreateSettings(); // 저장 설정 또는 기본 설정 준비
            Current.Validate(); // 설정값 유효 범위 보정
            ApplyLogLevel(); // 최소 로그 등급 우선 적용
            if (Application.isPlaying) // 실제 실행 중 그래픽 적용 여부 확인
            { // 조건 범위
                ApplyGraphics(); // 그래픽 설정 즉시 적용
            } // 조건 범위
            SaveCurrent(); // 보정된 최신 설정 파일 저장
            ProjectLog.Info(ProjectLogCategory.Core, "사용자 설정을 불러와 적용했습니다.", "SETTINGS_APPLIED"); // 설정 적용 완료 정보 출력
        } // 메서드 범위

        public void SetGraphics(string qualityName, int width, int height, FullScreenMode fullScreenMode, int vSyncCount, int targetFrameRate) // 그래픽 설정 변경과 저장
        { // 메서드 범위
            Current.GraphicsQualityName = qualityName; // 그래픽 품질 이름 변경
            Current.ResolutionWidth = width; // 가로 해상도 변경
            Current.ResolutionHeight = height; // 세로 해상도 변경
            Current.FullScreenModeValue = (int)fullScreenMode; // 전체 화면 모드 변경
            Current.VSyncCount = vSyncCount; // 수직 동기화 값 변경
            Current.TargetFrameRate = targetFrameRate; // 목표 프레임 변경
            CommitChanges(true, false); // 그래픽 적용과 설정 저장
        } // 메서드 범위

        public void SetAudio(float masterVolume, float musicVolume, float sfxVolume, bool isMuted) // 오디오 설정 변경과 저장
        { // 메서드 범위
            Current.MasterVolume = masterVolume; // 마스터 음량 변경
            Current.MusicVolume = musicVolume; // 배경 음악 음량 변경
            Current.SfxVolume = sfxVolume; // 효과음 음량 변경
            Current.IsMuted = isMuted; // 전체 음소거 상태 변경
            CommitChanges(false, false); // 설정 저장과 오디오 변경 알림
        } // 메서드 범위

        public void SetControls(float mouseSensitivity, float gamepadLookDegreesPerSecond, bool invertLookY) // 조작 설정 변경과 저장
        { // 메서드 범위
            Current.MouseSensitivity = mouseSensitivity; // 마우스 감도 변경
            Current.GamepadLookDegreesPerSecond = gamepadLookDegreesPerSecond; // 게임패드 감도 변경
            Current.InvertLookY = invertLookY; // 수직 시점 반전 변경
            CommitChanges(false, false); // 설정 저장과 조작 변경 알림
        } // 메서드 범위

        public void SetInputBindingOverrides(string bindingOverridesJson) // 입력 재지정 JSON 변경과 저장
        { // 메서드 범위
            Current.InputBindingOverridesJson = bindingOverridesJson ?? string.Empty; // 입력 재지정 JSON 변경
            CommitChanges(false, false); // 설정 저장과 입력 변경 알림
        } // 메서드 범위

        public void SetMinimumLogLevel(ProjectLogLevel minimumLevel) // 최소 로그 등급 변경과 저장
        { // 메서드 범위
            Current.MinimumLogLevelValue = (int)minimumLevel; // 최소 로그 등급 정수값 변경
            CommitChanges(false, true); // 로그 등급 적용과 설정 저장
        } // 메서드 범위

        public void ResetToDefaults() // 전체 사용자 설정 기본값 복원
        { // 메서드 범위
            Current = ProjectUserSettings.CreateDefault(); // 현재 환경 기반 기본 설정 생성
            CommitChanges(true, true); // 전체 설정 적용과 저장
            ProjectLog.Info(ProjectLogCategory.Core, "사용자 설정을 기본값으로 복원했습니다.", "SETTINGS_RESET"); // 기본값 복원 정보 출력
        } // 메서드 범위

        public bool SaveCurrent() // 현재 설정 JSON 파일 저장
        { // 메서드 범위
            Current.Validate(); // 저장 전 설정값 범위 보정
            string json = JsonUtility.ToJson(Current, true); // 읽기 쉬운 JSON 문자열 생성
            return saveService.SaveSettingsText(SettingsFileName, json); // 설정 파일 저장 결과 반환
        } // 메서드 범위

        private ProjectUserSettings LoadOrCreateSettings() // 저장 설정 읽기 또는 기본 설정 생성
        { // 메서드 범위
            if (!saveService.TryLoadSettingsText(SettingsFileName, out string json)) // 저장 설정 읽기 실패 또는 최초 실행 확인
            { // 조건 범위
                ProjectLog.Info(ProjectLogCategory.Core, "저장된 사용자 설정이 없어 기본값을 사용합니다.", "SETTINGS_DEFAULT_CREATED"); // 최초 설정 생성 정보 출력
                return ProjectUserSettings.CreateDefault(); // 기본 설정 반환
            } // 조건 범위

            try // JSON 변환 예외 감시
            { // 예외 감시 범위
                ProjectUserSettings loadedSettings = JsonUtility.FromJson<ProjectUserSettings>(json); // JSON 사용자 설정 변환

                if (loadedSettings == null) // 변환된 설정 누락 확인
                { // 조건 범위
                    throw new InvalidOperationException("설정 JSON 결과가 비어 있습니다."); // 잘못된 설정 예외 발생
                } // 조건 범위

                if (loadedSettings.Version != ProjectUserSettings.CurrentVersion) // 지원하지 않는 설정 파일 버전 확인
                { // 조건 범위
                    ProjectLog.Warning(ProjectLogCategory.Core, $"설정 파일 버전 {loadedSettings.Version}을 지원하지 않아 기본값으로 복원합니다.", "SETTINGS_VERSION_UNSUPPORTED"); // 설정 버전 복구 경고 출력
                    return ProjectUserSettings.CreateDefault(); // 현재 버전 기본 설정 반환
                } // 조건 범위

                return loadedSettings; // 저장된 사용자 설정 반환
            } // 예외 감시 범위
            catch (Exception exception) // 손상된 설정 JSON 처리
            { // 예외 처리 범위
                ProjectLog.Warning(ProjectLogCategory.Core, $"설정 파일이 손상되어 기본값으로 복원합니다. {exception.Message}", "SETTINGS_JSON_INVALID"); // 설정 복구 경고 출력
                return ProjectUserSettings.CreateDefault(); // 안전한 기본 설정 반환
            } // 예외 처리 범위
        } // 메서드 범위

        private void CommitChanges(bool applyGraphics, bool applyLogLevel) // 설정 보정과 적용과 저장 통합
        { // 메서드 범위
            Current.Validate(); // 변경된 설정값 범위 보정

            if (applyLogLevel) // 로그 등급 즉시 적용 여부 확인
            { // 조건 범위
                ApplyLogLevel(); // 최소 로그 등급 적용
            } // 조건 범위

            if (applyGraphics && Application.isPlaying) // 실제 실행 중 그래픽 즉시 적용 여부 확인
            { // 조건 범위
                ApplyGraphics(); // 그래픽 설정 적용
            } // 조건 범위

            SaveCurrent(); // 변경된 전체 설정 저장
            SettingsChanged?.Invoke(Current); // 연결된 런타임 기능에 변경 알림
        } // 메서드 범위

        private void ApplyGraphics() // 현재 그래픽 설정을 Unity에 적용
        { // 메서드 범위
            int qualityIndex = FindQualityIndex(Current.GraphicsQualityName); // 저장된 품질 이름의 현재 인덱스 검색

            if (qualityIndex >= 0) // 품질 단계 검색 성공 확인
            { // 조건 범위
                QualitySettings.SetQualityLevel(qualityIndex, true); // 그래픽 품질 단계와 고비용 변경 적용
            } // 조건 범위
            else // 저장된 품질 단계 누락 처리
            { // 대체 범위
                ProjectLog.Warning(ProjectLogCategory.Core, $"그래픽 품질 단계 '{Current.GraphicsQualityName}'를 찾지 못해 현재 단계를 유지합니다.", "GRAPHICS_QUALITY_NOT_FOUND"); // 품질 단계 누락 경고 출력
            } // 대체 범위

            QualitySettings.vSyncCount = Current.VSyncCount; // 수직 동기화 값 적용
            Application.targetFrameRate = Current.TargetFrameRate; // 목표 프레임 값 적용
            FullScreenMode fullScreenMode = (FullScreenMode)Current.FullScreenModeValue; // 저장된 전체 화면 모드 변환
            Screen.SetResolution(Current.ResolutionWidth, Current.ResolutionHeight, fullScreenMode); // 해상도와 전체 화면 모드 적용
        } // 메서드 범위

        private void ApplyLogLevel() // 현재 최소 로그 등급 적용
        { // 메서드 범위
            ProjectLogLevel minimumLevel = (ProjectLogLevel)Current.MinimumLogLevelValue; // 저장된 최소 로그 등급 변환
            ProjectLog.ConfigureMinimumLevel(minimumLevel); // 공통 로그 출력 기준 적용
        } // 메서드 범위

        private static int FindQualityIndex(string qualityName) // 품질 이름에 해당하는 현재 인덱스 검색
        { // 메서드 범위
            string[] qualityNames = QualitySettings.names; // 현재 플랫폼 품질 이름 배열 조회

            for (int index = 0; index < qualityNames.Length; index++) // 전체 품질 이름 순회
            { // 반복 범위
                if (string.Equals(qualityNames[index], qualityName, StringComparison.Ordinal)) // 저장 이름과 현재 이름 일치 확인
                { // 조건 범위
                    return index; // 일치하는 품질 인덱스 반환
                } // 조건 범위
            } // 반복 범위

            return -1; // 품질 이름 검색 실패 반환
        } // 메서드 범위
    } // 클래스 범위
} // 네임스페이스 범위
