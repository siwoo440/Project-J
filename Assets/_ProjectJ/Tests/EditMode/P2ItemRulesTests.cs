using NUnit.Framework; // EditMode 단위 테스트 기능 참조
using ProjectJ.Data; // 아이템 효과 종류 참조
using ProjectJ.Items; // P2 공통 판정 규칙 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{ // 프로젝트 EditMode 테스트 묶음
    public sealed class P2ItemRulesTests // P2 아이템 판정 규칙 테스트 선언
    { // P2 아이템 판정 규칙 테스트 묶음
        [Test] // Unity Test Runner 테스트 지정
        public void P2RangeIncludesExactlySevenEffects() // P2 효과 범위 일곱 종 확인
        { // P2 효과 범위 테스트 처리
            int p2Count = 0; // P2 효과 개수 초기화

            foreach (ItemEffectType effectType in System.Enum.GetValues(typeof(ItemEffectType))) // 전체 아이템 효과 순회
            { // 현재 아이템 우선순위 확인
                p2Count += P2ItemRules.IsP2Effect(effectType) ? 1 : 0; // P2 효과 개수 누적
            } // 현재 아이템 우선순위 확인 종료

            Assert.AreEqual(7, p2Count); // 확정된 P2 일곱 종 확인
        } // P2 효과 범위 테스트 처리 종료

        [Test] // Unity Test Runner 테스트 지정
        public void RewindProgressUsesLastSampleAtCompletion() // 되감기 완료 시 마지막 표본 선택 확인
        { // 되감기 표본 번호 테스트 처리
            int sampleIndex = P2ItemRules.CalculateRewindSampleIndex(1f, 101); // 완료 진행률의 표본 번호 계산
            Assert.AreEqual(100, sampleIndex); // 마지막 표본 번호 선택 확인
        } // 되감기 표본 번호 테스트 처리 종료

        [Test] // Unity Test Runner 테스트 지정
        public void DronePrefersHigherTruncatedHeight() // 드론이 소수 둘째 자리 절삭 높이 기준 상위 대상을 선택하는지 확인
        { // 드론 높이 우선순위 테스트 처리
            bool candidateWins = P2ItemRules.IsHigherPriorityTarget(10.129f, 10.119f, 8f, 2f); // 절삭 후 10.12와 10.11 높이 비교
            Assert.IsTrue(candidateWins); // 더 높은 후보 우선 확인
        } // 드론 높이 우선순위 테스트 처리 종료

        [Test] // Unity Test Runner 테스트 지정
        public void DroneUsesDistanceWhenTruncatedHeightsMatch() // 드론이 같은 절삭 높이에서 가까운 대상을 선택하는지 확인
        { // 드론 거리 우선순위 테스트 처리
            bool candidateWins = P2ItemRules.IsHigherPriorityTarget(10.129f, 10.121f, 2f, 8f); // 같은 10.12 높이와 서로 다른 거리 비교
            Assert.IsTrue(candidateWins); // 같은 높이의 가까운 후보 우선 확인
        } // 드론 거리 우선순위 테스트 처리 종료

        [Test] // Unity Test Runner 테스트 지정
        public void MiniatureScaleClampsUnsafeValues() // 소형화 배율 안전 범위 확인
        { // 소형화 배율 보정 테스트 처리
            Assert.AreEqual(0.4f, P2ItemRules.ClampMiniatureScale(0.1f)); // 지나치게 작은 배율 최소값 보정 확인
            Assert.AreEqual(1f, P2ItemRules.ClampMiniatureScale(1.5f)); // 커지는 배율 기본 크기 보정 확인
        } // 소형화 배율 보정 테스트 처리 종료

        [Test] // Unity Test Runner 테스트 지정
        public void SniperZoomClampsToOnePointFiveAndFour() // 저격 물총 배율 최소와 최대 확인
        { // 저격 배율 보정 테스트 처리
            Assert.AreEqual(1.5f, P2ItemRules.ClampSniperZoom(1f)); // 최소 1.5배 보정 확인
            Assert.AreEqual(4f, P2ItemRules.ClampSniperZoom(5f)); // 최대 4배 보정 확인
        } // 저격 배율 보정 테스트 처리 종료
    } // P2 아이템 판정 규칙 테스트 묶음 종료
} // 프로젝트 EditMode 테스트 묶음 종료
