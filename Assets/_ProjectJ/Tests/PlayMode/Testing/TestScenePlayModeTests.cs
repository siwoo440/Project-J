using System.Collections; // UnityTest 코루틴 반환 형식 참조
using NUnit.Framework; // PlayMode 테스트와 Assertion 기능 참조
using ProjectJ.Core.SceneFlow; // Tests 씬 이름 관리 기능 참조
using ProjectJ.Diagnostics; // 프로젝트 공통 로그 기능 참조
using ProjectJ.Testing; // 테스트 씬 마커와 테스트 Category 이름 참조
using UnityEngine; // Unity Object와 프레임 정보 기능 참조
using UnityEngine.SceneManagement; // Unity 런타임 씬 로드 기능 참조
using UnityEngine.TestTools; // UnityTest와 예상 로그 검증 기능 참조

namespace ProjectJ.Tests.PlayMode // 프로젝트 PlayMode 테스트 네임스페이스 선언
{
    [Category(ProjectTestFramework.SmokeCategory)] // 반복 실행할 기본 Smoke 테스트 Category 지정
    public sealed class TestScenePlayModeTests // Tests 씬 로드와 런타임 동작 검증 테스트 형식 선언
    {
        [UnitySetUp] // 각 PlayMode 테스트 실행 전 코루틴 준비 메서드 지정
        public IEnumerator SetUp() // Tests 씬을 단독 로드하고 첫 프레임 준비
        {
            string testsSceneName = GameSceneCatalog.GetSceneName(GameSceneId.Tests); // Tests 씬 이름 조회
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(testsSceneName, LoadSceneMode.Single); // Tests 씬 비동기 단독 로드 시작

            Assert.IsNotNull(loadOperation, "Tests 씬 로드 작업을 생성할 수 없습니다. Build Settings를 확인합니다."); // Tests 씬 로드 작업 생성 여부 검증

            while (!loadOperation.isDone) // Tests 씬 비동기 로드 완료 여부 확인
            {
                yield return null; // 다음 프레임까지 Tests 씬 로드 진행 대기
            }

            yield return null; // Tests 씬의 Awake와 Start 실행 완료를 위한 한 프레임 대기
        }

        [UnityTest] // 여러 프레임을 사용하는 PlayMode 테스트 지정
        [Category(ProjectTestFramework.SceneCategory)] // 씬 로드 검증 Category 추가
        public IEnumerator TestsSceneLoadsAsActiveScene() // Tests 씬이 활성 씬으로 정상 로드되는지 검증
        {
            Scene activeScene = SceneManager.GetActiveScene(); // 현재 활성 씬 정보 조회

            Assert.AreEqual(GameSceneCatalog.GetSceneName(GameSceneId.Tests), activeScene.name); // 활성 씬 이름이 Tests인지 검증
            Assert.IsTrue(activeScene.isLoaded); // Tests 씬 로드 완료 상태 검증
            yield return null; // PlayMode 테스트 코루틴 종료를 위한 한 프레임 반환
        }

        [UnityTest] // 여러 프레임을 사용하는 PlayMode 테스트 지정
        [Category(ProjectTestFramework.SceneCategory)] // 씬 마커 검증 Category 추가
        public IEnumerator TestSceneMarkerInitializesSuccessfully() // Tests 씬 마커의 런타임 초기화 상태 검증
        {
            ProjectTestSceneMarker marker = Object.FindFirstObjectByType<ProjectTestSceneMarker>(); // 현재 Tests 씬에서 테스트 씬 마커 검색

            Assert.IsNotNull(marker, "ProjectTestSceneMarker를 찾을 수 없습니다."); // 테스트 씬 마커 존재 여부 검증
            Assert.IsTrue(marker.IsInitialized); // 테스트 씬 마커 Awake 완료 상태 검증
            Assert.AreEqual(ProjectTestFramework.FrameworkVersion, marker.FrameworkVersion); // 테스트 프레임워크 버전 일치 여부 검증
            Assert.AreEqual(GameSceneCatalog.GetSceneName(GameSceneId.Tests), marker.LoadedSceneName); // 마커가 기록한 로드 씬 이름 검증
            yield return null; // PlayMode 테스트 코루틴 종료를 위한 한 프레임 반환
        }

        [UnityTest] // 여러 프레임을 사용하는 PlayMode 테스트 지정
        public IEnumerator RuntimeAdvancesAcrossFrames() // PlayMode에서 Unity 프레임이 실제로 진행되는지 검증
        {
            int startingFrame = Time.frameCount; // 테스트 시작 프레임 저장

            yield return null; // 다음 런타임 프레임까지 대기

            Assert.Greater(Time.frameCount, startingFrame); // 대기 후 프레임 번호 증가 여부 검증
        }

        [UnityTest] // 여러 프레임을 사용하는 PlayMode 테스트 지정
        [Category(ProjectTestFramework.LoggingCategory)] // 공통 로그 규칙 검증 Category 추가
        public IEnumerator ProjectLogWritesExpectedMessageInPlayMode() // PlayMode에서 공통 로그 출력과 LogAssert 연동 검증
        {
            string expectedMessage = ProjectLog.Format(ProjectLogCategory.Test, "PlayMode smoke log.", "TEST_PLAYMODE_LOG"); // PlayMode에서 기대하는 공통 로그 문자열 생성

            LogAssert.Expect(LogType.Log, expectedMessage); // 발생 예정인 일반 로그 문자열 등록
            ProjectLog.Info(ProjectLogCategory.Test, "PlayMode smoke log.", "TEST_PLAYMODE_LOG"); // 공통 로그 규칙을 사용하는 PlayMode 일반 로그 출력

            yield return null; // Unity Test Framework의 로그 수집 완료를 위한 한 프레임 대기
        }
    }
}
