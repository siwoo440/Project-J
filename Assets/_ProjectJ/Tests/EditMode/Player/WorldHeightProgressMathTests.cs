using NUnit.Framework; // EditMode 테스트 기능 참조
using ProjectJ.Player; // 월드 높이 계산 기능 참조

namespace ProjectJ.Tests.EditMode // 프로젝트 EditMode 테스트 네임스페이스 선언
{ // 월드 높이 진행 계산 테스트 범위
    public sealed class WorldHeightProgressMathTests // 높이와 최고 기록과 수직 구간 계산 테스트 선언
    { // 월드 높이 진행 계산 테스트 기능 범위
        private const float ComparisonTolerance = 0.0001f; // 부동소수점 비교 허용 오차

        [Test] // Unity Test Runner 테스트 지정
        public void HeightUsesOriginDifference() // 기준점과 월드 Y 차이를 높이로 사용하는지 검증
        { // 기준점 차이 높이 검증 범위
            float result = WorldHeightProgressMath.CalculateHeight(125f, 25f, 1f); // 기준점보다 100단위 높은 위치 계산
            Assert.That(result, Is.EqualTo(100f).Within(ComparisonTolerance)); // 100미터 높이 반환 확인
        } // 기준점 차이 높이 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void HeightNeverFallsBelowZero() // 기준점 아래 높이의 음수 방지 검증
        { // 음수 높이 방지 검증 범위
            float result = WorldHeightProgressMath.CalculateHeight(-10f, 5f, 1f); // 기준점 아래 위치 높이 계산
            Assert.That(result, Is.EqualTo(0f).Within(ComparisonTolerance)); // 최소 0미터 제한 확인
        } // 음수 높이 방지 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void HeightAppliesMetersPerUnityUnit() // 유니티 단위당 미터 비율 적용 검증
        { // 미터 변환 비율 검증 범위
            float result = WorldHeightProgressMath.CalculateHeight(12f, 2f, 2.5f); // 10단위 차이와 2.5배 비율 계산
            Assert.That(result, Is.EqualTo(25f).Within(ComparisonTolerance)); // 25미터 변환 결과 확인
        } // 미터 변환 비율 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void HighestHeightDoesNotDecreaseAfterFall() // 추락 뒤 최고 높이 유지 검증
        { // 최고 높이 유지 검증 범위
            float result = WorldHeightProgressMath.CalculateHighestHeight(350f, 120f); // 기존 기록보다 낮은 현재 높이 비교
            Assert.That(result, Is.EqualTo(350f).Within(ComparisonTolerance)); // 기존 최고 기록 유지 확인
        } // 최고 높이 유지 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void FirstSectionContainsStartingHeight() // 시작 높이의 첫 구간 판정 검증
        { // 첫 구간 판정 검증 범위
            int result = WorldHeightProgressMath.CalculateSectionIndex(0f, 1000f, 5); // 시작 높이 구간 계산
            Assert.That(result, Is.EqualTo(1)); // 첫 구간 반환 확인
        } // 첫 구간 판정 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void ExactBoundaryStartsNextSection() // 정확한 200미터 경계의 다음 구간 판정 검증
        { // 구간 경계 판정 검증 범위
            int result = WorldHeightProgressMath.CalculateSectionIndex(200f, 1000f, 5); // 첫 구간 종료 경계 계산
            Assert.That(result, Is.EqualTo(2)); // 두 번째 구간 시작 확인
        } // 구간 경계 판정 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void CourseTopRemainsInLastSection() // 정상 높이의 마지막 구간 유지 검증
        { // 마지막 구간 판정 검증 범위
            int result = WorldHeightProgressMath.CalculateSectionIndex(1000f, 1000f, 5); // 전체 코스 정상 구간 계산
            Assert.That(result, Is.EqualTo(5)); // 다섯 번째 구간 유지 확인
        } // 마지막 구간 판정 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void CourseProgressClampsAboveTop() // 정상 초과 높이의 전체 진행률 제한 검증
        { // 전체 진행률 제한 검증 범위
            float result = WorldHeightProgressMath.CalculateCourseProgress01(1200f, 1000f); // 정상 초과 높이 진행률 계산
            Assert.That(result, Is.EqualTo(1f).Within(ComparisonTolerance)); // 전체 진행률 100퍼센트 제한 확인
        } // 전체 진행률 제한 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void SectionProgressUsesCurrentSectionRange() // 현재 구간 내부 진행률 계산 검증
        { // 구간 내부 진행률 검증 범위
            float result = WorldHeightProgressMath.CalculateSectionProgress01(300f, 1000f, 5); // 두 번째 구간 중간 높이 계산
            Assert.That(result, Is.EqualTo(0.5f).Within(ComparisonTolerance)); // 현재 구간 50퍼센트 진행 확인
        } // 구간 내부 진행률 검증 범위 종료

        [Test] // Unity Test Runner 테스트 지정
        public void InvalidSectionCountFallsBackToOne() // 잘못된 구간 수의 안전 보정 검증
        { // 잘못된 구간 수 보정 검증 범위
            int result = WorldHeightProgressMath.CalculateSectionIndex(500f, 1000f, 0); // 0개 구간 설정 계산
            Assert.That(result, Is.EqualTo(1)); // 최소 한 개 구간 보정 확인
        } // 잘못된 구간 수 보정 검증 범위 종료
    } // 월드 높이 진행 계산 테스트 기능 범위 종료
} // 월드 높이 진행 계산 테스트 범위 종료
