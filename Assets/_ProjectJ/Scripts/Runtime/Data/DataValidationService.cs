using System; // 데이터 검증 실패 예외 기능 참조
using ProjectJ.Audio; // 오디오 서비스 형식 참조
using ProjectJ.Core.Services; // 공통 서비스 등록과 상태 형식 참조

namespace ProjectJ.Data // 프로젝트 데이터 네임스페이스 선언
{
    public sealed class DataValidationService : GameServiceBase // 공통 데이터 검증 준비 서비스 선언
    {
        public override string ServiceName => "DataValidation"; // 데이터 검증 서비스 이름 반환
        public override int InitializationOrder => 400; // 데이터 검증 서비스 초기화 순서 반환
        public bool LastValidationSucceeded { get; private set; } // 최근 필수 서비스 검증 성공 여부 저장

        protected override void OnInitialize() // 초기 공통 서비스 의존성 검증
        {
            ValidateInitializedService<SettingsService>(); // 설정 서비스 초기화 완료 여부 검증
            ValidateInitializedService<SaveService>(); // 저장 서비스 초기화 완료 여부 검증
            ValidateInitializedService<AudioService>(); // 오디오 서비스 초기화 완료 여부 검증
            LastValidationSucceeded = true; // 필수 서비스 검증 성공 상태 저장
        }

        private static void ValidateInitializedService<T>() where T : class, IGameService // 지정한 필수 서비스의 초기화 완료 여부 검증
        {
            T service = GameServiceRegistry.Get<T>(); // 등록된 필수 서비스 조회

            if (service.State != GameServiceState.Initialized) // 필수 서비스 초기화 완료 여부 확인
            {
                throw new InvalidOperationException($"{service.ServiceName} 서비스가 초기화되지 않았습니다."); // 필수 서비스 초기화 누락 예외 발생
            }
        }
    }
}
