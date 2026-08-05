using System; // 서비스 초기화 예외 기능 참조

namespace ProjectJ.Core.Services // 프로젝트 공통 서비스 네임스페이스 선언
{
    public abstract class GameServiceBase : IGameService // 공통 서비스 기본 초기화 형식 선언
    {
        public abstract string ServiceName { get; } // 파생 서비스 이름 반환 규칙 선언
        public abstract int InitializationOrder { get; } // 파생 서비스 초기화 순서 반환 규칙 선언
        public GameServiceState State { get; private set; } = GameServiceState.NotInitialized; // 현재 서비스 초기화 상태 저장

        public void Initialize() // 서비스를 중복 없이 초기화
        {
            if (State == GameServiceState.Initialized) // 서비스가 이미 초기화되었는지 확인
            {
                return; // 중복 초기화 없이 메서드 종료
            }

            if (State == GameServiceState.Initializing) // 같은 서비스의 재진입 초기화 여부 확인
            {
                throw new InvalidOperationException($"{ServiceName} 서비스가 이미 초기화 중입니다."); // 순환 또는 중복 초기화 예외 발생
            }

            State = GameServiceState.Initializing; // 서비스 상태를 초기화 진행 중으로 변경

            try // 파생 서비스 초기화 예외 감시 시작
            {
                OnInitialize(); // 파생 서비스의 실제 초기화 실행
                State = GameServiceState.Initialized; // 서비스 상태를 초기화 완료로 변경
            }
            catch // 초기화 과정에서 발생한 모든 예외 처리
            {
                State = GameServiceState.Failed; // 서비스 상태를 초기화 실패로 변경
                throw; // 원래 예외를 상위 초기화 담당자에게 전달
            }
        }

        protected abstract void OnInitialize(); // 파생 서비스의 실제 초기화 작업 선언
    }
}
