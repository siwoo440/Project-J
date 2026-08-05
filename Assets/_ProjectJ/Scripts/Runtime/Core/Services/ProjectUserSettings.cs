using System; // 직렬화와 열거형 검증 기능 참조
using UnityEngine; // Unity 화면과 품질 설정 기능 참조

namespace ProjectJ.Core.Services // 프로젝트 공통 서비스 네임스페이스
{ // 네임스페이스 범위
    [Serializable] // JSON 직렬화 대상 지정
    public sealed class ProjectUserSettings // 사용자 설정 저장 데이터
    { // 클래스 범위
        public const int CurrentVersion = 1; // 현재 설정 파일 버전

        public int Version = CurrentVersion; // 저장된 설정 파일 버전
        public string GraphicsQualityName = string.Empty; // 그래픽 품질 단계 이름
        public int ResolutionWidth = 1920; // 화면 가로 해상도
        public int ResolutionHeight = 1080; // 화면 세로 해상도
        public int FullScreenModeValue = (int)FullScreenMode.FullScreenWindow; // 전체 화면 모드 정수값
        public int VSyncCount = 1; // 수직 동기화 간격
        public int TargetFrameRate = -1; // 목표 프레임 수
        public float MasterVolume = 1f; // 마스터 음량
        public float MusicVolume = 1f; // 배경 음악 음량
        public float SfxVolume = 1f; // 효과음 음량
        public bool IsMuted; // 전체 음소거 여부
        public float MouseSensitivity = 0.12f; // 마우스 시점 감도
        public float GamepadLookDegreesPerSecond = 180f; // 게임패드 시점 회전 속도
        public bool InvertLookY; // 수직 시점 반전 여부
        public string InputBindingOverridesJson = string.Empty; // 입력 재지정 JSON
        public int MinimumLogLevelValue = 1; // 최소 로그 등급 정수값

        public static ProjectUserSettings CreateDefault() // 현재 실행 환경 기반 기본 설정 생성
        { // 메서드 범위
            ProjectUserSettings settings = new ProjectUserSettings(); // 기본 설정 인스턴스 생성
            settings.ResolutionWidth = Mathf.Max(640, Screen.currentResolution.width); // 현재 화면 기준 가로 해상도 적용
            settings.ResolutionHeight = Mathf.Max(360, Screen.currentResolution.height); // 현재 화면 기준 세로 해상도 적용
            settings.FullScreenModeValue = (int)Screen.fullScreenMode; // 현재 전체 화면 모드 적용
            settings.VSyncCount = QualitySettings.vSyncCount; // 현재 수직 동기화 값 적용
            settings.TargetFrameRate = Application.targetFrameRate; // 현재 목표 프레임 값 적용
            settings.GraphicsQualityName = GetCurrentQualityName(); // 현재 품질 단계 이름 적용
            return settings; // 생성된 기본 설정 반환
        } // 메서드 범위

        public void Validate() // 저장 데이터 범위와 누락값 보정
        { // 메서드 범위
            Version = CurrentVersion; // 현재 설정 파일 버전 적용
            ResolutionWidth = Mathf.Max(640, ResolutionWidth); // 최소 가로 해상도 보장
            ResolutionHeight = Mathf.Max(360, ResolutionHeight); // 최소 세로 해상도 보장
            FullScreenModeValue = ValidateFullScreenMode(FullScreenModeValue); // 전체 화면 모드 유효값 보정
            VSyncCount = Mathf.Clamp(VSyncCount, 0, 4); // 수직 동기화 범위 제한
            TargetFrameRate = Mathf.Clamp(TargetFrameRate, -1, 360); // 목표 프레임 범위 제한
            MasterVolume = Mathf.Clamp01(MasterVolume); // 마스터 음량 범위 제한
            MusicVolume = Mathf.Clamp01(MusicVolume); // 배경 음악 음량 범위 제한
            SfxVolume = Mathf.Clamp01(SfxVolume); // 효과음 음량 범위 제한
            MouseSensitivity = Mathf.Clamp(MouseSensitivity, 0.01f, 2f); // 마우스 감도 범위 제한
            GamepadLookDegreesPerSecond = Mathf.Clamp(GamepadLookDegreesPerSecond, 30f, 720f); // 게임패드 감도 범위 제한
            InputBindingOverridesJson ??= string.Empty; // 입력 재지정 누락값 보정
            MinimumLogLevelValue = Mathf.Clamp(MinimumLogLevelValue, 0, 4); // 최소 로그 등급 범위 제한

            if (string.IsNullOrWhiteSpace(GraphicsQualityName)) // 그래픽 품질 이름 누락 확인
            { // 조건 범위
                GraphicsQualityName = GetCurrentQualityName(); // 현재 품질 이름으로 보정
            } // 조건 범위
        } // 메서드 범위

        private static string GetCurrentQualityName() // 현재 그래픽 품질 이름 조회
        { // 메서드 범위
            string[] qualityNames = QualitySettings.names; // 사용 가능한 품질 이름 배열 조회
            int currentQualityIndex = QualitySettings.GetQualityLevel(); // 현재 품질 인덱스 조회

            if (qualityNames == null || qualityNames.Length == 0) // 품질 단계 없음 확인
            { // 조건 범위
                return string.Empty; // 빈 품질 이름 반환
            } // 조건 범위

            int safeIndex = Mathf.Clamp(currentQualityIndex, 0, qualityNames.Length - 1); // 안전한 품질 인덱스 계산
            return qualityNames[safeIndex]; // 현재 품질 이름 반환
        } // 메서드 범위

        private static int ValidateFullScreenMode(int modeValue) // 전체 화면 모드 정수값 검증
        { // 메서드 범위
            bool isDefined = Enum.IsDefined(typeof(FullScreenMode), modeValue); // Unity 전체 화면 모드 존재 여부 확인
            return isDefined ? modeValue : (int)FullScreenMode.FullScreenWindow; // 유효값 또는 기본값 반환
        } // 메서드 범위
    } // 클래스 범위
} // 네임스페이스 범위
