using NUnit.Framework; // NUnit 자동 테스트 기능 참조
using ProjectJ.MapGeneration; // 맵 생성 Runtime 기능 참조

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스 선언
{ // EditMode 테스트 묶음
    public sealed class MapVerticalBranchGenerationRulesTests // 수직 분기 생성 규칙 자동 테스트 선언
    { // 수직 분기 생성 규칙 테스트 묶음
        [Test] // 자동 테스트 항목 표시
        public void EightModulesWithTwoPairsCreateFourOrdinaryRouteSlots() // 8개 구조의 단일 경로 일반 슬롯 계산 확인
        { // 경로 일반 슬롯 계산 테스트 처리
            int slotCount = MapVerticalBranchGenerationRules.CalculateRouteOrdinarySlotCount(8, 2); // 시작·분기 2단계·합류 뒤 1개 구조 계산
            Assert.AreEqual(4, slotCount); // 단일 경로 일반 모듈 4개 확인
        } // 경로 일반 슬롯 계산 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void EqualCandidateHeightGainsAreCompatible() // 같은 좌우 상승량 허용 확인
        { // 같은 좌우 상승량 테스트 처리
            bool isCompatible = MapVerticalBranchGenerationRules.AreHeightGainsCompatible(2f, 2.01f, 0.02f); // 허용 오차 안의 좌우 상승량 검사
            Assert.IsTrue(isCompatible); // 같은 높이 후보 허용 확인
        } // 같은 좌우 상승량 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void DifferentCandidateHeightGainsAreRejected() // 다른 좌우 상승량 차단 확인
        { // 다른 좌우 상승량 테스트 처리
            bool isCompatible = MapVerticalBranchGenerationRules.AreHeightGainsCompatible(2f, 3f, 0.02f); // 허용 오차 밖의 좌우 상승량 검사
            Assert.IsFalse(isCompatible); // 다른 높이 후보 차단 확인
        } // 다른 좌우 상승량 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void PairThatCanReachTargetIsAccepted() // 남은 슬롯으로 목표를 달성하는 후보 허용 확인
        { // 목표 달성 가능 후보 테스트 처리
            bool isFeasible = MapVerticalBranchGenerationRules.IsBranchPairFeasible(2f, 1, 0f, 0, 0f, 0, 2f, 2f, 1, 1, 8f, 3, 1, 4f, 0.02f, false); // 좌우 2미터 상승 뒤 두 슬롯이 남는 조건 검사
            Assert.IsTrue(isFeasible); // 목표 달성 가능 후보 허용 확인
        } // 목표 달성 가능 후보 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void PairThatCannotReachTargetIsRejected() // 남은 슬롯으로 목표에 못 미치는 후보 차단 확인
        { // 목표 달성 불가 후보 테스트 처리
            bool isFeasible = MapVerticalBranchGenerationRules.IsBranchPairFeasible(0f, 0, 0f, 0, 0f, 0, 0f, 0f, 0, 1, 8f, 1, 0, 4f, 0.02f, false); // 평지 분기 뒤 공통 슬롯 하나만 남는 조건 검사
            Assert.IsFalse(isFeasible); // 목표 달성 불가 후보 차단 확인
        } // 목표 달성 불가 후보 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void MergeWithDifferentBranchHeightsIsRejected() // 좌우 누적 높이 불일치 검출 확인
        { // 합류 높이 불일치 테스트 처리
            bool isValid = MapVerticalBranchGenerationRules.TryValidateMerge(4f, 3f, 2, 2, 1, 0.02f, out string reason); // 1미터 차이 합류 데이터 검사
            Assert.IsFalse(isValid); // 합류 높이 불일치 차단 확인
            StringAssert.Contains("합류 높이", reason); // 정확한 실패 원인 포함 확인
        } // 합류 높이 불일치 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void MergeRequiresAscendingModuleOnEachBranch() // 분기별 최소 상승 모듈 검사 확인
        { // 분기별 상승 수 테스트 처리
            bool isValid = MapVerticalBranchGenerationRules.TryValidateMerge(4f, 4f, 2, 0, 1, 0.02f, out string reason); // 오른쪽 상승 모듈 없는 합류 데이터 검사
            Assert.IsFalse(isValid); // 분기별 상승 수 미달 차단 확인
            StringAssert.Contains("오른쪽 분기", reason); // 정확한 실패 분기 포함 확인
        } // 분기별 상승 수 테스트 처리 종료
    } // 수직 분기 생성 규칙 자동 테스트 묶음 종료
} // EditMode 테스트 묶음 종료
