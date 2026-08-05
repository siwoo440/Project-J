using System; // 서비스 형식과 예외 기능 참조
using System.Collections.Generic; // 서비스 사전과 정렬 목록 기능 참조
using UnityEngine; // Unity 런타임 초기화 기능 참조

namespace ProjectJ.Core.Services // 프로젝트 공통 서비스 네임스페이스 선언
{
    public static class GameServiceRegistry // 공통 서비스 등록과 조회 담당 형식 선언
    {
        private static readonly Dictionary<Type, IGameService> Services = new Dictionary<Type, IGameService>(); // 서비스 형식별 단일 인스턴스 저장
        private static readonly List<IGameService> InitializationBuffer = new List<IGameService>(); // 초기화 순서 정렬용 임시 목록 저장

        public static bool IsInitializing { get; private set; } // 전체 서비스 초기화 진행 여부 반환
        public static bool IsInitialized { get; private set; } // 전체 서비스 초기화 완료 여부 반환
        public static int RegisteredServiceCount => Services.Count; // 현재 등록된 서비스 수 반환

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] // Unity 런타임 시작 시 정적 상태 초기화 지정
        private static void ResetOnSubsystemRegistration() // 새로운 런타임 시작 전 정적 서비스 상태 초기화
        {
            ClearInternal(); // 등록된 서비스와 초기화 상태 제거
        }

        public static bool Contains<T>() where T : class, IGameService // 지정한 서비스 형식의 등록 여부 확인
        {
            return Services.ContainsKey(typeof(T)); // 서비스 형식 키 존재 여부 반환
        }

        public static bool Register<T>(T service) where T : class, IGameService // 지정한 서비스 인스턴스 등록
        {
            if (service == null) // 전달된 서비스 인스턴스의 null 여부 확인
            {
                throw new ArgumentNullException(nameof(service)); // null 서비스 등록 예외 발생
            }

            Type serviceType = typeof(T); // 등록할 서비스 형식 조회

            if (Services.ContainsKey(serviceType)) // 같은 서비스 형식이 이미 등록되었는지 확인
            {
                return false; // 중복 서비스 등록 거부
            }

            Services.Add(serviceType, service); // 서비스 형식과 인스턴스 등록
            return true; // 서비스 등록 성공 반환
        }

        public static T Get<T>() where T : class, IGameService // 지정한 서비스 인스턴스 조회
        {
            if (TryGet(out T service)) // 지정한 서비스 조회 성공 여부 확인
            {
                return service; // 등록된 서비스 인스턴스 반환
            }

            throw new InvalidOperationException($"{typeof(T).Name} 서비스가 등록되지 않았습니다."); // 미등록 서비스 조회 예외 발생
        }

        public static bool TryGet<T>(out T service) where T : class, IGameService // 지정한 서비스의 안전한 조회 시도
        {
            if (Services.TryGetValue(typeof(T), out IGameService registeredService) && registeredService is T typedService) // 등록 서비스 존재와 형식 일치 여부 확인
            {
                service = typedService; // 형식 변환된 서비스 인스턴스 저장
                return true; // 서비스 조회 성공 반환
            }

            service = null; // 조회 실패 결과로 null 저장
            return false; // 서비스 조회 실패 반환
        }

        public static void InitializeAll() // 등록된 모든 서비스를 정해진 순서로 초기화
        {
            if (IsInitialized) // 전체 서비스가 이미 초기화되었는지 확인
            {
                return; // 중복 전체 초기화 없이 메서드 종료
            }

            if (IsInitializing) // 전체 서비스 초기화가 이미 진행 중인지 확인
            {
                throw new InvalidOperationException("공통 서비스 초기화가 이미 진행 중입니다."); // 중복 전체 초기화 예외 발생
            }

            IsInitializing = true; // 전체 서비스 초기화 진행 상태 설정

            try // 전체 초기화 예외 감시 시작
            {
                InitializationBuffer.Clear(); // 이전 정렬 임시 목록 제거

                foreach (IGameService service in Services.Values) // 등록된 모든 서비스 순회
                {
                    InitializationBuffer.Add(service); // 초기화 정렬 목록에 서비스 추가
                }

                InitializationBuffer.Sort(CompareServices); // 초기화 순서와 이름 기준으로 서비스 정렬

                foreach (IGameService service in InitializationBuffer) // 정렬된 모든 서비스 순회
                {
                    service.Initialize(); // 현재 서비스 초기화 실행
                    Debug.Log($"[Services] {service.InitializationOrder}: {service.ServiceName} 초기화 완료"); // 서비스별 초기화 완료 로그 출력
                }

                IsInitialized = true; // 전체 서비스 초기화 완료 상태 설정
            }
            finally // 성공과 실패 여부에 관계없는 정리 처리
            {
                InitializationBuffer.Clear(); // 초기화 정렬 임시 목록 제거
                IsInitializing = false; // 전체 서비스 초기화 진행 상태 해제
            }
        }

        private static int CompareServices(IGameService left, IGameService right) // 두 서비스의 초기화 순서 비교
        {
            int orderComparison = left.InitializationOrder.CompareTo(right.InitializationOrder); // 초기화 순서 값 비교

            if (orderComparison != 0) // 초기화 순서 값이 서로 다른지 확인
            {
                return orderComparison; // 초기화 순서 비교 결과 반환
            }

            return string.CompareOrdinal(left.ServiceName, right.ServiceName); // 같은 순서에서는 서비스 이름 비교 결과 반환
        }

#if UNITY_EDITOR
        public static void ResetForTests() // EditMode 테스트 전후 서비스 정적 상태 초기화
        {
            ClearInternal(); // 등록된 서비스와 초기화 상태 제거
        }
#endif

        private static void ClearInternal() // 모든 서비스 등록과 초기화 상태 제거
        {
            Services.Clear(); // 등록된 모든 서비스 제거
            InitializationBuffer.Clear(); // 초기화 정렬 임시 목록 제거
            IsInitializing = false; // 전체 서비스 초기화 진행 상태 초기화
            IsInitialized = false; // 전체 서비스 초기화 완료 상태 초기화
        }
    }
}
