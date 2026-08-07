using System.Collections.Generic; // 서비스 초기화 순서 기록 목록 기능 참조
using NUnit.Framework; // EditMode 테스트 기능 참조
using ProjectJ.Core.Services; // 공통 서비스 등록과 기본 형식 참조
using UnityEngine; // 테스트용 게임 오브젝트 기능 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{
    public sealed class GameServiceRegistryTests // 공통 서비스 등록과 초기화 검증 테스트 형식 선언
    {
        [SetUp] // 각 테스트 실행 전 준비 메서드 지정
        public void SetUp() // 각 테스트 전에 정적 서비스 상태 초기화
        {
            GameServiceRegistry.ResetForTests(); // 이전 테스트의 서비스 등록과 초기화 상태 제거
        }

        [TearDown] // 각 테스트 실행 후 정리 메서드 지정
        public void TearDown() // 각 테스트 후 정적 서비스 상태 초기화
        {
            GameServiceRegistry.ResetForTests(); // 현재 테스트의 서비스 등록과 초기화 상태 제거
        }

        [Test] // Unity Test Runner 테스트 지정
        public void SameServiceTypeCanBeRegisteredOnlyOnce() // 같은 서비스 형식의 중복 등록 차단 여부 검증
        {
            RecordingServiceA firstService = new RecordingServiceA(new List<string>(), 100); // 첫 번째 같은 형식 서비스 생성
            RecordingServiceA duplicateService = new RecordingServiceA(new List<string>(), 100); // 두 번째 같은 형식 서비스 생성

            Assert.IsTrue(GameServiceRegistry.Register(firstService)); // 첫 번째 서비스 등록 성공 검증
            Assert.IsFalse(GameServiceRegistry.Register(duplicateService)); // 같은 형식 서비스 중복 등록 거부 검증
            Assert.AreSame(firstService, GameServiceRegistry.Get<RecordingServiceA>()); // 최초 등록 서비스 유지 여부 검증
            Assert.AreEqual(1, GameServiceRegistry.RegisteredServiceCount); // 등록된 서비스 수가 한 개인지 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void ServicesInitializeInConfiguredOrder() // 서비스가 설정된 순서로 초기화되는지 검증
        {
            List<string> initializationLog = new List<string>(); // 서비스 초기화 순서 기록 목록 생성
            RecordingServiceB laterService = new RecordingServiceB(initializationLog, 200); // 늦게 초기화될 서비스 생성
            RecordingServiceA earlierService = new RecordingServiceA(initializationLog, 100); // 먼저 초기화될 서비스 생성

            GameServiceRegistry.Register(laterService); // 늦은 순서 서비스를 먼저 등록
            GameServiceRegistry.Register(earlierService); // 이른 순서 서비스를 나중에 등록
            GameServiceRegistry.InitializeAll(); // 등록된 모든 서비스 초기화 실행

            CollectionAssert.AreEqual(new[] { "A", "B" }, initializationLog); // 등록 순서가 아닌 초기화 순서 적용 여부 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void InitializeAllDoesNotInitializeServiceTwice() // 전체 초기화 재호출 시 서비스 중복 초기화 방지 검증
        {
            RecordingServiceA service = new RecordingServiceA(new List<string>(), 100); // 초기화 횟수 검사용 서비스 생성

            GameServiceRegistry.Register(service); // 검사용 서비스 등록
            GameServiceRegistry.InitializeAll(); // 첫 번째 전체 초기화 실행
            GameServiceRegistry.InitializeAll(); // 두 번째 전체 초기화 실행

            Assert.AreEqual(1, service.InitializeCount); // 실제 서비스 초기화가 한 번만 실행됐는지 검증
            Assert.IsTrue(GameServiceRegistry.IsInitialized); // 전체 서비스 초기화 완료 상태 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void CommonServiceInitializerCreatesFourServicesOnlyOnce() // 공통 서비스 네 종류의 단일 생성과 재사용 여부 검증
        {
            GameObject initializerObject = new GameObject("CommonServiceInitializerTests"); // 공통 서비스 초기화 테스트 게임 오브젝트 생성
            CommonServiceInitializer initializer = initializerObject.AddComponent<CommonServiceInitializer>(); // 공통 서비스 초기화 컴포넌트 추가

            try // 테스트 게임 오브젝트 정리 보장을 위한 감시 시작
            {
                Assert.IsTrue(initializer.InitializeServices()); // 첫 번째 공통 서비스 초기화 성공 검증
                Assert.IsTrue(initializer.InitializeServices()); // 두 번째 공통 서비스 초기화 재사용 성공 검증
                Assert.AreEqual(4, GameServiceRegistry.RegisteredServiceCount); // 설정·저장·오디오·검증 서비스 네 개 등록 검증
                Assert.AreEqual(GameServiceState.Initialized, GameServiceRegistry.Get<SettingsService>().State); // 설정 서비스 초기화 완료 상태 검증
                Assert.IsTrue(GameServiceRegistry.IsInitialized); // 전체 공통 서비스 초기화 완료 상태 검증
            }
            finally // 테스트 성공과 실패에 관계없는 게임 오브젝트 정리
            {
                Object.DestroyImmediate(initializerObject); // 테스트용 게임 오브젝트 즉시 제거
            }
        }

        private sealed class RecordingServiceA : GameServiceBase // 초기화 순서와 횟수 검사용 첫 번째 서비스 선언
        {
            private readonly List<string> initializationLog; // 초기화 순서 기록 목록 참조 저장
            private readonly int initializationOrder; // 테스트용 초기화 순서 값 저장

            public RecordingServiceA(List<string> initializationLog, int initializationOrder) // 첫 번째 검사용 서비스 의존성 설정
            {
                this.initializationLog = initializationLog; // 전달된 초기화 기록 목록 저장
                this.initializationOrder = initializationOrder; // 전달된 초기화 순서 값 저장
            }

            public override string ServiceName => "RecordingServiceA"; // 첫 번째 검사용 서비스 이름 반환
            public override int InitializationOrder => initializationOrder; // 첫 번째 검사용 서비스 초기화 순서 반환
            public int InitializeCount { get; private set; } // 첫 번째 검사용 서비스 실제 초기화 횟수 저장

            protected override void OnInitialize() // 첫 번째 검사용 서비스 초기화 기록
            {
                InitializeCount++; // 실제 초기화 실행 횟수 증가
                initializationLog.Add("A"); // 초기화 순서 기록 목록에 A 추가
            }
        }

        private sealed class RecordingServiceB : GameServiceBase // 초기화 순서 검사용 두 번째 서비스 선언
        {
            private readonly List<string> initializationLog; // 초기화 순서 기록 목록 참조 저장
            private readonly int initializationOrder; // 테스트용 초기화 순서 값 저장

            public RecordingServiceB(List<string> initializationLog, int initializationOrder) // 두 번째 검사용 서비스 의존성 설정
            {
                this.initializationLog = initializationLog; // 전달된 초기화 기록 목록 저장
                this.initializationOrder = initializationOrder; // 전달된 초기화 순서 값 저장
            }

            public override string ServiceName => "RecordingServiceB"; // 두 번째 검사용 서비스 이름 반환
            public override int InitializationOrder => initializationOrder; // 두 번째 검사용 서비스 초기화 순서 반환

            protected override void OnInitialize() // 두 번째 검사용 서비스 초기화 기록
            {
                initializationLog.Add("B"); // 초기화 순서 기록 목록에 B 추가
            }
        }
    }
}
