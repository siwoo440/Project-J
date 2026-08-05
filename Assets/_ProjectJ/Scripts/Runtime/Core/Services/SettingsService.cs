using UnityEngine; // Unity 언어와 애플리케이션 정보 기능 참조

namespace ProjectJ.Core.Services // 프로젝트 공통 서비스 네임스페이스 선언
{
    public sealed class SettingsService : GameServiceBase // 게임 설정 준비 서비스 선언
    {
        public override string ServiceName => "Settings"; // 설정 서비스 이름 반환
        public override int InitializationOrder => 100; // 설정 서비스 초기화 순서 반환
        public float MasterVolume { get; private set; } // 현재 기본 마스터 음량 저장
        public SystemLanguage Language { get; private set; } // 현재 기본 언어 저장

        protected override void OnInitialize() // 설정 서비스의 기본값 준비
        {
            MasterVolume = 1f; // 기본 마스터 음량을 최대값으로 설정
            Language = Application.systemLanguage; // 운영체제 기준 기본 언어 저장
        }
    }
}
