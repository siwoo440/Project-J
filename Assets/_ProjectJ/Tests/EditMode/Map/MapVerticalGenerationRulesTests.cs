using NUnit.Framework; // NUnit 자동 테스트 기능 참조
using ProjectJ.MapGeneration; // 수직 맵 생성 규칙 참조

namespace ProjectJ.Tests.EditMode // EditMode 테스트 네임스페이스 선언
{ // EditMode 테스트 묶음
    public sealed class MapVerticalGenerationRulesTests // 수직 생성 목표 규칙 자동 테스트 선언
    { // 수직 생성 목표 규칙 자동 테스트 묶음
        [Test] // 자동 테스트 항목 표시
        public void ReachableConfigurationIsAccepted() // 달성 가능한 수직 설정 허용 확인
        { // 달성 가능한 수직 설정 테스트 처리
            bool isValid = MapVerticalGenerationRules.TryValidateConfiguration(8, 16f, 3, 2, 4f, out string reason); // 8개 슬롯과 최대 4미터 상승 후보 설정 검사
            Assert.IsTrue(isValid, reason); // 달성 가능한 설정 허용 확인
        } // 달성 가능한 수직 설정 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void MissingAscendingPrefabIsRejected() // 상승 후보 없는 목표 높이 차단 확인
        { // 상승 후보 없는 설정 테스트 처리
            bool isValid = MapVerticalGenerationRules.TryValidateConfiguration(8, 8f, 3, 2, 0f, out string reason); // 상승량 없는 후보 설정 검사
            Assert.IsFalse(isValid); // 상승 후보 없는 설정 차단 확인
            StringAssert.Contains("상승 모듈", reason); // 정확한 실패 원인 포함 확인
        } // 상승 후보 없는 설정 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void CandidateThatMakesTargetHeightImpossibleIsRejected() // 남은 슬롯으로 목표 높이를 달성할 수 없는 후보 차단 확인
        { // 목표 높이 달성 불가 후보 테스트 처리
            bool isFeasible = MapVerticalGenerationRules.IsCandidateFeasible(0f, 0, 0, 0f, 1, 5f, 1, 2, 2f, false); // 평지 선택 뒤 최대 2미터만 가능한 조건 검사
            Assert.IsFalse(isFeasible); // 목표 높이 달성 불가 후보 차단 확인
        } // 목표 높이 달성 불가 후보 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void CandidateThatBreaksAscendingCountIsRejected() // 최소 상승 모듈 수를 달성할 수 없는 후보 차단 확인
        { // 상승 모듈 수 달성 불가 후보 테스트 처리
            bool isFeasible = MapVerticalGenerationRules.IsCandidateFeasible(4f, 0, 0, 0f, 1, 4f, 2, 2, 2f, false); // 남은 두 슬롯 중 첫 평지 선택 조건 검사
            Assert.IsFalse(isFeasible); // 최소 상승 모듈 수 달성 불가 후보 차단 확인
        } // 상승 모듈 수 달성 불가 후보 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void ThirdConsecutiveFlatModuleIsRejected() // 세 번째 연속 평지 모듈 차단 확인
        { // 연속 평지 제한 테스트 처리
            bool isFeasible = MapVerticalGenerationRules.IsCandidateFeasible(8f, 3, 2, 0f, 3, 8f, 3, 2, 4f, false); // 연속 평지 두 개 뒤 평지 후보 검사
            Assert.IsFalse(isFeasible); // 세 번째 연속 평지 후보 차단 확인
        } // 연속 평지 제한 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void FinalAscendingCandidateThatReachesTargetIsAccepted() // 마지막 상승 후보의 목표 도달 허용 확인
        { // 마지막 상승 후보 테스트 처리
            bool isFeasible = MapVerticalGenerationRules.IsCandidateFeasible(6f, 2, 0, 2f, 0, 8f, 3, 2, 4f, false); // 마지막 2미터 상승 후보 검사
            Assert.IsTrue(isFeasible); // 목표 높이와 상승 수를 만족한 후보 허용 확인
        } // 마지막 상승 후보 테스트 처리 종료

        [Test] // 자동 테스트 항목 표시
        public void ResultBelowTargetHeightIsRejected() // 최종 목표 높이 미달 결과 차단 확인
        { // 최종 목표 높이 미달 테스트 처리
            bool isValid = MapVerticalGenerationRules.TryValidateResult(7.5f, 8f, 3, 3, 1, 2, out string reason); // 0.5미터 부족한 최종 결과 검사
            Assert.IsFalse(isValid); // 목표 높이 미달 결과 차단 확인
            StringAssert.Contains("목표 높이", reason); // 정확한 실패 원인 포함 확인
        } // 최종 목표 높이 미달 테스트 처리 종료
    } // 수직 생성 목표 규칙 자동 테스트 묶음 종료
} // EditMode 테스트 묶음 종료
