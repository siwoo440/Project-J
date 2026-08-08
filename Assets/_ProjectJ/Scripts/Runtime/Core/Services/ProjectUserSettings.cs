using System; // 직렬화와 문자열 비교 기능 참조
using UnityEngine; // Unity 화면과 품질 설정 기능 참조

namespace ProjectJ.Core.Services // 프로젝트 공통 서비스 네임스페이스 선언
{ // 사용자 설정 데이터 구성
    [Serializable] // JSON 직렬화 대상 지정
    public sealed class ProjectUserSettings // 사용자 설정 저장 데이터 선언
    { // 전체 사용자 설정 값과 복사 기능 구성
        public const int CurrentVersion = 2; // 밝기와 UI 음량이 추가된 현재 설정 파일 버전

        public int Version = CurrentVersion; // 저장된 설정 파일 버전
        public string GraphicsQualityName = string.Empty; // 그래픽 품질 단계 이름
        public int ResolutionWidth = 1920; // 화면 가로 해상도
        public int ResolutionHeight = 1080; // 화면 세로 해상도
        public int FullScreenModeValue = (int)FullScreenMode.FullScreenWindow; // 전체 화면 모드 정수값
        public int VSyncCount = 1; // 수직 동기화 간격
        public int TargetFrameRate = -1; // 목표 프레임 수
        public float Brightness = 1f; // 화면 밝기 배율
        public float MasterVolume = 1f; // 마스터 음량
        public float MusicVolume = 1f; // 배경 음악 음량
        public float SfxVolume = 1f; // 효과음 음량
        public float UiVolume = 1f; // UI 효과음 음량
        public bool IsMuted; // 전체 음소거 여부
        public float MouseSensitivity = 0.12f; // 마우스 시점 감도
        public float GamepadLookDegreesPerSecond = 180f; // 게임패드 시점 회전 속도
        public bool InvertLookY; // 수직 시점 반전 여부
        public string InputBindingOverridesJson = string.Empty; // 입력 재지정 JSON
        public int MinimumLogLevelValue = 1; // 최소 로그 등급 정수값

        public static ProjectUserSettings CreateDefault() // 현재 실행 환경 기반 기본 설정 생성
        { // 새 기본 설정 값 준비
            ProjectUserSettings settings = new ProjectUserSettings(); // 기본 설정 인스턴스 생성
            settings.ResolutionWidth = Mathf.Max(640, Screen.currentResolution.width); // 현재 화면 기준 가로 해상도 적용
            settings.ResolutionHeight = Mathf.Max(360, Screen.currentResolution.height); // 현재 화면 기준 세로 해상도 적용
            settings.FullScreenModeValue = (int)Screen.fullScreenMode; // 현재 전체 화면 모드 적용
            settings.VSyncCount = QualitySettings.vSyncCount; // 현재 수직 동기화 값 적용
            settings.TargetFrameRate = Application.targetFrameRate; // 현재 목표 프레임 값 적용
            settings.GraphicsQualityName = GetCurrentQualityName(); // 현재 품질 단계 이름 적용
            settings.Brightness = 1f; // 기본 밝기 100퍼센트 적용
            settings.UiVolume = 1f; // 기본 UI 음량 100퍼센트 적용
            settings.Validate(); // 생성된 기본 설정 유효 범위 보정
            return settings; // 생성된 기본 설정 반환
        } // 현재 실행 환경 기반 기본 설정 생성 마무리

        public ProjectUserSettings Clone() // 현재 설정의 독립 작업 복사본 생성
        { // 원본과 분리된 설정 객체 준비
            ProjectUserSettings clone = new ProjectUserSettings(); // 빈 설정 복사본 생성
            clone.CopyFrom(this); // 현재 설정 값을 새 객체에 복사
            return clone; // 독립 설정 복사본 반환
        } // 현재 설정의 독립 작업 복사본 생성 마무리

