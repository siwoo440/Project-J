using NUnit.Framework; // EditMode 단위 테스트 기능 참조
using ProjectJ.Items; // 아이템 상자 생성 규칙 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{ // 프로젝트 EditMode 테스트 묶음
    public sealed class ItemChestSpawnRulesTests // 상자 생성 순수 규칙 테스트 선언
    { // 상자 생성 순수 규칙 테스트 묶음
        [TestCase(0.00f, 0.35f, true)] // 확률 범위 시작값 생성 사례 지정
        [TestCase(0.34f, 0.35f, true)] // 확률 미만 생성 사례 지정
        [TestCase(0.35f, 0.35f, false)] // 확률 경계 미생성 사례 지정
        [TestCase(0.90f, 0.35f, false)] // 확률 초과 미생성 사례 지정
        public void ShouldSpawnUsesExclusiveProbabilityBoundary(float randomValue, float probability, bool expected) // 상자 확률 경계 규칙 확인
        { // 상자 확률 규칙 테스트 처리
            Assert.AreEqual(expected, ItemChestSpawnRules.ShouldSpawn(randomValue, probability)); // 예상 생성 여부 일치 확인
        } // 상자 확률 규칙 테스트 처리 종료

        [TestCase(0, 8, false)] // 시작 모듈 제외 사례 지정
        [TestCase(1, 8, true)] // 첫 중간 모듈 허용 사례 지정
        [TestCase(6, 8, true)] // 마지막 전 모듈 허용 사례 지정
        [TestCase(7, 8, false)] // 종료 모듈 제외 사례 지정
        public void IsEligibleModuleIndexExcludesStartAndFinish(int moduleIndex, int moduleCount, bool expected) // 시작과 종료 모듈 제외 규칙 확인
        { // 생성 대상 모듈 테스트 처리
            Assert.AreEqual(expected, ItemChestSpawnRules.IsEligibleModuleIndex(moduleIndex, moduleCount)); // 예상 모듈 포함 여부 일치 확인
        } // 생성 대상 모듈 테스트 처리 종료

        [Test] // Unity Test Runner 테스트 지정
        public void HasRequiredModuleGapRejectsAdjacentModule() // 인접 모듈 중복 생성 차단 확인
        { // 모듈 간격 규칙 테스트 처리
            Assert.IsFalse(ItemChestSpawnRules.HasRequiredModuleGap(3, 4, 2)); // 한 칸 차이 모듈 제외 확인
            Assert.IsTrue(ItemChestSpawnRules.HasRequiredModuleGap(3, 5, 2)); // 두 칸 차이 모듈 허용 확인
        } // 모듈 간격 규칙 테스트 처리 종료

        [Test] // Unity Test Runner 테스트 지정
        public void CanRespawnStopsAtConfiguredCount() // 지점별 재생성 횟수 제한 확인
        { // 재생성 횟수 규칙 테스트 처리
            Assert.IsTrue(ItemChestSpawnRules.CanRespawn(0, 1)); // 최초 획득 뒤 한 번 재생성 허용 확인
            Assert.IsFalse(ItemChestSpawnRules.CanRespawn(1, 1)); // 한 번 재생성 완료 뒤 추가 생성 차단 확인
        } // 재생성 횟수 규칙 테스트 처리 종료
    } // 상자 생성 순수 규칙 테스트 묶음 종료
} // 프로젝트 EditMode 테스트 묶음 종료
