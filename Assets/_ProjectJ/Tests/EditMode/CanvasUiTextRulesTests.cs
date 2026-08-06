using NUnit.Framework; // EditMode 단위 테스트 기능 참조
using ProjectJ.Gameplay; // 경기 결과와 순위 데이터 형식 참조
using ProjectJ.UI; // Canvas UI 공통 문구 규칙 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{ // 프로젝트 EditMode 테스트 묶음
    public sealed class CanvasUiTextRulesTests // Canvas UI 표시 문구 규칙 테스트 선언
    { // Canvas UI 표시 문구 규칙 테스트 묶음
        [TestCase(0f, "00:00")] // 0초 표시 사례 지정
        [TestCase(1.1f, "00:02")] // 남은 시간 올림 표시 사례 지정
        [TestCase(65f, "01:05")] // 1분 이상 표시 사례 지정
        [TestCase(-5f, "00:00")] // 음수 시간 보정 사례 지정
        public void FormatTimerReturnsTwoDigitMinuteAndSecond(float remainingTime, string expected) // 남은 시간 문구 형식 확인
        { // 남은 시간 문구 테스트 처리
            Assert.AreEqual(expected, CanvasUiTextRules.FormatTimer(remainingTime)); // 예상 분과 초 문구 일치 확인
        } // 남은 시간 문구 테스트 처리 종료

        [Test] // Unity Test Runner 테스트 지정
        public void FormatRankClampsInvalidValues() // 잘못된 순위와 참가자 수 보정 확인
        { // 순위 문구 보정 테스트 처리
            Assert.AreEqual("1 / 1", CanvasUiTextRules.FormatRank(0, 0)); // 최소 1위와 한 명 보정 확인
        } // 순위 문구 보정 테스트 처리 종료

        [TestCase(PrototypeMatchOutcome.Victory, "승리")] // 단독 승리 문구 사례 지정
        [TestCase(PrototypeMatchOutcome.SharedVictory, "공동 승리")] // 공동 승리 문구 사례 지정
        [TestCase(PrototypeMatchOutcome.Defeat, "패배")] // 패배 문구 사례 지정
        [TestCase(PrototypeMatchOutcome.None, "경기 종료")] // 미확정 결과 문구 사례 지정
        public void GetOutcomeTextReturnsExpectedKoreanText(PrototypeMatchOutcome outcome, string expected) // 경기 승패 문구 확인
        { // 경기 승패 문구 테스트 처리
            Assert.AreEqual(expected, CanvasUiTextRules.GetOutcomeText(outcome)); // 예상 경기 승패 문구 일치 확인
        } // 경기 승패 문구 테스트 처리 종료

        [Test] // Unity Test Runner 테스트 지정
        public void FormatRankingEntryIncludesLocalAndCourseTopMarkers() // 로컬과 정상 도달 표시 포함 확인
        { // 순위 한 줄 문구 테스트 처리
            PrototypeRankEntry entry = new PrototypeRankEntry("PLAYER", 1000f, 1f, true, 0, true, 1); // 로컬 정상 도달 순위 데이터 생성
            string formatted = CanvasUiTextRules.FormatRankingEntry(entry, true, true); // 공동 순위와 정상 도달 문구 생성
            Assert.AreEqual("▶ 공동 1위  PLAYER  |  1000.0 m  |  정상 도달", formatted); // 전체 표시 문구 일치 확인
        } // 순위 한 줄 문구 테스트 처리 종료
    } // Canvas UI 표시 문구 규칙 테스트 묶음 종료
} // 프로젝트 EditMode 테스트 묶음 종료
