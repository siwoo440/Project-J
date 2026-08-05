using NUnit.Framework; // NUnit 테스트 기능 참조
using ProjectJ.Gameplay; // 경기 순위 규칙 참조

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스 선언
{ // EditMode 테스트 묶음
    public sealed class MatchRankingRulesTests // 실제 높이와 공동 순위 규칙 테스트 선언
    { // 경기 순위 테스트 묶음
        [Test] // 자동 테스트 항목 표시
        public void HeightsInsideToleranceAreTied() // 허용 오차 안의 공동 높이 확인
        { // 공동 높이 테스트 처리
            bool result = MatchRankingRules.IsSameHeight(100f, 100.04f, 0.05f); // 허용 오차 안의 높이 비교
            Assert.IsTrue(result); // 공동 높이 결과 확인
        } // 공동 높이 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void HeightsOutsideToleranceAreNotTied() // 허용 오차 밖의 다른 높이 확인
        { // 다른 높이 테스트 처리
            bool result = MatchRankingRules.IsSameHeight(100f, 100.06f, 0.05f); // 허용 오차 밖의 높이 비교
            Assert.IsFalse(result); // 다른 높이 결과 확인
        } // 다른 높이 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void CourseTopAlwaysComesBeforeHeightOnlyEntry() // 정상 도달 참가자 우선 표시 확인
        { // 정상 우선 테스트 처리
            bool result = MatchRankingRules.ShouldComeBefore(true, 999f, 20f, 1, false, 1000f, 10f, 0, 0.05f); // 정상 도달과 높이 참가자 비교
            Assert.IsTrue(result); // 정상 도달 우선 결과 확인
        } // 정상 우선 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void EarlierArrivalComesFirstInsideSharedRank() // 공동 순위 안의 빠른 도달자 우선 표시 확인
        { // 도달 시간 표시 순서 테스트 처리
            bool result = MatchRankingRules.ShouldComeBefore(false, 200f, 5f, 1, false, 200.02f, 8f, 0, 0.05f); // 공동 높이 참가자의 도달 시간 비교
            Assert.IsTrue(result); // 빠른 도달자 우선 결과 확인
        } // 도달 시간 표시 순서 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void StableOrderBreaksCompleteDisplayTie() // 완전 동점 등록 순서 표시 확인
        { // 등록 순서 테스트 처리
            bool result = MatchRankingRules.ShouldComeBefore(false, 200f, 5f, 0, false, 200f, 5f, 1, 0.05f); // 완전 동점 참가자 등록 순서 비교
            Assert.IsTrue(result); // 앞 등록 참가자 우선 결과 확인
        } // 등록 순서 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void SecondEqualHeightKeepsFirstRank() // 동일 높이 두 번째 참가자의 공동 1위 확인
        { // 공동 1위 테스트 처리
            int result = MatchRankingRules.CalculateCompetitionRank(1, 1, false, 300f, false, 300.03f, 0.05f); // 두 번째 참가자 공동 순위 계산
            Assert.AreEqual(1, result); // 공동 1위 결과 확인
        } // 공동 1위 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void RankAfterTwoLeadersSkipsToThird() // 공동 1위 다음 3위 건너뛰기 확인
        { // 순위 건너뛰기 테스트 처리
            int result = MatchRankingRules.CalculateCompetitionRank(2, 1, false, 300.03f, false, 250f, 0.05f); // 세 번째 참가자 순위 계산
            Assert.AreEqual(3, result); // 공동 순위 이후 3위 결과 확인
        } // 순위 건너뛰기 테스트 종료

        [Test] // 자동 테스트 항목 표시
        public void TwoCourseTopEntriesShareRank() // 정상 도달 참가자 공동 순위 확인
        { // 정상 공동 순위 테스트 처리
            bool result = MatchRankingRules.AreTied(true, 999f, true, 1001f, 0.05f); // 두 정상 도달 참가자 공동 순위 판정
            Assert.IsTrue(result); // 정상 도달 공동 순위 결과 확인
        } // 정상 공동 순위 테스트 종료
    } // 경기 순위 테스트 묶음 종료
} // EditMode 테스트 묶음 종료
