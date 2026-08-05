using System; // 서비스 생성 함수와 예외 기능 참조
using ProjectJ.Audio; // 오디오 서비스 형식 참조
using ProjectJ.Data; // 데이터 검증 서비스 형식 참조
using UnityEngine; // Unity 컴포넌트와 로그 기능 참조

namespace ProjectJ.Core.Services // 프로젝트 공통 서비스 네임스페이스 선언
{
    [DisallowMultipleComponent] // 동일 게임 오브젝트의 중복 초기화 컴포넌트 추가 방지
    public sealed class CommonServiceInitializer : MonoBehaviour // 공통 서비스 등록과 초기화 담당 컴포넌트 선언
    {
        public bool IsInitialized => GameServiceRegistry.IsInitialized; // 전체 공통 서비스 초기화 완료 여부 반환

        public bool InitializeServices() // 필수 공통 서비스를 등록하고 순서대로 초기화
        {
            if (GameServiceRegistry.IsInitialized) // 공통 서비스가 이미 초기화되었는지 확인
            {
                Debug.Log("[Services] 공통 서비스가 이미 초기화되어 기존 인스턴스를 사용합니다."); // 기존 서비스 재사용 로그 출력
                return true; // 중복 생성 없이 성공 반환
            }

            try // 공통 서비스 등록과 초기화 예외 감시 시작
            {
                EnsureRegistered(() => new SettingsService()); // 설정 서비스가 없을 때만 생성과 등록
                EnsureRegistered(() => new SaveService()); // 저장 서비스가 없을 때만 생성과 등록
                EnsureRegistered(() => new AudioService()); // 오디오 서비스가 없을 때만 생성과 등록
                EnsureRegistered(() => new DataValidationService()); // 데이터 검증 서비스가 없을 때만 생성과 등록
                GameServiceRegistry.InitializeAll(); // 등록된 서비스를 정해진 순서로 초기화
                Debug.Log($"[Services] 공통 서비스 {GameServiceRegistry.RegisteredServiceCount}개 초기화를 완료했습니다."); // 전체 초기화 완료 로그 출력
                return true; // 공통 서비스 초기화 성공 반환
            }
            catch (Exception exception) // 공통 서비스 등록 또는 초기화 실패 처리
            {
                Debug.LogException(exception, this); // 전체 예외 내용과 초기화 컴포넌트 출력
                Debug.LogError("[Services] 공통 서비스 초기화에 실패하여 MainMenu 전환을 중단합니다.", this); // 게임 시작 중단 원인 로그 출력
                return false; // 공통 서비스 초기화 실패 반환
            }
        }

        private static void EnsureRegistered<T>(Func<T> serviceFactory) where T : class, IGameService // 지정한 서비스가 없을 때만 인스턴스 생성과 등록
        {
            if (GameServiceRegistry.Contains<T>()) // 지정한 서비스 형식이 이미 등록되었는지 확인
            {
                return; // 중복 인스턴스 생성 없이 메서드 종료
            }

            T service = serviceFactory(); // 등록할 서비스 인스턴스 생성

            if (!GameServiceRegistry.Register(service)) // 생성한 서비스 등록 성공 여부 확인
            {
                throw new InvalidOperationException($"{service.ServiceName} 서비스를 등록하지 못했습니다."); // 예기치 않은 서비스 등록 실패 예외 발생
            }
        }
    }
}