        public void CopyFrom(ProjectUserSettings source) // 다른 설정 객체의 전체 값 복사
        { // 설정 UI 적용과 복원용 일괄 복사
            if (source == null) // 복사 원본 누락 여부 확인
            { // 잘못된 복사 요청 방어
                throw new ArgumentNullException(nameof(source)); // null 설정 복사 예외 발생
            } // 잘못된 복사 요청 방어 마무리

            Version = source.Version; // 설정 파일 버전 복사
            GraphicsQualityName = source.GraphicsQualityName ?? string.Empty; // 그래픽 품질 이름 복사
            ResolutionWidth = source.ResolutionWidth; // 가로 해상도 복사
            ResolutionHeight = source.ResolutionHeight; // 세로 해상도 복사
            FullScreenModeValue = source.FullScreenModeValue; // 전체 화면 모드 복사
            VSyncCount = source.VSyncCount; // 수직 동기화 값 복사
            TargetFrameRate = source.TargetFrameRate; // 목표 프레임 값 복사
            Brightness = source.Brightness; // 화면 밝기 복사
            MasterVolume = source.MasterVolume; // 마스터 음량 복사
            MusicVolume = source.MusicVolume; // 배경 음악 음량 복사
            SfxVolume = source.SfxVolume; // 효과음 음량 복사
            UiVolume = source.UiVolume; // UI 음량 복사
            IsMuted = source.IsMuted; // 전체 음소거 상태 복사
            MouseSensitivity = source.MouseSensitivity; // 마우스 감도 복사
            GamepadLookDegreesPerSecond = source.GamepadLookDegreesPerSecond; // 게임패드 감도 복사
            InvertLookY = source.InvertLookY; // 수직 시점 반전 상태 복사
            InputBindingOverridesJson = source.InputBindingOverridesJson ?? string.Empty; // 입력 재지정 JSON 복사
            MinimumLogLevelValue = source.MinimumLogLevelValue; // 최소 로그 등급 복사
        } // 다른 설정 객체의 전체 값 복사 마무리

        public bool ContentEquals(ProjectUserSettings other) // 두 설정 객체의 실제 저장 값 동일 여부 확인
        { // 설정 UI 변경 여부 판단용 전체 값 비교
            if (other == null) // 비교 대상 누락 여부 확인
            { // null 비교 결과 처리
                return false; // 다른 설정으로 판정
            } // null 비교 결과 처리 마무리

            return Version == other.Version // 설정 파일 버전 비교
                && string.Equals(GraphicsQualityName, other.GraphicsQualityName, StringComparison.Ordinal) // 그래픽 품질 이름 비교
                && ResolutionWidth == other.ResolutionWidth // 가로 해상도 비교
                && ResolutionHeight == other.ResolutionHeight // 세로 해상도 비교
                && FullScreenModeValue == other.FullScreenModeValue // 전체 화면 모드 비교
                && VSyncCount == other.VSyncCount // 수직 동기화 값 비교
                && TargetFrameRate == other.TargetFrameRate // 목표 프레임 값 비교
                && Mathf.Approximately(Brightness, other.Brightness) // 화면 밝기 비교
                && Mathf.Approximately(MasterVolume, other.MasterVolume) // 마스터 음량 비교
                && Mathf.Approximately(MusicVolume, other.MusicVolume) // 배경 음악 음량 비교
                && Mathf.Approximately(SfxVolume, other.SfxVolume) // 효과음 음량 비교
                && Mathf.Approximately(UiVolume, other.UiVolume) // UI 음량 비교
                && IsMuted == other.IsMuted // 전체 음소거 상태 비교
                && Mathf.Approximately(MouseSensitivity, other.MouseSensitivity) // 마우스 감도 비교
                && Mathf.Approximately(GamepadLookDegreesPerSecond, other.GamepadLookDegreesPerSecond) // 게임패드 감도 비교
                && InvertLookY == other.InvertLookY // 수직 시점 반전 비교
                && string.Equals(InputBindingOverridesJson, other.InputBindingOverridesJson, StringComparison.Ordinal) // 입력 재지정 JSON 비교
                && MinimumLogLevelValue == other.MinimumLogLevelValue; // 최소 로그 등급 비교
        } // 두 설정 객체의 실제 저장 값 동일 여부 확인 마무리

