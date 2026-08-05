using ProjectJ.Core.Services; // 공통 서비스 기본 형식 참조
using UnityEngine; // Unity 오디오와 수치 제한 기능 참조

namespace ProjectJ.Audio // 프로젝트 오디오 네임스페이스 선언
{
    public sealed class AudioService : GameServiceBase // 프로젝트 전체 음량 관리 서비스 선언
    {
        public override string ServiceName => "Audio"; // 오디오 서비스 이름 반환
        public override int InitializationOrder => 300; // 오디오 서비스 초기화 순서 반환
        public float MasterVolume { get; private set; } // 현재 마스터 음량 저장
        public bool IsMuted { get; private set; } // 현재 전체 음소거 여부 저장

        protected override void OnInitialize() // 오디오 서비스 기본 상태 준비
        {
            MasterVolume = 1f; // 기본 마스터 음량을 최대값으로 설정
            IsMuted = false; // 기본 전체 음소거 상태 해제
            ApplyMasterVolume(); // 준비된 기본 음량을 Unity 오디오에 적용
        }

        public void SetMasterVolume(float volume) // 마스터 음량 변경과 즉시 적용
        {
            MasterVolume = Mathf.Clamp01(volume); // 입력 음량을 0부터 1 사이로 제한
            ApplyMasterVolume(); // 변경된 마스터 음량 적용
        }

        public void SetMuted(bool isMuted) // 전체 음소거 상태 변경과 즉시 적용
        {
            IsMuted = isMuted; // 전달된 전체 음소거 상태 저장
            ApplyMasterVolume(); // 변경된 음소거 상태 적용
        }

        private void ApplyMasterVolume() // 현재 음량과 음소거 상태를 Unity 오디오에 적용
        {
            AudioListener.volume = IsMuted ? 0f : MasterVolume; // 음소거 여부에 따른 실제 출력 음량 설정
        }
    }
}
