using ProjectJ.Core.Services; // 공통 서비스와 사용자 설정 기능 참조
using UnityEngine; // Unity 오디오와 수치 제한 기능 참조

namespace ProjectJ.Audio // 프로젝트 오디오 네임스페이스
{ // 네임스페이스 범위
    public sealed class AudioService : GameServiceBase // 프로젝트 전체 음량 관리 서비스
    { // 클래스 범위
        private SettingsService settingsService; // 사용자 설정 서비스

        public override string ServiceName => "Audio"; // 오디오 서비스 이름
        public override int InitializationOrder => 300; // 오디오 서비스 초기화 순서
        public float MasterVolume { get; private set; } = 1f; // 현재 마스터 음량
        public float MusicVolume { get; private set; } = 1f; // 현재 배경 음악 음량
        public float SfxVolume { get; private set; } = 1f; // 현재 효과음 음량
        public bool IsMuted { get; private set; } // 현재 전체 음소거 여부

        protected override void OnInitialize() // 저장된 오디오 설정 연결과 적용
        { // 메서드 범위
            settingsService = GameServiceRegistry.Get<SettingsService>(); // 초기화된 설정 서비스 조회
            settingsService.SettingsChanged += HandleSettingsChanged; // 설정 변경 이벤트 구독
            ApplySettings(settingsService.Current); // 저장된 오디오 설정 즉시 적용
        } // 메서드 범위

        public void SetMasterVolume(float volume) // 마스터 음량 변경과 저장
        { // 메서드 범위
            settingsService.SetAudio(volume, MusicVolume, SfxVolume, IsMuted); // 전체 오디오 설정 갱신 요청
        } // 메서드 범위

        public void SetMusicVolume(float volume) // 배경 음악 음량 변경과 저장
        { // 메서드 범위
            settingsService.SetAudio(MasterVolume, volume, SfxVolume, IsMuted); // 전체 오디오 설정 갱신 요청
        } // 메서드 범위

        public void SetSfxVolume(float volume) // 효과음 음량 변경과 저장
        { // 메서드 범위
            settingsService.SetAudio(MasterVolume, MusicVolume, volume, IsMuted); // 전체 오디오 설정 갱신 요청
        } // 메서드 범위

        public void SetMuted(bool isMuted) // 전체 음소거 상태 변경과 저장
        { // 메서드 범위
            settingsService.SetAudio(MasterVolume, MusicVolume, SfxVolume, isMuted); // 전체 오디오 설정 갱신 요청
        } // 메서드 범위

        private void HandleSettingsChanged(ProjectUserSettings settings) // 사용자 설정 변경 이벤트 처리
        { // 메서드 범위
            ApplySettings(settings); // 변경된 오디오 설정 적용
        } // 메서드 범위

        private void ApplySettings(ProjectUserSettings settings) // 사용자 설정에서 오디오 값 적용
        { // 메서드 범위
            MasterVolume = Mathf.Clamp01(settings.MasterVolume); // 마스터 음량 적용
            MusicVolume = Mathf.Clamp01(settings.MusicVolume); // 배경 음악 음량 적용
            SfxVolume = Mathf.Clamp01(settings.SfxVolume); // 효과음 음량 적용
            IsMuted = settings.IsMuted; // 전체 음소거 상태 적용
            if (Application.isPlaying) // 실제 실행 중 오디오 적용 여부 확인
            { // 조건 범위
                AudioListener.volume = IsMuted ? 0f : MasterVolume; // 실제 전체 오디오 출력 음량 적용
            } // 조건 범위
        } // 메서드 범위
    } // 클래스 범위
} // 네임스페이스 범위
