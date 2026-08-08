using ProjectJ.Core.Services; // 공통 서비스 레지스트리 기능 참조
using UnityEngine; // Unity AudioSource와 MonoBehaviour 기능 참조

namespace ProjectJ.Audio // 프로젝트 오디오 네임스페이스 선언
{ // 개별 AudioSource 사용자 채널 음량 적용 기능 구성
    [DisallowMultipleComponent] // 같은 AudioSource의 채널 음량 컴포넌트 중복 방지
    [RequireComponent(typeof(AudioSource))] // 실제 소리를 재생할 AudioSource 보장
    public sealed class AudioChannelVolumeController : MonoBehaviour // AudioSource에 BGM과 SFX와 UI 사용자 음량을 적용하는 컴포넌트 선언
    { // 원본 AudioSource 음량과 사용자 채널 음량 결합 기능 구성
        [SerializeField] private ProjectAudioChannel channel = ProjectAudioChannel.Sfx; // 현재 AudioSource 사용자 음량 카테고리
        [SerializeField, Range(0f, 1f)] private float baseVolume = 1f; // 사용자 설정 적용 전 원본 AudioSource 음량
        [SerializeField] private bool captureSourceVolumeOnAwake = true; // Awake에서 기존 AudioSource.volume을 원본 음량으로 사용할지 여부

        private AudioSource audioSource; // 같은 GameObject의 AudioSource 참조
        private AudioService audioService; // 프로젝트 전체 오디오 서비스 참조
        private bool isSubscribed; // 오디오 설정 변경 이벤트 연결 여부

        public ProjectAudioChannel Channel => channel; // 현재 AudioSource 오디오 채널 반환
        public float BaseVolume => baseVolume; // 현재 원본 AudioSource 음량 반환

        private void Awake() // AudioSource 참조와 원본 음량 준비
        { // 기존 Scene과 Prefab의 AudioSource 설정 보존
            audioSource = GetComponent<AudioSource>(); // 같은 GameObject의 AudioSource 조회

            if (captureSourceVolumeOnAwake && audioSource != null) // 기존 AudioSource 음량 캡처 사용 여부 확인
            { // 기존 제작자가 지정한 AudioSource 음량 보존
                baseVolume = Mathf.Clamp01(audioSource.volume); // 현재 AudioSource 음량을 원본 음량으로 저장
            } // 기존 제작자가 지정한 AudioSource 음량 보존 마무리
        } // AudioSource 참조와 원본 음량 준비 마무리

        private void OnEnable() // AudioService 연결과 현재 채널 음량 적용
        { // Scene 활성화 시 사용자 오디오 설정 연결
            TryConnectAudioService(); // 준비된 AudioService 연결 시도
            ApplyCurrentVolume(); // 현재 사용 가능한 값으로 AudioSource 음량 적용
        } // AudioService 연결과 현재 채널 음량 적용 마무리

        private void Update() // Bootstrap 초기화보다 먼저 활성화된 AudioSource 연결 보강
        { // AudioService가 늦게 준비되는 경우 한 번 더 연결
            if (!isSubscribed) // 아직 AudioService 이벤트 미연결 여부 확인
            { // 서비스 초기화 완료까지 연결 재시도
                TryConnectAudioService(); // 현재 프레임 AudioService 연결 시도
            } // 서비스 초기화 완료까지 연결 재시도 마무리
        } // Bootstrap 초기화보다 먼저 활성화된 AudioSource 연결 보강 마무리

        private void OnDisable() // 오디오 설정 이벤트 연결 해제
        { // 비활성 AudioSource의 불필요한 이벤트 호출 방지
            DisconnectAudioService(); // AudioService 변경 이벤트 구독 해제
        } // 오디오 설정 이벤트 연결 해제 마무리

        public void SetBaseVolume(float value) // 재생 로직에서 원본 AudioSource 음량 변경
        { // 페이드와 개별 효과를 사용자 채널 음량과 함께 사용
            baseVolume = Mathf.Clamp01(value); // 새 원본 음량 안전 범위 저장
            ApplyCurrentVolume(); // 새 원본 음량과 사용자 채널 음량 즉시 결합
        } // 재생 로직에서 원본 AudioSource 음량 변경 마무리

        private void TryConnectAudioService() // 초기화 완료 AudioService 조회와 이벤트 구독
        { // 준비된 공통 서비스가 있을 때만 채널 음량 연결
            if (isSubscribed) // 기존 AudioService 연결 여부 확인
            { // 중복 이벤트 연결 방지
                return; // 기존 연결 유지
            } // 중복 이벤트 연결 방지 마무리

            if (!GameServiceRegistry.TryGet(out AudioService service) || service.State != GameServiceState.Initialized) // AudioService 등록과 초기화 여부 확인
            { // 서비스 미준비 상태 유지
                return; // 다음 프레임 연결 재시도
            } // 서비스 미준비 상태 유지 마무리

            audioService = service; // 준비 완료 AudioService 참조 저장
            audioService.AudioSettingsChanged += ApplyCurrentVolume; // 사용자 오디오 설정 변경 이벤트 구독
            isSubscribed = true; // AudioService 이벤트 연결 상태 저장
            ApplyCurrentVolume(); // 연결 직후 현재 채널 음량 적용
        } // 초기화 완료 AudioService 조회와 이벤트 구독 마무리

        private void DisconnectAudioService() // AudioService 이벤트 구독 해제
        { // 비활성화와 파괴 시 이벤트 참조 정리
            if (!isSubscribed || audioService == null) // 연결된 AudioService 없음 여부 확인
            { // 해제할 이벤트 없음 처리
                isSubscribed = false; // 이벤트 연결 상태 안전 초기화
                audioService = null; // AudioService 참조 안전 초기화
                return; // 이벤트 해제 생략
            } // 해제할 이벤트 없음 처리 마무리

            audioService.AudioSettingsChanged -= ApplyCurrentVolume; // 사용자 오디오 설정 변경 이벤트 구독 해제
            isSubscribed = false; // 이벤트 연결 상태 초기화
            audioService = null; // AudioService 참조 초기화
        } // AudioService 이벤트 구독 해제 마무리

        private void ApplyCurrentVolume() // 현재 사용자 채널 음량을 AudioSource에 반영
        { // 원본 음량과 BGM 또는 SFX 또는 UI 배율 결합
            if (audioSource == null) // AudioSource 참조 누락 여부 확인
            { // 잘못된 AudioSource 상태 방어
                return; // 음량 적용 생략
            } // 잘못된 AudioSource 상태 방어 마무리

            float channelVolume = audioService != null ? audioService.GetChannelVolume(channel) : 1f; // 준비된 사용자 채널 음량 또는 기본 배율 조회
            audioSource.volume = Mathf.Clamp01(baseVolume * channelVolume); // 원본 음량과 사용자 채널 음량 결합 적용
        } // 현재 사용자 채널 음량을 AudioSource에 반영 마무리
    } // AudioSource 사용자 채널 음량 적용 컴포넌트 마무리
} // 프로젝트 오디오 네임스페이스 마무리
