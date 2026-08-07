using NUnit.Framework; // EditMode 테스트 기능 참조
using ProjectJ.Diagnostics; // 프로젝트 공통 로그 기능 참조
using ProjectJ.Testing; // 공통 테스트 Category 이름 참조
using UnityEngine; // Unity LogType 기능 참조
using UnityEngine.TestTools; // 예상 로그 검증 기능 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{
    [Category(ProjectTestFramework.LoggingCategory)] // 공통 로그 규칙 테스트 Category 지정
    public sealed class ProjectLogTests // Project J 공통 로그 문자열과 출력 규칙 검증 테스트 형식 선언
    {
        [Test] // Unity Test Runner 테스트 지정
        public void FormatIncludesProjectPrefixCategoryAndMessage() // 코드가 없는 기본 로그 형식 검증
        {
            string formattedMessage = ProjectLog.Format(ProjectLogCategory.Core, "Initialization complete."); // Core 분류 기본 로그 문자열 생성

            Assert.AreEqual("[ProjectJ][Core] Initialization complete.", formattedMessage); // 프로젝트 접두사와 분류와 메시지 형식 일치 여부 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void FormatNormalizesOptionalCode() // 선택적 로그 코드의 공백 제거와 대문자 밑줄 변환 검증
        {
            string formattedMessage = ProjectLog.Format(ProjectLogCategory.Test, "Smoke test complete.", " test smoke "); // 공백과 소문자가 포함된 로그 코드로 문자열 생성

            Assert.AreEqual("[ProjectJ][Test][TEST_SMOKE] Smoke test complete.", formattedMessage); // 정규화된 로그 코드 형식 일치 여부 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void EmptyMessageUsesFallbackText() // 빈 로그 메시지의 대체 문구 적용 여부 검증
        {
            string formattedMessage = ProjectLog.Format(ProjectLogCategory.Data, "   ", "DATA_EMPTY"); // 공백만 포함된 로그 메시지로 문자열 생성

            Assert.AreEqual("[ProjectJ][Data][DATA_EMPTY] (no message)", formattedMessage); // 빈 메시지 대체 문구와 로그 형식 일치 여부 검증
        }

        [Test] // Unity Test Runner 테스트 지정
        public void ExpectedErrorLogDoesNotFailTest() // 예상 오류 로그를 LogAssert로 명시하는 규칙 검증
        {
            string expectedMessage = ProjectLog.Format(ProjectLogCategory.Test, "Expected test error.", "TEST_EXPECTED_ERROR"); // 테스트에서 의도한 오류 로그 문자열 생성

            LogAssert.Expect(LogType.Error, expectedMessage); // 발생 예정인 오류 로그 형식과 순서 등록
            ProjectLog.Error(ProjectLogCategory.Test, "Expected test error.", "TEST_EXPECTED_ERROR"); // 공통 로그 규칙을 사용하는 예상 오류 출력
        }
    }
}
