using System; // 오디오 설정 변경 이벤트 기능 참조
using ProjectJ.Core.Services; // 공통 서비스와 사용자 설정 기능 참조
using UnityEngine; // Unity 오디오와 수치 제한 기능 참조

namespace ProjectJ.Audio // 프로젝트 오디오 네임스페이스 선언
{ // 프로젝트 전체 사용자 음량 관리 기능 구성
    public sealed class AudioService : GameServiceBase // 프로젝트 전체 음량 관리 서비스 선언
    { // Master와 BGM과 SFX와 UI 음량 관리 기능 구성
        private SettingsService settingsService; // 사용자 설정 서비스

        public override string ServiceName => "Audio"; // 오디오 서비스 이름
        public override int InitializationOrder => 300; // 오디오 서비스 초기화 순서
        public float MasterVolume { get; private set; } = 1f; // 현재 마스터 음량
        public float MusicVolume { get; private set; } = 1f; // 현재 배경 음악 음량
        public float SfxVolume { get; private set; } = 1f; // 현재 효과음 음량
        public float UiVolume { get; private set; } = 1f; // 현재 UI 효과음 음량
        public bool IsMuted { get; private set; } // 현재 전체 음소거 여부
        public event Action AudioSettingsChanged; // AudioSource 채널 컴포넌트용 음량 변경 알림

        protected override void OnInitialize() // 저장된 오디오 설정 연결과 적용
        { // SettingsService 변경 이벤트 구독과 최초 음량 반영
            settingsService = GameServiceRegistry.Get<SettingsService>(); // 초기화된 설정 서비스 조회
            settingsService.SettingsChanged += HandleSettingsChanged; // 설정 변경 이벤트 구독
            ApplySettings(settingsService.Current); // 저장된 오디오 설정 즉시 적용
        } // 저장된 오디오 설정 연결과 적용 마무리

        public void SetMasterVolume(float volume) // 마스터 음량 변경과 저장
        { // 전체 오디오 설정에서 Master 값만 변경
            settingsService.SetAudio(volume, MusicVolume, SfxVolume, UiVolume, IsMuted); // 전체 오디오 설정 갱신 요청
        } // 마스터 음량 변경과 저장 마무리

        public void SetMusicVolume(float volume) // 배경 음악 음량 변경과 저장
        { // 전체 오디오 설정에서 BGM 값만 변경
            settingsService.SetAudio(MasterVolume, volume, SfxVolume, UiVolume, IsMuted); // 전체 오디오 설정 갱신 요청
        } // 배경 음악 음량 변경과 저장 마무리

        public void SetSfxVolume(float volume) // 효과음 음량 변경과 저장
        { // 전체 오디오 설정에서 SFX 값만 변경
            settingsService.SetAudio(MasterVolume, MusicVolume, volume, UiVolume, IsMuted); // 전체 오디오 설정 갱신 요청
        } // 효과음 음량 변경과 저장 마무리

        public void SetUiVolume(float volume) // UI 효과음 음량 변경과 저장
        { // 전체 오디오 설정에서 UI 값만 변경
            settingsService.SetAudio(MasterVolume, MusicVolume, SfxVolume, volume, IsMuted); // 전체 오디오 설정 갱신 요청
        } // UI 효과음 음량 변경과 저장 마무리

        public void SetMuted(bool isMuted) // 전체 음소거 상태 변경과 저장
        { // 전체 오디오 설정에서 Mute 값만 변경
            settingsService.SetAudio(MasterVolume, MusicVolume, SfxVolume, UiVolume, isMuted); // 전체 오디오 설정 갱신 요청
        } // 전체 음소거 상태 변경과 저장 마무리

        public float GetChannelVolume(ProjectAudioChannel channel) // 지정 오디오 카테고리의 현재 음량 배율 반환
        { // AudioSource 채널별 BGM과 SFX와 UI 값 선택
            switch (channel) // 오디오 채널 종류 분기
            { // 채널별 사용자 음량 반환
                case ProjectAudioChannel.Music: // 배경 음악 채널 확인
                    return MusicVolume; // 현재 BGM 음량 반환
                case ProjectAudioChannel.UI: // UI 효과음 채널 확인
                    return UiVolume; // 현재 UI 음량 반환
                default: // 일반 게임 효과음 채널 처리
                    return SfxVolume; // 현재 SFX 음량 반환
            } // 채널별 사용자 음량 반환 마무리
        } // 지정 오디오 카테고리의 현재 음량 배율 반환 마무리

        private void HandleSettingsChanged(ProjectUserSettings settings) // 사용자 설정 변경 이벤트 처리
        { // 새 오디오 설정 전체 재적용
            ApplySettings(settings); // 변경된 오디오 설정 적용
        } // 사용자 설정 변경 이벤트 처리 마무리

        private void ApplySettings(ProjectUserSettings settings) // 사용자 설정에서 오디오 값 적용
        { // Master와 채널별 음량과 전체 음소거 반영
            MasterVolume = Mathf.Clamp01(settings.MasterVolume); // 마스터 음량 적용
            MusicVolume = Mathf.Clamp01(settings.MusicVolume); // 배경 음악 음량 적용
            SfxVolume = Mathf.Clamp01(settings.SfxVolume); // 효과음 음량 적용
            UiVolume = Mathf.Clamp01(settings.UiVolume); // UI 효과음 음량 적용
            IsMuted = settings.IsMuted; // 전체 음소거 상태 적용

            if (Application.isPlaying) // 실제 실행 중 오디오 적용 여부 확인
            { // AudioListener 전체 출력 음량 적용
                AudioListener.volume = IsMuted ? 0f : MasterVolume; // Master와 Mute 기반 전체 출력 음량 적용
            } // AudioListener 전체 출력 음량 적용 마무리

            AudioSettingsChanged?.Invoke(); // 채널 연결 AudioSource에 새 BGM과 SFX와 UI 음량 알림
        } // 사용자 설정에서 오디오 값 적용 마무리
    } // 프로젝트 전체 음량 관리 서비스 마무리
} // 프로젝트 오디오 네임스페이스 마무리