        public void Validate() // 저장 데이터 범위와 누락값 보정
        { // 저장과 적용 전에 안전한 설정 값 보장
            Version = CurrentVersion; // 현재 설정 파일 버전 적용
            ResolutionWidth = Mathf.Max(640, ResolutionWidth); // 최소 가로 해상도 보장
            ResolutionHeight = Mathf.Max(360, ResolutionHeight); // 최소 세로 해상도 보장
            FullScreenModeValue = ValidateFullScreenMode(FullScreenModeValue); // 전체 화면 모드 유효값 보정
            VSyncCount = Mathf.Clamp(VSyncCount, 0, 4); // 수직 동기화 범위 제한
            TargetFrameRate = Mathf.Clamp(TargetFrameRate, -1, 360); // 목표 프레임 범위 제한
            Brightness = Mathf.Clamp(Brightness, 0.5f, 1.5f); // 화면 밝기 50퍼센트부터 150퍼센트 범위 제한
            MasterVolume = Mathf.Clamp01(MasterVolume); // 마스터 음량 범위 제한
            MusicVolume = Mathf.Clamp01(MusicVolume); // 배경 음악 음량 범위 제한
            SfxVolume = Mathf.Clamp01(SfxVolume); // 효과음 음량 범위 제한
            UiVolume = Mathf.Clamp01(UiVolume); // UI 음량 범위 제한
            MouseSensitivity = Mathf.Clamp(MouseSensitivity, 0.01f, 2f); // 마우스 감도 범위 제한
            GamepadLookDegreesPerSecond = Mathf.Clamp(GamepadLookDegreesPerSecond, 30f, 720f); // 게임패드 감도 범위 제한
            InputBindingOverridesJson ??= string.Empty; // 입력 재지정 누락값 보정
            MinimumLogLevelValue = Mathf.Clamp(MinimumLogLevelValue, 0, 4); // 최소 로그 등급 범위 제한

            if (string.IsNullOrWhiteSpace(GraphicsQualityName)) // 그래픽 품질 이름 누락 확인
            { // 품질 이름 안전값 복구
                GraphicsQualityName = GetCurrentQualityName(); // 현재 품질 이름으로 보정
            } // 품질 이름 안전값 복구 마무리
        } // 저장 데이터 범위와 누락값 보정 마무리

        private static string GetCurrentQualityName() // 현재 그래픽 품질 이름 조회
        { // 현재 플랫폼 품질 단계 안전 조회
            string[] qualityNames = QualitySettings.names; // 사용 가능한 품질 이름 배열 조회
            int currentQualityIndex = QualitySettings.GetQualityLevel(); // 현재 품질 인덱스 조회

            if (qualityNames == null || qualityNames.Length == 0) // 품질 단계 없음 확인
            { // 품질 단계가 없는 예외 환경 처리
                return string.Empty; // 빈 품질 이름 반환
            } // 품질 단계가 없는 예외 환경 처리 마무리

            int safeIndex = Mathf.Clamp(currentQualityIndex, 0, qualityNames.Length - 1); // 안전한 품질 인덱스 계산
            return qualityNames[safeIndex]; // 현재 품질 이름 반환
        } // 현재 그래픽 품질 이름 조회 마무리

        private static int ValidateFullScreenMode(int modeValue) // 전체 화면 모드 정수값 검증
        { // 저장된 정수값의 Unity 열거형 유효성 확인
            bool isDefined = Enum.IsDefined(typeof(FullScreenMode), modeValue); // Unity 전체 화면 모드 존재 여부 확인
            return isDefined ? modeValue : (int)FullScreenMode.FullScreenWindow; // 유효값 또는 기본값 반환
        } // 전체 화면 모드 정수값 검증 마무리
    } // 사용자 설정 저장 데이터 선언 마무리
} // 프로젝트 공통 서비스 네임스페이스 마무리
